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
    void Start()
    {
        active = true;
        audioSource = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(active && collision.tag == "Player")
        {
            Debug.Log("Coin collected! Score increased by 1. New score: " + (HighScoreSet.gameScore + 1));
            active = false;
            this.enabled = false;
            HighScoreSet.gameScore += 1;
            Destroy(gameObject,1);
            spriteRenderer.enabled = false;
            audioSource.Play();
        }
    }
}
