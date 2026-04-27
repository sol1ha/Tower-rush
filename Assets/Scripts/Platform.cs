using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Platform : MonoBehaviour
{
    [SerializeField] AudioSource bulletbounce = null;
    public float jumpForce = 10f;
    [Tooltip("If true, this is a boost platform that speeds up the laser while the player is airborne")]
    public bool isBoostPlatform = false;
    public bool destroy;
    private Transform mainCamera;
    private static Laser_kill laserRef;

    private bool bouncedThisFrame = false;
    private Collider2D ownCollider;

    private void Awake()
    {
        bulletbounce = GetComponent<AudioSource>();
        if (bulletbounce != null)
        {
            bulletbounce.playOnAwake = false;
            bulletbounce.Stop();
            // Make sure the legacy clip slot is filled if only the new resource
            // slot has the AudioClip — otherwise PlayOneShot / Play might no-op.
            if (bulletbounce.clip == null && bulletbounce.resource is AudioClip rc)
                bulletbounce.clip = rc;
            if (bulletbounce.volume <= 0f) bulletbounce.volume = 0.25f;
            bulletbounce.mute = false;
            bulletbounce.enabled = true;
        }
        ownCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        if (Camera.main != null)
            mainCamera = Camera.main.transform;
        if (laserRef == null)
            laserRef = FindAnyObjectByType<Laser_kill>();
    }

    private void LateUpdate()
    {
        bouncedThisFrame = false;
    }

    private void Update()
    {
        if (destroy && mainCamera != null && mainCamera.position.y - 10 > transform.position.y)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore the small (disabled) player collider — only react to the main body collider.
        if (collision.collider != null && collision.collider.bounds.size.x < 1.0f) return;
        TryBounce(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider != null && collision.collider.bounds.size.x < 1.0f) return;
        TryBounce(collision);
    }

    void DebugLogContact(string phase, Collision2D collision)
    {
        Rigidbody2D rb = collision.collider.GetComponentInParent<Rigidbody2D>();
        if (rb == null) return;
        if (collision.collider.GetComponentInParent<Player>() == null) return;

        if (ownCollider == null) ownCollider = GetComponent<Collider2D>();
        float platformTop = ownCollider != null ? ownCollider.bounds.max.y : transform.position.y;
        float platformBottom = ownCollider != null ? ownCollider.bounds.min.y : transform.position.y;

        string contactInfo = "";
        ContactPoint2D[] contacts = collision.contacts;
        for (int i = 0; i < contacts.Length; i++)
        {
            contactInfo += $"\n  contact[{i}] point=({contacts[i].point.x:F2},{contacts[i].point.y:F2}) normal=({contacts[i].normal.x:F2},{contacts[i].normal.y:F2})";
        }

        Debug.Log(
            $"[Platform {gameObject.name}] {phase}\n" +
            $"  rb.pos.y={rb.position.y:F2}, vel.y={rb.linearVelocity.y:F2}\n" +
            $"  platformTop={platformTop:F2}, platformBottom={platformBottom:F2}\n" +
            $"  collider.collider={collision.collider.name} (size={collision.collider.bounds.size}, pos.y={collision.collider.bounds.center.y:F2})\n" +
            $"  collider.otherCollider={collision.otherCollider.name}" +
            contactInfo);
    }

    void TryBounce(Collision2D collision)
    {
        if (bouncedThisFrame) return;

        Rigidbody2D rb = collision.collider.GetComponentInParent<Rigidbody2D>();
        if (rb == null) return;

        // Don't double-bounce while already shooting up.
        if (rb.linearVelocity.y > 5f) return;

        Player player = collision.collider.GetComponentInParent<Player>();
        if (player != null && player.IsJetpacking) return;

        // Bounce on any valid contact — no X-range or side-graze early-out, so
        // a player landing on the edge of a platform still bounces normally.
        Vector2 velocity = rb.linearVelocity;
        velocity.y = jumpForce;
        rb.linearVelocity = velocity;
        bouncedThisFrame = true;

        if (laserRef != null)
        {
            if (isBoostPlatform)
                laserRef.BoostSpeed();
            else
                laserRef.ResetSpeed();
        }

        if (bulletbounce != null && bulletbounce.enabled && bulletbounce.clip != null)
            bulletbounce.PlayOneShot(bulletbounce.clip);
    }

    // Returns the player's main/largest Collider2D (the one that should define their footprint).
    // The player may have an extra trigger collider (from PlayerHealth); we want the main one.
    Collider2D FindPlayerMainCollider(Rigidbody2D rb)
    {
        Collider2D best = null;
        float bestArea = -1f;
        foreach (Collider2D c in rb.GetComponentsInChildren<Collider2D>())
        {
            if (c.isTrigger) continue;
            Vector2 size = c.bounds.size;
            float area = size.x * size.y;
            if (area > bestArea)
            {
                bestArea = area;
                best = c;
            }
        }
        return best;
    }
}
