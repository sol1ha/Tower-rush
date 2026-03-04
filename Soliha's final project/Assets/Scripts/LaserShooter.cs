using UnityEngine;

public class LaserShooter : MonoBehaviour
{
    [Header("Settings")]
    public GameObject laserPrefab;
    public float fireRate = 2f;
    public float laserSpeed = 10f;
    public float laserLifeTime = 5f;

    private float nextFireTime;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            FireLaser();
            nextFireTime = Time.time + fireRate;
        }
    }

    void FireLaser()
    {
        if (laserPrefab == null) return;

        GameObject laser = Instantiate(laserPrefab, transform.position, transform.rotation);

        // LaserProjectile should be on the prefab; configure its values here
        if (laser.TryGetComponent(out LaserProjectile proj))
        {
            proj.speed = laserSpeed;
            proj.lifeTime = laserLifeTime;
        }
        else
        {
            Debug.LogWarning("LaserProjectile component missing on laserPrefab assigned to " + gameObject.name);
        }
    }
}
