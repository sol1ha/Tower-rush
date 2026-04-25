using System;
using UnityEngine;

#if LUXODD_SDK
using Luxodd.Game.Scripts.Network;
#endif

public class InGameTransactionController : MonoBehaviour
{
    public static InGameTransactionController Instance { get; private set; }

#if LUXODD_SDK
    [Header("Plugin references")]
    [SerializeField] private WebSocketService _webSocketService;
    [SerializeField] private WebSocketCommandHandler _webSocketCommandHandler;
#endif

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

#if LUXODD_SDK
    private bool _allowRestart;
#endif

    // Call this from your gameplay when the run ends (0 lives, timer expired, etc.)
    public void OnGameOver(bool allowContinue, bool allowRestart)
    {
#if LUXODD_SDK
        _allowRestart = allowRestart;

        // 1) Freeze/pause gameplay first (VERY IMPORTANT)
        PauseGameplay();

        // 2) Show Continue first if allowed, then Restart on End
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

        // If neither is supported, finish the session normally
        EndSessionAndReturnToSystem();
#else
        Debug.LogWarning("Luxodd SDK is not enabled. Game over triggered, but transactions skipped.");
#endif
    }


#if LUXODD_SDK
    private void ShowContinuePopup()
    {
        if (_webSocketService != null)
        {
            _webSocketService.SendSessionOptionContinue(OnContinuePopupResult);
        }
        else
        {
            Debug.LogError("WebSocketService is missing!");
            EndSessionAndReturnToSystem();
        }
    }

    private void OnContinuePopupResult(SessionOptionAction action)
    {
        Debug.Log($"[Continue Popup] Player choice: {action}");

        switch (action)
        {
            case SessionOptionAction.Continue:
                ResumeGameplayWithContinueBonus();
                break;

            case SessionOptionAction.End:
                // Player declined to continue — offer Restart if allowed, otherwise exit
                if (_allowRestart)
                    ShowRestartPopup();
                else
                    EndSessionAndReturnToSystem();
                break;

            default:
                EndSessionAndReturnToSystem();
                break;
        }
    }

    private void ShowRestartPopup()
    {
        // IMPORTANT:
        // Before showing Restart popup you MUST send session results.
        if (_webSocketCommandHandler != null)
        {
            _webSocketCommandHandler.SendLevelEndRequestCommand(() =>
            {
                // After results are safely sent, show Restart popup
                if (_webSocketService != null)
                    _webSocketService.SendSessionOptionRestart(OnRestartPopupResult);
            });
        }
    }

    private void OnRestartPopupResult(SessionOptionAction action)
    {
        Debug.Log($"[Restart Popup] Player choice: {action}");

        // If player selects Restart, the system starts a new session automatically.
        if (action == SessionOptionAction.End)
        {
            if (_webSocketService != null)
                _webSocketService.BackToSystem();
        }
    }

    private void EndSessionAndReturnToSystem()
    {
        // Send results first, then return control to system UI/platform
        if (_webSocketCommandHandler != null)
        {
            _webSocketCommandHandler.SendLevelEndRequestCommand(() =>
            {
                if (_webSocketService != null)
                    _webSocketService.BackToSystem();
            });
        }
    }
#endif


    // -------------------------
    // Game-specific helpers
    // -------------------------

#if LUXODD_SDK
    private void PauseGameplay()
    {
        Time.timeScale = 0f;
        GameManager.instance.play = false;
    }

    private void ResumeGameplayWithContinueBonus()
    {
        Time.timeScale = 1f;
        GameManager.instance.play = true;

        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.RestoreForContinue();
        }

        Debug.Log("Continuing the same session: restoring gameplay state...");
    }
#endif
}
