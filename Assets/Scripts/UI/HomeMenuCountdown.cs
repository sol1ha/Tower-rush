using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visible, designed countdown shown on the home menu. If the
/// <see cref="countdownLabel"/> is assigned in the inspector (the TMP text you
/// created in Unity), the script styles and animates THAT label. Otherwise it
/// auto-creates a styled label inside the first canvas it finds.
///
/// Design:
///   - Vertex gradient (lighter on top, darker on bottom) for a shiny "metallic" feel
///   - Crisp dark outline for contrast over any background
///   - Soft drop-shadow via TMP underlay
///   - Wide character spacing for a more deliberate, designed feel
/// Effects (kept subtle, no constant wiggle):
///   - Quick scale pop on every second tick (1.12x, ease-out-back, ~0.35s)
///   - Brief shake right after each tick that decays in ~0.2s
///   - Color drift over time (calm -> warm -> alarm) applied to the gradient
///   - Outline width pulses very gently for a slow glow
/// </summary>
public class HomeMenuCountdown : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds before the game auto-starts.")]
    public float autoStartSeconds = 30f;

    [Header("Reference")]
    [Tooltip("Drag your countdown TMP text here. If left empty, one will be auto-created in the first canvas.")]
    public TMP_Text countdownLabel;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string format = "Starting in {0}s";

    [Header("Sizing (only used when auto-creating the label)")]
    public int fontSize = 96;
    public Vector2 anchoredPosition = new Vector2(0f, -180f);
    public Vector2 sizeDelta = new Vector2(900f, 200f);

    [Header("Design — gradient")]
    public bool useVertexGradient = true;
    [Tooltip("How much lighter the top of the letters is.")]
    [Range(0f, 1f)] public float gradientTopLighten = 0.35f;
    [Tooltip("How much darker the bottom of the letters is.")]
    [Range(0f, 1f)] public float gradientBottomDarken = 0.30f;

    [Header("Design — outline")]
    public Color outlineColor = new Color(0.04f, 0.05f, 0.10f, 1f);
    public float outlineMin = 0.18f;
    public float outlineMax = 0.26f;
    public float outlinePulseSpeed = 1.2f;

    [Header("Design — drop shadow (TMP underlay)")]
    public bool useDropShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.55f);
    public Vector2 shadowOffset = new Vector2(0.6f, -0.6f);
    [Range(0f, 1f)] public float shadowDilate = 0.2f;
    [Range(0f, 1f)] public float shadowSoftness = 0.45f;

    [Header("Design — character spacing")]
    public float characterSpacing = 8f;

    [Header("Colors over time")]
    public Color colorFull = new Color(0.55f, 0.85f, 1f, 1f);   // calm cyan-blue
    public Color colorMid = new Color(1f, 0.78f, 0.30f, 1f);    // warm gold
    public Color colorLow = new Color(1f, 0.32f, 0.32f, 1f);    // alarm red
    [Tooltip("Below this many seconds, switch to the low color.")]
    public float lowThreshold = 5f;
    [Tooltip("Below this many seconds, blend toward the mid color.")]
    public float midThreshold = 15f;

    [Header("Tick animation")]
    [Tooltip("Peak scale on each second tick. Keep it small for a 'tap' feel.")]
    public float popPeakScale = 1.12f;
    [Tooltip("Seconds the pop takes to settle.")]
    public float popDuration = 0.35f;
    [Tooltip("Pixel amplitude of the brief shake right after each tick.")]
    public float tickShakeAmount = 3.5f;
    [Tooltip("Seconds the tick shake takes to decay to zero.")]
    public float tickShakeDuration = 0.22f;
    [Tooltip("Extra shake amplitude in the low-time phase.")]
    public float tickShakeLowBonus = 2.5f;

    [Header("Entrance")]
    public float entranceDuration = 0.5f;
    public float entranceFromScale = 0.7f;

    private float remaining;
    private RectTransform rect;
    private Vector2 baseAnchoredPos;
    private bool fired;

    private int lastShownSecond = -1;
    private float popTimer = 999f;
    private float shakeTimer = 999f;
    private float entranceTimer;

    private bool styled;

    void Awake()
    {
        remaining = autoStartSeconds;
        EnsureLabel();
        StyleLabel();
    }

    void OnEnable()
    {
        remaining = autoStartSeconds;
        fired = false;
        lastShownSecond = -1;
        popTimer = 999f;
        shakeTimer = 999f;
        entranceTimer = 0f;
        EnsureLabel();
        StyleLabel();
        if (countdownLabel != null) countdownLabel.gameObject.SetActive(true);
    }

    void EnsureLabel()
    {
        if (countdownLabel != null)
        {
            rect = countdownLabel.rectTransform;
            baseAnchoredPos = rect.anchoredPosition;
            return;
        }
        // No label assigned — script does nothing. Drag your TMP text into
        // the 'Countdown Label' field on this component in the inspector.
    }

    void StyleLabel()
    {
        if (countdownLabel == null) return;

        // Wider letter spacing for a deliberate, designed feel.
        countdownLabel.characterSpacing = characterSpacing;

        // Outline.
        countdownLabel.outlineColor = outlineColor;
        countdownLabel.outlineWidth = outlineMin;

        // Drop shadow via TMP material underlay (an instanced material so we
        // don't mutate the shared font asset).
        if (useDropShadow && countdownLabel.fontMaterial != null)
        {
            Material mat = countdownLabel.fontMaterial;
            mat.EnableKeyword("UNDERLAY_ON");
            if (mat.HasProperty("_UnderlayColor")) mat.SetColor("_UnderlayColor", shadowColor);
            if (mat.HasProperty("_UnderlayOffsetX")) mat.SetFloat("_UnderlayOffsetX", shadowOffset.x);
            if (mat.HasProperty("_UnderlayOffsetY")) mat.SetFloat("_UnderlayOffsetY", shadowOffset.y);
            if (mat.HasProperty("_UnderlayDilate")) mat.SetFloat("_UnderlayDilate", shadowDilate);
            if (mat.HasProperty("_UnderlaySoftness")) mat.SetFloat("_UnderlaySoftness", shadowSoftness);
        }

        styled = true;
    }

    void Update()
    {
        if (fired) return;

        float dt = Time.unscaledDeltaTime;
        remaining -= dt;
        if (remaining < 0f) remaining = 0f;
        popTimer += dt;
        shakeTimer += dt;
        entranceTimer += dt;

        int sec = Mathf.Max(0, Mathf.CeilToInt(remaining));
        if (sec != lastShownSecond)
        {
            lastShownSecond = sec;
            popTimer = 0f;
            shakeTimer = 0f;
        }

        UpdateLabel();

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
        if (!styled) StyleLabel();

        countdownLabel.text = string.Format(format, lastShownSecond);

        // ---- color drift ----
        Color baseColor;
        if (remaining <= lowThreshold)
        {
            float t = Mathf.InverseLerp(0f, lowThreshold, remaining);
            baseColor = Color.Lerp(colorLow, colorMid, t);
        }
        else if (remaining <= midThreshold)
        {
            float t = Mathf.InverseLerp(lowThreshold, midThreshold, remaining);
            baseColor = Color.Lerp(colorMid, colorFull, t);
        }
        else
        {
            baseColor = colorFull;
        }

        if (useVertexGradient)
        {
            Color top = Color.Lerp(baseColor, Color.white, gradientTopLighten);
            Color bottom = Color.Lerp(baseColor, Color.black, gradientBottomDarken);
            countdownLabel.enableVertexGradient = true;
            countdownLabel.colorGradient = new VertexGradient(top, top, bottom, bottom);
            countdownLabel.color = Color.white; // gradient fully drives color when enabled
        }
        else
        {
            countdownLabel.enableVertexGradient = false;
            countdownLabel.color = baseColor;
        }

        // ---- gentle outline glow pulse (slow, not wiggly) ----
        float pulse = Mathf.Sin(Time.unscaledTime * outlinePulseSpeed * Mathf.PI) * 0.5f + 0.5f;
        countdownLabel.outlineWidth = Mathf.Lerp(outlineMin, outlineMax, pulse);

        // ---- scale: entrance + tick pop (no breath, no continuous wobble) ----
        float entrance = Mathf.Clamp01(entranceTimer / Mathf.Max(0.0001f, entranceDuration));
        float entranceScale = Mathf.LerpUnclamped(entranceFromScale, 1f, EaseOutBack(entrance));

        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));

        float finalScale = entranceScale * popScale;
        if (rect != null)
        {
            rect.localScale = new Vector3(finalScale, finalScale, 1f);
            rect.localRotation = Quaternion.identity;

            // ---- brief shake right after each tick (decays fast) ----
            float shakeProgress = Mathf.Clamp01(shakeTimer / Mathf.Max(0.0001f, tickShakeDuration));
            float shakeStrength = 1f - shakeProgress;
            float amp = tickShakeAmount + (remaining <= lowThreshold ? tickShakeLowBonus : 0f);
            Vector2 shake = Vector2.zero;
            if (shakeStrength > 0f)
            {
                float n1 = Mathf.PerlinNoise(Time.unscaledTime * 40f, 0f) - 0.5f;
                float n2 = Mathf.PerlinNoise(0f, Time.unscaledTime * 40f) - 0.5f;
                shake = new Vector2(n1, n2) * 2f * amp * shakeStrength;
            }
            rect.anchoredPosition = baseAnchoredPos + shake;
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
            rect.anchoredPosition = baseAnchoredPos;
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
