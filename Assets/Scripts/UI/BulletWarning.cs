using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BulletWarning : MonoBehaviour
{
    [Tooltip("Optional — caution sign / warning image. Hidden at runtime; kept for backward compatibility.")]
    public Image image;

    [Tooltip("Optional — TMP text. Auto-finds on this GameObject or children.")]
    public TMP_Text tmpText;

    [Tooltip("Optional — legacy Text. Auto-finds on this GameObject or children.")]
    public Text legacyText;

    [Tooltip("Optional — background panel behind the WARNING text (small rectangle).")]
    public Image background;

    [Header("Text")]
    public string warningText = "WARNING";

    [Header("Center position")]
    [Tooltip("Anchored position where the warning box sits (centered by default).")]
    public Vector2 centerPosition = Vector2.zero;

    [Header("Shake")]
    public float shakeAmount = 4f;
    public float shakeSpeed = 28f;

    [Header("Color pulse (applied to text + background)")]
    public Color textBaseColor = Color.white;
    public Color textPulseColor = new Color(1f, 0.15f, 0.15f, 1f);
    public Color backgroundBaseColor = new Color(0f, 0f, 0f, 0.55f);
    public Color backgroundPulseColor = new Color(0.6f, 0f, 0f, 0.85f);
    public float pulseSpeed = 8f;

    [Header("Scale pulse")]
    public float scaleMin = 0.95f;
    public float scaleMax = 1.1f;

    [Header("Flicker")]
    [Tooltip("How often the text briefly blinks off, in seconds. 0 disables.")]
    public float flickerInterval = 0.35f;
    [Tooltip("How long each flicker-off lasts.")]
    public float flickerDuration = 0.05f;

    private RectTransform rect;
    private CanvasGroup group;
    private Coroutine active;

    void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (tmpText == null) tmpText = GetComponentInChildren<TMP_Text>(true);
        if (legacyText == null) legacyText = GetComponentInChildren<Text>(true);

        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();

        if (image != null) image.enabled = false;

        Hide();
    }

    public void Show(float duration, bool fromLeft)
    {
        Show(duration);
    }

    public void Show(float duration)
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (group == null) group = GetComponent<CanvasGroup>();
        if (active != null) StopCoroutine(active);

        if (image != null) image.enabled = false;

        if (tmpText != null) tmpText.text = warningText;
        if (legacyText != null) legacyText.text = warningText;

        rect.anchoredPosition = centerPosition;
        rect.localScale = Vector3.one;

        group.alpha = 1f;
        active = StartCoroutine(Run(duration));
    }

    public void Hide()
    {
        if (active != null) StopCoroutine(active);
        active = null;
        if (group == null) group = GetComponent<CanvasGroup>();
        if (group != null) group.alpha = 0f;
    }

    IEnumerator Run(float duration)
    {
        float t = 0f;
        float nextFlickerAt = flickerInterval;
        bool flickerOff = false;
        float flickerOffUntil = 0f;

        while (t < duration)
        {
            float dx = (Mathf.PerlinNoise(t * shakeSpeed, 0f) - 0.5f) * 2f * shakeAmount;
            float dy = (Mathf.PerlinNoise(0f, t * shakeSpeed) - 0.5f) * 2f * shakeAmount;
            rect.anchoredPosition = centerPosition + new Vector2(dx, dy);

            float pulse = (Mathf.Sin(t * pulseSpeed) + 1f) * 0.5f;

            Color textTint = Color.Lerp(textBaseColor, textPulseColor, pulse);
            Color bgTint = Color.Lerp(backgroundBaseColor, backgroundPulseColor, pulse);

            float scale = Mathf.Lerp(scaleMin, scaleMax, pulse);
            rect.localScale = new Vector3(scale, scale, 1f);

            if (flickerInterval > 0f)
            {
                if (!flickerOff && t >= nextFlickerAt)
                {
                    flickerOff = true;
                    flickerOffUntil = t + flickerDuration;
                }
                if (flickerOff && t >= flickerOffUntil)
                {
                    flickerOff = false;
                    nextFlickerAt = t + flickerInterval;
                }
            }

            if (flickerOff)
            {
                textTint.a = 0f;
            }

            if (tmpText != null) tmpText.color = textTint;
            if (legacyText != null) legacyText.color = textTint;
            if (background != null) background.color = bgTint;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.anchoredPosition = centerPosition;
        rect.localScale = Vector3.one;
        group.alpha = 0f;
        active = null;
    }
}
