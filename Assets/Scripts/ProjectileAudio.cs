using UnityEngine;

/// <summary>
/// Drop on any Projectile prefab.
/// Plays a laser "shoot" SFX when the projectile spawns.
/// </summary>
public class ProjectileAudio : MonoBehaviour
{
    // The shoot sound is now handled explicitly by VirusController and EnemyShooter
    // so we don't need the generic projectile to play it on spawn anymore.
}
