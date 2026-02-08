using UnityEngine;

/// <summary>
/// Singleton GameManager for score tracking, game state management,
/// and "Increasing Tension" (The Trace) speed/atmosphere system.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private int score = 0;
    [SerializeField] private bool isGameOver = false;

    [Header("UI References (Optional)")]
    [Tooltip("Assign a UI Text component to display score")]
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    
    [Tooltip("Assign a GameOver panel to show on death")]
    [SerializeField] private GameObject gameOverPanel;

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
    [SerializeField] private Color stealthColor = new Color(0.4f, 0.6f, 1f); // Cool Blue
    
    [Tooltip("Warning Red color (Detected - high alert)")]
    [SerializeField] private Color detectedColor = new Color(1f, 0.3f, 0.3f); // Warning Red
    
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

    // Events for other scripts to subscribe to
    public System.Action<int> OnScoreChanged;
    public System.Action OnGameOver;
    public System.Action<float> OnSpeedChanged;
    public System.Action<float> OnTensionChanged;

    public int Score => score;
    public bool IsGameOver => isGameOver;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Initialize speed to base speed
        currentScrollSpeed = baseScrollSpeed;
        
        // Initialize UI
        UpdateScoreUI();
        
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Initialize light color to stealth mode
        UpdateLightColor();
    }

    private void Update()
    {
        if (isGameOver) return;
        
        // Time-based speed increase
        timeSinceLastSpeedIncrease += Time.deltaTime;
        
        if (timeSinceLastSpeedIncrease >= speedIncreaseInterval)
        {
            IncreaseSpeed();
            timeSinceLastSpeedIncrease = 0f;
        }
    }

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

    /// <summary>
    /// Adds points to the score.
    /// </summary>
    /// <param name="points">Points to add (default 1)</param>
    public void AddScore(int points = 1)
    {
        if (isGameOver) return;
        
        score += points;
        UpdateScoreUI();
        OnScoreChanged?.Invoke(score);
        
        Debug.Log($"Score: {score}");
    }

    /// <summary>
    /// Triggers game over state - System Format (virus detected).
    /// </summary>
    public void GameOver()
    {
        if (isGameOver) return;
        
        isGameOver = true;
        
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
    /// Updates the score UI text.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";
    }

    /// <summary>
    /// Resets the score and tension (for game restart without scene reload).
    /// </summary>
    public void ResetScore()
    {
        score = 0;
        isGameOver = false;
        
        // Reset tension system
        currentScrollSpeed = baseScrollSpeed;
        currentTensionLevel = 0f;
        barriersPassed = 0;
        timeSinceLastSpeedIncrease = 0f;
        barriersSinceLastSpeedIncrease = 0;
        totalSpeedIncreases = 0;
        
        UpdateScoreUI();
        UpdateLightColor();
    }
}
