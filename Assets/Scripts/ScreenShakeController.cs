using UnityEngine;

/// <summary>
/// Controls screen shake effects.
/// Works with or without Cinemachine.
/// Singleton for easy access from any script.
/// </summary>
public class ScreenShakeController : MonoBehaviour
{
    public static ScreenShakeController Instance { get; private set; }

    [Header("Shake Intensities")]
    [Tooltip("Subtle shake when shooting")]
    [SerializeField] private float shootIntensity = 0.1f;
    
    [Tooltip("Medium shake on close call")]
    [SerializeField] private float closeCallIntensity = 0.4f;
    
    [Tooltip("Violent shake on damage/death")]
    [SerializeField] private float damageIntensity = 1.0f;

    [Header("Shake Settings")]
    [Tooltip("How fast the shake settles")]
    [SerializeField] private float shakeDuration = 0.2f;

    [Header("Camera Reference")]
    [Tooltip("Main camera to shake (auto-finds if not set)")]
    [SerializeField] private Transform cameraTransform;

    private Vector3 originalPosition;
    private float currentShakeAmount = 0f;
    private float currentShakeDuration = 0f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Auto-find camera if not assigned
        if (cameraTransform == null)
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                cameraTransform = mainCam.transform;
            }
        }
    }

    private void Start()
    {
        if (cameraTransform != null)
        {
            originalPosition = cameraTransform.localPosition;
        }
    }

    private void Update()
    {
        if (currentShakeDuration > 0 && cameraTransform != null)
        {
            // Apply random shake offset
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeAmount;
            shakeOffset.z = 0; // Keep Z stable for 2D
            cameraTransform.localPosition = originalPosition + shakeOffset;

            // Reduce shake over time
            currentShakeDuration -= Time.deltaTime;
            currentShakeAmount = Mathf.Lerp(currentShakeAmount, 0f, Time.deltaTime / shakeDuration);

            // Reset position when done
            if (currentShakeDuration <= 0)
            {
                cameraTransform.localPosition = originalPosition;
            }
        }
    }

    /// <summary>
    /// Triggers shake with specified intensity.
    /// </summary>
    private void TriggerShake(float intensity)
    {
        currentShakeAmount = intensity;
        currentShakeDuration = shakeDuration;
        
        // Update original position (in case camera moved)
        if (cameraTransform != null)
        {
            // Only update X, keep our reference Y and Z
            originalPosition.x = cameraTransform.localPosition.x;
        }
    }

    /// <summary>
    /// Triggers a subtle shake for shooting.
    /// </summary>
    public void ShakeOnShoot()
    {
        TriggerShake(shootIntensity);
    }

    /// <summary>
    /// Triggers a medium shake for close calls.
    /// </summary>
    public void ShakeOnCloseCall()
    {
        TriggerShake(closeCallIntensity);
    }

    /// <summary>
    /// Triggers a violent shake for damage/death.
    /// </summary>
    public void ShakeOnDamage()
    {
        TriggerShake(damageIntensity);
    }

    /// <summary>
    /// Custom shake with specific intensity.
    /// </summary>
    public void Shake(float intensity)
    {
        TriggerShake(intensity);
    }
}
