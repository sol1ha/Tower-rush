using UnityEngine;
using TMPro;
using System.Collections.Generic;

/// Simple local leaderboard that tracks top 10 scores using PlayerPrefs.
/// When Luxodd plugin is installed, replace LoadScores/SaveScore with
/// SendLeaderboardRequestCommand / SendLevelEndRequestCommand calls.
///
/// In Unity setup:
/// 1. Create a Panel under HUD Canvas → name it "LeaderboardPanel"
/// 2. Add child TextMeshPro: "LEADERBOARD" (large, top)
/// 3. Add child TextMeshPro: leaderboardContent (multiline, center, left-aligned)
/// 4. Disable the panel by default
/// 5. Drag references into this script's Inspector fields
public class LeaderboardScreen : MonoBehaviour
{
    public static LeaderboardScreen Instance;

    [Header("UI References")]
    public GameObject panel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI contentText;

    [Header("Settings")]
    public int maxEntries = 10;

    private const string PrefsKey = "TowerRush_Leaderboard";

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this);

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(int currentScore)
    {
        SaveScore(currentScore);

        if (panel != null)
            panel.SetActive(true);

        List<int> scores = LoadScores();
        string display = "";
        for (int i = 0; i < scores.Count; i++)
        {
            string marker = (scores[i] == currentScore) ? " ◄ YOU" : "";
            display += "#" + (i + 1) + "   " + scores[i].ToString("D6") + marker + "\n";
        }

        if (contentText != null)
            contentText.text = display;
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    void SaveScore(int score)
    {
        List<int> scores = LoadScores();
        scores.Add(score);
        scores.Sort((a, b) => b.CompareTo(a)); // Descending

        if (scores.Count > maxEntries)
            scores.RemoveRange(maxEntries, scores.Count - maxEntries);

        // Store as comma-separated string
        string data = string.Join(",", scores);
        PlayerPrefs.SetString(PrefsKey, data);
        PlayerPrefs.Save();
    }

    List<int> LoadScores()
    {
        List<int> scores = new List<int>();
        string data = PlayerPrefs.GetString(PrefsKey, "");

        if (string.IsNullOrEmpty(data)) return scores;

        foreach (string s in data.Split(','))
        {
            if (int.TryParse(s, out int val))
                scores.Add(val);
        }

        scores.Sort((a, b) => b.CompareTo(a));
        return scores;
    }
}
