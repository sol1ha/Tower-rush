using UnityEngine;

/// <summary>
/// Makes a platform look "barely there" — its SpriteRenderer alpha is dropped
/// to a low value with a slow shimmer so the player has to look carefully to
/// notice it. The collider and gameplay behaviour are unchanged.
/// </summary>
public class GhostPlatformVisual : MonoBehaviour
{
    [Tooltip("Minimum alpha during the shimmer (0..1). Lower = harder to see.")]
    [Range(0f, 1f)] public float minAlpha = 0.10f;
    [Tooltip("Maximum alpha during the shimmer (0..1).")]
    [Range(0f, 1f)] public float maxAlpha = 0.30f;
    [Tooltip("Shimmer cycles per second. Slow values feel ghostly.")]
    public float shimmerSpeed = 0.4f;
    [Tooltip("Random phase offset so neighbouring ghosts don't pulse in sync.")]
    public bool randomPhase = true;

    private SpriteRenderer sr;
    private float phaseOffset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        phaseOffset = randomPhase ? Random.value * Mathf.PI * 2f : 0f;
        ApplyAlpha((minAlpha + maxAlpha) * 0.5f);
    }

    void Update()
    {
        if (sr == null) return;
        float t = (Mathf.Sin(Time.time * shimmerSpeed * Mathf.PI * 2f + phaseOffset) + 1f) * 0.5f;
        ApplyAlpha(Mathf.Lerp(minAlpha, maxAlpha, t));
    }

    void ApplyAlpha(float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }
}
