using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns decorative space props at a given height
/// </summary>
public class SpaceProps : MonoBehaviour
{
    public Transform player;

    public GameObject satellite;
    public int spawnOnHeight;
    private float cooldown;
    private bool inSpace;
    // Start is called before the first frame update
    void Start()
    {
        inSpace = false;   
    }

    // Disabled per user request — no flying satellites.
    void Update() { }
}
