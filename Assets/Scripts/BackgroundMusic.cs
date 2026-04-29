using UnityEngine;

/// <summary>
/// Plays a looping background music track. Optionally fades in on start
/// and survives scene reloads if marked as persistent.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Header("Music")]
    [Tooltip("The looping music clip. Drop your soft game music asset here.")]
    public AudioClip musicClip;

    [Tooltip("Volume (0 to 1). Music should be soft so it doesn't drown out SFX.")]
    [Range(0f, 1f)] public float volume = 0.35f;

    [Tooltip("If true, plays automatically on Start().")]
    public bool playOnStart = true;

    [Tooltip("Force-disable auto-play even if playOnStart is set in inspector.")]
    public bool forceMute = false;

    [Tooltip("Seconds to fade in. 0 = play at full volume immediately.")]
    public float fadeInSeconds = 1.5f;

    [Header("Persistence")]
    [Tooltip("Survive scene loads (so music doesn't restart between scenes). Only one instance allowed when on.")]
    public bool persistAcrossScenes = false;

    private static BackgroundMusic instance;
    private AudioSource source;

    void Awake()
    {
        if (persistAcrossScenes)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        source = GetComponent<AudioSource>();
        if (musicClip != null) source.clip = musicClip;
        else if (source.clip == null && source.resource is AudioClip rc) source.clip = rc;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f; // 2D sound, plays from anywhere.
        source.mute = false;
        source.enabled = true;
        source.volume = fadeInSeconds > 0f ? 0f : volume;
    }

    void Start()
    {
        if (forceMute)
        {
            if (source != null) source.Stop();
            return;
        }
        if (playOnStart && musicClip != null)
            Play();
    }

    public void Play()
    {
        if (forceMute) return;
        if (source == null) source = GetComponent<AudioSource>();
        if (source.clip == null && musicClip != null) source.clip = musicClip;
        if (source.clip == null) return;
        if (!gameObject.activeInHierarchy || !source.enabled) return;

        source.Play();
        if (fadeInSeconds > 0f) StartCoroutine(FadeTo(volume, fadeInSeconds));
        else source.volume = volume;
    }

    public void Stop(float fadeOutSeconds = 0.5f)
    {
        if (fadeOutSeconds <= 0f) { source.Stop(); return; }
        StartCoroutine(FadeOutAndStop(fadeOutSeconds));
    }

    public void SetVolume(float v) { volume = Mathf.Clamp01(v); source.volume = volume; }

    System.Collections.IEnumerator FadeTo(float target, float seconds)
    {
        float start = source.volume;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }
        source.volume = target;
    }

    System.Collections.IEnumerator FadeOutAndStop(float seconds)
    {
        float start = source.volume;
        float t = 0f;
        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, 0f, t / seconds);
            yield return null;
        }
        source.Stop();
        source.volume = volume;
    }
}
