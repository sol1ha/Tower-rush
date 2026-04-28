using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Hourglass / sand-jar elapsed-time clock. Replaces the digital pixel clock.
/// Drains the top orb over <see cref="minuteSeconds"/> seconds, fills the bottom
/// orb in sync, then "flips" — and bumps difficulty (passed to BulletSpawner).
/// Every minute boundary spawns a shaky "X MINUTE PASSED" toast text.
/// A small uppercase "MM:SS" label sits next to the jar.
/// </summary>
public class SandJarClock : MonoBehaviour
{
    public enum Corner { BottomRight, BottomLeft, TopRight, TopLeft }

    [Header("Placement")]
    public Corner anchorCorner = Corner.BottomRight;
    [Tooltip("Padding from the chosen corner.")]
    public Vector2 padding = new Vector2(160f, 120f);

    [Header("Sand jar look")]
    public Color sandColor = new Color(0.93f, 0.20f, 0.42f, 1f);  // hot pink
    public Color outlineColor = new Color(0.55f, 0.78f, 0.80f, 1f); // soft cyan
    public float orbDiameter = 110f;
    public float neckHeight = 22f;
    public float neckWidth = 10f;
    public float orbOverlap = 6f;          // pull orbs slightly toward the neck

    [Header("Timing & difficulty")]
    [Tooltip("How many seconds the top orb takes to drain (one 'minute' of game time).")]
    public float minuteSeconds = 60f;

    [Header("Small text label")]
    public Color smallTextColor = new Color(1f, 1f, 1f, 0.85f);
    public int smallTextFontSize = 18;
    public Vector2 smallTextOffset = new Vector2(0f, -78f);

    [Header("Minute-passed toast")]
    public string toastFormat = "{0} MINUTE PASSED";
    public string hardModeText = "HARD MODE";
    public string extremeModeText = "EXTREME MODE";
    public Color toastColor = new Color(1f, 0.95f, 0.40f, 1f);
    public int toastFontSize = 64;
    public float toastDuration = 1.8f;
    public float toastShakeAmount = 14f;

    private float elapsed;
    private int currentMinute = 0;
    private bool started;

    private RectTransform jarRect;
    private Image topOrbOutline;
    private Image topSandFill;
    private Image bottomOrbOutline;
    private Image bottomSandFill;
    private Image neckOutline;
    private Text smallText;

    private Sprite circleFill;
    private Sprite circleOutline;
    private Sprite roundedRect;

    void Start()
    {
        if (!started) Build();
    }

    void OnEnable()
    {
        elapsed = 0f;
        currentMinute = 0;
    }

    void Build()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        BuildSprites();

        // Container
        var jarGo = new GameObject("SandJar", typeof(RectTransform));
        jarGo.transform.SetParent(canvas.transform, false);
        jarRect = (RectTransform)jarGo.transform;

        Vector2 a, p, ap;
        switch (anchorCorner)
        {
            case Corner.BottomLeft:
                a = p = new Vector2(0f, 0f); ap = new Vector2(padding.x, padding.y); break;
            case Corner.TopLeft:
                a = p = new Vector2(0f, 1f); ap = new Vector2(padding.x, -padding.y); break;
            case Corner.TopRight:
                a = p = new Vector2(1f, 1f); ap = new Vector2(-padding.x, -padding.y); break;
            default: // BottomRight
                a = p = new Vector2(1f, 0f); ap = new Vector2(-padding.x, padding.y); break;
        }
        jarRect.anchorMin = a; jarRect.anchorMax = a; jarRect.pivot = p;
        float h = orbDiameter * 2f + neckHeight - orbOverlap * 2f;
        jarRect.sizeDelta = new Vector2(orbDiameter, h);
        jarRect.anchoredPosition = ap;

        // Top orb (outline + sand fill)
        var topY =  (h - orbDiameter) * 0.5f;
        var topOutlineGo = new GameObject("TopOrb", typeof(RectTransform));
        topOutlineGo.transform.SetParent(jarRect, false);
        var trt = (RectTransform)topOutlineGo.transform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.pivot = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(orbDiameter, orbDiameter);
        trt.anchoredPosition = new Vector2(0, topY);
        topOrbOutline = topOutlineGo.AddComponent<Image>();
        topOrbOutline.sprite = circleOutline; topOrbOutline.color = outlineColor;

