using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class FitBackgroundToCamera : MonoBehaviour
{
    public Camera targetCamera;
    public bool followCameraPosition = true;
    public float zOffset = 10f;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        if (targetCamera == null) targetCamera = Camera.main;
        if (targetCamera == null || sr == null || sr.sprite == null) return;

        if (sr.drawMode != SpriteDrawMode.Simple)
            sr.drawMode = SpriteDrawMode.Simple;

        float worldHeight = targetCamera.orthographicSize * 2f;
        float worldWidth = worldHeight * targetCamera.aspect;

        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

        transform.localScale = new Vector3(
            worldWidth / spriteSize.x,
            worldHeight / spriteSize.y,
            1f
        );

        if (followCameraPosition)
        {
            Vector3 camPos = targetCamera.transform.position;
            transform.position = new Vector3(camPos.x, camPos.y, camPos.z + zOffset);
        }
    }
}
