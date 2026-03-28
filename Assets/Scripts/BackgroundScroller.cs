using UnityEngine;

/// <summary>
/// Multi-layer parallax background scroller.
/// Attach one instance per background layer (far, mid, near).
/// Each layer scrolls at a different speed creating depth.
/// Supports both SpriteRenderer UV scrolling and Transform-based tiling scrolling.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Material / Renderer")]
    [Tooltip("SpriteRenderer whose material UV will be scrolled")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Parallax Settings")]
    [Tooltip("Fraction of GameManager scroll speed this layer moves at. 1 = full speed, 0.3 = far away.")]
    [SerializeField] [Range(0.01f, 2f)] private float parallaxFactor = 0.5f;

    [Header("Direction")]
    [SerializeField] private Vector2 scrollDirection = new Vector2(-1f, 0f);

    // Internal
    private Material materialInstance;
    private Vector2 currentUVOffset = Vector2.zero;

    private void Start()
    {
        if (backgroundRenderer == null) backgroundRenderer = GetComponent<SpriteRenderer>();

        if (backgroundRenderer != null)
        {
            // Instantiating the material avoids modifying the shared asset
            materialInstance = backgroundRenderer.material;
        }
    }

    private void Update()
    {
        if (materialInstance == null) return;

        float speed = GameManager.Instance != null ? GameManager.Instance.ScrollSpeed : 5f;
        float scrollSpeed = speed * parallaxFactor;

        currentUVOffset += scrollDirection.normalized * scrollSpeed * Time.deltaTime * 0.05f;
        materialInstance.SetTextureOffset("_MainTex", currentUVOffset);
    }

    private void OnDestroy()
    {
        if (materialInstance != null)
            Destroy(materialInstance);
    }
}
