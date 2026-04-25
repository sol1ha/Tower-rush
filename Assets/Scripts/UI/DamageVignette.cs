using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance;

    [Tooltip("The full-screen UI Image with the red splatter sprite.")]
    public Image image;

    [Header("Flash")]
    [Tooltip("Peak alpha when a hit lands.")]
    [Range(0f, 1f)] public float flashAlpha = 0.85f;
    [Tooltip("How fast the flash fades back out (seconds).")]
    public float flashDuration = 0.6f;

    [Header("Shake")]
    [Tooltip("Max pixel shake offset.")]
    public float shakeAmount = 18f;
    [Tooltip("How long the shake lasts.")]
    public float shakeDuration = 0.4f;
    public float shakeSpeed = 35f;

    [Header("Low-health pulse")]
    [Tooltip("When alive and health is this or lower, a constant pulse is shown.")]
    public int lowHealthThreshold = 1;
    [Tooltip("Pulse alpha range when low health is active.")]
    public float lowHealthMinAlpha = 0.15f;
    public float lowHealthMaxAlpha = 0.55f;
    public float lowHealthPulseSpeed = 3f;

    private RectTransform rect;
    private Vector2 basePos;
    private Coroutine flashRoutine;
    private Coroutine shakeRoutine;
    private bool initialized;

    void Awake()
    {
        Instance = this;
        if (image == null) image = GetComponent<Image>();
        EnsureInit();
        SetAlpha(0f);
    }

    void EnsureInit()
    {
        if (initialized || image == null) return;
        rect = image.rectTransform;
        basePos = rect.anchoredPosition;
        initialized = true;
    }

    void Update()
    {
        if (image == null) return;

        if (flashRoutine != null) return;

        if (PlayerHealth.Instance != null)
        {
            int hp = PlayerHealth.Instance.GetHealth();
            if (hp > 0 && hp <= lowHealthThreshold)
            {
                float t = (Mathf.Sin(Time.unscaledTime * lowHealthPulseSpeed) + 1f) * 0.5f;
                SetAlpha(Mathf.Lerp(lowHealthMinAlpha, lowHealthMaxAlpha, t));
            }
            else if (shakeRoutine == null)
            {
                SetAlpha(0f);
            }
        }
    }

    public void PlayHit()
    {
        EnsureInit();
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash());

        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(Shake());
    }

    IEnumerator Flash()
    {
        float t = 0f;
        while (t < flashDuration)
        {
            float a = Mathf.Lerp(flashAlpha, 0f, t / flashDuration);
            SetAlpha(a);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        SetAlpha(0f);
        flashRoutine = null;
    }

    IEnumerator Shake()
    {
        float t = 0f;
        while (t < shakeDuration)
        {
            float fall = 1f - (t / shakeDuration);
            float dx = (Mathf.PerlinNoise(t * shakeSpeed, 0f) - 0.5f) * 2f * shakeAmount * fall;
            float dy = (Mathf.PerlinNoise(0f, t * shakeSpeed) - 0.5f) * 2f * shakeAmount * fall;
            rect.anchoredPosition = basePos + new Vector2(dx, dy);
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        rect.anchoredPosition = basePos;
        shakeRoutine = null;
    }

    void SetAlpha(float a)
    {
        if (image == null) return;
        Color c = image.color;
        c.a = Mathf.Clamp01(a);
        image.color = c;
    }
}
