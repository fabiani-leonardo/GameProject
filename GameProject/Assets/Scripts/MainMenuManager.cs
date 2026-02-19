using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Generala")]
    public TextMeshProUGUI highScoreText;
    public GameObject settingsPanel;
    public GameObject customizePanel; // NUOVO

    [Header("Riferimenti Sfondo")]
    public Image backgroundSquare;          // <-- NUOVO! (Image al posto di SpriteRenderer)
    public Color[] skinColors;

    [Header("Impostazioni Settings")]
    public Toggle gridToggle;
    public Toggle bestScoreToggle;

    [Header("Pannelli")]
    public GameObject creditsPanel;

    [Header("Sistema Customize")]
    public Button[] skinButtons;      // Trascina qui i 5 bottoni
    public GameObject[] lockIcons;    // Trascina qui i 4 lucchetti (indice 0 = verde, ecc.)
    public GameObject[] selectOutlines; // Opzionale: cornici per evidenziare la scelta

    [Header("Impostazioni Audio")] // --- NUOVO ---
    public Slider musicSlider;
    public Slider sfxSlider;

    // Soglie di punteggio per sbloccare (Bianco è 0)
    private int[] scoreThresholds = { 0, 300, 600, 900, 1200 }; 

    void Start()
    {
        // --- FIX: Assicuriamoci che il tempo scorra normalmente! ---
        Time.timeScale = 1f; 

        // Setup base (come prima)
        int best = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "Best: " + best;

        gridToggle.isOn = PlayerPrefs.GetInt("ShowGrid", 1) == 1;

        // --- NUOVO: CARICA LA SKIN SULLO SFONDO ALL'AVVIO ---
        int savedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        UpdateBackgroundColor(savedSkin);
        // ---------------------------------------------------

        // --- IMPOSTAZIONI AUDIO ---
        if (musicSlider != null) 
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        if (sfxSlider != null) 
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }
    
    public void CloseSettings()
    {
        // Salva opzioni
        PlayerPrefs.SetInt("ShowGrid", gridToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowBestScore", bestScoreToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        settingsPanel.SetActive(false);
    }

    // --- NUOVA LOGICA CUSTOMIZE ---

    public void OpenCustomize()
    {
        customizePanel.SetActive(true);
        UpdateSkinsUI();
    }

    public void CloseCustomize()
    {
        customizePanel.SetActive(false);
    }

    public void OpenCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
    }

    void UpdateSkinsUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);

        for (int i = 0; i < skinButtons.Length; i++)
        {
            // 1. Controllo Sblocco
            bool isUnlocked = highScore >= scoreThresholds[i];
            skinButtons[i].interactable = isUnlocked;

            // Gestione Lucchetti
            if (i > 0 && lockIcons.Length > i - 1 && lockIcons[i - 1] != null)
            {
                lockIcons[i - 1].SetActive(!isUnlocked);
            }

            // 2. Controllo Colori (FIX COMPLETO)
            ColorBlock cb = skinButtons[i].colors;
            
            if (i == selectedSkin)
            {
                // Se è selezionato, forziamo TUTTI gli stati a essere bianchi/accesi
                cb.normalColor = Color.white;
                cb.highlightedColor = Color.white;
                cb.selectedColor = Color.white;
                cb.pressedColor = Color.white;
            }
            else
            {
                // Se NON è selezionato, forziamo TUTTI gli stati a essere grigi/spenti
                Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                cb.normalColor = dimColor;
                cb.highlightedColor = dimColor;
                cb.selectedColor = dimColor;
                // Lasciamo pressedColor un po' più chiaro per dare feedback al click se vuoi, 
                // oppure mettilo uguale a dimColor
                cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); 
            }
            
            skinButtons[i].colors = cb;
        }
    }

    // Questa funzione va collegata ai bottoni delle skin: 
    // Al bottone bianco passa 0, verde 1, ciano 2, ecc.
    public void SelectSkin(int skinIndex)
    {
        PlayerPrefs.SetInt("SelectedSkin", skinIndex);
        PlayerPrefs.Save();
        UpdateSkinsUI(); // Aggiorna la grafica per mostrare la nuova selezione
        UpdateBackgroundColor(skinIndex);
    }

    void UpdateBackgroundColor(int index)
    {
        // Controlla se abbiamo assegnato il quadrato e se l'indice è valido
        if (backgroundSquare != null && skinColors.Length > index)
        {
            backgroundSquare.color = skinColors[index];
        }
    }
    
    // Toggle Helpers (opzionali)
    public void SetGridPref(bool value) => PlayerPrefs.SetInt("ShowGrid", value ? 1 : 0);
    public void SetScorePref(bool value) => PlayerPrefs.SetInt("ShowBestScore", value ? 1 : 0);

    // --- NUOVE FUNZIONI PER L'AUDIO ---
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}