using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

/// <summary>
/// Enhanced Main Menu UI controller.
/// Handles button hover animations (scale + colour), background parallax,
/// and a neon scanline overlay animation. No hard scene-name coupling —
/// SceneManager calls go through public methods matching MainMenuController.
/// </summary>
[RequireComponent(typeof(MainMenuController))]
public class MainMenuUI : MonoBehaviour
{
    [Header("Menu Buttons to Animate")]
    [Tooltip("Array of all buttons on the main panel that should scale on hover")]
    [SerializeField] private Button[] menuButtons;

    [Header("Background Parallax Layers")]
    [Tooltip("Far background layer transform (slowest)")]
    [SerializeField] private RectTransform farLayer;
    [Tooltip("Near background layer transform (faster)")]
    [SerializeField] private RectTransform nearLayer;
    [SerializeField] private float farParallaxStrength = 15f;
    [SerializeField] private float nearParallaxStrength = 35f;

    [Header("Title Animation")]
    [Tooltip("The title TextMeshProUGUI to apply neon flicker effect to")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Color titleBaseColor = new Color(0.00f, 1.00f, 0.85f);
    [SerializeField] private Color titleFlickerColor = new Color(0.00f, 0.60f, 0.50f);

    [Header("Scanline Overlay")]
    [Tooltip("UI Image used as a scrolling scanline overlay (optional)")]
    [SerializeField] private RawImage scanlineImage;
    [SerializeField] private float scanlineScrollSpeed = 0.08f;

    [Header("Button Hover Style")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float pressScale = 0.94f;
    [SerializeField] private float scaleSpeed = 10f;
    [SerializeField] private Color hoverTextColor = new Color(0f, 1f, 0.85f); // neon cyan
    [SerializeField] private Color normalTextColor = Color.white;

    // Internal
    private Vector2 scanlineOffset;
    private float flickerTimer;

    // ─────────────────────────────────────────────────────────────────────────
    //  Lifecycle
    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Wire hover animations to every menu button
        foreach (Button btn in menuButtons)
        {
            if (btn == null) continue;
            ButtonHoverScaler scaler = btn.gameObject.GetComponent<ButtonHoverScaler>();
            if (scaler == null)
                scaler = btn.gameObject.AddComponent<ButtonHoverScaler>();

            scaler.hoverScale  = hoverScale;
            scaler.pressScale  = pressScale;
            scaler.speed       = scaleSpeed;
            scaler.hoverColor  = hoverTextColor;
            scaler.normalColor = normalTextColor;
        }

        // Start title flicker
        if (titleText != null) titleText.color = titleBaseColor;
        StartCoroutine(TitleFlicker());
    }

    private void Update()
    {
        // Scanline scroll
        if (scanlineImage != null)
        {
            scanlineOffset.y -= scanlineScrollSpeed * Time.deltaTime;
            scanlineImage.uvRect = new Rect(scanlineOffset, Vector2.one);
        }

        // Subtle mouse-parallax for far/near layers
        Vector2 mouseNorm = new Vector2(
            (Input.mousePosition.x / Screen.width  - 0.5f),
            (Input.mousePosition.y / Screen.height - 0.5f)
        );

        if (farLayer  != null) farLayer.anchoredPosition  = mouseNorm * farParallaxStrength;
        if (nearLayer != null) nearLayer.anchoredPosition = mouseNorm * nearParallaxStrength;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Coroutines
    // ─────────────────────────────────────────────────────────────────────────
    private IEnumerator TitleFlicker()
    {
        while (true)
        {
            // Occasional random flicker
            float wait = Random.Range(2.5f, 6f);
            yield return new WaitForSeconds(wait);

            if (titleText == null) yield break;

            int flickerCount = Random.Range(1, 4);
            for (int i = 0; i < flickerCount; i++)
            {
                titleText.color = titleFlickerColor;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.12f));
                titleText.color = titleBaseColor;
                yield return new WaitForSeconds(Random.Range(0.04f, 0.10f));
            }
        }
    }
}

// ═════════════════════════════════════════════════════════════════════════════
//  Helper component: added at runtime to each menu button
// ═════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Scales a button up on hover and down on press, and tints its TMP text neon on hover.
/// Added at runtime by MainMenuUI — never needs to be manually added.
/// </summary>
public class ButtonHoverScaler : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler, IPointerUpHandler
{
    [HideInInspector] public float hoverScale  = 1.08f;
    [HideInInspector] public float pressScale  = 0.94f;
    [HideInInspector] public float speed       = 10f;
    [HideInInspector] public Color hoverColor  = new Color(0f, 1f, 0.85f);
    [HideInInspector] public Color normalColor = Color.white;

    private float targetScale = 1f;
    private TextMeshProUGUI label;

    private void Awake()
    {
        label = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        float current = transform.localScale.x;
        float next = Mathf.Lerp(current, targetScale, Time.deltaTime * speed);
        transform.localScale = new Vector3(next, next, 1f);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        targetScale = hoverScale;
        if (label != null) label.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData e)
    {
        targetScale = 1f;
        if (label != null) label.color = normalColor;
    }

    public void OnPointerDown(PointerEventData e)
    {
        targetScale = pressScale;
    }

    public void OnPointerUp(PointerEventData e)
    {
        targetScale = hoverScale; // stay expanded until mouse exits
    }
}
