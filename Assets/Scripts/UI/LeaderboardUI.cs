using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance;

    [Header("Just assign your Canvas")]
    public Canvas canvas;

    private GameObject leaderboardPanel;
    private RectTransform panelRect;
    private Text titleText;
    private Text personalBestText;
    private List<RowWidgets> rowWidgets = new List<RowWidgets>();
    private CanvasGroup panelGroup;

    private static readonly string[] MEDALS = { "★ 1", "● 2", "● 3" };
    private static readonly Color GOLD   = new Color(1.00f, 0.84f, 0.10f);
    private static readonly Color SILVER = new Color(0.78f, 0.82f, 0.88f);
    private static readonly Color BRONZE = new Color(0.85f, 0.55f, 0.25f);

    private static readonly Dictionary<string, string> FRUIT_DISPLAY = new Dictionary<string, string>()
    {
        {"apple", "A"}, {"cherry", "C"}, {"grape", "G"},
        {"lemon", "L"}, {"orange", "O"}, {"peach", "P"},
        {"pear", "R"}, {"strawberry", "S"}, {"watermelon", "W"},
        {"banana", "B"}
    };

    class RowWidgets
    {
        public RectTransform rect;
        public Image background;
        public Text rank;
        public Text icon;
        public Text name;
        public Text score;
        public Text coins;
        public bool isCurrentPlayer;
    }

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
        leaderboardPanel = new GameObject("LeaderboardPanel");
        leaderboardPanel.transform.SetParent(canvas.transform, false);
        panelRect = leaderboardPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Full-screen dim background
        Image dim = leaderboardPanel.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        panelGroup = leaderboardPanel.AddComponent<CanvasGroup>();

        // Card container (centered)
        GameObject card = new GameObject("Card");
        card.transform.SetParent(leaderboardPanel.transform, false);
        RectTransform cardRect = card.AddComponent<RectTransform>();
        cardRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardRect.pivot = new Vector2(0.5f, 0.5f);
        cardRect.sizeDelta = new Vector2(680, 720);
        cardRect.anchoredPosition = Vector2.zero;
        Image cardBg = card.AddComponent<Image>();
        cardBg.color = new Color(0.08f, 0.10f, 0.18f, 0.98f);

        // Top accent bar (gold)
        GameObject accent = new GameObject("Accent");
        accent.transform.SetParent(card.transform, false);
        RectTransform accentRect = accent.AddComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 1f);
        accentRect.anchorMax = new Vector2(1f, 1f);
        accentRect.pivot = new Vector2(0.5f, 1f);
        accentRect.sizeDelta = new Vector2(0, 8);
        accentRect.anchoredPosition = Vector2.zero;
        accent.AddComponent<Image>().color = GOLD;

        // Title
        titleText = CreateText("★  LEADERBOARD  ★", card.transform,
            new Vector2(0, 320), 44, FontStyle.Bold, GOLD);
        titleText.rectTransform.sizeDelta = new Vector2(600, 60);

        // Subtitle / personal best
        personalBestText = CreateText("", card.transform,
            new Vector2(0, 270), 22, FontStyle.Bold, new Color(0.85f, 0.92f, 1f));
        personalBestText.rectTransform.sizeDelta = new Vector2(600, 36);

        // Header row
        float headerY = 220;
        Color headerCol = new Color(0.6f, 0.7f, 0.85f);
        CreateText("#",      card.transform, new Vector2(-260, headerY), 16, FontStyle.Bold, headerCol);
        CreateText("ICON",   card.transform, new Vector2(-180, headerY), 16, FontStyle.Bold, headerCol);
        CreateText("PLAYER", card.transform, new Vector2(-50,  headerY), 16, FontStyle.Bold, headerCol);
        CreateText("SCORE",  card.transform, new Vector2(140,  headerY), 16, FontStyle.Bold, headerCol);
        CreateText("COINS",  card.transform, new Vector2(240,  headerY), 16, FontStyle.Bold, headerCol);

        // Header underline
        GameObject headerLine = new GameObject("HeaderLine");
        headerLine.transform.SetParent(card.transform, false);
        RectTransform hlRect = headerLine.AddComponent<RectTransform>();
        hlRect.sizeDelta = new Vector2(600, 2);
        hlRect.anchoredPosition = new Vector2(0, headerY - 18);
        headerLine.AddComponent<Image>().color = new Color(GOLD.r, GOLD.g, GOLD.b, 0.4f);

        // Rows
        float rowHeight = 42f;
        float firstRowY = 175;
        for (int i = 0; i < 10; i++)
        {
            float y = firstRowY - i * rowHeight;
            rowWidgets.Add(BuildRow(card.transform, y));
        }

        // Close button
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(card.transform, false);
        RectTransform closeRect = closeBtn.AddComponent<RectTransform>();
        closeRect.anchoredPosition = new Vector2(0, -310);
        closeRect.sizeDelta = new Vector2(240, 50);
        Image closeBg = closeBtn.AddComponent<Image>();
        closeBg.color = new Color(0.85f, 0.22f, 0.22f, 1f);
        Button closeButton = closeBtn.AddComponent<Button>();
        ColorBlock cb = closeButton.colors;
        cb.highlightedColor = new Color(1f, 0.35f, 0.35f, 1f);
        cb.pressedColor = new Color(0.65f, 0.15f, 0.15f, 1f);
        cb.normalColor = closeBg.color;
        closeButton.colors = cb;
        closeButton.targetGraphic = closeBg;
        closeButton.onClick.AddListener(Hide);
        Text closeTxt = CreateText("CLOSE", closeBtn.transform, Vector2.zero, 22, FontStyle.Bold, Color.white);
        closeTxt.rectTransform.anchorMin = Vector2.zero;
        closeTxt.rectTransform.anchorMax = Vector2.one;
        closeTxt.rectTransform.offsetMin = Vector2.zero;
        closeTxt.rectTransform.offsetMax = Vector2.zero;

        leaderboardPanel.SetActive(false);
    }

    RowWidgets BuildRow(Transform parent, float y)
    {
        RowWidgets w = new RowWidgets();

        GameObject rowGo = new GameObject("Row");
        rowGo.transform.SetParent(parent, false);
        w.rect = rowGo.AddComponent<RectTransform>();
        w.rect.sizeDelta = new Vector2(600, 38);
        w.rect.anchoredPosition = new Vector2(0, y);
        w.background = rowGo.AddComponent<Image>();
        w.background.color = new Color(0.13f, 0.16f, 0.24f, 0.7f);

        w.rank  = CreateText("", rowGo.transform, new Vector2(-260, 0), 22, FontStyle.Bold, Color.white);
        w.icon  = CreateText("", rowGo.transform, new Vector2(-180, 0), 18, FontStyle.Bold, Color.white);
        w.name  = CreateText("", rowGo.transform, new Vector2(-50,  0), 20, FontStyle.Normal, Color.white);
        w.score = CreateText("", rowGo.transform, new Vector2(140,  0), 22, FontStyle.Bold, Color.white);
        w.coins = CreateText("", rowGo.transform, new Vector2(240,  0), 20, FontStyle.Bold, GOLD);

        w.name.rectTransform.sizeDelta = new Vector2(180, 30);
        w.name.alignment = TextAnchor.MiddleLeft;

        return w;
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

        Outline outline = obj.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.85f);
        outline.effectDistance = new Vector2(1.2f, -1.2f);

        return text;
    }

    void OnPlayerDeath(object sender, System.EventArgs e)
    {
        if (LeaderboardManager.Instance == null) return;

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

        // Personal best line
        int personalBest = 0;
        foreach (var e in entries)
        {
            if (e.playerName == currentPlayer && e.score > personalBest) personalBest = e.score;
        }
        if (personalBestText != null)
        {
            if (!string.IsNullOrEmpty(currentPlayer) && personalBest > 0)
                personalBestText.text = "Your best: " + FormatScore(personalBest) + "  •  " + currentPlayer;
            else
                personalBestText.text = "Climb higher to set a record!";
        }

        for (int i = 0; i < 10; i++)
        {
            var w = rowWidgets[i];
            if (i < entries.Count)
            {
                var entry = entries[i];
                bool isYou = entry.playerName == currentPlayer;
                w.isCurrentPlayer = isYou;

                // Row background — top 3 get medal-tinted backgrounds, others alternate.
                Color rowBg;
                if      (i == 0) rowBg = new Color(GOLD.r,   GOLD.g,   GOLD.b,   0.22f);
                else if (i == 1) rowBg = new Color(SILVER.r, SILVER.g, SILVER.b, 0.22f);
                else if (i == 2) rowBg = new Color(BRONZE.r, BRONZE.g, BRONZE.b, 0.22f);
                else             rowBg = (i % 2 == 0)
                                    ? new Color(0.13f, 0.16f, 0.24f, 0.7f)
                                    : new Color(0.10f, 0.13f, 0.20f, 0.7f);
                if (isYou) rowBg = new Color(1f, 0.92f, 0.20f, 0.30f);
                w.background.color = rowBg;

                // Rank text & color
                Color rankCol = i == 0 ? GOLD : i == 1 ? SILVER : i == 2 ? BRONZE
                              : new Color(0.85f, 0.88f, 0.95f);
                w.rank.text = i < 3 ? MEDALS[i] : "#" + (i + 1);
                w.rank.color = rankCol;
                w.rank.fontSize = i < 3 ? 24 : 22;

                string display;
                FRUIT_DISPLAY.TryGetValue(entry.icon, out display);
                w.icon.text = "[ " + (display ?? "?") + " ]";
                w.icon.color = isYou ? Color.white : new Color(0.92f, 0.95f, 1f);

                w.name.text = entry.playerName;
                w.name.color = isYou ? new Color(1f, 0.96f, 0.4f) : Color.white;
                w.name.fontStyle = isYou ? FontStyle.Bold : FontStyle.Normal;

                w.score.text = FormatScore(entry.score);
                w.score.color = isYou ? new Color(1f, 0.96f, 0.4f) : Color.white;

                w.coins.text = "$" + entry.coins;
                w.coins.color = GOLD;
            }
            else
            {
                w.isCurrentPlayer = false;
                w.background.color = new Color(0.10f, 0.12f, 0.18f, 0.4f);
                w.rank.text = "#" + (i + 1);
                w.rank.color = new Color(0.4f, 0.45f, 0.55f);
                w.icon.text = "—";
                w.icon.color = new Color(0.4f, 0.45f, 0.55f);
                w.name.text = "—";
                w.name.color = new Color(0.4f, 0.45f, 0.55f);
                w.score.text = "—";
                w.score.color = new Color(0.4f, 0.45f, 0.55f);
                w.coins.text = "";
            }
        }

        StopAllCoroutines();
        StartCoroutine(EntranceAnimation());
    }

    IEnumerator EntranceAnimation()
    {
        // Fade-in panel + stagger row slide-in.
        if (panelGroup != null) panelGroup.alpha = 0f;
        for (int i = 0; i < rowWidgets.Count; i++)
        {
            var w = rowWidgets[i];
            if (w.rect != null)
            {
                Vector2 baseTarget = new Vector2(0, w.rect.anchoredPosition.y);
                w.rect.anchoredPosition = baseTarget + new Vector2(380f, 0f);
                w.rect.localScale = new Vector3(0.95f, 0.95f, 1f);
            }
        }

        float panelFade = 0.18f;
        float t = 0f;
        while (t < panelFade)
        {
            t += Time.unscaledDeltaTime;
            if (panelGroup != null) panelGroup.alpha = Mathf.Clamp01(t / panelFade);
            yield return null;
        }
        if (panelGroup != null) panelGroup.alpha = 1f;

        float rowDuration = 0.35f;
        float stagger = 0.05f;
        float[] rowStart = new float[rowWidgets.Count];
        for (int i = 0; i < rowWidgets.Count; i++) rowStart[i] = i * stagger;

        float total = rowDuration + stagger * rowWidgets.Count;
        float elapsed = 0f;
        while (elapsed < total)
        {
            elapsed += Time.unscaledDeltaTime;
            for (int i = 0; i < rowWidgets.Count; i++)
            {
                var w = rowWidgets[i];
                float local = Mathf.Clamp01((elapsed - rowStart[i]) / rowDuration);
                float eased = EaseOutBack(local);
                if (w.rect != null)
                {
                    Vector2 baseTarget = new Vector2(0, w.rect.anchoredPosition.y);
                    w.rect.anchoredPosition = new Vector2(Mathf.LerpUnclamped(380f, 0f, eased), baseTarget.y);
                    float s = Mathf.LerpUnclamped(0.95f, 1f, eased);
                    w.rect.localScale = new Vector3(s, s, 1f);
                }
            }
            yield return null;
        }

        // Settle
        for (int i = 0; i < rowWidgets.Count; i++)
        {
            var w = rowWidgets[i];
            if (w.rect != null)
            {
                Vector2 baseTarget = new Vector2(0, w.rect.anchoredPosition.y);
                w.rect.anchoredPosition = new Vector2(0, baseTarget.y);
                w.rect.localScale = Vector3.one;
            }
        }

        // Continuous gentle pulse for the current player's row.
        while (leaderboardPanel != null && leaderboardPanel.activeSelf)
        {
            float pulse = 0.5f + Mathf.Sin(Time.unscaledTime * 3f) * 0.5f;
            for (int i = 0; i < rowWidgets.Count; i++)
            {
                var w = rowWidgets[i];
                if (!w.isCurrentPlayer || w.rect == null) continue;
                float s = Mathf.Lerp(1.00f, 1.04f, pulse);
                w.rect.localScale = new Vector3(s, s, 1f);
                Color baseC = new Color(1f, 0.92f, 0.20f, Mathf.Lerp(0.22f, 0.42f, pulse));
                w.background.color = baseC;
            }
            yield return null;
        }
    }

    static string FormatScore(int score)
    {
        if (score >= 1_000_000) return (score / 1000_000f).ToString("0.0") + "M";
        if (score >= 10_000)    return (score / 1000f).ToString("0.0") + "K";
        return score.ToString();
    }

    static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float k = t - 1f;
        return 1f + c3 * k * k * k + c1 * k * k;
    }

    public void Hide()
    {
        StopAllCoroutines();
        if (leaderboardPanel != null) leaderboardPanel.SetActive(false);
    }
}
