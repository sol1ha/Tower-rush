using UnityEngine;
using System.Collections;

public class PlatformDecay : MonoBehaviour
{
    public float baseDecayTime = 5f;
    private bool playerLeft = false;

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && !playerLeft)
        {
            playerLeft = true;
            StartCoroutine(DecayRoutine());
        }
    }

    private IEnumerator DecayRoutine()
    {
        // Platforms decay faster at higher difficulty
        float decayTime = baseDecayTime;
        if (GameManager.Instance != null)
            decayTime = baseDecayTime / GameManager.Instance.DifficultyMultiplier;

        yield return new WaitForSeconds(decayTime);
        Destroy(gameObject);
    }
}
