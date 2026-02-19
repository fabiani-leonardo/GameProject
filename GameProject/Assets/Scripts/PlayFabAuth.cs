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

    [Header("Input Field Login & Register")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;
    public TMP_InputField registerConfirmPassword;

    [Header("UI Profilo Loggato")] 
    public TextMeshProUGUI profileEmailText;
    public TextMeshProUGUI profileScoreText;

    [Header("Feedback Visivo")]
    public TextMeshProUGUI messageText; 

    [Header("--- CLASSIFICA E NICKNAME ---")]
    public GameObject leaderboardPanel;
    public GameObject nicknamePanel;
    public TMP_InputField nicknameInput;
    public TextMeshProUGUI leaderboardText;
    public TextMeshProUGUI currentPlayerNameText; // <-- NUOVO: Il testo in alto a destra

    private string currentEmail = ""; 
    private string tempPassword = ""; 
    private bool hasDisplayName = false; 
    private string currentDisplayName = ""; // <-- NUOVO: Salva il nome in memoria

    void Start()
    {
        if (authPanel != null) authPanel.SetActive(false);
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);

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
        var request = new LoginWithEmailAddressRequest { Email = currentEmail, Password = PlayerPrefs.GetString("AuthPass") };
        
        PlayFabClientAPI.LoginWithEmailAddress(request, res => {
            CheckEmailVerification(); 
        }, err => {
            PlayerPrefs.DeleteKey("AuthEmail");
            PlayerPrefs.DeleteKey("AuthPass");
        });
    }

    // --- GESTIONE INTERFACCIA ---
    public void OpenAuthPanel() 
    { 
        authPanel.SetActive(true); 
        messageText.text = ""; 
        if (PlayFabClientAPI.IsClientLoggedIn() || PlayerPrefs.HasKey("AuthEmail")) ShowProfile();
        else ShowLogin(); 
    }
    
    public void CloseAuthPanel() { authPanel.SetActive(false); messageText.text = ""; }
    public void ShowLogin() { profilePanel.SetActive(false); registerPanel.SetActive(false); loginPanel.SetActive(true); }
    public void ShowRegister() { profilePanel.SetActive(false); loginPanel.SetActive(false); registerPanel.SetActive(true); messageText.text = ""; }

    public void ShowProfile() 
    {
        loginPanel.SetActive(false); registerPanel.SetActive(false); profilePanel.SetActive(true); messageText.text = ""; 
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
        tempPassword = loginPassword.text; 
        var request = new LoginWithEmailAddressRequest { Email = loginEmail.text, Password = tempPassword };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    public void LogoutButton()
    {
        PlayFabClientAPI.ForgetAllCredentials(); 
        currentEmail = ""; 
        hasDisplayName = false;
        currentDisplayName = "";
        
        PlayerPrefs.DeleteKey("HighScore"); PlayerPrefs.DeleteKey("AuthEmail"); PlayerPrefs.DeleteKey("AuthPass");
        PlayerPrefs.SetInt("SelectedSkin", 0);
        
        var menu = FindAnyObjectByType<MainMenuManager>();
        if (menu != null) { menu.highScoreText.text = "Best: 0"; menu.SelectSkin(0); }

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
            ProfileConstraints = new PlayerProfileViewConstraints { ShowContactEmailAddresses = true, ShowDisplayName = true }
        }, 
        result => 
        {
            bool isVerified = false;
            if (result.PlayerProfile != null)
            {
                // Salviamo il nickname se esiste già
                currentDisplayName = result.PlayerProfile.DisplayName;
                hasDisplayName = !string.IsNullOrEmpty(currentDisplayName);

                if (result.PlayerProfile.ContactEmailAddresses != null)
                {
                    foreach (var contact in result.PlayerProfile.ContactEmailAddresses)
                    {
                        if (contact.VerificationStatus == EmailVerificationStatus.Confirmed) { isVerified = true; break; }
                    }
                }
            }

            if (isVerified)
            {
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
        error => { ShowLogin(); messageText.text = "Errore durante il controllo del profilo."; });
    }

    void OnError(PlayFabError error) { messageText.text = "Errore: " + error.ErrorMessage; }

    // --- CLOUD SAVE ---
    public void LoadBestScoreFromCloud()
    {
        PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(),
        result => 
        {
            int cloudScore = 0;
            foreach (var stat in result.Statistics) { if (stat.StatisticName == "HighScore") cloudScore = stat.Value; }
            PlayerPrefs.SetInt("HighScore", cloudScore);

            int currentSkin = PlayerPrefs.GetInt("SelectedSkin", 0);
            int[] thresholds = { 0, 300, 600, 900, 1200 }; 
            if (currentSkin > 0 && currentSkin < thresholds.Length && cloudScore < thresholds[currentSkin]) PlayerPrefs.SetInt("SelectedSkin", 0);
            
            PlayerPrefs.Save();

            var menu = FindAnyObjectByType<MainMenuManager>();
            if (menu != null) { menu.highScoreText.text = "Best: " + cloudScore; menu.SelectSkin(PlayerPrefs.GetInt("SelectedSkin", 0)); }
            if (authPanel.activeSelf) ShowProfile(); 
        },
        error => { if (authPanel.activeSelf) ShowProfile(); });
    }

    public static void SaveBestScoreToCloud(int newScore)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn()) return; 
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate> { new StatisticUpdate { StatisticName = "HighScore", Value = newScore } } };
        PlayFabClientAPI.UpdatePlayerStatistics(request, res => Debug.Log("Best Score Cloud OK"), err => Debug.Log("Errore Cloud"));
    }

    // --- SISTEMA LEADERBOARD E NICKNAME ---

    public void OpenLeaderboard()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            OpenAuthPanel();
            messageText.text = "Devi prima accedere per vedere la Classifica Globale!";
            return;
        }

        leaderboardPanel.SetActive(true);

        if (!hasDisplayName)
        {
            nicknamePanel.SetActive(true);
            leaderboardText.text = "Scegli un Nickname per partecipare alla classifica!";
            if (currentPlayerNameText != null) currentPlayerNameText.text = ""; // Nasconde il testo
        }
        else
        {
            nicknamePanel.SetActive(false);
            leaderboardText.text = "Caricamento classifica in corso...";
            if (currentPlayerNameText != null) currentPlayerNameText.text = "Nickname:\n" + currentDisplayName; // Mostra il testo
            FetchLeaderboardData();
        }
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    public void SubmitNicknameButton()
    {
        if (nicknameInput.text.Length < 3) 
        {
            leaderboardText.text = "Il Nickname deve avere almeno 3 caratteri!";
            return;
        }

        leaderboardText.text = "Salvataggio Nickname...";

        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = nicknameInput.text };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, 
        result => 
        {
            currentDisplayName = nicknameInput.text; // Salviamo il nuovo nome in memoria
            hasDisplayName = true;
            nicknamePanel.SetActive(false);
            
            // Aggiorniamo il testo in alto a destra
            if (currentPlayerNameText != null) currentPlayerNameText.text = "Nickname:\n" + currentDisplayName;

            FetchLeaderboardData();
        }, 
        error => 
        {
            leaderboardText.text = "Errore: Nickname non valido o già in uso.";
        });
    }

    void FetchLeaderboardData()
    {
        var request = new GetLeaderboardRequest
        {
            StatisticName = "HighScore", 
            StartPosition = 0,
            MaxResultsCount = 10 
        };

        PlayFabClientAPI.GetLeaderboard(request, 
        result => 
        {
            string classifica = "--- TOP 10 GLOBALE ---\n\n";
            foreach (var player in result.Leaderboard)
            {
                string nome = string.IsNullOrEmpty(player.DisplayName) ? "Anonimo" : player.DisplayName;
                classifica += (player.Position + 1) + ". " + nome + " - " + player.StatValue + "\n";
            }
            
            leaderboardText.text = classifica;
        }, 
        error => 
        {
            leaderboardText.text = "Errore di connessione alla classifica.";
        });
    }
}