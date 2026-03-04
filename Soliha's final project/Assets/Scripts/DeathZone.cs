using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null) UIManager.Instance.DisplayNotification("Fell off!");
            GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Hazard"))
        {
            // Laser hits the floor -> "its gone aswell"
            Destroy(other.gameObject);
        }
    }
}
