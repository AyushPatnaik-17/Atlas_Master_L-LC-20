using UnityEditor;
using UnityEngine;
using UnityEngine.Windows;
using UnityEditor.Build;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using Microsoft.MixedReality.Toolkit.Build.Editor;

namespace InnovateLabs.Projects
{
    public class ProjectSetup_Hololens : EditorWindow
    {
        [MenuItem("Innovate Labs/Project Setup/ HoloLens %#Q", false, 11)]
        public static void ProjectSetupHololensWindow()
        {
            ProjectSetup_Hololens wnd = GetWindow<ProjectSetup_Hololens>();
            wnd.titleContent = new GUIContent("Project Setup - HoloLens");
        }


        private TextField applicationName;
        private ObjectField appIcon;
        private TextField organisationName;
        private TextField packageName;
        private TextField packageDescription;
        private PathField buildFolderLocation;


        private DropdownField deviceType;
        private DropdownField architecture;

        private Button completeSetup;
        private Button saveSetup;
        private Button editSetup;
        private Button selectMarker;

        private ProjectData_Hololens projectData_Hololens;

        public delegate void D_ProjectSetup();
        public static event D_ProjectSetup e_projectSetup;

        private static readonly List<string> devices = new List<string>()
        {
            "Any device",
            "PC",
            "Mobile",
            "HoloLens"
        };

        private static readonly List<string> architectures = new List<string>()
        {
            "x86",
            "x64",
            "ARM",
            #if UNITY_2019_1_OR_NEWER
            "ARM64"
            #endif
        };

        #region Unity Methods
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ProjectSetup_Hololens.uxml");
            VisualElement UIFromXML = visualTree.Instantiate();
            root.Add(UIFromXML);

            InitElements(root);

