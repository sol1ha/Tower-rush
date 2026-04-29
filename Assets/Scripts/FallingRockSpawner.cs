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
    [Tooltip("Seconds the warning circle is shown before the rock spawns.")]
    public float warningDuration = 0.8f;

    [Header("Rock")]
    [Tooltip("World units off-screen (above the camera) where the rock spawns.")]
    public float spawnOffsetAboveCamera = 6f;
    [Tooltip("Damage dealt by each rock.")]
    public int rockDamage = 1;
    [Tooltip("Rock fall speed (world units/sec).")]
    public float fallSpeed = 14f;
    [Tooltip("Diameter of the rock sprite in world units.")]
    public float rockSize = 0.9f;
    [Tooltip("How far ahead of the player's current X the rock can land (random ± this).")]
    public float horizontalSpread = 4f;

    [Header("Warning indicator")]
    [Tooltip("Diameter of the warning circle in world units.")]
    public float warningDiameter = 1.5f;
    public Color warningColor = new Color(1f, 0.25f, 0.25f, 0.55f);
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

        // Pick a random X near the player.
        float targetX = player.position.x + Random.Range(-horizontalSpread, horizontalSpread);
        float warnY = player.position.y + 0.4f; // a bit above the player so it's visible

        // Warning circle that pulses at the impact X.
        var warnGo = new GameObject("RockWarning");
        warnGo.transform.position = new Vector3(targetX, warnY, 0f);
        var sr = warnGo.AddComponent<SpriteRenderer>();
        sr.sprite = warningSprite;
        sr.color = warningColor;
        sr.sortingOrder = 6;
        warnGo.transform.localScale = Vector3.one * (warningDiameter / 0.64f);

        float t = 0f;
        while (t < warningDuration)
        {
            t += Time.deltaTime;
            // Pulsing alpha for visibility.
            float pulse = 0.5f + Mathf.Sin(t * 12f) * 0.5f;
            Color c = warningColor;
            c.a = Mathf.Lerp(0.35f, 0.95f, pulse);
            sr.color = c;
            // Update X each frame in case the player moves the warning with them — keep static for fairness.
            yield return null;
        }
        Destroy(warnGo);

        // Spawn the rock above the camera at the same X, falling straight down.
        float topY = cam.transform.position.y + cam.orthographicSize + spawnOffsetAboveCamera;
        var rockGo = new GameObject("FallingRock");
        rockGo.transform.position = new Vector3(targetX, topY, 0f);
        var rsr = rockGo.AddComponent<SpriteRenderer>();
        rsr.sprite = rockSprite;
        rsr.sortingOrder = 7;
        rockGo.transform.localScale = Vector3.one * (rockSize / 0.64f);

        var col = rockGo.AddComponent<CircleCollider2D>();
        col.radius = 0.30f;
        col.isTrigger = true;

        var rb = rockGo.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // we move it ourselves
        rb.gravityScale = 0f;

        var rock = rockGo.AddComponent<FallingRock>();
        rock.damage = rockDamage;
        rock.fallSpeed = fallSpeed;
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
