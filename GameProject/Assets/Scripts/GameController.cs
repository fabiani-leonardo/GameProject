using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Interfaccia")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText; // NUOVO: Trascina qui il testo High Score
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText; // NUOVO: Testo nel pannello Game Over

    private float score;
    private bool isGameOver = false;
    private int highScore;

    void Start()
    {
        Time.timeScale = 1f;
        score = 0f;
        
        // Carica l'High Score salvato (se non esiste, mette 0)
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Aggiorna la UI dell'High Score subito
        UpdateHighScoreUI();

        if(gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameOver)
        {
            score += Time.deltaTime; 
            int displayScore = (int)(score * 10f); // Punteggio attuale

            if(scoreText != null)
                scoreText.text = "Score: " + displayScore;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        int finalScore = (int)(score * 10f);

        // Controlla se abbiamo battuto il record
        if (finalScore > highScore)
        {
            highScore = finalScore;
            // Salva permanentemente nella memoria del telefono/PC
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            // Mostra il punteggio finale e il record nel pannello
            if (finalScoreText != null)
                finalScoreText.text = "Score: " + finalScore + "\nBest: " + highScore;
        }
    }

    void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = "Best: " + highScore;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}