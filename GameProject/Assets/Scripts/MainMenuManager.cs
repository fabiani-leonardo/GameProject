using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Generala")]
    public TextMeshProUGUI highScoreText;
    public GameObject settingsPanel;
    public GameObject customizePanel;

    [Header("Riferimenti Sfondo")]
    public Image backgroundSquare;         
    public Color[] skinColors;

    [Header("Impostazioni Settings")]
    public Toggle gridToggle;
    public Toggle bestScoreToggle;

    [Header("Pannelli")]
    public GameObject creditsPanel;

    [Header("Sistema Customize")]
    public Button[] skinButtons;      
    public GameObject[] lockIcons;    
    public GameObject[] selectOutlines; 

    [Header("Impostazioni Audio")] 
    public Slider musicSlider;
    public Slider sfxSlider;

    
    private int[] scoreThresholds = { 0, 300, 600, 900, 1200 }; 

    void Start()
    {
       
        Time.timeScale = 1f; 

        
        int best = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "Best: " + best;

        gridToggle.isOn = PlayerPrefs.GetInt("ShowGrid", 1) == 1;

        
        int savedSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
        UpdateBackgroundColor(savedSkin);
       

        
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
        
        PlayerPrefs.SetInt("ShowGrid", gridToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ShowBestScore", bestScoreToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        settingsPanel.SetActive(false);
    }

  

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
            
            bool isUnlocked = highScore >= scoreThresholds[i];
            skinButtons[i].interactable = isUnlocked;

            
            if (i > 0 && lockIcons.Length > i - 1 && lockIcons[i - 1] != null)
            {
                lockIcons[i - 1].SetActive(!isUnlocked);
            }

            
            ColorBlock cb = skinButtons[i].colors;
            
            if (i == selectedSkin)
            {
                
                cb.normalColor = Color.white;
                cb.highlightedColor = Color.white;
                cb.selectedColor = Color.white;
                cb.pressedColor = Color.white;
            }
            else
            {
                
                Color dimColor = new Color(0.5f, 0.5f, 0.5f, 1f);
                cb.normalColor = dimColor;
                cb.highlightedColor = dimColor;
                cb.selectedColor = dimColor;
               
                cb.pressedColor = new Color(0.7f, 0.7f, 0.7f, 1f); 
            }
            
            skinButtons[i].colors = cb;
        }
    }

    
    public void SelectSkin(int skinIndex)
    {
        PlayerPrefs.SetInt("SelectedSkin", skinIndex);
        PlayerPrefs.Save();
        UpdateSkinsUI(); 
        UpdateBackgroundColor(skinIndex);
    }

    void UpdateBackgroundColor(int index)
    {
        
        if (backgroundSquare != null && skinColors.Length > index)
        {
            backgroundSquare.color = skinColors[index];
        }
    }
    
    
    public void SetGridPref(bool value) => PlayerPrefs.SetInt("ShowGrid", value ? 1 : 0);
    public void SetScorePref(bool value) => PlayerPrefs.SetInt("ShowBestScore", value ? 1 : 0);

   
    public void SetMusicVolume(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
    }
}