using InnovateLabs.Utilities;
using Microsoft.MixedReality.Toolkit.Build.Editor;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using Directory = System.IO.Directory;
using File = System.IO.File;
using System.Linq;

namespace InnovateLabs.Projects
{
    public class BuildWindow : EditorWindow
    {
      
        
        [MenuItem("Innovate Labs/Build Window %#B", false, 101)]
        public static void ProjectSetupHololensWindow()
        {
            BuildWindow wnd = GetWindow<BuildWindow>("Build Window",true, typeof(ErgonomicEvaluation));
            wnd.titleContent = new GUIContent($"Build Window");
        }

        private static string buildPath = "";
        private string previousBuild = "";

        Label buildPathLabel, previousBuildLabel;

        Toggle clearPreviousBuild, clearBuildFolder;

        Button openBuildFolder, openPreviousBuild, generateButton;

        VisualElement progressBarBase, progressBar, root;


        private void OnEnable()
        {
            
            BuildDeployWindow.e_progressBar += LoadProgressBar;

            ProjectSetup_Hololens.e_projectSetup += SetBuildPath;
        }

        public void CreateGUI()
        {
            root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/BuildWindow.uxml");
            VisualElement UIFromXML = visualTree.Instantiate();
            root.Add(UIFromXML);

            buildPathLabel = root.Q<Label>("BuildPath");
            previousBuildLabel = root.Q<Label>("PreviousBuild");

            clearPreviousBuild = root.Q<Toggle>("ClearPreviousBuild");
            clearPreviousBuild.RegisterValueChangedCallback<bool>(ClearPreviousBuild);

            clearBuildFolder = root.Q<Toggle>("ClearBuildFolder");
            clearBuildFolder.RegisterValueChangedCallback<bool>(ClearBuildFolder);

            progressBarBase = root.Q<VisualElement>("ProgressBarParentActual");
            
            progressBar = progressBarBase.Q<VisualElement>("ProgressBarActual");
            progressBarBase.style.display = DisplayStyle.None;


            /*if (File.Exists("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset"))
            {
                var projectData_Hololens = (ProjectData_Hololens)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset", typeof(ProjectData_Hololens));

                buildPath = projectData_Hololens.buildPath + "/";
                buildPathLabel.text = buildPath;
            }*/

            SetBuildPath();

            openBuildFolder = root.Q<Button>("OpenBuildFolder");
            openBuildFolder.clicked += OpenBuildFolder;

            openPreviousBuild = root.Q<Button>("OpenPreviousBuild");
            openPreviousBuild.clicked += OpenPreviousBuild;

            generateButton = root.Q<Button>("GenerateBuild");
            generateButton.clicked += () => HideBuildButton(false);
            generateButton.clicked += GenerateBuild;

            InitPreviousBuild();
        }

        public void SetBuildPath()
        {
            if (File.Exists("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset"))
            {
                var projectData_Hololens = (ProjectData_Hololens)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/ProjectData.asset", typeof(ProjectData_Hololens));

                buildPath = projectData_Hololens.buildPath + "/";
                //buildPathLabel.text = buildPath;
            }
        }

        public void Update()
        {
            if (progressBarBase.style.display == DisplayStyle.Flex)
                ProgressBarAnimation();
        }
        private void ClearPreviousBuild(ChangeEvent<bool> value)
        {
            clearPreviousBuild.value = value.newValue;
            if(clearPreviousBuild.value)
                clearBuildFolder.value = false;        
        }
        private void ClearBuildFolder(ChangeEvent<bool> value)
        {
            clearBuildFolder.value = value.newValue;
            if (clearBuildFolder.value)
                clearPreviousBuild.value = false;
        }

