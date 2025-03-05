using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoadAttribute]
public static class OnHeirarchyUpdated
{
    private static TapToPlaceManager tapToPlaceScript;
    private static List<GameObject> rootGameObjects = new List<GameObject>();
    static OnHeirarchyUpdated()
    {
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        SceneView.duringSceneGui += OnSceneGUI;

        GetTapToPlace();
    }

    static void GetTapToPlace()
    {
        tapToPlaceScript = null;
        var script = Resources.FindObjectsOfTypeAll(typeof(TapToPlaceManager));
        if (script != null || script.Length == 0)
        {
            if (script.Length > 1)
            {
                Debug.Log($"script {script.Length}-- {EditorUtility.InstanceIDToObject(script[0].GetInstanceID()).name}");
                EditorUtility.DisplayDialog("Error", "Deleting Multiple instance of TapToPlaceManager", "Understood");
            }
            if (script.Length != 0)
            {
                tapToPlaceScript = script[0] as TapToPlaceManager;
            }
        }
    }
    static void OnHierarchyChanged()
    {
        var activeScene = SceneManager.GetActiveScene();
        //UpdateTapToPlaceScript(activeScene);
    }
    private static void OnSceneGUI(SceneView scene)
    {
        OnHierarchyChanged();
    }

    private static void UpdateTapToPlaceScript(Scene scene)
    {
        if (BuildPipeline.isBuildingPlayer || EditorApplication.isCompiling || EditorApplication.isPlaying) return;

        var rootObjects = scene.GetRootGameObjects().ToList();

        if (tapToPlaceScript == null || rootGameObjects == null || rootGameObjects.Count == 0)
        {
            GetTapToPlace();
            rootGameObjects.Clear();
            rootGameObjects = rootObjects.ToList();
            tapToPlaceScript.GetTransformOffset();
        }



        if (rootGameObjects.Count != rootObjects.Count || rootGameObjects.Count != tapToPlaceScript.GetElementCount())
        {
            tapToPlaceScript.GetTransformOffset();
        }
        else
        {
            if (tapToPlaceScript != null)
            {
                int count = 0;
                for (int i = tapToPlaceScript.siblingIndex + 2; i < rootGameObjects.Count; i++)
                {
                    if (rootGameObjects[i] == null)
                    {
                        rootGameObjects.Clear();
                        return;
                    }
                    var offsetPos = tapToPlaceScript.CalculatePositionOffset(rootGameObjects[i].transform);
                    var offsetAngle = tapToPlaceScript.CalculateAngleOffset(rootGameObjects[i].transform);
                    if (offsetPos != tapToPlaceScript.GetObjectPosition(count) || offsetAngle != tapToPlaceScript.GetObjectAngle(count))
                    {
                        tapToPlaceScript.GetTransformOffset();
                        break;
                    }
                    count++;
                }
            }
        }
    }
}

