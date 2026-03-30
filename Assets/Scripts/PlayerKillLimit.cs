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

    private Transform mainCamera;
    private Camera cam;

    void Start()
    {
        triggerEvent = false;
        cam = Camera.main;
        mainCamera = cam.transform;
    }

    void Update()
    {
        if (triggerEvent)
        {
            triggerEvent = false;
            PlayerKill?.Invoke(this, EventArgs.Empty);
        }

        // Position the kill zone right at the bottom edge of the camera view
        if (mainCamera != null && cam != null)
        {
            Vector3 pos = transform.position;
            pos.y = mainCamera.position.y - cam.orthographicSize + 0.3f;
            transform.position = pos;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(GameManager.Playing() && collision.CompareTag("Player"))
        {
            PlayerKill?.Invoke(this, EventArgs.Empty);
        }
    }

    public static void TriggerEventStatic()
    {
        triggerEvent = true;
    }
}
