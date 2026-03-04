using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject platformPrefab;
    public GameObject coinPrefab;
    public GameObject spikePrefab;

    [Header("Spawn Settings")]
    public float platformSpacing = 1.115f;
    public float startY = 5.22f;

    [Header("Zig-Zag X Positions")]
    public float[] xPositions = { 2.79f, 3.615f, 4.44f };

    [Header("Spawn Chances (0 to 1)")]
    public float coinSpawnChance = 0.3f;
    public float spikeSpawnChance = 0.2f;

    [Header("References")]
    public Transform player;

    private float spawnY;
    private int platformCount = 0;
    private List<GameObject> activePlatforms = new List<GameObject>();

    void Start()
    {
        spawnY = startY;

        // Spawn 5 safe initial platforms so the player always has somewhere to start
        for (int i = 0; i < 5; i++)
        {
            SpawnPlatform(isSafe: true);
        }
    }

    void Update()
    {
        if (player == null) return;

        // Keep spawning ahead of the player
        if (player.position.y > spawnY - (2 * platformSpacing))
        {
            SpawnPlatform(isSafe: false);
            CleanupPlatforms();
        }
    }

    void SpawnPlatform(bool isSafe)
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning("Platform Prefab not assigned on " + gameObject.name);
            return;
        }

        // Zig-zag: cycle through xPositions array
        float targetX = xPositions[platformCount % xPositions.Length];
        Vector3 spawnPos = new Vector3(targetX, spawnY, 0f);
        Quaternion spawnRot = Quaternion.Euler(0, -88.331f, 0);

        GameObject newPlatform = Instantiate(platformPrefab, spawnPos, spawnRot);
        newPlatform.AddComponent<PlatformDecay>();
        newPlatform.AddComponent<PlatformColorizer>();
        activePlatforms.Add(newPlatform);

        if (!isSafe)
        {
            TrySpawnSpike(spawnPos, newPlatform.transform);
            TrySpawnCoins(spawnPos, newPlatform.transform);
        }

        platformCount++;
        spawnY += platformSpacing;
    }

    void TrySpawnSpike(Vector3 platformPos, Transform parent)
    {
        if (spikePrefab == null) return;
        if (platformCount <= 8) return;
        if (Random.value >= spikeSpawnChance) return;

        Vector3 spikePos = platformPos + Vector3.up * 0.5f;
        Instantiate(spikePrefab, spikePos, Quaternion.identity, parent);
    }

    void TrySpawnCoins(Vector3 platformPos, Transform parent)
    {
        if (coinPrefab == null) return;

        int count = Random.Range(2, 4);
        for (int i = 0; i < count; i++)
        {
            Vector3 coinPos = platformPos + Vector3.up * 1.5f + Vector3.right * (i - 1f) * 1.5f;
            Instantiate(coinPrefab, coinPos, Quaternion.identity, parent);
        }
    }

    void CleanupPlatforms()
    {
        while (activePlatforms.Count > 0 &&
               activePlatforms[0] != null &&
               activePlatforms[0].transform.position.y < player.position.y - 15f)
        {
            Destroy(activePlatforms[0]);
            activePlatforms.RemoveAt(0);
        }
    }
}
