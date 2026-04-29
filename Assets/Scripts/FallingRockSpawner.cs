using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Periodically drops rocks from above on the player. Each rock telegraphs a
/// short warning circle at the impact X for ~0.8 seconds so the player has a
/// chance to dodge. Rate scales with the player's height (climb higher = more
/// rocks). Procedurally generates the rock and warning sprites at runtime so
/// no asset import is needed.
/// </summary>
public class FallingRockSpawner : MonoBehaviour
{
    [Header("Activation")]
    [Tooltip("Player Y position where rocks start spawning. Below this, never spawns.")]
    public float activateAtHeight = 30f;
    [Tooltip("Player Y position where the spawn rate maxes out.")]
    public float fullDifficultyHeight = 250f;

    [Header("Timing (scales with height)")]
    [Tooltip("Seconds between rocks at activateAtHeight.")]
    public float intervalAtMinHeight = 9f;
    [Tooltip("Seconds between rocks at fullDifficultyHeight.")]
    public float intervalAtMaxHeight = 2.5f;
    [Tooltip("Seconds the warning ring is shown before the rock spawns. Longer = more time to dodge.")]
    public float warningDuration = 1.6f;

    [Header("Rock")]
    [Tooltip("World units above the player where the rock starts falling. Should be small enough to stay in view.")]
    public float spawnHeightAbovePlayer = 7f;
    [Tooltip("Damage dealt by each rock.")]
    public int rockDamage = 1;
    [Tooltip("Rock fall speed (world units/sec).")]
    public float fallSpeed = 9f;
    [Tooltip("Diameter of the rock sprite in world units.")]
    public float rockSize = 1.1f;
    [Tooltip("How far ahead of the player's current X the rock can land (random ± this).")]
    public float horizontalSpread = 4f;

    [Header("Warning indicator")]
    [Tooltip("Diameter of the warning ring in world units.")]
    public float warningDiameter = 2.4f;
    public Color warningColor = new Color(1f, 0.18f, 0.18f, 0.95f);
    public Color rockColor = new Color(0.55f, 0.30f, 0.20f, 1f);
    public Color rockOutlineColor = new Color(0.20f, 0.10f, 0.05f, 1f);

    private Camera cam;
    private Transform player;
    private float nextSpawnTime;
    private Sprite rockSprite;
    private Sprite warningSprite;

    void Start()
    {
        cam = Camera.main;
        TryFindPlayer();
        BuildSprites();
        nextSpawnTime = Time.time + intervalAtMinHeight;
    }

    void TryFindPlayer()
    {
        var po = GameObject.FindGameObjectWithTag("Player");
        if (po != null) player = po.transform;
    }

    void BuildSprites()
    {
        rockSprite = MakeRockSprite(64, rockColor, rockOutlineColor, 4);
        warningSprite = MakeRingSprite(64, Color.white, 5);
    }

    void Update()
    {
        if (!GameManager.Playing()) return;
        if (player == null) { TryFindPlayer(); if (player == null) return; }

        if (player.position.y < activateAtHeight) return;
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        if (Time.time < nextSpawnTime) return;

        // Decide next interval based on the player's height.
        float t = Mathf.InverseLerp(activateAtHeight, fullDifficultyHeight, player.position.y);
        float interval = Mathf.Lerp(intervalAtMinHeight, intervalAtMaxHeight, t);
        nextSpawnTime = Time.time + interval;

        StartCoroutine(SpawnWithWarning());
    }

