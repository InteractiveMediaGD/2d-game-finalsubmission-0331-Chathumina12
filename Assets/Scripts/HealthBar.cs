using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Polished health bar that smoothly lerps its fill and colour.
/// Colour: Green (full) → Yellow (half) → Red (critical).
/// Attach this to the health bar's root GameObject.
/// </summary>
public class HealthBar : MonoBehaviour
{
    [Header("Fill Image")]
    [Tooltip("Assign the fill Image (Fill Method = Horizontal, Fill Origin = Left)")]
    [SerializeField] private Image fillImage;

    [Header("Border / Background")]
    [Tooltip("Optional background / border image from Cyberpunk bar assets")]
    [SerializeField] private Image backgroundImage;

    [Header("HP Label")]
    [Tooltip("Optional TMP text showing numeric HP")]
    [SerializeField] private TextMeshProUGUI hpLabel;

    [Header("Colour Gradient")]
    [SerializeField] private Color colorFull     = new Color(0.10f, 0.95f, 0.40f); // vivid green
    [SerializeField] private Color colorHalf     = new Color(1.00f, 0.85f, 0.00f); // gold-yellow
    [SerializeField] private Color colorCritical = new Color(1.00f, 0.18f, 0.18f); // danger red

    [Header("Animation")]
    [Tooltip("How fast the bar lerps toward the target value (units/sec)")]
    [SerializeField] private float lerpSpeed = 6f;

    [Header("Low-HP Pulse")]
    [Tooltip("HP percentage below which the bar starts pulsing")]
    [SerializeField] [Range(0f, 1f)] private float pulseThreshold = 0.30f;
    [Tooltip("How fast the bar pulses (Hz)")]
    [SerializeField] private float pulseFrequency = 2.5f;
    [Tooltip("Brightness amplitude of the pulse")]
    [SerializeField] [Range(0f, 0.5f)] private float pulseAmplitude = 0.20f;

    // Internal state
    private float displayedFill = 1f;   // what we are currently rendering
    private float targetFill    = 1f;   // what we want to reach

    private void Awake()
    {
        if (fillImage != null)
        {
            fillImage.type      = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.fillAmount = 1f;
        }

        // Subscribe to GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHealthChanged += HandleHealthChanged;
            // Initialise immediately
            HandleHealthChanged(GameManager.Instance.PlayerHealth);
        }
    }

    private void Start()
    {
        // Fallback subscription in case Awake fires before GameManager.Instance is set
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHealthChanged += HandleHealthChanged;
            HandleHealthChanged(GameManager.Instance.PlayerHealth);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        if (fillImage == null) return;

        // Smooth lerp toward target
        displayedFill = Mathf.Lerp(displayedFill, targetFill, Time.deltaTime * lerpSpeed);
        fillImage.fillAmount = displayedFill;

        // Colour interpolation: full→half uses green→yellow, half→zero uses yellow→red
        Color targetColor;
        if (displayedFill > 0.5f)
            targetColor = Color.Lerp(colorHalf, colorFull, (displayedFill - 0.5f) * 2f);
        else
            targetColor = Color.Lerp(colorCritical, colorHalf, displayedFill * 2f);

        // Low-HP pulse (brightness oscillation)
        if (displayedFill <= pulseThreshold)
        {
            float pulse = 1f + pulseAmplitude * Mathf.Sin(Time.time * pulseFrequency * Mathf.PI * 2f);
            targetColor = new Color(
                Mathf.Clamp01(targetColor.r * pulse),
                Mathf.Clamp01(targetColor.g * pulse),
                Mathf.Clamp01(targetColor.b * pulse),
                1f);
        }

        fillImage.color = targetColor;
    }

    /// <summary>
    /// Called by GameManager.OnHealthChanged. Accepted value is the absolute HP integer.
    /// </summary>
    private void HandleHealthChanged(int currentHP)
    {
        int maxHP = GameManager.Instance != null ? GameManager.Instance.MaxHealth : 100;
        targetFill = Mathf.Clamp01((float)currentHP / Mathf.Max(1, maxHP));

        if (hpLabel != null)
            hpLabel.text = $"{currentHP} / {maxHP}";
    }

    /// <summary>
    /// Public hook for setting the bar externally without a GameManager.
    /// </summary>
    public void SetHealth(int current, int max)
    {
        targetFill = Mathf.Clamp01((float)current / Mathf.Max(1, max));
        if (hpLabel != null)
            hpLabel.text = $"{current} / {max}";
    }
}
