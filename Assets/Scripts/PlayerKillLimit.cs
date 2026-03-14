using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the <see cref="PlayerKill"/> event and triggers it when the player falls.
/// </summary>
public class PlayerKillLimit : MonoBehaviour
{
    public class PlayerKillArgs : EventArgs
    {

    }
    public static event EventHandler<EventArgs> PlayerKill;

    public static bool triggerEvent;
    public float followOffset = -15f;

    private Transform mainCamera;

    void Start()
    {
        triggerEvent = false;
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (triggerEvent)
        {
            triggerEvent = false;
            PlayerKill?.Invoke(this, EventArgs.Empty);
        }

        // Follow camera vertically, always staying below the screen
        if (mainCamera != null)
        {
            Vector3 pos = transform.position;
            pos.y = mainCamera.position.y + followOffset;
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
