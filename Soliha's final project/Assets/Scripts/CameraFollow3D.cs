using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 6f;

    [Header("Behaviour")]
    public bool followX = true;  // Uncheck in Inspector for a fully fixed side-view

    private float highestY;
    private float fixedX;

    void Start()
    {
        if (player == null) return;
        highestY = player.position.y;
        fixedX   = transform.position.x;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Camera only ever scrolls upward, never back down
        if (player.position.y > highestY)
            highestY = player.position.y;

        float targetX = followX ? player.position.x : fixedX;
        Vector3 desired = new Vector3(targetX, highestY, player.position.z) + offset;

        // Time.deltaTime makes the smooth follow frame-rate independent
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);

        // Always look at the player's current height reference point
        transform.LookAt(new Vector3(player.position.x, highestY, player.position.z));
    }
}
