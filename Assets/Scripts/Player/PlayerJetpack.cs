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
        jetpackParticles = GetComponent<ParticleSystem>();
        laser = FindAnyObjectByType<Laser_kill>();
    }

    void Update()
    {
        if (player == null) return;

        if (hasJetpack || player.IsJetpacking)
        {
            jetpackVisual.enabled = true;
        }
        else
        {
            jetpackVisual.enabled = false;
        }

        if (hasJetpack && Keyboard.current != null && Keyboard.current.wKey.wasPressedThisFrame)
        {
            hasJetpack = false;
            jetpackParticles.Play();
            player.IsJetpacking = true;
            timer = Time.time + jetPackDuration;
            audioSource.Play();
            if (laser != null) laser.BoostSpeed();
        }
        if (player.IsJetpacking && Time.time > timer)
        {
            player.IsJetpacking = false;
            if (laser != null) laser.ResetSpeed();
        }
    }
}
