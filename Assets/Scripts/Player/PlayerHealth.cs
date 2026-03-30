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
    private float invincibleTime = 1.5f;
    private float lastHitTime = -999f;

    public ParticleSystem bloodHitPS;
    public BoxCollider2D extraCollider;
    public SpriteRenderer shieldVisual;

    private BoxCollider2D playerCollider;
    private int recordHeight;
    private AudioSource audioSource;

    private void Awake()
    {
        health = maxHealth;
    }

    void Start()
    {
        bloodHitPS = GetComponent<ParticleSystem>();
        playerCollider = GetComponent<BoxCollider2D>();
        audioSource = GetComponent<AudioSource>();
        PlayerKillLimit.PlayerKill += PlayerKillLimit_PlayerKill;

        if (damagePopupText != null)
            damagePopupText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (HasShield)
            shieldVisual.enabled = true;
        else
            shieldVisual.enabled = false;
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
    /// Called by spikes. Only damages if player has landed (not jumping up).
    /// All spikes on one platform = 1 life thanks to invincibility frames.
    /// Last heart takes 0.5 damage instead of 1.
    /// </summary>
    public void DamagePlayer(int amount)
    {
        // Invincibility frames — all spikes on same platform = 1 hit
        if (Time.time - lastHitTime < invincibleTime) return;
        lastHitTime = Time.time;

        if (HasShield)
        {
            HasShield = false;
            return;
        }

        bloodHitPS.Play(false);

        // Half heart on last life
        float actualDamage = health <= 1f ? 0.5f : (float)amount;
        health -= actualDamage;

        // Show floating damage text
        ShowDamagePopup(actualDamage);

        if (health <= 0f)
        {
            PlayerKillLimit.TriggerEventStatic();
        }
    }

    /// <summary>
    /// Called by spikes — only damages if player is falling or standing (not jumping up).
    /// </summary>
    public void DamagePlayerIfLanded(int amount, Rigidbody2D playerRb)
    {
        // If moving upward fast, player is jumping through — no damage
        if (playerRb != null && playerRb.linearVelocity.y > 1f) return;

        DamagePlayer(amount);
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
        audioSource.Play();
        PlayerKillLimit.PlayerKill -= PlayerKillLimit_PlayerKill;
        GetComponent<SpriteRenderer>().sprite = deathSprite;
        playerCollider.enabled = false;
        extraCollider.enabled = false;
        recordHeight = (int)transform.position.y;
        HighScoreSet.SetHighscore(recordHeight + HighScoreSet.gameScore);
    }
}
