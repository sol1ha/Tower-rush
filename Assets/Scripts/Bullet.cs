using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Tooltip("Units per second.")]
    public float speed = 12f;

    [Tooltip("Damage dealt to the player on contact.")]
    public int damage = 1;

    [Tooltip("Auto-destroy after this many seconds.")]
    public float lifetime = 8f;

    [HideInInspector] public Vector2 direction = Vector2.right;

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (direction.x != 0f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (direction.x >= 0 ? 1f : -1f);
            transform.localScale = s;
        }
    }

    void Update()
    {
        transform.position += (Vector3)(direction.normalized * speed * Time.deltaTime);
    }

    void HitPlayer(Collider2D col)
    {
        if (col == null || !col.CompareTag("Player")) return;

        var health = PlayerHealth.Instance;
        if (health != null) health.DamagePlayer(damage);
        else PlayerKillLimit.TriggerEventStatic();

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision) { HitPlayer(collision); }
    private void OnCollisionEnter2D(Collision2D collision) { HitPlayer(collision.collider); }
}
