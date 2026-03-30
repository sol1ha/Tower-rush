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
    private GameObject usernamePanel;
    private InputField usernameInput;
    private Text messageText;
    private List<Text[]> rows = new List<Text[]>(); // Each row: rank, icon, name, score, stars

    private int pendingScore = 0;
    private bool waitingForName = false;

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
        PlayerKillLimit.PlayerKill += OnPlayerDeath;

        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        BuildLeaderboardPanel();
        BuildUsernamePanel();
    }

    void OnDestroy()
    {
        PlayerKillLimit.PlayerKill -= OnPlayerDeath;
    }

    void Update()
    {
        if (waitingForName && Input.GetKeyDown(KeyCode.Return))
            OnSubmitName();
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

    void BuildUsernamePanel()
    {
        usernamePanel = CreatePanel("UsernamePanel", canvas.transform);
        Image bg = usernamePanel.GetComponent<Image>();
        bg.color = new Color(0.05f, 0.1f, 0.2f, 0.95f);

        // Title
        CreateText("NEW HIGH SCORE!", usernamePanel.transform,
            new Vector2(0, 100), 28, FontStyle.Bold, new Color(1f, 0.84f, 0f));

        // Score message
        messageText = CreateText("Score: 0 - Enter your name!", usernamePanel.transform,
            new Vector2(0, 50), 20, FontStyle.Normal, Color.white);

        // Input field
        GameObject inputObj = new GameObject("UsernameInput");
        inputObj.transform.SetParent(usernamePanel.transform, false);
        RectTransform inputRect = inputObj.AddComponent<RectTransform>();
        inputRect.anchoredPosition = new Vector2(0, -10);
        inputRect.sizeDelta = new Vector2(300, 40);

        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.15f, 0.2f, 0.3f, 1f);

        usernameInput = inputObj.AddComponent<InputField>();
        usernameInput.characterLimit = 12;

        // Input text
        GameObject inputTextObj = new GameObject("InputText");
        inputTextObj.transform.SetParent(inputObj.transform, false);
        RectTransform textRect = inputTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        Text inputText = inputTextObj.AddComponent<Text>();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.fontSize = 22;
        inputText.color = Color.white;
        inputText.alignment = TextAnchor.MiddleCenter;
        usernameInput.textComponent = inputText;

        // Placeholder
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        RectTransform phRect = placeholderObj.AddComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = new Vector2(10, 0);
        phRect.offsetMax = new Vector2(-10, 0);
        Text placeholder = placeholderObj.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.fontSize = 22;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.color = new Color(1, 1, 1, 0.3f);
        placeholder.text = "Type your name...";
        placeholder.alignment = TextAnchor.MiddleCenter;
        usernameInput.placeholder = placeholder;

        // Submit button
        GameObject btnObj = new GameObject("SubmitButton");
        btnObj.transform.SetParent(usernamePanel.transform, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0, -70);
        btnRect.sizeDelta = new Vector2(200, 45);

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = new Color(0f, 0.6f, 0.3f, 1f);

        Button btn = btnObj.AddComponent<Button>();
        btn.onClick.AddListener(OnSubmitName);

        GameObject btnTextObj = new GameObject("ButtonText");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        Text btnText = btnTextObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 22;
        btnText.text = "SUBMIT";
        btnText.color = Color.white;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;

        usernamePanel.SetActive(false);
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
        pendingScore = ActualScoreDisplay.CurrentScore;
        Debug.Log("LEADERBOARD: Player died! Score = " + pendingScore);
        HighScoreSet.SetHighscore(pendingScore);

        if (LeaderboardManager.Instance == null)
        {
            Debug.Log("LEADERBOARD: LeaderboardManager.Instance is NULL!");
            return;
        }

        if (pendingScore < LeaderboardManager.Instance.GetMinScoreToQualify())
        {
            Debug.Log("LEADERBOARD: Score " + pendingScore + " too low (need " + LeaderboardManager.Instance.GetMinScoreToQualify() + ")");
            return;
        }

        string savedName = LeaderboardManager.Instance.GetSavedPlayerName();
        Debug.Log("LEADERBOARD: Saved player name = '" + savedName + "'");

        if (!string.IsNullOrEmpty(savedName))
        {
            LeaderboardManager.Instance.TryAddScore(savedName, pendingScore);
            Debug.Log("LEADERBOARD: Showing leaderboard for returning player");
            ShowLeaderboard();
        }
        else
        {
            Debug.Log("LEADERBOARD: Showing username entry for new player");
            ShowUsernameEntry();
        }
    }

    void ShowUsernameEntry()
    {
        waitingForName = true;
        usernamePanel.SetActive(true);

        if (usernameInput != null)
        {
            usernameInput.text = "";
            usernameInput.ActivateInputField();
        }

        if (messageText != null)
            messageText.text = "Score: " + pendingScore + " - Enter your name!";
    }

    void OnSubmitName()
    {
        string playerName = usernameInput != null ? usernameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(playerName))
            playerName = "Player" + Random.Range(100, 999);
        if (playerName.Length > 12)
            playerName = playerName.Substring(0, 12);

        waitingForName = false;
        usernamePanel.SetActive(false);

        LeaderboardManager.Instance.SetCurrentPlayer(playerName);
        LeaderboardManager.Instance.TryAddScore(playerName, pendingScore);
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
        if (usernamePanel != null) usernamePanel.SetActive(false);
    }
}
