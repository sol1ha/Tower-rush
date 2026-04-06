using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Procedural, on demand map generation
/// </summary>
public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public enum GeneratorMode { Simple, PositionBased};

    public GeneratorMode generatorMode;
    public GameObject platformPrefab;
    public GameObject spikedPlatformPrefab;
    public GameObject boostPrefab;
    public GameObject coinPrefab;
    public GameObject spikePrefab;

    public int numberOfPlatforms = 200;
    public float levelWidth = 3f;
    public float minY = .2f;
    public float maxY = 1.5f;

    [Header("Spike Settings")]
    [Tooltip("First platform index where spikes can appear (0-based)")]
    public int spikeStartPlatform = 8;
    [Tooltip("Base chance (0-1) that a platform gets spikes")]
    public float spikeChance = 0.2f;
    [Tooltip("How much spike chance increases per 100 height units")]
    public float spikeDifficultyScale = 0.05f;
    [Tooltip("Max spike chance regardless of height")]
    public float maxSpikeChance = 0.4f;
    [Tooltip("Distance threshold to detect a double-jump platform (large X gap)")]
    public float doubleJumpGapThreshold = 4f;
    [Tooltip("Minimum safe platforms between spiked platforms")]
    public int safePlatformsBetweenSpikes = 2;

    public int boostEach;
    public int coinEach;
    public Vector3 coinOffset;
    private int boostCounter = 0;
    private int coinCounter = 5;

    private Transform player;
    Vector2 lastPosition = new Vector2(0, 0);
    private Vector3 lastSimplePos = Vector3.zero;
    private int platformCount = 0;
    private int platformsSinceLastSpike = 999; // Start high so first spikes can spawn


    void Start()
    {
        // Always spawn a starter platform above y=1 so all generated platforms stay above the target height
        Vector3 startPlatformPos = new Vector3(0f, 1f, 0f);
        Instantiate(platformPrefab, startPlatformPos, Quaternion.identity);
        platformCount = 1;
        lastPosition = new Vector2(startPlatformPos.x, startPlatformPos.y);
        lastSimplePos = startPlatformPos;

        if(generatorMode == GeneratorMode.Simple)
        {
            boostCounter = 0;
            
            Vector3 spawnPosition = Vector3.zero;
            
            for (int i = 0; i < numberOfPlatforms; i++)
            {
                spawnPosition.y += Random.Range(minY, maxY);
                spawnPosition.x = (Random.Range(-spawnPosition.x, spawnPosition.x) * 0.5f + Random.Range(-levelWidth, levelWidth) * 1.5f) / 2;
                boostCounter++;
                coinCounter++;
                if (boostCounter % boostEach == 0)
                {
                    Instantiate(boostPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    bool rollResult = RollForSpikes(spawnPosition, lastSimplePos, platformCount + 1);
                    bool isCoin = coinCounter % coinEach == 0;

                    if (isCoin)
                    {
                        Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                        Instantiate(coinPrefab, spawnPosition + coinOffset, Quaternion.identity);
                    }
                    else if (rollResult && spikedPlatformPrefab != null)
                    {
                        Instantiate(spikedPlatformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                    }
                    else
                    {
                        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                        if (rollResult) TrySpawnSpikes(platform, spawnPosition, lastSimplePos);
                    }
                    lastSimplePos = spawnPosition;
                }

            }
        }
        else if(generatorMode == GeneratorMode.PositionBased)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            boostCounter = 0;
        }
    }
    void Update()
    {
        if(generatorMode == GeneratorMode.PositionBased)
        {
            if(player.position.y + 10 > lastPosition.y)
            {
                Vector3 spawnPosition = new Vector3(lastPosition.x,lastPosition.y);
                spawnPosition.y += Random.Range(minY, maxY);
                spawnPosition.x = (Random.Range(-spawnPosition.x, spawnPosition.x) * 0.5f + Random.Range(-levelWidth, levelWidth) * 1.5f) / 2;
                boostCounter++;
                coinCounter++;
                if (boostCounter % boostEach == 0)
                {
                    Instantiate(boostPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    bool rollResult = RollForSpikes(spawnPosition, lastPosition, platformCount + 1);
                    bool isCoin = coinCounter % coinEach == 0;

                    if (isCoin)
                    {
                        Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                        Instantiate(coinPrefab, spawnPosition + coinOffset, Quaternion.identity);
                    }
                    else if (rollResult && spikedPlatformPrefab != null)
                    {
                        Instantiate(spikedPlatformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                    }
                    else
                    {
                        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                        platformCount++;
                        if (rollResult) TrySpawnSpikes(platform, spawnPosition, lastPosition);
                    }
                }
                lastPosition = spawnPosition;
            }
        }
    }

    bool RollForSpikes(Vector3 platformPos, Vector2 previousPos, int platformIndex)
    {
        // Platforms 1-8 are safe — no spikes
        if (platformIndex <= spikeStartPlatform) return false;

        // Must have at least 2 safe platforms between spiked ones
        platformsSinceLastSpike++;
        if (platformsSinceLastSpike <= safePlatformsBetweenSpikes) return false;

        // Calculate difficulty-scaled spike chance
        float currentChance = Mathf.Min(spikeChance + (platformPos.y / 100f) * spikeDifficultyScale, maxSpikeChance);

        // Detect double-jump platform (large horizontal gap)
        float xGap = Mathf.Abs(platformPos.x - previousPos.x);
        if (xGap >= doubleJumpGapThreshold)
        {
            currentChance *= 0.2f; // 80% reduction
        }

        if (Random.value > currentChance) return false;

        // Reset cooldown — next 2 platforms will be safe
        platformsSinceLastSpike = 0;
        return true;
    }

    void TrySpawnSpikes(GameObject platform, Vector3 platformPos, Vector2 previousPos)
    {
        if (spikePrefab == null) return;

        // Spawn 2 or 3 spikes, spread evenly across the platform
        int spikeCount = Random.Range(2, 4); // 2 or 3
        SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
        float platformWidth = sr != null ? sr.bounds.size.x : 1.5f;
        float platformTop = sr != null ? sr.bounds.size.y * 0.5f : 0.2f;

        float spacing = platformWidth / (spikeCount + 1);
        float startX = platformPos.x - platformWidth * 0.5f;

        for (int i = 0; i < spikeCount; i++)
        {
            float x = startX + spacing * (i + 1);
            Vector3 spikePos = new Vector3(x, platformPos.y + platformTop, 0f);
            GameObject spike = Instantiate(spikePrefab, spikePos, Quaternion.identity);
            spike.transform.SetParent(platform.transform);
        }
    }
}
