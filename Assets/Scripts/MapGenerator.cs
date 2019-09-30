using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;

    [Range(0,1)]
    public float obstaclePercent;

    public float tileSize;

    //the spawn position of the player
    Coord mapCenter;

    //create different maps
    [Range(0, 1000)]
    public int seed = 10;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    

    private void Start()
    {
        GenerateMap();
    }


    public void GenerateMap() {
        //put all the coords in a list
        allTileCoords = new List<Coord>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        //shuffle the list of coords
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArrays(allTileCoords.ToArray(), seed));


        //put all generated tiles in a new Gameobject
        string holderName = "Generated Map";
        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //create and locate the tiles
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                //Vector3 tilePosition = new Vector3(x, 0, y);
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;

                newTile.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newTile.name = "Tile [" + x + "," + y + "]";
                newTile.parent = mapHolder;
            }
        }

        //create random obstacles and put on the tiles
        bool[,] obsctaleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        int obstacleCount = (int)(mapSize.x * mapSize.y * obstaclePercent);
        mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);
        int currentObstacleCount = 0;

        for (int count = 0; count < obstacleCount; count++)
        {
            Coord randomCoord = GetRandomCoord();
            obsctaleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            //if the obsctale is OK and can be added to the position
            if (randomCoord != mapCenter && MapIsFullyAccessible(obsctaleMap, currentObstacleCount)){
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * (obstaclePrefab.localScale.y / 2), Quaternion.identity) as Transform;

                newObstacle.name = "obstacle [" + randomCoord.x + "," + randomCoord.y + "]";
                newObstacle.localScale = Vector3.one * (1 - outlinePercent) * tileSize;
                newObstacle.parent = mapHolder;
            }
            //if the obstacle creation is rejected
            else
            {
                obsctaleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }


        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 1) * tileSize;

        //-The massks used for navMesh--------------------
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;
        maskLeft.name = "navMesh Mask Left";
        maskLeft.parent = mapHolder;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (mapSize.x + maxMapSize.x) / 4 * tileSize, Quaternion.identity) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x - mapSize.x) / 2, 1, mapSize.y) * tileSize;
        maskRight.name = "navMesh Mask Right";
        maskRight.parent = mapHolder;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;
        maskTop.name = "navMesh Mask Top";
        maskTop.parent = mapHolder;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (mapSize.y + maxMapSize.y) / 4 * tileSize, Quaternion.identity) as Transform;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - mapSize.y) / 2) * tileSize;
        maskBottom.name = "navMesh Mask Bottom";
        maskBottom.parent = mapHolder;
        //---------------------------------------------
    }


    // Flood fill 4-direction algorithm
    // https://guide.freecodecamp.org/algorithms/flood-fill/
    // it is a recursive algorithm which goes through all the tiles.
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount) {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(mapCenter);
        mapFlags[mapCenter.x, mapCenter.y] = true;

        //mapCenter is added at the begining, so it starts from 1
        int accessibleTileCount = 1;

        while(queue.Count  > 0){
            Coord tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;

                    //only check N,S,W,E directions
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0)
                            && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            //if the neighbour is not checked yet && there is no obstacle there
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coord(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y) - currentObstacleCount;
        return targetAccessibleTileCount == accessibleTileCount;
    }


    Vector3 CoordToPosition(int x, int y) {
        Vector3 position = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y) * tileSize;
        return position;
    }

    //Whole queue is random, so just pick the first item,
    //and then put it back at the end of queue
    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }


    //An structure to hold tile's coordinate data
    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator ==(Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2)
        {
            return !(c1 == c2);
        }
    }
}
