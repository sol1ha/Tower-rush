using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject platformPrefab;

    [Header("Spawning Settings")]
    public Transform player;
    public float spawnDistanceAbove = 10f; // How far ahead to spawn
    public float platformGap = 3f;         // Vertical distance between platforms
    public float xRange = 5f;              // Random X variation

    private float nextSpawnY = 5f;

    void Update()
    {
        if (player == null) return;

        // While the next spawn position is within range of the player
        while (nextSpawnY < player.position.y + spawnDistanceAbove)
        {
            SpawnPlatform();
        }
    }

    void SpawnPlatform()
    {
        // Random X position, fixed Y increment, fixed Z
        Vector3 spawnPos = new Vector3(Random.Range(-xRange, xRange), nextSpawnY, 0);
        
        Instantiate(platformPrefab, spawnPos, Quaternion.identity);
        
        // Increment the next spawn height
        nextSpawnY += platformGap;
    }
}
