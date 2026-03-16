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

    [Header("JetPack")]
    public float jetPackForce = 12f;
    public float jetPackDuration = 3f;

    [Header("Physics")]
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundDistance = 0.4f;

    private Rigidbody rb;
    private bool isGrounded;
    private bool canDoubleJump = false;
    private bool isDashing = false;
    private bool isJetPacking = false;
    public bool IsJetPacking => isJetPacking;
    private float dashCooldown = 1f;
    private float lastDashTime;
    private float lockedZ;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        lockedZ = transform.position.z;

        if (groundCheck == null)
        {
            groundCheck = transform.Find("GroundCheck");
        }
    }

    void Update()
    {
        // During jetpack: fly up, still allow left/right
        if (isJetPacking)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector3(moveX * moveSpeed, jetPackForce, 0);
            return;
        }

        Vector3 checkPos = groundCheck != null ? groundCheck.position : transform.position + Vector3.down * 1f;
        isGrounded = Physics.CheckSphere(checkPos, groundDistance, groundLayer);

        if (isGrounded)
            canDoubleJump = true;

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

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time > lastDashTime + dashCooldown)
        {
            StartCoroutine(Dash());
        }

        Move();
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.z != lockedZ)
        {
            transform.position = new Vector3(pos.x, pos.y, lockedZ);
        }
    }

    void Move()
    {
        if (isDashing) return;

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

    public void ActivateJetPack()
    {
        if (!isJetPacking)
            StartCoroutine(JetPackRoutine());
    }

    private IEnumerator JetPackRoutine()
    {
        isJetPacking = true;
        yield return new WaitForSeconds(jetPackDuration);
        isJetPacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Hazard"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TakeDamage();
        }
        else if (other.CompareTag("RisingFloor"))
        {
            if (GameManager.Instance != null)
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
