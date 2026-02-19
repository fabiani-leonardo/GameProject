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
    public GameObject explosionPrefab; 
    public AudioClip explosionSound;  
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    [Header("Anti-Camping")]
    public GameObject bombPrefab; 
    public float maxIdleTime = 2.5f; 

    
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
        
        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        switch (selectedSkin)
        {
            case 0: 
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
        
        if (Time.timeScale == 0f) return;

        HandleInput();
        CheckCamping(); 
    }



    void HandleInput()
    {
        
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
            
            return; 
        }

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

        currentGridPos.x = newX;
        currentGridPos.y = newY;
        UpdateRealPosition();
    }

    void UpdateRealPosition()
    {
        transform.position = new Vector3(currentGridPos.x * cellSize, currentGridPos.y * cellSize, transform.position.z);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Laser"))
        {
            Die();
        }
    }

    void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }


        if (explosionSound != null)
        {
          
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
            
           
            AudioSource.PlayClipAtPoint(explosionSound, Camera.main.transform.position, soundVolume * sfxVol);
        }

     
        FindAnyObjectByType<GameController>().GameOver();

       
        Destroy(gameObject);
    }

    void CheckCamping()
    {
       
        if ((Vector2)transform.position == lastRecordedPos)
        {
            idleTimer += Time.deltaTime;

          
            if (idleTimer >= maxIdleTime)
            {
                SpawnBombOnMe();
                idleTimer = 0f; 
            }
        }
        else
        {
           
            idleTimer = 0f;
            lastRecordedPos = transform.position;
        }
    }

    void SpawnBombOnMe()
    {
        if (bombPrefab != null)
        {
          
            Instantiate(bombPrefab, transform.position, Quaternion.identity);
        }
    }
}