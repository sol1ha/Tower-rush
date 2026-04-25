using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the <see cref="PlayerKill"/> event and triggers it when the player falls.
/// Kill zone sits right at the bottom edge of the camera so the player dies
/// the moment the rising floor almost catches them.
/// </summary>
public class PlayerKillLimit : MonoBehaviour
{
    public class PlayerKillArgs : EventArgs
    {

    }
    public static event EventHandler<EventArgs> PlayerKill;

    public static bool triggerEvent;

    [Tooltip("Extra units below the camera bottom before the player is killed.")]
    public float killMargin = 0.3f;

    private Transform mainCamera;
    private Camera cam;
    private Transform player;

    void Start()
    {
        triggerEvent = false;
        cam = Camera.main;
        if (cam != null) mainCamera = cam.transform;
        TryFindPlayer();
    }

    void TryFindPlayer()
    {
        var obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null) player = obj.transform;
    }

    void Update()
    {
        if (triggerEvent)
        {
            triggerEvent = false;
            InvokeKillSafely(this);
            return;
        }

        if (mainCamera == null || cam == null) return;

        float cameraBottomY = mainCamera.position.y - cam.orthographicSize + killMargin;

        Vector3 pos = transform.position;
        pos.y = cameraBottomY;
        transform.position = pos;

        if (!GameManager.Playing()) return;

        if (player == null) TryFindPlayer();
        if (player == null) return;

        if (player.position.y < cameraBottomY)
        {
            Debug.Log($"PlayerKillLimit: player below camera (player.y={player.position.y:F2}, killY={cameraBottomY:F2}) — killing.");
            InvokeKillSafely(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(GameManager.Playing() && collision.CompareTag("Player"))
        {
            InvokeKillSafely(this);
        }
    }

    // Invoke each subscriber independently so one throwing handler doesn't block the others
    static void InvokeKillSafely(object sender)
    {
        var handler = PlayerKill;
        if (handler == null) return;
        foreach (EventHandler<EventArgs> d in handler.GetInvocationList())
        {
            try { d(sender, EventArgs.Empty); }
            catch (System.Exception e) { Debug.LogError("PlayerKill handler threw: " + e); }
        }
    }

    public static void TriggerEventStatic()
    {
        triggerEvent = true;
        InvokeKillSafely(null);
    }
}