    IEnumerator SpawnWithWarning()
    {
        if (player == null) yield break;

        // Pick a random X near the player. We freeze it so a moving player can
        // actually escape the predicted strike zone.
        float targetX = player.position.x + Random.Range(-horizontalSpread, horizontalSpread);
        float warnY = player.position.y + 1.2f; // a bit above the player

        // Group: red ring + bright "!" mark in the middle so the threat reads
        // immediately at a glance.
        var warnGo = new GameObject("RockWarning");
        warnGo.transform.position = new Vector3(targetX, warnY, 0f);
        warnGo.transform.localScale = Vector3.one * (warningDiameter / 0.64f);

        var ringGo = new GameObject("Ring");
        ringGo.transform.SetParent(warnGo.transform, false);
        var ringSr = ringGo.AddComponent<SpriteRenderer>();
        ringSr.sprite = warningSprite;
        ringSr.color = warningColor;
        ringSr.sortingOrder = 6;

        // "!" mark drawn as 3 small filled bars (top stem + bottom dot) using
        // procedural sprites — tiny addition for unmistakable danger reading.
        var bangGo = new GameObject("Bang");
        bangGo.transform.SetParent(warnGo.transform, false);
        bangGo.transform.localScale = Vector3.one * 0.55f;
        var bangSr = bangGo.AddComponent<SpriteRenderer>();
        if (cachedBangSprite == null) cachedBangSprite = MakeBangSprite();
        bangSr.sprite = cachedBangSprite;
        bangSr.color = warningColor;
        bangSr.sortingOrder = 7;

        // Vertical "drop line" connecting the warning to the spawn point so the
        // player can see EXACTLY where the rock is coming from.
        var lineGo = new GameObject("DropLine");
        lineGo.transform.SetParent(warnGo.transform, false);
        var lineSr = lineGo.AddComponent<SpriteRenderer>();
        if (cachedLineSprite == null) cachedLineSprite = MakeLineSprite();
        lineSr.sprite = cachedLineSprite;
        Color lineCol = warningColor; lineCol.a = 0.55f;
        lineSr.color = lineCol;
        lineSr.sortingOrder = 5;
        // Line stretches from warning up to the spawn height.
        float lineHeight = spawnHeightAbovePlayer;
        lineGo.transform.localScale = new Vector3(0.05f / 0.04f, lineHeight / (0.64f), 1f);
        lineGo.transform.localPosition = new Vector3(0f, lineHeight * 0.5f / (warningDiameter / 0.64f), 0f);

        float t = 0f;
        while (t < warningDuration)
        {
            t += Time.deltaTime;
            // Faster, stronger pulse so it's unmissable.
            float pulse = 0.5f + Mathf.Sin(t * 16f) * 0.5f;
            Color c = warningColor;
            c.a = Mathf.Lerp(0.55f, 1f, pulse);
            ringSr.color = c;
            bangSr.color = c;
            // Slight scale pulse on the ring for extra urgency.
            float s = 1f + Mathf.Sin(t * 8f) * 0.10f;
            ringGo.transform.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        Destroy(warnGo);

        // Spawn the rock just above the player so it's visibly falling in
        // their view, not popping in from offscreen at the last second.
        float spawnY = player != null ? player.position.y + spawnHeightAbovePlayer
                                       : cam.transform.position.y + cam.orthographicSize;
        var rockGo = new GameObject("FallingRock");
        rockGo.transform.position = new Vector3(targetX, spawnY, 0f);
        var rsr = rockGo.AddComponent<SpriteRenderer>();
        rsr.sprite = rockSprite;
        rsr.sortingOrder = 7;
        rockGo.transform.localScale = Vector3.one * (rockSize / 0.64f);

        var col = rockGo.AddComponent<CircleCollider2D>();
        col.radius = 0.30f;
        col.isTrigger = true;

        var rb = rockGo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var rock = rockGo.AddComponent<FallingRock>();
        rock.damage = rockDamage;
        rock.fallSpeed = fallSpeed;
    }

    static Sprite cachedBangSprite;
    static Sprite cachedLineSprite;

    Sprite MakeBangSprite()
    {
        int w = 64, h = 96;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        // Top stem (y >= 28): vertical bar, 12 px wide centered.
        // Bottom dot (y < 18): small filled rounded square.
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            bool inStem = y >= 28 && y <= h - 6 && Mathf.Abs(x - w / 2f) < 7f;
            bool inDot  = y >= 6 && y <= 22 && Mathf.Abs(x - w / 2f) < 9f && Mathf.Abs(y - 14) < 9f;
            px[y * w + x] = (inStem || inDot) ? Color.white : Color.clear;
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite MakeLineSprite()
    {
        int w = 4, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color[] px = new Color[w * h];
        for (int i = 0; i < px.Length; i++) px[i] = Color.white;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    // ---------- procedural sprites ----------
    Sprite MakeRockSprite(int diameter, Color fill, Color border, int borderWidth)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[diameter * diameter];
        float r = diameter / 2f;
        for (int y = 0; y < diameter; y++)
        for (int x = 0; x < diameter; x++)
        {
            float dx = x - r + 0.5f;
            float dy = y - r + 0.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            // Slightly noisy edge so it looks more like a rock than a clean disc.
            float jitter = Mathf.PerlinNoise(x * 0.18f, y * 0.18f) * 2f;
            float effectiveR = r - jitter * 0.5f;
            if (d > effectiveR) px[y * diameter + x] = Color.clear;
            else if (borderWidth > 0 && d > effectiveR - borderWidth) px[y * diameter + x] = border;
            else
            {
                // Subtle radial shading for depth.
                float shade = Mathf.Lerp(1f, 0.7f, d / effectiveR);
                Color c = fill; c.r *= shade; c.g *= shade; c.b *= shade;
                px[y * diameter + x] = c;
            }
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite MakeRingSprite(int diameter, Color color, int thickness)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[diameter * diameter];
        float r = diameter / 2f;
        for (int y = 0; y < diameter; y++)
        for (int x = 0; x < diameter; x++)
        {
            float dx = x - r + 0.5f;
            float dy = y - r + 0.5f;
            float d = Mathf.Sqrt(dx * dx + dy * dy);
            if (d > r || d < r - thickness) px[y * diameter + x] = Color.clear;
            else px[y * diameter + x] = color;
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }
}