        private void InitPreviousBuild()
        {

            #region shut up
            /*var buildFolder = BuildDeployPreferences.BuildDirectory;
            
            var appPackages = buildFolder + @"/AppPackages/";

            if(!Directory.Exists(appPackages))
            {
                previousBuildLabel.text = $"No build available";
                return;
            }

            var info = new DirectoryInfo(appPackages);
            
            var appPackageInfo = info.GetDirectories();
            
            var currentApplication = PlayerSettings.productName;

            if(appPackageInfo == null || appPackageInfo.Length == 0)
            {
                previousBuildLabel.text = $"No build available";
            }

            foreach (var app in appPackageInfo)
            {
                if(app.Name == currentApplication)
                {
                    var currentAppPath = app.FullName + @"\";
                    var currentApp = new DirectoryInfo(currentAppPath);
                    var allVersions = currentApp.GetDirectories();
                    int highestVersion = 0;
                    for(int i = 0; i < allVersions.Length; i++)
                    {
                        var versionName = allVersions[i].Name.Split('_');
                        var version = versionName[1].Split('.');

                        var lastIndex = version[version.Length - 2];

                        int index = int.Parse(lastIndex);

                        if (index > highestVersion)
                        {
                            highestVersion = i;
                        }
                    }
                    
                    var path = allVersions[highestVersion].FullName.ToUnityPath();
                    previousBuild = path;
                    previousBuildLabel.text = $"<b>Created :</b> {allVersions[highestVersion].LastWriteTime}\n<b>Location :</b> {previousBuild}";

                }
            }*/

            #endregion
            var buildFolderDirectory = BuildDeployPreferences.BuildDirectory;
            var appPackages = $"{buildFolderDirectory}/AppPackages/{PlayerSettings.productName}/";

            /*Debug.Log($"appPackages : {appPackages}");*/

            if (!Directory.Exists(appPackages))
            {
                Debug.Log("AppPackages Directory doesnt exist");
                previousBuildLabel.text = "No build available";
                return;
            }

            var info = new DirectoryInfo(appPackages);
            var allFiles = info.GetDirectories();
            if (allFiles.Length == 0)
            {
                //Debug.Log("Ooga booga your info directory is null!");
                previousBuildLabel.text = "No build available";
                return;
            }

            /*if(allFiles.Length == 0)
            {
                Debug.Log("Files not found! Big F");
            }
            else
            {
                foreach (var file in allFiles)
                {
                    Debug.Log($"Current File in  info Directory : {file}");
                }
            }*/


            var latestFile = allFiles.OrderByDescending(f => f.LastWriteTime).First();
            /*Debug.Log(latestFile);*/
            var previousBuildPath = latestFile.FullName.ToUnityPath();
            previousBuild = latestFile.FullName.ToExplorerPath();
            previousBuildLabel.text = $"<b>Created :</b> {latestFile.LastWriteTime}\n<b>Location :</b> {previousBuildPath}";
        }

        private void OpenBuildFolder()
        {
            var appPackages = $"{BuildDeployPreferences.BuildDirectory}/AppPackages/{PlayerSettings.productName}/";
            var path = appPackages.ToExplorerPath();
            var buildFolderPath = buildPath.ToExplorerPath();
            /*Debug.Log($"Build path : {buildFolderPath}");
            Debug.Log($"path : {path}");

            Debug.Log($"path? : {Directory.Exists(path)}");
            Debug.Log($"build folder? : {Directory.Exists(buildFolderPath)}");*/

            if (!Directory.Exists(path))
            {
                Debug.Log("path doesnot exist");
                System.Diagnostics.Process.Start("explorer.exe", buildFolderPath);
            }
            else
            {
                Debug.Log("path exists");
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            //System.Diagnostics.Process.Start("explorer.exe", "/select," + path);

        }

        private void OpenPreviousBuild()
        {
            var path = previousBuild.ToExplorerPath();   // explorer doesn't like front slashes
            if (!Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Unable to open the directory!",
                                                         "No previous builds were found, please generate a build first", "ok");
                return;
            }
            System.Diagnostics.Process.Start("explorer.exe",path);
        }

        private void HideBuildButton(bool flag)
        {
            generateButton.SetEnabled(flag);
        }

