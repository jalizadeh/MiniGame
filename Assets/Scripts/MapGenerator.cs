using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    Map currentMap;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    //public Vector2 mapSize;
    public Vector2 maxMapSize;

    [Range(0,1)]
    public float outlinePercent;

    public float tileSize;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    



    private void Start()
    {
        GenerateMap();
    }


    public void GenerateMap() {
        currentMap = maps[mapIndex];
        System.Random prng = new System.Random(currentMap.seed);

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        //put all the coords in a list
        allTileCoords = new List<Coord>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x, y));
            }
        }

        //shuffle the list of coords
        shuffledTileCoords = new Queue<Coord>(Utility.ShuffleArrays(allTileCoords.ToArray(), currentMap.seed));


        //put all generated tiles in a new Gameobject
        string holderName = "Generated Map";
        if (transform.Find(holderName))
            DestroyImmediate(transform.Find(holderName).gameObject);

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        //create and locate the tiles
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
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
        bool[,] obsctaleMap = new bool[currentMap.mapSize.x, currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        for (int count = 0; count < obstacleCount; count++)
        {
            Coord randomCoord = GetRandomCoord();
            obsctaleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            //if the obsctale is OK and can be added to the position
            if (randomCoord != currentMap.mapCenter && MapIsFullyAccessible(obsctaleMap, currentObstacleCount)){
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)prng.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * (obstaclePrefab.localScale.y / 2), Quaternion.identity) as Transform;

                newObstacle.name = "obstacle [" + randomCoord.x + "," + randomCoord.y + "]";
                newObstacle.localScale = new Vector3((1 - outlinePercent) * tileSize, obstacleHeight, (1 - outlinePercent) * tileSize);
                newObstacle.parent = mapHolder;

                //set the color based on obstacle position on the map
                Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour, currentMap.backgroundColour, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;
            }
            //if the obstacle creation is rejected
            else
            {
                obsctaleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }


        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y, 1) * tileSize;

        //-The masks used for navMesh--------------------
        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        maskLeft.name = "navMesh Mask Left";
        maskLeft.parent = mapHolder;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4f * tileSize, Quaternion.identity) as Transform;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2f, 1, currentMap.mapSize.y) * tileSize;
        maskRight.name = "navMesh Mask Right";
        maskRight.parent = mapHolder;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskTop.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
        maskTop.name = "navMesh Mask Top";
        maskTop.parent = mapHolder;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4f * tileSize, Quaternion.identity) as Transform;
        maskBottom.localScale = new Vector3(maxMapSize.x, 1, (maxMapSize.y - currentMap.mapSize.y) / 2f) * tileSize;
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
        queue.Enqueue(currentMap.mapCenter);
        mapFlags[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

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

        int targetAccessibleTileCount = currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount;
        return targetAccessibleTileCount == accessibleTileCount;
    }


    Vector3 CoordToPosition(int x, int y) {
        Vector3 position = new Vector3(-currentMap.mapSize.x / 2f + 0.5f + x, 0, -currentMap.mapSize.y / 2f + 0.5f + y) * tileSize;
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
    [System.Serializable]
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


    [System.Serializable]
    public class Map {
        public Coord mapSize;

        //how many obstacle should be in the map by %
        [Range(0, 1)]
        public float obstaclePercent;

        //create different maps
        [Range(0, 1000)]
        public int seed = 10;

        public float minObstacleHeight;
        public float maxObstacleHeight;

        public Color foregroundColour;
        public Color backgroundColour;


        //the spawn position of the player
        public Coord mapCenter {
            get {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
