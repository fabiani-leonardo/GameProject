using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

public class PlayFabAuth : MonoBehaviour
{
    [Header("Pannelli UI")]
    public GameObject authPanel;     
    public GameObject loginPanel;    
    public GameObject registerPanel; 
    public GameObject profilePanel;

    [Header("Input Field Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;

    [Header("Input Field Registrazione")]
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;
    public TMP_InputField registerConfirmPassword;

    [Header("UI Profilo Loggato")] 
    public TextMeshProUGUI profileEmailText;
    public TextMeshProUGUI profileScoreText;

    [Header("Feedback Visivo")]
    public TextMeshProUGUI messageText; 

    private string currentEmail = ""; 
    private string tempPassword = ""; // Serve per memorizzare la password per l'Auto-Login

    void Start()
    {
        if (authPanel != null) authPanel.SetActive(false);

        // --- RISOLTO PUNTO 2 e 3: AUTO-LOGIN SILENTE ---
        // Se troviamo dati salvati e non siamo loggati, facciamo l'accesso in background
        if (PlayerPrefs.HasKey("AuthEmail") && !PlayFabClientAPI.IsClientLoggedIn())
        {
            AutoLogin();
        }
        else if (PlayFabClientAPI.IsClientLoggedIn())
        {
            currentEmail = PlayerPrefs.GetString("AuthEmail", "");
        }
    }

    void AutoLogin()
    {
        currentEmail = PlayerPrefs.GetString("AuthEmail");
        var request = new LoginWithEmailAddressRequest {
            Email = PlayerPrefs.GetString("AuthEmail"),
            Password = PlayerPrefs.GetString("AuthPass")
        };
        
        PlayFabClientAPI.LoginWithEmailAddress(request, res => {
            // Login automatico riuscito, riscarichiamo il best score dal cloud
            LoadBestScoreFromCloud();
        }, err => {
            // Se fallisce (es. ha cambiato password su un altro PC), cancelliamo l'auto-login
            PlayerPrefs.DeleteKey("AuthEmail");
            PlayerPrefs.DeleteKey("AuthPass");
        });
    }

    // --- GESTIONE INTERFACCIA ---
    public void OpenAuthPanel() 
    { 
        authPanel.SetActive(true); 
        messageText.text = ""; 
        
        if (PlayFabClientAPI.IsClientLoggedIn() || PlayerPrefs.HasKey("AuthEmail")) 
            ShowProfile();
        else 
            ShowLogin(); 
    }
    
    public void CloseAuthPanel() { authPanel.SetActive(false); messageText.text = ""; }
    
    public void ShowLogin() 
    { 
        profilePanel.SetActive(false); 
        registerPanel.SetActive(false); 
        loginPanel.SetActive(true); 
    }
    
    public void ShowRegister() 
    { 
        profilePanel.SetActive(false); 
        loginPanel.SetActive(false); 
        registerPanel.SetActive(true); 
        messageText.text = ""; 
    }

    public void ShowProfile() 
    {
        loginPanel.SetActive(false); 
        registerPanel.SetActive(false); 
        profilePanel.SetActive(true); // RISOLTO PUNTO 1: Forza l'apertura del profilo
        messageText.text = ""; 

        // RISOLTO PUNTO 5: Modificato in "Best: "
        profileEmailText.text = "Account:\n" + currentEmail;
        profileScoreText.text = "Best: " + PlayerPrefs.GetInt("HighScore", 0);
    }

    // --- LOGICA PLAYFAB ---
    public void RegisterButton()
    {
        if (registerPassword.text != registerConfirmPassword.text) { messageText.text = "Errore: Le password non coincidono!"; return; }
        if (registerPassword.text.Length < 6) { messageText.text = "Errore: La password deve avere almeno 6 caratteri!"; return; }

        messageText.text = "Registrazione in corso...";
        var request = new RegisterPlayFabUserRequest { Email = registerEmail.text, Password = registerPassword.text, RequireBothUsernameAndEmail = false };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void LoginButton()
    {
        messageText.text = "Accesso in corso...";
        tempPassword = loginPassword.text; // Salviamo la password temporaneamente per l'auto-login
        var request = new LoginWithEmailAddressRequest { Email = loginEmail.text, Password = tempPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    public void LogoutButton()
    {
        PlayFabClientAPI.ForgetAllCredentials(); 
        currentEmail = ""; 
        
        // Cancelliamo i dati dell'account e dell'Auto-Login
        PlayerPrefs.DeleteKey("HighScore"); 
        PlayerPrefs.DeleteKey("AuthEmail");
        PlayerPrefs.DeleteKey("AuthPass");

        // Resettiamo la skin al logout per sicurezza
        PlayerPrefs.SetInt("SelectedSkin", 0);
        
        var menu = FindAnyObjectByType<MainMenuManager>();
        if (menu != null)
        {
            menu.highScoreText.text = "Best: 0";
            menu.SelectSkin(0); // Aggiorna visivamente lo sfondo e i bottoni
        }

        ShowLogin(); 
        messageText.text = "Disconnesso con successo.";
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        var emailReq = new AddOrUpdateContactEmailRequest { EmailAddress = registerEmail.text };
        PlayFabClientAPI.AddOrUpdateContactEmail(emailReq, res => {}, err => {});

        ShowLogin(); 
        messageText.text = "Registrato! Controlla la mail per confermare.";
    }

    void OnLoginSuccess(LoginResult result)
    {
        currentEmail = loginEmail.text;
        messageText.text = "Controllo verifica email...";
        CheckEmailVerification();
    }

    void CheckEmailVerification()
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest {
            ProfileConstraints = new PlayerProfileViewConstraints { ShowContactEmailAddresses = true }
        }, 
        result => 
        {
            bool isVerified = false;
            if (result.PlayerProfile != null && result.PlayerProfile.ContactEmailAddresses != null)
            {
                foreach (var contact in result.PlayerProfile.ContactEmailAddresses)
                {
                    if (contact.VerificationStatus == EmailVerificationStatus.Confirmed)
                    {
                        isVerified = true;
                        break;
                    }
                }
            }

            if (isVerified)
            {
                // Salviamo le credenziali nel dispositivo per le prossime volte (Auto-Login)
                PlayerPrefs.SetString("AuthEmail", currentEmail);
                PlayerPrefs.SetString("AuthPass", tempPassword);
                PlayerPrefs.Save();

                messageText.text = "Caricamento Best Score...";
                LoadBestScoreFromCloud();
            }
            else
            {
                PlayFabClientAPI.ForgetAllCredentials();
                currentEmail = "";
                ShowLogin(); 
                messageText.text = "ERRORE: Devi confermare l'email dal link ricevuto!"; 
            }
        }, 
        error => 
        {
            ShowLogin();
            messageText.text = "Errore durante il controllo del profilo.";
            Debug.LogError("ERRORE PLAYFAB PROFILO: " + error.GenerateErrorReport());
        });
    }

    void OnError(PlayFabError error)
    {
        messageText.text = "Errore: " + error.ErrorMessage;
    }

    // --- SISTEMA DI CLOUD SAVE E CONTROLLO SKIN ---
    public void LoadBestScoreFromCloud()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
        result => 
        {
            int cloudScore = 0;
            foreach (var stat in result.Statistics)
            {
                if (stat.StatisticName == "HighScore") cloudScore = stat.Value;
            }

            PlayerPrefs.SetInt("HighScore", cloudScore);

            // --- RISOLTO PUNTO 4: CONTROLLO SKIN ILLEGALE ---
            int currentSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
            int[] thresholds = { 0, 300, 600, 900, 1200 }; // Le tue soglie di sblocco
            
            // Se hai una skin equipaggiata ma il tuo cloudScore è troppo basso per averla...
            if (currentSkin > 0 && currentSkin < thresholds.Length)
            {
                if (cloudScore < thresholds[currentSkin])
                {
                    PlayerPrefs.SetInt("SelectedSkin", 0); // Te la tolgo e metto il bianco (0)
                }
            }
            PlayerPrefs.Save();

            // Aggiorniamo la UI del Main Menu
            var menu = FindAnyObjectByType<MainMenuManager>();
            if (menu != null)
            {
                menu.highScoreText.text = "Best: " + cloudScore;
                menu.SelectSkin(PlayerPrefs.GetInt("SelectedSkin", 0)); // Applica il colore giusto
            }

            // Se il pannello account è aperto, mostra subito il profilo aggiornato
            if (authPanel.activeSelf) 
            {
                ShowProfile(); 
            }
        },
        error => 
        { 
            if (authPanel.activeSelf) ShowProfile(); 
        });
    }

    public static void SaveBestScoreToCloud(int newScore)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return; 

        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate> {
                new StatisticUpdate { StatisticName = "HighScore", Value = newScore }
            }
        };

        PlayFabClientAPI.UpdatePlayerStatistics(request, res => Debug.Log("Best Score Cloud OK"), err => Debug.Log("Errore Cloud"));
    }
}