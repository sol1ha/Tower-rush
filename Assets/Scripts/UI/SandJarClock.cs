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

    [Header("Sand jar look (gold sand on a dark-wood frame)")]
    public Color sandColor       = new Color(0.86f, 0.62f, 0.28f, 1f); // amber sand
    public Color woodColor       = new Color(0.32f, 0.18f, 0.10f, 1f); // dark walnut caps
    public Color pillarColor     = new Color(0.83f, 0.66f, 0.28f, 1f); // brass pillars
    public Color glassColor      = new Color(0.92f, 0.95f, 1.00f, 0.18f); // faint glass tint
    public Color glassEdgeColor  = new Color(0.55f, 0.78f, 0.80f, 0.55f); // glass outline highlight
    public float jarWidth = 130f;
    public float chamberHeight = 110f;
    public float capHeight = 22f;
    public float pillarWidth = 7f;
    public float neckHeight = 14f;

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

    [Header("Swing animation")]
    [Tooltip("Max swing angle in degrees (the jar rocks gently like it's hanging).")]
    public float swingAngle = 6f;
    [Tooltip("Swing cycles per second.")]
    public float swingSpeed = 0.6f;

    private float elapsed;
    private int currentMinute = 0;
    private bool started;

    private RectTransform jarRect;        // root — gets rotated for swing
    private RectTransform jarBodyRect;    // child — holds all the drawn parts (so swing rotates everything together)
    private Image topSandFill;
    private Image bottomSandFill;
    private Text smallText;

    private Sprite triDown;       // inverted triangle (▽) for top chamber sand
    private Sprite triUp;         // upright triangle (△) for bottom chamber sand
    private Sprite triDownGlass;  // glass overlay (faint) for top
    private Sprite triUpGlass;    // glass overlay (faint) for bottom
    private Sprite triDownEdge;   // glass edge outline for top
    private Sprite triUpEdge;     // glass edge outline for bottom
    private Sprite roundedRect;   // wood caps + neck

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

        // ---- root container (gets rotated for the swing) ----
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
        float totalH = chamberHeight * 2f + neckHeight + capHeight * 2f;
        jarRect.sizeDelta = new Vector2(jarWidth + pillarWidth * 2 + 12, totalH + 30f);
        jarRect.anchoredPosition = ap;

        // ---- body (rotation pivot for swing — top center, like it hangs from above) ----
        var bodyGo = new GameObject("Body", typeof(RectTransform));
        bodyGo.transform.SetParent(jarRect, false);
        jarBodyRect = (RectTransform)bodyGo.transform;
        jarBodyRect.anchorMin = jarBodyRect.anchorMax = new Vector2(0.5f, 1f);
        jarBodyRect.pivot = new Vector2(0.5f, 1f);
        jarBodyRect.sizeDelta = jarRect.sizeDelta;
        jarBodyRect.anchoredPosition = Vector2.zero;

        // ---- vertical layout: top-cap | top chamber | neck | bottom chamber | bottom-cap ----
        // Y positions are relative to body center.
        float yTopCap   =  (totalH - capHeight) * 0.5f;
        float yTopChamb =  (capHeight + chamberHeight) * 0.5f;
        // Use yTopChamb if needed below; we mostly position via anchored positions.

        // ---- gold side pillars (long thin rounded rects spanning full height) ----
        AddPart("PillarLeft", new Vector2(-(jarWidth * 0.5f + pillarWidth * 0.5f + 1f), 0f),
                new Vector2(pillarWidth, totalH), pillarColor, roundedRect);
        AddPart("PillarRight", new Vector2( (jarWidth * 0.5f + pillarWidth * 0.5f + 1f), 0f),
                new Vector2(pillarWidth, totalH), pillarColor, roundedRect);

        // ---- wood caps ----
        AddPart("CapTop", new Vector2(0f, yTopCap),
                new Vector2(jarWidth + pillarWidth * 2 + 16f, capHeight), woodColor, roundedRect);
        AddPart("CapBottom", new Vector2(0f, -yTopCap),
                new Vector2(jarWidth + pillarWidth * 2 + 16f, capHeight), woodColor, roundedRect);

        // Helpful y centres for chambers
        float yChamberTop = capHeight * 0.5f + chamberHeight * 0.5f + neckHeight * 0.5f;
        float yChamberBot = -yChamberTop;

        // ---- glass background (very faint) for top chamber (▽) ----
        AddPart("GlassTop", new Vector2(0f, yChamberTop),
                new Vector2(jarWidth, chamberHeight), glassColor, triDownGlass);

        // ---- top sand (▽), filled vertically from bottom origin so it 'drains' ----
        var topSand = AddPart("TopSand", new Vector2(0f, yChamberTop),
                              new Vector2(jarWidth, chamberHeight), sandColor, triDown);
        topSand.type = Image.Type.Filled;
        topSand.fillMethod = Image.FillMethod.Vertical;
        topSand.fillOrigin = (int)Image.OriginVertical.Bottom;
        topSand.fillAmount = 1f;
        topSandFill = topSand;

        // ---- top glass edge (outline) ----
        AddPart("GlassTopEdge", new Vector2(0f, yChamberTop),
                new Vector2(jarWidth, chamberHeight), glassEdgeColor, triDownEdge);

        // ---- glass background for bottom chamber (△) ----
        AddPart("GlassBottom", new Vector2(0f, yChamberBot),
                new Vector2(jarWidth, chamberHeight), glassColor, triUpGlass);

        // ---- bottom sand (△), filled vertically from bottom origin so it 'fills up' ----
        var botSand = AddPart("BottomSand", new Vector2(0f, yChamberBot),
                              new Vector2(jarWidth, chamberHeight), sandColor, triUp);
        botSand.type = Image.Type.Filled;
        botSand.fillMethod = Image.FillMethod.Vertical;
        botSand.fillOrigin = (int)Image.OriginVertical.Bottom;
        botSand.fillAmount = 0f;
        bottomSandFill = botSand;

        // ---- bottom glass edge ----
        AddPart("GlassBottomEdge", new Vector2(0f, yChamberBot),
                new Vector2(jarWidth, chamberHeight), glassEdgeColor, triUpEdge);

        // ---- neck (thin sand-colored line between chambers) ----
        AddPart("Neck", new Vector2(0f, 0f),
                new Vector2(6f, neckHeight + 4f), sandColor, roundedRect);

        // ---- small MM:SS label below ----
        var textGo = new GameObject("SmallTime", typeof(RectTransform));
        textGo.transform.SetParent(jarRect, false);
        var trtxt = (RectTransform)textGo.transform;
        trtxt.anchorMin = trtxt.anchorMax = new Vector2(0.5f, 0f);
        trtxt.pivot = new Vector2(0.5f, 1f);
        trtxt.sizeDelta = new Vector2(jarWidth * 1.6f, 30f);
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

    Image AddPart(string name, Vector2 anchored, Vector2 size, Color color, Sprite sprite)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(jarBodyRect, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchored;
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sprite != null && sprite.border != Vector4.zero) img.type = Image.Type.Sliced;
        return img;
    }

    void BuildSprites()
    {
        triDown      = MakeTriangle(160, 160, pointDown: true,  fill: Color.white, border: Color.clear, borderWidth: 0);
        triUp        = MakeTriangle(160, 160, pointDown: false, fill: Color.white, border: Color.clear, borderWidth: 0);
        triDownGlass = triDown;   // same shape, alpha applied by the Image color
        triUpGlass   = triUp;
        triDownEdge  = MakeTriangle(160, 160, pointDown: true,  fill: Color.clear, border: Color.white, borderWidth: 3);
        triUpEdge    = MakeTriangle(160, 160, pointDown: false, fill: Color.clear, border: Color.white, borderWidth: 3);
        roundedRect  = MakeRoundedRect(64, 32, 8, Color.white);
    }

    Sprite MakeTriangle(int w, int h, bool pointDown, Color fill, Color border, int borderWidth)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // For ▽ (pointDown): apex at (w/2, 0), base at top (y = h).
                //   inside if y >= h - 2h/w * min(x, w-x)
                // For △ (pointUp):   apex at (w/2, h), base at bottom (y = 0).
                //   inside if y <= 2h/w * min(x, w-x)
                float md = Mathf.Min(x, w - 1 - x);
                float boundary = (2f * h / w) * md;
                bool inside;
                float distFromEdge;
                if (pointDown)
                {
                    inside = y >= h - boundary;
                    distFromEdge = (y - (h - boundary));
                }
                else
                {
                    inside = y <= boundary;
                    distFromEdge = boundary - y;
                }
                if (!inside) { px[y * w + x] = Color.clear; continue; }

                if (borderWidth > 0)
                {
                    if (distFromEdge < borderWidth || md < borderWidth)
                    {
                        px[y * w + x] = border;
                        continue;
                    }
                }
                px[y * w + x] = fill;
            }
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
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

        // Swing animation runs even on the menu so it always feels alive.
        if (jarBodyRect != null && swingAngle > 0f)
        {
            float angle = Mathf.Sin(Time.unscaledTime * swingSpeed * Mathf.PI * 2f) * swingAngle;
            jarBodyRect.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

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
