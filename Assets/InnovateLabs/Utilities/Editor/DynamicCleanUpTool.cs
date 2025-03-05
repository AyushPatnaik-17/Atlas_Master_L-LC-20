using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;



using Debug = UnityEngine.Debug;

namespace InnovateLabs.Projects
{
    public class DynamicCleanUpTool : EditorWindow
    {
        private static string s_fromPath;
        private static string s_templateName;

        private BuildTarget _activeBuildTarget;

        private TextField _currentVersion;

        private bool shouldRunConsole;
        /// <summary>
        /// Change this value to create template according to template type
        /// 1.HoloLens
        /// 2.Android
        /// 3.PC-VR
        /// </summary>
        private static int i_currentTemplateIndex = 1;

        //[MenuItem("Innovate Labs/ Utilities/ Start Clean Up", priority = 9998)]
        public static void CleanUpWindow()
        {
            DynamicCleanUpTool window = GetWindow<DynamicCleanUpTool>("Project Clean-up");
            window.Show();
        }

        public void OnEnable()
        {
            _activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            s_fromPath = Directory.GetCurrentDirectory();
        }

        private void CreateGUI()
        {
            //Debug.Log("CreateGUI from cleanuptool");
            var root = rootVisualElement;
            Label header = new Label("header");
            header.text = "project clean up";

            _currentVersion = new TextField("Current Version");
            _currentVersion.value = ReturnTemplateInfo().version;
            _currentVersion.SetEnabled(false);
            _currentVersion.RegisterValueChangedCallback(evt =>
            {
                UpdateJSONPackageVersion(evt.newValue);
            });

            Toggle editVersion = new Toggle("Edit Version");
            editVersion.RegisterValueChangedCallback(evt =>
            {
                _currentVersion.SetEnabled(evt.newValue);
            });

            Toggle runConsole = new Toggle("Run Console App");
            runConsole.RegisterValueChangedCallback(evt =>
            {
                shouldRunConsole = evt.newValue;
            });

            Button cleanAll = new Button(CleanAll);
            cleanAll.name = "cleanAll";
            cleanAll.text = "Clean All";
            cleanAll.style.fontSize = 22f;

            
            root.Add(_currentVersion);
            root.Add(editVersion);
            root.Add(runConsole);
            root.Add(cleanAll);
        }

        private JsonIdentifier ReturnTemplateInfo()
        {
            string identifierJsonPath = CleanUpToolPaths.jsonIdentifierPath;

            if (!File.Exists(identifierJsonPath))
            {
                return new JsonIdentifier()
                {
                    displayName = "Identifier not found",
                    version = null
                };
            }

            string identifierJson = File.ReadAllText(identifierJsonPath);
            JsonIdentifier identifier = JsonUtility.FromJson<JsonIdentifier>(identifierJson);

            return new JsonIdentifier()
            {
                displayName = identifier.displayName,
                version = identifier.version
            };
        }

        private void UpdateJSONPackageVersion(string newVersion)
        {

            string packageJson = CleanUpToolPaths.jsonIdentifierPath;
            if (!File.Exists(packageJson)) return;

            Debug.LogError($"Json File Doesnt exist");

            var jsonString = File.ReadAllText(packageJson, Encoding.UTF8);
            var jsonData = JsonUtility.FromJson<JsonIdentifier>(jsonString);
            var versions = newVersion.Split('.');
            Debug.Log($"Version : {versions}");
            Debug.Log($"Json data Version : {jsonData.version}");
            Debug.Log($"verions  : {versions[0]},{versions[1]},{versions[2]}");
            int major = string.IsNullOrEmpty(versions[0]) ? 0 : int.Parse(versions[0]);
            int minor = string.IsNullOrEmpty(versions[1]) ? 0 : int.Parse(versions[1]);
            int build = string.IsNullOrEmpty(versions[2]) ? 0 : int.Parse(versions[2]);

            string updatedVersion = major.ToString() + "." + minor.ToString() + "." + build.ToString();

            jsonData.version = updatedVersion;

            var newJson = JsonUtility.ToJson(jsonData, true);

            Debug.Log($"new json {newJson}");
            File.Delete(packageJson);

            using StreamWriter writer = new StreamWriter(packageJson);
            writer.Write(newJson);
        }