            if (File.Exists("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset"))
            {
                projectData_Hololens = (ProjectData_Hololens)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset", typeof(ProjectData_Hololens));
                LoadData(projectData_Hololens);
                ToggleEdit(false);

                completeSetup.style.display = DisplayStyle.None;
                editSetup.style.display = DisplayStyle.Flex;
                selectMarker.style.display = DisplayStyle.Flex;
                saveSetup.style.display = DisplayStyle.None;
            }

        }
            
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            if(EditorGUI.EndChangeCheck())
            {
                ProjectSetup_Hololens wnd = GetWindow<ProjectSetup_Hololens>();
                EditorSceneManager.MarkAllScenesDirty();  
            }
        }
        #endregion

        private void InitElements(VisualElement root)
        {

            applicationName = root.Q<TextField>("ApplicationName");
            appIcon = root.Q<ObjectField>("ApplicationIcon");
            organisationName = root.Q<TextField>("OrganisationName");
            packageName = root.Q<TextField>("PackageName");
            packageDescription = root.Q<TextField>("PackageDescription");

            buildFolderLocation = new PathField(root.Q<VisualElement>("BuildFolderLocation"));
            buildFolderLocation.ImportSettings("Build Path", "", "");


            deviceType = root.Q<DropdownField>("DeviceType");
            deviceType.choices = devices;
            deviceType.value = devices[0];

            architecture = root.Q<DropdownField>("Architecture");
            architecture.choices = architectures;
            architecture.value = architectures[0];

            completeSetup = root.Q<Button>("SetupProject");
            completeSetup.clicked += OnSetupComplete;

            saveSetup = root.Q<Button>("SaveSetup");
            saveSetup.clicked += OnSaveSetup;
            saveSetup.style.display = DisplayStyle.None;

            editSetup = root.Q<Button>("EditSetup");
            editSetup.clicked += OnEditSetup;
            editSetup.style.display = DisplayStyle.None;

            selectMarker = root.Q<Button>("SelectMarker");
            selectMarker.clicked += OnSelectMarker;
            selectMarker.style.display = DisplayStyle.None;
        }

        private void OnSetupComplete()
        {
            projectData_Hololens = ScriptableObject.CreateInstance<ProjectData_Hololens>();
            string projectDataPath = "Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset";

            if(!SetData()) return;

            AssetDatabase.CreateAsset(projectData_Hololens, projectDataPath);
            AssetDatabase.SaveAssets();

            ToggleEdit(false);

            completeSetup.style.display = DisplayStyle.None;
            editSetup.style.display = DisplayStyle.Flex;
            selectMarker.style.display = DisplayStyle.Flex;

            
            var sceneview = EditorWindow.GetWindow<SceneView>();
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);

            EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene();

            var activeScene = EditorSceneManager.GetActiveScene();

            sceneToAdd.path = activeScene.path;
            sceneToAdd.enabled = true;

            var buildScenes = new List<EditorBuildSettingsScene>();

            EditorBuildSettings.scenes = buildScenes.ToArray();

            HideUtilHeirarchy();
            SetSceneToBuild();
        }

        private void OnSaveSetup()
        {
            if (!SetData()) return;
            ToggleEdit(false);

            saveSetup.style.display = DisplayStyle.None;
            editSetup.style.display = DisplayStyle.Flex;
            selectMarker.style.display = DisplayStyle.Flex;
            //HideUtilHeirarchy();
        }

        private void OnEditSetup()
        {
            ToggleEdit(true);

            saveSetup.style.display = DisplayStyle.Flex;
            editSetup.style.display = DisplayStyle.None;
            selectMarker.style.display = DisplayStyle.None;
        }

        private void OnSelectMarker()
        {
            GetWindow<SelectMarkerSize>("Select Marker", true, typeof(ProjectSetup_Hololens));
        }

        private void ToggleEdit(bool isEditable)
        {
            applicationName.SetEnabled(isEditable);
            appIcon.SetEnabled(isEditable);
            organisationName.SetEnabled(isEditable);
            packageName.SetEnabled(isEditable);
            packageDescription.SetEnabled(isEditable);
            deviceType.SetEnabled(isEditable);
            architecture.SetEnabled(isEditable);
            buildFolderLocation.SetEnabled(isEditable);
        }


        #region Write Data

        private bool SetData()
        {
            if(
                SetProductName(applicationName) &&
                SetAppIcon(appIcon) &&
                SetOrganisationName(organisationName) &&
                SetPackageName(packageName) &&
                SetPackageDescription(packageDescription) &&
                SetDeviceType(deviceType) &&
                SetArchitecture(architecture) &&
                SetBuildPath(buildFolderLocation.GetFile()))
            {
                return true;
            }
            return false;
        }

        private bool SetProductName(TextField applicationName)
        {

            if (string.IsNullOrEmpty(applicationName.value))
            {
                return !EditorUtility.DisplayDialog("Error Application Name", "No application name found. Insert new application name", "OK");
            }

            PlayerSettings.productName = applicationName.value;
            projectData_Hololens.applicationName = applicationName.value;
            return true;
        }
        private bool SetAppIcon(ObjectField appIcon)
        {

            if (appIcon.value == null)
            {
                var icon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/Icons/Dx_Logo.png", typeof(Texture2D));
                PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { icon }, IconKind.Any);
                projectData_Hololens.appIcon = icon;
                appIcon.value = icon;
                return EditorUtility.DisplayDialog("Warning Application Icon", "No application icon found. Default icon will be Dx's logo", "OK");
            }

            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new Texture2D[] { appIcon.value as Texture2D }, IconKind.Any);
            projectData_Hololens.appIcon = appIcon.value as Texture2D;
            return true;
        }

        private bool SetOrganisationName(TextField organisationName)
        {

            if (string.IsNullOrEmpty(organisationName.value))
            {
                return !EditorUtility.DisplayDialog("Error Organisation Name", "No organisation name found. Please insert new organisation name", "OK");
            }

            PlayerSettings.companyName = organisationName.value;
            projectData_Hololens.organisationName = organisationName.value;
            return true;
        }

        private bool SetPackageName(TextField packageName)
        {

            if (string.IsNullOrEmpty(packageName.value))
            {
                return !EditorUtility.DisplayDialog("Error Package Name", "No package name found. Please insert new package name", "OK");
            }

            PlayerSettings.WSA.packageName = packageName.value;
            projectData_Hololens.packageName = packageName.value;
            return true;
        }
        private bool SetPackageDescription(TextField packageDescription)
        {

            if (string.IsNullOrEmpty(packageDescription.value))
            {
                return !EditorUtility.DisplayDialog("Error Package Description", "No package description found. Please insert new package description", "OK");
            }

            PlayerSettings.WSA.applicationDescription = packageDescription.value;
            projectData_Hololens.packageDescription = packageDescription.value;
            return true;
        }
        private bool SetDeviceType(DropdownField deviceType)
        {
#pragma warning disable 0618
            if (deviceType.index != (int)WSASubtarget.HoloLens)
            {
                return !EditorUtility.DisplayDialog("Error Device Type", "Wrong device type selected. Please select HoloLens under device type dropdown", "OK");
            }

            EditorUserBuildSettings.wsaSubtarget = WSASubtarget.HoloLens;
            projectData_Hololens.deviceType = deviceType.value;
            return true;
        }
        private bool SetArchitecture(DropdownField architecture)
        {

            if (architecture.value != architectures[3])
            {
                return !EditorUtility.DisplayDialog("Error Architecture", "Wrong architecture selected. Please select ARM64 under architecture dropdown", "OK");
            }

            EditorUserBuildSettings.wsaArchitecture = architecture.value;
            projectData_Hololens.architecture = architecture.value;
            return true;
        }
        private bool SetBuildPath(string buildPath)
        {

            if (string.IsNullOrEmpty(buildPath))
            {
                return !EditorUtility.DisplayDialog("Error Build Path", "Empty build folder path found. Please insert build folder path", "OK");
            }

            if (!Directory.Exists(buildPath))
            {
                EditorUtility.DisplayDialog("Error Build Path", "No build folder location found. Please insert correct build folder path", "OK");
                return false;
            }

            // Assign build path to build window;

            projectData_Hololens.buildPath = buildPath;
            BuildDeployPreferences.BuildDirectory = buildPath;
            EditorUtility.SetDirty(projectData_Hololens);
            AssetDatabase.SaveAssetIfDirty(projectData_Hololens);
            e_projectSetup?.Invoke();
            return true;
        }

        private void LoadData(ProjectData_Hololens projectData_Hololens)
        {
            applicationName.value = projectData_Hololens.applicationName;
            appIcon.value = projectData_Hololens.appIcon;
            organisationName.value = projectData_Hololens.organisationName;
            packageName.value = projectData_Hololens.packageName;
            packageDescription.value = projectData_Hololens.packageDescription;
            deviceType.index = deviceType.choices.IndexOf(projectData_Hololens.deviceType);
            architecture.index = architecture.choices.IndexOf(projectData_Hololens.architecture);
            buildFolderLocation.SetValue(projectData_Hololens.buildPath);
        }

        #endregion

        #region Utility

        [MenuItem("Innovate Labs/Utilities/Hide Utility Heirarchy &L", false, 11)]
        public static void HideUtilHeirarchy()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var rootObjects = scene.GetRootGameObjects();

            var disableHeirarcyy = new List<GameObject>();
            var unpickable = new List<GameObject>();
            int isolatedIndex = -1;
            
            for(int i  = 0; i < rootObjects.Length; i++)
            {
                if (rootObjects[i].name == "SpatialPlacementManager")
                {
                    isolatedIndex = i;
                }
                if(isolatedIndex != -1)
                {
                    disableHeirarcyy.Add(rootObjects[i]);
                }
            }

            for(int i= 0; i < rootObjects.Length; i++)
            {
                unpickable.Add(rootObjects[i]);
                if (rootObjects[i].name == "-------------------------")
                {
                    break;
                }
                
            }
            HideGameObject.HideSelected(disableHeirarcyy);
            HideGameObject.DisablePicking(unpickable);
        }
        [MenuItem("Innovate Labs/Utilities/ Add Scene", false, 99)]
        public static void SetSceneToBuild()
        {
            var currentScene = EditorSceneManager.GetActiveScene();

            var sceneToAdd = new EditorBuildSettingsScene();

            sceneToAdd.path = currentScene.path;
            sceneToAdd.enabled = true;

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();

            scenes.Add(sceneToAdd);
            EditorBuildSettings.scenes = scenes.ToArray();
        }
        #endregion
    }
}
