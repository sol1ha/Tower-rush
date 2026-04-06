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

    void Start()
    {
        spikeCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D playerCollider)
    {
        if (!playerCollider.CompareTag("Player")) return;

        Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();
        PlayerHealth health = playerCollider.GetComponent<PlayerHealth>();

        if (health == null || spikeCollider == null) return;

        // Check if player is landing on TOP of the spike
        // Player's bottom edge must be above spike's top edge
        float playerBottom = playerCollider.bounds.min.y;
        float spikeTop = spikeCollider.bounds.max.y;

        // If player's bottom is above spike's top and falling, they landed on the spike
        bool isOnTopOfSpike = playerBottom >= spikeTop - 0.1f;
        bool isFalling = rb == null || rb.linearVelocity.y <= 0.1f;

        if (isOnTopOfSpike && isFalling)
        {
            health.DamagePlayer(damage);
        }
    }
}
