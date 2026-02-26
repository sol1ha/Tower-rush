using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashForce = 15f;
    public float dashDuration = 0.2f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;
    public float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Physics")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.4f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool canDoubleJump = false;
    private bool isDashing = false;
    private float dashCooldown = 1f;
    private float lastDashTime;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Auto-assign groundCheck if it's null
        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
            if (groundCheck == null)
            {
                Debug.LogWarning("GroundCheck transform not assigned on " + gameObject.name + ". Falling back to player center.");
            }
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        // Ground Check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
        }
        else
        {
            isGrounded = Physics.CheckSphere(transform.position + Vector3.down * 1f, groundDistance, groundLayer);
        }

        if (isGrounded)
        {
            canDoubleJump = true;
        }

        // Jump Buffering: If we land and have a jump buffered, jump!
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (jumpBufferCounter > 0 && isGrounded)
        {
            Debug.Log("<color=green><b>[SUCCESS]</b> Ground Jump Triggered!</color>");
            Jump();
            jumpBufferCounter = 0;
        }
        else if (Keyboard.current.spaceKey.wasPressedThisFrame && canDoubleJump)
        {
            Debug.Log("<color=cyan><b>[SUCCESS]</b> Double Jump Triggered!</color>");
            Jump();
            canDoubleJump = false;
        }

        Move();

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    void Move()
    {
        if (isDashing) return;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        Vector3 vel = rb.linearVelocity; 
        rb.linearVelocity = new Vector3(move.x * moveSpeed, vel.y, move.z * moveSpeed);
    }

    void Jump()
    {
        // Reset Y velocity for consistent jump height
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float moveX = 0f;
        float moveZ = 0f;

        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = 1f;
        if (Keyboard.current.wKey.isPressed) moveZ = 1f;
        if (Keyboard.current.sKey.isPressed) moveZ = -1f;

        Vector3 dashDir = (transform.right * moveX + transform.forward * moveZ).normalized;
        if (dashDir == Vector3.zero) dashDir = transform.forward;

        rb.linearVelocity = dashDir * dashForce;
        
        yield return new WaitForSeconds(dashDuration);
        
        isDashing = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard") || other.CompareTag("RisingFloor"))
        {
            GameManager.Instance.GameOver();
        }
    }

    // THIS WILL DRAW THE GROUND CHECK SPHERE IN THE SCENE VIEW
    private void OnDrawGizmos()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, groundDistance + 1f);
        }
    }
}
