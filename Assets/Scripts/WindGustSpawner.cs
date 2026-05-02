using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Periodically launches a sideways wind gust that pushes the player
/// horizontally for a few seconds. Gives the player a clear warning banner
/// (e.g. "WIND GUST →") for 1.5s before the gust hits, then animated wind
/// streaks while it's active. Doesn't damage — just disrupts platforming.
/// Activates above a configurable height so early-game stays calm, and
/// scales frequency with the player's altitude.
/// </summary>
public class WindGustSpawner : MonoBehaviour
{
    [Header("Activation")]
    [Tooltip("Player Y where wind gusts start. Below this, never spawns.")]
    public float activateAtHeight = 25f;
    [Tooltip("Player Y where the gust frequency maxes out.")]
    public float fullDifficultyHeight = 220f;

    [Header("Timing")]
    [Tooltip("Seconds between gusts at activateAtHeight.")]
    public float intervalAtMinHeight = 22f;
    [Tooltip("Seconds between gusts at fullDifficultyHeight.")]
    public float intervalAtMaxHeight = 12f;
    [Tooltip("Minimum quiet time after the previous gust ends before another can fire.")]
    public float minGapAfterGust = 10f;
    [Tooltip("Warning banner duration before the gust takes effect.")]
    public float warningDuration = 1.5f;
    [Tooltip("How long the gust pushes the player.")]
    public float gustDuration = 2.5f;

    [Header("Force")]
    [Tooltip("Horizontal speed the wind drags the player at, in world units/sec. Direction matches the banner arrow.")]
    public float gustForce = 9f;

    [Header("Look")]
    public Color warningColor = new Color(1.00f, 0.85f, 0.20f, 1f);
    public Color streakColor = new Color(0.85f, 0.95f, 1.00f, 0.95f);
    public int streakCount = 28;

    /// <summary>
    /// Set by the active wind coroutine and read by Player.FixedUpdate so the
    /// wind force isn't overwritten by movement input every frame. Positive =
    /// pushes right; negative = pushes left; 0 = no wind.
    /// </summary>
    public static float CurrentForce = 0f;

    private Camera cam;
    private Rigidbody2D playerRb;
    private Transform player;
    private float nextGustTime;
    private Sprite streakSprite;

    void Start()
    {
        cam = Camera.main;
        TryFindPlayer();
        streakSprite = MakeStreakSprite();
        nextGustTime = Time.time + intervalAtMinHeight;
    }

