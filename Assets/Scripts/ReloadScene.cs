using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Reloads the scene after a brief delay on <see cref="PlayerKillLimit.PlayerKill"/> event
/// </summary>
public class ReloadScene : MonoBehaviour
{
    private bool firecd;
    private float cd;

    void Start()
    {
        firecd = false;
        PlayerKillLimit.PlayerKill += PlayerKillLimit_PlayerKill;
    }
    private void PlayerKillLimit_PlayerKill(object sender, System.EventArgs e)
    {
        cd = Time.time + 1f;
        firecd = true;
    }
    void Update()
    {
        if (firecd && Time.time > cd && (Keyboard.current != null && Keyboard.current.anyKey.isPressed || Mouse.current != null && Mouse.current.leftButton.isPressed))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
}
