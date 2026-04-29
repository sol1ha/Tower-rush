using UnityEngine;

/// <summary>
/// A rock that drops straight down at a target X. Damages the player on hit
/// and self-destructs if it falls below the camera or hits a platform.
/// </summary>
public class FallingRock : MonoBehaviour
{
    [Tooltip("Damage dealt to the player on contact.")]
    public int damage = 1;
    [Tooltip("Seconds the rock lives before despawning if it never hits anything.")]
    public float lifetime = 8f;
    [Tooltip("Downward speed in world units / sec.")]
    public float fallSpeed = 14f;
    [Tooltip("Z-rotation speed in degrees / sec for visual flair.")]
    public float spinSpeed = 90f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (other.CompareTag("Player") || other.GetComponentInParent<Player>() != null)
        {
            var ph = PlayerHealth.Instance;
            if (ph != null) ph.DamagePlayer(damage);
            Destroy(gameObject);
            return;
        }
        // Hit a platform / spike / ground — burst on impact.
        if (other.GetComponentInParent<Platform>() != null)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider == null) return;
        if (col.collider.CompareTag("Player") || col.collider.GetComponentInParent<Player>() != null)
        {
            var ph = PlayerHealth.Instance;
            if (ph != null) ph.DamagePlayer(damage);
            Destroy(gameObject);
        }
        else if (col.collider.GetComponentInParent<Platform>() != null)
        {
            Destroy(gameObject);
        }
    }
}
