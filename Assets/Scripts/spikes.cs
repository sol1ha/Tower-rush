using UnityEngine;

/// <summary>
/// Spike hazard that damages the player ONLY when they land on the platform.
/// If the player is jumping upward (passing through), no damage.
/// All spikes on one platform = 1 life (handled by invincibility in PlayerHealth).
/// </summary>
public class spikes : MonoBehaviour
{
    public int damage = 1;
    private Collider2D spikeCollider;

    void Awake()
    {
        spikeCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        TryDamage(playerCollider);
    }

    private void OnTriggerStay2D(Collider2D playerCollider)
    {
        TryDamage(playerCollider);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void TryDamage(Collider2D playerCollider)
    {
        if (!playerCollider.CompareTag("Player") && !playerCollider.GetComponentInParent<Player>()) return;

        PlayerHealth health = playerCollider.GetComponentInParent<PlayerHealth>();
        if (health == null || spikeCollider == null) return;

        Rigidbody2D rb = playerCollider.GetComponentInParent<Rigidbody2D>();

        // Get the ID of the platform we are attached to (if any) or our own ID
        int platformId = transform.parent != null ? transform.parent.gameObject.GetHashCode() : gameObject.GetHashCode();

        health.DamagePlayerIfLanded(damage, rb, platformId);
    }
}
