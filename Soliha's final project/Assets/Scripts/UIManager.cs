using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [Header("HUD Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI notificationText;
    public TextMeshProUGUI damagePopupText;

    private float notificationClearTime;

    void Start()
    {
        if (damagePopupText != null)
            damagePopupText.gameObject.SetActive(false);
    }

    public void DisplayNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationClearTime = Time.time + 3f;
        }
    }

    // Shows a red shaking "-1 ❤" or "-0.5 ❤" popup for 0.5 seconds
    public void ShowDamagePopup(string text)
    {
        if (damagePopupText == null) return;

        StopCoroutine("DamagePopupRoutine");
        StartCoroutine(DamagePopupRoutine(text));
    }

    private IEnumerator DamagePopupRoutine(string text)
    {
        damagePopupText.gameObject.SetActive(true);
        damagePopupText.text = text;
        damagePopupText.color = Color.red;

        Vector3 originalPos = damagePopupText.rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            // Shake the text
            float shakeX = Random.Range(-5f, 5f);
            float shakeY = Random.Range(-5f, 5f);
            damagePopupText.rectTransform.localPosition = originalPos + new Vector3(shakeX, shakeY, 0);

            // Fade out over time
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            damagePopupText.color = new Color(1f, 0f, 0f, alpha);

            // Scale up slightly then shrink
            float scale = elapsed < duration * 0.3f
                ? Mathf.Lerp(1f, 1.5f, elapsed / (duration * 0.3f))
                : Mathf.Lerp(1.5f, 0.8f, (elapsed - duration * 0.3f) / (duration * 0.7f));
            damagePopupText.rectTransform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        damagePopupText.rectTransform.localPosition = originalPos;
        damagePopupText.rectTransform.localScale = Vector3.one;
        damagePopupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance == null) return;

        if (scoreText != null)
            scoreText.text = GameManager.Instance.score.ToString("D6");

        if (levelText != null)
            levelText.text = "LEVEL " + GameManager.Instance.currentLevel;

        if (multiplierText != null)
            multiplierText.text = "X" + GameManager.Instance.multiplier;

        // Build hearts display: full hearts + half heart
        if (livesText != null)
        {
            float lives = GameManager.Instance.currentLives;
            int fullHearts = Mathf.FloorToInt(lives);
            bool halfHeart = (lives - fullHearts) >= 0.5f;

            string hearts = new string('\u2764', fullHearts);   // ❤ full hearts
            if (halfHeart) hearts += "\u2665";                   // ♥ half heart (smaller)
            livesText.text = hearts;
        }

        if (notificationText != null && Time.time > notificationClearTime)
            notificationText.text = "";
    }
}
