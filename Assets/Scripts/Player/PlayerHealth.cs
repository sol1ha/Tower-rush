using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Handles the player's health, shields, hit particles, sfx, death and record height.
/// Now supports half-heart damage on last life.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public bool HasShield = false;
    public int maxHealth = 3;
    public Sprite deathSprite;

    [Header("Damage Popup")]
    public TextMeshProUGUI damagePopupText;

    private float health;
    private bool isInvincible = false;
    private int lastHitPlatformId = -1;

    public static PlayerHealth Instance;

    public ParticleSystem bloodHitPS;
    public BoxCollider2D extraCollider;
    public SpriteRenderer shieldVisual;

    private BoxCollider2D playerCollider;
    private int recordHeight;
    private AudioSource audioSource;

    private Sprite originalSprite;

    private void Awake()
    {
        Instance = this;
        health = maxHealth;
        originalSprite = GetComponent<SpriteRenderer>().sprite;
    }

    void Start()
    {
        bloodHitPS = GetComponent<ParticleSystem>();
        playerCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.Stop();
            if (audioSource.clip == null && audioSource.resource is AudioClip rc)
                audioSource.clip = rc;
            audioSource.mute = false;
            audioSource.enabled = true;
        }
        PlayerKillLimit.PlayerKill += PlayerKillLimit_PlayerKill;

        if (damagePopupText != null)
            damagePopupText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (shieldVisual == null) return;

        if (HasShield)
        {
            shieldVisual.enabled = true;
        }
        else
        {
            // Only disable if it's NOT the player's main renderer
            if (shieldVisual.gameObject != gameObject)
                shieldVisual.enabled = false;
        }
    }

    private void PlayerKillLimit_PlayerKill(object sender, System.EventArgs e)
    {
        KillPlayer();
    }

    public int GetHealth()
    {
        return Mathf.CeilToInt(health);
    }

    public float GetHealthFloat()
    {
        return health;
    }

    /// <summary>
    /// Called by spikes. Passes the parent platform's ID to prevent multiple spike hits
    /// on the same platform from draining all lives instantly.
    /// </summary>
    public void DamagePlayer(int amount, int sourcePlatformId = -1)
    {
        // If we hit the EXACT SAME platform as the last hit, ignore it.
        // This ensures one platform (regardless of spike count) only ever takes 1 life.
        if (sourcePlatformId != -1 && sourcePlatformId == lastHitPlatformId) 
        {
            return;
        }

        // Standard user-requested cooldown
        if (isInvincible) 
        {
            return;
        }
        
        lastHitPlatformId = sourcePlatformId;
        StartCoroutine(TakeDamageRoutine(amount));
    }

    IEnumerator TakeDamageRoutine(int amount)
    {
        isInvincible = true;

        if (HasShield)
        {
            HasShield = false;
        }
        else
        {
            bloodHitPS.Play(false);
            health -= amount;
            ShowDamagePopup(amount);
            if (DamageVignette.Instance != null) DamageVignette.Instance.PlayHit();

            // Play "ouch" sound on losing a heart.
            AudioHelper.PlayDamage(transform.position);

            if (health <= 0f)
            {
                PlayerKillLimit.TriggerEventStatic();
            }
        }

        yield return new WaitForSeconds(1f); // 1 second protection
        isInvincible = false;
    }

    /// <summary>
    /// Called by spikes — only damages if player is falling or standing (not jumping up).
    /// </summary>
    public void DamagePlayerIfLanded(int amount, Rigidbody2D playerRb, int sourcePlatformId = -1)
    {
        if (playerRb != null && playerRb.linearVelocity.y > 1f) return;
        DamagePlayer(amount, sourcePlatformId);
    }

    void ShowDamagePopup(float damage)
    {
        if (damagePopupText == null) return;

        StopCoroutine("DamagePopupRoutine");
        string text = damage >= 1f ? "-1 \u2764" : "-0.5 \u2764";
        StartCoroutine(DamagePopupRoutine(text));
    }

    private IEnumerator DamagePopupRoutine(string text)
    {
        damagePopupText.gameObject.SetActive(true);
        damagePopupText.text = text;
        damagePopupText.color = Color.red;

        Vector3 originalPos = damagePopupText.rectTransform.localPosition;
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            float shakeX = Random.Range(-5f, 5f);
            float shakeY = Random.Range(-5f, 5f);
            damagePopupText.rectTransform.localPosition = originalPos + new Vector3(shakeX, shakeY, 0);

            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            damagePopupText.color = new Color(1f, 0f, 0f, alpha);

            float scale = elapsed < duration * 0.3f
                ? Mathf.Lerp(1f, 1.5f, elapsed / (duration * 0.3f))
                : Mathf.Lerp(1.5f, 0.8f, (elapsed - duration * 0.3f) / (duration * 0.7f));
            damagePopupText.rectTransform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        damagePopupText.rectTransform.localPosition = originalPos;
        damagePopupText.rectTransform.localScale = Vector3.one;
        damagePopupText.gameObject.SetActive(false);
    }

    void KillPlayer()
    {
        // Immediately stop all gameplay
        if (GameManager.instance != null)
            GameManager.instance.play = false;

        PlayerKillLimit.PlayerKill -= PlayerKillLimit_PlayerKill;

        if (audioSource != null) audioSource.Play();
        // Backup death sound via PlayClipAtPoint in case the AudioSource is
        // misconfigured / disabled — guarantees the player hears 'game over'.
        AudioHelper.PlayDeath(transform.position);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && deathSprite != null) sr.sprite = deathSprite;

        if (playerCollider != null) playerCollider.enabled = false;
        if (extraCollider != null) extraCollider.enabled = false;

        recordHeight = (int)transform.position.y;
        HighScoreSet.SetHighscore(recordHeight + HighScoreSet.gameScore);
    }

    public void RestoreForContinue()
    {
        health = maxHealth;
        GetComponent<SpriteRenderer>().sprite = originalSprite;
        
        playerCollider.enabled = true;
        extraCollider.enabled = true;
        
        // Give player a little bump upwards and pause momentum so they don't fall back immediately
        transform.position += Vector3.up * 3f; 
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        
        // Reset invulnerability frames
        StartCoroutine(SpawnInvincibility()); 
        
        // Clear kill flag and resubscribe
        PlayerKillLimit.triggerEvent = false;
        PlayerKillLimit.PlayerKill += PlayerKillLimit_PlayerKill;
    }

    IEnumerator SpawnInvincibility()
    {
        isInvincible = true;
        yield return new WaitForSeconds(1.5f);
        isInvincible = false;
    }
}
