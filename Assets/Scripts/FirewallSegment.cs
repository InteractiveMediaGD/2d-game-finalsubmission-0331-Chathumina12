using UnityEngine;

/// <summary>
/// Component for individual Firewall barrier segments.
/// Manages the positioning of top/bottom walls and gap particle effects.
/// </summary>
public class FirewallSegment : MonoBehaviour
{
    [Header("Wall References")]
    [Tooltip("Reference to the Top_Wall child transform")]
    [SerializeField] private Transform topWall;
    
    [Tooltip("Reference to the Bottom_Wall child transform")]
    [SerializeField] private Transform bottomWall;

    [Header("Particle Systems")]
    [Tooltip("Particle system at the top edge of the gap (flickering code effect)")]
    [SerializeField] private ParticleSystem topGapParticles;
    
    [Tooltip("Particle system at the bottom edge of the gap (flickering code effect)")]
    [SerializeField] private ParticleSystem bottomGapParticles;

    [Header("Wall Settings")]
    [Tooltip("How far the walls extend beyond screen bounds")]
    [SerializeField] private float wallExtension = 10f;
    
    [Tooltip("Screen top boundary (match camera orthographic size)")]
    [SerializeField] private float screenTop = 5f;
    
    [Tooltip("Screen bottom boundary (negative of orthographic size)")]
    [SerializeField] private float screenBottom = -5f;

    /// <summary>
    /// Sets the gap position and size for this firewall segment.
    /// Positions the top and bottom walls accordingly.
    /// </summary>
    /// <param name="gapCenterY">Y position of the gap center</param>
    /// <param name="gapSize">Vertical size of the gap opening</param>
    public void SetGapPosition(float gapCenterY, float gapSize)
    {
        float halfGap = gapSize / 2f;
        
        // Calculate gap edges
        float gapTop = gapCenterY + halfGap;
        float gapBottom = gapCenterY - halfGap;
        
        // Position and scale Top Wall
        if (topWall != null)
        {
            float topWallHeight = (screenTop + wallExtension) - gapTop;
            float topWallCenterY = gapTop + (topWallHeight / 2f);
            
            topWall.localPosition = new Vector3(0, topWallCenterY, 0);
            topWall.localScale = new Vector3(1, topWallHeight, 1);
        }
        
        // Position and scale Bottom Wall
        if (bottomWall != null)
        {
            float bottomWallHeight = gapBottom - (screenBottom - wallExtension);
            float bottomWallCenterY = gapBottom - (bottomWallHeight / 2f);
            
            bottomWall.localPosition = new Vector3(0, bottomWallCenterY, 0);
            bottomWall.localScale = new Vector3(1, bottomWallHeight, 1);
        }
        
        // Position particle systems at gap edges
        if (topGapParticles != null)
        {
            topGapParticles.transform.localPosition = new Vector3(0, gapTop, 0);
            if (!topGapParticles.isPlaying)
                topGapParticles.Play();
        }
        
        if (bottomGapParticles != null)
        {
            bottomGapParticles.transform.localPosition = new Vector3(0, gapBottom, 0);
            if (!bottomGapParticles.isPlaying)
                bottomGapParticles.Play();
        }
    }

    /// <summary>
    /// Stops particle effects when segment is recycled.
    /// </summary>
    public void StopParticles()
    {
        if (topGapParticles != null)
            topGapParticles.Stop();
        if (bottomGapParticles != null)
            bottomGapParticles.Stop();
    }
}
