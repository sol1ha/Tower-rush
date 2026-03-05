using UnityEngine;
using System.Collections;

public class PlayerController3D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float dashForce = 15f;
    public float dashDuration = 0.2f;

    [Header("Jump Settings")]
    public float jumpForce = 7f;

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

        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
        }
    }

    void Update()
    {
        // Ground check
        Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 1f;
        isGrounded = Physics.CheckSphere(checkPos, groundDistance, groundLayer);

        if (isGrounded)
            canDoubleJump = true;

        // Jump — Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (canDoubleJump)
            {
                Jump();
                canDoubleJump = false;
            }
        }

        // Dash — Left Shift
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }

        Move();
    }

    void Move()
    {
        if (isDashing) return;

        // WASD / Arrow keys — A/D or Left/Right for horizontal
        float moveX = Input.GetAxisRaw("Horizontal");
        float currentYVel = rb.linearVelocity.y;
        rb.linearVelocity = new Vector3(moveX * moveSpeed, currentYVel, 0);
    }

    void Jump()
    {
        Vector3 currentVel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(currentVel.x, 0, 0);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        lastDashTime = Time.time;

        float moveX = Input.GetAxisRaw("Horizontal");
        Vector3 dashDir = new Vector3(moveX, 0, 0).normalized;
        if (dashDir == Vector3.zero) dashDir = Vector3.right;

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
        Vector3 pos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 1f;
        Gizmos.DrawWireSphere(pos, groundDistance);
    }
}
