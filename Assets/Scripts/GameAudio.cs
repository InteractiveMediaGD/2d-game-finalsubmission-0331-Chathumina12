using UnityEngine;

/// <summary>
/// Attach this to any enemy that can be damaged.
/// Plays hit/death sounds and hooks into GameManager events for game-state audio.
/// </summary>
public class GameAudio : MonoBehaviour
{
    [Tooltip("Set true if this is the singleton game audio manager (attach to GameManager object)")]
    [SerializeField] private bool isGameController = false;

    private void Start()
    {
        if (!isGameController) return;

        // Ensure AudioManager persists from menu into game scene
        if (AudioManager.Instance == null)
        {
            var go = new GameObject("[AudioManager]");
            go.AddComponent<AudioManager>();
        }

        // Cross-fade from the menu music to the game music
        AudioManager.CrossfadeTo("music_game", 1.0f);

        // Hook into GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHealthChanged   += OnHealthChanged;
            GameManager.Instance.OnGameOver        += OnGameOver;
        }
    }

    private void OnDestroy()
    {
        if (!isGameController) return;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnHealthChanged   -= OnHealthChanged;
            GameManager.Instance.OnGameOver        -= OnGameOver;
        }
    }

    // ─── GameManager event handlers ──────────────────────────────────────────
    private void OnHealthChanged(int newHP)
    {
        // Only play player hurt when HP drops (not when healing via pickup)
        // We detect a drop by comparing against last known value via a static field
        if (newHP < GameAudio.lastHp)
            AudioManager.Play("player_hurt");
        GameAudio.lastHp = newHP;
    }
    private static int lastHp = 100;

    private void OnGameOver()
    {
        AudioManager.Play("game_over");
        AudioManager.StopMusic();
    }

    // ─── Public static hooks — call these from existing game scripts ──────────

    /// <summary>Call from VirusController when shooting.</summary>
    public static void PlayerShoot() => AudioManager.Play("shoot_player");

    /// <summary>Call from EnemyShooter when firing.</summary>
    public static void EnemyShoot() => AudioManager.Play("shoot_enemy");

    /// <summary>Call from EnemyAntivirus/EnemyShooter when hit by player projectile.</summary>
    public static void EnemyHit()   => AudioManager.Play("enemy_hit");

    /// <summary>Call from EnemyAntivirus when they die.</summary>
    public static void EnemyDiedAntivirus()  => AudioManager.Play("enemy_die_antivirus");

    /// <summary>Call from EnemyShooter when they die.</summary>
    public static void EnemyDiedShooter()  => AudioManager.Play("enemy_die_shooter");

    /// <summary>Call from PickupSystem when health collected.</summary>
    public static void GotHealth()  => AudioManager.Play("pickup_hp");

    /// <summary>Call from PickupSystem when rapid fire collected.</summary>
    public static void GotRapidFire() => AudioManager.Play("pickup_rapid");

    /// <summary>Call from PickupSystem when shield collected.</summary>
    public static void GotShield() => AudioManager.Play("pickup_shield");

    /// <summary>Call from DataFragmentPickup when collected.</summary>
    public static void GotFragment() => AudioManager.Play("pickup_fragment");

    /// <summary>Call from LevelGenerator/GameManager when a barrier is passed.</summary>
    public static void BarrierPassed() => AudioManager.Play("barrier_pass");

    /// <summary>Call from GameManager when boss fight starts.</summary>
    public static void BossStarted()
    {
        AudioManager.Play("boss_roar");
        AudioManager.CrossfadeTo("music_boss", 1.5f);
    }

    /// <summary>Call from GameManager when boss is defeated.</summary>
    public static void BossDefeated()
    {
        AudioManager.Play("boss_die");
        AudioManager.CrossfadeTo("music_game", 2.0f);
    }
}
