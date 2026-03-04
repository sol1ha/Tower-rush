using UnityEngine;
using System.Collections;

public class PlatformDecay : MonoBehaviour
{
    public float decayTime = 5f;
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
        yield return new WaitForSeconds(decayTime);
        Destroy(gameObject);
    }
}
