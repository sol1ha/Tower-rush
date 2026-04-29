using UnityEngine;

/// <summary>
/// Lightweight helper that plays one-shot sound effects loaded from
/// Resources/Sounds/. Avoids needing to wire AudioSources / AudioClips into
/// the scene for every script that wants to play a sound.
/// </summary>
public static class AudioHelper
{
    static AudioClip cachedExplosion;
    static AudioClip cachedGameOver;
    static AudioClip cachedImpact;
    static AudioClip cachedLaser;

    public static AudioClip ExplosionClip
    {
        get
        {
            if (cachedExplosion == null) cachedExplosion = Resources.Load<AudioClip>("Sounds/explosion");
            return cachedExplosion;
        }
    }

    public static AudioClip GameOverClip
    {
        get
        {
            if (cachedGameOver == null) cachedGameOver = Resources.Load<AudioClip>("Sounds/game_over");
            return cachedGameOver;
        }
    }

    public static AudioClip ImpactClip
    {
        get
        {
            if (cachedImpact == null) cachedImpact = Resources.Load<AudioClip>("Sounds/impact");
            return cachedImpact;
        }
    }

    public static AudioClip LaserClip
    {
        get
        {
            if (cachedLaser == null) cachedLaser = Resources.Load<AudioClip>("Sounds/laser");
            return cachedLaser;
        }
    }

    public static void PlayAt(AudioClip clip, Vector3 worldPos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, Mathf.Clamp01(volume));
    }

    /// <summary>
    /// Plays a one-shot clip at a world position with a specific pitch.
    /// PlayClipAtPoint doesn't expose pitch, so we hand-roll a temporary
    /// AudioSource on a throwaway GameObject and let it self-destroy.
    /// </summary>
    public static void PlayAtPitched(AudioClip clip, Vector3 worldPos, float volume, float pitch)
    {
        if (clip == null) return;
        var go = new GameObject("OneShotAudio_" + clip.name);
        go.transform.position = worldPos;
        var src = go.AddComponent<AudioSource>();
        src.clip = clip;
        src.volume = Mathf.Clamp01(volume);
        src.pitch = Mathf.Max(0.05f, pitch);
        src.spatialBlend = 0f; // 2D so position doesn't fade it
        src.playOnAwake = false;
        src.Play();
        // Destroy after the clip finishes (account for pitch — lower pitch
        // stretches the clip, higher pitch shortens it).
        Object.Destroy(go, clip.length / src.pitch + 0.1f);
    }

    public static void PlayDamage(Vector3 worldPos)  => PlayAt(ExplosionClip, worldPos, 0.55f);
    public static void PlayDeath(Vector3 worldPos)   => PlayAt(GameOverClip, worldPos, 0.85f);
    // Landing thud: louder (close to music level) + slightly slower playback
    // so it reads as a heavier, deeper bounce.
    // Landing thud — matched to the platform's own AudioSource volume (0.25)
    // so the FIRST landing and every subsequent auto-bounce landing sound
    // identical, just a bit nudged up to compensate for PlayClipAtPoint
    // attenuation. Pitch back to natural 1.0 so it's the same clip you hear
    // on the first landing.
    public static void PlayLanding(Vector3 worldPos) => PlayAtPitched(ImpactClip, worldPos, 0.30f, 1.0f);
    public static void PlayLaserHit(Vector3 worldPos) => PlayAt(LaserClip, worldPos, 0.70f);
}
