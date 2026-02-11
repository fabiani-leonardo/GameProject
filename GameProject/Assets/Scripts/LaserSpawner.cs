using UnityEngine;

public class LaserSpawner : MonoBehaviour
{
    [Header("Riferimenti")]
    public GameObject laserPrefab;
    public PlayerGridMovement playerRef; 

    [Header("Difficoltà Crescente")]
    public float initialSpawnRate = 2.0f; // Tempo iniziale tra i laser
    public float minSpawnRate = 0.6f;     // Velocità massima (non scende sotto questo)
    public float difficultyFactor = 0.05f; // Di quanto velocizza ogni volta

    private float currentSpawnRate;
    private float nextSpawnTime;

    void Start()
    {
        currentSpawnRate = initialSpawnRate;
        nextSpawnTime = Time.time + 1.0f; // Primo laser dopo 1 secondo
    }

    void Update()
    {
        // Controlliamo il tempo. Se è il momento, spara un laser.
        // NON controlliamo più se "currentLaser" esiste, così possono sovrapporsi.
        if (Time.time >= nextSpawnTime)
        {
            SpawnLaser();
            IncreaseDifficulty();
        }
    }

    void IncreaseDifficulty()
    {
        // Riduci il tempo di attesa per il prossimo laser
        currentSpawnRate -= difficultyFactor;
        
        // Assicuriamoci di non scendere sotto il minimo (altrimenti diventa ingiocabile)
        if (currentSpawnRate < minSpawnRate)
        {
            currentSpawnRate = minSpawnRate;
        }

        // Imposta il prossimo timer
        nextSpawnTime = Time.time + currentSpawnRate;
    }

    void SpawnLaser()
    {
        // 0 = Orizzontale, 1 = Verticale, 2 = Diagonale /, 3 = Diagonale \
        int type = Random.Range(0, 4);

        // Calcoliamo le dimensioni totali per essere sicuri che il laser sia lunghissimo
        float gridWidth = (playerRef.maxRight - playerRef.maxLeft + 1) * playerRef.cellSize;
        float gridHeight = (playerRef.maxUp - playerRef.maxDown + 1) * playerRef.cellSize;
        float maxLength = Mathf.Max(gridWidth, gridHeight) * 2f; // Moltiplichiamo per 2 per sicurezza

        // Scegliamo una casella casuale DELLA GRIGLIA come "bersaglio"
        int targetX = Random.Range(playerRef.maxLeft, playerRef.maxRight + 1);
        int targetY = Random.Range(playerRef.maxDown, playerRef.maxUp + 1);
        
        // Convertiamo la coordinata griglia in posizione mondo reale
        Vector3 targetPos = new Vector3(targetX * playerRef.cellSize, targetY * playerRef.cellSize, 0);

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;
        Vector3 spawnScale = Vector3.one;

        switch (type)
        {
            case 0: // --- ORIZZONTALE ---
                // Centriamo sulla Y scelta, ma la X deve essere 0 (centro schermo)
                spawnPos = new Vector3(0, targetPos.y, 0);
                spawnRot = Quaternion.identity; 
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;

            case 1: // --- VERTICALE ---
                // Centriamo sulla X scelta, ma la Y deve essere 0
                spawnPos = new Vector3(targetPos.x, 0, 0);
                spawnRot = Quaternion.Euler(0, 0, 90);
                spawnScale = new Vector3(maxLength, 0.5f, 1); // Nota: lunghezza su X locale
                break;

            case 2: // --- DIAGONALE ( / ) ---
                // Qui usiamo ESATTAMENTE la posizione target.
                // Ruotando di 45 gradi attorno a quel punto, la linea passerà per quella casella.
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, 45);
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;

            case 3: // --- DIAGONALE ( \ ) ---
                spawnPos = targetPos;
                spawnRot = Quaternion.Euler(0, 0, -45);
                spawnScale = new Vector3(maxLength, 0.5f, 1);
                break;
        }

        // Crea il laser
        GameObject newLaser = Instantiate(laserPrefab, spawnPos, spawnRot);
        newLaser.transform.localScale = spawnScale;
    }
}