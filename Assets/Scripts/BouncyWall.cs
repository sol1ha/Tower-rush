using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to a wall with "Is Trigger" enabled on its Collider2D.
/// When the player enters the trigger while pressing toward the wall,
/// they cling to it and slowly climb upward for a set duration, then launch off.
/// </summary>
public class BouncyWall : MonoBehaviour
{
    [Tooltip("How fast the player climbs up the wall")]
    public float climbSpeed = 2f;

    [Tooltip("How many seconds the player can cling to the wall")]
    public float maxClimbDuration = 3f;

    [Tooltip("How hard the player launches off the wall horizontally")]
    public float launchHorizontalForce = 8f;

    [Tooltip("How high the player launches upward when leaving the wall")]
    public float launchVerticalForce = 10f;

    private bool isLeftWall;
    private Player climbingPlayer = null;
    private Rigidbody2D climbingRb = null;
    private float climbTimer = 0f;
    private bool isClimbing = false;
    private float originalGravity = 1f;

    void Update()
    {
        if (!isClimbing || climbingPlayer == null || climbingRb == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // Check if the player is still pressing INTO the wall
        bool pressingIntoWall = isLeftWall
            ? (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
            : (kb.dKey.isPressed || kb.rightArrowKey.isPressed);

        if (!pressingIntoWall)
        {
            LaunchOff();
            return;
        }

        climbTimer += Time.deltaTime;

        // Slowly walk upward on the wall
        climbingRb.linearVelocity = new Vector2(0f, climbSpeed);

        // After the timer expires, automatically launch off
        if (climbTimer >= maxClimbDuration)
        {
            LaunchOff();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isClimbing) return;

        Player player = other.GetComponent<Player>();
        if (player == null)
            player = other.GetComponentInParent<Player>();
        if (player == null || player.IsJetpacking) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = other.GetComponentInParent<Rigidbody2D>();
        if (rb == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        // Figure out which side this wall is on
        isLeftWall = transform.position.x < player.transform.position.x;

        // Only cling if pressing toward the wall
        bool pressingIntoWall = isLeftWall
            ? (kb.aKey.isPressed || kb.leftArrowKey.isPressed)
            : (kb.dKey.isPressed || kb.rightArrowKey.isPressed);

        if (!pressingIntoWall) return;

        // Start climbing
        climbingPlayer = player;
        climbingRb = rb;
        climbTimer = 0f;
        isClimbing = true;
        originalGravity = rb.gravityScale;

        // Freeze gravity and stop all movement so the player sticks
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Player player = other.GetComponent<Player>();
        if (player == null)
            player = other.GetComponentInParent<Player>();

        if (player != null && isClimbing)
        {
            StopClimbing();
        }
    }

    void LaunchOff()
    {
        if (climbingRb == null) return;

        // Restore gravity
        climbingRb.gravityScale = originalGravity;

        // Launch away from the wall and upward
        float direction = isLeftWall ? 1f : -1f;
        climbingRb.linearVelocity = new Vector2(direction * launchHorizontalForce, launchVerticalForce);

        StopClimbing();
    }

    void StopClimbing()
    {
        if (climbingRb != null)
            climbingRb.gravityScale = originalGravity;

        climbingPlayer = null;
        climbingRb = null;
        climbTimer = 0f;
        isClimbing = false;
    }
}
