using UnityEngine;

/// <summary>
/// Makes the camera follow the player horizontally.
/// Attach this to your Main Camera.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player/target to follow")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Tooltip("Offset from the target position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Tooltip("How smoothly the camera follows (lower = smoother)")]
    [SerializeField] private float smoothSpeed = 0.125f;
    
    [Tooltip("Only follow on X-axis (horizontal)")]
    [SerializeField] private bool horizontalOnly = true;

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition;
        
        if (horizontalOnly)
        {
            // Only follow X, keep current Y and Z
            desiredPosition = new Vector3(
                target.position.x + offset.x,
                transform.position.y,
                offset.z
            );
        }
        else
        {
            // Follow both X and Y
            desiredPosition = target.position + offset;
        }

        // Smooth follow
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
