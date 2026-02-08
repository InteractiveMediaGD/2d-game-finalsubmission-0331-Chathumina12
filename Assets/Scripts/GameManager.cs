using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Singleton GameManager for score tracking, health management, game state,
/// and "Increasing Tension" (The Trace) speed/atmosphere system.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern - ensures only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Score Variables
    [Header("Score Settings")]
    [Tooltip("Current player score")]
    [SerializeField] private int currentScore = 0;
    
    [Tooltip("High score (persisted with PlayerPrefs)")]
    [SerializeField] private int highScore = 0;
    
    [Tooltip("Points earned per second (distance traveled)")]
    [SerializeField] private float scorePerSecond = 10f;
    
    // Accumulator for fractional score
    private float scoreAccumulator = 0f;
    
    // PlayerPrefs key for high score
    private const string HIGH_SCORE_KEY = "HighScore";
    
    // Public accessors
    public int CurrentScore => currentScore;
    public int HighScore => highScore;
    #endregion

    #region Health Variables
    [Header("Health Settings")]
    [Tooltip("Current player health")]
    [SerializeField] private int playerHealth = 100;
    
    [Tooltip("Maximum player health")]
    [SerializeField] private int maxHealth = 100;
    
    // Public accessors
    public int PlayerHealth => playerHealth;
    public int MaxHealth => maxHealth;
    #endregion

    #region Game State
    [Header("Game State")]
    [SerializeField] private bool isGameOver = false;
    
    public bool IsGameOver => isGameOver;
    #endregion

    #region UI References
    [Header("UI References")]
    [Tooltip("TextMeshPro element for displaying current score")]
    [SerializeField] private TextMeshProUGUI scoreText;
    
    [Tooltip("TextMeshPro element for displaying high score")]
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Tooltip("Slider for health bar display")]
    [SerializeField] private Slider healthSlider;
    
    [Tooltip("GameOver panel to show on death")]
    [SerializeField] private GameObject gameOverPanel;
    #endregion

    #region Increasing Tension Settings
    [Header("Increasing Tension - Speed Settings")]
    [Tooltip("Base speed at game start")]
    [SerializeField] private float baseScrollSpeed = 5f;
    
    [Tooltip("Current scroll speed (increases over time)")]
    [SerializeField] private float currentScrollSpeed = 5f;
    
    [Tooltip("Speed increase percentage per interval (0.05 = 5%)")]
    [SerializeField] private float speedIncreasePercent = 0.05f;
    
    [Tooltip("Time interval for speed increase (seconds)")]
    [SerializeField] private float speedIncreaseInterval = 10f;
    
    [Tooltip("Number of barriers to pass before speed increase")]
    [SerializeField] private int barriersForSpeedIncrease = 5;
    
    [Header("Increasing Tension - Light Settings")]
    [Tooltip("Reference to the global light (Directional or 2D Global Light)")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight;
    
    [Tooltip("Cool Blue color (Stealth - undetected)")]
    [SerializeField] private Color stealthColor = new Color(0.4f, 0.6f, 1f);
    
    [Tooltip("Warning Red color (Detected - high alert)")]
    [SerializeField] private Color detectedColor = new Color(1f, 0.3f, 0.3f);
    
    [Tooltip("Maximum tension level (1.0 = fully detected)")]
    [SerializeField] private float maxTensionLevel = 1f;
    
    [Header("Tension Tracking")]
    [SerializeField] private float currentTensionLevel = 0f;
    [SerializeField] private int barriersPassed = 0;
    
    // Internal tracking
    private float timeSinceLastSpeedIncrease = 0f;
    private int barriersSinceLastSpeedIncrease = 0;
    private int totalSpeedIncreases = 0;
    
    // Public accessors
    public float ScrollSpeed => currentScrollSpeed;
    public float TensionLevel => currentTensionLevel;
    public int BarriersPassed => barriersPassed;
    #endregion

    #region Events
    // Events for other scripts to subscribe to
    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnHighScoreChanged;
    public System.Action<int> OnHealthChanged;
    public System.Action OnGameOver;
    public System.Action<float> OnSpeedChanged;
    public System.Action<float> OnTensionChanged;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        // Initialize speed
        currentScrollSpeed = baseScrollSpeed;
        
        // Initialize health
        playerHealth = maxHealth;
        
        // Load high score from PlayerPrefs
        LoadHighScore();
        
        // Initialize UI
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateHealthUI();
        
        // Hide game over panel
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Initialize light color to stealth mode
        UpdateLightColor();
    }

    private void Update()
    {
        if (isGameOver) return;
        
        // Auto-increment score based on time (distance traveled)
        AccumulateDistanceScore();
        
        // Time-based speed increase
        timeSinceLastSpeedIncrease += Time.deltaTime;
        
        if (timeSinceLastSpeedIncrease >= speedIncreaseInterval)
        {
            IncreaseSpeed();
            timeSinceLastSpeedIncrease = 0f;
        }
    }
    #endregion

    #region Score Methods
    /// <summary>
    /// Accumulates score over time based on distance traveled.
    /// </summary>
    private void AccumulateDistanceScore()
    {
        scoreAccumulator += scorePerSecond * Time.deltaTime;
        
        // Add whole points when accumulator reaches 1 or more
        if (scoreAccumulator >= 1f)
        {
            int pointsToAdd = Mathf.FloorToInt(scoreAccumulator);
            currentScore += pointsToAdd;
            scoreAccumulator -= pointsToAdd;
            
            // Check and update high score
            CheckHighScore();
            
            // Update UI
            UpdateScoreUI();
            
            // Invoke event
            OnScoreChanged?.Invoke(currentScore);
        }
    }

    /// <summary>
    /// Adds score for killing enemies or other bonus actions.
    /// </summary>
    /// <param name="amount">Points to add</param>
    public void AddScore(int amount)
    {
        if (isGameOver) return;
        
        currentScore += amount;
        
        // Check and update high score
        CheckHighScore();
        
        // Update UI
        UpdateScoreUI();
        
        // Invoke event
        OnScoreChanged?.Invoke(currentScore);
        
        Debug.Log($"Score added: {amount}. Total: {currentScore}");
    }

    /// <summary>
    /// Checks if current score exceeds high score and updates if necessary.
    /// </summary>
    private void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            UpdateHighScoreUI();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    /// <summary>
    /// Saves high score to PlayerPrefs.
    /// </summary>
    private void SaveHighScore()
    {
        PlayerPrefs.SetInt(HIGH_SCORE_KEY, highScore);
        PlayerPrefs.Save();
        Debug.Log($"High Score saved: {highScore}");
    }

    /// <summary>
    /// Loads high score from PlayerPrefs.
    /// </summary>
    private void LoadHighScore()
    {
        highScore = PlayerPrefs.GetInt(HIGH_SCORE_KEY, 0);
        Debug.Log($"High Score loaded: {highScore}");
    }
    #endregion

    #region Health Methods
    /// <summary>
    /// Modifies player health by the specified amount.
    /// Positive values heal, negative values damage.
    /// Triggers GameOver if health drops to 0 or below.
    /// </summary>
    /// <param name="amount">Amount to modify health (positive = heal, negative = damage)</param>
    public void ModifyHealth(int amount)
    {
        if (isGameOver) return;
        
        // Modify health and clamp between 0 and max
        playerHealth = Mathf.Clamp(playerHealth + amount, 0, maxHealth);
        
        // Update UI
        UpdateHealthUI();
        
        // Invoke event
        OnHealthChanged?.Invoke(playerHealth);
        
        Debug.Log($"Health modified by {amount}. Current: {playerHealth}/{maxHealth}");
        
        // Check for death
        if (playerHealth <= 0)
        {
            GameOver();
        }
    }

    /// <summary>
    /// Deals damage to the player.
    /// </summary>
    /// <param name="damage">Amount of damage (positive value)</param>
    public void TakeDamage(int damage)
    {
        ModifyHealth(-Mathf.Abs(damage));
    }

    /// <summary>
    /// Heals the player.
    /// </summary>
    /// <param name="healAmount">Amount to heal (positive value)</param>
    public void Heal(int healAmount)
    {
        ModifyHealth(Mathf.Abs(healAmount));
    }
    #endregion

    #region UI Update Methods
    /// <summary>
    /// Updates the score text UI.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";
    }

    /// <summary>
    /// Updates the high score text UI.
    /// </summary>
    private void UpdateHighScoreUI()
    {
        if (highScoreText != null)
            highScoreText.text = $"High Score: {highScore}";
    }

    /// <summary>
    /// Updates the health slider UI.
    /// </summary>
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = playerHealth;
        }
    }
    #endregion

    #region Game State Methods
    /// <summary>
    /// Triggers game over state - System Format (virus detected).
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
        // Ensure high score is saved
        CheckHighScore();
        SaveHighScore();
        
        // Pause the game
        Time.timeScale = 0f;
        
        // Show game over UI
        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);
        
        // Invoke event for other systems
        OnGameOver?.Invoke();
        
        Debug.Log("GAME OVER - System Format Initiated!");
    }

    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    public void RestartGame()
    {
        // Reset time scale
        Time.timeScale = 1f;
        isGameOver = false;
        
        // Reload current scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    /// <summary>
    /// Resets all game values without reloading the scene.
    /// </summary>
    public void ResetGame()
    {
        // Reset score (but keep high score)
        currentScore = 0;
        scoreAccumulator = 0f;
        
        // Reset health
        playerHealth = maxHealth;
        
        // Reset game state
        isGameOver = false;
        
        // Reset tension system
        currentScrollSpeed = baseScrollSpeed;
        currentTensionLevel = 0f;
        barriersPassed = 0;
        timeSinceLastSpeedIncrease = 0f;
        barriersSinceLastSpeedIncrease = 0;
        totalSpeedIncreases = 0;
        
        // Update all UI
        UpdateScoreUI();
        UpdateHighScoreUI();
        UpdateHealthUI();
        UpdateLightColor();
    }
    #endregion

    #region Tension System Methods
    /// <summary>
    /// Called when the player passes a barrier.
    /// Tracks barrier count and triggers speed increase every N barriers.
    /// </summary>
    public void OnBarrierPassed()
    {
        if (isGameOver) return;
        
        barriersPassed++;
        barriersSinceLastSpeedIncrease++;
        
        // Barrier-based speed increase
        if (barriersSinceLastSpeedIncrease >= barriersForSpeedIncrease)
        {
            IncreaseSpeed();
            barriersSinceLastSpeedIncrease = 0;
        }
        
        Debug.Log($"Barrier passed! Total: {barriersPassed}");
    }

    /// <summary>
    /// Increases the scroll speed by the configured percentage.
    /// Also increases tension level and updates the atmosphere.
    /// </summary>
    private void IncreaseSpeed()
    {
        totalSpeedIncreases++;
        
        // Increase speed by percentage
        currentScrollSpeed *= (1f + speedIncreasePercent);
        
        // Increase tension level (caps at maxTensionLevel)
        currentTensionLevel = Mathf.Min(currentTensionLevel + 0.1f, maxTensionLevel);
        
        // Update atmosphere (light color)
        UpdateLightColor();
        
        // Invoke events
        OnSpeedChanged?.Invoke(currentScrollSpeed);
        OnTensionChanged?.Invoke(currentTensionLevel);
        
        Debug.Log($"TENSION INCREASING! Speed: {currentScrollSpeed:F2}, Tension: {currentTensionLevel:P0}");
    }

    /// <summary>
    /// Updates the global light color based on current tension level.
    /// Transitions from Cool Blue (stealth) to Warning Red (detected).
    /// </summary>
    private void UpdateLightColor()
    {
        if (globalLight == null) return;
        
        // Lerp between stealth and detected colors based on tension
        Color targetColor = Color.Lerp(stealthColor, detectedColor, currentTensionLevel);
        globalLight.color = targetColor;
    }
    #endregion
}
