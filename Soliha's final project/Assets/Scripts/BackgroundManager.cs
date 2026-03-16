using UnityEngine;
using UnityEngine.UI;

/// Controls background transitions: Ground → Clouds → Space as player climbs.
///
/// Unity Setup:
/// 1. Create 3 UI Images under your Canvas (behind everything else):
///    - "GroundBG" — your ground/grass background
///    - "CloudBG" — your cloud background
///    - "SpaceBG" — a dark/starry space background
/// 2. Set all 3 to stretch across the full screen (Anchor: stretch-stretch)
/// 3. Set their order: GroundBG on top, CloudBG below, SpaceBG at the bottom
/// 4. Create an empty GameObject called "BackgroundManager"
/// 5. Add this script → drag the 3 images + player into the fields
public class BackgroundManager : MonoBehaviour
{
    [Header("Background Images")]
    public Image groundBG;
    public Image cloudBG;
    public Image spaceBG;

    [Header("Height Zones")]
    public float cloudStartHeight = 30f;
    public float cloudFullHeight = 60f;
    public float spaceStartHeight = 90f;
    public float spaceFullHeight = 120f;

    [Header("Sky Color (if using Camera background)")]
    public bool changeCameraColor = true;
    public Color groundColor = new Color(0.53f, 0.81f, 0.92f); // Light blue sky
    public Color cloudColor = new Color(0.85f, 0.85f, 0.95f);  // White-ish
    public Color spaceColor = new Color(0.02f, 0.02f, 0.1f);   // Dark blue/black

    [Header("References")]
    public Transform player;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;

        // Start fully visible ground, hidden cloud/space
        if (groundBG != null) SetAlpha(groundBG, 1f);
        if (cloudBG != null) SetAlpha(cloudBG, 0f);
        if (spaceBG != null) SetAlpha(spaceBG, 0f);
    }

    void Update()
    {
        if (player == null) return;

        float height = player.position.y;

        // Zone 1: Ground → Clouds
        // Ground fades out, clouds fade in
        if (height < cloudStartHeight)
        {
            // Pure ground
            if (groundBG != null) SetAlpha(groundBG, 1f);
            if (cloudBG != null) SetAlpha(cloudBG, 0f);
            if (spaceBG != null) SetAlpha(spaceBG, 0f);

            if (changeCameraColor && mainCam != null)
                mainCam.backgroundColor = groundColor;
        }
        else if (height < cloudFullHeight)
        {
            // Transitioning from ground to clouds
            float t = (height - cloudStartHeight) / (cloudFullHeight - cloudStartHeight);
            if (groundBG != null) SetAlpha(groundBG, 1f - t);
            if (cloudBG != null) SetAlpha(cloudBG, t);
            if (spaceBG != null) SetAlpha(spaceBG, 0f);

            if (changeCameraColor && mainCam != null)
                mainCam.backgroundColor = Color.Lerp(groundColor, cloudColor, t);
        }
        // Zone 2: Clouds → Space
        else if (height < spaceStartHeight)
        {
            // Pure clouds
            if (groundBG != null) SetAlpha(groundBG, 0f);
            if (cloudBG != null) SetAlpha(cloudBG, 1f);
            if (spaceBG != null) SetAlpha(spaceBG, 0f);

            if (changeCameraColor && mainCam != null)
                mainCam.backgroundColor = cloudColor;
        }
        else if (height < spaceFullHeight)
        {
            // Transitioning from clouds to space
            float t = (height - spaceStartHeight) / (spaceFullHeight - spaceStartHeight);
            if (groundBG != null) SetAlpha(groundBG, 0f);
            if (cloudBG != null) SetAlpha(cloudBG, 1f - t);
            if (spaceBG != null) SetAlpha(spaceBG, t);

            if (changeCameraColor && mainCam != null)
                mainCam.backgroundColor = Color.Lerp(cloudColor, spaceColor, t);
        }
        else
        {
            // Pure space
            if (groundBG != null) SetAlpha(groundBG, 0f);
            if (cloudBG != null) SetAlpha(cloudBG, 0f);
            if (spaceBG != null) SetAlpha(spaceBG, 1f);

            if (changeCameraColor && mainCam != null)
                mainCam.backgroundColor = spaceColor;
        }
    }

    void SetAlpha(Image img, float alpha)
    {
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
