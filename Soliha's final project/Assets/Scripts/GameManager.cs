using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game State")]
    public int score = 0;
    public int currentLevel = 1;
    public int multiplier = 1;
    public bool isGameOver = false;

    [Header("Lives")]
    public float maxLives = 3f;
    public float currentLives = 3f;
    public float invincibleTime = 1.5f;
    private float lastHitTime = -999f;

    [Header("Player")]
    public Transform player;

    [Header("Settings")]
    [SerializeField] private float fallDeathY = -5f;
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float comboTimeout = 3f;

    private float highestY = 0f;
    private float lastCoinTime = -999f;

    public float DifficultyMultiplier => 1f + (currentLevel - 1) * 0.15f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentLives = maxLives;
    }

    void Update()
    {
        if (player == null || isGameOver) return;

        currentLevel = (score / 500) + 1;

        if (multiplier > 1 && Time.time - lastCoinTime > comboTimeout)
        {
            multiplier = 1;
        }

        if (player.position.y < fallDeathY)
        {
            GameOver();
            return;
        }

        if (player.position.y > highestY)
        {
            float distanceGained = player.position.y - highestY;
            AddScore(Mathf.RoundToInt(distanceGained * 10));
            highestY = player.position.y;
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;
        score += points * multiplier;
    }

    public void AddCoinScore(int points)
    {
        if (isGameOver) return;

        if (Time.time - lastCoinTime < comboTimeout)
        {
            multiplier = Mathf.Min(multiplier + 1, 5);
            if (UIManager.Instance != null)
                UIManager.Instance.DisplayNotification("COMBO X" + multiplier + "!");
        }
        else
        {
            multiplier = 1;
        }

        lastCoinTime = Time.time;
        score += points * multiplier;
    }

    // Called by spikes — takes 1 heart normally, 0.5 when on last heart
    public bool TakeDamage()
    {
        if (isGameOver) return false;

        // Invincibility frames — all spikes on same platform only hit once
        if (Time.time - lastHitTime < invincibleTime) return true;
        lastHitTime = Time.time;

        // When at 1 or less, only take half heart
        float damage = currentLives <= 1f ? 0.5f : 1f;
        currentLives -= damage;

        // Screen shake
        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.2f, 0.15f);

        // Show floating damage text
        if (UIManager.Instance != null)
        {
            string heartSymbol = "\u2764";
            string damageText = damage >= 1f ? "-1 " + heartSymbol : "-0.5 " + heartSymbol;
            UIManager.Instance.ShowDamagePopup(damageText);
        }

        if (currentLives <= 0f)
        {
            GameOver();
            return false;
        }

        return true;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.5f, 0.3f);

        if (GameOverScreen.Instance != null)
        {
            GameOverScreen.Instance.Show(score);
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.DisplayNotification("Game Over! Score: " + score);
            StartCoroutine(ReloadAfterDelay());
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);
        RestartGame();
    }
}
