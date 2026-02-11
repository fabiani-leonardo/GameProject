using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject laserPrefab;
    public PlayerGridMovement playerRef; 

    [Header("Difficoltà Crescente")]
    public float initialSpawnRate = 2.0f; 
    public float minSpawnRate = 0.5f;     
    public float difficultyFactor = 0.05f;

    [Header("Caos")]
    [Range(0f, 1f)] public float doubleLaserChance = 0f; // Probabilità iniziale di doppio laser
    public float maxDoubleLaserChance = 0.4f; // Al massimo il 40% delle volte ne escono due
    public float currentWarningTime = 1.0f; // Tempo di avviso iniziale
    public float minWarningTime = 0.4f; // Tempo minimo di avviso (velocissimo!)

    private float currentSpawnRate;
    private float nextSpawnTime;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1.0f;
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnLaserLogic(); // Chiama la logica di spawn
            IncreaseDifficulty();
        }
    }

    void IncreaseDifficulty()
    {
        // 1. Spawna più spesso
        currentSpawnRate -= difficultyFactor;
        if (currentSpawnRate < minSpawnRate) currentSpawnRate = minSpawnRate;

        // 2. Aumenta probabilità di doppi laser (Caos)
        doubleLaserChance += 0.01f; // Sale dell'1% ogni spawn
        if (doubleLaserChance > maxDoubleLaserChance) doubleLaserChance = maxDoubleLaserChance;

        // 3. Riduci il tempo di avviso (Riflessi)
        currentWarningTime -= 0.01f;
        if (currentWarningTime < minWarningTime) currentWarningTime = minWarningTime;

        nextSpawnTime = Time.time + currentSpawnRate;
    }

    void SpawnLaserLogic()
    {
        SpawnSingleLaser();

        // Controllo Caos: Tiriamo un dado per vedere se spawnarne un secondo
        if (Random.value < doubleLaserChance)
        {
            SpawnSingleLaser();
        }
    }

    void SpawnSingleLaser()
    {
        // --- (Tutta la tua vecchia logica di calcolo posizione rimane qui) ---
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

        switch (type)
        {
            case 0: // Orizzontale
                spawnPos = new Vector3(0, targetPos.y, 0);
                spawnRot = Quaternion.identity; 
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;
            case 1: // Verticale
                spawnPos = new Vector3(targetPos.x, 0, 0);
                spawnRot = Quaternion.Euler(0, 0, 90);
                spawnScale = new Vector3(maxLength, 0.5f, 1); 
                break;
            case 2: // Diagonale /
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, 45);
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;
            case 3: // Diagonale \
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, -45);
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;
        }

        GameObject newLaser = Instantiate(laserPrefab, spawnPos, spawnRot);
        newLaser.transform.localScale = spawnScale;

        // --- NUOVO: Applichiamo il tempo di avviso dinamico al laser ---
        LaserBehavior behavior = newLaser.GetComponent<LaserBehavior>();
        if (behavior != null)
        {
            behavior.warningDuration = currentWarningTime;
        }
    }
}