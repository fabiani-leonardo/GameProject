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

    private Vector2 startTouchPos;
    private Vector2 currentGridPos;
    private Vector2 endTouchPos;

    void Start()
    {
        currentGridPos = Vector2.zero;
    }

    void Update()
    {
        HandleInput();
    }

    // ... (Tutta la parte HandleInput, DetectSwipe, Move, UpdateRealPosition resta uguale) ...
    // ... Copia pure quella parte dallo script precedente se non l'hai cambiata ...

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) startTouchPos = Input.mousePosition;
        else if (Input.GetMouseButtonUp(0)) { endTouchPos = Input.mousePosition; DetectSwipe(); }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) startTouchPos = touch.position;
            else if (touch.phase == TouchPhase.Ended) { endTouchPos = touch.position; DetectSwipe(); }
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

        // 2. Riproduci il suono
        // Usiamo PlayClipAtPoint perché l'oggetto Player sta per essere distrutto.
        // Se usassimo un normale AudioSource sul player, il suono si interromperebbe subito.
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, soundVolume);
        }

        // 3. Distruggi il Player
        Destroy(gameObject);

        Debug.Log("GAME OVER");
        // Qui in futuro chiamerai il GameManager per mostrare il menu "Ricomincia"
    }
}