using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=red>Killed by Spikes!</color>");
            GameManager.Instance.GameOver();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=red>Killed by Spikes!</color>");
            GameManager.Instance.GameOver();
        }
    }
}
