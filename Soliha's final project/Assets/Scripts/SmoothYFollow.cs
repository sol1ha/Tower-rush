using UnityEngine;

public class SmoothYFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Settings")]
    public float smoothSpeed = 0.125f;
    
    private float fixedX;
    private float fixedZ;
    private float initialYOffset;

    void Start()
    {
        if (player != null)
        {
            // Store initial X and Z to keep them fixed
            fixedX = transform.position.x;
            fixedZ = transform.position.z;
            
            // Calculate initial Y distance from player
            initialYOffset = transform.position.y - player.position.y;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate desired position: Fixed X/Z, but Y follows player + offset
        float desiredY = player.position.y + initialYOffset;
        Vector3 desiredPosition = new Vector3(fixedX, desiredY, fixedZ);

        // Smoothly move the camera to the desired position
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
    }
}