        #region Cleanup HoloLens Function
        private void CleanAll()
        {
            DecideActiveBuildTarget();

            #region methods called based on build target
            /*#if UNITY_WSA
                        CleanBuildNumber();
                        ClearProjectTitle();
                        ClearErgoData();
                        CleanHierarchy();
                        ClearModels();
                        //ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();
            #else
                        ClearProjectTitle();
                        CleanHierarchy();
                        ClearModels();
                        //ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();  
            #endif*/
            #endregion

            if (shouldRunConsole)
            {
                switch (ReturnTemplateInfo().displayName)
                {
                    case "Dx_HoloLens":
                        CleanBuildNumber();
                        ClearProjectTitle();
                        ClearErgoData();
                        CleanHierarchy();
                        ClearModels();
                        ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();
                        Debug.Log("Running clean all from hololens");
                        break;

                    default:
                        ClearProjectTitle();
                        CleanHierarchy();
                        ClearModels();
                        ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();
                        Debug.Log("Running clean all from Android orn VR");
                        break;

                }
                LaunchConsoleApp(s_fromPath, s_templateName);
            }
            else
            {
                switch (ReturnTemplateInfo().displayName)
                {
                    case "Dx_HoloLens":
                        CleanBuildNumber();
                        ClearProjectTitle();
                        ClearErgoData();
                        CleanHierarchy();
                        ClearModels();
                        ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();
                        Debug.Log("Running clean all from hololens");
                        break;

                    default:
                        ClearProjectTitle();
                        CleanHierarchy();
                        ClearModels();
                        ClearAssetFolder();
                        ClearProjectSetup();
                        LoadDxLayout();
                        Debug.Log("Running clean all from Android orn VR");
                        break;

                }
            }
        }

        private void DecideActiveBuildTarget()
        {
            #region switch case based on build taret
            /*switch (_activeBuildTarget)
            {
                case BuildTarget.Android:
                    _templateName = CleanUpToolPaths.templateName[0];
                    _currentTemplateIndex = 1;
                    break;
                case BuildTarget.WSAPlayer:
                    _templateName = CleanUpToolPaths.templateName[1];
                    _currentTemplateIndex = 0;
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    _templateName = CleanUpToolPaths.templateName[2];
                    _currentTemplateIndex = 2;
                    break;
                default:
                    Debug.LogError("Appropriate Build Target not found");
                    break;
            }
*/
            #endregion
            switch (ReturnTemplateInfo().displayName)
            {
                case "Dx_HoloLens":
                    s_templateName = CleanUpToolPaths.templateName[1];
                    i_currentTemplateIndex = 0;
                    break;
                case "Dx_Android":
                    s_templateName = CleanUpToolPaths.templateName[0];
                    i_currentTemplateIndex = 1;
                    break;
                case "Dx_SteamVR":
                    s_templateName = CleanUpToolPaths.templateName[2];
                    i_currentTemplateIndex = 2;
                    break;
                default:
                    Debug.LogError("Appropriate Build Target not found");
                    break;
            }

            Debug.Log(i_currentTemplateIndex);
            Debug.Log(s_templateName);
        }

        public void CleanBuildNumber()
        {
            PlayerSettings.WSA.packageVersion = new Version(1, 0, 0);
            Debug.Log($"Cleaning build number : ....{PlayerSettings.WSA.packageVersion.Major}.{PlayerSettings.WSA.packageVersion.Minor}.{PlayerSettings.WSA.packageVersion.Build}");
        }

        public void ClearErgoData()
        {
            var ergoDataDir = CleanUpToolPaths.ergonomicDataPath;
            if (!Directory.Exists(ergoDataDir))
                return;
            DirectoryInfo ergoDir = new DirectoryInfo(ergoDataDir);
            ergoDir.Delete(true);
            AssetDatabase.Refresh();
            Debug.Log("clearing Ergo Data");
        }

        #endregion Cleanup HoloLens Function
        public void ClearProjectTitle()
        {
            PlayerSettings.productName = "Sample Dx Project";
            PlayerSettings.companyName = "Company Name";
            Debug.Log($"product name : {PlayerSettings.productName } -- {PlayerSettings.companyName}");
            Debug.Log("Clearing Project Title");
        }

