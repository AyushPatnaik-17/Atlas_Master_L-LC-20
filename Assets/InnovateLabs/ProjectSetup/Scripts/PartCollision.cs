using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using InnovateLabs.Projects;

using GUID = InnovateLabs.Projects.GUID;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public static class PartCollision 
{
    static string managerPrefabPath = "Assets/InnovateLabs/ProjectSetup/Prefabs/CollisionChecker.prefab";

#if UNITY_EDITOR

    //[MenuItem("Innovate Labs/Part Collision Check")]
    public static string InstantiateManager(int stepNo)
    {

        var currentScene = EditorSceneManager.GetActiveScene();

        //var currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "SampleScene" && !currentScene.GetRootGameObjects().ToList().Exists(x => x.name == "MRTK XR Rig")) return null;

        GameObject partCollisionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPrefabPath);

        var pcInstance = GameObject.Instantiate(partCollisionPrefab, Vector3.zero, Quaternion.identity);
        pcInstance.name = $"{partCollisionPrefab.name}_{stepNo}";

        var pcGUID = pcInstance.AddComponent<GUID>();

        return pcGUID.GetGUID();
    }
    public static void InstantiateManager(StepData currentStep)
    {

        var currentScene = EditorSceneManager.GetActiveScene();

        //var currentScene = SceneManager.GetActiveScene();

        if (currentScene.name != "SampleScene" && !currentScene.GetRootGameObjects().ToList().Exists(x => x.name == "MRTK XR Rig")) return;

        GameObject partCollisionPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(managerPrefabPath);

        var pcInstance = GameObject.Instantiate(partCollisionPrefab, Vector3.zero, Quaternion.identity);
        pcInstance.name = partCollisionPrefab.name;

        var pcGUID = pcInstance.AddComponent<GUID>();

        currentStep.collisionManager = pcGUID.GetGUID();
    }
#endif
    public static async Task RemoveParts(GameObject collisionManager)
    {
        var partCollisionChecker = collisionManager.GetComponentInChildren<PartsCollisionChecker>(true);
        var partPosition = collisionManager.GetComponentInChildren<PartPosition>(true);

        if (partCollisionChecker != null && partPosition != null)
        {
            while(partCollisionChecker.transform.childCount > 0)
            {
                var part = partCollisionChecker.transform.GetChild(0);
                await RemoveConvexMeshes(part);
                part.parent = null;
                part.SetAsLastSibling();
            }
        }
        await Task.Yield();
    }

    public static async Task RemoveConvexMeshes(Transform part)
    {
        var convexMeshes = part.GetComponentsInChildren<PartCollisionListener>();
        foreach(var convexMesh in convexMeshes)
        {
            Object.DestroyImmediate(convexMesh.gameObject);
            
        }
        await Task.Yield();
    }
#if UNITY_EDITOR

    //[MenuItem("Innovate Labs/Part Collision Destroy All")]
    public static void DestroyAllManager()
    {
        var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

        //var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

        foreach (var root in rootObjects)
        {
            var partCollisionCheckers = root.GetComponentsInChildren<PartsCollisionChecker>(true);
            if (partCollisionCheckers != null)
            {
                foreach (var partCollisionChecker in partCollisionCheckers)
                {
                    var collisionManager = partCollisionChecker.transform.parent.parent;
                    var partPosition = collisionManager.GetComponentInChildren<PartPosition>(true);

                    if (partCollisionChecker != null && partPosition != null)
                    {
                        foreach (Transform part in partCollisionChecker.transform.parent)
                        {
                            if (part != partCollisionChecker.transform)
                            {
                                part.parent = null;
                                part.SetAsLastSibling();
                            }
                        }

                        Object.DestroyImmediate(collisionManager.gameObject);
                    }
                }
            }
        }
    }
#endif
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

        if (manager.GetComponentInChildren<PartsCollisionChecker>(true) != null
            && manager.GetComponentInChildren<PartPosition>(true) != null)
        {
            return manager;
        }

        return null;
    }
    public static List<GameObject> GetCollisionParts(string managerGUID)
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

        if (manager.GetComponentInChildren<PartsCollisionChecker>(true) != null
            && manager.GetComponentInChildren<PartPosition>(true) != null)
        {
            var collisionParts = new List<GameObject>();
            foreach (Transform part in manager.GetComponentInChildren<PartsCollisionChecker>(true).transform)
            {
                if (part.GetComponent<PartsCollisionChecker>() == null)
                {
                    collisionParts.Add(part.gameObject);
                }
            }
            return collisionParts;
        }
        return null;
    }

    public static PartsCollisionChecker GetCollisionChecker(string managerGUID)
    {
        return FindManagerWithGUID(managerGUID).GetComponentInChildren<PartsCollisionChecker>(true);
    }
    public static Transform GetSnapTransform(string managerGUID)
    {
        return FindManagerWithGUID(managerGUID).GetComponentInChildren<PartPosition>(true).transform;
    }


    public static void FitCollisionVolume(string managerGuid)
    {
        var partCollisionChecker = GetCollisionChecker(managerGuid);
        partCollisionChecker.FitCollisionVolume();
    }
    public static void FitSnapVolume(string managerGuid)
    {
        var partCollisionChecker = GetCollisionChecker(managerGuid);
        partCollisionChecker.FitSnapVolume();
    }

    public static async Task RemoveStepCollision(StepData stepData)
    {
        var manager = FindManagerWithGUID(stepData.collisionManager);
        await RemoveParts(manager);
        Object.DestroyImmediate(manager);
    }

    public static void ToggleVisaulization(StepData stepData, bool value)
    {
        var checker = GetCollisionChecker(stepData.collisionManager);
        checker.ToggleVisibility(value);
    }

    private static List<GameObject> GetAllManagersInScene()
    {
        var collisionManagers = new List<GameObject>();
#if UNITY_EDITOR
        var rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
#else
        var rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
#endif

        foreach (var root in rootObjects)
        {
            var partCollisionCheckers = root.GetComponentsInChildren<PartsCollisionChecker>(true);
            if (partCollisionCheckers != null)
            {
                foreach (var partCollisionChecker in partCollisionCheckers)
                {
                    var collisionManager = partCollisionChecker.transform.parent;
                    var partPosition = collisionManager.GetComponentInChildren<PartPosition>(true);

                    if (partCollisionChecker != null && partPosition != null)
                    {
                        collisionManagers.Add(collisionManager.gameObject);
                    }
                }
            }
        }
        return collisionManagers;
    }

    public static void ToggleOffVisaulization()
    {
        var collisionManagers = GetAllManagersInScene();
        foreach (var manager in collisionManagers)
        {
            var checker = manager.GetComponentInChildren<PartsCollisionChecker>(true);
            if (checker != null)
            {
                checker.ToggleVisibility(false);
            }
        }
    }
}
