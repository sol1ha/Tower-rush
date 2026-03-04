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
    
    [Header("Coyote Time")]
    public float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    [Header("Input Action References")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;
    public InputActionReference dashAction;

    private void OnEnable()
    {
        if (moveAction != null) moveAction.action.Enable();
        if (jumpAction != null) jumpAction.action.Enable();
        if (dashAction != null) dashAction.action.Enable();
    }

    private void OnDisable()
    {
        if (moveAction != null) moveAction.action.Disable();
        if (jumpAction != null) jumpAction.action.Disable();
        if (dashAction != null) dashAction.action.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Validation: Ensure actions are assigned
        if (moveAction == null) Debug.LogError("Move Action is not assigned on " + gameObject.name + " in the Inspector!");
        if (jumpAction == null) Debug.LogError("Jump Action is not assigned on " + gameObject.name + " in the Inspector!");
        if (dashAction == null) Debug.LogError("Dash Action is not assigned on " + gameObject.name + " in the Inspector!");

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
            coyoteTimeCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        // Jump Input Capture
        if (jumpAction != null && jumpAction.action.WasPressedThisFrame())
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jumping Logic: Coyote Time + Buffer
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            Jump();
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0; // Prevent jumping multiple times in one coyote window
        }
        else if (jumpAction != null && jumpAction.action.WasPressedThisFrame() && canDoubleJump)
        {
            Jump();
            canDoubleJump = false;
        }

        Move();

        if (dashAction != null && dashAction.action.WasPressedThisFrame() && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }
    }

    void Move()
    {
        if (isDashing) return;

        Vector2 moveInput = Vector2.zero;
        if (moveAction != null) moveInput = moveAction.action.ReadValue<Vector2>();
        
        float moveX = moveInput.x;
        float currentYVel = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(moveX * moveSpeed, currentYVel, 0);
    }

    void Jump()
    {
        // Preserve horizontal velocity but reset Y for consistent jump height
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0, 0); 
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        
        if (UIManager.Instance != null) UIManager.Instance.DisplayNotification("Jump!");
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        Vector2 moveInput = Vector2.zero;
        if (moveAction != null) moveInput = moveAction.action.ReadValue<Vector2>();
        float moveX = moveInput.x;
        float moveZ = moveInput.y;

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
