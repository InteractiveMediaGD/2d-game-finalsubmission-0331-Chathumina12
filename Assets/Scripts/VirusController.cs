using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the Player (Virus) with auto-movement on X-axis
/// and player-controlled vertical movement on Y-axis.
/// Now includes shooting capability.
/// </summary>
public class VirusController : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Constant horizontal speed (auto-run)")]
    [SerializeField] private float currentSpeed = 5f;
    
    /// <summary>
    /// Public accessor for speed (used by BackgroundScroller).
    /// Returns GameManager scroll speed when available.
    /// </summary>
    public float CurrentSpeed => GameManager.Instance != null ? GameManager.Instance.ScrollSpeed : currentSpeed;
    
    [Tooltip("Vertical movement speed")]
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Screen Bounds")]
    [Tooltip("Minimum Y position (bottom of play area)")]
    [SerializeField] private float minY = -4f;
    
    [Tooltip("Maximum Y position (top of play area)")]
    [SerializeField] private float maxY = 4f;

    [Header("Shooting Settings")]
    [Tooltip("Projectile prefab to fire")]
    [SerializeField] private GameObject projectilePrefab;
    
    [Tooltip("Transform where projectiles spawn (should be in front of player)")]
    [SerializeField] private Transform firePoint;
    
    [Tooltip("Minimum time between shots (seconds)")]
    [SerializeField] private float fireRate = 0.25f;

    // Components
    private Rigidbody2D rb;
    
    // Input
    private float inputY = 0f;
    
    // Shooting
    private float nextFireTime = 0f;

    private void Awake()
    {
        // Cache the Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            Debug.LogError("VirusController: Rigidbody2D component is missing!");
        }
        
        // Create firePoint if not assigned
        if (firePoint == null)
        {
            GameObject fp = new GameObject("FirePoint");
            fp.transform.SetParent(transform);
            fp.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            firePoint = fp.transform;
        }
    }

    private void FixedUpdate()
    {
        // Get speed from GameManager if available, otherwise use local speed
        float speed = currentSpeed;
        if (GameManager.Instance != null)
        {
            speed = GameManager.Instance.ScrollSpeed;
        }
        
        // Auto-run: Strictly apply velocity to the X-axis
        // Vertical movement is controlled by player input
        rb.velocity = new Vector2(speed, inputY * verticalSpeed);

        // Clamp Y position to keep player within screen bounds
        ClampPosition();
    }

    /// <summary>
    /// Clamps the player's Y position within the defined screen bounds.
    /// </summary>
    private void ClampPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, minY, maxY);
        transform.position = clampedPosition;
    }

    /// <summary>
    /// Called by the Input System when vertical movement input is received.
    /// Map this to your Move action's Y-axis (W/S or Up/Down arrows).
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnVerticalMove(InputAction.CallbackContext context)
    {
        // Read the vertical input value (-1 to 1)
        inputY = context.ReadValue<float>();
    }

    /// <summary>
    /// Called by the Input System when Fire input is received (Spacebar).
    /// </summary>
    /// <param name="context">Input action callback context</param>
    public void OnFire(InputAction.CallbackContext context)
    {
        // Only fire on button press (not release)
        if (context.performed)
        {
            Fire();
        }
    }

    /// <summary>
    /// Fires a projectile if enough time has passed since last shot.
    /// </summary>
    private void Fire()
    {
        // Check fire rate
        if (Time.time < nextFireTime) return;
        
        // Check if we have a projectile prefab
        if (projectilePrefab == null)
        {
            Debug.LogWarning("VirusController: No projectile prefab assigned!");
            return;
        }
        
        // Spawn projectile at fire point
        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        
        // Trigger screen shake (subtle)
        if (ScreenShakeController.Instance != null)
        {
            ScreenShakeController.Instance.ShakeOnShoot();
        }
        
        // Set next fire time
        nextFireTime = Time.time + fireRate;
    }

    /// <summary>
    /// Alternative: Called by legacy Input System or for direct value setting.
    /// </summary>
    /// <param name="value">Vertical input value from -1 to 1</param>
    public void SetVerticalInput(float value)
    {
        inputY = value;
    }

    /// <summary>
    /// Updates screen bounds. Call this if your camera or play area changes.
    /// </summary>
    /// <param name="newMinY">New minimum Y bound</param>
    /// <param name="newMaxY">New maximum Y bound</param>
    public void SetScreenBounds(float newMinY, float newMaxY)
    {
        minY = newMinY;
        maxY = newMaxY;
    }

    #region Rapid Fire System
    // Rapid fire state
    private bool isRapidFireActive = false;
    private float normalFireRate;
    private Coroutine rapidFireCoroutine;

    /// <summary>
    /// Activates rapid fire mode, decreasing the weapon cooldown temporarily.
    /// Called by the PickupSystem when collecting a RapidFire pickup.
    /// </summary>
    /// <param name="duration">How long rapid fire lasts (seconds)</param>
    /// <param name="multiplier">Fire rate multiplier (0.5 = 2x faster)</param>
    public void ActivateRapidFire(float duration, float multiplier)
    {
        // Cancel existing rapid fire if active
        if (rapidFireCoroutine != null)
        {
            StopCoroutine(rapidFireCoroutine);
        }
        
        rapidFireCoroutine = StartCoroutine(RapidFireCoroutine(duration, multiplier));
    }

    /// <summary>
    /// Coroutine that handles the rapid fire duration and restoration.
    /// </summary>
    private System.Collections.IEnumerator RapidFireCoroutine(float duration, float multiplier)
    {
        // Store normal fire rate if not already in rapid fire
        if (!isRapidFireActive)
        {
            normalFireRate = fireRate;
        }
        
        // Apply rapid fire
        isRapidFireActive = true;
        fireRate = normalFireRate * multiplier;
        
        Debug.Log($"Rapid Fire activated! Fire rate: {fireRate} (was {normalFireRate})");
        
        // Wait for duration
        yield return new WaitForSeconds(duration);
        
        // Restore normal fire rate
        fireRate = normalFireRate;
        isRapidFireActive = false;
        rapidFireCoroutine = null;
        
        Debug.Log($"Rapid Fire ended. Fire rate restored to: {fireRate}");
    }
    #endregion
}

