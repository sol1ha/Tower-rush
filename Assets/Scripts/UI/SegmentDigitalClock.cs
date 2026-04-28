using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A simple 7-segment LCD-style clock display built from <see cref="Image"/>
/// components. Renders 4 digits + a colon ("MM:SS") with no font dependency,
/// so it always looks like a digital clock regardless of which fonts are imported.
/// Use <see cref="SetTotalSeconds"/> to update the displayed time.
/// </summary>
public class SegmentDigitalClock : MonoBehaviour
{
    [Header("Look")]
    [Tooltip("Color of lit segments.")]
    public Color onColor = new Color(0.18f, 0.40f, 1.0f, 1f);
    [Tooltip("Color of unlit (off) segments. Set alpha to 0 to hide them.")]
    public Color offColor = new Color(0.18f, 0.40f, 1.0f, 0.10f);

    [Header("Digit dimensions")]
    public float digitWidth = 56f;
    public float digitHeight = 96f;
    public float segmentThickness = 14f;
    public float segmentGap = 3f;
    public float digitSpacing = 10f;
    public float colonWidth = 24f;
    public float colonDotSize = 14f;

    [Header("Segment slant (degrees)")]
    [Tooltip("Italic-style slant applied to each digit. 0 = upright, 6-10 = classic LCD look.")]
    public float slantDegrees = 6f;

    private DigitWidget[] digits;
    private Image colonTop;
    private Image colonBot;

    private class DigitWidget
    {
        public RectTransform rect;
        // Segments: 0=top, 1=topLeft, 2=topRight, 3=middle, 4=botLeft, 5=botRight, 6=bottom
        public Image[] segments = new Image[7];
    }

    // Lookup of which segments are ON for each digit 0..9.
    // bits: bit0=top, bit1=topLeft, bit2=topRight, bit3=middle, bit4=botLeft, bit5=botRight, bit6=bottom
    private static readonly int[] DIGIT_BITS = new int[]
    {
        0b1110111, // 0 — all except middle
        0b0100100, // 1 — topR + botR
        0b1011101, // 2 — top, topR, mid, botL, bot
        0b1101101, // 3 — top, topR, mid, botR, bot
        0b0101110, // 4 — topL, topR, mid, botR
        0b1101011, // 5 — top, topL, mid, botR, bot
        0b1111011, // 6 — top, topL, mid, botL, botR, bot
        0b0100101, // 7 — top, topR, botR
        0b1111111, // 8 — all
        0b1101111  // 9 — top, topL, topR, mid, botR, bot
    };

    void Awake()
    {
        if (digits == null) Build();
    }

    void Build()
    {
        digits = new DigitWidget[4];
        // Layout: D D : D D
        float colonX = 0f;                                  // centered
        float halfDigit = digitWidth * 0.5f;
        float gap = digitSpacing;
        float colonHalf = colonWidth * 0.5f;
        // positions of digit centers
        float x_d0 = -(colonHalf + gap + halfDigit + digitWidth + digitSpacing);
        float x_d1 = -(colonHalf + gap + halfDigit);
        float x_d2 =  (colonHalf + gap + halfDigit);
        float x_d3 =  (colonHalf + gap + halfDigit + digitWidth + digitSpacing);

        digits[0] = BuildDigit(new Vector2(x_d0, 0f));
        digits[1] = BuildDigit(new Vector2(x_d1, 0f));
        BuildColon(new Vector2(0f, 0f));
        digits[2] = BuildDigit(new Vector2(x_d2, 0f));
        digits[3] = BuildDigit(new Vector2(x_d3, 0f));

        SetTotalSeconds(0);
    }

