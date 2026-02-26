using UnityEngine;

public class RisingFloor3D : MonoBehaviour
{
    public float initialSpeed = 1f;
    public float acceleration = 0.05f;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = initialSpeed;
    }

    void Update()
    {
        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
        currentSpeed += acceleration * Time.deltaTime;
    }
}
