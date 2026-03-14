using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPlatform : MonoBehaviour
{
    [SerializeField] private float breakDelay = 4f;
    [SerializeField] private float movementThreshold = 0.1f;
    
    private float idleTimer = 0f;
    private bool playerOnPlatform = false;
    private Player currentPlayer = null;

    void Update()
    {
        if (playerOnPlatform && currentPlayer != null)
        {
            // Check if player is moving horizontally
            bool isMoving = Mathf.Abs(currentPlayer.HorizontalMovement) > movementThreshold;
            
            if (!isMoving)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= breakDelay)
                {
                    BreakPlatform();
                }
            }
            else
            {
                // Reset timer if they move
                idleTimer = 0f;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            currentPlayer = collision.gameObject.GetComponent<Player>();
            if (currentPlayer != null)
            {
                playerOnPlatform = true;
                idleTimer = 0f;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnPlatform = false;
            currentPlayer = null;
            idleTimer = 0f;
        }
    }

    private void BreakPlatform()
    {
        Debug.Log("Trap platform breaking!");
        // Add particle effects or sound here if available
        Destroy(gameObject);
    }
}
