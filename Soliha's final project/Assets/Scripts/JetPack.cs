using UnityEngine;

/// Sits on a platform. When the player touches it, it activates the jetpack
/// on the player — the player flies upward for a few seconds.
public class JetPack : MonoBehaviour
{
    public float rotationSpeed = 80f;

    void Update()
    {
        // Spin so the player can see it
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tell the player to fly
            PlayerController3D player = other.GetComponent<PlayerController3D>();
            if (player != null)
                player.ActivateJetPack();

            if (UIManager.Instance != null)
                UIManager.Instance.DisplayNotification("JETPACK!");

            Destroy(gameObject);
        }
    }
}
