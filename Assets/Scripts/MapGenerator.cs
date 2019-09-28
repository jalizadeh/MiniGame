using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;

    public Vector2 mapSize;

    [Range(0,1)]
    public float outlinePercent;

    [Range(10, 1000)]
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

                newTile.localScale = Vector3.one * (1 - outlinePercent);
                newTile.parent = mapHolder;
            }
        }

        //create random obstacles and put on the tiles
        for (int count = 0; count < 10; count++)
        {
            Coord rndC = GetRandomCoord();
            Vector3 obstaclePosition = CoordToPosition(rndC.x, rndC.y);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * (obstaclePrefab.localScale.y / 2), Quaternion.identity) as Transform;

            newObstacle.parent = mapHolder;
        }
    }


    Vector3 CoordToPosition(int x, int y) {
        Vector3 position = new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
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
    }
}
