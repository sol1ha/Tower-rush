using UnityEngine;

/// <summary>
/// Rising laser that waits 60 seconds, then moves upward forever at speed 2.
/// Kills the player on contact.
/// </summary>
public class Laser_kill : MonoBehaviour
{
    public float speed = 3.5f;
    public float boostedSpeed = 5f;
    
    private float normalSpeed;
    private bool activated = false;
    private Transform player;

    void Start()
    {
        normalSpeed = speed;
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        if (!GameManager.Playing())
        {
            if (Time.frameCount % 1000 == 0) Debug.Log("Laser waiting: GameManager.Playing() is false");
            return;
        }

        if (!activated)
        {
            if (ActualScoreDisplay.CurrentScore >= 20)
            {
                if (player == null)
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                    {
                        player = playerObj.transform;
                    }
                }

                if (player != null)
                {
                    activated = true;
                    Debug.Log("Laser Activated at Score: " + ActualScoreDisplay.CurrentScore);
                    // Position -15 down from player
                    Vector3 newPos = transform.position;
                    newPos.y = player.position.y - 15f;
                    transform.position = newPos;
                    Debug.Log("Laser initial position set to y=" + transform.position.y + " (Player at y=" + player.position.y + ")");
                }
            }
            else
            {
                return;
            }
        }

        transform.position += Vector3.up * speed * Time.deltaTime;
        
        // Log movement occasionally
        if (Time.frameCount % 300 == 0) // Roughly every 5-10 seconds
        {
            Debug.Log("Laser moving... Current Y: " + transform.position.y + ", Speed: " + speed);
        }
    }

    public void BoostSpeed()
    {
        speed = boostedSpeed;
    }

    public void ResetSpeed()
    {
        speed = normalSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Kill(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        Kill(collision);
    }

    // Fallback for cases where the collider is NOT a trigger
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Kill(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        Kill(collision.collider);
    }

    private void Kill(Collider2D collision)
    {
        if (GameManager.Playing() && collision.CompareTag("Player"))
        {
            PlayerKillLimit.TriggerEventStatic();
            Debug.Log("Laser Kill Triggered!");
        }
    }
}
