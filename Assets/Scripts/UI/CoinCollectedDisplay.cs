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
        text = GetComponent<Text>();
        if (text == null)
        {
            Debug.LogError("CoinCollectedDisplay: No Text component found on this GameObject. Please attach this script to a UI Text element.");
            enabled = false;
            return;
        }
        Debug.Log("CoinCollectedDisplay initialized on: " + gameObject.name);
    }

    void Update()
    {
        if (text != null)
        {
            int coins = HighScoreSet.gameScore;
            text.text = "Coins: " + coins;
        }
    }
}
