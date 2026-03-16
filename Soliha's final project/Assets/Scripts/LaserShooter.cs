using UnityEngine;

public class LaserShooter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject laserPrefab;
    public float baseFireRate = 3f;
    public float laserSpeed = 10f;
    public float laserLifeTime = 5f;

    [Header("Tracking Player")]
    public Transform player;
    public float normalTrackingSpeed = 2f;
    public float boostTrackingSpeed = 15f; // Fast enough to keep up with jetpack

    private float nextFireTime;
    private PlayerController3D pController;

    void Start()
    {
        if (player != null)
        {
            pController = player.GetComponent<PlayerController3D>();
        }
    }

    void Update()
    {
        if (player != null)
        {
            // Follow the player's Y position
            float targetY = player.position.y;
            float currentSpeed = normalTrackingSpeed;

            // Go upwards much faster if the player is using the jetpack (boost platform)
            if (pController != null && pController.IsJetPacking)
            {
                currentSpeed = boostTrackingSpeed;
            }

            Vector3 newPos = transform.position;
            newPos.y = Mathf.Lerp(transform.position.y, targetY, currentSpeed * Time.deltaTime);
            transform.position = newPos;
        }

        if (Time.time >= nextFireTime)
        {
            FireLaser();

            // Fire rate decreases (faster) with difficulty
            float fireRate = baseFireRate;
            if (GameManager.Instance != null)
                fireRate = Mathf.Max(baseFireRate / GameManager.Instance.DifficultyMultiplier, 1f);

            nextFireTime = Time.time + fireRate;
        }
    }

    void FireLaser()
    {
        if (laserPrefab == null) return;

        GameObject laser = Instantiate(laserPrefab, transform.position, transform.rotation);

        if (laser.TryGetComponent(out LaserProjectile proj))
        {
            proj.speed = laserSpeed;
            proj.lifeTime = laserLifeTime;
        }
    }
}
