using UnityEngine;

/// <summary>
/// Kept as a stub so the existing scene reference doesn't break — the death
/// retry / press-any-key-to-reload flow has been intentionally removed, so
/// once the player dies the run is over and there's no way to restart from
/// inside the play session.
/// </summary>
public class ReloadScene : MonoBehaviour
{
    public static bool CanReloadAfterDeath = false;
}
