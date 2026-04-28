using UnityEngine;

/// <summary>
/// Spawns a small floating gold up-arrow above a boost platform so the player
/// can see at a glance that landing on it will launch them higher. Pure visual:
/// adds no colliders, doesn't change the platform's shape or color.
/// </summary>
public class BoostIndicator : MonoBehaviour
{
    [Tooltip("Color of the indicator arrow.")]
    public Color color = new Color(1f, 0.82f, 0.18f, 1f); // bright gold
    [Tooltip("Outline color drawn around the arrow for contrast.")]
    public Color outlineColor = new Color(0.30f, 0.18f, 0.05f, 1f);
    [Tooltip("World units above the platform's transform where the arrow sits.")]
    public float yOffset = 0.95f;
    [Tooltip("Width of the arrow in world units.")]
    public float arrowWidth = 0.85f;
    [Tooltip("Height of the arrow in world units.")]
    public float arrowHeight = 0.55f;
    [Tooltip("Vertical bob amplitude (world units).")]
    public float bobAmount = 0.10f;
    [Tooltip("Bob cycles per second.")]
    public float bobSpeed = 1.6f;
    [Tooltip("Subtle scale pulse amplitude (0 = no pulse).")]
    public float pulseAmount = 0.08f;
    [Tooltip("Number of stacked chevrons to draw (1 or 2 looks best).")]
    public int chevronCount = 2;
    [Tooltip("Vertical spacing between stacked chevrons (world units).")]
    public float chevronSpacing = 0.45f;
    [Tooltip("Sorting order for the indicator sprite (higher = drawn in front).")]
    public int sortingOrder = 5;

    private static Sprite cachedArrowSprite;
    private Transform[] arrows;
    private Vector3[] baseLocal;
    private float phaseOffset;

    void Start()
    {
        if (cachedArrowSprite == null) cachedArrowSprite = MakeArrowSprite(64, 64, 6);

        chevronCount = Mathf.Max(1, chevronCount);
        arrows = new Transform[chevronCount];
        baseLocal = new Vector3[chevronCount];
        phaseOffset = Random.value * Mathf.PI * 2f;

        for (int i = 0; i < chevronCount; i++)
        {
            var go = new GameObject("BoostIndicator_" + i);
            go.transform.SetParent(transform, false);
            // Stack vertically — index 0 is the bottom one closest to the platform.
            go.transform.localPosition = new Vector3(0f, yOffset + i * chevronSpacing, -0.1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = cachedArrowSprite;
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            // Scale so the rendered size matches arrowWidth x arrowHeight.
            // Sprite is 64x64 with PPU 100 → native size 0.64 × 0.64 world units.
            float sx = arrowWidth / 0.64f;
            float sy = arrowHeight / 0.64f;
            // Slight scale falloff for stacked chevrons (top one a bit smaller).
            float falloff = 1f - i * 0.18f;
            go.transform.localScale = new Vector3(sx * falloff, sy * falloff, 1f);

            arrows[i] = go.transform;
            baseLocal[i] = go.transform.localPosition;
        }
    }

    void Update()
    {
        if (arrows == null) return;
        float t = Time.time * bobSpeed * Mathf.PI * 2f + phaseOffset;
        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] == null) continue;
            float bob = Mathf.Sin(t + i * 0.6f) * bobAmount;
            Vector3 p = baseLocal[i];
            p.y += bob;
            arrows[i].localPosition = p;

            float pulse = 1f + Mathf.Sin(t * 0.9f + i * 0.4f) * pulseAmount;
            float falloff = 1f - i * 0.18f;
            arrows[i].localScale = new Vector3(
                (arrowWidth / 0.64f) * falloff * pulse,
                (arrowHeight / 0.64f) * falloff * pulse,
                1f);
        }
    }

    /// <summary>
    /// Generates an upward-pointing chevron / arrow sprite procedurally so this
    /// works without any imported art. Filled gold body with a dark outline.
    /// </summary>
    Sprite MakeArrowSprite(int w, int h, int outlineThickness)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];

        // Chevron geometry: filled upward-pointing chevron (V-shape pointing up).
        // Apex at (w/2, h-1). Two slanted edges go down-left and down-right.
        // Inner notch at (w/2, h*0.45) creates the "chevron" indent.
        float halfW = w * 0.5f;
        float chevronThickness = h * 0.5f; // arms are this thick vertically
        float apexY = h - 1f;
        float notchY = h * 0.40f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float dx = Mathf.Abs(x - halfW);
                // Outer slope: y goes from apexY down to 0 as we go from center to edge.
                float yOuter = Mathf.Lerp(apexY, 0f, dx / halfW);
                // Inner slope (the notch): y goes from notchY down to bottom.
                float yInner = Mathf.Lerp(notchY, -chevronThickness, dx / halfW);

                bool insideOuter = y <= yOuter;
                bool aboveInner = y >= yInner;
                bool inside = insideOuter && aboveInner;

                if (!inside) { px[y * w + x] = Color.clear; continue; }

                // Outline test — distance from any boundary.
                float distToOuter = yOuter - y;
                float distToInner = y - yInner;
                float distToHorizSide = halfW - dx;
                float distMin = Mathf.Min(distToOuter, Mathf.Min(distToInner, distToHorizSide));

                if (distMin < outlineThickness) px[y * w + x] = outlineColor;
                else px[y * w + x] = Color.white;
            }
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
