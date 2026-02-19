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

    [Header("Pausa")]
    public GameObject pausePanel;
    private bool isPaused = false;

    [Header("Impostazioni Grafiche")]
    public GameObject gridContainer; 

    private float score;
    private bool isGameOver = false;
    private int highScore;

    void Start()
    {
        Time.timeScale = 1f;
        score = 0f;
        
        
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        
        
        
       
        bool showBest = PlayerPrefs.GetInt("ShowBestScore", 1) == 1;
        if (highScoreText != null)
            highScoreText.gameObject.SetActive(showBest);

       
        bool showGrid = PlayerPrefs.GetInt("ShowGrid", 1) == 1;
        if (gridContainer != null)
            gridContainer.SetActive(showGrid);
            
        

        UpdateHighScoreUI();

        if(gameOverPanel != null) 
            gameOverPanel.SetActive(false);
    }

    
    
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
            
           
            PlayFabAuth.SaveBestScoreToCloud(highScore);
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

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void TogglePause()
    {
        
        if (isGameOver) return; 

        isPaused = !isPaused; 
        
        if (pausePanel != null)
            pausePanel.SetActive(isPaused);

        
        Time.timeScale = isPaused ? 0f : 1f; 
    }

    public void ResumeGame()
    {
        
        TogglePause();
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene("MainMenu");
    }
}