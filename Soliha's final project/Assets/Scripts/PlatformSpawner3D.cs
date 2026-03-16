using UnityEngine;
using System.Collections.Generic;

public class PlatformSpawner3D : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject platformPrefab;
    public GameObject coinPrefab;
    public GameObject spikePrefab;
    public GameObject jetPackPrefab;

    [Header("Spawn Settings")]
    public float platformSpacing = 1.115f;
    public float startY = 5.22f;

    [Header("Zig-Zag X Positions")]
    public float[] xPositions = { 2.79f, 3.615f, 4.44f };

    [Header("Base Spawn Chances (0 to 1)")]
    public float coinSpawnChance = 0.3f;
    public float baseSpikeChance = 0.5f;

    [Header("JetPack Settings")]
    public int jetPackSpawnAfterPlatform = 15;

    [Header("References")]
    public Transform player;

    private float spawnY;
    private int platformCount = 0;
    private bool jetPackSpawned = false;
    private List<GameObject> activePlatforms = new List<GameObject>();
    private List<GameObject> activeSpikes = new List<GameObject>();

    void Start()
    {
        spawnY = startY;

        for (int i = 0; i < 5; i++)
        {
            SpawnPlatform(isSafe: true);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.y > spawnY - (2 * platformSpacing))
        {
            SpawnPlatform(isSafe: false);
            CleanupPlatforms();
            CleanupSpikes();
        }
    }

    void SpawnPlatform(bool isSafe)
    {
        if (platformPrefab == null)
        {
            Debug.LogWarning("Platform Prefab not assigned on " + gameObject.name);
            return;
        }

        float targetX = xPositions[platformCount % xPositions.Length];
        Vector3 spawnPos = new Vector3(targetX, spawnY, 0f);
        Quaternion spawnRot = Quaternion.Euler(0, -88.331f, 0);

        GameObject newPlatform = Instantiate(platformPrefab, spawnPos, spawnRot);
        newPlatform.AddComponent<PlatformDecay>();
        newPlatform.AddComponent<PlatformColorizer>();
        activePlatforms.Add(newPlatform);

        if (!isSafe)
        {
            if (!jetPackSpawned && platformCount == jetPackSpawnAfterPlatform)
            {
                TrySpawnJetPack(spawnPos);
            }
            else
            {
                TrySpawnSpike(spawnPos);
            }

            TrySpawnCoins(spawnPos);
        }

        platformCount++;
        spawnY += platformSpacing;
    }

    void TrySpawnSpike(Vector3 platformPos)
    {
        if (spikePrefab == null)
        {
            Debug.LogWarning("Spike Prefab is NULL — assign it in Inspector!");
            return;
        }

        // No spikes on first 5 platforms (they are safe starting area)
        if (platformCount <= 5) return;

        // Spike chance increases with difficulty
        float spikeChance = baseSpikeChance;
        if (GameManager.Instance != null)
            spikeChance = Mathf.Min(baseSpikeChance * GameManager.Instance.DifficultyMultiplier, 0.6f);

        // Reduce chance on double-jump platforms
        bool isDoubleJumpPlatform = (platformCount % xPositions.Length == 0);
        if (isDoubleJumpPlatform)
            spikeChance *= 0.2f;

        if (Random.value >= spikeChance) return;

        // Spawn 2 to 3 spikes ON TOP of the platform (not parented, so rotation doesn't mess them up)
        int count = Random.Range(2, 4);
        float spacing = 0.6f;

        for (int i = 0; i < count; i++)
        {
            float xOffset = (i - (count - 1) * 0.5f) * spacing;
            Vector3 spikePos = new Vector3(platformPos.x + xOffset, platformPos.y + 0.7f, platformPos.z);
            GameObject spike = Instantiate(spikePrefab, spikePos, Quaternion.identity);
            spike.transform.localScale = new Vector3(0.3f, 0.9f, 0.3f);
            activeSpikes.Add(spike);
        }
    }

    void TrySpawnCoins(Vector3 platformPos)
    {
        if (coinPrefab == null) return;
        if (Random.value >= coinSpawnChance) return;

        int count = Random.Range(1, 3);
        for (int i = 0; i < count; i++)
        {
            Vector3 coinPos = new Vector3(platformPos.x + (i - 0.5f) * 1.5f, platformPos.y + 1.5f, platformPos.z);
            Instantiate(coinPrefab, coinPos, Quaternion.identity);
        }
    }

    void TrySpawnJetPack(Vector3 platformPos)
    {
        if (jetPackPrefab == null) return;

        Vector3 jetPackPos = new Vector3(platformPos.x, platformPos.y + 1f, platformPos.z);
        Instantiate(jetPackPrefab, jetPackPos, Quaternion.identity);
        jetPackSpawned = true;
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

    void CleanupSpikes()
    {
        while (activeSpikes.Count > 0 &&
               activeSpikes[0] != null &&
               activeSpikes[0].transform.position.y < player.position.y - 15f)
        {
            Destroy(activeSpikes[0]);
            activeSpikes.RemoveAt(0);
        }

        // Also clean up destroyed spike references
        activeSpikes.RemoveAll(s => s == null);
    }
}
