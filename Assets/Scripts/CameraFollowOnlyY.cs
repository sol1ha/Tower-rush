using UnityEngine;

public class CameraFollowOnlyY : MonoBehaviour
{
    public Transform target;
    [Tooltip("How quickly the camera catches up to the player (higher = tighter follow). 12-20 feels good.")]
    public float followLerpSpeed = 14f;

    void LateUpdate()
    {
        if (target == null) return;

        if (target.position.y > transform.position.y)
        {
            Vector3 newPos = new Vector3(transform.position.x, target.position.y, transform.position.z);
            float t = 1f - Mathf.Exp(-followLerpSpeed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, newPos, t);
        }
    }
}