        private void CleanHierarchy()
        {

            #region code depends on build target proprocessor directives
#if UNITY_ANDROID
            var ttpm = FindObjectOfType<TapToPlaceManager>();
            var objs = ttpm.GetObjects();
            foreach (var obj in objs)
            {
                DestroyImmediate(obj.gameObject);
            }
            EditorUtility.SetDirty(ttpm.gameObject);
#elif UNITY_WSA
            var ttpm = FindObjectOfType<TapToPlaceManager>();
            var objs = ttpm.GetObjects();
            foreach (var obj in objs)
            {
                DestroyImmediate(obj.gameObject);
            }
            EditorUtility.SetDirty(ttpm.gameObject);
#elif UNITY_STANDALONE
            var currentScene = SceneManager.GetActiveScene();
            var rootGameObjects = currentScene.GetRootGameObjects();
            var heirarchyMarker = GameObject.Find("-------------------------");
            foreach (GameObject root in rootGameObjects)
            {
                if (root.transform.GetSiblingIndex() > heirarchyMarker.transform.GetSiblingIndex())
                {
                    DestroyImmediate(root.gameObject);
                }
            }
            EditorUtility.SetDirty(heirarchyMarker);
#endif
            #endregion

            #region Switching between the build targets depending on the identifier
            /*switch (ReturnTemplateName())
            {
                case "Dx_Android":
                case "Dx_HoloLens":
                    var ttpm = FindObjectOfType<TapToPlaceManager>();
                    var objs = ttpm.GetObjects();
                    foreach (var obj in objs)
                    {
                        DestroyImmediate(obj.gameObject);
                    }
                    break;

                case "Dx_SteamVR":
                    var currentScene = SceneManager.GetActiveScene();
                    var rootGameObjects = currentScene.GetRootGameObjects();
                    var heirarchyMarker = GameObject.Find("-------------------------");
                    foreach (GameObject root in rootGameObjects)
                    {
                        if (root.transform.GetSiblingIndex() > heirarchyMarker.transform.GetSiblingIndex())
                        {
                            DestroyImmediate(root.gameObject);
                        }
                    }
                    break;
                default:
                    Debug.LogError("Appropriate Build Target not found");
                    break;
            }*/

            #endregion

            Debug.Log("Cleaning the heirarchy");
        }
        private void ClearModels()
        {
            var modelPath = CleanUpToolPaths.modelsPath;
            if (!Directory.Exists(modelPath)) return;

            DirectoryInfo modeldir = new DirectoryInfo(modelPath);
            modeldir.Delete(true);
            AssetDatabase.Refresh();
            Debug.Log("Clearing Models");
        }

        #region code for clearing Assets Folder

        private void ClearAssetFolder()
        {
            var assetPath = Application.dataPath + "/";
            if (!Directory.Exists(assetPath)) return;

            DirectoryInfo assetDir = new DirectoryInfo(assetPath);

            var files = assetDir.GetFiles();

            foreach (var file in files)
            {
                var checkMeta = file.Name.Split('.');
                if (checkMeta[checkMeta.Length - 1] != "meta")
                {
                    File.Delete(file.FullName);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Clearing Asset Folder");

        }

        #endregion
        private void ClearProjectSetup()
        {
            var projectData = CleanUpToolPaths.projectDataPath;
            if (!File.Exists(projectData)) return;

            File.Delete(projectData);
            AssetDatabase.Refresh();
            Debug.Log("Clearing Project Setup");
        }
        

        private void LoadDxLayout()
        {
            Debug.Log($"LoadingLayout : {CleanUpToolPaths.layoutName[i_currentTemplateIndex]}");
            var defaultLayoutPath = Directory.GetCurrentDirectory() + "\\UserSettings" + "\\Layouts\\";
            var layoutFile = CleanUpToolPaths.layoutName[i_currentTemplateIndex];

            Debug.Log($"Loading layout {File.Exists(defaultLayoutPath + layoutFile)} : {defaultLayoutPath + layoutFile}");
            if (!File.Exists(defaultLayoutPath + layoutFile))
            {
                Debug.LogError("LayoutFile doesn't exist");
                return;
            }
            LayoutUtility.LoadLayoutFromPreference(defaultLayoutPath, layoutFile);
        }

        public void LaunchConsoleApp(string fromLocation, string templateDataDestination)
        {
            Process currentUnityInstance = Process.GetCurrentProcess();
            int currentUnityInstanceID = currentUnityInstance.Id;

            Process.Start(CleanUpToolPaths.consoleAppPath, $"{fromLocation} {templateDataDestination} {currentUnityInstanceID}");

            closeUnity();
        }

        private async void closeUnity()
        {
            GetWindow<DynamicCleanUpTool>("Project Clean-up", true, typeof(DynamicCleanUpTool)).Close();
            await Task.Delay(500);
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            await Task.Delay(500);
            EditorApplication.Exit(0);
        }
        
    }
}

public static class CleanUpToolPaths
{
    public static string[] templateName = { @"Android", @"HoloLens", @"SteamVR" };
    public static string consoleAppPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\DxDev\ConsoleApp\bin\Debug\ConsoleApp.exe";
    public static string jsonIdentifierPath = Application.dataPath + "/InnovateLabs/Identifier.json";
    public static string ergonomicDataPath = Application.dataPath + "/InnovateLabs/ProjectSetup/Data/ErgonomicInteractionData/";
    public static string modelsPath =  Application.dataPath + "/3DModels/";
    public static string projectDataPath = Application.dataPath + "/InnovateLabs/ProjectSetup/Data/ProjectData.asset";

    public static string[] layoutName =
    {
        "Dx-HoloLens.wlt",
        "Dx-Android.wlt",
        "Dx-VR.wlt",
    };
}

public class JsonIdentifier
{
    public string name;
    public string displayName;
    public string version;
    public string type;
    public string unity;
    public string description;
    public object dependencies;
}
