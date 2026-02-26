using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class JumpingPlayer : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float groundCheckDistance = 1.1f; // Slightly more than half the player's height
    public LayerMask groundLayer;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Prevent the player from falling over if using a capsule/cube
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Check for jump input using New Input System
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && IsGrounded())
        {
            Jump();
        }
    }

    void Jump()
    {
        // Standard Rigidbody jump applying an upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    bool IsGrounded()
    {
        // Cast a ray from the center of the player downwards
        // Returns true if the ray hits an object on the groundLayer
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    // Optional: Draw the ray in the editor for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}
