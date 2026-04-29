using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plain elapsed-play-time text display: shows "MM:SS" and ticks up while
/// the game is being played. No animations, no effects, no extras —
/// just a clock label anchored to a corner of the canvas.
/// </summary>
public class PlayTimeText : MonoBehaviour
{
    public enum Corner { BottomRight, BottomLeft, TopRight, TopLeft }

    [Header("Placement")]
    public Corner anchorCorner = Corner.BottomRight;
    [Tooltip("Padding from the chosen corner.")]
    public Vector2 padding = new Vector2(140f, 70f);

    [Header("Look")]
    [Tooltip("Font size. The user wanted this 'a bit bigger' — default 56.")]
    public int fontSize = 56;
    public Color textColor = new Color(0.90f, 0.18f, 0.18f, 1f); // bold red
    public Vector2 textBoxSize = new Vector2(220f, 80f);

    private Text label;
    private RectTransform labelRect;
    private float elapsed;
    private bool built;

    void Start()
    {
        if (!built) Build();
    }

    public void ResetTime()
    {
        elapsed = 0f;
        if (label != null) label.text = "00:00";
    }

    public void Show() { if (label != null) label.gameObject.SetActive(true); }
    public void Hide() { if (label != null) label.gameObject.SetActive(false); }

    void Build()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("PlayTimeText", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);
        labelRect = (RectTransform)go.transform;

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
        labelRect.anchorMin = a; labelRect.anchorMax = a; labelRect.pivot = p;
        labelRect.sizeDelta = textBoxSize;
        labelRect.anchoredPosition = ap;

        label = go.AddComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.fontSize = fontSize;
        label.color = textColor;
        label.alignment = TextAnchor.MiddleCenter;
        label.fontStyle = FontStyle.Bold;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.text = "00:00";

        // Subtle outline so the text reads on any background.
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
        outline.effectDistance = new Vector2(2f, -2f);

        built = true;
    }

    void Update()
    {
        if (!built) Build();
        if (label == null) return;
        if (GameManager.instance == null || !GameManager.instance.play) return;

        elapsed += Time.unscaledDeltaTime;
        int total = Mathf.FloorToInt(elapsed);
        int mm = total / 60;
        int ss = total % 60;
        label.text = mm.ToString("00") + ":" + ss.ToString("00");
    }
}
