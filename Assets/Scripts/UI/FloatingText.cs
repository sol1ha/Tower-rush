using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Spawns a small world-space text that floats upward and fades out, then
/// destroys itself. Used for coin combo "+1 x2!" popups.
/// </summary>
public class FloatingText : MonoBehaviour
{
    public float lifetime = 1.0f;
    public float floatDistance = 1.4f;
    public Color color = Color.white;

    Text text;
    Outline outline;
    RectTransform rect;
    float elapsed;

    public static void Spawn(Vector3 worldPos, string content, Color color)
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        // We need a screen-space text positioned to follow worldPos. Easiest:
        // spawn a worldspace canvas-less Text by attaching to a separate
        // canvas at the right position. Simpler: use the existing canvas in
        // overlay mode and convert worldPos to canvas coords.
        var go = new GameObject("FloatingText", typeof(RectTransform));
        go.transform.SetParent(canvas.transform, false);

        var ft = go.AddComponent<FloatingText>();
        ft.color = color;

        ft.text = go.AddComponent<Text>();
        ft.text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        ft.text.fontSize = 36;
        ft.text.fontStyle = FontStyle.Bold;
        ft.text.alignment = TextAnchor.MiddleCenter;
        ft.text.color = color;
        ft.text.text = content;
        ft.text.horizontalOverflow = HorizontalWrapMode.Overflow;
        ft.text.verticalOverflow = VerticalWrapMode.Overflow;

        ft.outline = go.AddComponent<Outline>();
        ft.outline.effectColor = new Color(0, 0, 0, 0.85f);
        ft.outline.effectDistance = new Vector2(2f, -2f);

        ft.rect = (RectTransform)go.transform;
        ft.rect.sizeDelta = new Vector2(220, 60);

        // Convert world position to canvas anchored position.
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 screen = cam.WorldToScreenPoint(worldPos);
            var canvasRect = canvas.transform as RectTransform;
            if (canvasRect != null)
            {
                Vector2 anchored;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screen, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam, out anchored);
                ft.rect.anchorMin = ft.rect.anchorMax = ft.rect.pivot = new Vector2(0.5f, 0.5f);
                ft.rect.anchoredPosition = anchored;
            }
        }
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        // Float upward
        if (rect != null)
        {
            Vector2 ap = rect.anchoredPosition;
            ap.y += floatDistance * 60f * Time.deltaTime; // scale by ~60 px/world-unit
            rect.anchoredPosition = ap;
            float scale = Mathf.LerpUnclamped(1.2f, 1f, EaseOutBack(Mathf.Clamp01(elapsed / 0.25f)));
            rect.localScale = new Vector3(scale, scale, 1f);
        }

        // Fade out in last 40%
        if (text != null)
        {
            float alpha = t < 0.6f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.6f) / 0.4f);
            Color c = color; c.a = alpha; text.color = c;
        }

        if (elapsed >= lifetime) Destroy(gameObject);
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
