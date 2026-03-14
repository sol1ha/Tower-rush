using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Simple platform script handling <see cref="Player"/> and <see cref="Bullet"/> bouncing
/// </summary>
public class Platform : MonoBehaviour
{
    [SerializeField] AudioSource bulletbounce = null;
    public float jumpForce = 10f;
    [Tooltip("If true, this is a boost platform that speeds up the laser while the player is airborne")]
    public bool isBoostPlatform = false;
    public bool destroy;
    private Transform mainCamera;
    private static Laser_kill laserRef;

    private void Start()
    {
        mainCamera = Camera.main.transform;
        bulletbounce = GetComponent<AudioSource>();
        if (laserRef == null)
            laserRef = FindAnyObjectByType<Laser_kill>();
    }
    private void Update()
    {
        if(destroy && mainCamera.position.y - 10 > transform.position.y)
        {
            Destroy(gameObject);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.y <= 0f)
        {
            Rigidbody2D rb;
            Player player = collision.collider.GetComponent<Player>();
            if (player != null)
            {
                if (!player.IsJetpacking)
                {
                    rb = collision.collider.GetComponentInParent<Rigidbody2D>();
                    if (rb != null)
                    {
                        Vector2 velocity = rb.linearVelocity;
                        velocity.y = jumpForce;
                        rb.linearVelocity = velocity;
                    }

                    // Boost platform speeds up laser, normal platform resets it
                    if (laserRef != null)
                    {
                        if (isBoostPlatform)
                            laserRef.BoostSpeed();
                        else
                            laserRef.ResetSpeed();
                    }
                    return;
                }
            }
            rb = collision.collider.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 velocity = rb.linearVelocity;
                velocity.y = jumpForce;
                rb.linearVelocity = velocity;
            }

            if (bulletbounce != null && bulletbounce.enabled)
            {
                bulletbounce.Play();
            }
        }
    }
}
