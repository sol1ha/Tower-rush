using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the jetpack visual and tells the <see cref="Player"/> when its active
/// </summary>
public class PlayerJetpack : MonoBehaviour
{
    public float jetPackDuration = 5f;
    public float jetPackThrust;

    public bool hasJetpack = false;

    Player player;
    SpriteRenderer jetpackVisual;
    AudioSource audioSource;
    ParticleSystem jetpackParticles;
    Laser_kill laser;
    float timer;
    // Start is called before the first frame update
    void Start()
    {
        jetpackVisual = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<Player>();
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.GetComponent<Player>();
        }
        if (player != null)
            player.SetJetpackThrust(jetPackThrust);
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.mute = true;
            audioSource.volume = 0f;
            audioSource.enabled = false;
        }
        jetpackParticles = GetComponent<ParticleSystem>();
        if (jetpackParticles != null)
        {
            var emission = jetpackParticles.emission;
            emission.enabled = false;
            jetpackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        laser = FindAnyObjectByType<Laser_kill>();
    }

    void Update()
    {
        if (player == null) return;

        // Jetpack engine sound permanently disabled (per user request).
        if (audioSource != null)
        {
            if (audioSource.isPlaying) audioSource.Stop();
            if (audioSource.enabled) audioSource.enabled = false;
        }

        if (hasJetpack || player.IsJetpacking)
        {
            if (jetpackVisual != null) jetpackVisual.enabled = true;
        }
        else
        {
            // Only disable if it's NOT the player's main renderer
            if (jetpackVisual != null && jetpackVisual.gameObject != player.gameObject)
                jetpackVisual.enabled = false;
        }

        if (hasJetpack && Keyboard.current != null && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
        {
            hasJetpack = false;
            if (jetpackParticles != null)
            {
                var emission = jetpackParticles.emission;
                emission.enabled = true;
                jetpackParticles.Play();
            }
            player.IsJetpacking = true;
            timer = Time.time + jetPackDuration;
            // Jetpack engine sound permanently disabled — do not play.
            if (laser != null) laser.BoostSpeed();
        }
        if (player.IsJetpacking && Time.time > timer)
        {
            player.IsJetpacking = false;
            if (audioSource != null) audioSource.Stop();
            if (jetpackParticles != null)
            {
                var emission = jetpackParticles.emission;
                emission.enabled = false;
                jetpackParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            if (laser != null) laser.ResetSpeed();
        }
    }
}
