using UnityEngine;

/// <summary>
/// Kill laser that follows the bottom of the camera.
/// Only active during gameplay. Kills player on contact.
/// </summary>
public class Laser_kill : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        if (!GameManager.Playing()) return;

        // Follow the bottom edge of the camera
        Vector3 pos = transform.position;
        pos.y = cam.transform.position.y - cam.orthographicSize + 0.5f;
        transform.position = pos;
    }

    public void BoostSpeed() { }
    public void ResetSpeed() { }

    private void OnTriggerEnter2D(Collider2D collision) { Kill(collision); }
    private void OnTriggerStay2D(Collider2D collision) { Kill(collision); }
    private void OnCollisionEnter2D(Collision2D collision) { Kill(collision.collider); }
    private void OnCollisionStay2D(Collision2D collision) { Kill(collision.collider); }

    private void Kill(Collider2D collision)
    {
        if (GameManager.Playing() && collision.CompareTag("Player"))
        {
            Debug.Log("LASER KILLED PLAYER!");
            PlayerKillLimit.TriggerEventStatic();
        }
    }
}
