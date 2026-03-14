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
    public float maxSpikeChance = 0.6f;
    [Tooltip("Distance threshold to detect a double-jump platform (large X gap)")]
    public float doubleJumpGapThreshold = 4f;

    public int boostEach;
    public int coinEach;
    public Vector3 coinOffset;
    private int boostCounter = 0;
    private int coinCounter = 5;

    private Transform player;
    Vector2 lastPosition = new Vector2(0, 0);
    private bool playerOnBoost = false;
    private Vector3 lastSimplePos = Vector3.zero;
    private int platformCount = 0;

    void Start()
    {
        if(generatorMode == GeneratorMode.Simple)
        {
            boostCounter = 0;
            Vector3 spawnPosition = new Vector3();
            for (int i = 0; i < numberOfPlatforms; i++)
            {
                spawnPosition.y += Random.Range(minY, maxY);
                spawnPosition.x = (Random.Range(-spawnPosition.x, spawnPosition.x) * 0.5f + Random.Range(-levelWidth, levelWidth) * 1.5f) / 2;
                boostCounter++;
                if (boostCounter % boostEach == 0)
                {
                    Instantiate(boostPrefab, spawnPosition, Quaternion.identity);
                }
                else
                {
                    GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                    platformCount++;
                    TrySpawnSpikes(platform, spawnPosition, lastSimplePos, platformCount);
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
                    GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
                    platformCount++;
                    TrySpawnSpikes(platform, spawnPosition, lastPosition, platformCount);
                }

                if(coinCounter % coinEach == 0)
                {
                    Instantiate(coinPrefab, spawnPosition + coinOffset, Quaternion.identity);
                }
                lastPosition = spawnPosition;
            }
        }
    }

    void TrySpawnSpikes(GameObject platform, Vector3 platformPos, Vector2 previousPos, int platformIndex)
    {
        if (spikePrefab == null) return;

        // Platforms 1-8 are safe — no spikes
        if (platformIndex <= spikeStartPlatform) return;

        // Calculate difficulty-scaled spike chance
        float currentChance = Mathf.Min(spikeChance + (platformPos.y / 100f) * spikeDifficultyScale, maxSpikeChance);

        // Detect double-jump platform (large horizontal gap)
        float xGap = Mathf.Abs(platformPos.x - previousPos.x);
        if (xGap >= doubleJumpGapThreshold)
        {
            currentChance *= 0.2f; // 80% reduction
        }

        if (Random.value > currentChance) return;

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
