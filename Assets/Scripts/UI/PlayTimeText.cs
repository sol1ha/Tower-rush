using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Elapsed-play-time label. Counts up in MM:SS while the game is being played,
/// in a sci-fi/digital style font (Kenney Future Narrow, loaded from Resources)
/// with built-in animation: scale pop on every second tick, slow idle breath,
/// rotation kick at each tick, and a soft outline + drop-shadow for legibility.
///
/// Resolution order for the label it drives:
///   1. <see cref="tmpLabel"/>   (assigned in inspector)
///   2. <see cref="legacyLabel"/> (assigned in inspector)
///   3. A TMP_Text or Text component on the SAME GameObject this script lives on
///   4. Auto-create a styled legacy Text in the canvas (default — looks great
///      out of the box, no Unity setup required)
/// </summary>
public class PlayTimeText : MonoBehaviour
{
    public enum Corner { BottomRight, BottomLeft, TopRight, TopLeft }

    [Header("Reference (optional)")]
    [Tooltip("Drag a TextMeshProUGUI from your canvas. If null, the script auto-creates a styled label.")]
    public TMP_Text tmpLabel;
    [Tooltip("Or drag a legacy UI Text. Used if Tmp Label is null.")]
    public Text legacyLabel;

    [Header("Auto-created label look")]
    public Corner anchorCorner = Corner.BottomRight;
    public Vector2 padding = new Vector2(160f, 90f);
    public int fontSize = 72;
    public Color textColor = new Color(0.30f, 0.95f, 0.55f, 1f);   // bright green
    public Color outlineColor = new Color(0.05f, 0.20f, 0.10f, 1f); // deep forest outline
    public Vector2 textBoxSize = new Vector2(280f, 110f);
    [Tooltip("Path under any 'Resources' folder for a custom font (no extension). Leave empty to use Unity's built-in LiberationSans.")]
    public string customFontResourcePath = "Fonts/KenneyFutureNarrow";

    [Header("Animation — tick pop")]
    [Tooltip("Peak scale on every second tick.")]
    public float popPeakScale = 1.18f;
    [Tooltip("Seconds the pop takes to settle back to 1.")]
    public float popDuration = 0.32f;
    [Tooltip("Degrees of rotation kick at each tick (decays with the pop).")]
    public float popRotationKick = 5f;

    [Header("Animation — idle breath")]
    [Tooltip("Continuous scale wave amplitude (0 = none).")]
    public float breathAmount = 0.03f;
    [Tooltip("Breath cycles per second.")]
    public float breathSpeed = 0.9f;

    [Header("Animation — color flash")]
    [Tooltip("Brief brighten on each tick (0 = none).")]
    public float flashStrength = 0.35f;

    private RectTransform rect;
    private bool built;
    private float elapsed;
    private int lastSecond = -1;
    private float popTimer = 999f;
    private Color baseColor;

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

    void SetColor(Color c)
    {
        if (tmpLabel != null) tmpLabel.color = c;
        if (legacyLabel != null) legacyLabel.color = c;
    }

    void Build()
    {
        // 1) Use whatever was wired in the inspector.
        if (tmpLabel != null) { rect = tmpLabel.rectTransform; baseColor = tmpLabel.color; built = true; return; }
        if (legacyLabel != null) { rect = legacyLabel.rectTransform; baseColor = legacyLabel.color; built = true; return; }

        // 2) Same-GO components.
        var tmpHere = GetComponent<TMP_Text>();
        if (tmpHere != null) { tmpLabel = tmpHere; rect = tmpHere.rectTransform; baseColor = tmpHere.color; built = true; return; }
        var legacyHere = GetComponent<Text>();
        if (legacyHere != null) { legacyLabel = legacyHere; rect = legacyHere.rectTransform; baseColor = legacyHere.color; built = true; return; }

        // 3) No label wired up — do nothing. The auto-create fallback was
        //    removed because the user has their own scene TMP timer and the
        //    extra label was a duplicate.
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

        // ---- scale: pop on tick × idle breath ----
        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));
        float breath = 1f + Mathf.Sin(Time.unscaledTime * breathSpeed * Mathf.PI * 2f) * breathAmount;
        float scale = popScale * breath;
        rect.localScale = new Vector3(scale, scale, 1f);

        // ---- rotation: small kick on tick that decays ----
        float kick = Mathf.Sin(popProgress * Mathf.PI) * popRotationKick * (1f - popProgress);
        rect.localRotation = Quaternion.Euler(0f, 0f, kick);

        // ---- color flash on tick ----
        if (flashStrength > 0f)
        {
            float flash = Mathf.Lerp(flashStrength, 0f, popProgress);
            Color flashColor = Color.Lerp(baseColor, Color.white, flash);
            SetColor(flashColor);
        }
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
