using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Central audio manager for the entire game.
/// All sound effects are synthesised procedurally at runtime — no external audio
/// files are required. The manager is a persistent singleton so it survives scene changes.
///
/// Usage:  AudioManager.Play("shoot");
///         AudioManager.PlayMusic("menu");
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ─── Settings ───────────────────────────────────────────────────────────
    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume = 0.85f;
    [Range(0f, 1f)] public float sfxVolume    = 0.80f;
    [Range(0f, 1f)] public float musicVolume  = 0.45f;

    // ─── Internal ────────────────────────────────────────────────────────────
    private AudioSource musicSource;       // dedicated loop source
    private readonly List<AudioSource> sfxPool = new();
    private const int SFX_POOL_SIZE = 16;

    // Pre-baked clips keyed by name
    private readonly Dictionary<string, AudioClip> clipCache = new();

    // Sample rate used when baking clips
    private const int SAMPLE_RATE = 44100;

    // ─────────────────────────────────────────────────────────────────────────
    //  Singleton boot
    // ─────────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Dedicated music source
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop   = true;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.spatialBlend = 0f;

        // SFX pool
        for (int i = 0; i < SFX_POOL_SIZE; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;
            sfxPool.Add(src);
        }

        BakeAllClips();
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Public API
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Play a one-shot SFX by name.</summary>
    public static void Play(string clipName)
    {
        if (Instance == null) return;
        Instance.PlaySFX(clipName);
    }

    /// <summary>Start looping background music by name.</summary>
    public static void PlayMusic(string clipName)
    {
        if (Instance == null) return;
        Instance.StartMusic(clipName);
    }

    /// <summary>Stop the music instantly.</summary>
    public static void StopMusic()
    {
        if (Instance == null) return;
        Instance.musicSource.Stop();
    }

    /// <summary>Cross-fade to a new music track.</summary>
    public static void CrossfadeTo(string clipName, float duration = 1.2f)
    {
        if (Instance == null) return;
        Instance.StartCoroutine(Instance.DoCrossfade(clipName, duration));
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Internal helpers
    // ─────────────────────────────────────────────────────────────────────────
    private void PlaySFX(string name)
    {
        if (!clipCache.TryGetValue(name, out AudioClip clip)) return;

        // Find a free pool slot
        foreach (var src in sfxPool)
        {
            if (!src.isPlaying)
            {
                src.clip   = clip;
                src.volume = sfxVolume * masterVolume;
                src.pitch  = Random.Range(0.97f, 1.03f); // tiny humanise
                src.Play();
                return;
            }
        }
        // All busy — steal the first one
        sfxPool[0].Stop();
        sfxPool[0].clip   = clip;
        sfxPool[0].volume = sfxVolume * masterVolume;
        sfxPool[0].Play();
    }

    private void StartMusic(string name)
    {
        if (!clipCache.TryGetValue(name, out AudioClip clip)) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        musicSource.clip   = clip;
        musicSource.volume = musicVolume * masterVolume;
        musicSource.Play();
    }

    private IEnumerator DoCrossfade(string nextName, float duration)
    {
        if (!clipCache.TryGetValue(nextName, out AudioClip next)) yield break;

        float startVol = musicSource.volume;
        float half = duration * 0.5f;

        // Fade out
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / half);
            yield return null;
        }

        musicSource.clip = next;
        musicSource.Play();

        // Fade in
        for (float t = 0; t < half; t += Time.unscaledDeltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, musicVolume * masterVolume, t / half);
            yield return null;
        }
        musicSource.volume = musicVolume * masterVolume;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Clip baking — every sound is synthesised from maths, no files needed
    // ─────────────────────────────────────────────────────────────────────────
    private void BakeAllClips()
    {
        // ── SFX ──────────────────────────────────────────────────────────────
        // UI
        clipCache["btn_hover"]   = BakeClip(BtnHover,    0.10f);
        clipCache["btn_click"]   = BakeClip(BtnClick,    0.15f);
        clipCache["difficulty_select"] = BakeClip(DifficultySelect, 0.30f);
        clipCache["menu_back"]   = BakeClip(MenuBack,    0.20f);

        // Gameplay
        // Gameplay - Shooting
        clipCache["shoot_player"] = BakeClip(ShootPlayer, 0.12f);
        clipCache["shoot_enemy"]  = BakeClip(ShootEnemy,  0.18f);

        // Gameplay - Impacts & Deaths
        clipCache["enemy_hit"]    = BakeClip(EnemyHit,    0.18f);
        clipCache["enemy_die_antivirus"] = BakeClip(EnemyDieAntivirus, 0.35f);
        clipCache["enemy_die_shooter"]   = BakeClip(EnemyDieShooter,   0.45f);
        clipCache["player_hurt"]  = BakeClip(PlayerHurt,  0.30f);

        // Gameplay - Pickups
        clipCache["pickup_hp"]       = BakeClip(PickupHP,       0.40f);
        clipCache["pickup_rapid"]    = BakeClip(PickupRapid,    0.35f);
        clipCache["pickup_shield"]   = BakeClip(PickupShield,   0.45f);
        clipCache["pickup_fragment"] = BakeClip(PickupFragment, 0.30f);

        // Gameplay - Progression
        clipCache["barrier_pass"] = BakeClip(BarrierPass, 0.25f);
        clipCache["boss_roar"]    = BakeClip(BossRoar,    1.00f);
        clipCache["boss_die"]     = BakeClip(BossDie,     1.50f);
        clipCache["game_over"]    = BakeClip(GameOver,    2.00f);

        // ── Music (longer synthesised loops) ─────────────────────────────────
        clipCache["music_menu"]  = BakeClip(MusicMenu,   8.00f);
        clipCache["music_game"]  = BakeClip(MusicGame,   8.00f);
        clipCache["music_boss"]  = BakeClip(MusicBoss,   8.00f);
    }

    // Helper: allocate and fill an AudioClip using a generator delegate
    private delegate float SampleFn(float t, float duration);
    private AudioClip BakeClip(SampleFn fn, float duration)
    {
        int total = Mathf.RoundToInt(SAMPLE_RATE * duration);
        float[] data = new float[total];
        for (int i = 0; i < total; i++)
        {
            float t = (float)i / SAMPLE_RATE;
            data[i] = Mathf.Clamp(fn(t, duration), -1f, 1f);
        }
        AudioClip clip = AudioClip.Create("_", total, 1, SAMPLE_RATE, false);
        clip.SetData(data, 0);
        return clip;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  SFX Synthesisers
    // ─────────────────────────────────────────────────────────────────────────

    // Soft UI hover — rising sine blip
    static float BtnHover(float t, float d)
    {
        float env = Mathf.Exp(-t * 18f);
        float freq = 800f + t * 400f;
        return env * Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f;
    }

    // Sharp tap + quick decay — confirms click
    static float BtnClick(float t, float d)
    {
        float env = Mathf.Exp(-t * 25f);
        float body = Mathf.Sin(2 * Mathf.PI * 600f * t);
        float click = (t < 0.004f) ? 0.8f : 0f;
        return env * (body * 0.5f + click);
    }

    // Upward chime — difficulty chosen
    static float DifficultySelect(float t, float d)
    {
        float env = Mathf.Exp(-t * 6f);
        float f1  = Mathf.Sin(2 * Mathf.PI * 523f * t);  // C5
        float f2  = Mathf.Sin(2 * Mathf.PI * 659f * t);  // E5 (slightly delayed)
        float delay = t > 0.10f ? Mathf.Sin(2 * Mathf.PI * 784f * (t - 0.10f)) : 0f; // G5
        return env * (f1 * 0.35f + f2 * 0.3f + delay * 0.25f);
    }

    // Downward whoosh — back/close menu
    static float MenuBack(float t, float d)
    {
        float env  = Mathf.Exp(-t * 12f);
        float freq = 700f * Mathf.Exp(-t * 8f);
        return env * Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f;
    }

    // Laser pew — high, clean, short (Player)
    static float ShootPlayer(float t, float d)
    {
        float env  = Mathf.Exp(-t * 40f);
        float freq = 900f - t * 3000f;
        return env * Mathf.Sin(2 * Mathf.PI * Mathf.Max(freq, 100f) * t) * 0.6f;
    }

    // Gritty lower zap — (Enemy)
    static float ShootEnemy(float t, float d)
    {
        float env = Mathf.Exp(-t * 30f);
        float freq = 500f + t * 500f; // Rising slightly or staying steady
        float noise = Mathf.Sin(t * 12345f) * 0.2f;
        return env * (Mathf.Sin(2 * Mathf.PI * freq * t) + noise) * 0.5f;
    }

    // Metallic thud — enemy registers a hit
    static float EnemyHit(float t, float d)
    {
        float env  = Mathf.Exp(-t * 20f);
        float noise = (Mathf.Sin(2 * Mathf.PI * 440f * t) + Mathf.Sin(2 * Mathf.PI * 880f * t)) * 0.5f;
        return env * noise * 0.55f;
    }

    // High crunch — Antivirus Triangle dies
    static float EnemyDieAntivirus(float t, float d)
    {
        float env  = Mathf.Exp(-t * 15f);
        float wave = Mathf.Sin(2 * Mathf.PI * 220f * t)
                   + Mathf.Sin(2 * Mathf.PI * 347f * t) * 0.6f;
        float pseudo = Mathf.Sin(t * 12345.6f) * Mathf.Sin(t * 23456.7f) * 0.4f;
        return env * (wave * 0.4f + pseudo) * 0.6f;
    }

    // Heavy booming explosion — Shooter Square dies
    static float EnemyDieShooter(float t, float d)
    {
        float env = Mathf.Exp(-t * 10f);
        float drop = Mathf.Max(80f, 400f - t * 1500f);
        float wave = Mathf.Sin(2 * Mathf.PI * drop * t);
        float noise = Mathf.Sin(t * 34567f) * 0.3f;
        return env * (wave * 0.6f + noise) * 0.7f;
    }

    // Low grunt — player takes damage
    static float PlayerHurt(float t, float d)
    {
        float env  = Mathf.Exp(-t * 8f);
        float sub  = Mathf.Sin(2 * Mathf.PI * 80f  * t);
        float tone = Mathf.Sin(2 * Mathf.PI * 260f * t);
        return env * (sub * 0.6f + tone * 0.3f);
    }

    // Warm rising heal — HealthPack collected
    static float PickupHP(float t, float d)
    {
        float env  = Mathf.Exp(-t * 5f);
        float f1   = Mathf.Sin(2 * Mathf.PI * 440f * t);
        float f2   = Mathf.Sin(2 * Mathf.PI * 660f * (t + 0.08f));
        float f3   = Mathf.Sin(2 * Mathf.PI * 880f * (t + 0.16f));
        return env * (f1 * 0.4f + f2 * 0.3f + f3 * 0.2f);
    }

    // Electric zap — RapidFire collected
    static float PickupRapid(float t, float d)
    {
        float env    = Mathf.Exp(-t * 9f);
        float rising = 200f + t * 1400f;
        float zap    = Mathf.Sin(2 * Mathf.PI * rising * t);
        float crackle= Mathf.Sin(t * 23000f) * 0.3f;
        return env * (zap * 0.5f + crackle) * 0.6f;
    }

    // Resonant hum — Shield collected
    static float PickupShield(float t, float d)
    {
        float env = Mathf.Sin(Mathf.PI * (t/d)) * Mathf.Exp(-t * 3f);
        float f1 = Mathf.Sin(2 * Mathf.PI * 300f * t);
        float f2 = Mathf.Sin(2 * Mathf.PI * 450f * t);
        return env * (f1 + f2) * 0.4f;
    }

    // Bright synthetic twinkle — Data Fragment collected
    static float PickupFragment(float t, float d)
    {
        float env = Mathf.Exp(-t * 15f);
        float f1 = Mathf.Sin(2 * Mathf.PI * 1200f * t);
        float f2 = t > 0.05f ? Mathf.Sin(2 * Mathf.PI * 1600f * (t - 0.05f)) : 0f;
        return env * (f1 * 0.4f + f2 * 0.4f);
    }

    // Quick high blip — barrier passed
    static float BarrierPass(float t, float d)
    {
        float env  = Mathf.Exp(-t * 15f);
        float freq = 1200f - t * 500f;
        return env * Mathf.Sin(2 * Mathf.PI * freq * t) * 0.4f;
    }

    // Deep rumbling roar — boss appears
    static float BossRoar(float t, float d)
    {
        float env  = t < 0.05f ? t / 0.05f : Mathf.Exp(-(t - 0.05f) * 3f);
        float low  = Mathf.Sin(2 * Mathf.PI * 55f  * t);
        float mid  = Mathf.Sin(2 * Mathf.PI * 110f * t) * 0.5f;
        float growl= Mathf.Sin(2 * Mathf.PI * 7f   * t) * 0.4f; // modulation tremolo
        return env * (low + mid) * (0.7f + growl) * 0.7f;
    }

    // Satisfying descending explosion — boss defeated
    static float BossDie(float t, float d)
    {
        float env  = Mathf.Exp(-t * 2.5f);
        float freq = 400f * Mathf.Exp(-t * 2f);
        float rich = Mathf.Sin(2 * Mathf.PI * freq * t)
                   + Mathf.Sin(2 * Mathf.PI * freq * 2 * t) * 0.5f
                   + Mathf.Sin(2 * Mathf.PI * freq * 3 * t) * 0.25f;
        float pseudo = Mathf.Sin(t * 15000f) * Mathf.Sin(t * 7500f) * 0.5f;
        return env * (rich * 0.5f + pseudo) * 0.75f;
    }

    // Mournful descending minor chord — game over
    static float GameOver(float t, float d)
    {
        float env   = t < 0.1f ? t / 0.1f : Mathf.Exp(-(t - 0.1f) * 2f);
        float root  = Mathf.Sin(2 * Mathf.PI * 220f  * t);
        float third = Mathf.Sin(2 * Mathf.PI * 261f  * t) * 0.7f; // minor 3rd
        float fifth = Mathf.Sin(2 * Mathf.PI * 330f  * t) * 0.5f;
        return env * (root + third + fifth) * 0.3f;
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  Music Synthesisers — simple 8-second looping pads/patterns
    // ─────────────────────────────────────────────────────────────────────────

    // Menu: slow ambient cyberpunk pad, eerie and calm
    static float MusicMenu(float t, float d)
    {
        // Slow arpeggiated bass pad at 70 BPM feel
        // Notes: Am pentatonic — A2(110Hz), C3(130Hz), E3(165Hz), G3(196Hz)
        float[] freqs = { 110f, 130.8f, 164.8f, 196f, 220f };
        float step  = d / freqs.Length;
        int   idx   = (int)(t / step) % freqs.Length;
        float tLocal= t % step;
        
        float env   = Mathf.Sin(Mathf.PI * Mathf.Min(tLocal / 0.02f, 1f))       // 20ms attack
                    * Mathf.Exp(-tLocal * 1.8f);                                   // 560ms decay
        float note  = Mathf.Sin(2 * Mathf.PI * freqs[idx] * t);
        float octave= Mathf.Sin(2 * Mathf.PI * freqs[idx] * 2 * t) * 0.3f;
        // Slow pulsing tremolo
        float tremolo = 0.75f + 0.25f * Mathf.Sin(2 * Mathf.PI * 0.5f * t);
        return env * (note + octave) * tremolo * 0.35f;
    }

    // Game: driving, energetic pulse — faster arpeggios
    static float MusicGame(float t, float d)
    {
        // 140 BPM feel, notes in Dm: D3(147Hz), F3(174Hz), A3(220Hz), C4(261Hz)
        float[] freqs = { 146.8f, 174.6f, 220f, 261.6f, 293.7f, 220f, 174.6f, 146.8f };
        float bps   = 140f / 60f * 2f;  // 8th note speed
        int   idx   = (int)(t * bps) % freqs.Length;
        float tBeat = (t * bps) % 1f;

        float env     = Mathf.Exp(-tBeat * 5f);
        float melody  = Mathf.Sin(2 * Mathf.PI * freqs[idx] * t);
        float sub     = Mathf.Sin(2 * Mathf.PI * 73.4f  * t) * 0.4f; // D2 bass drone
        // Kick-like thump every beat
        float kickPhase = (t * 140f / 60f) % 1f;
        float kick    = Mathf.Exp(-kickPhase * 10f) * Mathf.Sin(2 * Mathf.PI * 60f * t) * 0.5f;
        return (env * melody * 0.4f + sub + kick) * 0.45f;
    }

    // Boss: tense, distorted, rapid — high intensity
    static float MusicBoss(float t, float d)
    {
        // 180 BPM, diminished feel — A2, Eb3, A3, Eb4
        float[] freqs = { 110f, 155.6f, 220f, 311.1f, 440f, 311.1f, 220f, 155.6f };
        float bps    = 180f / 60f * 2f;
        int   idx    = (int)(t * bps) % freqs.Length;
        float tBeat  = (t * bps) % 1f;

        float env    = Mathf.Exp(-tBeat * 6f);
        // Distort the wave by clipping it hard to give it a "screaming" quality
        float raw    = Mathf.Sin(2 * Mathf.PI * freqs[idx] * t);
        float dist   = Mathf.Clamp(raw * 3f, -1f, 1f) * 0.5f;
        float tremolo= 0.7f + 0.3f * Mathf.Sin(2 * Mathf.PI * 7f * t);
        float sub    = Mathf.Sin(2 * Mathf.PI * 55f * t) * 0.35f;
        // 16th-note hat click approximation
        float hatPhase = (t * 180f / 60f * 4f) % 1f;
        float hat    = (hatPhase < 0.05f) ? 0.3f * Mathf.Sin(t * 18000f) : 0f;
        return (env * dist * tremolo + sub + hat) * 0.5f;
    }
}
