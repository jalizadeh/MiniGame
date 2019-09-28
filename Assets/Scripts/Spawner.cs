using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    float nextSpawnTime;
    int enemiesAreAlive; //for next wave

    // Start is called before the first frame update
    void Start()
    {
        NextWave();

    }

    // Update is called once per frame
    void Update()
    {
        if(enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;

            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath;
        }
    }


    void OnEnemyDeath() {
        enemiesAreAlive--;
        if (enemiesAreAlive == 0)
            NextWave();
    }

    void NextWave() {
        currentWaveNumber++;
        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesAreAlive = currentWave.enemyCount;
        }
    }

    [System.Serializable]
    public class Wave{
        public int enemyCount;
        public float timeBetweenSpawn;
    }
}
