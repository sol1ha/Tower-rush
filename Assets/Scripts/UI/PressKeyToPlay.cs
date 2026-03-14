using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Simple script which handles game init and sets the player's <see cref="Rigidbody2D"/> constraints
/// </summary>
public class PressKeyToPlay : MonoBehaviour
{
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
    }
    void Update()
    {
        if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame))
        {
            GameManager.StartGame();
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            this.enabled = false;
        }
    }
}
