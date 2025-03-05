using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using System.Linq;
using GUID = InnovateLabs.Projects.GUID;
using System;
using InnovateLabs.Projects;
using System.Threading.Tasks;


public static partial class RotationUtil
{
    static string managerPrefabPath = "Assets/InnovateLabs/ProjectSetup/Prefabs/RotationManager.prefab";

#if UNITY_EDITOR
    public static string InstantiateManager(int stepNo)
    {
        //Debug.Log("Rotation Manager Instantiated");
        var currentScene = EditorSceneManager.GetActiveScene();

        //var currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "SampleScene" && !currentScene.GetRootGameObjects().ToList().Exists(x => x.name == "MRTK XR Rig")) return null;

        GameObject rotationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPrefabPath);
        var pcInstance = GameObject.Instantiate(rotationPrefab, Vector3.zero, Quaternion.identity);
        if (pcInstance.GetComponent<RotationInteraction>() == null)
            pcInstance.AddComponent<RotationInteraction>();
        pcInstance.name = $"{rotationPrefab.name}_{stepNo}";

        var pcGUID = pcInstance.AddComponent<GUID>();

        return pcGUID.GetGUID();

    }

    public static async Task RemoveManager(string managerGUID)
    {
        var manager = FindManagerWithGUID(managerGUID);
        //Debug.Log($"Removing this Rotation Manager : {manager}");
        UnityEngine.Object.DestroyImmediate(manager);
        await Task.Yield();
    }
#endif

    public static void ToggleFbPointers(bool setActive, string managerGUID)
    {
        var manager = FindManagerWithGUID(managerGUID);
        var feedbackPointer = manager.GetComponentInChildren<HighlightSphere>(true);
        feedbackPointer.gameObject.SetActive(setActive);
    }

    public static void ToggleOffAllFbPointers()
    {
        var rotationManagers = new List<GameObject>();
#if UNITY_EDITOR
        var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            if (rootObjects[i].HasComponent<RotationInteraction>())
            {
                rotationManagers.Add(rootObjects[i]);
            }
        }
#endif

        for (int i = 0; i < rotationManagers.Count; i++)
        {
            rotationManagers[i].GetComponentInChildren<HighlightSphere>(true).
                                                    gameObject.SetActive(false);
        }

    }


    public static GameObject FindManagerWithGUID(string guid)
    {
#if (UNITY_EDITOR)
        var currentScene = EditorSceneManager.GetActiveScene();
#else
        var currentScene = SceneManager.GetActiveScene();
#endif
        var manager = currentScene.GetRootGameObjects()
            .ToList().
            Find(x =>
            x.GetComponent<GUID>() != null
            && x.GetComponent<GUID>().GetGUID() == guid);

        if (manager.GetComponent<RotationInteraction>() != null)
        {
            return manager;
        }

        return null;
    }

    #region Methods Related to Rotation Part
    public static void AddRotationPart(GameObject part, string guid)
    {
        //Debug.Log("Add Rotation Parts");
        var currentManager = FindManagerWithGUID(guid);
        currentManager.GetComponent<RotationInteraction>().rotationParts.Add(part);

        #region CHUP REH
        //Debug.Log($"current manager : {currentManager.name}");
        /*foreach (var rotPart in currentManager.GetComponent<RotationInteraction>().rotationParts)
        {
            Debug.Log($"Part in this list called from Add Rotation Parts: {rotPart}");
        }*/

        //Debug.Log(currentManager.GetComponent<RotationInteraction>().rotationParts.Count);
        #endregion
    }

    public static void RemoveRotationPart(GameObject part, string guid)
    {
        var currentManager = FindManagerWithGUID(guid);
        currentManager.GetComponent<RotationInteraction>().rotationParts.Remove(part);
    }

    public static bool ContainsRotationPart(GameObject part, string guid)
    {

        var currentManager = FindManagerWithGUID(guid);
        return currentManager.GetComponent<RotationInteraction>().rotationParts.Contains(part);
    }

    static List<GameObject> listGo = new();
    public static List<GameObject> GetRotationParts(string managerGUID)
    {
#if (UNITY_EDITOR)
        var currentScene = EditorSceneManager.GetActiveScene();
#else
        var currentScene = SceneManager.GetActiveScene();
#endif
        var manager = currentScene.GetRootGameObjects()
            .ToList().
            Find(x =>
            x.GetComponent<GUID>() != null
            && x.GetComponent<GUID>().GetGUID() == managerGUID);

        if (manager.GetComponent<RotationInteraction>() != null)
        {
            //return manager.GetComponent<RotationInteraction>().GetRotationParts();
            //Debug.Log("parts: "+manager.GetComponent<RotationInteraction>().rotationParts.Count);
            listGo = manager.GetComponent<RotationInteraction>().rotationParts;
            return listGo;
        }
        return null;
    }

    #endregion

    #region Methods Related to grab Parts

    public static bool ContainsGrabPart(GameObject part, string guid)
    {

        var currentManager = FindManagerWithGUID(guid);
        return currentManager.GetComponent<RotationInteraction>().grabParts.Contains(part);
    }

    public static void AddGrabPart(int maxParts, GameObject part, string guid)
    {
        var currentManager = FindManagerWithGUID(guid);
#if UNITY_EDITOR
        if (currentManager.GetComponent<RotationInteraction>().grabParts.Count >= maxParts)
        {
            EditorUtility.DisplayDialog("Grab Part Defined!", "A Grab Part has already been defined for this step. " +
                                                               "Remove the current part to add a new one", "OK");
            return;
        }
#endif

        currentManager.GetComponent<RotationInteraction>().grabParts.Add(part);
    }

    public static void RemoveGrabPart(GameObject part, string guid)
    {
        var currentManager = FindManagerWithGUID(guid);
        currentManager.GetComponent<RotationInteraction>().grabParts.Remove(part);

    }

    static List<GameObject> listGp = new();
    public static List<GameObject> GetGrabParts(string managerGUID)
    {
#if (UNITY_EDITOR)
        var currentScene = EditorSceneManager.GetActiveScene();
#else
        var currentScene = SceneManager.GetActiveScene();
#endif
        var manager = currentScene.GetRootGameObjects()
            .ToList().
            Find(x =>
            x.GetComponent<GUID>() != null
            && x.GetComponent<GUID>().GetGUID() == managerGUID);

        if (manager.GetComponent<RotationInteraction>() != null)
        {
            //return manager.GetComponent<RotationInteraction>().GetRotationParts();
            //Debug.Log("grab parts: " + manager.GetComponent<RotationInteraction>().grabParts.Count);
            listGp = manager.GetComponent<RotationInteraction>().grabParts;
            return listGp;
        }
        return null;
    }
    #endregion

}