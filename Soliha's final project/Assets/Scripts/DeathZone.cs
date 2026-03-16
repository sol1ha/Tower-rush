using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (UIManager.Instance != null)
                UIManager.Instance.DisplayNotification("Fell off!");
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
        else if (other.CompareTag("Hazard"))
        {
            Destroy(other.gameObject);
        }
    }
}
