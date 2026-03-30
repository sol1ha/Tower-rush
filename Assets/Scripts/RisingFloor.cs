using UnityEngine;

/// <summary>
/// A rising floor that chases the player from below.
/// Works like the laser but visually is a floor that rises up.
/// Activates at score 20, starts 15 units below the player, then rises forever.
/// Kills the player on contact. Destroys platforms it passes.
/// </summary>
public class RisingFloor : MonoBehaviour
{
    [Header("Speed")]
    public float speed = 2f;
    public float boostedSpeed = 5f;

    [Header("Activation")]
    public int activateAtScore = 20;
    public float startOffsetBelowPlayer = 15f;

    private float normalSpeed;
    private bool activated = false;
    private Transform player;

    void Start()
    {
        normalSpeed = speed;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void Update()
    {
        if (!GameManager.Playing()) return;

        if (!activated)
        {
            if (ActualScoreDisplay.CurrentScore >= activateAtScore)
            {
                if (player == null)
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                    if (playerObj != null)
                        player = playerObj.transform;
                }

                if (player != null)
                {
                    activated = true;
                    Vector3 newPos = transform.position;
                    newPos.y = player.position.y - startOffsetBelowPlayer;
                    transform.position = newPos;
                }
            }
            return;
        }

        // Rise up forever
        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    public void BoostSpeed()
    {
        speed = boostedSpeed;
    }

    public void ResetSpeed()
    {
        speed = normalSpeed;
    }

    // Kill player on contact
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        HandleCollision(collision);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleCollision(collision.collider);
    }

    private void HandleCollision(Collider2D col)
    {
        if (!GameManager.Playing()) return;

        // Kill player instantly
        if (col.CompareTag("Player"))
        {
            PlayerKillLimit.TriggerEventStatic();
        }

        // Destroy platforms the floor has passed
        if (col.CompareTag("Platform") || col.CompareTag("Boost"))
        {
            Destroy(col.gameObject);
        }
    }
}
