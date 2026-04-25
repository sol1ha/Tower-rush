using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reloads the scene after a brief delay on <see cref="PlayerKillLimit.PlayerKill"/> event
/// </summary>
public class ReloadScene : MonoBehaviour
{
    public static bool CanReloadAfterDeath = true;
    private bool firecd;
    private float cd;

    void Start()
    {
        firecd = false;
        CanReloadAfterDeath = true;
        PlayerKillLimit.PlayerKill += PlayerKillLimit_PlayerKill;
    }
    private void PlayerKillLimit_PlayerKill(object sender, System.EventArgs e)
    {
#if LUXODD_SDK
        if (InGameTransactionController.Instance != null)
        {
            InGameTransactionController.Instance.OnGameOver(allowContinue: true, allowRestart: true);
            firecd = false;
            return;
        }
#endif
        cd = Time.time + 1f;
        firecd = true;
    }
    void Update()
    {
        if (firecd && CanReloadAfterDeath && Time.time > cd && (Keyboard.current != null && Keyboard.current.anyKey.isPressed || Mouse.current != null && Mouse.current.leftButton.isPressed))
        {
            if (LeaderboardUI.Instance != null)
                LeaderboardUI.Instance.Hide();

            HighScoreSet.FinalizeSessionCoins();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
