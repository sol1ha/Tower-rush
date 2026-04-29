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

    // ----- Combo state (shared across all coins) -----
    [Tooltip("Seconds between coin pickups within which the combo keeps growing.")]
    public static float comboWindowSeconds = 2.5f;
    [Tooltip("Maximum combo multiplier (caps the points-per-coin bonus).")]
    public static int maxCombo = 5;
    private static int currentCombo = 0;
    private static float lastCollectTime = -999f;
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
            active = false;
            this.enabled = false;

            // Combo: if we picked up a coin within the window, increment the
            // multiplier (capped). Otherwise reset to 1.
            if (Time.time - lastCollectTime <= comboWindowSeconds) currentCombo = Mathf.Min(currentCombo + 1, maxCombo);
            else currentCombo = 1;
            lastCollectTime = Time.time;

            int gain = currentCombo;
            HighScoreSet.gameScore += gain;
            Debug.Log("Coin collected: +" + gain + " (combo x" + currentCombo + ")");

            // Floating reward popup near the coin.
            string popup = currentCombo > 1 ? "+" + gain + "  x" + currentCombo : "+" + gain;
            Color popupColor = currentCombo >= maxCombo
                ? new Color(1f, 0.40f, 0.20f) // orange-red at max
                : currentCombo >= 3
                    ? new Color(1f, 0.85f, 0.20f) // gold for big chains
                    : new Color(1f, 1f, 0.55f);  // light yellow base
            FloatingText.Spawn(transform.position, popup, popupColor);

            if (audioSource != null && audioSource.clip != null)
                AudioSource.PlayClipAtPoint(audioSource.clip, transform.position, audioSource.volume);
            else if (audioSource != null)
                audioSource.Play();

            if (spriteRenderer != null) spriteRenderer.enabled = false;
            Destroy(gameObject, 0.05f);
        }
    }
}
