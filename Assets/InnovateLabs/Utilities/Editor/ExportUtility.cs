using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using InnovateLabs;
using System;

using InnovateLabs.ModelData;

//using UnityEngine.PixyzPlugin4Unity.Components;
//using UnityEgnine.PixyzCommons.Components;

namespace InnovateLabs.Utilities
{
    public class ExportUtility : Editor
    {
        static string pixyzPrefabPath = "Assets/3DModels";       
        //Set savepath to user's preference path
        static string defaultSavePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        static string defaultFileName = "pixyz-prefab";
        static string saveWindowTitle = "Save Pixyz Prefab";
        static string saveFileFormat = "unitypackage";
        public static string dateFormat = "dd-MM-yyyy HH-mm-ss";

        [MenuItem("GameObject/Innovate Labs/Export/ Pixyz Prefab", priority = -9)]
        public static void ExportPixyzPrefab()
        {
            GameObject go = Selection.activeObject as GameObject;
            if (go == null)
                return;

            var isPrefab = PrefabUtility.IsAnyPrefabInstanceRoot(go);

            if (!isPrefab)
            {
                EditorUtility.DisplayDialog("Export failed - 1", "Selected model is not supported", "Ok");
                return;
            }

            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

            var checkPrefabPath = prefabPath.Split('/');

            if(checkPrefabPath.Length <= 2)
            {
                EditorUtility.DisplayDialog("Export failed", "Selected Model wasn't found at source location", "Ok");
                return;
            }
            else
            {
                if(!($"{checkPrefabPath[0]}/{checkPrefabPath[1]}" == pixyzPrefabPath))
                {
                    EditorUtility.DisplayDialog("Export failed", "Selected Model wasn't found at source location", "Ok");
                    return;
                }
            }

            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);


            var importStamp = prefab.GetComponent<ImportStamp>();           //Import stamp need to be changed Pixyz's script
            var metaData = prefab.GetComponentsInChildren<Metadata>();      //MetaData need to be changed Pixyz's script
            var modelInfo = prefab.GetComponent<ModelInfo>();

            if (importStamp == null && metaData == null && modelInfo == null)
            {
                EditorUtility.DisplayDialog("Export failed - 2", "Selected model is not supported", "Ok");
                return;
            }

            if (importStamp != null)
            {
                DestroyImmediate(importStamp);
                Undo.RecordObject(prefab, $"Removed import stamp from {go.name}");
            }

            foreach (var data in metaData)
            {
                var metaObject = data.gameObject;
                DestroyImmediate(data);
                Undo.RecordObject(metaObject, $"Removed metadata from {metaObject.name}");
            }

            if (modelInfo == null)
            {
                modelInfo = prefab.AddComponent<ModelInfo>();
            }

            modelInfo.lastExported = DateTime.Now.ToString(dateFormat);

            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);

            var fileName = EditorUtility.SaveFilePanel(saveWindowTitle, defaultSavePath, defaultFileName, saveFileFormat);

            if (fileName.Length != 0)
            {
                AssetDatabase.ExportPackage(prefabPath, fileName, ExportPackageOptions.Recurse);
            }
        }

    }

}