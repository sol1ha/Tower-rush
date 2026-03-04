using UnityEngine;

public class RisingFloor3D : MonoBehaviour
{
    public float moveSpeed = 0.2f;
    public float delayBeforeStart = 5f;
    private float startTime;

    void Start()
    {
        startTime = Time.time + delayBeforeStart;
        
        if (UIManager.Instance != null)
            UIManager.Instance.DisplayNotification("The floor is rising!");
    }

    void Update()
    {
        if (Time.time < startTime) return;

        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Platform"))
        {
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            GameManager.Instance.GameOver();
        }
        else if (other.name.ToLower().Contains("floor") && other.gameObject != gameObject)
        {
            Destroy(gameObject); 
        }
    }
}
