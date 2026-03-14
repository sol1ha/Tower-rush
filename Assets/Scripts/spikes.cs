using UnityEngine;

/// <summary>
/// Spike hazard that damages the player on contact.
/// Spawned by <see cref="LevelGenerator"/> on platforms.
/// </summary>
public class spikes : MonoBehaviour
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            PlayerHealth health = collision.collider.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.DamagePlayer(damage);
            }
        }
    }
}
