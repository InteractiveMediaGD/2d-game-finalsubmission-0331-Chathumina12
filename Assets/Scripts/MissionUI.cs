using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Controls a single Mission Card on the screen.
/// Uses native Unity Coroutines for smooth sliding and progress filling!
/// </summary>
public class MissionUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI progressText;
    public Slider progressBar;

    [Header("Neon Style")]
    [Tooltip("Optional header/title label (e.g. 'MISSION' label above description)")]
    public TextMeshProUGUI titleLabel;

    [Tooltip("Fill image of the progress bar slider — tinted neon on update")]
    public Image progressBarFill;

    [Header("Completion VFX")]
    public Image backgroundImage;

    // ── Neon Cyberpunk Colour Palette ─────────────────────────────
    // Dark semi-transparent panel (near-black, 88% opacity)
    public Color defaultColor     = new Color(0.05f, 0.07f, 0.12f, 0.88f);
    // Vivid neon cyan-green on completion
    public Color completionColor  = new Color(0.00f, 0.90f, 0.70f, 0.95f);
    // Neon cyan for progress text and bar
    public Color neonAccentColor  = new Color(0.00f, 1.00f, 0.85f, 1.00f);
    // Soft white for description label
    public Color descriptionColor = new Color(0.85f, 0.90f, 1.00f, 1.00f);
    // Small title label (MISSION tag)
    public Color titleColor       = new Color(0.40f, 0.80f, 1.00f, 0.80f);
    // ──────────────────────────────────────────────────────────────

    private int missionIndex = -1;
    private bool isCompleted = false;
    private Coroutine progressAnimRoutine;

    // Optional sliding rect
    private RectTransform rectTransform;
    private Vector2 hiddenPos = new Vector2(-400, 0); // Slide from left off-screen
    private Vector2 visiblePos = Vector2.zero;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplyNeonStyle();
    }

    /// <summary>
    /// Applies the neon cyberpunk colour palette to all UI text and image elements.
    /// Called once on Awake and again after completion is cleared.
    /// </summary>
    private void ApplyNeonStyle()
    {
        if (backgroundImage != null)
            backgroundImage.color = defaultColor;

        if (descriptionText != null)
            descriptionText.color = descriptionColor;

        if (progressText != null)
            progressText.color = neonAccentColor;

        if (titleLabel != null)
        {
            titleLabel.text  = "MISSION";
            titleLabel.color = titleColor;
        }

        if (progressBarFill != null)
            progressBarFill.color = neonAccentColor;
    }

    public void Setup(int index, MissionData data)
    {
        missionIndex = index;
        isCompleted = false;

        if (descriptionText != null)
            descriptionText.text = data.description;
            
        if (progressText != null)
            progressText.text = $"0 / {data.targetAmount}";
            
        if (progressBar != null)
        {
            progressBar.maxValue = data.targetAmount;
            progressBar.value = 0f;
        }

        // Snap completely off-screen, then slide in!
        rectTransform.anchoredPosition = hiddenPos;
        StartCoroutine(SlideRoutine(visiblePos, 0.5f, index * 0.2f)); // Staggered slide in!
    }

    public void UpdateProgress(float current, float target, bool completed)
    {
        if (isCompleted) return; // Prevent multiple complete triggers

        if (progressText != null)
            progressText.text = $"{(int)current} / {target}";

        // Smooth fill the progress bar instead of instant snap
        if (progressBar != null)
        {
            if (progressAnimRoutine != null) StopCoroutine(progressAnimRoutine);
            progressAnimRoutine = StartCoroutine(SmoothFillRoutine(current));
        }

        if (completed)
        {
            isCompleted = true;
            StartCoroutine(CompletionVFXRoutine());
        }
    }

    private IEnumerator SmoothFillRoutine(float targetValue)
    {
        float startValue = progressBar.value;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f; // Fast lerp
            progressBar.value = Mathf.Lerp(startValue, targetValue, t);
            yield return null;
        }
        progressBar.value = targetValue;
    }

    private IEnumerator CompletionVFXRoutine()
    {
        // 1. Flash background to neon completion colour + change progress text to white
        if (backgroundImage != null) backgroundImage.color = completionColor;
        if (progressText != null)    progressText.color    = Color.white;
        if (progressBarFill != null) progressBarFill.color = Color.white;
        if (descriptionText != null) descriptionText.color = Color.white;

        Vector3 startScale  = Vector3.one;
        Vector3 bumpedScale = new Vector3(1.08f, 1.08f, 1f);

        // Scale Up
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            transform.localScale = Vector3.Lerp(startScale, bumpedScale, t);
            yield return null;
        }

        // Scale Down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            transform.localScale = Vector3.Lerp(bumpedScale, startScale, t);
            yield return null;
        }
        transform.localScale = startScale;

        // 2. Hold for 2 seconds so the player can see the completion
        yield return new WaitForSeconds(2.0f);

        // 3. Slide completely off the screen
        yield return StartCoroutine(SlideRoutine(hiddenPos, 0.4f, 0f));
    }

    private IEnumerator SlideRoutine(Vector2 targetPos, float duration, float delay)
    {
        if (delay > 0)
            yield return new WaitForSeconds(delay);

        Vector2 startPos = rectTransform.anchoredPosition;
        float t = 0f;
        while (t < 1f)
        {
            // Easy out lerp
            t += Time.deltaTime / duration;
            float easeT = 1f - (1f - t) * (1f - t); 
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }
        
        rectTransform.anchoredPosition = targetPos;
    }
}
