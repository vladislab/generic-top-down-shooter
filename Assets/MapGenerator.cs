using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;
    public int randomSeed = 10;
    [Range(0,1)]
    public float outlinePercent;
    [Range(0,1)]
    public float obstaclePercent;
    Coord mapCentre;

    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    

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
        /*List of all the Coordinates */
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
        
        /*mapCentre is where the player spawn at the centre of the map */
        mapCentre = new Coord((int)mapSize.x/2,(int)mapSize.y/2);

        string holderName = "Generated Map";
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
        bool [,] obstacleMap = new bool[(int)mapSize.x,(int)mapSize.y];

        int obstacleCount =(int)(mapSize.x*mapSize.y*obstaclePercent);
        int currentObstacleCount = 0;

        /*This for loop generates obstacles on randomized Coordinates */
        for(int i=0;i<obstacleCount;i++){
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x,randomCoord.y]=true;
            currentObstacleCount ++;

            /*If the Coordinate is not on the Player Spawn AND the map is full accessible,
            then Instantiate all the obstacles.
            Set parent of the obstacles to be the Map Holder.
            If not map the Coordinate of the obstacle to be FALSE - meaning the tile is empty,
            and reduce the current number of obstacles */
            if(randomCoord != mapCentre && MapIsFullyAccessible(obstacleMap,currentObstacleCount)){
                Vector3 obstaclePos = CoordToPosition(randomCoord.x,randomCoord.y);

                Transform newObstacle = Instantiate(obstaclePrefab,obstaclePos+Vector3.up*0.5f,Quaternion.identity) as Transform;
                newObstacle.parent= mapHolder;
            }
            else{
                obstacleMap[randomCoord.x,randomCoord.y] = false;
                currentObstacleCount--;

            }
            
        }   
    }
    /*This method uses Flood-fill Algorithm to detect if the generated obstacles might block accessible paths
    First it flags the spawning tile of the player (mapCentre) so nothing can block that tile.
    ->The algorithm starts from the centre tile.
    ->Find a suitable candidate, CHECK them.
    ->In the next iteration, that tile is dequeued then the algorithm find its neighboring tiles.
    ->And repeats until all the Coordinates are assessed.
    **A candidate (tile) is CHECKED only when it is not on obstacle tile AND it has not been checked before -
    meaning it is either already been flagged or is mapped as an obstacle tile.
    The method returns true if the number of Checked tiles(accessible tiles) equals the non-obstacle tiles*/
    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount){
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0),obstacleMap.GetLength(1)];
        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(mapCentre);
        mapFlags[mapCentre.x,mapCentre.y]=true;

        int accessibleTileCount = 1;

        while(queue.Count >0){
            Coord tile = queue.Dequeue();

            for(int x=-1;x<=1;x++){
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x+x;
                    int neighbourY = tile.y+y;

                    if(x==0 || y==0){
                        if(neighbourX>=0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >=0 && neighbourY<obstacleMap.GetLength(1)){
                            if(!mapFlags[neighbourX,neighbourY] && !obstacleMap[neighbourX,neighbourY]){
                                mapFlags[neighbourX,neighbourY]=true;
                                queue.Enqueue(new Coord(neighbourX,neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(mapSize.x*mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
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

        
        public static bool operator ==(Coord c1, Coord c2){
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator !=(Coord c1, Coord c2){
            return !(c1 == c2);
        }
    }
}
