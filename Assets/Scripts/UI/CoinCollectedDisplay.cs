using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the total coins collected during the current session.
/// Attach this script to a Text UI element to show coin count.
/// </summary>
public class CoinCollectedDisplay : MonoBehaviour
{
    private Text text;

    void Start()
    {
        // First look for the specifically named text object in children
        Transform t = transform.Find("coinAmountTxt");
        if (t != null)
        {
            text = t.GetComponent<Text>();
        }
        
        // Fallback to getting any text component on this object
        if (text == null)
        {
            text = GetComponent<Text>();
        }

        if (text == null)
        {
            Debug.LogError("CoinCollectedDisplay: No Text component found. Please ensure 'coinAmountTxt' is a child or this object has a Text component.");
        }
        else
        {
            // Initially show the permanent total coins (Home Page state)
            text.text = HighScoreSet.PersistentCoins.ToString();
        }
    }

    void Update()
    {
        if (text != null)
        {
            // Always show coins collected this session.
            // gameScore resets to 0 when the scene reloads (new game start),
            // so after death the last run's coins remain visible until the player restarts.
            text.text = HighScoreSet.gameScore.ToString();
        }
    }
}
