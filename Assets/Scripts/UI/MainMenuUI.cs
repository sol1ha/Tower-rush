using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Self-building main menu with styled buttons.
/// Hides when game starts, shows on scene load.
///
/// Unity Setup:
/// 1. Create empty GameObject → name "MainMenu"
/// 2. Add component: MainMenuUI
/// 3. Drag your Canvas into the "Canvas" field
/// That's it!
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;

    [Header("Just assign your Canvas")]
    public Canvas canvas;

    [Header("Colors")]
    public Color buttonColor = new Color(0.05f, 0.2f, 0.5f, 1f);
    public Color buttonHighlight = new Color(0.1f, 0.35f, 0.7f, 1f);
    public Color titleColor = new Color(1f, 0.84f, 0f);
    public Color textColor = Color.white;

    private GameObject menuPanel;
    private GameObject helpPanel;
    private GameObject scoresPanel;
    private GameObject aboutPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(this); return; }
    }

    void Start()
    {
        if (canvas == null)
            canvas = FindFirstObjectByType<Canvas>();

        BuildMainMenu();
        BuildHelpPanel();
        BuildScoresPanel();
        BuildAboutPanel();
    }

    void Update()
    {
        if (GameManager.Playing() && menuPanel != null && menuPanel.activeSelf)
        {
            menuPanel.SetActive(false);
        }
    }

    // ========== MAIN MENU ==========

    void BuildMainMenu()
    {
        menuPanel = CreatePanel("MainMenuPanel", canvas.transform);
        Image bg = menuPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.05f, 0.15f, 0.9f);

        // Game title
        Text title = CreateText("INFINITY RUSH", menuPanel.transform,
            new Vector2(0, 180), 42, FontStyle.Bold, titleColor);
        title.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 60);

        // Subtitle
        Text sub = CreateText("How high can you go?", menuPanel.transform,
            new Vector2(0, 135), 16, FontStyle.Italic, new Color(0.7f, 0.8f, 1f));
        sub.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 30);

        // Buttons
        CreateStyledButton("PLAY", menuPanel.transform, new Vector2(0, 60), OnPlayClicked);
        CreateStyledButton("SCORES", menuPanel.transform, new Vector2(0, -5), OnScoresClicked);
        CreateStyledButton("HELP", menuPanel.transform, new Vector2(0, -70), OnHelpClicked);
        CreateStyledButton("ABOUT", menuPanel.transform, new Vector2(0, -135), OnAboutClicked);

        // Bottom text
        CreateText("Press SPACE or W to start", menuPanel.transform,
            new Vector2(0, -210), 14, FontStyle.Normal, new Color(1, 1, 1, 0.4f));
    }

    // ========== SUB PANELS ==========

    void BuildHelpPanel()
    {
        helpPanel = CreatePanel("HelpPanel", canvas.transform);
        Image bg = helpPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.05f, 0.15f, 0.95f);

        CreateText("HELP", helpPanel.transform, new Vector2(0, 200), 36, FontStyle.Bold, titleColor);

        string[] helpLines = {
            "W / SPACE  -  Jump",
            "A / D  -  Move Left / Right",
            "Collect coins for bonus score",
            "Avoid spikes! They cost you a life",
            "The floor is rising - keep climbing!",
            "Grab the jetpack for a boost!",
            "Score 10+ to enter the leaderboard"
        };

        for (int i = 0; i < helpLines.Length; i++)
        {
            Text t = CreateText(helpLines[i], helpPanel.transform,
                new Vector2(0, 130 - i * 40), 18, FontStyle.Normal, Color.white);
            t.GetComponent<RectTransform>().sizeDelta = new Vector2(500, 35);
        }

        CreateStyledButton("BACK", helpPanel.transform, new Vector2(0, -200), OnBackClicked);
        helpPanel.SetActive(false);
    }

    void BuildScoresPanel()
    {
        scoresPanel = CreatePanel("ScoresPanel", canvas.transform);
        Image bg = scoresPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.05f, 0.15f, 0.95f);

        CreateText("HIGH SCORES", scoresPanel.transform, new Vector2(0, 200), 36, FontStyle.Bold, titleColor);

        // Show high score
        int highScore = PlayerPrefs.GetInt(Constants.HighScore_Pref, 0);
        CreateText("Your Best: " + highScore, scoresPanel.transform,
            new Vector2(0, 130), 24, FontStyle.Normal, Color.white);

        // Show top scores from leaderboard if available
        if (LeaderboardManager.Instance != null)
        {
            var entries = LeaderboardManager.Instance.GetEntries();
            for (int i = 0; i < Mathf.Min(entries.Count, 5); i++)
            {
                string medal = i < 3 ? new string[] { "1st", "2nd", "3rd" }[i] : (i + 1).ToString();
                string row = medal + "   " + entries[i].playerName + "   " + entries[i].score;
                Color rowColor = i == 0 ? new Color(1f, 0.84f, 0f) :
                                 i == 1 ? new Color(0.75f, 0.75f, 0.75f) :
                                 i == 2 ? new Color(0.8f, 0.5f, 0.2f) : Color.white;
                Text t = CreateText(row, scoresPanel.transform,
                    new Vector2(0, 70 - i * 40), 20, FontStyle.Normal, rowColor);
                t.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 35);
            }
        }
        else
        {
            CreateText("Play to set scores!", scoresPanel.transform,
                new Vector2(0, 50), 20, FontStyle.Italic, new Color(1, 1, 1, 0.5f));
        }

        CreateStyledButton("BACK", scoresPanel.transform, new Vector2(0, -200), OnBackClicked);
        scoresPanel.SetActive(false);
    }

    void BuildAboutPanel()
    {
        aboutPanel = CreatePanel("AboutPanel", canvas.transform);
        Image bg = aboutPanel.GetComponent<Image>();
        bg.color = new Color(0.02f, 0.05f, 0.15f, 0.95f);

        CreateText("ABOUT", aboutPanel.transform, new Vector2(0, 200), 36, FontStyle.Bold, titleColor);

        string[] aboutLines = {
            "INFINITY RUSH",
            "",
            "An endless climbing game",
            "Jump higher, score more!",
            "",
            "Made by Soliha"
        };

        for (int i = 0; i < aboutLines.Length; i++)
        {
            CreateText(aboutLines[i], aboutPanel.transform,
                new Vector2(0, 120 - i * 40), 20, FontStyle.Normal, Color.white);
        }

        CreateStyledButton("BACK", aboutPanel.transform, new Vector2(0, -200), OnBackClicked);
        aboutPanel.SetActive(false);
    }

    // ========== BUTTON CALLBACKS ==========

    void OnPlayClicked()
    {
        menuPanel.SetActive(false);
        GameManager.StartGame();

        // Also unfreeze player (same as PressKeyToPlay)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            PressKeyToPlay pktp = playerObj.GetComponent<PressKeyToPlay>();
            if (pktp != null)
                pktp.enabled = false;
        }
    }

    void OnScoresClicked()
    {
        menuPanel.SetActive(false);
        scoresPanel.SetActive(true);
    }

    void OnHelpClicked()
    {
        menuPanel.SetActive(false);
        helpPanel.SetActive(true);
    }

    void OnAboutClicked()
    {
        menuPanel.SetActive(false);
        aboutPanel.SetActive(true);
    }

    void OnBackClicked()
    {
        helpPanel.SetActive(false);
        scoresPanel.SetActive(false);
        aboutPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // ========== UI BUILDERS ==========

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
        rect.sizeDelta = new Vector2(300, 35);

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.color = color;
        text.text = content;
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }

    void CreateStyledButton(string label, Transform parent, Vector2 position, UnityEngine.Events.UnityAction onClick)
    {
        // Outer glow/border (light blue offset)
        GameObject glowObj = new GameObject("ButtonGlow_" + label);
        glowObj.transform.SetParent(parent, false);
        RectTransform glowRect = glowObj.AddComponent<RectTransform>();
        glowRect.anchoredPosition = position + new Vector2(4, -3);
        glowRect.sizeDelta = new Vector2(260, 50);
        glowRect.localRotation = Quaternion.Euler(0, 0, -2f);
        Image glowImg = glowObj.AddComponent<Image>();
        glowImg.color = new Color(0.2f, 0.6f, 0.9f, 0.6f);

        // Main button background
        GameObject btnObj = new GameObject("Button_" + label);
        btnObj.transform.SetParent(parent, false);
        RectTransform btnRect = btnObj.AddComponent<RectTransform>();
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(250, 48);
        btnRect.localRotation = Quaternion.Euler(0, 0, -1.5f);

        Image btnBg = btnObj.AddComponent<Image>();
        btnBg.color = buttonColor;

        Button btn = btnObj.AddComponent<Button>();

        // Hover color
        ColorBlock colors = btn.colors;
        colors.normalColor = buttonColor;
        colors.highlightedColor = buttonHighlight;
        colors.pressedColor = new Color(0.02f, 0.15f, 0.4f, 1f);
        colors.selectedColor = buttonHighlight;
        btn.colors = colors;
        btn.targetGraphic = btnBg;

        btn.onClick.AddListener(onClick);

        // Button text (straight, not rotated)
        GameObject textObj = new GameObject("ButtonLabel");
        textObj.transform.SetParent(btnObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.localRotation = Quaternion.Euler(0, 0, 1.5f); // Counter-rotate so text is straight

        Text btnText = textObj.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 22;
        btnText.text = label;
        btnText.color = textColor;
        btnText.fontStyle = FontStyle.Bold;
        btnText.alignment = TextAnchor.MiddleCenter;
    }

    public void ShowMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(true);
    }
}
