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

        // Spawn the laser at the shooter's position and orientation
        GameObject laser = Instantiate(laserPrefab, transform.position, transform.rotation);
        
        // Add movement component or just set its velocity if it has a Rigidbody
        LaserProjectile proj = laser.AddComponent<LaserProjectile>();
        proj.speed = laserSpeed;
        proj.lifeTime = laserLifeTime;
    }
}

public class LaserProjectile : MonoBehaviour
{
    public float speed;
    public float lifeTime;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Move in the forward direction of the object
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=red>Killed by Laser!</color>");
            GameManager.Instance.GameOver();
        }
    }
}
