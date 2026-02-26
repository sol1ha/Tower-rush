using UnityEngine;
using TMPro; // Make sure you have TextMeshPro installed!

public class UIManager : MonoBehaviour
{
    [Header("HUD Text Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI multiplierText;

    void Update()
    {
        // Update HUD strings every frame
        if (scoreText != null)
            scoreText.text = GameManager.Instance.score.ToString("D6"); // Formats as 000000 Like the image

        if (levelText != null)
            levelText.text = "LEVEL " + GameManager.Instance.currentLevel;

        if (multiplierText != null)
            multiplierText.text = "X" + GameManager.Instance.multiplier;
    }
}
