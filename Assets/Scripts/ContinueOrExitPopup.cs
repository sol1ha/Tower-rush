using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class ContinueOrExitPopup : MonoBehaviour
{
    public int continueCost = 2;
    public Canvas canvas;
    private GameObject popupPanel;
    private Text titleText;
    private Text bankText;
    private Text cardNumberText;
    private Text cardHolderText;
    private Text balanceText;
    private Text messageText;
    private Button continueButton;
    private Button endButton;

    void Start()
    {
        if (canvas == null)
            canvas = FindAnyObjectByType<Canvas>();

        if (canvas == null)
        {
            Debug.LogError("ContinueOrExitPopup: No Canvas found in the scene.");
            return;
        }

        EnsureEventSystem();
        BuildPopup();
        HidePopup();
        PlayerKillLimit.PlayerKill += OnPlayerDeath;
    }

    void OnDestroy()
    {
        PlayerKillLimit.PlayerKill -= OnPlayerDeath;
    }

    void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }

    void BuildPopup()
    {
        popupPanel = new GameObject("ContinueExitPopupPanel");
        popupPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = popupPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.25f, 0.25f);
        panelRect.anchorMax = new Vector2(0.75f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = popupPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);

        CreateTextElement("PopupTitle", popupPanel.transform, new Vector2(0, 165), 28, FontStyle.Bold, "Continue or End", out titleText);
        CreateTextElement("PopupBank", popupPanel.transform, new Vector2(120, 120), 18, FontStyle.Bold, "BANK NAME", out bankText).alignment = TextAnchor.UpperRight;
        CreateTextElement("PopupCardLabel", popupPanel.transform, new Vector2(-120, 120), 18, FontStyle.Bold, "CREDIT CARD", out Text cardLabelText);
        CreateTextElement("PopupCardNumber", popupPanel.transform, new Vector2(0, 80), 22, FontStyle.Bold, "1234 5678 9012 3456", out cardNumberText);
        CreateTextElement("PopupHolder", popupPanel.transform, new Vector2(-80, 35), 20, FontStyle.Normal, "NAME SURNAME", out cardHolderText).alignment = TextAnchor.MiddleLeft;
        CreateTextElement("PopupBalance", popupPanel.transform, new Vector2(0, -5), 18, FontStyle.Normal, "Balance: $0", out balanceText);
        CreateTextElement("PopupMessage", popupPanel.transform, new Vector2(0, -40), 18, FontStyle.Normal, "Continue playing from current place", out messageText);

        continueButton = CreateButton("ContinueButton", popupPanel.transform, new Vector2(-80, -100), new Vector2(180, 50), "Continue $2", OnContinueClicked);
        endButton = CreateButton("EndButton", popupPanel.transform, new Vector2(80, -100), new Vector2(180, 50), "End", OnEndClicked);
    }

    Text CreateTextElement(string name, Transform parent, Vector2 position, int size, FontStyle style, string textValue, out Text textOut)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(420, 40);

        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = size;
        text.fontStyle = style;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = textValue;
        textOut = text;
        return text;
    }

    Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);
        RectTransform rect = btnObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = btnObj.AddComponent<Image>();
        image.color = new Color(0.1f, 0.45f, 0.2f, 1f);

        Button button = btnObj.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        GameObject labelObj = new GameObject(name + "Text");
        labelObj.transform.SetParent(btnObj.transform, false);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        Text labelText = labelObj.AddComponent<Text>();
        labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        labelText.fontSize = 20;
        labelText.fontStyle = FontStyle.Bold;
        labelText.alignment = TextAnchor.MiddleCenter;
        labelText.color = Color.white;
        labelText.text = label;

        return button;
    }

    void OnPlayerDeath(object sender, System.EventArgs e)
    {
        ShowPopup();
    }

    void ShowPopup()
    {
        if (popupPanel == null) return;

        UpdatePopupText();
        popupPanel.SetActive(true);
        Time.timeScale = 0f;
        if (GameManager.instance != null)
            GameManager.instance.play = false;

        ReloadScene.CanReloadAfterDeath = false;
    }

    void HidePopup()
    {
        if (popupPanel == null) return;

        popupPanel.SetActive(false);
        Time.timeScale = 1f;
        ReloadScene.CanReloadAfterDeath = true;
    }

    void UpdatePopupText()
    {
        if (cardHolderText != null)
        {
            string playerName = "NAME SURNAME";
            if (LeaderboardManager.Instance != null)
            {
                playerName = LeaderboardManager.Instance.GetSavedPlayerName();
                if (string.IsNullOrEmpty(playerName))
                    playerName = "NAME SURNAME";
            }
            cardHolderText.text = playerName.ToUpper();
        }

        if (balanceText != null)
        {
            balanceText.text = "Balance: $" + HighScoreSet.PersistentCoins;
        }

        if (messageText != null)
        {
            messageText.text = "Continue from current place for $" + continueCost + " or End session.";
        }
    }

    void OnContinueClicked()
    {
        if (HighScoreSet.PersistentCoins < continueCost)
        {
            if (messageText != null)
                messageText.text = "Not enough balance. You need $" + continueCost + ".";
            return;
        }

        HighScoreSet.PersistentCoins -= continueCost;
        HidePopup();

        if (GameManager.instance != null)
            GameManager.instance.play = true;

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.RestoreForContinue();
    }

    void OnEndClicked()
    {
        HidePopup();
        SceneManager.LoadScene(0);
    }
}
