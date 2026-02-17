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

    

    [Header("Impostazioni Settings")]
    public Toggle gridToggle;
    public Toggle bestScoreToggle;

    [Header("Sistema Customize")]
    public Button[] skinButtons;      // Trascina qui i 5 bottoni
    public GameObject[] lockIcons;    // Trascina qui i 4 lucchetti (indice 0 = verde, ecc.)
    public GameObject[] selectOutlines; // Opzionale: cornici per evidenziare la scelta

    // Soglie di punteggio per sbloccare (Bianco è 0)
    private int[] scoreThresholds = { 0, 300, 600, 900, 1200 }; 

    void Start()
    {
        // Setup base (come prima)
        int best = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "Best:" + best;

        gridToggle.isOn = PlayerPrefs.GetInt("ShowGrid", 1) == 1;
        bestScoreToggle.isOn = PlayerPrefs.GetInt("ShowBestScore", 1) == 1;

        settingsPanel.SetActive(false);
        customizePanel.SetActive(false);
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

    void UpdateSkinsUI()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        int selectedSkin = PlayerPrefs.GetInt("SelectedSkin", 0); // 0 = Bianco

        // Ciclo per gestire i 5 bottoni (0 a 4)
        for (int i = 0; i < skinButtons.Length; i++)
        {
            // 1. Controllo Sblocco
            bool isUnlocked = highScore >= scoreThresholds[i];
            
            // Il bottone è cliccabile solo se sbloccato
            skinButtons[i].interactable = isUnlocked;

            // Gestione Lucchetti (i lucchetti sono 4, partono dall'indice 1 del colore)
            if (i > 0) // Il bianco non ha lucchetto
            {
                if (lockIcons.Length > i - 1 && lockIcons[i - 1] != null)
                {
                    // Se sbloccato, nascondi il lucchetto. Se bloccato, mostralo.
                    lockIcons[i - 1].SetActive(!isUnlocked);
                }
            }

            // 2. Controllo Selezione (Opzionale: cambia colore o mostra bordo)
            // Qui usiamo un semplice sistema: se è selezionato, il bottone è un po' più scuro o chiaro
            ColorBlock cb = skinButtons[i].colors;
            if (i == selectedSkin)
            {
                cb.normalColor = Color.white; // Selezionato: Pieno colore
                cb.selectedColor = Color.white;
            }
            else
            {
                cb.normalColor = new Color(0.5f, 0.5f, 0.5f, 1f); // Non selezionato: Più scuro
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
    }
    
    // Toggle Helpers (opzionali)
    public void SetGridPref(bool value) => PlayerPrefs.SetInt("ShowGrid", value ? 1 : 0);
    public void SetScorePref(bool value) => PlayerPrefs.SetInt("ShowBestScore", value ? 1 : 0);
}