using System.Collections;
using UnityEngine;

/// <summary>
/// Counts UP from 00:00 while the game is being played and displays the elapsed
/// time as a 7-segment digital clock anchored to a corner of the canvas. Pauses
/// while the home menu is up or the player has died (GameManager.Playing()==false).
/// Adds subtle animation: a quick scale pop on every second tick, a colon blink,
/// and a soft scale-in entrance.
/// </summary>
public class PlayTimeClock : MonoBehaviour
{
    public enum Corner { BottomRight, BottomLeft, TopRight, TopLeft }

    [Header("Placement")]
    public Corner anchorCorner = Corner.BottomRight;
    [Tooltip("Padding from the chosen corner (positive numbers move the clock inward).")]
    public Vector2 padding = new Vector2(180f, 80f);

    [Header("Look")]
    public Color clockColor = new Color(0.95f, 0.85f, 0.55f, 1f); // warm sand-yellow
    public float digitWidth = 32f;
    public float digitHeight = 56f;
    public float segmentThickness = 8f;
    public float segmentSlant = 5f;

    [Header("Animation")]
    [Tooltip("Peak scale on each second tick.")]
    public float popPeakScale = 1.08f;
    [Tooltip("Seconds the pop takes to settle.")]
    public float popDuration = 0.28f;
    [Tooltip("Seconds the entrance scale-in takes.")]
    public float entranceDuration = 0.45f;
    [Tooltip("Initial scale of the entrance (smaller = more punchy).")]
    public float entranceFromScale = 0.5f;

    private SegmentDigitalClock clock;
    private RectTransform clockRect;
    private Vector2 baseAnchored;
    private float elapsed;
    private int lastShownSecond = -1;
    private float popTimer = 999f;
    private float entranceTimer = 0f;
    private bool built;

    void Start()
    {
        if (!built) Build();
    }

    void OnEnable()
    {
        elapsed = 0f;
        lastShownSecond = -1;
        popTimer = 999f;
        entranceTimer = 0f;
        if (clock != null) clock.gameObject.SetActive(true);
    }

    public void ResetTime()
    {
        elapsed = 0f;
        lastShownSecond = -1;
        popTimer = 999f;
        entranceTimer = 0f;
    }

    void Build()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("PlayTimeClock", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        clockRect = (RectTransform)go.transform;
        Vector2 anchorMin, anchorMax, pivot, anchored;
        switch (anchorCorner)
        {
            case Corner.BottomLeft:
                anchorMin = anchorMax = pivot = new Vector2(0f, 0f);
                anchored = new Vector2(padding.x, padding.y);
                break;
            case Corner.TopLeft:
                anchorMin = anchorMax = pivot = new Vector2(0f, 1f);
                anchored = new Vector2(padding.x, -padding.y);
                break;
            case Corner.TopRight:
                anchorMin = anchorMax = pivot = new Vector2(1f, 1f);
                anchored = new Vector2(-padding.x, -padding.y);
                break;
            default: // BottomRight
                anchorMin = anchorMax = pivot = new Vector2(1f, 0f);
                anchored = new Vector2(-padding.x, padding.y);
                break;
        }
        clockRect.anchorMin = anchorMin;
        clockRect.anchorMax = anchorMax;
        clockRect.pivot = pivot;
        clockRect.sizeDelta = new Vector2(digitWidth * 4 + 50f, digitHeight * 1.2f);
        clockRect.anchoredPosition = anchored;
        baseAnchored = anchored;

        clock = go.AddComponent<SegmentDigitalClock>();
        clock.digitWidth = digitWidth;
        clock.digitHeight = digitHeight;
        clock.segmentThickness = segmentThickness;
        clock.slantDegrees = segmentSlant;
        clock.onColor = clockColor;
        clock.SetTotalSeconds(0);

        built = true;
    }

    void Update()
    {
        if (!built) Build();
        if (clock == null || clockRect == null) return;

        // Only tick while the game is actually being played.
        if (GameManager.instance == null || !GameManager.instance.play) return;

        float dt = Time.unscaledDeltaTime;
        elapsed += dt;
        popTimer += dt;
        entranceTimer += dt;

        int sec = Mathf.FloorToInt(elapsed);
        if (sec != lastShownSecond)
        {
            lastShownSecond = sec;
            popTimer = 0f;
            clock.SetTotalSeconds(sec);
        }

        // Soft entrance scale-in.
        float ent = Mathf.Clamp01(entranceTimer / Mathf.Max(0.0001f, entranceDuration));
        float entranceScale = Mathf.LerpUnclamped(entranceFromScale, 1f, EaseOutBack(ent));

        // Pop on every second.
        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));

        float s = entranceScale * popScale;
        clockRect.localScale = new Vector3(s, s, 1f);

        // Colon blink (1 Hz).
        clock.SetColonBlink(((int)(Time.unscaledTime * 2f)) % 2 == 0);
    }

    static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float k = t - 1f;
        return 1f + c3 * k * k * k + c1 * k * k;
    }

    public void Hide()
    {
        if (clock != null) clock.gameObject.SetActive(false);
    }

    public void Show()
    {
        if (clock != null) clock.gameObject.SetActive(true);
    }
}
