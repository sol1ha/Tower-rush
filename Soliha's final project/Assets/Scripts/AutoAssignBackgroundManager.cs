using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class AutoAssignBackgroundManager
{
    [MenuItem("Tools/Auto-Assign Background Manager")]
    public static void AssignFields()
    {
        BackgroundManager bgManager = Object.FindObjectOfType<BackgroundManager>();
        
        if (bgManager == null)
        {
            Debug.LogError("Could not find a BackgroundManager in the scene.");
            return;
        }

        // Try to find the images by name
        GameObject groundObj = GameObject.Find("GroundBG");
        GameObject cloudObj = GameObject.Find("CloudBG");
        GameObject spaceObj = GameObject.Find("SpaceBG");
        
        if (groundObj != null) bgManager.groundBG = groundObj.GetComponent<Image>();
        if (cloudObj != null) bgManager.cloudBG = cloudObj.GetComponent<Image>();
        if (spaceObj != null) bgManager.spaceBG = spaceObj.GetComponent<Image>();

        // Try to find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            bgManager.player = playerObj.transform;
        }
        else
        {
            // Fallback: try finding by name
            playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                 bgManager.player = playerObj.transform;
            }
        }

        // Mark the object as dirty so Unity saves the changes
        EditorUtility.SetDirty(bgManager);
        
        Debug.Log("BackgroundManager fields auto-assigned successfully!");
    }
}
