using UnityEngine;

/// <summary>
/// Spike hazard that damages the player ONLY when they land on the platform.
/// If the player is jumping upward (passing through), no damage.
/// All spikes on one platform = 1 life (handled by invincibility in PlayerHealth).
/// </summary>
public class spikes : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        Rigidbody2D rb = collision.GetComponent<Rigidbody2D>();
        PlayerHealth health = collision.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.DamagePlayerIfLanded(damage, rb);
        }
    }
}
