using UnityEngine;
using System.Collections.Generic;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance;

    private const string LEADERBOARD_KEY = "InfinityRush_Leaderboard_v2";
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

    public string GetSavedPlayerName()
    {
        return PlayerPrefs.GetString(CURRENT_PLAYER_KEY, "");
    }

    public void SetCurrentPlayer(string playerName)
    {
        PlayerPrefs.SetString(CURRENT_PLAYER_KEY, playerName);
        PlayerPrefs.Save();

        if (!HasPlayerCard(playerName))
            SavePlayerCard(playerName);
    }

    public bool TryAddScore(string playerName, int score, int coins)
    {
        if (string.IsNullOrEmpty(playerName)) return false;

        string icon = GetPlayerIcon(playerName);
        LeaderboardEntry newEntry = new LeaderboardEntry(playerName, score, coins, icon);

        // Only keep best score per player — update if higher, skip if lower
        for (int i = 0; i < leaderboard.entries.Count; i++)
        {
            if (leaderboard.entries[i].playerName == playerName)
            {
                if (score > leaderboard.entries[i].score)
                {
                    leaderboard.entries[i] = newEntry;
                    SortAndTrim();
                    SaveLeaderboard();
                    return true;
                }
                return false;
            }
        }

        leaderboard.entries.Add(newEntry);
        SortAndTrim();
        SaveLeaderboard();
        return true;
    }

    public List<LeaderboardEntry> GetEntries()
    {
        return new List<LeaderboardEntry>(leaderboard.entries);
    }

    string GetPlayerIcon(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        if (!string.IsNullOrEmpty(cardsJson))
        {
            PlayerCardList cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);
            foreach (var card in cards.cards)
            {
                if (card.playerName == playerName)
                    return card.icon;
            }
        }
        return ProfileIcons[Random.Range(0, ProfileIcons.Length)];
    }

    bool HasPlayerCard(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        if (string.IsNullOrEmpty(cardsJson)) return false;

        PlayerCardList cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);
        foreach (var card in cards.cards)
        {
            if (card.playerName == playerName) return true;
        }
        return false;
    }

    void SavePlayerCard(string playerName)
    {
        string cardsJson = PlayerPrefs.GetString("InfinityRush_PlayerCards", "");
        PlayerCardList cards;
        if (string.IsNullOrEmpty(cardsJson))
            cards = new PlayerCardList();
        else
            cards = JsonUtility.FromJson<PlayerCardList>(cardsJson);

        PlayerCard newCard = new PlayerCard();
        newCard.playerName = playerName;
        newCard.icon = ProfileIcons[Random.Range(0, ProfileIcons.Length)];
        cards.cards.Add(newCard);

        PlayerPrefs.SetString("InfinityRush_PlayerCards", JsonUtility.ToJson(cards));
        PlayerPrefs.Save();
    }

    void SortAndTrim()
    {
        leaderboard.entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (leaderboard.entries.Count > MAX_ENTRIES)
            leaderboard.entries.RemoveRange(MAX_ENTRIES, leaderboard.entries.Count - MAX_ENTRIES);
    }

    void LoadLeaderboard()
    {
        string json = PlayerPrefs.GetString(LEADERBOARD_KEY, "");
        if (string.IsNullOrEmpty(json))
            leaderboard = new LeaderboardData();
        else
            leaderboard = JsonUtility.FromJson<LeaderboardData>(json);
    }

    void SaveLeaderboard()
    {
        PlayerPrefs.SetString(LEADERBOARD_KEY, JsonUtility.ToJson(leaderboard));
        PlayerPrefs.Save();
    }
}
