using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Self-building leaderboard UI. Creates all UI elements automatically.
///
/// Unity Setup (EASY — only 2 steps):
/// 1. Create an empty GameObject → name it "LeaderboardSystem"
/// 2. Add components: LeaderboardManager + LeaderboardUI
/// 3. Drag your Canvas into the "Canvas" field
/// That's it! Everything else builds itself.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance;

    [Header("Just assign your Canvas")]
    public Canvas canvas;

    private GameObject leaderboardPanel;
    private List<Text[]> rows = new List<Text[]>(); // Each row: rank, icon, name, score, stars

    private static readonly string[] MEDALS = { "1st", "2nd", "3rd" };
    private static readonly Dictionary<string, string> FRUIT_DISPLAY = new Dictionary<string, string>()
    {
        {"apple", "A"}, {"cherry", "C"}, {"grape", "G"},
        {"lemon", "L"}, {"orange", "O"}, {"peach", "P"},
        {"pear", "R"}, {"strawberry", "S"}, {"watermelon", "W"},
        {"banana", "B"}
    };

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
    }

    void Start()
    {
        // PlayerKillLimit.PlayerKill += OnPlayerDeath; // Replaced by InGameTransaction flow

        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        BuildLeaderboardPanel();
    }

    void OnDestroy()
    {
        // PlayerKillLimit.PlayerKill -= OnPlayerDeath;
    }

    // ========== AUTO-BUILD UI ==========

    void BuildLeaderboardPanel()
    {
        // Main panel
        leaderboardPanel = CreatePanel("LeaderboardPanel", canvas.transform);
        Image bg = leaderboardPanel.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);

        // Title
        CreateText("LEADERBOARD", leaderboardPanel.transform,
            new Vector2(0, 200), 32, FontStyle.Bold, new Color(1f, 0.84f, 0f));

        // Column headers
        float headerY = 160;
        CreateText("RANK", leaderboardPanel.transform, new Vector2(-200, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("", leaderboardPanel.transform, new Vector2(-130, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("PLAYER", leaderboardPanel.transform, new Vector2(-20, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("SCORE", leaderboardPanel.transform, new Vector2(130, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("STARS", leaderboardPanel.transform, new Vector2(220, headerY), 14, FontStyle.Bold, Color.gray);

        // 10 rows
        for (int i = 0; i < 10; i++)
        {
            float y = 120 - i * 35;
            Text[] row = new Text[5];
            row[0] = CreateText("", leaderboardPanel.transform, new Vector2(-200, y), 18, FontStyle.Normal, Color.white); // rank
            row[1] = CreateText("", leaderboardPanel.transform, new Vector2(-130, y), 18, FontStyle.Normal, Color.white); // icon
            row[2] = CreateText("", leaderboardPanel.transform, new Vector2(-20, y), 18, FontStyle.Normal, Color.white);  // name
            row[3] = CreateText("", leaderboardPanel.transform, new Vector2(130, y), 18, FontStyle.Normal, Color.white);  // score
            row[4] = CreateText("", leaderboardPanel.transform, new Vector2(220, y), 18, FontStyle.Normal, new Color(1f, 0.84f, 0f)); // stars
            rows.Add(row);
        }

        // "Press any key to restart" at bottom
        CreateText("Press any key to restart", leaderboardPanel.transform,
            new Vector2(0, -230), 16, FontStyle.Italic, Color.gray);

        leaderboardPanel.SetActive(false);
    }

    // ========== HELPERS ==========

    GameObject CreatePanel(string name, Transform parent)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        panel.AddComponent<Image>();
        return panel;
    }

    Text CreateText(string content, Transform parent, Vector2 position, int size, FontStyle style, Color color)
    {
        GameObject obj = new GameObject("Text_" + content);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 30);

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.text = content;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    // ========== GAME LOGIC ==========

    void OnPlayerDeath(object sender, System.EventArgs e)
    {
        int score = ActualScoreDisplay.CurrentScore;
        Debug.Log("LEADERBOARD: Player died! Score = " + score);
        HighScoreSet.SetHighscore(score);

        if (LeaderboardManager.Instance == null)
        {
            Debug.Log("LEADERBOARD: LeaderboardManager.Instance is NULL!");
            return;
        }

        if (score < LeaderboardManager.Instance.GetMinScoreToQualify())
        {
            Debug.Log("LEADERBOARD: Score " + score + " too low (need " + LeaderboardManager.Instance.GetMinScoreToQualify() + ")");
            return;
        }

        string playerName = "amy_susu2"; // Default name
        LeaderboardManager.Instance.SetCurrentPlayer(playerName);
        LeaderboardManager.Instance.TryAddScore(playerName, score);
        ShowLeaderboard();
    }

    void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);

        List<LeaderboardManager.LeaderboardEntry> entries = LeaderboardManager.Instance.GetEntries();
        string currentPlayer = LeaderboardManager.Instance.GetSavedPlayerName();

        for (int i = 0; i < 10; i++)
        {
            if (i < entries.Count)
            {
                var entry = entries[i];
                bool isYou = entry.playerName == currentPlayer;
                Color rowColor = isYou ? Color.yellow : Color.white;

                // Rank
                rows[i][0].text = i < 3 ? MEDALS[i] : (i + 1).ToString();
                rows[i][0].color = i == 0 ? new Color(1f, 0.84f, 0f) :
                                   i == 1 ? new Color(0.75f, 0.75f, 0.75f) :
                                   i == 2 ? new Color(0.8f, 0.5f, 0.2f) : rowColor;

                // Icon
                string display;
                FRUIT_DISPLAY.TryGetValue(entry.icon, out display);
                rows[i][1].text = "[" + (display ?? "?") + "]";
                rows[i][1].color = rowColor;

                // Name
                rows[i][2].text = entry.playerName;
                rows[i][2].color = rowColor;
                rows[i][2].fontStyle = isYou ? FontStyle.Bold : FontStyle.Normal;

                // Score
                rows[i][3].text = entry.score.ToString();
                rows[i][3].color = rowColor;

                // Stars
                string stars = "";
                for (int s = 0; s < entry.stars; s++) stars += "*";
                rows[i][4].text = stars;
            }
            else
            {
                for (int j = 0; j < 5; j++)
                    rows[i][j].text = "";
            }
        }
    }

    public void Hide()
    {
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }
}
