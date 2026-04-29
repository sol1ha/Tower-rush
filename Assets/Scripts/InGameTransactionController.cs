using System;
using UnityEngine;
using Luxodd.Game.Scripts.Network;
using Luxodd.Game.Scripts.Network.CommandHandler;

/// <summary>
/// Bridges Tower Rush's local game-over flow into Luxodd's in-game-transaction
/// API. On game over the local leaderboard is shown first; once the player
/// dismisses it, this controller fires the system Continue / Restart popups.
/// Survives scene loads (DontDestroyOnLoad) so the WebSocket reference stays
/// valid across reloads.
/// </summary>
public class InGameTransactionController : MonoBehaviour
{
    public static InGameTransactionController Instance { get; private set; }

    [Header("Plugin references — auto-found in scene if left empty")]
    [SerializeField] private WebSocketService _webSocketService;
    [SerializeField] private WebSocketCommandHandler _webSocketCommandHandler;

    private bool _allowRestart;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        ResolvePluginRefs();
    }

    void ResolvePluginRefs()
    {
        if (_webSocketService == null)
            _webSocketService = FindAnyObjectByType<WebSocketService>(FindObjectsInactive.Include);
        if (_webSocketCommandHandler == null)
            _webSocketCommandHandler = FindAnyObjectByType<WebSocketCommandHandler>(FindObjectsInactive.Include);
    }

    /// <summary>
    /// Call when the player has died and any local UI (leaderboard, "you lost"
    /// banner, etc.) has finished. Pauses gameplay and routes through the
    /// Luxodd Continue popup if allowed, then Restart popup, then exit.
    /// </summary>
    public void OnGameOver(bool allowContinue, bool allowRestart)
    {
        ResolvePluginRefs();
        _allowRestart = allowRestart;

        PauseGameplay();

        if (allowContinue)
        {
            ShowContinuePopup();
            return;
        }

        if (allowRestart)
        {
            ShowRestartPopup();
            return;
        }

        EndSessionAndReturnToSystem();
    }

    private void ShowContinuePopup()
    {
        if (_webSocketService != null)
        {
            _webSocketService.SendSessionOptionContinue(OnContinuePopupResult);
        }
        else
        {
            Debug.LogWarning("[Luxodd] WebSocketService missing — skipping Continue popup.");
            if (_allowRestart) ShowRestartPopup();
            else EndSessionAndReturnToSystem();
        }
    }

    private void OnContinuePopupResult(SessionOptionAction action)
    {
        Debug.Log($"[Luxodd] Continue popup result: {action}");
        switch (action)
        {
            case SessionOptionAction.Continue:
                ResumeGameplayWithContinueBonus();
                break;
            case SessionOptionAction.End:
                if (_allowRestart) ShowRestartPopup();
                else EndSessionAndReturnToSystem();
                break;
            default:
                EndSessionAndReturnToSystem();
                break;
        }
    }

    private void ShowRestartPopup()
    {
        if (_webSocketCommandHandler != null)
        {
            // Restart finalises the session, so send level-end results FIRST.
            int score = (HighScoreSet.gameScore + Mathf.Max(0, GetMaxPlayerHeight()));
            _webSocketCommandHandler.SendLevelEndRequestCommand(
                level: 1,
                score: score,
                onSuccessCallback: () =>
                {
                    if (_webSocketService != null)
                        _webSocketService.SendSessionOptionRestart(OnRestartPopupResult);
                },
                onFailureCallback: (code, msg) =>
                {
                    Debug.LogWarning($"[Luxodd] Level-end failed before restart: {code} {msg}");
                    if (_webSocketService != null)
                        _webSocketService.SendSessionOptionRestart(OnRestartPopupResult);
                });
        }
        else
        {
            Debug.LogWarning("[Luxodd] WebSocketCommandHandler missing — skipping Restart popup.");
            EndSessionAndReturnToSystem();
        }
    }

    private void OnRestartPopupResult(SessionOptionAction action)
    {
        Debug.Log($"[Luxodd] Restart popup result: {action}");
        // System auto-creates a new session on Restart; we only handle End here.
        if (action == SessionOptionAction.End && _webSocketService != null)
            _webSocketService.BackToSystem();
    }

    private void EndSessionAndReturnToSystem()
    {
        if (_webSocketCommandHandler != null)
        {
            int score = (HighScoreSet.gameScore + Mathf.Max(0, GetMaxPlayerHeight()));
            _webSocketCommandHandler.SendLevelEndRequestCommand(
                level: 1,
                score: score,
                onSuccessCallback: () => { if (_webSocketService != null) _webSocketService.BackToSystem(); },
                onFailureCallback: (code, msg) =>
                {
                    Debug.LogWarning($"[Luxodd] Final level-end failed: {code} {msg}");
                    if (_webSocketService != null) _webSocketService.BackToSystem();
                });
        }
        else if (_webSocketService != null)
        {
            _webSocketService.BackToSystem();
        }
    }

    int GetMaxPlayerHeight()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        return playerObj != null ? Mathf.Max(0, (int)playerObj.transform.position.y) : 0;
    }

    // -------- gameplay helpers --------

    private void PauseGameplay()
    {
        Time.timeScale = 0f;
        if (GameManager.instance != null) GameManager.instance.play = false;
    }

    private void ResumeGameplayWithContinueBonus()
    {
        Time.timeScale = 1f;
        if (GameManager.instance != null) GameManager.instance.play = true;

        var playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null) playerHealth.RestoreForContinue();

        Debug.Log("[Luxodd] Continuing same session: restored gameplay state.");
    }
}
