using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    public static LeaderboardUI Instance;

    [Header("Just assign your Canvas")]
    public Canvas canvas;

    [Header("Timing")]
    [Tooltip("Seconds to wait after the player dies before showing the leaderboard, so the 'You Lost' animation can play first.")]
    public float delayBeforeShow = 1.6f;

    // ----- palette inspired by the reference card -----
    static readonly Color TEAL_BG       = new Color(0.27f, 0.62f, 0.60f, 1f);
    static readonly Color TEAL_OUTER    = new Color(0.20f, 0.34f, 0.36f, 1f);
    static readonly Color CARD_WHITE    = new Color(1.00f, 1.00f, 1.00f, 1f);
    static readonly Color ORANGE        = new Color(0.97f, 0.62f, 0.27f, 1f);
    static readonly Color ORANGE_LIGHT  = new Color(0.99f, 0.78f, 0.45f, 1f);
    static readonly Color BROWN_TEXT    = new Color(0.27f, 0.15f, 0.10f, 1f);
    static readonly Color WHITE_TEXT    = Color.white;

    // ----- runtime layout -----
    private GameObject root;
    private GameObject card;
    private CanvasGroup rootGroup;

    private class PodiumCard
    {
        public RectTransform rect;
        public Image cardBg;
        public Image avatarRing;
        public Text avatarLetter;
        public Text nameText;
        public Text scoreText;
        public Image rankBadgeBg;
        public Text rankBadgeText;
        public Vector2 baseAnchored;
        public bool isCurrentPlayer;
    }
    private PodiumCard[] podium = new PodiumCard[3];

    private class RowWidgets
    {
        public RectTransform rect;
        public Image rowBg;
        public Image avatarBg;
        public Text avatarLetter;
        public Text rankText;
        public Text nameText;
        public Image scoreBadge;
        public Text scoreText;
        public Vector2 baseAnchored;
        public bool isCurrentPlayer;
    }
    private List<RowWidgets> bottomRows = new List<RowWidgets>();

    private Sprite roundedCardSprite;
    private Sprite roundedRowSprite;
    private Sprite roundedBadgeSprite;
    private Sprite circleSprite;
    private Sprite ringSprite;

    private Coroutine entranceCoroutine;
    private Coroutine pulseCoroutine;
    private Coroutine deathCoroutine;

    // Two rankings: top scores vs top coin collectors. Toggle between them.
    private LeaderboardManager.SortMode currentSort = LeaderboardManager.SortMode.ByScore;
    private Text titleText;        // "★  LEADERBOARD  ★"
    private Image scoreTabBg;
    private Text  scoreTabLabel;
    private Image coinsTabBg;
    private Text  coinsTabLabel;
    private Text  scoreColumnHeader; // "SCORE" / "COINS"
    private Text  coinsColumnHeader; // "COINS" / "SCORE"

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
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        BuildSprites();
        BuildLeaderboardPanel();
        PlayerKillLimit.PlayerKill += OnPlayerDeath;
    }

    void OnDestroy()
    {
        PlayerKillLimit.PlayerKill -= OnPlayerDeath;
    }

    // ============================================================ sprites
    void BuildSprites()
    {
        roundedCardSprite  = MakeRoundedRect(256, 256, 32, Color.white);
        roundedRowSprite   = MakeRoundedRect(256, 64, 18, Color.white);
        roundedBadgeSprite = MakeRoundedRect(128, 64, 12, Color.white);
        circleSprite       = MakeCircle(128, Color.white, Color.clear, 0);
        ringSprite         = MakeCircle(128, Color.white, BROWN_TEXT, 4);
    }

    Sprite MakeRoundedRect(int w, int h, int r, Color color)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                bool inCorner = false;
                int cx = x, cy = y;
                if (x < r && y < r)                 { cx = r - x; cy = r - y; inCorner = true; }
                else if (x >= w - r && y < r)       { cx = x - (w - r - 1); cy = r - y; inCorner = true; }
                else if (x < r && y >= h - r)       { cx = r - x; cy = y - (h - r - 1); inCorner = true; }
                else if (x >= w - r && y >= h - r)  { cx = x - (w - r - 1); cy = y - (h - r - 1); inCorner = true; }

                if (inCorner)
                {
                    float d = Mathf.Sqrt(cx * cx + cy * cy);
                    if (d > r) { px[y * w + x] = Color.clear; continue; }
                    float alpha = Mathf.Clamp01(r - d);
                    Color c = color; c.a *= alpha;
                    px[y * w + x] = c;
                }
                else px[y * w + x] = color;
            }
        }
        tex.SetPixels(px); tex.Apply();
        Sprite s = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        return s;
    }

    Sprite MakeCircle(int diameter, Color fill, Color border, int borderWidth)
    {
        Texture2D tex = new Texture2D(diameter, diameter, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] px = new Color[diameter * diameter];
        float radius = diameter / 2f;
        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - radius + 0.5f;
                float dy = y - radius + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > radius) px[y * diameter + x] = Color.clear;
                else if (borderWidth > 0 && dist > radius - borderWidth) px[y * diameter + x] = border;
                else px[y * diameter + x] = fill;
            }
        }
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, diameter, diameter), new Vector2(0.5f, 0.5f), 100f);
    }

    // ============================================================ layout
    GameObject MakeGo(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    Image AddImage(GameObject go, Sprite sprite, Color color, bool sliced = true)
    {
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = color;
        if (sliced && sprite != null && sprite.border != Vector4.zero) img.type = Image.Type.Sliced;
        return img;
    }

    Text MakeText(string content, Transform parent, Vector2 pos, Vector2 size, int fontSize, FontStyle style, Color color, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        var go = new GameObject("T_" + content);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.fontStyle = style;
        t.color = color;
        t.text = content;
        t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        return t;
    }

    void BuildLeaderboardPanel()
    {
        // ---- root + dim ----
        root = MakeGo("LeaderboardRoot", canvas.transform);
        var rRect = (RectTransform)root.transform;
        rRect.anchorMin = Vector2.zero; rRect.anchorMax = Vector2.one;
        rRect.offsetMin = rRect.offsetMax = Vector2.zero;
        var dim = root.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        rootGroup = root.AddComponent<CanvasGroup>();

        // ---- main card ----
        card = MakeGo("Card", root.transform);
        var cRect = (RectTransform)card.transform;
        cRect.anchorMin = cRect.anchorMax = new Vector2(0.5f, 0.5f);
        cRect.pivot = new Vector2(0.5f, 0.5f);
        cRect.sizeDelta = new Vector2(820, 1080);
        cRect.anchoredPosition = Vector2.zero;
        AddImage(card, roundedCardSprite, TEAL_BG);

        // outer dark frame (slightly larger behind the card)
        var frame = MakeGo("Frame", root.transform);
        var fRect = (RectTransform)frame.transform;
        fRect.anchorMin = fRect.anchorMax = new Vector2(0.5f, 0.5f);
        fRect.pivot = new Vector2(0.5f, 0.5f);
        fRect.sizeDelta = new Vector2(840, 1100);
        fRect.anchoredPosition = Vector2.zero;
        AddImage(frame, roundedCardSprite, TEAL_OUTER);
        frame.transform.SetSiblingIndex(card.transform.GetSiblingIndex());

        // ---- title ----
        titleText = MakeText("•  TOP SCORES  •", card.transform,
            new Vector2(0, 480), new Vector2(700, 60),
            36, FontStyle.Bold, WHITE_TEXT);

        // ---- sort tabs ('SCORES' / 'COINS') — pushed up below the title so
        // they're not buried behind the podium / leaderboard rows. ----
        BuildSortTab(out scoreTabBg, out scoreTabLabel, "SCORES",
            new Vector2(-130, 410), LeaderboardManager.SortMode.ByScore);
        BuildSortTab(out coinsTabBg, out coinsTabLabel, "COINS",
            new Vector2( 130, 410), LeaderboardManager.SortMode.ByCoins);
        UpdateTabHighlight();

        // ---- top 3 podium ----
        BuildPodium();

        // ---- rows 4..10 ----
        float topY = 30;
        float rowHeight = 70;
        float gap = 12;
        for (int i = 0; i < 7; i++)
        {
            float y = topY - i * (rowHeight + gap);
            bottomRows.Add(BuildBottomRow(card.transform, y, rowHeight));
        }

        // ---- close button ----
        var closeGo = MakeGo("CloseButton", card.transform);
        var closeRT = (RectTransform)closeGo.transform;
        closeRT.anchoredPosition = new Vector2(0, -410);
        closeRT.sizeDelta = new Vector2(220, 56);
        var closeBg = AddImage(closeGo, roundedRowSprite, ORANGE);
        var closeBtn = closeGo.AddComponent<Button>();
        var cb = closeBtn.colors;
        cb.normalColor = ORANGE; cb.highlightedColor = ORANGE_LIGHT; cb.pressedColor = new Color(0.85f, 0.5f, 0.2f);
        closeBtn.colors = cb;
        closeBtn.targetGraphic = closeBg;
        closeBtn.onClick.AddListener(Hide);

        var closeText = MakeText("CLOSE", closeGo.transform, Vector2.zero, new Vector2(220, 56), 26, FontStyle.Bold, BROWN_TEXT);
        closeText.rectTransform.anchorMin = Vector2.zero;
        closeText.rectTransform.anchorMax = Vector2.one;
        closeText.rectTransform.offsetMin = Vector2.zero;
        closeText.rectTransform.offsetMax = Vector2.zero;

        // After EVERYTHING is built, lift the SCORES / COINS tabs to the front
        // so the podium / rows can never render on top of them. Title also goes
        // to the front so it's never covered.
        if (titleText != null) titleText.transform.SetAsLastSibling();
        if (scoreTabBg != null) scoreTabBg.transform.SetAsLastSibling();
        if (coinsTabBg != null) coinsTabBg.transform.SetAsLastSibling();

        root.SetActive(false);
    }

    void BuildSortTab(out Image bg, out Text label, string title, Vector2 pos, LeaderboardManager.SortMode mode)
    {
        var tabGo = MakeGo("SortTab_" + title, card.transform);
        var rt = (RectTransform)tabGo.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 50);
        bg = AddImage(tabGo, roundedRowSprite, new Color(1f, 1f, 1f, 0.20f));
        var btn = tabGo.AddComponent<Button>();
        btn.targetGraphic = bg;
        var cb = btn.colors;
        cb.highlightedColor = new Color(1f, 1f, 1f, 0.30f);
        cb.pressedColor = new Color(1f, 1f, 1f, 0.45f);
        btn.colors = cb;
        btn.onClick.AddListener(() => SetSortMode(mode));

        label = MakeText(title, tabGo.transform, Vector2.zero, new Vector2(200, 50), 20, FontStyle.Bold, WHITE_TEXT);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;
    }

    public void SetSortMode(LeaderboardManager.SortMode mode)
    {
        if (mode == currentSort) return;
        currentSort = mode;
        UpdateTabHighlight();
        if (root != null && root.activeSelf) ShowLeaderboard();
    }

    void UpdateTabHighlight()
    {
        Color active   = new Color(GOLD.r, GOLD.g, GOLD.b, 0.85f);
        Color inactive = new Color(1f, 1f, 1f, 0.18f);
        if (scoreTabBg != null) scoreTabBg.color = currentSort == LeaderboardManager.SortMode.ByScore ? active : inactive;
        if (coinsTabBg != null) coinsTabBg.color = currentSort == LeaderboardManager.SortMode.ByCoins ? active : inactive;
        if (titleText != null) titleText.text = currentSort == LeaderboardManager.SortMode.ByCoins
            ? "•  TOP COIN COLLECTORS  •"
            : "•  TOP SCORES  •";
    }

    static readonly Color GOLD = new Color(1.00f, 0.84f, 0.10f, 1f);

    void BuildPodium()
    {
        // Layout: P2 left, P1 center (taller), P3 right.
        // Pushed down vs the previous layout so the podium #1 avatar doesn't
        // poke up into the SCORES / COINS tab area.
        BuildPodiumSlot(0, new Vector2(0,    150), new Vector2(180, 260), 130, 56);  // 1st
        BuildPodiumSlot(1, new Vector2(-180, 100), new Vector2(150, 200), 105, 46);  // 2nd
        BuildPodiumSlot(2, new Vector2( 180, 100), new Vector2(150, 200), 105, 46);  // 3rd
    }

    void BuildPodiumSlot(int index, Vector2 pos, Vector2 size, int avatarSize, int badgeSize)
    {
        var slotGo = MakeGo("Podium" + (index + 1), card.transform);
        var rt = (RectTransform)slotGo.transform;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        var bg = AddImage(slotGo, roundedCardSprite, CARD_WHITE);

        // Avatar ring
        var ringGo = MakeGo("Avatar", slotGo.transform);
        var ringRt = (RectTransform)ringGo.transform;
        ringRt.anchoredPosition = new Vector2(0, size.y * 0.5f - avatarSize * 0.5f - 12);
        ringRt.sizeDelta = new Vector2(avatarSize, avatarSize);
        var ringImg = AddImage(ringGo, circleSprite, ORANGE);

        // Avatar inner circle (lighter)
        var innerGo = MakeGo("AvatarInner", ringGo.transform);
        var innerRt = (RectTransform)innerGo.transform;
        innerRt.anchorMin = innerRt.anchorMax = new Vector2(0.5f, 0.5f);
        innerRt.sizeDelta = new Vector2(avatarSize * 0.78f, avatarSize * 0.78f);
        innerRt.anchoredPosition = Vector2.zero;
        AddImage(innerGo, circleSprite, ORANGE_LIGHT);

        // Avatar letter — Unity's legacy Text leaves a baseline gap that
        // makes glyphs read as slightly low. Nudge the anchor up by ~8% of
        // the avatar size so the letter looks visually centred in the ring.
        var letter = MakeText("?", innerGo.transform,
            new Vector2(0f, avatarSize * 0.08f),
            new Vector2(avatarSize, avatarSize),
            Mathf.RoundToInt(avatarSize * 0.55f), FontStyle.Bold, BROWN_TEXT);

        // Name text
        var nameY = size.y * 0.5f - avatarSize - 32;
        var nameText = MakeText("PLAYER", slotGo.transform, new Vector2(0, nameY), new Vector2(size.x - 16, 20), 14, FontStyle.Bold, BROWN_TEXT);

        // Score text
        var scoreY = nameY - 28;
        var scoreText = MakeText("0", slotGo.transform, new Vector2(0, scoreY), new Vector2(size.x - 16, 36), 28, FontStyle.Bold, BROWN_TEXT);

        // Rank badge (orange circle with rank number) - poking out at bottom center
        var badgeGo = MakeGo("RankBadge", slotGo.transform);
        var badgeRt = (RectTransform)badgeGo.transform;
        badgeRt.anchoredPosition = new Vector2(0, -size.y * 0.5f - badgeSize * 0.1f);
        badgeRt.sizeDelta = new Vector2(badgeSize, badgeSize);
        var badgeBg = AddImage(badgeGo, circleSprite, ORANGE);
        var badgeText = MakeText((index + 1).ToString(), badgeGo.transform,
            new Vector2(0f, badgeSize * 0.08f),
            new Vector2(badgeSize, badgeSize),
            Mathf.RoundToInt(badgeSize * 0.55f), FontStyle.Bold, WHITE_TEXT);

        var pc = new PodiumCard
        {
            rect = rt,
            cardBg = bg,
            avatarRing = ringImg,
            avatarLetter = letter,
            nameText = nameText,
            scoreText = scoreText,
            rankBadgeBg = badgeBg,
            rankBadgeText = badgeText,
            baseAnchored = pos
        };
        podium[index] = pc;
    }

    RowWidgets BuildBottomRow(Transform parent, float y, float rowHeight)
    {
        var rowGo = MakeGo("Row", parent);
        var rt = (RectTransform)rowGo.transform;
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(540, rowHeight);
        var rowBg = AddImage(rowGo, roundedRowSprite, CARD_WHITE);

        // Rank text (left)
        var rank = MakeText("4.", rowGo.transform, new Vector2(-220, 0), new Vector2(60, rowHeight), 28, FontStyle.Bold, BROWN_TEXT);

        // Avatar circle
        var avatarGo = MakeGo("Avatar", rowGo.transform);
        var aRt = (RectTransform)avatarGo.transform;
        aRt.anchoredPosition = new Vector2(-160, 0);
        aRt.sizeDelta = new Vector2(rowHeight - 14, rowHeight - 14);
        var aImg = AddImage(avatarGo, circleSprite, ORANGE_LIGHT);

        var letter = MakeText("?", avatarGo.transform,
            new Vector2(0f, rowHeight * 0.08f),
            new Vector2(rowHeight, rowHeight),
            Mathf.RoundToInt((rowHeight - 14) * 0.55f), FontStyle.Bold, BROWN_TEXT);

        // Name text (center-left)
        var nameText = MakeText("PLAYER", rowGo.transform, new Vector2(-30, 0), new Vector2(220, rowHeight), 22, FontStyle.Bold, BROWN_TEXT, TextAnchor.MiddleLeft);

        // Score badge (right side, orange)
        var badgeGo = MakeGo("ScoreBadge", rowGo.transform);
        var bRt = (RectTransform)badgeGo.transform;
        bRt.anchoredPosition = new Vector2(190, 0);
        bRt.sizeDelta = new Vector2(140, rowHeight - 8);
        var bImg = AddImage(badgeGo, roundedRowSprite, ORANGE);
        var scoreText = MakeText("0", badgeGo.transform, Vector2.zero, new Vector2(140, rowHeight - 8), 26, FontStyle.Bold, WHITE_TEXT);

        return new RowWidgets
        {
            rect = rt,
            rowBg = rowBg,
            avatarBg = aImg,
            avatarLetter = letter,
            rankText = rank,
            nameText = nameText,
            scoreBadge = bImg,
            scoreText = scoreText,
            baseAnchored = rt.anchoredPosition
        };
    }

    // ============================================================ data
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

        LeaderboardManager.Instance.TryAddScore(playerName, score, coins);

        if (deathCoroutine != null) StopCoroutine(deathCoroutine);
        deathCoroutine = StartCoroutine(ShowAfterDelay());
    }

    IEnumerator ShowAfterDelay()
    {
        // Wait so the "You Lost" animation can play first.
        float t = 0f;
        while (t < delayBeforeShow)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        ShowLeaderboard();
    }

    public void ShowLeaderboard()
    {
        root.SetActive(true);

        var entries = LeaderboardManager.Instance.GetEntries(currentSort);
        string currentPlayer = LeaderboardManager.Instance.GetSavedPlayerName();
        bool byCoins = currentSort == LeaderboardManager.SortMode.ByCoins;

        // Top 3 podium
        for (int i = 0; i < 3; i++)
        {
            var pc = podium[i];
            if (i < entries.Count)
            {
                var entry = entries[i];
                bool isYou = entry.playerName == currentPlayer;
                pc.isCurrentPlayer = isYou;

                pc.cardBg.color = CARD_WHITE;
                pc.avatarRing.color = ORANGE;
                pc.rankBadgeBg.color = ORANGE;
                pc.rankBadgeText.text = (i + 1).ToString();

                pc.avatarLetter.text = GetIconLetter(entry.icon);
                pc.nameText.text = entry.playerName.ToUpper();
                pc.scoreText.text = byCoins ? "$" + entry.coins : FormatScore(entry.score);
            }
            else
            {
                pc.isCurrentPlayer = false;
                pc.cardBg.color = new Color(1f, 1f, 1f, 0.5f);
                pc.avatarLetter.text = "?";
                pc.nameText.text = "—";
                pc.scoreText.text = "—";
            }
        }

        // Bottom rows: ranks 4..10
        for (int i = 0; i < bottomRows.Count; i++)
        {
            var w = bottomRows[i];
            int rankIndex = i + 3; // entries[3] is rank 4
            if (rankIndex < entries.Count)
            {
                var entry = entries[rankIndex];
                bool isYou = entry.playerName == currentPlayer;
                w.isCurrentPlayer = isYou;

                w.rowBg.color = isYou ? new Color(1f, 0.95f, 0.5f) : CARD_WHITE;
                w.rankText.text = (rankIndex + 1) + ".";
                w.rankText.color = BROWN_TEXT;
                w.avatarBg.color = ORANGE_LIGHT;
                w.avatarLetter.text = GetIconLetter(entry.icon);
                w.nameText.text = entry.playerName.ToUpper();
                w.nameText.color = BROWN_TEXT;
                w.nameText.fontStyle = isYou ? FontStyle.Bold : FontStyle.Bold;
                w.scoreBadge.color = ORANGE;
                w.scoreText.text = byCoins ? "$" + entry.coins : FormatScore(entry.score);
            }
            else
            {
                w.isCurrentPlayer = false;
                w.rowBg.color = new Color(1f, 1f, 1f, 0.5f);
                w.rankText.text = (rankIndex + 1) + ".";
                w.rankText.color = new Color(0.55f, 0.45f, 0.40f);
                w.avatarBg.color = new Color(1f, 0.85f, 0.6f, 0.5f);
                w.avatarLetter.text = "?";
                w.nameText.text = "—";
                w.nameText.color = new Color(0.55f, 0.45f, 0.40f);
                w.scoreBadge.color = new Color(0.97f, 0.62f, 0.27f, 0.5f);
                w.scoreText.text = "—";
            }
        }

        if (entranceCoroutine != null) StopCoroutine(entranceCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        entranceCoroutine = StartCoroutine(EntranceAnimation());
    }

    string GetIconLetter(string iconKey)
    {
        if (string.IsNullOrEmpty(iconKey)) return "?";
        if (FRUIT_DISPLAY.TryGetValue(iconKey, out var d)) return d;
        return iconKey.Length > 0 ? iconKey.Substring(0, 1).ToUpper() : "?";
    }

    static string FormatScore(int score)
    {
        if (score >= 1_000_000) return (score / 1_000_000f).ToString("0.0") + "M";
        if (score >= 1_000)     return (score / 1_000f).ToString("0.0") + "K";
        return score.ToString();
    }

    // ============================================================ animation
    IEnumerator EntranceAnimation()
    {
        rootGroup.alpha = 0f;

        // initial offsets
        for (int i = 0; i < 3; i++)
        {
            var p = podium[i];
            p.rect.localScale = new Vector3(0.4f, 0.4f, 1f);
            p.rect.anchoredPosition = p.baseAnchored + new Vector2(0, -100f);
        }
        for (int i = 0; i < bottomRows.Count; i++)
        {
            var w = bottomRows[i];
            w.rect.localScale = new Vector3(0.92f, 0.92f, 1f);
            w.rect.anchoredPosition = w.baseAnchored + new Vector2(420f, 0f);
        }

        // fade in dim
        float t = 0f;
        while (t < 0.22f)
        {
            t += Time.unscaledDeltaTime;
            rootGroup.alpha = Mathf.Clamp01(t / 0.22f);
            yield return null;
        }
        rootGroup.alpha = 1f;

        // podium pop-in (centered first, then sides)
        int[] order = { 0, 1, 2 };
        float popDuration = 0.55f;
        float stagger = 0.12f;
        float[] starts = { 0f, stagger, stagger * 2 };
        float total = popDuration + stagger * 2;
        float elapsed = 0f;
        while (elapsed < total)
        {
            elapsed += Time.unscaledDeltaTime;
            for (int k = 0; k < 3; k++)
            {
                int i = order[k];
                var p = podium[i];
                float local = Mathf.Clamp01((elapsed - starts[k]) / popDuration);
                float eased = EaseOutBack(local);
                float s = Mathf.LerpUnclamped(0.4f, 1f, eased);
                p.rect.localScale = new Vector3(s, s, 1f);
                p.rect.anchoredPosition = Vector2.LerpUnclamped(p.baseAnchored + new Vector2(0, -100f), p.baseAnchored, eased);
            }
            yield return null;
        }
        for (int i = 0; i < 3; i++)
        {
            podium[i].rect.localScale = Vector3.one;
            podium[i].rect.anchoredPosition = podium[i].baseAnchored;
        }

        // rows slide in from right, staggered
        float rowDur = 0.35f;
        float rowStagger = 0.05f;
        float rowTotal = rowDur + rowStagger * bottomRows.Count;
        float rowElapsed = 0f;
        while (rowElapsed < rowTotal)
        {
            rowElapsed += Time.unscaledDeltaTime;
            for (int i = 0; i < bottomRows.Count; i++)
            {
                var w = bottomRows[i];
                float local = Mathf.Clamp01((rowElapsed - i * rowStagger) / rowDur);
                float eased = EaseOutCubic(local);
                w.rect.anchoredPosition = Vector2.LerpUnclamped(w.baseAnchored + new Vector2(420f, 0f), w.baseAnchored, eased);
                float s = Mathf.LerpUnclamped(0.92f, 1f, eased);
                w.rect.localScale = new Vector3(s, s, 1f);
            }
            yield return null;
        }
        for (int i = 0; i < bottomRows.Count; i++)
        {
            bottomRows[i].rect.localScale = Vector3.one;
            bottomRows[i].rect.anchoredPosition = bottomRows[i].baseAnchored;
        }

        // continuous gentle bob for top 3 + pulse for current player
        pulseCoroutine = StartCoroutine(IdleAnimation());
    }

    IEnumerator IdleAnimation()
    {
        while (root != null && root.activeSelf)
        {
            float t = Time.unscaledTime;
            // Top 3 bob (different phase per slot)
            for (int i = 0; i < 3; i++)
            {
                var p = podium[i];
                float bob = Mathf.Sin(t * 1.4f + i * 0.7f) * 4f;
                p.rect.anchoredPosition = p.baseAnchored + new Vector2(0, bob);

                // Subtle scale pulse for #1 (more eye-catching)
                if (i == 0)
                {
                    float pulse = 1f + Mathf.Sin(t * 2.2f) * 0.015f;
                    p.rect.localScale = new Vector3(pulse, pulse, 1f);
                }
            }

            // current-player highlight pulse
            float playerPulse = 0.5f + Mathf.Sin(t * 3f) * 0.5f;
            for (int i = 0; i < bottomRows.Count; i++)
            {
                var w = bottomRows[i];
                if (!w.isCurrentPlayer) continue;
                float s = Mathf.Lerp(1.00f, 1.04f, playerPulse);
                w.rect.localScale = new Vector3(s, s, 1f);
                w.rowBg.color = Color.Lerp(new Color(1f, 0.95f, 0.5f), new Color(1f, 0.85f, 0.3f), playerPulse);
            }
            yield return null;
        }
    }

    static float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float k = t - 1f;
        return 1f + c3 * k * k * k + c1 * k * k;
    }

    static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float k = 1f - t;
        return 1f - k * k * k;
    }

    public void Hide()
    {
        if (entranceCoroutine != null) StopCoroutine(entranceCoroutine);
        if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
        if (root != null) root.SetActive(false);
    }
}
