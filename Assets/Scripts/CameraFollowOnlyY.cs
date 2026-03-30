using UnityEngine;

/// <summary>
/// Camera follows the player upward AND auto-rises at a constant speed.
/// If the player falls below the screen, PlayerKillLimit handles the death.
/// The auto-rise acts as the "rising floor" — no separate object needed.
/// </summary>
public class CameraFollowOnlyY : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = .3f;

    [Header("Auto-Rise (acts as rising floor)")]
    public float riseSpeed = 2f;
    public int activateAtScore = 20;

    private Vector3 currentVelocity;
    private bool riseActivated = false;
    private float autoY;

    void Start()
    {
        autoY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Activate auto-rise once score is reached
        if (!riseActivated && GameManager.Playing() && ActualScoreDisplay.CurrentScore >= activateAtScore)
        {
            riseActivated = true;
            autoY = transform.position.y;
        }

        // Auto-rise: camera keeps moving up at constant speed
        if (riseActivated && GameManager.Playing())
        {
            autoY += riseSpeed * Time.deltaTime;
        }

        // Camera follows whichever is higher: the player or the auto-rise
        float targetY = Mathf.Max(target.position.y, autoY);

        // Only move up, never down
        if (targetY > transform.position.y)
        {
            Vector3 newPos = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref currentVelocity, smoothSpeed * Time.deltaTime);
        }

        // Keep autoY synced if player is above it
        if (transform.position.y > autoY)
        {
            autoY = transform.position.y;
        }
    }
}