    void TryFindPlayer()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
        {
            player = go.transform;
            playerRb = go.GetComponent<Rigidbody2D>();
        }
    }

    void Update()
    {
        if (!GameManager.Playing()) return;
        if (player == null) { TryFindPlayer(); if (player == null) return; }
        if (player.position.y < activateAtHeight) return;
        if (Time.time < nextGustTime) return;

        // Don't start a gust while bullets are about to fire — both at once is
        // overwhelming. Wait a bit and re-check next frame.
        var spawner = FindAnyObjectByType<BulletSpawner>();
        if (spawner != null && spawner.IsWaveQueuedOrFiring())
        {
            nextGustTime = Time.time + 1f;
            return;
        }

        float t = Mathf.InverseLerp(activateAtHeight, fullDifficultyHeight, player.position.y);
        float interval = Mathf.Lerp(intervalAtMinHeight, intervalAtMaxHeight, t);
        // Add minGapAfterGust so each cycle has a clearly-quiet phase.
        nextGustTime = Time.time + interval + warningDuration + gustDuration + minGapAfterGust;

        bool fromLeft = Random.value < 0.5f;
        StartCoroutine(RunGust(fromLeft));
    }

    IEnumerator RunGust(bool windBlowsRight)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) yield break;

        // ---- warning banner: huge, centred on screen so it's unmissable ----
        var bannerGo = new GameObject("WindBanner", typeof(RectTransform));
        bannerGo.transform.SetParent(canvas.transform, false);
        var bannerRt = (RectTransform)bannerGo.transform;
        bannerRt.anchorMin = bannerRt.anchorMax = new Vector2(0.5f, 0.5f);
        bannerRt.pivot = new Vector2(0.5f, 0.5f);
        bannerRt.sizeDelta = new Vector2(1100, 180);
        bannerRt.anchoredPosition = new Vector2(0, 100);

        // Solid backing panel so the text isn't lost against the desert sky.
        var bannerBg = new GameObject("Bg", typeof(RectTransform));
        bannerBg.transform.SetParent(bannerGo.transform, false);
        var bgRt = (RectTransform)bannerBg.transform;
        bgRt.anchorMin = Vector2.zero; bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        var bgImg = bannerBg.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.78f);
        bgImg.raycastTarget = false;

        var bannerText = bannerGo.AddComponent<Text>();
        bannerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bannerText.fontSize = 96;
        bannerText.fontStyle = FontStyle.Bold;
        bannerText.alignment = TextAnchor.MiddleCenter;
        bannerText.color = warningColor;
        bannerText.horizontalOverflow = HorizontalWrapMode.Overflow;
        bannerText.verticalOverflow = VerticalWrapMode.Overflow;
        bannerText.raycastTarget = false;
        // Single-phase banner: WIND GUST + arrow (no separate "warning" then
        // "active" phase — one banner shown for the whole event).
        bannerText.text = windBlowsRight ? "WIND GUST  →" : "←  WIND GUST";

        var bannerOutline = bannerGo.AddComponent<Outline>();
        bannerOutline.effectColor = new Color(0f, 0f, 0f, 1f);
        bannerOutline.effectDistance = new Vector2(4f, -4f);

        var bannerShadow = bannerGo.AddComponent<Shadow>();
        bannerShadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        bannerShadow.effectDistance = new Vector2(6f, -6f);

        float wt = 0f;
        Vector2 baseAnchor = bannerRt.anchoredPosition;
        while (wt < warningDuration)
        {
            wt += Time.deltaTime;
            // Pulse + small shake.
            float pulse = 0.5f + Mathf.Sin(wt * 12f) * 0.5f;
            float scale = Mathf.Lerp(0.95f, 1.10f, pulse);
            bannerRt.localScale = new Vector3(scale, scale, 1f);
            float jx = (Mathf.PerlinNoise(Time.time * 30f, 0) - 0.5f) * 2f * 6f;
            bannerRt.anchoredPosition = baseAnchor + new Vector2(jx, 0f);
            yield return null;
        }
        bannerRt.localScale = Vector3.one;

        // (Single-phase banner — text stays the same for the whole event.)

        // ---- spawn streaks that scroll across the screen during the gust ----
        var streakHolder = new GameObject("WindStreaks");
        streakHolder.transform.position = new Vector3(0, 0, 0);
        SpriteRenderer[] streaks = SpawnStreaks(streakHolder.transform, windBlowsRight);

        // ---- apply force loop ----
        float gt = 0f;
        while (gt < gustDuration)
        {
            gt += Time.deltaTime;
            // Sine-eased ramp 0..1..0 so the wind feels like a real gust
            // (kicks in, peaks mid-way, eases out).
            float ramp = Mathf.Sin(Mathf.Clamp01(gt / gustDuration) * Mathf.PI);
            float push = gustForce * ramp * (windBlowsRight ? 1f : -1f);

            // Player.FixedUpdate overwrites velocity.x with movement input
            // every frame, so we expose CurrentForce statically and let
            // Player.cs add it to velocity.x there.
            CurrentForce = push;

            UpdateStreaks(streaks, windBlowsRight, gt);
            yield return null;
        }
        CurrentForce = 0f;

        // Cleanup banner + streaks (fade out 0.4s).
        float fadeT = 0f;
        while (fadeT < 0.4f)
        {
            fadeT += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, fadeT / 0.4f);
            Color bc = bannerText.color; bc.a = a; bannerText.color = bc;
            foreach (var s in streaks) if (s != null) { Color c = s.color; c.a *= 0.85f; s.color = c; }
            yield return null;
        }
        Destroy(bannerGo);
        Destroy(streakHolder);
    }

    SpriteRenderer[] SpawnStreaks(Transform parent, bool blowsRight)
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return new SpriteRenderer[0];

        var arr = new SpriteRenderer[streakCount];
        float halfW = cam.orthographicSize * cam.aspect;
        float halfH = cam.orthographicSize;
        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        for (int i = 0; i < streakCount; i++)
        {
            var go = new GameObject("Streak" + i);
            go.transform.SetParent(parent, false);
            float x = camX + Random.Range(-halfW - 2f, halfW + 2f);
            float y = camY + Random.Range(-halfH * 0.95f, halfH * 0.95f);
            go.transform.position = new Vector3(x, y, 0f);
            // Longer + thicker streaks, with brighter alpha for visibility.
            float length = Random.Range(1.4f, 2.6f);
            float thick = Random.Range(0.10f, 0.18f);
            go.transform.localScale = new Vector3(length / 0.04f, thick / 0.64f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = streakSprite;
            // Vary alpha per streak so they shimmer.
            Color c = streakColor;
            c.a *= Random.Range(0.7f, 1.0f);
            sr.color = c;
            sr.sortingOrder = 8; // above platforms / decor
            arr[i] = sr;
        }
        return arr;
    }

    void UpdateStreaks(SpriteRenderer[] arr, bool blowsRight, float gustT)
    {
        if (cam == null) return;
        float halfW = cam.orthographicSize * cam.aspect;
        // Streaks scroll fast across the screen so the gust is unmistakable.
        float dx = (blowsRight ? 1f : -1f) * 32f * Time.deltaTime;

        foreach (var sr in arr)
        {
            if (sr == null) continue;
            Vector3 p = sr.transform.position;
            p.x += dx;
            // Wrap around when streak leaves the camera.
            float camX = cam.transform.position.x;
            if (blowsRight && p.x > camX + halfW + 1.5f)
                p.x = camX - halfW - 1.5f;
            else if (!blowsRight && p.x < camX - halfW - 1.5f)
                p.x = camX + halfW + 1.5f;

            // Keep streaks roughly aligned to camera Y.
            float camY = cam.transform.position.y;
            float halfH = cam.orthographicSize;
            if (Mathf.Abs(p.y - camY) > halfH * 0.85f)
                p.y = camY + Random.Range(-halfH * 0.85f, halfH * 0.85f);

            sr.transform.position = p;
        }
    }

    Sprite MakeStreakSprite()
    {
        int w = 4, h = 64;
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];
        // Soft horizontal gradient — more solid in the middle, fading at ends.
        for (int y = 0; y < h; y++)
        for (int x = 0; x < w; x++)
        {
            float dy = Mathf.Abs(y - h / 2f) / (h / 2f);
            float a = Mathf.Pow(1f - dy, 1.5f);
            px[y * w + x] = new Color(1f, 1f, 1f, a);
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }
}
