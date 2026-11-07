//==========================================( Neverway 2025 )=========================================================//
// Author
//  Liz M.
//
// Contributors
//
//
//====================================================================================================================//

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Strips the word "Variant" from the end of selected prefabs in the project window
/// </summary>
public class ToolStripVariant
{
    [MenuItem("Neverway/Tools/Strip 'Variant' from selected prefabs")]
    private static void StripVariantFromNames()
    {
        Object[] selectedAssets = Selection.GetFiltered(typeof(GameObject), SelectionMode.Assets);

        foreach (Object asset in selectedAssets)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            string directory = Path.GetDirectoryName(assetPath);

            if (fileName.EndsWith("Variant"))
            {
                string newFileName = fileName.Substring(0, fileName.Length - "Variant".Length).TrimEnd();
                string newPath = Path.Combine(directory, newFileName + ".prefab");

                AssetDatabase.RenameAsset(assetPath, newFileName);
                Debug.Log($"Renamed '{fileName}' to '{newFileName}'");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
