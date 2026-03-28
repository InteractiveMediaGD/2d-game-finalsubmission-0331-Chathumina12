using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles the logic for the precise "MainMenu" scene in the two-scene architecture.
/// Displays High Score and transitions to the "GameScene".
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The parent container for the main menu UI")]
    [SerializeField] private GameObject mainMenuPanel;
    
    [Tooltip("The parent container for the Store UI")]
    [SerializeField] private GameObject storePanel;
    
    [Tooltip("The parent container for the Difficulty Selection UI")]
    [SerializeField] private GameObject difficultyPanel;

    [Header("UI Elements")]
    [Tooltip("Text element displaying the local High Score")]
    [SerializeField] private TextMeshProUGUI highScoreText;

    [Header("Data Fragments Counter")]
    [Tooltip("Text element displaying the live Data Fragments balance on the main menu")]
    [SerializeField] private TextMeshProUGUI dataFragmentsText;

    [Header("Buttons")]
    [SerializeField] private Button storeButton;
    [SerializeField] private Button selectDifficultyButton; // Replaces 'playButton' to just open the diff panel
    
    [Header("Difficulty Buttons")]
    [SerializeField] private Button btnEasy;
    [SerializeField] private Button btnMedium;
    [SerializeField] private Button btnHard;

    [Tooltip("Buttons to close sub-menus and return to main menu")]
    [SerializeField] private Button closeStoreButton;
    [SerializeField] private Button closeDifficultyButton;
    [SerializeField] private Button quitButton;

    // PlayerPrefs Key for high score MUST match GameManager's key
    private const string HIGH_SCORE_KEY = "HighScore";

    private void Start()
    {
        // Go straight to the Main Menu (skipping auth)
        openMainMenu();

        // Assign button listeners (code-side fallback — Inspector OnClick wiring is also valid)
        if (selectDifficultyButton != null) selectDifficultyButton.onClick.AddListener(OpenDifficulty);
        if (storeButton != null) storeButton.onClick.AddListener(OpenShop);
        
        // Difficulty listeners
        if (btnEasy != null) btnEasy.onClick.AddListener(() => SetDifficultyAndPlay(0));
        if (btnMedium != null) btnMedium.onClick.AddListener(() => SetDifficultyAndPlay(1));
        if (btnHard != null) btnHard.onClick.AddListener(() => SetDifficultyAndPlay(2));
        
        if (closeStoreButton != null) closeStoreButton.onClick.AddListener(GoBackToMenu);
        if (closeDifficultyButton != null) closeDifficultyButton.onClick.AddListener(GoBackToMenu);
        
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }

    private void UpdateHighScoreText()
    {
        int highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        
        if (highScoreText != null)
        {
            highScoreText.text = $"High Score: {highScore}";
        }
    }

    /// <summary>
    /// Refreshes the Data Fragments counter from EconomyManager.
    /// </summary>
    private void UpdateDataFragmentsUI()
    {
        if (dataFragmentsText == null) return;
        int balance = EconomyManager.Instance != null ? EconomyManager.Instance.GetBalance() : 0;
        dataFragmentsText.text = $"\u25c6 {balance} Fragments";
    }

    // ─────────────────────────────────────────────────────────────
    // Public button methods — wire these directly in the Inspector
    // OnClick event lists so no missing-reference issues occur.
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Saves the selected difficulty (0=Easy, 1=Medium, 2=Hard) and loads GameScene.
    /// </summary>
    public void SetDifficultyAndPlay(int level)
    {
        PlayerPrefs.SetInt("DifficultyLevel", level);
        PlayerPrefs.Save();
        Debug.Log($"[MainMenuController] Difficulty chosen: {level}. Loading GameScene...");
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Opens the Difficulty Panel
    /// </summary>
    public void OpenDifficulty()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (storePanel != null) storePanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(true);
    }

    /// <summary>
    /// Opens the Shop panel. Wire to the Shop button's OnClick in the Inspector.
    /// </summary>
    public void OpenShop()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        if (storePanel != null) storePanel.SetActive(true);
    }

    /// <summary>
    /// Returns to the main menu panel. Wire to any Back/Close button's OnClick in the Inspector.
    /// </summary>
    public void GoBackToMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (storePanel != null) storePanel.SetActive(false);
        if (difficultyPanel != null) difficultyPanel.SetActive(false);
        UpdateHighScoreText();
        UpdateDataFragmentsUI();
    }

    // Keep private alias
    private void openMainMenu() => GoBackToMenu();

    /// <summary>
    /// Quits the application. Wire to the Quit button's OnClick in the Inspector.
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
