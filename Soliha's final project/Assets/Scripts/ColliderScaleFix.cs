using UnityEngine;

/// <summary>
/// Utility script to fix "BoxCollider does not support negative scale" errors.
/// This script enforces a positive local scale for the object it is attached to.
/// </summary>
public class ColliderScaleFix : MonoBehaviour
{
    private void Awake()
    {
        // Enforce positive scale to avoid BoxCollider issues
        Vector3 currentScale = transform.localScale;
        transform.localScale = new Vector3(Mathf.Abs(currentScale.x), Mathf.Abs(currentScale.y), Mathf.Abs(currentScale.z));
    }
}
