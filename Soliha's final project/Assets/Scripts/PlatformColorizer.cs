using UnityEngine;

/// Add this to a platform prefab (or let PlatformSpawner3D add it automatically).
/// On Awake it picks a random neon colour and applies it + emission to every
/// Renderer on the object and its children, giving each platform a unique glow.
///
/// REQUIRES: your platform material must have _EmissionColor (Standard or URP/Lit shader).
///           Enable "Emission" on the material in the Inspector at least once so Unity
///           bakes the keyword — this script will keep it on at runtime.
public class PlatformColorizer : MonoBehaviour
{
    [Header("Emission")]
    public float emissionIntensity = 2.5f;

    // The neon palette shown in the reference image
    private static readonly Color[] NeonPalette =
    {
        new Color(0.8f, 0f,   1f  ),   // purple
        new Color(0f,   0.9f, 1f  ),   // cyan
        new Color(1f,   0.3f, 0f  ),   // orange
        new Color(1f,   0f,   0.4f),   // hot pink
        new Color(0f,   1f,   0.4f),   // green-cyan
        new Color(1f,   0.9f, 0f  ),   // yellow
    };

    void Awake()
    {
        Color neon = NeonPalette[Random.Range(0, NeonPalette.Length)];
        Color emissive = neon * emissionIntensity;

        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            // Instance the material so platforms don't share one asset
            Material mat = r.material;
            mat.color = neon;

            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", emissive);
            }
        }
    }
}
