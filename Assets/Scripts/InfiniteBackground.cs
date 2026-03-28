using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A flawless, infinitely scrolling background that runs independently of 
/// GameManager speed (so it never stops during boss fights).
/// Drag any Texture into the Background Image field in the Inspector!
/// </summary>
public class InfiniteBackground : MonoBehaviour
{
    [Header("Your Custom Background")]
    [Tooltip("Drag the image you want for the background here!")]
    public Texture2D backgroundImage;

    [Header("Scroll Settings")]
    [Tooltip("How fast the background moves")]
    public float scrollSpeed = 0.05f;
    
    [Tooltip("Color tint (Keep white for original colors)")]
    public Color tint = Color.white;

    private RawImage rawImage;

    private void Start()
    {
        // 1. Build a perfectly sized Canvas behind the game
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
        
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = Camera.main;
        canvas.planeDistance = 90f; // Push it way to the back
        canvas.sortingOrder = -100; // Render behind all sprites

        // 2. Add the RawImage to display the texture
        rawImage = GetComponent<RawImage>();
        if (rawImage == null) rawImage = gameObject.AddComponent<RawImage>();
        
        rawImage.texture = backgroundImage;
        rawImage.color = tint;

        // Force the texture to perfectly repeat endlessly
        if (backgroundImage != null)
        {
            backgroundImage.wrapMode = TextureWrapMode.Repeat;
        }

        // 3. Stretch to cover the entire screen
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private void Update()
    {
        // Simply slide the UV coordinates horizontally to create boundless scrolling.
        // It ignores all boss fights and pauses because it uses Time.deltaTime blindly!
        if (rawImage != null && rawImage.texture != null)
        {
            Rect r = rawImage.uvRect;
            r.x += scrollSpeed * Time.deltaTime;
            rawImage.uvRect = r;
        }
    }
}
