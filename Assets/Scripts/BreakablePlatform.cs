using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// A trap platform that breaks after the player stays on it without moving for 4 seconds.
/// </summary>
public class BreakablePlatform : MonoBehaviour
{
    public float breakTime = 4f;
    public float jumpForce = 10f;
    public bool destroy = true;

    private float timer;
    private bool playerOnPlatform;
    private Transform mainCamera;

    void Start()
    {
        mainCamera = Camera.main.transform;
    }

    void Update()
    {
        if (destroy && mainCamera.position.y - 10 > transform.position.y)
        {
            Destroy(gameObject);
        }

        if (!playerOnPlatform) return;

        float horizontal = 0f;
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
        }

        if (Mathf.Abs(horizontal) > 0.1f)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= breakTime)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.y > 0f) return;

        Player player = collision.collider.GetComponent<Player>();
        if (player != null && !player.IsJetpacking)
        {
            Rigidbody2D rb = collision.collider.GetComponentInParent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;
            }

            playerOnPlatform = true;
            timer = 0f;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.GetComponent<Player>() != null)
        {
            playerOnPlatform = false;
            timer = 0f;
        }
    }
}
