using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Health display with half-heart support.
/// Full heart = active full sprite, half heart = half-size or different color.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    public List<GameObject> fullHearts;

    [Header("Half Heart (optional)")]
    public GameObject halfHeart;

    private PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();

        if (halfHeart != null)
            halfHeart.SetActive(false);
    }

    void Update()
    {
        float health = playerHealth.GetHealthFloat();
        int maxHealth = playerHealth.maxHealth;

        // Ensure we don't exceed the number of heart objects
        int heartsToShow = Mathf.Min(maxHealth, fullHearts.Count);

        // Full hearts
        for (int i = 0; i < fullHearts.Count; i++)
        {
            if (fullHearts[i] != null)
                fullHearts[i].SetActive(health >= i + 1);
        }

        // Show half heart when health has a fractional part >= 0.4
        if (halfHeart != null)
        {
            bool showHalf = (health % 1f) >= 0.4f && health < maxHealth;
            halfHeart.SetActive(showHalf);

            // Position the half heart next to the last full heart
            if (showHalf)
            {
                int fullCount = Mathf.FloorToInt(health);
                if (fullCount < fullHearts.Count && fullHearts[fullCount] != null)
                {
                    halfHeart.transform.position = fullHearts[fullCount].transform.position;
                }
            }
        }
    }
}