    DigitWidget BuildDigit(Vector2 center)
    {
        var go = new GameObject("Digit", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(digitWidth, digitHeight);
        rt.anchoredPosition = center;
        rt.localRotation = Quaternion.Euler(0f, 0f, -slantDegrees);

        var w = new DigitWidget { rect = rt };

        float sx = digitWidth - segmentThickness - segmentGap * 2f;          // horizontal segment length
        float sy = (digitHeight - segmentThickness - segmentGap * 4f) * 0.5f; // vertical segment length

        // 0=top
        w.segments[0] = MakeSegment(go.transform, new Vector2(0, (digitHeight - segmentThickness) * 0.5f), new Vector2(sx, segmentThickness));
        // 1=top-left
        w.segments[1] = MakeSegment(go.transform, new Vector2(-(digitWidth - segmentThickness) * 0.5f, sy * 0.5f + segmentThickness * 0.5f + segmentGap), new Vector2(segmentThickness, sy));
        // 2=top-right
        w.segments[2] = MakeSegment(go.transform, new Vector2( (digitWidth - segmentThickness) * 0.5f, sy * 0.5f + segmentThickness * 0.5f + segmentGap), new Vector2(segmentThickness, sy));
        // 3=middle
        w.segments[3] = MakeSegment(go.transform, new Vector2(0, 0), new Vector2(sx, segmentThickness));
        // 4=bot-left
        w.segments[4] = MakeSegment(go.transform, new Vector2(-(digitWidth - segmentThickness) * 0.5f, -(sy * 0.5f + segmentThickness * 0.5f + segmentGap)), new Vector2(segmentThickness, sy));
        // 5=bot-right
        w.segments[5] = MakeSegment(go.transform, new Vector2( (digitWidth - segmentThickness) * 0.5f, -(sy * 0.5f + segmentThickness * 0.5f + segmentGap)), new Vector2(segmentThickness, sy));
        // 6=bot
        w.segments[6] = MakeSegment(go.transform, new Vector2(0, -(digitHeight - segmentThickness) * 0.5f), new Vector2(sx, segmentThickness));

        return w;
    }

    Image MakeSegment(Transform parent, Vector2 anchored, Vector2 size)
    {
        var seg = new GameObject("Seg", typeof(RectTransform));
        seg.transform.SetParent(parent, false);
        var rt = (RectTransform)seg.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = anchored;

        var img = seg.AddComponent<Image>();
        img.color = offColor;
        return img;
    }

    void BuildColon(Vector2 center)
    {
        var go = new GameObject("Colon", typeof(RectTransform));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(colonWidth, digitHeight);
        rt.anchoredPosition = center;
        rt.localRotation = Quaternion.Euler(0f, 0f, -slantDegrees);

        float dotOffset = digitHeight * 0.18f;
        colonTop = MakeSegment(go.transform, new Vector2(0, dotOffset), new Vector2(colonDotSize, colonDotSize));
        colonBot = MakeSegment(go.transform, new Vector2(0, -dotOffset), new Vector2(colonDotSize, colonDotSize));
        colonTop.color = onColor;
        colonBot.color = onColor;
    }

    public void SetTotalSeconds(int totalSeconds)
    {
        if (digits == null) Build();
        if (totalSeconds < 0) totalSeconds = 0;
        int mm = totalSeconds / 60;
        int ss = totalSeconds % 60;
        SetDigit(0, mm / 10);
        SetDigit(1, mm % 10);
        SetDigit(2, ss / 10);
        SetDigit(3, ss % 10);
    }

    void SetDigit(int slot, int value)
    {
        if (slot < 0 || slot >= digits.Length || digits[slot] == null) return;
        if (value < 0 || value > 9) return;
        int bits = DIGIT_BITS[value];
        var w = digits[slot];
        for (int i = 0; i < 7; i++)
        {
            bool on = (bits & (1 << i)) != 0;
            w.segments[i].color = on ? onColor : offColor;
        }
    }

    public void SetColor(Color on)
    {
        onColor = on;
        // Update lit segments only — preserve which segments are on/off.
        if (digits == null) return;
        if (colonTop != null) colonTop.color = on;
        if (colonBot != null) colonBot.color = on;
        // Match alpha for off color (keep a faint ghost segment in same hue).
        offColor = new Color(on.r, on.g, on.b, 0.10f);
        for (int s = 0; s < digits.Length; s++)
        {
            var w = digits[s];
            if (w == null) continue;
            for (int i = 0; i < 7; i++)
            {
                var img = w.segments[i];
                if (img == null) continue;
                if (img.color.a > 0.5f) img.color = on; // was lit
                else img.color = offColor;
            }
        }
    }

    public void SetColonBlink(bool visible)
    {
        if (colonTop != null) colonTop.color = visible ? onColor : offColor;
        if (colonBot != null) colonBot.color = visible ? onColor : offColor;
    }
}
