using UnityEngine;

public class PlayerGridMovement : MonoBehaviour
{
    [Header("Impostazioni Griglia")]
    public float cellSize = 2.0f; // Quanto è grande ogni quadrato (distanza tra i centri)

    // Limiti della griglia (basati sul fatto che il centro è 0,0)
    // Esempio: griglia 5x3. X va da -2 a 2. Y va da -1 a 1.
    public int maxRight = 2;
    public int maxLeft = -2;
    public int maxUp = 1;
    public int maxDown = -1;

    [Header("Impostazioni Swipe")]
    public float swipeThreshold = 50f; // Distanza minima per registrare uno swipe

    private Vector2 startTouchPos;
    private Vector2 currentGridPos; // La nostra posizione logica (es: 0,0)
    private Vector2 endTouchPos;

    void Start()
    {
        // Inizializza la posizione logica basandosi su dove si trova l'oggetto all'inizio
        // Assicurati che il player parta esattamente al centro di una cella (es: 0,0,0)
        currentGridPos = Vector2.zero;
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // --- LOGICA MOUSE (Per testare su PC) ---
        if (Input.GetMouseButtonDown(0))
        {
            startTouchPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            endTouchPos = Input.mousePosition;
            DetectSwipe();
        }

        // --- LOGICA TOUCH (Per Mobile) ---
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
        }
    }

    void DetectSwipe()
    {
        // Calcola la distanza e la direzione
        Vector2 distance = endTouchPos - startTouchPos;

        // Se lo swipe è troppo corto, ignoralo (evita tocchi accidentali)
        if (distance.magnitude < swipeThreshold)
            return;

        // Controlla se lo swipe è orizzontale o verticale
        if (Mathf.Abs(distance.x) > Mathf.Abs(distance.y))
        {
            // Orizzontale
            if (distance.x > 0)
                Move(1, 0); // Destra
            else
                Move(-1, 0); // Sinistra
        }
        else
        {
            // Verticale
            if (distance.y > 0)
                Move(0, 1); // Su
            else
                Move(0, -1); // Giù
        }
    }

    void Move(int xDir, int yDir)
    {
        // Calcola la nuova posizione ipotetica
        float newX = currentGridPos.x + xDir;
        float newY = currentGridPos.y + yDir;

        // CONTROLLO LIMITI (La logica che hai chiesto)
        // Se provi ad andare oltre i limiti, non aggiorniamo la posizione.
        if (newX > maxRight || newX < maxLeft || newY > maxUp || newY < maxDown)
        {
            // Opzionale: Qui puoi mettere un suono di "errore" o un piccolo shake
            return;
        }

        // Se siamo qui, il movimento è valido. Aggiorna la griglia logica
        currentGridPos.x = newX;
        currentGridPos.y = newY;

        // Aggiorna la posizione reale (Teletrasporto)
        UpdateRealPosition();
    }

    void UpdateRealPosition()
    {
        // Moltiplica la coordinata griglia per la dimensione della cella
        transform.position = new Vector3(currentGridPos.x * cellSize, currentGridPos.y * cellSize, transform.position.z);
    }
}