        var topSandGo = new GameObject("TopSand", typeof(RectTransform));
        topSandGo.transform.SetParent(trt, false);
        var tsrt = (RectTransform)topSandGo.transform;
        tsrt.anchorMin = Vector2.zero; tsrt.anchorMax = Vector2.one;
        tsrt.offsetMin = tsrt.offsetMax = Vector2.zero;
        topSandFill = topSandGo.AddComponent<Image>();
        topSandFill.sprite = circleFill; topSandFill.color = sandColor;
        topSandFill.type = Image.Type.Filled;
        topSandFill.fillMethod = Image.FillMethod.Vertical;
        topSandFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        topSandFill.fillAmount = 1f;

        // Bottom orb
        var botY = -(h - orbDiameter) * 0.5f;
        var botOutlineGo = new GameObject("BottomOrb", typeof(RectTransform));
        botOutlineGo.transform.SetParent(jarRect, false);
        var brt = (RectTransform)botOutlineGo.transform;
        brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0.5f);
        brt.pivot = new Vector2(0.5f, 0.5f);
        brt.sizeDelta = new Vector2(orbDiameter, orbDiameter);
        brt.anchoredPosition = new Vector2(0, botY);
        bottomOrbOutline = botOutlineGo.AddComponent<Image>();
        bottomOrbOutline.sprite = circleOutline; bottomOrbOutline.color = outlineColor;

        var botSandGo = new GameObject("BottomSand", typeof(RectTransform));
        botSandGo.transform.SetParent(brt, false);
        var bsrt = (RectTransform)botSandGo.transform;
        bsrt.anchorMin = Vector2.zero; bsrt.anchorMax = Vector2.one;
        bsrt.offsetMin = bsrt.offsetMax = Vector2.zero;
        bottomSandFill = botSandGo.AddComponent<Image>();
        bottomSandFill.sprite = circleFill; bottomSandFill.color = sandColor;
        bottomSandFill.type = Image.Type.Filled;
        bottomSandFill.fillMethod = Image.FillMethod.Vertical;
        bottomSandFill.fillOrigin = (int)Image.OriginVertical.Bottom;
        bottomSandFill.fillAmount = 0f;

        // Neck (thin pink line + outline frames)
        var neckGo = new GameObject("Neck", typeof(RectTransform));
        neckGo.transform.SetParent(jarRect, false);
        var nrt = (RectTransform)neckGo.transform;
        nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0.5f);
        nrt.pivot = new Vector2(0.5f, 0.5f);
        nrt.sizeDelta = new Vector2(neckWidth, neckHeight);
        nrt.anchoredPosition = Vector2.zero;
        neckOutline = neckGo.AddComponent<Image>();
        neckOutline.sprite = roundedRect; neckOutline.color = sandColor;

        // Small "MM:SS" text below the jar
        var textGo = new GameObject("SmallTime", typeof(RectTransform));
        textGo.transform.SetParent(jarRect, false);
        var trtxt = (RectTransform)textGo.transform;
        trtxt.anchorMin = trtxt.anchorMax = new Vector2(0.5f, 0f);
        trtxt.pivot = new Vector2(0.5f, 1f);
        trtxt.sizeDelta = new Vector2(orbDiameter * 1.6f, 30f);
        trtxt.anchoredPosition = smallTextOffset;
        smallText = textGo.AddComponent<Text>();
        smallText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        smallText.fontSize = smallTextFontSize;
        smallText.color = smallTextColor;
        smallText.alignment = TextAnchor.MiddleCenter;
        smallText.fontStyle = FontStyle.Bold;
        smallText.text = "00:00";

        var outline = textGo.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.85f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        started = true;
    }

    void BuildSprites()
    {
        circleFill    = MakeCircle(128, Color.white, Color.clear, 0);
        circleOutline = MakeCircleOutline(128, Color.white, 5);
        roundedRect   = MakeRoundedRect(64, 32, 8, Color.white);
    }

    Sprite MakeCircle(int diameter, Color fill, Color border, int borderWidth)
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
                if (d > r) px[y * diameter + x] = Color.clear;
                else if (borderWidth > 0 && d > r - borderWidth) px[y * diameter + x] = border;
                else px[y * diameter + x] = fill;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite MakeCircleOutline(int diameter, Color color, int thickness)
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

    Sprite MakeRoundedRect(int w, int h, int r, Color color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int cx = x, cy = y; bool inCorner = false;
                if (x < r && y < r)                 { cx = r - x; cy = r - y; inCorner = true; }
                else if (x >= w - r && y < r)       { cx = x - (w - r - 1); cy = r - y; inCorner = true; }
                else if (x < r && y >= h - r)       { cx = r - x; cy = y - (h - r - 1); inCorner = true; }
                else if (x >= w - r && y >= h - r)  { cx = x - (w - r - 1); cy = y - (h - r - 1); inCorner = true; }
                if (inCorner)
                {
                    float d = Mathf.Sqrt(cx * cx + cy * cy);
                    if (d > r) px[y * w + x] = Color.clear;
                    else { Color c = color; c.a *= Mathf.Clamp01(r - d); px[y * w + x] = c; }
                }
                else px[y * w + x] = color;
            }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        if (!started) Build();
        if (jarRect == null) return;
        if (GameManager.instance == null || !GameManager.instance.play) return;

        float dt = Time.unscaledDeltaTime;
        elapsed += dt;

        float minuteProgress = Mathf.Clamp01((elapsed - currentMinute * minuteSeconds) / minuteSeconds);
        if (topSandFill != null) topSandFill.fillAmount = 1f - minuteProgress;
        if (bottomSandFill != null) bottomSandFill.fillAmount = minuteProgress;

        // Small text "MM:SS"
        int totalSec = Mathf.FloorToInt(elapsed);
        if (smallText != null)
        {
            int mm = totalSec / 60;
            int ss = totalSec % 60;
            smallText.text = mm.ToString("00") + ":" + ss.ToString("00");
        }

        // Cross a minute boundary?
        int minute = Mathf.FloorToInt(elapsed / minuteSeconds);
        if (minute > currentMinute)
        {
            currentMinute = minute;
            OnMinuteCompleted(minute);
        }
    }

    void OnMinuteCompleted(int minute)
    {
        // Difficulty: minute 1 = hard, minute 2+ = extreme
        var spawner = FindAnyObjectByType<BulletSpawner>();
        if (spawner != null) spawner.SetDifficultyLevel(minute);

        // Toast text
        string toast;
        if (minute == 1) toast = string.Format(toastFormat, "1") + "  •  " + hardModeText;
        else if (minute >= 2) toast = string.Format(toastFormat, minute.ToString()) + "  •  " + extremeModeText;
        else toast = string.Format(toastFormat, minute.ToString());
        StartCoroutine(ShowToast(toast));
    }

    IEnumerator ShowToast(string message)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) yield break;

        var go = new GameObject("MinuteToast", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 120f);
        rt.anchoredPosition = new Vector2(0, 60f);

        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = toastFontSize;
        t.color = toastColor;
        t.alignment = TextAnchor.MiddleCenter;
        t.fontStyle = FontStyle.Bold;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.text = message;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.95f);
        outline.effectDistance = new Vector2(2.5f, -2.5f);

        // Animate in (scale + shake) and out (fade)
        float duration = toastDuration;
        float t0 = 0f;
        Vector2 baseAnchored = rt.anchoredPosition;
        while (t0 < duration)
        {
            t0 += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t0 / duration);

            // Scale: 0.4 -> 1.1 -> 1.0 across the toast lifetime
            float scale;
            if (u < 0.25f) scale = Mathf.Lerp(0.4f, 1.15f, u / 0.25f);
            else if (u < 0.4f) scale = Mathf.Lerp(1.15f, 1.0f, (u - 0.25f) / 0.15f);
            else scale = 1f;
            rt.localScale = new Vector3(scale, scale, 1f);

            // Shake: strongest at first, decays
            float shakeStrength = Mathf.Clamp01(1f - u * 1.4f);
            float jx = (Mathf.PerlinNoise(Time.unscaledTime * 35f, 0f) - 0.5f) * 2f;
            float jy = (Mathf.PerlinNoise(0f, Time.unscaledTime * 35f) - 0.5f) * 2f;
            rt.anchoredPosition = baseAnchored + new Vector2(jx, jy) * toastShakeAmount * shakeStrength;

            // Fade out at the end
            float alpha = u < 0.7f ? 1f : Mathf.Lerp(1f, 0f, (u - 0.7f) / 0.3f);
            t.color = new Color(toastColor.r, toastColor.g, toastColor.b, alpha);

            yield return null;
        }
        Destroy(go);
    }

    public void Hide()
    {
        if (jarRect != null) jarRect.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (jarRect != null) jarRect.gameObject.SetActive(true);
    }

    public void ResetTime()
    {
        elapsed = 0f;
        currentMinute = 0;
        if (topSandFill != null) topSandFill.fillAmount = 1f;
        if (bottomSandFill != null) bottomSandFill.fillAmount = 0f;
        // Reset bullet difficulty to normal too.
        var spawner = FindAnyObjectByType<BulletSpawner>();
        if (spawner != null) spawner.SetDifficultyLevel(0);
    }
}
