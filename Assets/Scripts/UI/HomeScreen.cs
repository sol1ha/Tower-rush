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

    [Header("Music")]
    [Tooltip("Music that plays on the home/menu screen.")]
    public BackgroundMusic menuMusic;

    [Tooltip("Music that plays during gameplay (after Play is pressed).")]
    public BackgroundMusic gameMusic;

    [Tooltip("Seconds to crossfade between menu and game music.")]
    public float musicFadeSeconds = 0.6f;

    [Header("Auto-start countdown")]
    [Tooltip("If true, a visible animated countdown is auto-attached to the home panel and triggers StartGame() at zero.")]
    public bool autoCreateCountdown = true;
    [Tooltip("Seconds before the menu auto-starts the game.")]
    public float autoStartSeconds = 30f;

    private HomeMenuCountdown countdown;

    void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(StartGame);
            playButton.onClick.AddListener(StartGame);
        }

        if (autoCreateCountdown)
        {
            GameObject host = homePanel != null ? homePanel : gameObject;
            countdown = host.GetComponentInChildren<HomeMenuCountdown>(true);
            if (countdown == null) countdown = host.AddComponent<HomeMenuCountdown>();
            countdown.autoStartSeconds = autoStartSeconds;
        }
    }

    void Start()
    {
        ShowHome();
    }

    public void ShowHome()
    {
        // Always keep time running. Hiding gameplay objects via SetActive(false)
        // is enough to keep the scene quiet during the menu, and avoids the
        // catastrophic failure mode where Time.timeScale gets stuck at 0
        // (no bouncing, no audio).
        Time.timeScale = 1f;

        if (homePanel != null) homePanel.SetActive(true);

        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(false);
        }

        if (GameManager.instance != null) GameManager.instance.play = false;

        // (Re)enable the countdown when returning to the home menu so it
        // restarts from full time.
        if (countdown != null)
        {
            countdown.autoStartSeconds = autoStartSeconds;
            countdown.enabled = true;
            countdown.gameObject.SetActive(true);
        }

        try { if (gameMusic != null) gameMusic.Stop(musicFadeSeconds); } catch { }
        try { if (menuMusic != null) menuMusic.Play(); } catch { }
    }

    public void StartGame()
    {
        Time.timeScale = 1f;

        if (homePanel != null) homePanel.SetActive(false);

        // Activate gameplay objects FIRST so GameManager (often listed in
        // hideUntilPlay) gets its Awake run and registers GameManager.instance.
        foreach (var go in hideUntilPlay)
        {
            if (go != null) go.SetActive(true);
        }

        // Now set play=true AFTER GameManager has had a chance to Awake.
        // GameManager.Awake() forces play=false, so this must come after the loop.
        if (GameManager.instance != null) GameManager.instance.play = true;
        else GameManager.StartGame();

        try { if (menuMusic != null) menuMusic.Stop(musicFadeSeconds); } catch { }
        try { if (gameMusic != null) gameMusic.Play(); } catch { }
    }
}
