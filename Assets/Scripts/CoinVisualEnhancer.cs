using UnityEngine;

/// <summary>
/// Makes spawned coins more visible: scales up the sprite, brightens the
/// color, adds a gentle bobbing motion and a slow Y-axis spin so they
/// stand out against the busy desert background.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CoinVisualEnhancer : MonoBehaviour
{
    [Tooltip("Multiplier applied to the coin's local scale on Start.")]
    public float scaleMultiplier = 1.6f;
    [Tooltip("Color tint multiplied with the sprite (white = leave alone).")]
    public Color tint = new Color(1.10f, 1.00f, 0.45f, 1f); // warm gold
    [Tooltip("Vertical bob amplitude in world units.")]
    public float bobAmount = 0.18f;
    [Tooltip("Bob cycles per second.")]
    public float bobSpeed = 1.4f;
    [Tooltip("Y-axis flip cycles per second (0 = no spin).")]
    public float spinSpeed = 1.0f;
    [Tooltip("Subtle scale pulse amplitude (0 = none).")]
    public float pulseAmount = 0.06f;

    private SpriteRenderer sr;
    private Vector3 baseScale;
    private Vector3 baseLocalPos;
    private float phase;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = tint;
        transform.localScale *= scaleMultiplier;
        baseScale = transform.localScale;
        baseLocalPos = transform.localPosition;
        phase = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        float t = Time.time;
        // Vertical bob.
        Vector3 p = baseLocalPos;
        p.y += Mathf.Sin(t * bobSpeed * Mathf.PI * 2f + phase) * bobAmount;
        transform.localPosition = p;

        // Y-axis spin (flips the sprite back and forth — looks like a spinning coin).
        if (spinSpeed > 0f)
        {
            float yAngle = Mathf.Sin(t * spinSpeed * Mathf.PI * 2f + phase) * 90f;
            transform.localRotation = Quaternion.Euler(0f, yAngle, 0f);
        }

        // Scale pulse.
        float pulse = 1f + Mathf.Sin(t * (bobSpeed * 1.3f) * Mathf.PI * 2f + phase) * pulseAmount;
        transform.localScale = baseScale * pulse;
    }
}
