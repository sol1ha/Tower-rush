using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 5, -10);
    public float smoothSpeed = 0.125f;

    private float highestY;

    void Start()
    {
        if (player != null) highestY = player.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Only move upward
        if (player.position.y > highestY)
        {
            highestY = player.position.y;
        }

        Vector3 desiredPosition = new Vector3(player.position.x, highestY, player.position.z) + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(new Vector3(player.position.x, highestY, player.position.z));
    }
}
