using UnityEngine;

/// <summary>
/// Attach to any power-up pickup GameObject.
/// Produces a smooth sine-wave floating animation and optional glow pulse.
/// Works alongside the existing pickup trigger logic.
/// </summary>
public class PowerUpFloat : MonoBehaviour
{
    [Header("Float Animation")]
    [Tooltip("Vertical distance of the sine-wave bob (world units)")]
    [SerializeField] private float floatAmplitude = 0.25f;
    [Tooltip("How many complete bob cycles per second")]
    [SerializeField] private float floatFrequency = 1.2f;
    [Tooltip("Phase offset so pickups spawned at the same time don't all bob in sync")]
    [SerializeField] private float phaseOffset = 0f;

    [Header("Rotation")]
    [Tooltip("Degrees per second the pickup spins (0 = no spin)")]
    [SerializeField] private float spinSpeed = 45f;

    [Header("Glow Pulse")]
    [Tooltip("SpriteRenderer whose color alpha will pulse for a glow effect")]
    [SerializeField] private SpriteRenderer glowRenderer;
    [SerializeField] private float glowMinAlpha = 0.3f;
    [SerializeField] private float glowMaxAlpha = 0.85f;
    [SerializeField] private float glowFrequency = 1.8f;

    private Vector3 originPos;

    private void Awake()
    {
        originPos = transform.position;
        // Give each instance a random phase so clusters look organic
        if (phaseOffset == 0f)
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    private void Update()
    {
        // Sine-wave vertical bob
        float yOffset = Mathf.Sin((Time.time * floatFrequency * Mathf.PI * 2f) + phaseOffset) * floatAmplitude;
        transform.position = new Vector3(originPos.x, originPos.y + yOffset, originPos.z);

        // Spin
        if (spinSpeed != 0f)
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);

        // Glow pulse
        if (glowRenderer != null)
        {
            float alpha = Mathf.Lerp(
                glowMinAlpha,
                glowMaxAlpha,
                (Mathf.Sin(Time.time * glowFrequency * Mathf.PI * 2f) + 1f) * 0.5f
            );
            Color c = glowRenderer.color;
            c.a = alpha;
            glowRenderer.color = c;
        }
    }
}
