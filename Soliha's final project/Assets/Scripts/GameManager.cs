using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int score = 0;
    public int currentLevel = 1;
    public int multiplier = 1;
    public bool isGameOver = false;

    private float highestY = 0f;
    public Transform player;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Level Logic (every 500 points)
        currentLevel = (score / 500) + 1;

        // 2. Height-based Scoring
        if (player != null && !isGameOver)
        {
            if (player.position.y > highestY)
            {
                float distanceGained = player.position.y - highestY;
                AddScore(Mathf.RoundToInt(distanceGained * 10)); // 10 points per 1 unit of height
                highestY = player.position.y;
            }
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
        Debug.Log("Game Over! Score: " + score);
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
