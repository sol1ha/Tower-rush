using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Updates current session max score on the <see cref="GameObject"/>'s <see cref="Text"/> component
/// </summary>
public class ActualScoreDisplay : MonoBehaviour
{
    private Text text;
    private Transform player;

    private int maxAltura;

    public static int CurrentScore { get; private set; }

    void Start()
    {
        maxAltura = 0;
        CurrentScore = 0;
        text = GetComponent<Text>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return;
            }
        }

        if(player.position.y > maxAltura)
        {
            maxAltura = (int)player.position.y;
            Debug.Log("New Max Height: " + maxAltura);
        }
        
        // Update the score variable regardless of whether the UI text is working
        CurrentScore = HighScoreSet.gameScore + maxAltura;
        
        if (text != null)
        {
            text.text = "Score: " + CurrentScore;
        }

        if (CurrentScore % 10 == 0 && CurrentScore > 0)
        {
            // Log every 10 points to avoid spam but show progress
            Debug.Log("Current Score: " + CurrentScore);
        }
    }
}