        private void GenerateBuild()
        {
            var currentScene = EditorSceneManager.GetActiveScene();
            bool dontHide = true;
            if (currentScene.isDirty)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                HideBuildButton(dontHide);
                return;
            }
            
            if(EditorUserBuildSettings.activeBuildTarget != BuildTarget.WSAPlayer)
            {
                
                bool option = EditorUtility.DisplayDialog("Invalid Build Target!",
                                                         $"Current Build Target is set to " +
                                                         $"{EditorUserBuildSettings.activeBuildTarget} instead of " +
                                                         $"{BuildTarget.WSAPlayer}","Switch Build Target","Ok");
                if (option)
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
                    HideBuildButton(dontHide);
                    return;
                }
                HideBuildButton(dontHide);
                return;
            }


            if (EditorUserBuildSettings.wsaArchitecture != "ARM64" || UwpBuildDeployPreferences.BuildConfigType != WSABuildType.Master) /*|| UwpBuildDeployPreferences.BuildConfigType != WSABuildType.Release)*/
            { 
                bool option = EditorUtility.DisplayDialog("Invalid Build Settings!",
                                                         $"Current Build Settings are set to " +
                                                         $"{EditorUserBuildSettings.wsaArchitecture} and {UwpBuildDeployPreferences.BuildConfig} " +
                                                         $"instead of ARM64 and {WSABuildType.Master}", "Switch Build Settings");

                if (option)
                {
                    EditorUserBuildSettings.wsaArchitecture = "ARM64";
                    UwpBuildDeployPreferences.BuildConfig = "Master";
                    HideBuildButton(dontHide);
                }
                HideBuildButton(dontHide);
                return;
            }

            var tapToPlace = FindObjectOfType<TapToPlaceManager>();
            if (tapToPlace != null)
            {
                //tapToPlace.GetTransformOffset();
            }
            else
            {
                Debug.LogError("Unable to find TapToPlaceManager.cs");
                return;
            }
            var startTime = System.DateTime.Now;
            int i = IEGenerateBuild();

        }

        private float timer = 50f;
        private float valToLerp = 1f;
        private float animationSpeed = 10f;
        private float previousTime;
        private float deltaTime;
        public async void ProgressBarAnimation()
        {
            timer = 0;
            if (timer <= 100f)
            {
                //float delta = Time.deltaTime * animationSpeed;

                valToLerp += 0.05f;

                progressBar.style.width = Length.Percent(valToLerp);

                if (valToLerp >= 100f)
                {
                    valToLerp = 0f;
                }
                timer += 0.005f;
                await Task.Delay((int)animationSpeed * 1000);
                
            }
        }
        private int IEGenerateBuild()
        {
            int build = 0, folder = 0;
            if (clearPreviousBuild.value)
            {
                build = DeletePreviousBuild();
            }
            if (clearBuildFolder.value)
            {
                folder = DeleteBuildFolder();
            }

            return BuildAll();
        }

        private int DeletePreviousBuild()
        {
            if(Directory.Exists(previousBuild.ToExplorerPath()))
            {
                Directory.Delete(previousBuild.ToExplorerPath(), true);
                return 1;
            }

            return 0;
        }
        private int DeleteBuildFolder()
        {
            if(Directory.Exists(buildPath.ToExplorerPath()))
            {
                Directory.Delete(buildPath.ToExplorerPath(), true);
                return 1;
            }
            return 0;
        }

        private int BuildAll()
        {
            BuildDeployWindow.OpenBuild += OpenNewBuild;
            BuildDeployWindow.BuildAll(false);
            
            return 0;
        }


        private void OpenNewBuild()
        {
            InitPreviousBuild();
            OpenPreviousBuild();
        }

        
        public void LoadProgressBar()
        {
            if(BuildDeployWindow.isBuilding == false)
            {
                progressBarBase.style.display = DisplayStyle.None;
                generateButton.SetEnabled(true);
                return;
            }
            //progressBarBase.style.display = DisplayStyle.Flex;
        }
    }
    
}



