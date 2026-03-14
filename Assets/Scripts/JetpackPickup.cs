using UnityEngine;

/// <summary>
/// Spawned jetpack pickup. When the player touches it, grants the jetpack.
/// </summary>
public class JetpackPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerJetpack jetpack = collision.GetComponentInChildren<PlayerJetpack>();
            if (jetpack != null && !jetpack.hasJetpack)
            {
                jetpack.hasJetpack = true;
                Destroy(gameObject);
            }
        }
    }
}
