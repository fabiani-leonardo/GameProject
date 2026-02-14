using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [Header("Interfaccia")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI finalScoreText;
    public GameObject gameOverPanel;

    [Header("Impostazioni Grafiche")]
    public GameObject gridContainer; // NUOVO: Trascina qui l'oggetto "Division"

    private float score;
    private bool isGameOver = false;
    private int highScore;

    void Start()
    {
        Time.timeScale = 1f;
        score = 0f;
        
        // Carica High Score
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        // --- NUOVO: CONTROLLO IMPOSTAZIONI ---
        
        // 1. Controlla se mostrare il Best Score in gioco
        bool showBest = PlayerPrefs.GetInt("ShowBestScore", 1) == 1;
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(showBest);

        // 2. Controlla se mostrare la Griglia
        bool showGrid = PlayerPrefs.GetInt("ShowGrid", 1) == 1;
        if (gridContainer != null)
            gridContainer.SetActive(showGrid);
            
        // -------------------------------------

        UpdateHighScoreUI();

        if(gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    // ... (TUTTO IL RESTO DELLO SCRIPT RIMANE UGUALE: Update, GameOver, RestartGame) ...
    
    void Update()
    {
        if (!isGameOver)
        {
            score += Time.deltaTime; 
            int displayScore = (int)(score * 10f); 

            if(scoreText != null)
                scoreText.text = "Score: " + displayScore;
        }
    }

    public void GameOver()
    {
        isGameOver = true;
        Time.timeScale = 0f;

        int finalScore = (int)(score * 10f);

        if (finalScore > highScore)
        {
            highScore = finalScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
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