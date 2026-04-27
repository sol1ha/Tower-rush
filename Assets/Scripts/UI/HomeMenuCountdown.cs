using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visible countdown shown on the home menu. Auto-creates a large, animated TMP
/// label inside the home panel's canvas and starts the game when the timer hits
/// zero. Also fires HomeScreen.StartGame() on Space / W / gamepad confirm.
///
/// Effects (timer-appropriate, not boring):
///   - Pop on every second tick (ease-out-back overshoot then settle)
///   - Per-character vertical wave (each letter bobs out of phase)
///   - Subtle rotation wobble + breathing scale
///   - Outline width pulse so the text "glows" on every beat
///   - Color drift: cool blue -> warm orange -> alarm red as time drains
///   - Smooth scale-in entrance (small to full size with overshoot)
///   - Stronger pop, faster wobble, hue shimmer in the last 5 seconds
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

    [Header("Colors")]
    public Color colorFull = new Color(0.55f, 0.85f, 1f, 1f);   // calm cyan-blue
    public Color colorMid = new Color(1f, 0.75f, 0.25f, 1f);    // warm orange
    public Color colorLow = new Color(1f, 0.30f, 0.30f, 1f);    // alarm red
    [Tooltip("Below this many seconds, switch to the low color and intensify effects.")]
    public float lowThreshold = 5f;
    [Tooltip("Below this many seconds, blend toward the mid color.")]
    public float midThreshold = 15f;

    [Header("Pop on tick (ease-out-back)")]
    [Tooltip("Peak scale right after each second tick.")]
    public float popPeakScale = 1.55f;
    [Tooltip("Seconds the pop animation takes to settle back to 1.")]
    public float popDuration = 0.45f;
    [Tooltip("Extra peak when the timer is in the low (red) range.")]
    public float popPeakLowBonus = 0.25f;

    [Header("Wobble & breath")]
    [Tooltip("Max ± Z rotation in degrees for the gentle wobble.")]
    public float wobbleAngle = 4f;
    [Tooltip("Wobble cycles per second.")]
    public float wobbleSpeed = 1.3f;
    [Tooltip("Breathing scale amplitude (added to pop).")]
    public float breathAmount = 0.04f;
    [Tooltip("Breathing cycles per second.")]
    public float breathSpeed = 1.1f;

    [Header("Letter wave")]
    [Tooltip("Vertical pixel amplitude of the per-character wave.")]
    public float waveAmplitude = 6f;
    [Tooltip("Wave cycles per second.")]
    public float waveSpeed = 2.6f;
    [Tooltip("Phase offset between adjacent characters (radians).")]
    public float wavePhasePerChar = 0.55f;

    [Header("Outline pulse")]
    public float outlineMin = 0.18f;
    public float outlineMax = 0.32f;
    public float outlineSpeed = 2f;

    [Header("Entrance")]
    [Tooltip("Seconds the scale-in entrance takes.")]
    public float entranceDuration = 0.55f;
    public float entranceFromScale = 0.2f;

    private float remaining;
    private RectTransform rect;
    private Vector2 baseAnchoredPos;
    private bool fired;

    // Pop / second-change tracking
    private int lastShownSecond = -1;
    private float popTimer = 999f;

    // Entrance tracking
    private float entranceTimer;

    void Awake()
    {
        remaining = autoStartSeconds;
        EnsureLabel();
    }

    void OnEnable()
    {
        remaining = autoStartSeconds;
        fired = false;
        lastShownSecond = -1;
        popTimer = 999f;
        entranceTimer = 0f;
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
        countdownLabel.outlineWidth = outlineMin;
        countdownLabel.outlineColor = new Color(0f, 0f, 0f, 0.9f);
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
        float dt = Time.unscaledDeltaTime;
        remaining -= dt;
        if (remaining < 0f) remaining = 0f;
        popTimer += dt;
        entranceTimer += dt;

        // Detect second-tick — re-trigger the pop animation.
        int sec = Mathf.Max(0, Mathf.CeilToInt(remaining));
        if (sec != lastShownSecond)
        {
            lastShownSecond = sec;
            popTimer = 0f;
        }

        UpdateLabel();

        // Allow Space / W / Enter / gamepad confirm to skip.
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

        countdownLabel.text = string.Format(format, lastShownSecond);

        // ----- Color: full -> mid -> low as time drains, with a hue shimmer near 0 -----
        Color c;
        if (remaining <= lowThreshold)
        {
            float t = Mathf.InverseLerp(0f, lowThreshold, remaining);
            c = Color.Lerp(colorLow, colorMid, t);
            // small hue shimmer in the alarm phase
            float shimmer = Mathf.Sin(Time.unscaledTime * 8f) * 0.06f;
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h = Mathf.Repeat(h + shimmer, 1f);
            c = Color.HSVToRGB(h, s, v);
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

        // ----- Outline pulse so the text "glows" -----
        float pulsePhase = Mathf.Sin(Time.unscaledTime * outlineSpeed * Mathf.PI) * 0.5f + 0.5f;
        countdownLabel.outlineWidth = Mathf.Lerp(outlineMin, outlineMax, pulsePhase);

        // ----- Scale: entrance -> breath -> pop combined -----
        float entrance = Mathf.Clamp01(entranceTimer / Mathf.Max(0.0001f, entranceDuration));
        float entranceScale = Mathf.LerpUnclamped(entranceFromScale, 1f, EaseOutBack(entrance));

        float breath = 1f + Mathf.Sin(Time.unscaledTime * breathSpeed * Mathf.PI * 2f) * breathAmount;

        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float peak = popPeakScale + (remaining <= lowThreshold ? popPeakLowBonus : 0f);
        // Start at peak, ease back to 1 with a gentle overshoot/settle.
        float popScale = Mathf.LerpUnclamped(peak, 1f, EaseOutBack(popProgress));

        float finalScale = entranceScale * breath * popScale;
        if (rect != null) rect.localScale = new Vector3(finalScale, finalScale, 1f);

        // ----- Wobble (Z rotation) + idle position -----
        if (rect != null)
        {
            float angleBase = wobbleAngle * (remaining <= lowThreshold ? 1.6f : 1f);
            float angle = Mathf.Sin(Time.unscaledTime * wobbleSpeed * Mathf.PI * 2f) * angleBase;
            // Add a punchy rotation kick on each pop (decays with popProgress).
            float popKick = Mathf.Sin(popProgress * Mathf.PI) * 6f * (1f - popProgress);
            rect.localRotation = Quaternion.Euler(0f, 0f, angle + popKick);

            rect.anchoredPosition = baseAnchoredPos;
        }

        // ----- Per-character wave (each letter bobs out of phase) -----
        ApplyLetterWave();
    }

    void ApplyLetterWave()
    {
        if (countdownLabel == null) return;
        countdownLabel.ForceMeshUpdate();
        var textInfo = countdownLabel.textInfo;
        if (textInfo == null) return;

        float waveBoost = remaining <= lowThreshold ? 1.6f : 1f;
        float speed = waveSpeed * waveBoost * Mathf.PI * 2f;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int matIdx = charInfo.materialReferenceIndex;
            int vIdx = charInfo.vertexIndex;
            var verts = textInfo.meshInfo[matIdx].vertices;

            float y = Mathf.Sin(Time.unscaledTime * speed + i * wavePhasePerChar) * waveAmplitude * waveBoost;
            Vector3 offset = new Vector3(0f, y, 0f);

            verts[vIdx + 0] += offset;
            verts[vIdx + 1] += offset;
            verts[vIdx + 2] += offset;
            verts[vIdx + 3] += offset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            countdownLabel.UpdateGeometry(meshInfo.mesh, i);
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

    void StartNow()
    {
        if (fired) return;
        fired = true;

        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
        }
        if (countdownLabel != null) countdownLabel.gameObject.SetActive(false);

        HomeScreen home = FindAnyObjectByType<HomeScreen>();
        if (home != null) home.StartGame();
        else
        {
            Time.timeScale = 1f;
            if (GameManager.instance != null) GameManager.instance.play = true;
        }

        var presses = FindObjectsByType<PressKeyToPlay>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var p in presses) if (p != null) p.enabled = false;
    }
}
