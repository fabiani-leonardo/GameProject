using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject laserPrefab;
    public PlayerGridMovement playerRef; 

    [Header("Difficoltà Bilanciata")]
    public float initialSpawnRate = 2.0f; 
    public float minSpawnRate = 0.7f;     // Un po' più lento del vecchio 0.5
    public float difficultyFactor = 0.02f; // Molto più basso (era 0.05)

    [Header("Caos Controllato")]
    public float maxDoubleLaserChance = 0.3f; // Max 30%
    public float timeBeforeChaos = 30f; // I doppi laser iniziano solo dopo 30 secondi
    
    // Variabili private per gestire lo stato
    private float currentSpawnRate;
    private float nextSpawnTime;
    private float doubleLaserChance = 0f; 
    private float currentWarningTime = 1.0f;
    private float minWarningTime = 0.5f;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1.0f;
        doubleLaserChance = 0f; // Si inizia sempre da zero
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
        // 1. AUMENTO VELOCITÀ (Lineare e lento)
        // Riduciamo il tempo di attesa molto lentamente
        currentSpawnRate -= difficultyFactor;
        if (currentSpawnRate < minSpawnRate) currentSpawnRate = minSpawnRate;

        // 2. AUMENTO CAOS (Basato sul tempo di gioco)
        // Solo se il giocatore sopravvive più di "timeBeforeChaos" (es. 30 secondi)
        // iniziamo ad introdurre la possibilità di doppi laser.
        if (Time.timeSinceLevelLoad > timeBeforeChaos)
        {
            doubleLaserChance += 0.005f; // Sale dello 0.5% alla volta (molto piano)
            if (doubleLaserChance > maxDoubleLaserChance) doubleLaserChance = maxDoubleLaserChance;
        }

        // 3. RIFLESSI (Warning time)
        // Anche questo scende piano piano
        currentWarningTime -= 0.005f;
        if (currentWarningTime < minWarningTime) currentWarningTime = minWarningTime;

        nextSpawnTime = Time.time + currentSpawnRate;
    }

    void SpawnLaserLogic()
    {
        SpawnSingleLaser();

        // Ora il dado viene tirato solo se siamo nella fase avanzata della partita
        if (Random.value < doubleLaserChance)
        {
            SpawnSingleLaser();
        }
    }

    void SpawnSingleLaser()
    {
        // --- LOGICA DI POSIZIONAMENTO (Invariata) ---
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

        LaserBehavior behavior = newLaser.GetComponent<LaserBehavior>();
        if (behavior != null)
        {
            behavior.warningDuration = currentWarningTime;
        }
    }
}