using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles coin pickup audio and logic.
/// </summary>
public class Coin : MonoBehaviour
{
    AudioSource audioSource;
    SpriteRenderer spriteRenderer;
    bool active = true;
    // Start is called before the first frame update
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(active && (collision.CompareTag("Player") || collision.GetComponentInParent<Player>() != null))
        {
            Debug.Log("Coin collected! Score increased by 1. New score: " + (HighScoreSet.gameScore + 1));
            active = false;
            this.enabled = false;
            HighScoreSet.gameScore += 1;
            Destroy(gameObject,1);
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            if (audioSource != null) audioSource.Play();
        }
    }
}
