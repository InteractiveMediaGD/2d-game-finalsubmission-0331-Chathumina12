using UnityEngine;

/// <summary>
/// Scrolls background material UV based on player speed.
/// Creates the "Flow State" visual effect where faster movement = faster background scroll.
/// </summary>
public class BackgroundScroller : MonoBehaviour
{
    [Header("Material Reference")]
    [Tooltip("The background material with scrolling shader")]
    [SerializeField] private Material backgroundMaterial;
    
    [Tooltip("If using a SpriteRenderer, assign it here instead")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Scroll Settings")]
    [Tooltip("Base scroll speed multiplier")]
    [SerializeField] private float scrollMultiplier = 0.1f;
    
    [Tooltip("Scroll direction")]
    [SerializeField] private Vector2 scrollDirection = new Vector2(-1f, 0f);

    [Header("Player Reference")]
    [Tooltip("Reference to get current speed")]
    [SerializeField] private VirusController player;

    // Shader property names
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");
    private static readonly int OffsetProperty = Shader.PropertyToID("_Offset");
    
    private Vector2 currentOffset = Vector2.zero;
    private Material materialInstance;

    private void Start()
    {
        // Create material instance to avoid editing the original
        if (backgroundRenderer != null)
        {
            materialInstance = backgroundRenderer.material;
        }
        else if (backgroundMaterial != null)
        {
            materialInstance = new Material(backgroundMaterial);
        }
    }

    private void Update()
    {
        if (materialInstance == null) return;

        // Get player speed (use default if no player assigned)
        float speed = 5f;
        if (player != null)
        {
            speed = player.CurrentSpeed;
        }

        // Calculate scroll offset based on speed
        float scrollSpeed = speed * scrollMultiplier;
        currentOffset += scrollDirection * scrollSpeed * Time.deltaTime;

        // Apply offset to material
        // Try standard texture offset first
        materialInstance.SetTextureOffset("_MainTex", currentOffset);
        
        // If using custom shader, also try custom property
        if (materialInstance.HasProperty("_Offset"))
        {
            materialInstance.SetVector("_Offset", new Vector4(currentOffset.x, currentOffset.y, 0, 0));
        }
    }

    private void OnDestroy()
    {
        // Clean up material instance
        if (materialInstance != null && backgroundRenderer == null)
        {
            Destroy(materialInstance);
        }
    }
}
