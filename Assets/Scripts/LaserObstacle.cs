using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserObstacle : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float startDelay = 60f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < startDelay) return;

        // Move upward at 'speed' units per second
        transform.Translate(Vector3.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Laser hit player!");
            Destroy(other.gameObject);
            
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Laser hit player!");
            Destroy(collision.gameObject);
            
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }
    }

    // 3D variants just in case the project is 3D (though 'Rush' usually implies 2D/2.5D, the previous conversation mentioned 3D platforms)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Laser hit player (3D)!");
            Destroy(other.gameObject);
            
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Laser hit player (3D)!");
            Destroy(collision.gameObject);
            
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver();
            }
        }
    }
}
