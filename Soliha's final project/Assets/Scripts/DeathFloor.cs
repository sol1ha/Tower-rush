using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathFloor : MonoBehaviour
{
    [Header("Movement")]
    public float riseSpeed = 0.5f;

    void Update()
    {
        // Move upward constant speed
        transform.Translate(Vector3.up * riseSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object we hit is tagged "Player"
        if (other.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Also check collision just in case the collider isn't a trigger
        if (collision.gameObject.CompareTag("Player"))
        {
            RestartLevel();
        }
    }

    void RestartLevel()
    {
        // Reload the current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
