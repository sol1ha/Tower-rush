using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dynamic background manager: Ground → Clouds → Space.
/// Each layer fades out as the player climbs, revealing the next layer behind it.
/// Layer order (back to front): Space (always visible behind) → Clouds → Ground
/// </summary>
public class DynamicBackgrounds : MonoBehaviour
{
    public Transform player;

    [Header("Height Thresholds")]
    [Tooltip("Height where ground starts fading out and clouds appear")]
    public int groundCeiling = 50;
    [Tooltip("Height where clouds start fading out and space appears")]
    public int skyCeiling = 150;

    [Header("Background Layers (front to back)")]
    public SpriteRenderer[] groundSprites;
    public SpriteRenderer[] cloudSprites;
    public SpriteRenderer[] skySprites;

    [Header("Fade Speeds")]
    public float groundFadeSpeed = 2f;
    public float cloudFadeSpeed = 0.3f;

    Color whiteNoAlpha = new Color(1, 1, 1, 0);
    Color whiteFullAlpha = new Color(1, 1, 1, 1);
    bool fadingGround;
    bool fadingClouds;

    void Start()
    {
        fadingGround = false;
        fadingClouds = false;

        // Clouds start invisible — they fade IN as ground fades out
        foreach (SpriteRenderer sr in cloudSprites)
        {
            sr.color = whiteNoAlpha;
        }
    }

    void Update()
    {
        if (player.position.y > groundCeiling)
        {
            fadingGround = true;
        }
        if (player.position.y > skyCeiling)
        {
            fadingClouds = true;
        }

        // Ground fades out, clouds fade in
        if (fadingGround)
        {
            foreach (SpriteRenderer sr in groundSprites)
            {
                sr.color = Color.Lerp(sr.color, whiteNoAlpha, groundFadeSpeed * Time.deltaTime);
            }
            foreach (SpriteRenderer sr in cloudSprites)
            {
                sr.color = Color.Lerp(sr.color, whiteFullAlpha, groundFadeSpeed * Time.deltaTime);
            }
        }

        // Clouds fade out, revealing space behind
        if (fadingClouds)
        {
            foreach (SpriteRenderer sr in cloudSprites)
            {
                sr.color = Color.Lerp(sr.color, whiteNoAlpha, cloudFadeSpeed * Time.deltaTime);
            }
        }
    }
}
