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
            // Use AddCoinScore for combo multiplier tracking
            if (GameManager.Instance != null)
                GameManager.Instance.AddCoinScore(pointValue);
            Destroy(gameObject);
        }
    }
}
