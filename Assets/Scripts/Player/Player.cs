using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public float movementSpeed = 10f;
    public bool IsJetpacking = false;

    [Header("Auto-bounce (fallback)")]
    [Tooltip("Vertical impulse applied when the player lands on something. Mirrors Platform.jumpForce so bouncing works even if Platform's OnCollision path doesn't fire.")]
    public float autoBounceForce = 12f;
    [Tooltip("How far below the player's collider to probe for ground.")]
    public float groundProbeDistance = 0.15f;
    [Tooltip("Layers considered ground (default: Everything).")]
    public LayerMask groundMask = ~0;

    Rigidbody2D rb;
    Collider2D mainCol;

    float movement = 0f;
    public float HorizontalMovement => movement;
    float jetPackThrust;

    [Header("AFK auto-kill")]
    [Tooltip("If true, attaches an AfkKiller to the player so they're auto-killed after some idle time.")]
    public bool autoAttachAfkKiller = true;
    [Tooltip("Seconds without input before the player is auto-killed by the AfkKiller.")]
    public float afkTimeoutSeconds = 30f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // Pick the largest non-trigger collider for ground checks.
        Collider2D best = null;
        float bestArea = -1f;
        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
        {
            if (c.isTrigger) continue;
            float area = c.bounds.size.x * c.bounds.size.y;
            if (area > bestArea) { bestArea = area; best = c; }
        }
        mainCol = best;

        // Attach an AfkKiller (if not already present) so the player is killed
        // after afkTimeoutSeconds of no input. The script existed but wasn't
        // wired up to anything in the scene, so the auto-kill never fired.
        if (autoAttachAfkKiller)
        {
            var afk = GetComponent<AfkKiller>();
            if (afk == null) afk = gameObject.AddComponent<AfkKiller>();
            afk.afkTimeoutSeconds = afkTimeoutSeconds;
        }
    }

    void OnEnable()
    {
        // Safety net: when the Player goes active (game has left the menu),
        // make absolutely sure time isn't stuck frozen and that GameManager.play
        // is true, so a half-broken StartGame path can't strand the player
        // kinematic and silent.
        Time.timeScale = 1f;
        if (GameManager.instance != null) GameManager.instance.play = true;
        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) rb2d.bodyType = RigidbodyType2D.Dynamic;

        // Make sure the global AudioListener isn't silenced (e.g. left at 0 by
        // some other script) so coin / bounce / death SFX are audible.
        AudioListener.volume = 1f;
        AudioListener.pause = false;
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
            // Movement input + active wind force (set by WindGustSpawner during
            // a gust). Wind is added on top of movement, so the player's input
            // can fight it but never fully cancel a strong gust — exactly the
            // 'pushed sideways' feel.
            velocity.x = movement + WindGustSpawner.CurrentForce;

            // Auto-bounce fallback: if player is descending or stationary AND
            // touching ground beneath them, apply that platform's actual bounce
            // force (so a slow landing on a boost platform still goes high, not
            // a generic fallback height).
            if (velocity.y <= 0.1f)
            {
                Platform platUnder;
                if (IsGroundedOnPlatform(out platUnder))
                {
                    if (platUnder != null && platUnder.isBoostPlatform)
                        velocity.y = platUnder.jumpForce * platUnder.boostJumpMultiplier;
                    else if (platUnder != null)
                        velocity.y = platUnder.jumpForce;
                    else
                        velocity.y = autoBounceForce;

                    // Always play a landing sound on a successful auto-bounce so
                    // repeat landings on the same platform still 'thud'.
                    Vector3 sfxPos = platUnder != null ? platUnder.transform.position : transform.position;
                    AudioHelper.PlayLanding(sfxPos);
                }
            }

            rb.linearVelocity = velocity;
        }
    }

    bool IsGroundedOnPlatform(out Platform platform)
    {
        platform = null;
        if (mainCol == null) return false;
        Bounds b = mainCol.bounds;
        Vector2 boxCenter = new Vector2(b.center.x, b.min.y - groundProbeDistance * 0.5f);
        Vector2 boxSize = new Vector2(b.size.x * 0.9f, groundProbeDistance);
        Collider2D[] hits = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f, groundMask);
        foreach (var h in hits)
        {
            if (h == null) continue;
            if (h.transform.IsChildOf(transform) || h.transform == transform) continue;
            if (h.isTrigger) continue;
            platform = h.GetComponentInParent<Platform>();
            return true;
        }
        return false;
    }

    public void SetJetpackThrust(float value)
    {
        jetPackThrust = value;
    }
}
