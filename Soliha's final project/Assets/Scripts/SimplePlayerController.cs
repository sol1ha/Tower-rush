using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class SimplePlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashForce = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private float lastDashTime;
    private bool isDashing;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public int maxJumps = 2;
    private int jumpsRemaining;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 1.1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (Keyboard.current == null || isDashing) return;

        // 1. Movement (WASD + Arrow Keys)
        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveZ = -1f;

        Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;
        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        // 2. Dash (Shift Key)
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(PerformDash(moveDir));
        }

        // 3. Jumping Logic
        bool grounded = IsGrounded();
        if (grounded) jumpsRemaining = maxJumps;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (grounded || jumpsRemaining > 0)
            {
                PerformJump();
            }
        }
    }

    void PerformJump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        jumpsRemaining--;
    }

    System.Collections.IEnumerator PerformDash(Vector3 dir)
    {
        isDashing = true;
        lastDashTime = Time.time;

        // If not moving, dash forward
        if (dir == Vector3.zero) dir = transform.forward;

        rb.linearVelocity = dir * dashForce;
        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;
    }

    bool IsGrounded()
    {
        bool hit1 = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
        bool hit2 = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, groundCheckDistance, groundLayer);
        return hit1 || hit2;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);
    }
}
