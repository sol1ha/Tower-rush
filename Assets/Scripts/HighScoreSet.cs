using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class used to set highscores to the local save system, also tries to update it to the Kongregate's servers
/// </summary>
public class HighScoreSet : MonoBehaviour
{
    public static int gameScore;
    
    public static int PersistentCoins
    {
        get { return PlayerPrefs.GetInt(Constants.Money_Pref, 0); }
        set { PlayerPrefs.SetInt(Constants.Money_Pref, value); }
    }

    void Start()
    {
        // Reset session coins every time a new game/scene starts
        gameScore = 0;
        
        if (!PlayerPrefs.HasKey(Constants.HighScore_Pref))
        {
            PlayerPrefs.SetInt(Constants.HighScore_Pref, 0);
        }
    }

    public static void SetHighscore(int score)
    {
        int actual = PlayerPrefs.GetInt(Constants.HighScore_Pref);
        if(score > actual)
        {
            PlayerPrefs.SetInt(Constants.HighScore_Pref, score);
        }
        // Kongregate submission removed (deprecated)
    }

    /// <summary>
    /// Adds session coins (doubled) to persistent savings.
    /// Call this when the round is officially over.
    /// </summary>
    public static void FinalizeSessionCoins()
    {
        // Add collected coins 1-to-1 to persistent total
        PersistentCoins += gameScore;
        Debug.Log($"Session coins: {gameScore}. Added to total. New Total: {PersistentCoins}");
        
        // Reset after finalizing so it doesn't double-add if called multiple times
        gameScore = 0;
    }
}
