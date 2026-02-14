using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Serve per i Toggle
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Riferimenti")]
    public TextMeshProUGUI highScoreText;
    public GameObject settingsPanel;
    public Toggle gridToggle;
    public Toggle bestScoreToggle;

    void Start()
    {
        // 1. Mostra l'High Score nel menu
        int best = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "Best: " + best;

        // 2. Imposta i Toggle come li avevamo lasciati (Default: Accesi = 1)
        bool gridStatus = PlayerPrefs.GetInt("ShowGrid", 1) == 1;
        bool scoreStatus = PlayerPrefs.GetInt("ShowBestScore", 1) == 1;

        gridToggle.isOn = gridStatus;
        bestScoreToggle.isOn = scoreStatus;

        // Assicuriamoci che il pannello opzioni sia chiuso
        settingsPanel.SetActive(false);
    }

    // Collegare al tasto GIOCA
    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Assicurati che la scena di gioco si chiami così
    }

    // Collegare al tasto OPZIONI
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Collegare al tasto CHIUDI nel pannello
    public void CloseSettings()
    {
        // Salviamo le preferenze quando chiudiamo
        PlayerPrefs.SetInt("ShowGrid", gridToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowBestScore", bestScoreToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        settingsPanel.SetActive(false);
    }

    // Collegare al Toggle GRIGLIA (On Value Changed) - Opzionale, per salvare subito
    public void SetGridPref(bool value)
    {
        PlayerPrefs.SetInt("ShowGrid", value ? 1 : 0);
    }

    // Collegare al Toggle SCORE (On Value Changed) - Opzionale
    public void SetScorePref(bool value)
    {
        PlayerPrefs.SetInt("ShowBestScore", value ? 1 : 0);
    }
}