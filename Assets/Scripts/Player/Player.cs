using UnityEngine;
using UnityEngine.InputSystem;

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
        if (!GameManager.Playing())
        {
            movement = 0f;
            return;
        }

        float horizontal = 0f;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
        }

        var gp = Gamepad.current;
        if (gp != null)
        {
            float stick = gp.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > 0.1f) horizontal += stick;
        }

        movement = horizontal * movementSpeed;
    }

    void FixedUpdate()
    {
        if (!GameManager.Playing())
        {
            // Hold the player completely still until the game starts.
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            return;
        }

        if (rb.bodyType == RigidbodyType2D.Kinematic)
            rb.bodyType = RigidbodyType2D.Dynamic;

        if (IsJetpacking)
        {
            Vector2 velocity;
            velocity.y = jetPackThrust;
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

    public void SetJetpackThrust(float value)
    {
        jetPackThrust = value;
    }
}
