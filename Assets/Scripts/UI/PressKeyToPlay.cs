using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class PressKeyToPlay : MonoBehaviour
{
    [Header("Auto-start timer")]
    [Tooltip("Seconds before the game auto-starts if no input is detected.")]
    public float autoStartSeconds = 30f;

    [Header("Optional countdown UI (legacy)")]
    [Tooltip("Optional TMP text that shows the remaining seconds. Format: \"Auto-start: {0}s\".")]
    public TMP_Text countdownTmp;
    [Tooltip("Optional legacy Text that shows the remaining seconds.")]
    public Text countdownText;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string countdownFormat = "Auto-start: {0}s";

    [Header("Digital 7-segment clock (matches the home menu)")]
    [Tooltip("If true, auto-creates a SegmentDigitalClock during play that mirrors the home-menu clock.")]
    public bool useSegmentClock = true;
    [Tooltip("Position of the in-game segment clock (relative to canvas center).")]
    public Vector2 segmentClockPosition = new Vector2(360f, 220f);
    public float segmentDigitWidth = 44f;
    public float segmentDigitHeight = 76f;
    public float segmentThickness = 11f;
    public float segmentSlant = 6f;

    [Header("Colors (dark-red palette to match home menu)")]
    public Color colorFull = new Color(0.58f, 0.08f, 0.08f, 1f);
    public Color colorMid  = new Color(0.75f, 0.14f, 0.14f, 1f);
    public Color colorLow  = new Color(0.95f, 0.22f, 0.22f, 1f);
    public float lowThreshold = 5f;
    public float midThreshold = 15f;

    private Rigidbody2D rb;
    private float autoStartTimer;

    private SegmentDigitalClock segmentClock;
    private RectTransform segmentClockRect;
    private Vector2 segmentClockBasePos;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        autoStartTimer = autoStartSeconds;

        if (useSegmentClock) BuildSegmentClock();
        UpdateCountdownUi();
    }

    void BuildSegmentClock()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        var go = new GameObject("InGameSegmentClock", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        segmentClockRect = (RectTransform)go.transform;
        segmentClockRect.anchorMin = new Vector2(0.5f, 0.5f);
        segmentClockRect.anchorMax = new Vector2(0.5f, 0.5f);
        segmentClockRect.pivot = new Vector2(0.5f, 0.5f);
        segmentClockRect.sizeDelta = new Vector2(segmentDigitWidth * 4 + 60f, segmentDigitHeight * 1.2f);
        segmentClockRect.anchoredPosition = segmentClockPosition;
        segmentClockBasePos = segmentClockPosition;

        segmentClock = go.AddComponent<SegmentDigitalClock>();
        segmentClock.digitWidth = segmentDigitWidth;
        segmentClock.digitHeight = segmentDigitHeight;
        segmentClock.segmentThickness = segmentThickness;
        segmentClock.slantDegrees = segmentSlant;
        segmentClock.onColor = colorFull;
        segmentClock.SetTotalSeconds(Mathf.CeilToInt(autoStartSeconds));
    }

    void Update()
    {
        autoStartTimer -= Time.deltaTime;
        UpdateCountdownUi();

        bool inputPressed = false;
        if (Keyboard.current != null)
            inputPressed = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame;
        if (!inputPressed && Gamepad.current != null)
            inputPressed = Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame;

        if (inputPressed || autoStartTimer <= 0f)
        {
            HideCountdownUi();
            this.enabled = false;

            // Route through HomeScreen.StartGame so time.timeScale is unfrozen,
            // hidden gameplay objects are activated, and music is swapped — not
            // just GameManager.play, which by itself leaves the world frozen.
            HomeScreen home = FindAnyObjectByType<HomeScreen>();
            if (home != null)
            {
                home.StartGame();
            }
            else
            {
                Time.timeScale = 1f;
                if (GameManager.instance != null)
                    GameManager.StartGame();
            }

            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 12f);
            }
        }
    }

    void UpdateCountdownUi()
    {
        int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(autoStartTimer));

        // Legacy text fields if assigned.
        if (countdownTmp != null || countdownText != null)
        {
            string text = string.Format(countdownFormat, secondsLeft);
            if (countdownTmp != null) countdownTmp.text = text;
            if (countdownText != null) countdownText.text = text;
        }

        // Segment clock — drives both digit display and color.
        if (segmentClock != null)
        {
            segmentClock.SetTotalSeconds(secondsLeft);
            Color baseColor;
            if (autoStartTimer <= lowThreshold)
            {
                float t = Mathf.InverseLerp(0f, lowThreshold, autoStartTimer);
                baseColor = Color.Lerp(colorLow, colorMid, t);
            }
            else if (autoStartTimer <= midThreshold)
            {
                float t = Mathf.InverseLerp(lowThreshold, midThreshold, autoStartTimer);
                baseColor = Color.Lerp(colorMid, colorFull, t);
            }
            else
            {
                baseColor = colorFull;
            }
            segmentClock.SetColor(baseColor);
            segmentClock.SetColonBlink(((int)(Time.unscaledTime * 2f)) % 2 == 0);
        }
    }

    void HideCountdownUi()
    {
        if (countdownTmp != null) countdownTmp.gameObject.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        if (segmentClock != null) segmentClock.gameObject.SetActive(false);
    }
}
