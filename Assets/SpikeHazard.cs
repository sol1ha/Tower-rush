using UnityEngine;

/// <summary>
/// Spike hazard that damages the player on contact.
/// The BoxCollider2D on this prefab must have "Is Trigger" enabled.
/// </summary>
public class SpikeHazard : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.DamagePlayer(damage);
            }
        }
    }
}
