using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;

public class PlayFabAuth : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject authPanel;     
    public GameObject loginPanel;    
    public GameObject registerPanel; 
    public GameObject profilePanel;

    [Header("Login & Register Input Fields")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;
    public TMP_InputField registerConfirmPassword;

    [Header("Logged Profile UI")] 
    public TextMeshProUGUI profileEmailText;
    public TextMeshProUGUI profileScoreText;

    [Header("Visual Feedback")]
    public TextMeshProUGUI messageText; 

    [Header("--- LEADERBOARD & NICKNAME ---")]
    public GameObject leaderboardPanel;
    public GameObject nicknamePanel;
    public TMP_InputField nicknameInput;
    public TextMeshProUGUI leaderboardText;
    public TextMeshProUGUI currentPlayerNameText; 

    private string currentEmail = ""; 
    private string tempPassword = ""; 
    private bool hasDisplayName = false; 
    private string currentDisplayName = ""; 

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
            
            // --- FIX: Reload Nickname from memory when changing scenes! ---
            currentDisplayName = PlayerPrefs.GetString("DisplayName", "");
            hasDisplayName = !string.IsNullOrEmpty(currentDisplayName);
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

    // --- UI MANAGEMENT ---
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

    // --- PLAYFAB LOGIC ---
    public void RegisterButton()
    {
        if (registerPassword.text != registerConfirmPassword.text) { messageText.text = "Error: Passwords do not match!"; return; }
        if (registerPassword.text.Length < 6) { messageText.text = "Error: Password must be at least 6 characters long!"; return; }

        messageText.text = "Registering...";
        var request = new RegisterPlayFabUserRequest { Email = registerEmail.text, Password = registerPassword.text, RequireBothUsernameAndEmail = false };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void LoginButton()
    {
        messageText.text = "Logging in...";
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
        
        PlayerPrefs.DeleteKey("HighScore"); 
        PlayerPrefs.DeleteKey("AuthEmail"); 
        PlayerPrefs.DeleteKey("AuthPass");
        PlayerPrefs.DeleteKey("DisplayName"); // Clear the name on logout!
        PlayerPrefs.SetInt("SelectedSkin", 0);
        
        var menu = FindAnyObjectByType<MainMenuManager>();
        if (menu != null) { menu.highScoreText.text = "Best: 0"; menu.SelectSkin(0); }

        ShowLogin(); 
        messageText.text = "Logged out successfully.";
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        var emailReq = new AddOrUpdateContactEmailRequest { EmailAddress = registerEmail.text };
        PlayFabClientAPI.AddOrUpdateContactEmail(emailReq, res => {}, err => {});
        ShowLogin(); 
        messageText.text = "Registered! Check your email to confirm.";
    }

    void OnLoginSuccess(LoginResult result)
    {
        currentEmail = loginEmail.text;
        messageText.text = "Checking email verification...";
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
                // Save the nickname in memory if it already exists on the servers
                currentDisplayName = result.PlayerProfile.DisplayName != null ? result.PlayerProfile.DisplayName : "";
                hasDisplayName = !string.IsNullOrEmpty(currentDisplayName);
                PlayerPrefs.SetString("DisplayName", currentDisplayName);

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
                messageText.text = "Loading Best Score...";
                LoadBestScoreFromCloud();
            }
            else
            {
                // Automatic email resend if not verified
                var emailReq = new AddOrUpdateContactEmailRequest { EmailAddress = currentEmail };
                PlayFabClientAPI.AddOrUpdateContactEmail(emailReq, 
                    res => Debug.Log("SUCCESS: New email sent!"), 
                    err => Debug.LogError("EMAIL SEND ERROR: " + err.GenerateErrorReport()));

                PlayFabClientAPI.ForgetAllCredentials();
                currentEmail = "";
                ShowLogin(); 
                messageText.text = "Email not confirmed!\nWe just sent you a NEW link. Check your Spam folder too!"; 
            }
        }, 
        error => { ShowLogin(); messageText.text = "Error checking profile."; });
    }

    void OnError(PlayFabError error) { messageText.text = "Error: " + error.ErrorMessage; }

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
        PlayFabClientAPI.UpdatePlayerStatistics(request, res => Debug.Log("Cloud Best Score OK"), err => Debug.Log("Cloud Error"));
    }

    // --- LEADERBOARD AND NICKNAME SYSTEM ---

    public void OpenLeaderboard()
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            OpenAuthPanel();
            messageText.text = "You must log in first to see the Global Leaderboard!";
            return;
        }

        leaderboardPanel.SetActive(true);

        if (!hasDisplayName)
        {
            nicknamePanel.SetActive(true);
            leaderboardText.text = "Choose a Nickname to join the leaderboard!";
            if (currentPlayerNameText != null) currentPlayerNameText.text = ""; 
        }
        else
        {
            nicknamePanel.SetActive(false);
            leaderboardText.text = "Loading leaderboard...";
            if (currentPlayerNameText != null) currentPlayerNameText.text = "Nickname:\n" + currentDisplayName; 
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
            leaderboardText.text = "Nickname must be at least 3 characters long!";
            return;
        }

        leaderboardText.text = "Saving Nickname...";

        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = nicknameInput.text };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, 
        result => 
        {
            currentDisplayName = nicknameInput.text; 
            hasDisplayName = true;
            
            // --- FIX: Save the name locally as soon as you create it! ---
            PlayerPrefs.SetString("DisplayName", currentDisplayName);
            PlayerPrefs.Save();
            
            nicknamePanel.SetActive(false);
            if (currentPlayerNameText != null) currentPlayerNameText.text = "Nickname:\n" + currentDisplayName;

            FetchLeaderboardData();
        }, 
        error => 
        {
            leaderboardText.text = "Error: Nickname invalid or already in use.";
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
            string classifica = "--- GLOBAL TOP 10 ---\n\n";
            foreach (var player in result.Leaderboard)
            {
                string nome = string.IsNullOrEmpty(player.DisplayName) ? "Anonymous" : player.DisplayName;
                classifica += (player.Position + 1) + ". " + nome + " - " + player.StatValue + "\n";
            }
            
            leaderboardText.text = classifica;
        }, 
        error => 
        {
            leaderboardText.text = "Error connecting to leaderboard.";
        });
    }
}