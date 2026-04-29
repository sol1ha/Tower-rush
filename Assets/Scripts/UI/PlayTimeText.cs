using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Elapsed-play-time label. Counts up in MM:SS while the game is being played.
///
/// Resolution order for the label it drives:
///   1. <see cref="tmpLabel"/>   (assigned in inspector)
///   2. <see cref="legacyLabel"/> (assigned in inspector)
///   3. A TMP_Text or Text component on the SAME GameObject this script lives on
///   4. Auto-create a styled legacy Text in the canvas (fallback)
///
/// Adds a subtle scale pop on every second tick and a slow breathing pulse so
/// the timer feels alive without being distracting.
/// </summary>
public class PlayTimeText : MonoBehaviour
{
    public enum Corner { BottomRight, BottomLeft, TopRight, TopLeft }

    [Header("Reference — drag YOUR TMP text here")]
    [Tooltip("Drag a TextMeshProUGUI from your canvas. If assigned, the script writes MM:SS into it and animates it.")]
    public TMP_Text tmpLabel;
    [Tooltip("Or drag a legacy UI Text. Used if Tmp Label is null.")]
    public Text legacyLabel;

    [Header("Auto-create fallback (only if no label is assigned anywhere)")]
    public Corner anchorCorner = Corner.BottomRight;
    public Vector2 padding = new Vector2(140f, 70f);
    public int fontSize = 56;
    public Color autoCreatedTextColor = new Color(0.20f, 0.85f, 0.30f, 1f); // green
    public Vector2 textBoxSize = new Vector2(220f, 80f);

    [Header("Animation")]
    [Tooltip("Peak scale on each second tick (1.0 = no pop).")]
    public float popPeakScale = 1.10f;
    [Tooltip("Seconds the pop takes to settle back to 1.")]
    public float popDuration = 0.30f;
    [Tooltip("Idle breathing scale amplitude (0 = none).")]
    public float breathAmount = 0.025f;
    [Tooltip("Idle breath cycles per second.")]
    public float breathSpeed = 0.7f;

    private RectTransform rect;
    private bool built;
    private float elapsed;
    private int lastSecond = -1;
    private float popTimer = 999f;

    void Start()
    {
        if (!built) Build();
    }

    public void ResetTime()
    {
        elapsed = 0f;
        lastSecond = -1;
        popTimer = 999f;
        SetText("00:00");
    }

    public void Show()
    {
        if (tmpLabel != null) tmpLabel.gameObject.SetActive(true);
        if (legacyLabel != null) legacyLabel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (tmpLabel != null) tmpLabel.gameObject.SetActive(false);
        if (legacyLabel != null) legacyLabel.gameObject.SetActive(false);
    }

    void SetText(string s)
    {
        if (tmpLabel != null) tmpLabel.text = s;
        if (legacyLabel != null) legacyLabel.text = s;
    }

    void Build()
    {
        // 1) Use whatever was wired up in the inspector.
        if (tmpLabel != null) { rect = tmpLabel.rectTransform; built = true; return; }
        if (legacyLabel != null) { rect = legacyLabel.rectTransform; built = true; return; }

        // 2) If a TMP_Text or Text component sits on the SAME GameObject as this
        //    script (the user just dropped this script onto their TMP text), use it.
        var tmpHere = GetComponent<TMP_Text>();
        if (tmpHere != null) { tmpLabel = tmpHere; rect = tmpHere.rectTransform; built = true; return; }
        var legacyHere = GetComponent<Text>();
        if (legacyHere != null) { legacyLabel = legacyHere; rect = legacyHere.rectTransform; built = true; return; }

        // 3) Fallback — auto-create a basic legacy Text in the first canvas we find.
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("PlayTimeText", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        rect = (RectTransform)go.transform;

        Vector2 a, p, ap;
        switch (anchorCorner)
        {
            case Corner.BottomLeft:
                a = p = new Vector2(0f, 0f); ap = new Vector2(padding.x, padding.y); break;
            case Corner.TopLeft:
                a = p = new Vector2(0f, 1f); ap = new Vector2(padding.x, -padding.y); break;
            case Corner.TopRight:
                a = p = new Vector2(1f, 1f); ap = new Vector2(-padding.x, -padding.y); break;
            default:
                a = p = new Vector2(1f, 0f); ap = new Vector2(-padding.x, padding.y); break;
        }
        rect.anchorMin = a; rect.anchorMax = a; rect.pivot = p;
        rect.sizeDelta = textBoxSize;
        rect.anchoredPosition = ap;

        legacyLabel = go.AddComponent<Text>();
        legacyLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        legacyLabel.fontSize = fontSize;
        legacyLabel.color = autoCreatedTextColor;
        legacyLabel.alignment = TextAnchor.MiddleCenter;
        legacyLabel.fontStyle = FontStyle.Bold;
        legacyLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        legacyLabel.verticalOverflow = VerticalWrapMode.Overflow;
        legacyLabel.text = "00:00";

        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        built = true;
    }

    void Update()
    {
        if (!built) Build();
        if (rect == null) return;
        if (GameManager.instance == null || !GameManager.instance.play) return;

        float dt = Time.unscaledDeltaTime;
        elapsed += dt;
        popTimer += dt;

        int sec = Mathf.FloorToInt(elapsed);
        if (sec != lastSecond)
        {
            lastSecond = sec;
            popTimer = 0f;
        }

        int mm = sec / 60;
        int ss = sec % 60;
        SetText(mm.ToString("00") + ":" + ss.ToString("00"));

        // Pop on every second + slow idle breath.
        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));
        float breath = 1f + Mathf.Sin(Time.unscaledTime * breathSpeed * Mathf.PI * 2f) * breathAmount;
        float scale = popScale * breath;
        rect.localScale = new Vector3(scale, scale, 1f);
    }

    static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float k = t - 1f;
        return 1f + c3 * k * k * k + c1 * k * k;
    }
}
