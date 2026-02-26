using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject platformPrefab;
    public GameObject coinPrefab;
    public GameObject spikePrefab;

    [Header("Spawn Settings")]
    public int initialPlatforms = 10;
    public float platformSpacing = 4f;
    public float xRange = 5f;
    public float zRange = 2f;
    
    [Header("Spawn Chances (0 to 1)")]
    public float coinSpawnChance = 0.3f;
    public float spikeSpawnChance = 0.2f;

    private float spawnY = 0f;
    private List<GameObject> activePlatforms = new List<GameObject>();
    public Transform player;

    void Start()
    {
        // Don't spawn hazards on the very first few platforms to give the player a safe start
        for (int i = 0; i < initialPlatforms; i++)
        {
            SpawnPlatform(i < 3); // isSafe = true for the first 3
        }
    }

    void Update()
    {
        if (player != null && player.position.y > spawnY - (initialPlatforms * platformSpacing))
        {
            SpawnPlatform(false);
            CleanupPlatforms();
        }
    }

    void SpawnPlatform(bool isSafe)
    {
        Vector3 spawnPos = new Vector3(Random.Range(-xRange, xRange), spawnY, Random.Range(-zRange, zRange));
        GameObject newPlatform = Instantiate(platformPrefab, spawnPos, Quaternion.identity);
        activePlatforms.Add(newPlatform);

        if (!isSafe)
        {
            // Spawn Spikes (on top of platform)
            if (spikePrefab != null && Random.value < spikeSpawnChance)
            {
                // Spikes usually go slightly above the platform surface
                Vector3 spikePos = spawnPos + Vector3.up * 0.5f; 
                Instantiate(spikePrefab, spikePos, Quaternion.identity, newPlatform.transform);
            }
            // OR Spawn Coin
            else if (coinPrefab != null && Random.value < coinSpawnChance)
            {
                Vector3 coinPos = spawnPos + Vector3.up * 1.5f;
                Instantiate(coinPrefab, coinPos, Quaternion.identity, newPlatform.transform);
            }
        }

        spawnY += platformSpacing;
    }

    void CleanupPlatforms()
    {
        // Keep a healthy buffer of platforms below the player
        while (activePlatforms.Count > 0 && activePlatforms[0].transform.position.y < player.position.y - 15f)
        {
            GameObject oldPlatform = activePlatforms[0];
            activePlatforms.RemoveAt(0);
            Destroy(oldPlatform);
        }
    }
}
