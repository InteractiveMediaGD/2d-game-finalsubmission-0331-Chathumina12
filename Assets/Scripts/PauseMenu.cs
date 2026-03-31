using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles pausing, resuming, restarting, and adjusting audio.
/// Place this script on an empty UI Manager object or the Canvas in your GameScene.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("The parent GameObject representing your Pause Menu visual panel.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Audio Sliders")]
    [Tooltip("Slider for controlling Background Music volume.")]
    [SerializeField] private Slider musicSlider;
    
    [Tooltip("Slider for controlling Sound Effects volume.")]
    [SerializeField] private Slider sfxSlider;

    [Header("Buttons")]
    [Tooltip("The Resume Button")]
    [SerializeField] private Button resumeButton;

    [Tooltip("The Restart Button")]
    [SerializeField] private Button restartButton;

    [Tooltip("The Main Menu Button")]
    [SerializeField] private Button mainMenuButton;

    // Track state to prevent toggling if game is over
    private bool isPaused = false;

    private void Start()
    {
        // Ensure the pause menu starts hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Initialize sliders with the current actual AudioManager values
        if (AudioManager.Instance != null)
        {
            if (musicSlider != null)
            {
                musicSlider.value = AudioManager.Instance.musicVolume;
                // Add listener to automatically update audio when slider moves
                musicSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            if (sfxSlider != null)
            {
                sfxSlider.value = AudioManager.Instance.sfxVolume;
                // Add listener to automatically update audio when slider moves
                sfxSlider.onValueChanged.AddListener(SetSFXVolume);
            }
        }

        // Dynamically add button listeners so we don't have to rely on tricky Editor bindings.
        if (resumeButton != null) resumeButton.onClick.AddListener(Resume);
        if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(LoadMainMenu);
    }

    private void Update()
    {
        // Don't allow pausing if the player is dead/GameOver
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver) return;

        // Toggle pause on Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    /// <summary>
    /// Resumes the game and hides the UI.
    /// Link this to your Resume Button's OnClick event.
    /// </summary>
    public void Resume()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        
        // Optional: Play a menu sound
        AudioManager.Play("menu_back");
    }

    /// <summary>
    /// Pauses the game by freezing time scaling and shows the UI.
    /// </summary>
    public void Pause()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        AudioManager.Play("btn_hover");
    }

    /// <summary>
    /// Wrapper for restarting. Use this on your GameOver or Pause Panel Restart Button.
    /// </summary>
    public void RestartGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        AudioManager.Play("btn_click");

        // Prefer GameManager logic if available, otherwise force reload
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    /// <summary>
    /// Exits back to the Main Menu scene. Link this to your Main Menu Button.
    /// </summary>
    public void LoadMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANT: Must reset time scale or the next scene will be frozen!
        isPaused = false;
        AudioManager.Play("btn_click");
        
        // "MainMenu" should be the exact name of your main menu scene file (without .unity extension)
        SceneManager.LoadScene("MainMenu"); 
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Real-time Audio Settings (triggered automatically if Sliders are assigned)
    // ─────────────────────────────────────────────────────────────────────────

    public void SetMusicVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(volume);
        }
    }
}
