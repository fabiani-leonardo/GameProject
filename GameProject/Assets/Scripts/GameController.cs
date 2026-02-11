using UnityEngine;
using TMPro; // Serve per usare i testi TextMeshPro
using UnityEngine.SceneManagement; // Serve per ricaricare la scena

public class GameController : MonoBehaviour
{
    [Header("Interfaccia")]
    public TextMeshProUGUI scoreText;      // Trascina qui lo ScoreText
    public GameObject gameOverPanel;       // Trascina qui il pannello Game Over

    private float score;
    private bool isGameOver = false;

    void Start()
    {
        // Assicuriamoci che il gioco non sia in pausa all'inizio
        Time.timeScale = 1f;
        score = 0f;
        
        // Assicuriamoci che il pannello sia spento all'inizio
        if(gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

   void Update()
    {
        if (!isGameOver)
        {
            score += Time.deltaTime; 
            
            // Moltiplichiamo per 10 e convertiamo in intero
            // Esempio: 1.5 secondi diventa "Score: 15"
            // Esempio: 10.2 secondi diventa "Score: 102"
            int displayScore = (int)(score * 10f); 
            
            if(scoreText != null)
                scoreText.text = "Score: " + displayScore;
        }
    }

    // Questa funzione viene chiamata quando il player muore
    public void GameOver()
    {
        isGameOver = true;

        // Attiva il pannello Game Over
        if(gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // Ferma il tempo del gioco (tutto si blocca)
        Time.timeScale = 0f;
    }

    // Questa funzione sarà collegata al bottone
    public void RestartGame()
    {
        // Ricarica la scena corrente
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}