#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One-shot editor script: on Unity startup, if `Assets/TextMesh Pro/` is missing,
/// find the TMP Essentials .unitypackage in the ugui package cache and import it
/// silently. Solves the repeating "TextMesh Pro Essential Resources are missing"
/// error when the normal menu-based importer won't cooperate.
/// </summary>
[InitializeOnLoad]
public static class AutoImportTMP
{
    static AutoImportTMP()
    {
        // Delay by one frame so AssetDatabase is ready.
        EditorApplication.delayCall += TryImport;
    }

    static void TryImport()
    {
        if (AssetDatabase.IsValidFolder("Assets/TextMesh Pro"))
            return;

        string packagePath = FindEssentialsPackage();
        if (packagePath == null)
        {
            Debug.LogWarning("AutoImportTMP: could not find TMP Essential Resources.unitypackage in Library/PackageCache.");
            return;
        }

        Debug.Log("AutoImportTMP: importing " + packagePath);
        AssetDatabase.ImportPackage(packagePath, interactive: false);
    }

    static string FindEssentialsPackage()
    {
        string cacheRoot = Path.Combine(Directory.GetCurrentDirectory(), "Library", "PackageCache");
        if (!Directory.Exists(cacheRoot)) return null;

        foreach (string dir in Directory.GetDirectories(cacheRoot))
        {
            string candidate = Path.Combine(dir, "Package Resources", "TMP Essential Resources.unitypackage");
            if (File.Exists(candidate)) return candidate;
        }
        return null;
    }
}
#endif
