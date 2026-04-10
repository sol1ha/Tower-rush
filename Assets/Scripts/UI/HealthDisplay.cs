using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Health display with half-heart support.
/// Full heart = active full sprite, half heart = half-size or different color.
/// </summary>
public class HealthDisplay : MonoBehaviour
{
    private GameObject heart1;
    private GameObject heart2;
    private GameObject heart3;

    void Start()
    {
        // Finds exactly the 3 items by deep global search, ignoring where this script is attached
        GameObject healthContainer = GameObject.Find("HealthDisplay");
        if (healthContainer != null) 
        {
            if (healthContainer.transform.Find("lives") != null) heart1 = healthContainer.transform.Find("lives").gameObject;
            if (healthContainer.transform.Find("lives2") != null) heart2 = healthContainer.transform.Find("lives2").gameObject;
            if (healthContainer.transform.Find("lives3") != null) heart3 = healthContainer.transform.Find("lives3").gameObject;
        }

        // Fallbacks in case names are different
        if (heart1 == null) heart1 = GameObject.Find("lives");
        if (heart2 == null) heart2 = GameObject.Find("lives2");
        if (heart3 == null) heart3 = GameObject.Find("lives3");
    }

    void Update()
    {
        if (PlayerHealth.Instance == null) return;

        int currentHealth = Mathf.CeilToInt(PlayerHealth.Instance.GetHealthFloat());

        // Heart 1 stays on if health is 1, 2, or 3
        if (heart1 != null) heart1.SetActive(currentHealth >= 1);
        
        // Heart 2 stays on if health is 2 or 3
        if (heart2 != null) heart2.SetActive(currentHealth >= 2);
        
        // Heart 3 stays on ONLY if health is 3
        if (heart3 != null) heart3.SetActive(currentHealth >= 3);
    }
}
