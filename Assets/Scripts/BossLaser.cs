using UnityEngine;

/// <summary>
/// Attached to the Boss Laser BoxCollider2D. Deals massive continuous damage or instant kills when active.
/// </summary>
public class BossLaser : MonoBehaviour
{
    [Tooltip("Damage dealt by the massive laser beam")]
    public int beamDamage = 35;
    
    [Tooltip("How often the laser ticks damage while standing in it (seconds)")]
    public float damageTickInterval = 0.2f;

    private float nextDamageTime = 0f;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time >= nextDamageTime)
            {
                PlayerHealthController ph = other.GetComponent<PlayerHealthController>();
                if (ph != null && !ph.IsInvincible)
                {
                    ph.TakeDamage(beamDamage);
                    
                    if (ScreenShakeController.Instance != null)
                        ScreenShakeController.Instance.ShakeOnDamage();
                        
                    nextDamageTime = Time.time + damageTickInterval;
                }
            }
        }
    }
}
