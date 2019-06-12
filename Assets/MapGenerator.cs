using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;
    public int randomSeed = 10;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    [Range(0,1)]public float outlinePercent;

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateMap(){
        string holderName = "Generated Map";
        allTileCoords = new List<Coord>();

        /*
            This for loop add the coordinates of the generated tiles into a List<Coord>
         */
        for(int x =0 ; x< mapSize.x; x++){
            for (int y = 0; y < mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x,y));
            }
        }

        /*The returned shuffled tiles now become a Queue 
        (class Utilites returns an array of shuffled deck of generic objects) 
        In this case the tile coordinates are shuffled using Fisher-Yates algorithm*/
        shuffledTileCoords = new Queue<Coord>(Utilities.ShuffleArray(allTileCoords.ToArray(),randomSeed));

        /*Find existing Generated Map object and destroy it immediately */
        if(transform.Find(holderName)){
            DestroyImmediate(transform.Find(holderName).gameObject);
        }

        /*Creates a mapHolder object to store the position of Generated Map object.
        Keep all Tiles and Obstacles as Children of Generated Map object */
        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;
        

        /*This for loop generates tilePrefabs: x amount of rows, y amount of columns
        Each tilePrefab are put into the specified position with 90 degree about x axis
        Each tilePrefab (Block) is scaled down mimicking outlines
        The Table centers at the Origin (0,0) */
        for(int x =0 ; x< mapSize.x; x++){
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile= Instantiate(tilePrefab,tilePosition,Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = Vector3.one*(1-outlinePercent);
                newTile.parent = mapHolder;
            }
        }

        /*This for loop generates obstacles on randomized Coordinates */
        int obstacleCount =10;
        for(int i=0;i<obstacleCount;i++){
            Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePos = CoordToPosition(randomCoord.x,randomCoord.y);
            Transform newObstacle = Instantiate(obstaclePrefab,obstaclePos+Vector3.up*0.5f,Quaternion.identity) as Transform;
            newObstacle.parent= mapHolder;
        }   
    }

    /*This method returns Vector3 Position of the specified Coordinates
    Each Coordinate
    */
    Vector3 CoordToPosition(int x, int y){
        return new Vector3(-mapSize.x/2+0.5f+x,0,-mapSize.y/2+0.5f+y);
    }
    /*This method fetch a Coordinate from the Queue which stores randomized list of Coordinates
    Returns the Coordinate and Enqueue such Coordinate back to the Queue */
    public Coord GetRandomCoord(){
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    public struct Coord{
        public int x;
        public int y;

        public Coord(int _x,int _y){
            x=_x;
            y=_y;
        }
    }
}
