using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the player's movement and jetpacking.
/// </summary>
public class Player : MonoBehaviour
{
    public float movementSpeed = 10f;
    public bool IsJetpacking = false;

    Rigidbody2D rb;

    float movement = 0f;
    public float HorizontalMovement => movement;
    float jetPackThrust;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (GameManager.Playing())
        {
            float horizontal = 0f;
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
            }
            movement = horizontal * movementSpeed;
        }
    }

    void FixedUpdate()
    {
        if (IsJetpacking)
        {
            Vector2 velocity;    
            velocity.y = jetPackThrust; //Jittery TODO FIX, move to Update and use the transform to move it in steps.
            velocity.x = movement;
            rb.linearVelocity = velocity;
        }
        else
        {
            Vector2 velocity = rb.linearVelocity;
            velocity.x = movement;
            rb.linearVelocity = velocity;
        }
    }
    /// <summary>
    /// Used by <see cref="PlayerJetpack"/> to set the thrust.
    /// </summary>
    public void SetJetpackThrust(float value)
    {
        jetPackThrust = value;
    }
}
