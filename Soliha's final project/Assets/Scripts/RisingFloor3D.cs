using UnityEngine;

public class RisingFloor3D : MonoBehaviour
{
    public float baseMoveSpeed = 0.2f;
    public float boostMoveSpeed = 8f; // Fast speed when player is jetpacking
    public float delayBeforeStart = 5f;
    private float startTime;
    private PlayerController3D player;

    void Start()
    {
        startTime = Time.time + delayBeforeStart;

        // Try to find the player automatically
        player = Object.FindFirstObjectByType<PlayerController3D>();

        if (UIManager.Instance != null)
            UIManager.Instance.DisplayNotification("The floor is rising!");
    }

    void Update()
    {
        if (Time.time < startTime) return;

        // Speed increases with difficulty level
        float speed = baseMoveSpeed;
        if (GameManager.Instance != null)
            speed = baseMoveSpeed * GameManager.Instance.DifficultyMultiplier;

        // Move the rising floor exceptionally fast when the player uses the boost pad (Jetpack)
        if (player != null && player.IsJetPacking)
        {
            speed = boostMoveSpeed;
        }

        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Platform"))
        {
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.GameOver();
        }
    }
}
