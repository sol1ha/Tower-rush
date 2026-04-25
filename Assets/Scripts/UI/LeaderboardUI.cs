using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance;

    [Header("Just assign your Canvas")]
    public Canvas canvas;

    private GameObject leaderboardPanel;
    private List<Text[]> rows = new List<Text[]>();

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
        if (Instance != this) return;

        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        BuildLeaderboardPanel();
        PlayerKillLimit.PlayerKill += OnPlayerDeath;
    }

    void OnDestroy()
    {
        PlayerKillLimit.PlayerKill -= OnPlayerDeath;
    }

    void BuildLeaderboardPanel()
    {
        leaderboardPanel = CreatePanel("LeaderboardPanel", canvas.transform);
        Image bg = leaderboardPanel.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);

        CreateText("LEADERBOARD", leaderboardPanel.transform,
            new Vector2(0, 200), 32, FontStyle.Bold, new Color(1f, 0.84f, 0f));

        float headerY = 160;
        CreateText("RANK", leaderboardPanel.transform, new Vector2(-200, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("", leaderboardPanel.transform, new Vector2(-130, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("PLAYER", leaderboardPanel.transform, new Vector2(-20, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("SCORE", leaderboardPanel.transform, new Vector2(130, headerY), 14, FontStyle.Bold, Color.gray);
        CreateText("COINS", leaderboardPanel.transform, new Vector2(220, headerY), 14, FontStyle.Bold, Color.gray);

        for (int i = 0; i < 10; i++)
        {
            float y = 120 - i * 35;
            Text[] row = new Text[5];
            row[0] = CreateText("", leaderboardPanel.transform, new Vector2(-200, y), 18, FontStyle.Normal, Color.white);
            row[1] = CreateText("", leaderboardPanel.transform, new Vector2(-130, y), 18, FontStyle.Normal, Color.white);
            row[2] = CreateText("", leaderboardPanel.transform, new Vector2(-20, y), 18, FontStyle.Normal, Color.white);
            row[3] = CreateText("", leaderboardPanel.transform, new Vector2(130, y), 18, FontStyle.Normal, Color.white);
            row[4] = CreateText("", leaderboardPanel.transform, new Vector2(220, y), 18, FontStyle.Normal, new Color(1f, 0.84f, 0f));
            rows.Add(row);
        }

        // Close button
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(leaderboardPanel.transform, false);
        RectTransform closeRect = closeBtn.AddComponent<RectTransform>();
        closeRect.anchoredPosition = new Vector2(0, -230);
        closeRect.sizeDelta = new Vector2(200, 45);
        Image closeBg = closeBtn.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.15f, 0.15f, 1f);
        Button closeButton = closeBtn.AddComponent<Button>();
        closeButton.targetGraphic = closeBg;
        closeButton.onClick.AddListener(Hide);

        Text closeTxt = CreateText("CLOSE", closeBtn.transform, Vector2.zero, 20, FontStyle.Bold, Color.white);
        closeTxt.rectTransform.anchorMin = Vector2.zero;
        closeTxt.rectTransform.anchorMax = Vector2.one;
        closeTxt.rectTransform.offsetMin = Vector2.zero;
        closeTxt.rectTransform.offsetMax = Vector2.zero;

        leaderboardPanel.SetActive(false);
    }

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

    void OnPlayerDeath(object sender, System.EventArgs e)
    {
        if (LeaderboardManager.Instance == null) return;

        // Calculate score directly — don't rely on ActualScoreDisplay timing
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        int height = 0;
        if (playerObj != null)
            height = Mathf.Max(0, (int)playerObj.transform.position.y);
        int coins = HighScoreSet.gameScore;
        int score = height + coins;

        string playerName = LeaderboardManager.Instance.GetSavedPlayerName();
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
            LeaderboardManager.Instance.SetCurrentPlayer(playerName);
        }

        Debug.Log("LEADERBOARD: Adding score=" + score + " coins=" + coins + " for " + playerName);
        LeaderboardManager.Instance.TryAddScore(playerName, score, coins);
        ShowLeaderboard();
    }

    public void ShowLeaderboard()
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

                rows[i][0].text = i < 3 ? MEDALS[i] : (i + 1).ToString();
                rows[i][0].color = i == 0 ? new Color(1f, 0.84f, 0f) :
                                   i == 1 ? new Color(0.75f, 0.75f, 0.75f) :
                                   i == 2 ? new Color(0.8f, 0.5f, 0.2f) : rowColor;

                string display;
                FRUIT_DISPLAY.TryGetValue(entry.icon, out display);
                rows[i][1].text = "[" + (display ?? "?") + "]";
                rows[i][1].color = rowColor;

                rows[i][2].text = entry.playerName;
                rows[i][2].color = rowColor;
                rows[i][2].fontStyle = isYou ? FontStyle.Bold : FontStyle.Normal;

                rows[i][3].text = entry.score.ToString();
                rows[i][3].color = rowColor;

                rows[i][4].text = entry.coins.ToString();
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
