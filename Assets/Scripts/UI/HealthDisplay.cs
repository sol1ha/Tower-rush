using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Health display with half-heart support.
/// Full heart = active full sprite, half heart = half-size or different color.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    public GameObject corazon1;
    public GameObject corazon2;
    public GameObject corazon3;

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

        // Full hearts
        corazon1.SetActive(health >= 1f);
        corazon2.SetActive(health >= 2f);
        corazon3.SetActive(health >= 3f);

        // Show half heart when health is 0.5, 1.5, or 2.5
        if (halfHeart != null)
        {
            bool showHalf = (health % 1f) >= 0.4f;
            halfHeart.SetActive(showHalf);

            // Position the half heart next to the last full heart
            if (showHalf)
            {
                int fullCount = Mathf.FloorToInt(health);
                GameObject lastHeart = fullCount >= 2 ? corazon3 :
                                       fullCount >= 1 ? corazon2 : corazon1;
                halfHeart.transform.position = lastHeart.transform.position;
            }
        }
    }
}
