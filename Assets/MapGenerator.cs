using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navMeshFloor;

    public Transform navMeshMaskPrefab;
    public Vector2 maxMapSize;

    public Map[] maps;
    public int mapIndex;

    Map currentMap;
    
    [Range(0,1)]
    public float outlinePercent;
    
    public float tileSize;
    
    List<Coord> allTileCoords;
    Queue<Coord> shuffledTileCoords;
    Queue<Coord> shuffledOpenTileCoords;
    Transform [,] tileMap;
    

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
        currentMap = maps[mapIndex];

        tileMap = new Transform[currentMap.mapSize.x,currentMap.mapSize.y];

        System.Random prng = new System.Random(currentMap.randomSeed);
        /*Set the Size of Ground BoxCollider based on the Map Size*/
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f,currentMap.mapSize.y*tileSize);

        /*List of all the Coordinates */
        allTileCoords = new List<Coord>();
        
        /*
            This for loop add the coordinates of the generated tiles into a List<Coord>
         */
        for(int x =0 ; x< currentMap.mapSize.x; x++){
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                allTileCoords.Add(new Coord(x,y));
            }
        }

        /*The returned shuffled tiles now become a Queue 
        (class Utilites returns an array of shuffled deck of generic objects) 
        In this case the tile coordinates are shuffled using Fisher-Yates algorithm*/
        shuffledTileCoords = new Queue<Coord>(Utilities.ShuffleArray(allTileCoords.ToArray(),currentMap.randomSeed));
        

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
        for(int x =0 ; x< currentMap.mapSize.x; x++){
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x,y);
                Transform newTile= Instantiate(tilePrefab,tilePosition,Quaternion.Euler(Vector3.right*90)) as Transform;
                newTile.localScale = Vector3.one*(1-outlinePercent)*tileSize;
                newTile.parent = mapHolder;
                /*Add new instantiated tiles positions to a tile map Array */
                tileMap[x,y] = newTile;
            }
        }
        bool [,] obstacleMap = new bool[(int)currentMap.mapSize.x,(int)currentMap.mapSize.y];

        int obstacleCount =(int)(currentMap.mapSize.x*currentMap.mapSize.y*currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        /*Stores all tile coordinates into another List of Coordinates.
        Soon to be used to narrow down Accessible Path */
        List<Coord> allOpenCoords = new List<Coord>(allTileCoords);

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
            if(randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap,currentObstacleCount)){
                /*Interpolate the height of obstacles between min and max height value */
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight,currentMap.maxObstacleHeight,(float)prng.NextDouble());
                /*Get Position of Obstacle */
                Vector3 obstaclePos = CoordToPosition(randomCoord.x,randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab,obstaclePos+Vector3.up*obstacleHeight/2,Quaternion.identity) as Transform;
                
                newObstacle.parent= mapHolder;
                newObstacle.localScale = new Vector3((1-outlinePercent)*tileSize,obstacleHeight,(1-outlinePercent)*tileSize);
                
                /*Get Rederer Component and Material of an Obstacle.
                 Set color interpolation percentage.
                 Interpolate foreground color to background color by percentage.*/
                Renderer obstacleRender = newObstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRender.sharedMaterial);
                float colourPercent = randomCoord.y/(float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColour,currentMap.backgroundColour,colourPercent);
                obstacleRender.sharedMaterial = obstacleMaterial;

                /*Removes each tile that will be occupied by Obstacles.
                Leaving only Accessible Paths */
                allOpenCoords.Remove(randomCoord);
            }
            else{
                obstacleMap[randomCoord.x,randomCoord.y] = false;
                currentObstacleCount--;

            }
            
        }
        shuffledOpenTileCoords = new Queue<Coord>(Utilities.ShuffleArray(allOpenCoords.ToArray(),currentMap.randomSeed));

        SetMapEdgeMaskSize(mapHolder);
        navMeshFloor.localScale = new Vector3(maxMapSize.x,maxMapSize.y)*tileSize;   
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
        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x,currentMap.mapCentre.y]=true;

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

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x*currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;
    }
    /*This method returns Vector3 Position of the specified Coordinates
    Each Coordinate
    */
    Vector3 CoordToPosition(int x, int y){
        return new Vector3(-currentMap.mapSize.x/2f+0.5f+x,0,-currentMap.mapSize.y/2f+0.5f+y)*tileSize;
    }
    /*This method fetch a Coordinate from the Queue which stores randomized list of Coordinates
    Returns the Coordinate and Enqueue such Coordinate back to the Queue */
    public Coord GetRandomCoord(){
        Coord randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }

    /*This method returns Tile Positions that are on the Accessible Path 
    which is not occupied by Obstacles*/
    public Transform GetRandomOpenTile(){
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x,randomCoord.y];
    }
    /*This method takes in a position in the World and convert to Tile Position 
    .ie Snap To Grid */
    public Transform GetTileFromPosition(Vector3 position){
        int x = Mathf.RoundToInt(position.x/tileSize+(currentMap.mapSize.x-1)/2f);
        int y = Mathf.RoundToInt(position.z/tileSize+(currentMap.mapSize.y-1)/2f);
        x=Mathf.Clamp(x,0,tileMap.GetLength(0)-1);
        y=Mathf.Clamp(y,0,tileMap.GetLength(1)-1);
        return tileMap[x,y];
    }
    /*This method generates four walls, encapsulating the tiles so player and enemies cannot walk outside
    of the tile map.
    The size of the masks scales proportionally to the size of the Map not the size of the tile map */
    public void SetMapEdgeMaskSize(Transform mapHolder){
        Transform maskLeft = Instantiate(navMeshMaskPrefab,Vector3.left*((currentMap.mapSize.x+maxMapSize.x)/4f*tileSize),Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x-currentMap.mapSize.x)/2f,1,currentMap.mapSize.y)*tileSize;

        Transform maskRight = Instantiate(navMeshMaskPrefab,Vector3.right*((currentMap.mapSize.x+maxMapSize.x)/4f*tileSize),Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x-currentMap.mapSize.x)/2f,1,currentMap.mapSize.y)*tileSize;

        Transform maskTop = Instantiate(navMeshMaskPrefab,Vector3.forward*((currentMap.mapSize.y+maxMapSize.y)/4f*tileSize),Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f)*tileSize;

        Transform maskBottom = Instantiate(navMeshMaskPrefab,Vector3.back*((currentMap.mapSize.y+maxMapSize.y)/4f*tileSize),Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3(maxMapSize.x,1,(maxMapSize.y-currentMap.mapSize.y)/2f)*tileSize;
    }

    [System.Serializable]
    public class Map{
        
        public Coord mapSize;
        public Coord mapCentre{
            get{
                return new Coord((int)mapSize.x/2,(int)mapSize.y/2);
            }
        }
        [Range(0,1)]
        public float obstaclePercent;
        public int randomSeed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColour;
        public Color backgroundColour;

    }

    [System.Serializable]
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
