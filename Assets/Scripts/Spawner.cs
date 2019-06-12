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
    Wave currentWave;
    int currentWaveNumber;

    [System.Serializable]
    public class Wave{
        public int enemyCount;
        public float timeBetweenSpawn;

    }
    private void Start() {
        //enemy.OnDeath.AddListener(OnEnemyDeath);
        NextWave();

    }
    private void Update() {
        if(enemiesRemainingToSpawn > 0 && Time.time>nextSpawnTime){
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;
            
            Enemy spawnedEnemy = Instantiate(enemy,Vector3.zero,Quaternion.identity) as Enemy;
        }

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
}
