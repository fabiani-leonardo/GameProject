using UnityEngine;

public class PlayerGridMovement : MonoBehaviour
{
    [Header("Impostazioni Griglia")]
    public float cellSize = 2.0f;
    public int maxRight = 2;
    public int maxLeft = -2;
    public int maxUp = 1;
    public int maxDown = -1;

    [Header("Impostazioni Swipe")]
    public float swipeThreshold = 50f;

    [Header("Effetti & Audio")]
    public GameObject explosionPrefab; // Trascina qui il prefab dell'esplosione
    public AudioClip explosionSound;   // Trascina qui il file audio
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    [Header("Anti-Camping")]
    public GameObject bombPrefab; // Trascina qui il prefab GridBomb
    public float maxIdleTime = 2.5f; // Tempo limite fermo

    //[Header("Effetti")]
    //public ParticleSystem moveParticlesPrefab; // Trascina qui il prefab del sistema di particelle per il movimento
    
    private float idleTimer = 0f;
    private Vector2 lastRecordedPos;

    private Vector2 startTouchPos;
    private Vector2 currentGridPos;
    private Vector2 endTouchPos;

    void Start()
    {
        currentGridPos = Vector2.zero;
        lastRecordedPos = transform.position;

        ApplySelectedSkin();
    }

    void ApplySelectedSkin()
    {
        // Leggi quale skin ha scelto il giocatore (Default 0 = Bianco)
        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (selectedSkin)
        {
            case 0: // Bianco
                sr.color = Color.white;
                break;
            case 1:
                sr.color = new Color(0.0f, 0.64f, 1f, 1f); 
                break;
            case 2:
                sr.color = new Color(0.21f, 0.21f, 1f, 1f); 
                break;
            case 3:
                sr.color = new Color(0.64f, 0.18f, 1f, 1f); 
                break;
            case 4:
                sr.color = new Color(1f, 0.18f, 0.65f, 1f); 
                break;
            default:
                sr.color = Color.white;
                break;
        }
    }

    void Update()
    {
        HandleInput();
        CheckCamping(); // Nuova funzione
    }

    // ... (Tutta la parte HandleInput, DetectSwipe, Move, UpdateRealPosition resta uguale) ...
    // ... Copia pure quella parte dallo script precedente se non l'hai cambiata ...

    void HandleInput()
    {
        // 1. PRIORITÀ AL TOUCH (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            
            if (touch.phase == TouchPhase.Began) 
            {
                startTouchPos = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended) 
            { 
                endTouchPos = touch.position; 
                DetectSwipe(); 
            }
            
            // IMPORTANTE: Se abbiamo rilevato un tocco, usciamo dalla funzione.
            // In questo modo il codice sotto (Mouse) non viene eseguito.
            return; 
        }

        // 2. FALLBACK MOUSE (Solo per test su PC / Editor)
        if (Input.GetMouseButtonDown(0)) 
        {
            startTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0)) 
        { 
            endTouchPos = Input.mousePosition; 
            DetectSwipe(); 
        }
    }

    void DetectSwipe()
    {
        Vector2 distance = endTouchPos - startTouchPos;
        if (distance.magnitude < swipeThreshold) return;

        if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            if (distance.x > 0) Move(1, 0); else Move(-1, 0);
        }
        else
        {
            if (distance.y > 0) Move(0, 1); else Move(0, -1);
        }
    }

    void Move(int xDir, int yDir)
    {
        float newX = currentGridPos.x + xDir;
        float newY = currentGridPos.y + yDir;

        if (newX > maxRight || newX < maxLeft || newY > maxUp || newY < maxDown) return;

        /*// 1. Istanzia le particelle nella vecchia posizione (dove eravamo prima di muoverci)
        if (moveParticlesPrefab != null)
        {
            // Creiamo l'effetto
            ParticleSystem p = Instantiate(moveParticlesPrefab, transform.position, Quaternion.identity);
            
            // Distruggiamo l'oggetto particellare dopo 1 secondo per non intasare la memoria
            Destroy(p.gameObject, 1f); 
        }*/

        // 2. Aggiorna la posizione (come prima)
        currentGridPos.x = newX;
        currentGridPos.y = newY;
        UpdateRealPosition();
    }

    void UpdateRealPosition()
    {
        transform.position = new Vector3(currentGridPos.x * cellSize, currentGridPos.y * cellSize, transform.position.z);
    }

    // --- GESTIONE COLLISIONE E MORTE ---
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Laser"))
        {
            Die();
        }
    }

    void Die()
    {
        // 1. Crea l'effetto visivo (Esplosione)
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 2. Riproduci il suono (CON CONTROLLO VOLUME)
        if (explosionSound != null)
        {
            // --- FIX: Leggiamo il volume globale degli SFX ---
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
            // Moltiplichiamo il volume base per quello dello slider
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, soundVolume * sfxVol);
        }

        // 3. Chiama il Game Over
        FindAnyObjectByType<GameController>().GameOver();

        // 4. Distruggi il Player
        Destroy(gameObject);
    }

    void CheckCamping()
    {
        // Se la posizione attuale è uguale all'ultima registrata
        if ((Vector2)transform.position == lastRecordedPos)
        {
            idleTimer += Time.deltaTime;

            // Se supera il tempo limite...
            if (idleTimer >= maxIdleTime)
            {
                SpawnBombOnMe();
                idleTimer = 0f; // Resetta per non spawnarne 100 insieme
            }
        }
        else
        {
            // Se ci siamo mossi, resetta il timer e aggiorna la posizione
            idleTimer = 0f;
            lastRecordedPos = transform.position;
        }
    }

    void SpawnBombOnMe()
    {
        if (bombPrefab != null)
        {
            // Spawna la bomba esattamente dove si trova il player
            Instantiate(bombPrefab, transform.position, Quaternion.identity);
        }
    }
}