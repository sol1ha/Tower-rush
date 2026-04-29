using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    [Header("Home page UI")]
    [Tooltip("Root GameObject of your 'Jump or Die' home page (background + Play button).")]
    public GameObject homePanel;

    [Tooltip("The Play button. When clicked, StartGame() is called.")]
    public Button playButton;

    [Header("Things to hide until Play is pressed")]
    [Tooltip("Drag every gameplay GameObject here: player, platforms parent, HUD, bullet spawner, laser, etc.")]
    public List<GameObject> hideUntilPlay = new List<GameObject>();

    [Tooltip("If true, Time.timeScale is 0 on the home page so nothing ticks.")]
    public bool freezeTimeOnHome = true;

    [Header("Music")]
    [Tooltip("Music that plays on the home/menu screen.")]
    public BackgroundMusic menuMusic;

    [Tooltip("Music that plays during gameplay (after Play is pressed).")]
    public BackgroundMusic gameMusic;

    [Tooltip("Seconds to crossfade between menu and game music.")]
    public float musicFadeSeconds = 0.6f;

    // ---------- Designed countdown built into HomeScreen ----------
    [Header("Countdown — drag YOUR TMP text here (optional)")]
    [Tooltip("Drag the TMP text you created in Unity. If left empty AND useSegmentClock is OFF, a basic TMP label is auto-created instead.")]
    public TMP_Text countdownLabel;
    [Tooltip("Seconds before the menu auto-starts.")]
    public float autoStartSeconds = 30f;
    [Tooltip("Show the timer as MM:SS (e.g. 00:30) instead of as plain seconds.")]
    public bool digitalClockFormat = true;
    [Tooltip("Format string used when 'digitalClockFormat' is OFF. {0} = seconds remaining.")]
    public string countdownFormat = "{0}s";

    [Header("Digital 7-segment clock (overrides TMP when ON)")]
    [Tooltip("If true, builds a real 7-segment LCD clock from rectangle Images — looks like a digital clock without needing a special font.")]
    public bool useSegmentClock = true;
    [Tooltip("Position of the segment clock on the canvas (relative to centre).")]
    public Vector2 segmentClockPosition = new Vector2(360f, -40f);
    [Tooltip("Width of each segment digit (in canvas units).")]
    public float segmentDigitWidth = 56f;
    [Tooltip("Height of each segment digit.")]
    public float segmentDigitHeight = 96f;
    [Tooltip("How thick each segment bar is.")]
    public float segmentThickness = 14f;
    [Tooltip("Italic-style slant in degrees (0 = upright, 6-10 = classic LCD).")]
    public float segmentSlant = 6f;

    [Header("Auto-create TMP label (only if useSegmentClock is OFF and Countdown Label empty)")]
    public int autoLabelFontSize = 60;
    public Vector2 autoLabelAnchoredPosition = new Vector2(360f, -40f);
    public Vector2 autoLabelSizeDelta = new Vector2(360f, 110f);

    [Header("In-game elapsed-time text")]
    [Tooltip("If true, auto-spawns a simple MM:SS time label in a corner of the canvas during gameplay. Off by default since the user has their own TMP timer in the scene.")]
    public bool spawnPlayTimeText = false;
    public PlayTimeText.Corner playTimeTextCorner = PlayTimeText.Corner.BottomRight;

    [Header("Hazards")]
    [Tooltip("If true, auto-attaches a WindGustSpawner that periodically blows the player sideways at higher altitudes.")]
    public bool spawnWindGusts = true;
    private WindGustSpawner windSpawner;

    private SegmentDigitalClock segmentClock;
    private RectTransform segmentClockRect;
    private Vector2 segmentClockBasePos;
    private PlayTimeText playTimeText;

    [Header("Countdown design")]
    public bool useVertexGradient = true;
    [Range(0f, 1f)] public float gradientTopLighten = 0.35f;
    [Range(0f, 1f)] public float gradientBottomDarken = 0.30f;
    public Color outlineColor = new Color(0.04f, 0.05f, 0.10f, 1f);
    public float outlineMin = 0.18f;
    public float outlineMax = 0.26f;
    public float outlinePulseSpeed = 1.2f;
    public bool useDropShadow = true;
    public Color shadowColor = new Color(0f, 0f, 0f, 0.55f);
    public Vector2 shadowOffset = new Vector2(0.6f, -0.6f);
    [Range(0f, 1f)] public float shadowDilate = 0.2f;
    [Range(0f, 1f)] public float shadowSoftness = 0.45f;
    public float characterSpacing = 8f;

    [Header("Countdown colors (dark-red palette)")]
    public Color colorFull = new Color(0.58f, 0.08f, 0.08f, 1f); // dark red
    public Color colorMid  = new Color(0.75f, 0.14f, 0.14f, 1f); // mid red (warning)
    public Color colorLow  = new Color(0.95f, 0.22f, 0.22f, 1f); // bright red (alarm)
    public float lowThreshold = 5f;
    public float midThreshold = 15f;

    [Header("Countdown tick animation")]
    public float popPeakScale = 1.12f;
    public float popDuration = 0.35f;
    public float tickShakeAmount = 3.5f;
    public float tickShakeDuration = 0.22f;
    public float tickShakeLowBonus = 2.5f;

    [Header("Countdown entrance")]
    public float entranceDuration = 0.5f;
    public float entranceFromScale = 0.7f;

    private float countdownRemaining;
    private bool countdownFired;
    private RectTransform countdownRect;
    private Vector2 countdownBasePos;
    private int lastShownSecond = -1;
    private float popTimer = 999f;
    private float shakeTimer = 999f;
    private float entranceTimer;
    private bool styledOnce;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartGame);
            playButton.onClick.AddListener(StartGame);
        }

        countdownRemaining = autoStartSeconds;

        if (useSegmentClock)
        {
            BuildSegmentClock();
        }
        else
        {
            if (countdownLabel == null) AutoCreateCountdownLabel();
            if (countdownLabel != null)
            {
                countdownRect = countdownLabel.rectTransform;
                countdownBasePos = countdownRect.anchoredPosition;
                StyleCountdown();
            }
        }
    }

    void BuildSegmentClock()
    {
        Canvas canvas = null;
        if (homePanel != null) canvas = homePanel.GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform parent = homePanel != null ? homePanel.transform : (Transform)canvas.transform;

        var go = new GameObject("SegmentClock", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        segmentClockRect = (RectTransform)go.transform;
        segmentClockRect.anchorMin = new Vector2(0.5f, 0.5f);
        segmentClockRect.anchorMax = new Vector2(0.5f, 0.5f);
        segmentClockRect.pivot = new Vector2(0.5f, 0.5f);
        // Width: 4 digits + colon + spacing. Ballpark, the display centers itself.
        segmentClockRect.sizeDelta = new Vector2(segmentDigitWidth * 4 + 80f, segmentDigitHeight * 1.2f);
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

    void AutoCreateCountdownLabel()
    {
        Canvas canvas = null;
        if (homePanel != null) canvas = homePanel.GetComponentInParent<Canvas>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        Transform parent = homePanel != null ? homePanel.transform : (Transform)canvas.transform;

        GameObject go = new GameObject("CountdownLabel", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        tmp.enableWordWrapping = false;
        tmp.fontSize = autoLabelFontSize;
        tmp.text = FormatCountdownText(Mathf.CeilToInt(autoStartSeconds));

        // Anchor to the screen center (slightly above middle by default) so the
        // label sits visually around the centre of the home menu, not glued to
        // the top of the canvas.
        var rt = tmp.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = autoLabelSizeDelta;
        rt.anchoredPosition = autoLabelAnchoredPosition;

        countdownLabel = tmp;
    }

    string FormatCountdownText(int totalSeconds)
    {
        if (digitalClockFormat)
        {
            int mm = totalSeconds / 60;
            int ss = totalSeconds % 60;
            return mm.ToString("00") + ":" + ss.ToString("00");
        }
        return string.Format(countdownFormat, totalSeconds);
    }

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        Time.timeScale = 1f;

        if (homePanel != null) homePanel.SetActive(true);

        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(false);
        }

        if (GameManager.instance != null) GameManager.instance.play = false;

        // Reset and show the countdown when the menu reappears.
        countdownRemaining = autoStartSeconds;
        countdownFired = false;
        lastShownSecond = -1;
        popTimer = 999f;
        shakeTimer = 999f;
        entranceTimer = 0f;
        if (countdownLabel != null)
        {
            countdownLabel.gameObject.SetActive(true);
            StyleCountdown();
        }
        if (segmentClock != null)
        {
            segmentClock.gameObject.SetActive(true);
            segmentClock.SetTotalSeconds(Mathf.CeilToInt(autoStartSeconds));
        }

        // Hide the in-game play-time text while we're back on the home menu.
        if (playTimeText != null) playTimeText.Hide();

        try { if (gameMusic != null) gameMusic.Stop(musicFadeSeconds); } catch { }
        try { if (menuMusic != null) menuMusic.Play(); } catch { }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (homePanel != null) homePanel.SetActive(false);

        // Hide the countdown label / segment clock as we leave the menu.
        countdownFired = true;
        if (countdownLabel != null)
        {
            if (countdownRect != null)
            {
                countdownRect.localScale = Vector3.one;
                countdownRect.anchoredPosition = countdownBasePos;
                countdownRect.localRotation = Quaternion.identity;
            }
            countdownLabel.gameObject.SetActive(false);
        }
        if (segmentClock != null)
        {
            segmentClockRect.localScale = Vector3.one;
            segmentClockRect.anchoredPosition = segmentClockBasePos;
            segmentClockRect.localRotation = Quaternion.identity;
            segmentClock.gameObject.SetActive(false);
        }

        // Activate gameplay objects FIRST so GameManager (often listed in
        // hideUntilPlay) gets its Awake run and registers GameManager.instance.
        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(true);
        }

        // Now set play=true AFTER GameManager has had a chance to Awake.
        // GameManager.Awake() forces play=false, so this must come after the loop.
        if (GameManager.instance != null) GameManager.instance.play = true;
        else GameManager.StartGame();

        // Auto-spawned play-time text is disabled (the user has their own
        // TMP timer in the scene). If they ever flip spawnPlayTimeText back
        // on, we'll create a host GameObject and let PlayTimeText drive
        // whatever label is wired up there.
        if (spawnPlayTimeText)
        {
            if (playTimeText == null)
            {
                var go = new GameObject("PlayTimeTextHost");
                playTimeText = go.AddComponent<PlayTimeText>();
                playTimeText.anchorCorner = playTimeTextCorner;
            }
            playTimeText.ResetTime();
            playTimeText.Show();
        }

        // Wind-gust hazard host — telegraphs a banner then pushes the player
        // sideways for a few seconds. Activates above its own height threshold.
        if (spawnWindGusts && windSpawner == null)
        {
            var wsGo = new GameObject("WindGustSpawner");
            windSpawner = wsGo.AddComponent<WindGustSpawner>();
        }

        try { if (menuMusic != null) menuMusic.Stop(musicFadeSeconds); } catch { }
        try { if (gameMusic != null) gameMusic.Play(); } catch { }
    }

    void Update()
    {
        if (countdownFired) return;
        if (countdownLabel == null && segmentClock == null) return;
        if (homePanel != null && !homePanel.activeInHierarchy) return;

        float dt = Time.unscaledDeltaTime;
        countdownRemaining -= dt;
        if (countdownRemaining < 0f) countdownRemaining = 0f;
        popTimer += dt;
        shakeTimer += dt;
        entranceTimer += dt;

        int sec = Mathf.Max(0, Mathf.CeilToInt(countdownRemaining));
        if (sec != lastShownSecond)
        {
            lastShownSecond = sec;
            popTimer = 0f;
            shakeTimer = 0f;
        }

        UpdateCountdownVisuals();

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

        if (skip || countdownRemaining <= 0f) StartGame();
    }

    void StyleCountdown()
    {
        if (countdownLabel == null) return;

        countdownLabel.characterSpacing = characterSpacing;
        countdownLabel.outlineColor = outlineColor;
        countdownLabel.outlineWidth = outlineMin;

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

        styledOnce = true;
    }

    void UpdateCountdownVisuals()
    {
        // ---- color drift (used by both modes) ----
        Color baseColor;
        if (countdownRemaining <= lowThreshold)
        {
            float t = Mathf.InverseLerp(0f, lowThreshold, countdownRemaining);
            baseColor = Color.Lerp(colorLow, colorMid, t);
        }
        else if (countdownRemaining <= midThreshold)
        {
            float t = Mathf.InverseLerp(lowThreshold, midThreshold, countdownRemaining);
            baseColor = Color.Lerp(colorMid, colorFull, t);
        }
        else
        {
            baseColor = colorFull;
        }

        // Tick pop / shake values (shared)
        float entrance = Mathf.Clamp01(entranceTimer / Mathf.Max(0.0001f, entranceDuration));
        float entranceScale = Mathf.LerpUnclamped(entranceFromScale, 1f, EaseOutBack(entrance));
        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));
        float finalScale = entranceScale * popScale;
        float shakeProgress = Mathf.Clamp01(shakeTimer / Mathf.Max(0.0001f, tickShakeDuration));
        float shakeStrength = 1f - shakeProgress;
        float amp = tickShakeAmount + (countdownRemaining <= lowThreshold ? tickShakeLowBonus : 0f);
        Vector2 shake = Vector2.zero;
        if (shakeStrength > 0f)
        {
            float n1 = Mathf.PerlinNoise(Time.unscaledTime * 40f, 0f) - 0.5f;
            float n2 = Mathf.PerlinNoise(0f, Time.unscaledTime * 40f) - 0.5f;
            shake = new Vector2(n1, n2) * 2f * amp * shakeStrength;
        }

        // ---- segment clock path ----
        if (segmentClock != null && segmentClockRect != null)
        {
            segmentClock.SetTotalSeconds(lastShownSecond);
            segmentClock.SetColor(baseColor);
            // Blink the colon every other second.
            segmentClock.SetColonBlink(((int)(Time.unscaledTime * 2f)) % 2 == 0);
            segmentClockRect.localScale = new Vector3(finalScale, finalScale, 1f);
            segmentClockRect.localRotation = Quaternion.identity;
            segmentClockRect.anchoredPosition = segmentClockBasePos + shake;
            return;
        }

        // ---- TMP label path ----
        if (countdownLabel == null) return;
        if (!styledOnce) StyleCountdown();

        countdownLabel.text = FormatCountdownText(lastShownSecond);

        if (useVertexGradient)
        {
            Color top = Color.Lerp(baseColor, Color.white, gradientTopLighten);
            Color bottom = Color.Lerp(baseColor, Color.black, gradientBottomDarken);
            countdownLabel.enableVertexGradient = true;
            countdownLabel.colorGradient = new VertexGradient(top, top, bottom, bottom);
            countdownLabel.color = Color.white;
        }
        else
        {
            countdownLabel.enableVertexGradient = false;
            countdownLabel.color = baseColor;
        }

        float pulse = Mathf.Sin(Time.unscaledTime * outlinePulseSpeed * Mathf.PI) * 0.5f + 0.5f;
        countdownLabel.outlineWidth = Mathf.Lerp(outlineMin, outlineMax, pulse);

        if (countdownRect == null) return;
        countdownRect.localScale = new Vector3(finalScale, finalScale, 1f);
        countdownRect.localRotation = Quaternion.identity;
        countdownRect.anchoredPosition = countdownBasePos + shake;
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
