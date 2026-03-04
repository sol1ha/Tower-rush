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

    [Header("Player")]
    public Transform player;

    [Header("Settings")]
    [SerializeField] private float fallDeathY = -5f;
    [SerializeField] private float gameOverDelay = 2f;

    private float highestY = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (player == null || isGameOver) return;

        currentLevel = (score / 500) + 1;

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

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        if (UIManager.Instance != null)
            UIManager.Instance.DisplayNotification("Game Over! Score: " + score);

        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay()
    {
        yield return new WaitForSeconds(gameOverDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
