using UnityEngine;

/// Attach this to the Player. Assign two child Particle Systems in the Inspector:
///   thrusterParticles  -> points downward, always on (the main flame/trail)
///   dashParticles      -> points backward, plays only during a dash
///
/// The emission rate of thrusterParticles scales automatically with Rigidbody velocity
/// so it looks stronger when the player is jumping and quieter when falling/idle.
[RequireComponent(typeof(Rigidbody))]
public class JetpackEffect : MonoBehaviour
{
    [Header("Particle Systems (assign child objects)")]
    public ParticleSystem thrusterParticles;
    public ParticleSystem dashParticles;

    [Header("Emission Rates")]
    public float idleRate   = 5f;
    public float risingRate = 30f;
    public float dashRate   = 60f;

    [Header("Dash Detection")]
    [Tooltip("Speed above which the player is considered to be dashing (dashForce = 15 by default)")]
    public float dashSpeedThreshold = 12f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (thrusterParticles != null)
            thrusterParticles.Play();
    }

    void Update()
    {
        if (thrusterParticles == null) return;

        var emission = thrusterParticles.emission;
        float speed  = rb.linearVelocity.magnitude;
        float yVel   = rb.linearVelocity.y;

        if (speed > dashSpeedThreshold)
        {
            // Dashing: full blast
            emission.rateOverTime = dashRate;
            if (dashParticles != null && !dashParticles.isPlaying)
                dashParticles.Play();
        }
        else
        {
            if (dashParticles != null && dashParticles.isPlaying)
                dashParticles.Stop();

            // Rising (jumping): strong thrust; falling/idle: gentle idle flame
            emission.rateOverTime = yVel > 0.5f ? risingRate : idleRate;
        }
    }
}
