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
            // If the clip slot is empty but the (newer) resource slot holds
            // an AudioClip, copy it so legacy Play()/PlayOneShot()/clip queries work.
            if (audioSource.clip == null && audioSource.resource is AudioClip rc)
                audioSource.clip = rc;
            if (audioSource.volume <= 0f) audioSource.volume = 0.5f;
            audioSource.mute = false;
            audioSource.enabled = true;
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
            // Play sound BEFORE destroying so we can use PlayClipAtPoint as a
            // safety net — that detaches playback from this GameObject.
            if (audioSource != null && audioSource.clip != null)
            {
                AudioSource.PlayClipAtPoint(audioSource.clip, transform.position, audioSource.volume);
            }
            else if (audioSource != null)
            {
                audioSource.Play();
            }
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            Destroy(gameObject, 0.05f);
        }
    }
}
