using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visible countdown shown on the home menu. Auto-creates a large, animated TMP
/// label inside the home panel's canvas and starts the game when the timer hits
/// zero. Also fires HomeScreen.StartGame() on Space / W / gamepad confirm.
/// </summary>
public class HomeMenuCountdown : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds before the game auto-starts.")]
    public float autoStartSeconds = 30f;

    [Header("Optional manual reference (auto-created if left empty)")]
    [Tooltip("If null, the script creates a TMP label inside the first Canvas it finds.")]
    public TMP_Text countdownLabel;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string format = "Starting in {0}s";

    [Header("Sizing")]
    public int fontSize = 96;
    public Vector2 anchoredPosition = new Vector2(0f, -180f);
    public Vector2 sizeDelta = new Vector2(900f, 200f);

    [Header("Effects")]
    [Tooltip("How fast the label pulses (scale wobble per second).")]
    public float pulseSpeed = 2.5f;
    [Tooltip("How much the label scales up at the peak of each pulse.")]
    public float pulseAmount = 0.12f;
    [Tooltip("Color when the timer is full.")]
    public Color colorFull = new Color(1f, 1f, 1f, 1f);
    [Tooltip("Color used when the timer is low (last 5 seconds).")]
    public Color colorLow = new Color(1f, 0.25f, 0.25f, 1f);
    [Tooltip("Color used at the mid range to give a warning hue.")]
    public Color colorMid = new Color(1f, 0.85f, 0.2f, 1f);
    [Tooltip("Below this many seconds, switch to the low color and shake.")]
    public float lowThreshold = 5f;
    [Tooltip("Below this many seconds, start blending toward the mid color.")]
    public float midThreshold = 15f;
    [Tooltip("Pixel amplitude of the shake at low time.")]
    public float lowShakeAmount = 8f;

    private float remaining;
    private RectTransform rect;
    private Vector2 baseAnchoredPos;
    private bool fired;

    void Awake()
    {
        remaining = autoStartSeconds;
        EnsureLabel();
    }

    void OnEnable()
    {
        remaining = autoStartSeconds;
        fired = false;
        EnsureLabel();
        if (countdownLabel != null) countdownLabel.gameObject.SetActive(true);
    }

    void EnsureLabel()
    {
        if (countdownLabel != null)
        {
            rect = countdownLabel.rectTransform;
            baseAnchoredPos = anchoredPosition;
            ApplyLayout();
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject go = new GameObject("HomeMenuCountdownLabel", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        countdownLabel = go.AddComponent<TextMeshProUGUI>();
        countdownLabel.alignment = TextAlignmentOptions.Center;
        countdownLabel.fontStyle = FontStyles.Bold;
        countdownLabel.enableWordWrapping = false;
        countdownLabel.color = colorFull;
        countdownLabel.fontSize = fontSize;
        countdownLabel.outlineWidth = 0.25f;
        countdownLabel.outlineColor = new Color(0f, 0f, 0f, 0.85f);
        countdownLabel.text = string.Format(format, Mathf.CeilToInt(remaining));

        rect = countdownLabel.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        baseAnchoredPos = anchoredPosition;
    }

    void ApplyLayout()
    {
        if (rect == null) return;
        rect.sizeDelta = sizeDelta;
        rect.anchoredPosition = anchoredPosition;
        baseAnchoredPos = anchoredPosition;
        if (countdownLabel != null)
        {
            countdownLabel.fontSize = fontSize;
        }
    }

    void Update()
    {
        if (fired) return;

        // Use unscaled time so the countdown ticks even if Time.timeScale is 0.
        remaining -= Time.unscaledDeltaTime;
        if (remaining < 0f) remaining = 0f;

        UpdateLabel();

        // Allow Space / W / gamepad confirm to skip the wait.
        bool skip = false;
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            var kb = UnityEngine.InputSystem.Keyboard.current;
            skip = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame
                || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame;
        }
        if (!skip && UnityEngine.InputSystem.Gamepad.current != null)
        {
            var gp = UnityEngine.InputSystem.Gamepad.current;
            skip = gp.buttonSouth.wasPressedThisFrame || gp.startButton.wasPressedThisFrame;
        }

        if (skip || remaining <= 0f) StartNow();
    }

    void UpdateLabel()
    {
        if (countdownLabel == null) return;

        int secondsLeft = Mathf.Max(0, Mathf.CeilToInt(remaining));
        countdownLabel.text = string.Format(format, secondsLeft);

        // Color: full -> mid (yellow) -> low (red) as time drains.
        Color c;
        if (remaining <= lowThreshold)
        {
            float t = Mathf.InverseLerp(0f, lowThreshold, remaining);
            c = Color.Lerp(colorLow, colorMid, t);
        }
        else if (remaining <= midThreshold)
        {
            float t = Mathf.InverseLerp(lowThreshold, midThreshold, remaining);
            c = Color.Lerp(colorMid, colorFull, t);
        }
        else
        {
            c = colorFull;
        }
        countdownLabel.color = c;

        // Pulse: scale wobble, faster when timer is low.
        float speed = remaining <= lowThreshold ? pulseSpeed * 2.4f : pulseSpeed;
        float amount = remaining <= lowThreshold ? pulseAmount * 1.6f : pulseAmount;
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * speed * Mathf.PI) * amount;
        if (rect != null) rect.localScale = new Vector3(pulse, pulse, 1f);

        // Shake when time is critical.
        if (rect != null)
        {
            if (remaining <= lowThreshold && remaining > 0f)
            {
                float jx = (Mathf.PerlinNoise(Time.unscaledTime * 18f, 0f) - 0.5f) * 2f;
                float jy = (Mathf.PerlinNoise(0f, Time.unscaledTime * 18f) - 0.5f) * 2f;
                rect.anchoredPosition = baseAnchoredPos + new Vector2(jx, jy) * lowShakeAmount;
            }
            else
            {
                rect.anchoredPosition = baseAnchoredPos;
            }
        }
    }

    void StartNow()
    {
        if (fired) return;
        fired = true;

        if (rect != null) rect.localScale = Vector3.one;
        if (countdownLabel != null) countdownLabel.gameObject.SetActive(false);

        HomeScreen home = FindAnyObjectByType<HomeScreen>();
        if (home != null) home.StartGame();
        else
        {
            Time.timeScale = 1f;
            if (GameManager.instance != null) GameManager.instance.play = true;
        }

        // Also disable any leftover PressKeyToPlay on the player so it doesn't
        // double-trigger (it does the same thing, but only when the player is active).
        var presses = FindObjectsByType<PressKeyToPlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in presses) if (p != null) p.enabled = false;
    }
}
