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

    public static void PlayAt(AudioClip clip, Vector3 worldPos, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPos, Mathf.Clamp01(volume));
    }

    public static void PlayDamage(Vector3 worldPos)        => PlayAt(ExplosionClip, worldPos, 0.55f);
    public static void PlayDeath(Vector3 worldPos)         => PlayAt(GameOverClip, worldPos, 0.85f);
    public static void PlayLanding(Vector3 worldPos)       => PlayAt(ImpactClip, worldPos, 0.40f);
}
