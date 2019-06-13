using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;
    private int enemiesRemainingToSpawn;
    private int enemiesRemainingAlive;

    float nextSpawnTime;
    public float timeBetweenCampingChecks = 2f;
    float nextCampCheckTime;
    float campThresholdDistance = 1.5f;
    bool isCamping;

    bool isDisabled;
    Vector3 campPosOld;

    int currentWaveNumber;
    Wave currentWave;
    MapGenerator map;
    LivingEntity playerEntity;
    Transform playerT;


    [System.Serializable]
    public class Wave{
        public int enemyCount;
        public float timeBetweenSpawn;

    }
    private void Start() {
        //enemy.OnDeath.AddListener(OnEnemyDeath);
        map = FindObjectOfType<MapGenerator>();
        playerEntity = FindObjectOfType<Player>();
        playerT = playerEntity.transform;

        nextCampCheckTime = timeBetweenCampingChecks+Time.time;
        campPosOld = playerT.position;
        
        NextWave();

    }
    private void Update() {
        if(!isDisabled){
            /*Check if the Player is camping.
            Store Player's recent position every check.
            Player must moves atleast 1.5 Unit in order to avoid camping.
            If Player camps for 2 seconds, triggers isCamping  */
            if(Time.time>nextCampCheckTime){
            nextCampCheckTime = Time.time+timeBetweenCampingChecks;
            isCamping = (Vector3.Distance(playerT.position,campPosOld))< campThresholdDistance;
            campPosOld = playerT.position;
            }
            if(enemiesRemainingToSpawn > 0 && Time.time>nextSpawnTime){
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;
                StartCoroutine(SpawnEnemy());
            }
       
        }
    }

    IEnumerator SpawnEnemy(){
        float spawnDelay =1f;
        float tileFlashSpeed = 10f;

        /*Spawn enemies on randomized empty tiles.
        If Player camps for 2 or more seconds, enemies will instead spawn right under the player */
        Transform spawnTile = map.GetRandomOpenTile();
        if(isCamping){
            spawnTile = map.GetTileFromPosition(playerT.position);
        }

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer =0;
        /*Flashing the tile where enemies will be spawned.
        Interpolate Colors from initial color to flashing color.
        Uses Mathf.Pingpong to bounce between two colors within the time intervals */
        while(spawnTimer<spawnDelay){
            tileMat.color = Color.Lerp(initialColor,flashColor,Mathf.PingPong(spawnTimer*tileFlashSpeed,1));
            spawnTimer += Time.deltaTime;
            yield return null;
        }
        Enemy spawnedEnemy = Instantiate(enemy,spawnTile.position+Vector3.up,Quaternion.identity) as Enemy;

    }
    private void NextWave(){
        currentWaveNumber++;
        Debug.Log("Wave: "+currentWaveNumber);
        if(currentWaveNumber-1<waves.Length){
            currentWave = waves[currentWaveNumber-1];
            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }

    }
    public void OnEnemyDeath(){
        Debug.Log("Enemy Died");
        enemiesRemainingAlive--;
        Debug.Log("Enemy Remaining: "+enemiesRemainingAlive);
        if(enemiesRemainingAlive==0){
            NextWave();
        }
    }

    public void OnPlayerDeath(){
        isDisabled=true;
    }
}
