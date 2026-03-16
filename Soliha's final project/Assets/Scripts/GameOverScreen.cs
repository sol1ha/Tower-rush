using UnityEngine;
using TMPro;

/// Attach to a UI Panel called "GameOverPanel" that is disabled by default.
/// GameManager.GameOver() calls Show() to display it.
///
/// In Unity setup:
/// 1. Create a Panel under your HUD Canvas → name it "GameOverPanel"
/// 2. Add child TextMeshPro: "GAME OVER" (large, center-top)
/// 3. Add child TextMeshPro: "SCORE: 000000" (medium, center)
/// 4. Add child TextMeshPro: "LEVEL: 1" (smaller, below score)
/// 5. Add child TextMeshPro: "Press SPACE to restart" (bottom)
/// 6. Disable the panel by default (uncheck the checkbox)
/// 7. Drag the texts into this script's Inspector fields
/// 8. Add this script to the GameOverPanel
public class GameOverScreen : MonoBehaviour
{
    public static GameOverScreen Instance;

    [Header("UI References")]
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI finalLevelText;
    public TextMeshProUGUI restartPromptText;
    public GameObject panel;

    [Header("Settings")]
    public float autoRestartTime = 30f;

    private bool isShowing = false;
    private float showTime;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        if (panel != null)
            panel.SetActive(false);
    }

    void Update()
    {
        if (!isShowing) return;

        // Restart on Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
            return;
        }

        // Auto-restart after timeout (Luxodd requirement: 30 seconds)
        if (Time.time - showTime > autoRestartTime)
        {
            Restart();
        }
    }

    public void Show(int finalScore)
    {
        isShowing = true;
        showTime = Time.time;

        if (panel != null)
            panel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "SCORE: " + finalScore.ToString("D6");

        if (finalLevelText != null && GameManager.Instance != null)
            finalLevelText.text = "LEVEL: " + GameManager.Instance.currentLevel;

        if (restartPromptText != null)
            restartPromptText.text = "Press SPACE to restart";

        // Also show leaderboard
        if (LeaderboardScreen.Instance != null)
            LeaderboardScreen.Instance.Show(finalScore);
    }

    void Restart()
    {
        isShowing = false;

        if (panel != null)
            panel.SetActive(false);

        if (LeaderboardScreen.Instance != null)
            LeaderboardScreen.Instance.Hide();

        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}
