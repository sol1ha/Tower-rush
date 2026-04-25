using UnityEngine;

public class Laser_kill : MonoBehaviour
{
    [Tooltip("Optional — sprite used for kill bounds. Auto-finds on self or children.")]
    public SpriteRenderer killSprite;

    [Tooltip("Follow the bottom of the camera.")]
    public bool followCameraBottom = true;

    [Tooltip("Offset above the camera's bottom edge when following.")]
    public float bottomOffset = 0.5f;

    [Tooltip("Force the visible sprite's local position to zero on Start so it aligns with this object.")]
    public bool alignSpriteToSelf = true;

    [Tooltip("Kill the player if they fall below the laser's top edge (catches tunneling).")]
    public bool killIfBelow = true;

    [Header("Damage")]
    [Tooltip("Damage dealt per hit.")]
    public int damageAmount = 1;

    [Tooltip("How far above the laser the player is bumped after surviving a hit.")]
    public float respawnYOffset = 3f;

    private Camera cam;
    private Transform player;
    private Collider2D playerCollider;
    private Rigidbody2D playerRb;
    private Vector3 lastPlayerPos;
    private bool hasLastPos;
    private float nextHitTime;

    void Start()
    {
        cam = Camera.main;

        if (killSprite == null) killSprite = GetComponent<SpriteRenderer>();
        if (killSprite == null) killSprite = GetComponentInChildren<SpriteRenderer>();

        if (killSprite == null)
        {
            Debug.LogError("Laser_kill: no SpriteRenderer found.");
            enabled = false;
            return;
        }

        if (alignSpriteToSelf && killSprite.transform != transform)
        {
            killSprite.transform.localPosition = Vector3.zero;
            Debug.Log($"Laser_kill: aligned '{killSprite.name}' local position to (0,0,0).");
        }

        TryFindPlayer();
    }

    void TryFindPlayer()
    {
        var obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null)
        {
            player = obj.transform;
            playerCollider = obj.GetComponent<Collider2D>();
            playerRb = obj.GetComponent<Rigidbody2D>();
        }
    }

    void LateUpdate()
    {
        if (cam == null || !GameManager.Playing()) return;

        if (followCameraBottom)
        {
            Vector3 pos = transform.position;
            pos.y = cam.transform.position.y - cam.orthographicSize + bottomOffset;
            transform.position = pos;
        }

        CheckPlayerKill();
    }

    void CheckPlayerKill()
    {
        if (player == null) TryFindPlayer();
        if (player == null || killSprite == null) return;

        Bounds killBounds = killSprite.bounds;
        Bounds playerBounds = playerCollider != null ? playerCollider.bounds
                                                     : new Bounds(player.position, Vector3.one);

        bool hit = killBounds.Intersects(playerBounds);

        if (!hit && killIfBelow && player.position.y <= killBounds.max.y
            && player.position.x >= killBounds.min.x && player.position.x <= killBounds.max.x)
        {
            hit = true;
        }

        if (!hit && hasLastPos && killIfBelow)
        {
            if (lastPlayerPos.y > killBounds.max.y && player.position.y <= killBounds.max.y)
            {
                hit = true;
            }
        }

        lastPlayerPos = player.position;
        hasLastPos = true;

        if (hit) ApplyHit(killBounds);
    }

    void ApplyHit(Bounds killBounds)
    {
        if (Time.time < nextHitTime) return;
        nextHitTime = Time.time + 1.1f;

        var health = PlayerHealth.Instance;
        if (health == null)
        {
            Debug.LogWarning("Laser_kill: no PlayerHealth.Instance found; falling back to full kill.");
            PlayerKillLimit.TriggerEventStatic();
            return;
        }

        int before = health.GetHealth();
        health.DamagePlayer(damageAmount);
        int after = health.GetHealth();

        Debug.Log($"LASER HIT: health {before} -> {after}");

        if (after > 0 && player != null)
        {
            Vector3 p = player.position;
            p.y = killBounds.max.y + respawnYOffset;
            player.position = p;
            if (playerRb != null) playerRb.linearVelocity = Vector2.zero;
            lastPlayerPos = p;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision) { TryHitCollider(collision); }
    private void OnTriggerStay2D(Collider2D collision) { TryHitCollider(collision); }
    private void OnCollisionEnter2D(Collision2D collision) { TryHitCollider(collision.collider); }
    private void OnCollisionStay2D(Collision2D collision) { TryHitCollider(collision.collider); }

    void TryHitCollider(Collider2D col)
    {
        if (!GameManager.Playing() || col == null || !col.CompareTag("Player")) return;
        if (killSprite == null) return;
        ApplyHit(killSprite.bounds);
    }

    public void BoostSpeed() { }
    public void ResetSpeed() { }
}
