using UnityEngine;

public class Coin3D : MonoBehaviour
{
    public int pointValue = 100;
    public float rotationSpeed = 100f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddScore(pointValue);
            Destroy(gameObject);
        }
    }
}
