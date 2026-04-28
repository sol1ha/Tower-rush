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
    public float minY = 1.8f;
    public float maxY = 3f;

    [Header("Horizontal spread")]
    [Tooltip("Half-width of the area platforms can spawn in. Platforms sit in [-spreadX, +spreadX].")]
    public float spreadX = 14f;
    [Tooltip("Minimum X distance between two platforms in the same row.")]
    public float minXGap = 3f;
    [Tooltip("Maximum X distance from the previous row's main platform so jumps stay reachable.")]
    public float maxXGap = 9f;
    [Tooltip("Extra platforms placed at the same Y as the main platform. 0 = one platform per row (recommended).")]
    public int extraPlatformsPerRow = 0;
    [Tooltip("Chance (0-1) that a row gets an extra platform. Lower = fewer platforms overall.")]
    [Range(0f, 1f)] public float extraRowChance = 0.2f;

    [Header("Anti-overlap")]
    [Tooltip("Minimum X distance any new platform must keep from every recent platform when their Y is close.")]
    public float minNoOverlapX = 3f;
    [Tooltip("Vertical range in which the anti-overlap check is applied.")]
    public float overlapYWindow = 2f;
    [Tooltip("How many recent platforms to remember for the anti-overlap check.")]
    public int overlapMemory = 12;

    private readonly List<Vector2> recentPlatforms = new List<Vector2>();

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


    private bool generated;

    void Start()
    {
        if (!GameManager.Playing()) return;
        Generate();
    }

    void Update()
    {
        if (!generated && GameManager.Playing()) Generate();

        if(generatorMode == GeneratorMode.PositionBased && generated)
        {
            UpdatePositionBased();
        }
    }

    void Generate()
    {
        if (generated) return;
        generated = true;

        // Always spawn a starter platform above y=1 so all generated platforms stay above the target height
        Vector3 startPlatformPos = new Vector3(0f, 1f, 0f);
        Instantiate(platformPrefab, startPlatformPos, Quaternion.identity, transform);
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
                spawnPosition.x = PickSpreadX(lastSimplePos.x, spawnPosition.y);
                RememberPlatform(spawnPosition.x, spawnPosition.y);
                boostCounter++;
                coinCounter++;
                if (boostCounter % boostEach == 0)
                {
                    SpawnBoostFromBase(spawnPosition);
                }
                else
                {
                    bool rollResult = RollForSpikes(spawnPosition, lastSimplePos, platformCount + 1);
                    bool isCoin = coinCounter % coinEach == 0;

                    if (isCoin)
                    {
                        Instantiate(platformPrefab, spawnPosition, Quaternion.identity, transform);
                        platformCount++;
                        Instantiate(coinPrefab, spawnPosition + coinOffset, Quaternion.identity, transform);
                    }
                    else
                    {
                        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity, transform);
                        platformCount++;
                        if (rollResult) TrySpawnSpikes(platform, spawnPosition, lastSimplePos);
                    }
                    SpawnRowExtras(spawnPosition.y, spawnPosition.x);
                    lastSimplePos = spawnPosition;
                }

            }
        }
        else if(generatorMode == GeneratorMode.PositionBased)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
            boostCounter = 0;
        }
    }
    void UpdatePositionBased()
    {
        if(generatorMode == GeneratorMode.PositionBased)
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj == null) return;
                player = playerObj.transform;
            }
            if(player.position.y + 10 > lastPosition.y)
            {
                Vector3 spawnPosition = new Vector3(0f, lastPosition.y, 0f);
                spawnPosition.y += Random.Range(minY, maxY);
                spawnPosition.x = PickSpreadX(lastPosition.x, spawnPosition.y);
                RememberPlatform(spawnPosition.x, spawnPosition.y);
                boostCounter++;
                coinCounter++;
                if (boostCounter % boostEach == 0)
                {
                    SpawnBoostFromBase(spawnPosition);
                }
                else
                {
                    bool rollResult = RollForSpikes(spawnPosition, lastPosition, platformCount + 1);
                    bool isCoin = coinCounter % coinEach == 0;

                    if (isCoin)
                    {
                        Instantiate(platformPrefab, spawnPosition, Quaternion.identity, transform);
                        platformCount++;
                        Instantiate(coinPrefab, spawnPosition + coinOffset, Quaternion.identity, transform);
                    }
                    else
                    {
                        GameObject platform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity, transform);
                        platformCount++;
                        if (rollResult) TrySpawnSpikes(platform, spawnPosition, lastPosition);
                    }
                    SpawnRowExtras(spawnPosition.y, spawnPosition.x);
                }
                lastPosition = spawnPosition;
            }
        }
    }

    // Spawns a boost platform that visually matches the regular platform exactly
    // (same prefab, same size, same collider). It just gets isBoostPlatform=true
    // and a tinted sprite so it's recognizable.
    void SpawnBoostFromBase(Vector3 pos)
    {
        GameObject p = Instantiate(platformPrefab, pos, Quaternion.identity, transform);
        platformCount++;

        var plat = p.GetComponent<Platform>();
        if (plat != null) plat.isBoostPlatform = true;

        var sr = p.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = new Color(1f, 0.55f, 0.20f); // orange tint = boost
    }

    bool OverlapsRecent(float x, float y)
    {
        foreach (var p in recentPlatforms)
        {
            if (Mathf.Abs(p.y - y) > overlapYWindow) continue;
            if (Mathf.Abs(p.x - x) < minNoOverlapX) return true;
        }
        return false;
    }

    void RememberPlatform(float x, float y)
    {
        recentPlatforms.Add(new Vector2(x, y));
        if (recentPlatforms.Count > overlapMemory)
            recentPlatforms.RemoveAt(0);
    }

    float PickSpreadX(float previousX, float y)
    {
        for (int i = 0; i < 16; i++)
        {
            float candidate = Random.Range(-spreadX, spreadX);
            float gap = Mathf.Abs(candidate - previousX);
            if (gap < minXGap || gap > maxXGap) continue;
            if (OverlapsRecent(candidate, y)) continue;
            return candidate;
        }

        float sign = Random.value < 0.5f ? -1f : 1f;
        float fallback = previousX + sign * Random.Range(minXGap, maxXGap);
        return Mathf.Clamp(fallback, -spreadX, spreadX);
    }

    bool SpawnExtraRowPlatform(float y, List<float> occupiedX)
    {
        for (int attempt = 0; attempt < 16; attempt++)
        {
            float x = Random.Range(-spreadX, spreadX);
            bool tooClose = false;
            foreach (float ox in occupiedX)
            {
                if (Mathf.Abs(x - ox) < minXGap) { tooClose = true; break; }
            }
            if (tooClose) continue;
            if (OverlapsRecent(x, y)) continue;

            Vector3 pos = new Vector3(x, y, 0f);
            Instantiate(platformPrefab, pos, Quaternion.identity, transform);
            occupiedX.Add(x);
            RememberPlatform(x, y);
            platformCount++;
            return true;
        }
        return false;
    }

    void SpawnRowExtras(float y, float mainX)
    {
        if (extraPlatformsPerRow <= 0) return;
        if (Random.value > extraRowChance) return;
        var occupied = new List<float> { mainX };
        for (int i = 0; i < extraPlatformsPerRow; i++)
        {
            if (!SpawnExtraRowPlatform(y, occupied)) break;
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
