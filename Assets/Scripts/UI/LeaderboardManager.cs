using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    public enum SortMode { ByScore, ByCoins }

    private const string LEADERBOARD_KEY_LEGACY = "InfinityRush_Leaderboard_v2";
    private const string LEADERBOARD_KEY = "InfinityRush_Leaderboard_v3"; // bumped: dual-list payload
    private const string CURRENT_PLAYER_KEY = "InfinityRush_CurrentPlayer";
    private const int MAX_ENTRIES = 10;

    public static readonly string[] ProfileIcons = {
        "apple", "cherry", "grape", "lemon", "orange",
        "peach", "pear", "strawberry", "watermelon", "banana"
    };

    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;
        public int coins;
        public string icon;

        public LeaderboardEntry(string name, int score, int coins, string icon)
        {
            this.playerName = name;
            this.score = score;
            this.coins = coins;
            this.icon = icon;
        }
    }

    [System.Serializable]
    private class LeaderboardData
    {
        // One ranking sorted by score (best score per player), one by coins
        // (best coin total per player). Entries in each are independent.
        public List<LeaderboardEntry> byScore = new List<LeaderboardEntry>();
        public List<LeaderboardEntry> byCoins = new List<LeaderboardEntry>();
    }

    [System.Serializable]
    private class LegacyLeaderboardData
    {
        public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
    }

    [System.Serializable]
    private class PlayerCard
    {
        public string playerName;
        public string icon;
    }

    [System.Serializable]
    private class PlayerCardList
    {
        public List<PlayerCard> cards = new List<PlayerCard>();
    }

    private LeaderboardData leaderboard;

    void Awake()
    {
        Instance = this;
        LoadLeaderboard();
    }

    public string GetSavedPlayerName() => PlayerPrefs.GetString(CURRENT_PLAYER_KEY, "");

    public void SetCurrentPlayer(string playerName)
    {
        PlayerPrefs.SetString(CURRENT_PLAYER_KEY, playerName);
        PlayerPrefs.Save();

        if (!HasPlayerCard(playerName))
            SavePlayerCard(playerName);
    }

    /// <summary>
    /// Inserts/updates the player's best entry on BOTH rankings independently
    /// (best score in byScore, best coins in byCoins). Returns true if either
    /// list was changed.
    /// </summary>
    public bool TryAddScore(string playerName, int score, int coins)
    {
        if (string.IsNullOrEmpty(playerName)) return false;

        string icon = GetPlayerIcon(playerName);
        bool changedScore = UpsertBest(leaderboard.byScore, playerName, score, coins, icon, byScore: true);
        bool changedCoins = UpsertBest(leaderboard.byCoins, playerName, score, coins, icon, byScore: false);

        if (changedScore || changedCoins)
        {
            SaveLeaderboard();
            return true;
        }
        return false;
    }

    /// <summary>
    /// In <paramref name="list"/> (the score-ranked or coin-ranked list), update
    /// the player's row if their incoming metric is higher than what's there;
    /// otherwise leave it alone. Insert a new row if the player isn't present.
    /// </summary>
    bool UpsertBest(List<LeaderboardEntry> list, string playerName, int score, int coins, string icon, bool byScore)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].playerName == playerName)
            {
                int existing = byScore ? list[i].score : list[i].coins;
                int incoming = byScore ? score : coins;
                if (incoming > existing)
                {
                    list[i] = new LeaderboardEntry(playerName, score, coins, icon);
                    SortAndTrim(list, byScore);
                    return true;
                }
                return false;
            }
        }

        list.Add(new LeaderboardEntry(playerName, score, coins, icon));
        SortAndTrim(list, byScore);
        return true;
    }

    /// <summary>
    /// Returns a copy of the leaderboard sorted by the requested metric.
    /// </summary>
    public List<LeaderboardEntry> GetEntries(SortMode mode = SortMode.ByScore)
    {
        var src = mode == SortMode.ByCoins ? leaderboard.byCoins : leaderboard.byScore;
        return new List<LeaderboardEntry>(src);
    }

    /// <summary>Backward-compatible default — score ranking.</summary>
    public List<LeaderboardEntry> GetEntries() => GetEntries(SortMode.ByScore);

    string GetPlayerIcon(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        if (!string.IsNullOrEmpty(cardsJson))
        {
            PlayerCardList cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);
            foreach (var card in cards.cards)
                if (card.playerName == playerName) return card.icon;
        }
        return ProfileIcons[Random.Range(0, ProfileIcons.Length)];
    }

    bool HasPlayerCard(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        if (string.IsNullOrEmpty(cardsJson)) return false;
        PlayerCardList cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);
        foreach (var card in cards.cards) if (card.playerName == playerName) return true;
        return false;
    }

    void SavePlayerCard(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        PlayerCardList cards;
        if (string.IsNullOrEmpty(cardsJson)) cards = new PlayerCardList();
        else cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);

        var newCard = new PlayerCard();
        newCard.playerName = playerName;
        newCard.icon = ProfileIcons[Random.Range(0, ProfileIcons.Length)];
        cards.cards.Add(newCard);

        PlayerPrefs.SetString("InfinityRush_PlayerCards", JsonUtility.ToJson(cards));
        PlayerPrefs.Save();
    }

    void SortAndTrim(List<LeaderboardEntry> list, bool byScore)
    {
        if (byScore) list.Sort((a, b) => b.score.CompareTo(a.score));
        else         list.Sort((a, b) => b.coins.CompareTo(a.coins));
        if (list.Count > MAX_ENTRIES) list.RemoveRange(MAX_ENTRIES, list.Count - MAX_ENTRIES);
    }

    void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
        if (!string.IsNullOrEmpty(json))
        {
            leaderboard = JsonUtility.FromJson<LeaderboardData>(json);
            if (leaderboard.byScore == null) leaderboard.byScore = new List<LeaderboardEntry>();
            if (leaderboard.byCoins == null) leaderboard.byCoins = new List<LeaderboardEntry>();
            return;
        }

        // Migrate legacy single-list data -> new dual-list format.
        leaderboard = new LeaderboardData();
        string legacyJson = PlayerPrefs.GetString(LEADERBOARD_KEY_LEGACY, "");
        if (!string.IsNullOrEmpty(legacyJson))
        {
            var legacy = JsonUtility.FromJson<LegacyLeaderboardData>(legacyJson);
            if (legacy != null && legacy.entries != null)
            {
                foreach (var e in legacy.entries)
                {
                    leaderboard.byScore.Add(new LeaderboardEntry(e.playerName, e.score, e.coins, e.icon));
                    leaderboard.byCoins.Add(new LeaderboardEntry(e.playerName, e.score, e.coins, e.icon));
                }
                SortAndTrim(leaderboard.byScore, byScore: true);
                SortAndTrim(leaderboard.byCoins, byScore: false);
                SaveLeaderboard();
            }
        }
    }

    void SaveLeaderboard()
    {
        PlayerPrefs.SetString(LEADERBOARD_KEY, JsonUtility.ToJson(leaderboard));
        PlayerPrefs.Save();
    }
}
