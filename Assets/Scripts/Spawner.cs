using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    //if activated, by pressing Enter, the current wave (level) is skipped
    public bool devMode;

    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    float nextSpawnTime;
    int enemiesAreAlive; //for next wave

    MapGenerator map;


    //if the player tries to hide or stay in a position for a specific time,
    //the new enemy will spawn near him to force him move
    float timeBetweenCampingChecks = 2;
    float campThresholdDistance = 1.5f;
    float nextCampCheckTime;
    Vector3 campPositionOld;
    bool isCamping;

    LivingEntity player;
    Transform playerT;

    //if the player dies, disable enemy spawning
    bool isDisabled;

    //generate new map with new wave
    public event System.Action<int> OnNewWave;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
        playerT = player.transform;
        campPositionOld = playerT.position;
        nextCampCheckTime = timeBetweenCampingChecks + Time.time;
        player.OnDeath += onPlayerDeath;


        map = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDisabled)
        {
            if (Time.time > nextCampCheckTime)
            {
                nextCampCheckTime = timeBetweenCampingChecks + Time.time;

                isCamping = Vector3.Distance(playerT.position, campPositionOld) < campThresholdDistance;
                campPositionOld = playerT.position;
            }

            if ((enemiesRemainingToSpawn > 0 || currentWave.infinite) && Time.time > nextSpawnTime)
            {
                enemiesRemainingToSpawn--;
                nextSpawnTime = Time.time + currentWave.timeBetweenSpawn;

                StartCoroutine("SpawnEnemy");
            }
        }


        /*
         * If I press "Enter", it will skip to next map
         * 1. stop generating new enemies
         * 2. destroty all current enemies
         * 3. start next wave
         */
        if (devMode) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                StopCoroutine("SpawnEnemy");
                foreach (Enemy enemy in FindObjectsOfType<Enemy>()) {
                    GameObject.Destroy(enemy.gameObject);
                }

                NextWave();
            }
        }
    }


    IEnumerator SpawnEnemy() {
        float spawnDelay = 1f;
        float tileFlashSpeed = 4f; //in 1 sec

        Transform spawnTile;

        if (isCamping)
            spawnTile = map.GetTileFromPosition(playerT.position);
        else
            spawnTile = map.GetRandomOpenTile();

        Material tileMat = spawnTile.GetComponent<Renderer>().material;
        Color initialColor = tileMat.color;
        Color flashColor = Color.red;
        float spawnTimer = 0;

        while (spawnTimer < spawnDelay) {
            spawnTimer += Time.deltaTime;
            tileMat.color = Color.Lerp(initialColor, flashColor, Mathf.PingPong(spawnTimer * tileFlashSpeed, 1));
            yield return null;
        }

        //at the, reset the color, unless it may not come back completely to it's initial color
        tileMat.color = initialColor;

        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity) as Enemy;
        spawnedEnemy.OnDeath += OnEnemyDeath;
        spawnedEnemy.SetCharacteristics(currentWave.moveSpeed, currentWave.hitsToKillPlayer,
            currentWave.enemyHealth, currentWave.skinColor);
    }


    void onPlayerDeath() {
        isDisabled = true;
    }

    //on each new wave, the player goes back to the center of the map
    void ResetPlayerPosition() {
        playerT.position = map.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
    }

    void OnEnemyDeath() {
        enemiesAreAlive--;
        if (enemiesAreAlive == 0)
            NextWave();
    }

    void NextWave() {
        if(currentWaveNumber > 0)
        {
            //AudioManager.instance.PlaySound("Level Complete", Vector3.zero);
            AudioManager.instance.PlaySound2D("Level Complete");
        }

        currentWaveNumber++;

        if (currentWaveNumber - 1 < waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesAreAlive = currentWave.enemyCount;

            if (OnNewWave != null)
                OnNewWave(currentWaveNumber);

            ResetPlayerPosition();
        }
    }

    [System.Serializable]
    public class Wave{
        public bool infinite;
        public int enemyCount;
        public float timeBetweenSpawn;

        public float moveSpeed;
        public int hitsToKillPlayer;
        public float enemyHealth;
        public Color skinColor;
    }
}
