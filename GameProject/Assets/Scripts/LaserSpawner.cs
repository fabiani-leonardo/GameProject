using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject laserPrefab;
    public PlayerGridMovement playerRef; 

    [Header("Aspetto")]
    public float laserWidth = 0.3f; // <--- NUOVO: Modifica questo valore nell'Inspector!
                                     // 0.5f era il vecchio valore (grosso)
                                     // 0.2f o 0.15f saranno molto più sottili

    [Header("Difficoltà Bilanciata")]
    public float initialSpawnRate = 2.0f; 
    public float minSpawnRate = 0.7f;     
    public float difficultyFactor = 0.02f;

    [Header("Caos Controllato")]
    public float maxDoubleLaserChance = 0.3f; 
    public float timeBeforeChaos = 30f; 
    
    private float currentSpawnRate;
    private float nextSpawnTime;
    private float doubleLaserChance = 0f; 
    private float currentWarningTime = 1.0f;
    private float minWarningTime = 0.5f;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1.0f;
        doubleLaserChance = 0f;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLaserLogic();
            IncreaseDifficulty();
        }
    }

    void IncreaseDifficulty()
    {
        currentSpawnRate -= difficultyFactor;
        if (currentSpawnRate < minSpawnRate) currentSpawnRate = minSpawnRate;

        if (Time.timeSinceLevelLoad > timeBeforeChaos)
        {
            doubleLaserChance += 0.005f; 
            if (doubleLaserChance > maxDoubleLaserChance) doubleLaserChance = maxDoubleLaserChance;
        }

        currentWarningTime -= 0.005f;
        if (currentWarningTime < minWarningTime) currentWarningTime = minWarningTime;

        nextSpawnTime = Time.time + currentSpawnRate;
    }

    void SpawnLaserLogic()
    {
        SpawnSingleLaser();
        if (Random.value < doubleLaserChance)
        {
            SpawnSingleLaser();
        }
    }

    void SpawnSingleLaser()
    {
        int type = Random.Range(0, 4);
        float gridWidth = (playerRef.maxRight - playerRef.maxLeft + 1) * playerRef.cellSize;
        float gridHeight = (playerRef.maxUp - playerRef.maxDown + 1) * playerRef.cellSize;
        float maxLength = Mathf.Max(gridWidth, gridHeight) * 2f; 

        int targetX = Random.Range(playerRef.maxLeft, playerRef.maxRight + 1);
        int targetY = Random.Range(playerRef.maxDown, playerRef.maxUp + 1);
        Vector3 targetPos = new Vector3(targetX * playerRef.cellSize, targetY * playerRef.cellSize, 0);

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        Vector3 spawnScale = Vector3.one;

        // NOTA: Qui sotto usiamo "laserWidth" invece di 0.5f
        switch (type)
        {
            case 0: // Orizzontale
                spawnPos = new Vector3(0, targetPos.y, 0);
                spawnRot = Quaternion.identity; 
                spawnScale = new Vector3(maxLength, laserWidth, 1); // <--- USA VARIABILE
                break;
            case 1: // Verticale
                spawnPos = new Vector3(targetPos.x, 0, 0);
                spawnRot = Quaternion.Euler(0, 0, 90);
                spawnScale = new Vector3(maxLength, laserWidth, 1); // <--- USA VARIABILE
                break;
            case 2: // Diagonale /
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, 45);
                spawnScale = new Vector3(maxLength, laserWidth, 1); // <--- USA VARIABILE
                break;
            case 3: // Diagonale \
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, -45);
                spawnScale = new Vector3(maxLength, laserWidth, 1); // <--- USA VARIABILE
                break;
        }

        GameObject newLaser = Instantiate(laserPrefab, spawnPos, spawnRot);
        newLaser.transform.localScale = spawnScale;

        LaserBehavior behavior = newLaser.GetComponent<LaserBehavior>();
        if (behavior != null)
        {
            behavior.warningDuration = currentWarningTime;
        }
    }
}