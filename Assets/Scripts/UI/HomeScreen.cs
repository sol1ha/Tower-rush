using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : MonoBehaviour
{
    [Header("Home page UI")]
    [Tooltip("Root GameObject of your 'Jump or Die' home page (background + Play button).")]
    public GameObject homePanel;

    [Tooltip("The Play button. When clicked, StartGame() is called.")]
    public Button playButton;

    [Header("Things to hide until Play is pressed")]
    [Tooltip("Drag every gameplay GameObject here: player, platforms parent, HUD, bullet spawner, laser, etc.")]
    public List<GameObject> hideUntilPlay = new List<GameObject>();

    [Tooltip("If true, Time.timeScale is 0 on the home page so nothing ticks.")]
    public bool freezeTimeOnHome = true;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartGame);
            playButton.onClick.AddListener(StartGame);
        }
    }

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        if (homePanel != null) homePanel.SetActive(true);

        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(false);
        }

        if (GameManager.instance != null) GameManager.instance.play = false;
        if (freezeTimeOnHome) Time.timeScale = 0f;
    }

    public void StartGame()
    {
        if (homePanel != null) homePanel.SetActive(false);

        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(true);
        }

        Time.timeScale = 1f;

        if (GameManager.instance != null) GameManager.instance.play = true;
        else GameManager.StartGame();
    }
}
