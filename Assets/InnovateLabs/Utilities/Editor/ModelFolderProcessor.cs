using UnityEditor;
using UnityEngine;

public class ModelFolderProcessor : AssetPostprocessor
{
    private static readonly string folderToMonitor = "Assets/3DModels";
    public static event System.Action AssetsChanged;

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
    {
        bool assetsChanged = false;

        foreach (string asset in importedAssets)
        {
            if (asset.StartsWith(folderToMonitor))
            {
                assetsChanged = true;
            }
        }

        foreach (string asset in deletedAssets)
        {
            if (asset.StartsWith(folderToMonitor))
            {
                assetsChanged = true;
            }
        }
        
        if (assetsChanged)
        {
            AssetsChanged?.Invoke();
        }
    }
}
