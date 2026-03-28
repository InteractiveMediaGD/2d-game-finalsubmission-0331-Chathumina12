using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the UI interactions for logging an account in via AuthManager.
/// </summary>
public class LoginUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private TextMeshProUGUI errorText;

    private void Start()
    {
        if (loginButton != null)
        {
            loginButton.onClick.AddListener(AttemptLogin);
        }

        if (errorText != null)
        {
            errorText.text = "";
        }

        // If AuthManager remembers the user, pre-fill it!
        string rememberedUser = PlayerPrefs.GetString("LastLoggedInUser", "");
        if (!string.IsNullOrEmpty(rememberedUser) && usernameInput != null)
        {
            usernameInput.text = rememberedUser;
        }
    }

    private void AttemptLogin()
    {
        if (AuthManager.Instance == null)
        {
            ShowError("System Error: AuthManager not found!");
            return;
        }

        string rawName = usernameInput != null ? usernameInput.text : "";
        string cleanedName = rawName.Trim();

        // 1. Validation
        if (string.IsNullOrEmpty(cleanedName))
        {
            ShowError("Username cannot be empty.");
            return;
        }

        if (cleanedName.Length < 3)
        {
            ShowError("Username must be at least 3 characters.");
            return;
        }

        if (cleanedName.Length > 12)
        {
            ShowError("Username max limit is 12 characters.");
            return;
        }

        // 2. Auth Flow
        ShowError(""); // Clear errors
        
        AuthManager.Instance.Login(cleanedName);
        
        // Hide this panel! The MainMenuController usually handles activating the Main Menu after this.
        gameObject.SetActive(false);
    }

    private void ShowError(string msg)
    {
        if (errorText != null)
            errorText.text = msg;
        else
            Debug.LogWarning($"[LoginUI Error]: {msg}");
    }
}
