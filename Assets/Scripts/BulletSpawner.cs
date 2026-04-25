using System.Collections;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("The bullet prefab. Must have the Bullet script attached.")]
    public Bullet bulletPrefab;

    [Header("Timing")]
    [Tooltip("Minimum seconds between waves.")]
    public float minInterval = 60f;
    [Tooltip("Maximum seconds between waves.")]
    public float maxInterval = 60f;
    [Tooltip("Warning shown this long before bullets actually fire.")]
    public float warningDuration = 5f;
    [Tooltip("First wave waits at least this long after the game starts.")]
    public float initialDelay = 60f;

    [Header("Spawn sides")]
    public bool spawnLeft = true;
    public bool spawnRight = true;
    [Tooltip("Extra units outside the camera edge where bullets appear.")]
    public float spawnEdgeMargin = 1f;

    [Header("Wave")]
    [Tooltip("How many bullets fire in one wave.")]
    public int bulletsPerWave = 1;
    [Tooltip("Delay between bullets in the same wave.")]
    public float bulletSpacing = 0.15f;
    [Tooltip("Random Y offset (around the camera center) where each bullet spawns.")]
    public float spawnYJitter = 2.5f;

    [Header("UI")]
    [Tooltip("The warning bar shown before bullets fire.")]
    public BulletWarning warning;

    private Camera cam;
    private float nextWaveTime;
    private bool waveQueued;
    private bool nextWaveFromLeft;

    void Start()
    {
        cam = Camera.main;
        nextWaveTime = Time.time + initialDelay;
    }

    void Update()
    {
        if (cam == null || bulletPrefab == null) return;
        if (!GameManager.Playing()) return;

        if (!waveQueued && Time.time >= nextWaveTime - warningDuration)
        {
            waveQueued = true;
            nextWaveFromLeft = PickSide();
            if (warning != null) warning.Show(warningDuration, nextWaveFromLeft);
            StartCoroutine(FireWaveAfter(warningDuration));
        }
    }

    bool PickSide()
    {
        if (spawnLeft && spawnRight) return Random.value < 0.5f;
        if (spawnLeft) return true;
        return false;
    }

    IEnumerator FireWaveAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < bulletsPerWave; i++)
        {
            SpawnOne(nextWaveFromLeft);
            if (i < bulletsPerWave - 1) yield return new WaitForSeconds(bulletSpacing);
        }

        nextWaveTime = Time.time + Random.Range(minInterval, maxInterval);
        waveQueued = false;
    }

    void SpawnOne(bool fromLeft)
    {
        if (!spawnLeft && !spawnRight) return;

        float halfWidth = cam.orthographicSize * cam.aspect;
        float camX = cam.transform.position.x;
        float camY = cam.transform.position.y;

        float x = fromLeft ? camX - halfWidth - spawnEdgeMargin
                           : camX + halfWidth + spawnEdgeMargin;
        float y = camY + Random.Range(-spawnYJitter, spawnYJitter);

        Bullet b = Instantiate(bulletPrefab, new Vector3(x, y, 0f), Quaternion.identity);
        b.direction = fromLeft ? Vector2.right : Vector2.left;
    }
}
