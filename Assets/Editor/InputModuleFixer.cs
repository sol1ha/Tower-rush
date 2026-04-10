#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[InitializeOnLoad]
public class InputModuleFixer
{
    static InputModuleFixer()
    {
        EditorApplication.delayCall += CheckAndFixInputModule;
        EditorApplication.hierarchyChanged += CheckAndFixInputModule;
    }

    private static void CheckAndFixInputModule()
    {
        var oldModules = Object.FindObjectsByType<StandaloneInputModule>();
        bool changed = false;
        foreach (var module in oldModules)
        {
            GameObject go = module.gameObject;
            Object.DestroyImmediate(module);
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            Debug.LogWarning("🚀 Auto-fixed legacy StandaloneInputModule on: " + go.name + ". Replaced with InputSystemUIInputModule.");
            changed = true;
        }

        if (changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
    }
}
#endif
