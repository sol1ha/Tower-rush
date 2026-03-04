using UnityEngine;
using TMPro; // Make sure you have TextMeshPro installed!

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Header("HUD Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI multiplierText;
    public TextMeshProUGUI notificationText;

    private float notificationClearTime;

    public void DisplayNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationClearTime = Time.time + 3f; // Clear after 3 seconds
        }
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

        if (notificationText != null && Time.time > notificationClearTime)
            notificationText.text = "";
    }
}
