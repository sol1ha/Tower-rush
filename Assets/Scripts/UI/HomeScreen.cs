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
    [Header("Countdown — drag YOUR TMP text here")]
    [Tooltip("Drag the TMP text you created in Unity. Leave empty to disable the countdown.")]
    public TMP_Text countdownLabel;
    [Tooltip("Format string. {0} is replaced with the integer seconds remaining.")]
    public string countdownFormat = "Starting in {0}s";
    [Tooltip("Seconds before the menu auto-starts.")]
    public float autoStartSeconds = 30f;

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

    [Header("Countdown colors")]
    public Color colorFull = new Color(0.55f, 0.85f, 1f, 1f);   // calm cyan-blue
    public Color colorMid = new Color(1f, 0.78f, 0.30f, 1f);    // warm gold
    public Color colorLow = new Color(1f, 0.32f, 0.32f, 1f);    // alarm red
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

        if (countdownLabel != null)
        {
            countdownRect = countdownLabel.rectTransform;
            countdownBasePos = countdownRect.anchoredPosition;
            countdownRemaining = autoStartSeconds;
            StyleCountdown();
        }
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
        if (countdownLabel != null)
        {
            countdownLabel.gameObject.SetActive(true);
            countdownRemaining = autoStartSeconds;
            countdownFired = false;
            lastShownSecond = -1;
            popTimer = 999f;
            shakeTimer = 999f;
            entranceTimer = 0f;
            StyleCountdown();
        }

        try { if (gameMusic != null) gameMusic.Stop(musicFadeSeconds); } catch { }
        try { if (menuMusic != null) menuMusic.Play(); } catch { }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (homePanel != null) homePanel.SetActive(false);

        // Hide the countdown label as we leave the menu.
        if (countdownLabel != null)
        {
            countdownFired = true;
            if (countdownRect != null)
            {
                countdownRect.localScale = Vector3.one;
                countdownRect.anchoredPosition = countdownBasePos;
                countdownRect.localRotation = Quaternion.identity;
            }
            countdownLabel.gameObject.SetActive(false);
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

        try { if (menuMusic != null) menuMusic.Stop(musicFadeSeconds); } catch { }
        try { if (gameMusic != null) gameMusic.Play(); } catch { }
    }

    void Update()
    {
        if (countdownLabel == null || countdownFired) return;
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
        if (!styledOnce) StyleCountdown();

        countdownLabel.text = string.Format(countdownFormat, lastShownSecond);

        // ---- color drift ----
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

        // ---- gentle outline glow pulse ----
        float pulse = Mathf.Sin(Time.unscaledTime * outlinePulseSpeed * Mathf.PI) * 0.5f + 0.5f;
        countdownLabel.outlineWidth = Mathf.Lerp(outlineMin, outlineMax, pulse);

        if (countdownRect == null) return;

        // ---- scale: entrance + tick pop ----
        float entrance = Mathf.Clamp01(entranceTimer / Mathf.Max(0.0001f, entranceDuration));
        float entranceScale = Mathf.LerpUnclamped(entranceFromScale, 1f, EaseOutBack(entrance));

        float popProgress = Mathf.Clamp01(popTimer / Mathf.Max(0.0001f, popDuration));
        float popScale = Mathf.LerpUnclamped(popPeakScale, 1f, EaseOutBack(popProgress));

        float finalScale = entranceScale * popScale;
        countdownRect.localScale = new Vector3(finalScale, finalScale, 1f);
        countdownRect.localRotation = Quaternion.identity;

        // ---- brief shake right after each tick (decays fast) ----
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
