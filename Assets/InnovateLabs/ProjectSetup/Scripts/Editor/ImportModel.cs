using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using Dummiesman;

using SysFile = System.IO.File;
using Object = UnityEngine.Object;
using UnityEditor.SceneManagement;
using UnityLengthUnit = UnityEngine.UIElements.LengthUnit;
using System.Threading.Tasks;
using InnovateLabs.Utilities;

namespace InnovateLabs.Projects
{
    public class ImportModel : EditorWindow
    {
        DropdownField modelFormat;

        DropdownField importUnit;

        PathField pathField;

        Button importModel;

        Button ergonomicEvalutation;

        ListView modelListView;
        string isErgonmicActive = "false";

        static readonly string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        private static readonly string modelAssetLocation = "/3DModels/";
        private static ImportModel wnd;
        [MenuItem("Innovate Labs/ Import 3D Model %#E", false, 23)]
        public static void ImportModelWindow()
        {
            GetWindow<ImportModel>("Import 3D Model", true, typeof(SelectMarkerSize));
        }


        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ImportModel.uxml");
            VisualElement UIFromXML = visualTree.Instantiate();
            root.Add(UIFromXML);

            Init(root);
        }

        private void OnEnable()
        {
            //ModelFolderProcessor.AssetsChanged += DrawModelList;
            ModelFolderProcessor.AssetsChanged += OpenClosewWindow;
        }
        private void OnDisable()
        {
            //ModelFolderProcessor.AssetsChanged -= DrawModelList;
            ModelFolderProcessor.AssetsChanged -= OpenClosewWindow;
        }

        private void OpenClosewWindow()
        {
            GetWindow<ImportModel>("Import 3D Model", true, typeof(ImportModel)).Close();
            GetWindow<ImportModel>("Import 3D Model", true, typeof(SelectMarkerSize));

            if(modelFileInfo.Count == 0)
            {
                ergonomicEvalutation.style.display = DisplayStyle.None;
                
            }
            else
            {
                ergonomicEvalutation.style.display = DisplayStyle.Flex;
             
            }
        }

        private void Init(VisualElement root)
        {
            modelFormat = root.Q<DropdownField>("ModelFormat");

            pathField = new PathField(root.Q<VisualElement>("ModelPathField"));

            importModel = root.Q<Button>("ImportModel");
            importModel.SetEnabled(false);

            ergonomicEvalutation = root.Q<Button>("ErgonomicEvaluation");

            importUnit = root.Q<DropdownField>("WorkingUnit");

            pathField.ImportSettings("Import 3D Model", myDocumentPath, "fbx");

            pathField.OnPathEmpty += OnPathEmpty;
            pathField.OnPathNotEmpty += OnPathNotEmpty;

            importModel.clicked += OnImportClicked;
            ergonomicEvalutation.clicked += OnErgonomicEvaluation;

            modelFormat.RegisterCallback<ChangeEvent<string>>(e =>
            {
                OnFormatChanged();
            });

            PixyzImportInit();

            /*var isModelExist = FindObjectOfType<ModelInfo>();

            if (isModelExist != null)
            {
                ergonomicEvalutation.style.display = DisplayStyle.Flex;
                Debug.Log("Ye waala init se call ho rela hai, model exist karta hai bhai");
            }
            else
            {
                ergonomicEvalutation.style.display = DisplayStyle.None;
                Debug.Log("Init se call hua hai aur model exist nahi karta!");
            }*/

            if (modelFileInfo.Count != 0)
            {
                ergonomicEvalutation.style.display = DisplayStyle.Flex;
            }
            else
            {
                ergonomicEvalutation.style.display = DisplayStyle.None;
            }

            DrawModelList();
        }


        private void OnPathEmpty()
        {
            importModel.SetEnabled(false);
        }

        private void OnPathNotEmpty()
        {
            importModel.SetEnabled(true);
        }

        private void OnFormatChanged()
        {
            switch (modelFormat.index)
            {
                case 0: //fbx
                    TogglePixyzImport(false);
                    pathField.ImportSettings("Import 3D Model", myDocumentPath, "fbx");
                    break;
                case 1: //obj
                    TogglePixyzImport(false);
                    pathField.ImportSettings("Import 3D Model", myDocumentPath, "obj");
                    break;
                case 2: //blend
                    TogglePixyzImport(false);
                    pathField.ImportSettings("Import 3D Model", myDocumentPath, "blend");
                    break;
                case 3: //pixyz
                    TogglePixyzImport(true);
                    //pathField.ImportSettings("Import 3D Model", myDocumentPath, "blend");
                    break;
                default:
                    break;
            }
            pathField.SetPathEmpty();

        }


        private void OnSaveClicked()
        {

        }
        private void OnErgonomicEvaluation()
        {
            GetWindow<ErgonomicEvaluation>("Ergonomic Evaluation", true, typeof(ImportModel));
        }

        private void OnImportClicked()
        {
            switch (modelFormat.index)
            {
                case 0: //fbx
                    ImportFbx(pathField.GetFile(), importUnit.value);
                    break;
                case 1: //obj
                    ImportObj(pathField.GetFile(), importUnit.value);
                    break;
                case 2: //blend
                    ImportBlend(pathField.GetFile(), importUnit.value);
                    break;
                default:
                    break;
            }
            if(modelFileInfo.Count != 0)
            {
                ergonomicEvalutation.style.display = DisplayStyle.Flex;
            }
            else
            {
                ergonomicEvalutation.style.display = DisplayStyle.None;
            }

        }

        private void ToggleEdit(bool isToggle)
        {

        }

        private bool isInside = false;

        private void OnCollisionEnter()
        {
            var muffler = new GameObject();
            if (isInside)
            {
                muffler.layer = LayerMask.NameToLayer("Inside");
                isInside = false;
            }
            else
            {
                muffler.layer = LayerMask.NameToLayer("Outside");
                isInside = true;
            }
        }
        private void ImportFbx(string modelSourceLocation, string unit)
        {
            Import3DModel(modelSourceLocation, unit, ".fbx");
        }

        private void ImportObj(string modelSourceLocation, string unit)
        {
            OBJLoader objLoader = new OBJLoader();
            var importedObj = objLoader.Load(modelSourceLocation);

            var modelPath = modelSourceLocation.Split('/');
            var modelNameWithExtension = modelPath[modelPath.Length - 1];
            float scaleValue = 1f;

            switch (unit)
            {
                case "mm":
                    scaleValue = 0.001f;
                    break;
                case "cm":
                    scaleValue = 0.01f;
                    break;
                case "m":
                    scaleValue = 1f;
                    break;
            }

            var meshes = importedObj.GetComponentsInChildren<MeshFilter>();
            foreach (var mesh in meshes)
            {
                ScaleObjMesh(mesh.sharedMesh, 1 / scaleValue);
            }

            var objHolder = new GameObject();
            objHolder.name = importedObj.name;
            importedObj.transform.SetParent(objHolder.transform);

            objHolder.transform.localScale = Vector3.one * scaleValue;
            importedObj.transform.localScale = Vector3.one;

            var importedModel = Application.dataPath + modelAssetLocation + modelNameWithExtension;
        }

        private void ImportBlend(string modelSourceLocation, string unit)
        {
            Import3DModel(modelSourceLocation, unit, ".blend");
        }
        private void Import3DModel(string modelSourceLocation, string unit, string modelFormat)
        {
            var modelPath = modelSourceLocation.Split('/');
            var modelNameWithExtension = modelPath[modelPath.Length - 1];

            var importedModel = Application.dataPath + modelAssetLocation + modelNameWithExtension;

            if (SysFile.Exists(importedModel))
            {
                int importError = EditorUtility.DisplayDialogComplex("Error Importing : ",
                                modelNameWithExtension + "Already Exists!\nDo you want to override existing file?",
                                "Yes",
                                "Abort",
                                "Rename");
                switch (importError)
                {
                    case 0:
                        SysFile.Delete(importedModel);
                        break;

                    case 1:
                        return;

                    case 2:

                        DirectoryInfo dir = new DirectoryInfo(Application.dataPath + modelAssetLocation);
                        var models = dir.GetFiles("*" + modelFormat).ToList();

                        if (models.Count > 0)
                        {
                            var modelFileName = modelNameWithExtension.Remove(modelNameWithExtension.IndexOf(modelFormat));
                            var modelDupilcates = models.FindAll(x => x.Name.Contains(modelFileName));

                            modelNameWithExtension = modelFileName + "_" + modelDupilcates.Count + modelFormat;
                            importedModel = Application.dataPath + modelAssetLocation + modelNameWithExtension;

                        }

                        break;

                    default:

                        Debug.LogError("Unknown Input Recveied");
                        break;
                }
            }

            FileUtil.CopyFileOrDirectory(modelSourceLocation, importedModel);

            AssetDatabase.ImportAsset(importedModel);

            var modelName = modelNameWithExtension.Remove(modelNameWithExtension.IndexOf(modelFormat));
            var materialFolder = "Assets" + modelAssetLocation + "Materials/";

            AssetDatabase.CreateFolder(materialFolder, modelName);

            AssetDatabase.Refresh();

            ExtractMaterials(importedModel, materialFolder + modelName);

            ModelImporter modelImporter = (ModelImporter)ModelImporter.GetAtPath("Assets" + modelAssetLocation + modelNameWithExtension);

            float globalScaleValue = 1f;
            float localScaleValue = GetLocalScale(out globalScaleValue);

            modelImporter.globalScale = globalScaleValue;
            modelImporter.useFileScale = true;
            modelImporter.useFileUnits = true;
            modelImporter.importCameras = false;
            modelImporter.importLights = false;
            modelImporter.optimizeMeshPolygons = false;
            modelImporter.optimizeMeshVertices = false;
            modelImporter.isReadable = true;


            modelImporter.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
            modelImporter.materialLocation = ModelImporterMaterialLocation.InPrefab;

            #region Material Extraction

            //var assetsToReload = new HashSet<string>();
            ////var materials = AssetDatabase.LoadAllAssetsAtPath(importedModel).Where(x => x.GetType() == typeof(Material)).ToArray();

            //IEnumerable<Object> materials = from x in AssetDatabase.LoadAllAssetsAtPath(importedModel)
            //                where x.GetType() == typeof(Material)
            //                select x;

            //Debug.Log(string.Format("{0} materials load from {1}", materials.ToList().Count, importedModel)) ;

            //foreach(var material in materials)
            //{
            //    var newAssetPath = materialFolder + modelName + "/" + material.name + ".mat";
            //    Debug.Log("New Asset Path : " + newAssetPath);
            //    //var newAssetPath = materialFolder + modelName + "/" + material.name + ".mat";
            //    newAssetPath = AssetDatabase.GenerateUniqueAssetPath(newAssetPath);
            //    var error = AssetDatabase.ExtractAsset(material, newAssetPath);

            //    if (String.IsNullOrEmpty(error))
            //    {
            //        assetsToReload.Add(importedModel);
            //    }
            //}

            //foreach (var path in assetsToReload)
            //{
            //    AssetDatabase.WriteImportSettingsIfDirty(path);
            //    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            //}

            //modelImporter.ExtractTextures(materialFolder + modelName + "/Textures");
            #endregion Material Extraction

           modelImporter.SaveAndReimport();

            AssetDatabase.Refresh();

            #region code that directly add the model to the heirarchy
            //commented out for now
            /*var load_model = (GameObject)AssetDatabase.LoadAssetAtPath("Assets" + modelAssetLocation + modelNameWithExtension, typeof(GameObject));

            var model = Instantiate(load_model, Vector3.zero, Quaternion.identity);

            model.name = modelName;
            model.transform.localScale *= localScaleValue;
            var modelInfo = model.AddComponent<ModelInfo>();
            modelInfo.SetCurrentUnit(importUnit.value);*/
            #endregion
            // Manage Heirarchy
        }

        public static void ExtractMaterials(string assetPath, string destinationPath)
        {
            HashSet<string> hashSet = new HashSet<string>();
            IEnumerable<Object> enumerable = from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
                                             where x.GetType() == typeof(Material)
                                             select x;

            foreach (Object item in enumerable)
            {
                string path = System.IO.Path.Combine(destinationPath, item.name) + ".mat";
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                string value = AssetDatabase.ExtractAsset(item, path);
                if (string.IsNullOrEmpty(value))
                {
                    hashSet.Add(assetPath);
                }
            }

            foreach (string item2 in hashSet)
            {
                AssetDatabase.WriteImportSettingsIfDirty(item2);
                AssetDatabase.ImportAsset(item2, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void ScaleObjMesh(Mesh mesh, float scale)
        {
            var baseVertices = mesh.vertices;

            var vertices = new Vector3[baseVertices.Length];

            for (var i = 0; i < vertices.Length; i++)
            {
                var vertex = baseVertices[i];
                vertex.x = vertex.x * scale;
                vertex.y = vertex.y * scale;
                vertex.z = vertex.z * scale;

                vertices[i] = vertex;
            }

            mesh.vertices = vertices;

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private float GetLocalScale(out float globalScaleValue)
        {
            globalScaleValue = 1f;
            float localScaleValue = 1f;

            switch (importUnit.value)
            {
                case "mm":
                    globalScaleValue = 1000f;
                    localScaleValue = 0.001f;
                    break;
                case "cm":
                    globalScaleValue = 100f;
                    localScaleValue = 0.01f;
                    break;
                case "m":
                    globalScaleValue = 1f;
                    localScaleValue = 1f;
                    break;
                default:
                    globalScaleValue = 1f;
                    localScaleValue = 1f;
                    break;
            }

            return localScaleValue;
        }


        #region Models In Project
        
        public static List<FileInfo> modelFileInfo = new List<FileInfo>();

        bool hasRefreshed = false;
        private void DrawModelList()
        {
            AssetDatabase.Refresh();

            if (modelFileInfo != null)
                modelFileInfo.Clear();

            var modelList = rootVisualElement.Q<ListView>("ModelList");
            modelListView = modelList;

            var modelListElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ButtonListElement.uxml");

            var modelDir = new DirectoryInfo(Application.dataPath + modelAssetLocation);
            
            var files = modelDir.GetFiles();

            for(int i = 0; i < files.Length; i++)
            {
                if (modelFileInfo.Contains(files[i]))
                {
                    return;
                }
                var fileName = files[i].Name.Split('.');

                if (fileName[fileName.Length - 1] == "prefab" ||
                fileName[fileName.Length - 1] == "fbx" ||
                fileName[fileName.Length - 1] == "blend" ||
                fileName[fileName.Length - 1] == "obj")
                {
                    modelFileInfo.Add(files[i]);
                }
            }

            modelFileInfo = modelFileInfo.OrderByDescending(x => x.CreationTime).ToList();

            

            modelList.makeItem = modelListElement.Instantiate;
            modelList.itemsSource = modelFileInfo;
            modelList.bindItem = (e, i) =>
            {
                if (i >= modelFileInfo.Count && i < 0) return;
                if (e != null && modelFileInfo[i] != null)
                {
                    var modelLabel = e.Q<Label>();
                    modelLabel.style.fontSize = 15f;
                    var modelID = modelFileInfo[i].Name.Split('.');
                    modelLabel.text = $"{modelID[0]}";

                    var modelButton = e.Q<Button>("button1");
                    modelButton.text = "Add";

                    var showButton = e.Q<Button>("button2");
                    showButton.text = "Show";



                    modelButton.clicked += () =>
                    {
                        AddSelectedModel(modelFileInfo[i].FullName, modelID[0], modelID[modelID.Length - 1]);
                    };

                    showButton.clicked += () =>
                    {
                        string path = Path.GetRelativePath(Application.dataPath, modelFileInfo[i].FullName);
                        path = "Assets/" + path.ToUnityPath();
                        var obj = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

                        EditorWindow projectWindow = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.ProjectBrowser"));

                        if (projectWindow == null)
                        {
                            System.Type projectBrowserType = System.Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
                            EditorWindow projectBrowser = EditorWindow.GetWindow(projectBrowserType);
                            projectBrowser.Show();
                        }
                        Selection.activeObject = obj;
                        EditorGUIUtility.PingObject(obj);
                    };

                   
                }

            };

            modelList.headerTitle = $"{modelFileInfo.Count} model{(modelFileInfo.Count > 1 ? "s " : " ")}in this project";
            
            modelList.selectionType = SelectionType.Single;
            modelList.showBorder = false;
            modelList.showFoldoutHeader = true;
            modelList.showBoundCollectionSize = false;
            modelList.showAddRemoveFooter = false;
            modelList.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            modelList.style.marginTop = new StyleLength(15f);
            modelList.style.marginBottom = new StyleLength(5f);
            modelList.style.marginLeft = new StyleLength(10f);
            modelList.style.marginRight = new StyleLength(5f);

            
            modelList.onSelectionChange += objects =>
            {
                var torray = objects.First();
            };
            
        }

        private void AddSelectedModel(string path, string modelName, string format)
        {
            path = Path.GetRelativePath(Application.dataPath, path);
            path = "Assets/" + path.ToUnityPath();
            var load_model = (GameObject)AssetDatabase.LoadAssetAtPath( path, typeof(GameObject));
            var model = Instantiate(load_model, Vector3.zero, Quaternion.identity); 
            Undo.RegisterCreatedObjectUndo(model, $"Added To Heirarchy : {model.name}");
            
            model.name = modelName;
            ModelInfo modelInfo;
            
            if (format == "prefab")
            {
                model.transform.localScale = load_model.transform.localScale;
            }
            else
            {
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                modelInfo = model.AddComponent<ModelInfo>();
                switch (importer.globalScale)
                {
                    case 1000f:
                        model.transform.localScale *= 0.001f;
                        modelInfo.SetCurrentUnit("mm");
                        break;
                    case 100f:
                        model.transform.localScale *= 0.01f;
                        modelInfo.SetCurrentUnit("cm");
                        break;
                    case 1f:
                        model.transform.localScale *= 1f;
                        modelInfo.SetCurrentUnit("m");
                        break;
                    default:
                        model.transform.localScale *= 0.001f;
                        modelInfo.SetCurrentUnit("mm");
                        break;
                    
                }
            }

            
        }
        #endregion Models In Project



        #region PIXYZ IMPORT

        PathField pixyzPrefabField;
        ListView pixyzPrefabList;

        private List<FileInfo> pixyzPrefabInfo = new List<FileInfo>();
        private void PixyzImportInit()
        {
            InitPixyzPrefabPath();

        }

        private void InitPixyzPrefabPath()
        {
            if (pixyzPrefabField == null)
            {
                pixyzPrefabField = new PathField(rootVisualElement.Q<VisualElement>("PixyzPrefabPath"));
                pixyzPrefabField.ImportSettings("Pixyz Prefabs", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "");
                pixyzPrefabField.OnPathChanged += DrawPixyzPrefabList;
            }
        }

        private void DrawPixyzPrefabList(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if(!Directory.Exists(path))
            {
                EditorUtility.DisplayDialog("Error", "Unable to find path", "Ok");
                return;
            }

            pixyzPrefabList = rootVisualElement.Q<ListView>("PixyzPrefabList");

            var prefabListElement = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ButtonListElement.uxml");

            var prefabDir = new DirectoryInfo(pixyzPrefabField.GetFile());

            var files = prefabDir.GetFiles();

            if (pixyzPrefabInfo != null) pixyzPrefabInfo.Clear();

            foreach (var file in files)
            {
                var fileName = file.Name.Split('.');
                if (fileName[fileName.Length - 1] == "unitypackage")
                {
                    pixyzPrefabInfo.Add(file);
                }
            }

            pixyzPrefabInfo = pixyzPrefabInfo.OrderByDescending(x => x.CreationTime).ToList();

            pixyzPrefabList.makeItem = prefabListElement.Instantiate;
            pixyzPrefabList.itemsSource = pixyzPrefabInfo;
            
            pixyzPrefabList.bindItem = (e, i) =>
            {
                if (i >= pixyzPrefabInfo.Count && i < 0) return;
                if (e != null && pixyzPrefabInfo[i] != null)
                {
                    var modelLabel = e.Q<Label>();
                    modelLabel.style.fontSize = 15f;
                    modelLabel.text = $"{pixyzPrefabInfo[i].Name.Split('.')[0]}";
                    var modelButton = e.Q<Button>("button1");
                    modelButton.text = "Import";
                    modelButton.clicked += () => ImportPixyzPrefab(pixyzPrefabInfo[i].FullName);
                }
            };

            pixyzPrefabList.headerTitle = $"{pixyzPrefabInfo.Count} items";
            
            pixyzPrefabList.selectionType = SelectionType.Single;
            pixyzPrefabList.showBorder = false;
            pixyzPrefabList.showFoldoutHeader = true;
            pixyzPrefabList.showBoundCollectionSize = false;
            pixyzPrefabList.showAddRemoveFooter = false;
            pixyzPrefabList.showAlternatingRowBackgrounds = AlternatingRowBackground.All;
            pixyzPrefabList.style.marginTop = new StyleLength(15f);
            pixyzPrefabList.style.marginBottom = new StyleLength(5f);
            pixyzPrefabList.style.marginLeft = new StyleLength(10f);
            pixyzPrefabList.style.marginRight = new StyleLength(5f);

            pixyzPrefabList.onSelectionChange += objects =>
            {
                var torray = objects.First();
            };

        }

        private void TogglePixyzImport(bool isVisible)
        {
            var pixyzPrefabImport = rootVisualElement.Q<VisualElement>("PixyzPrefabImport");
            if (isVisible)
            {
                importModel.style.display = DisplayStyle.None;
                importUnit.style.display = DisplayStyle.None;
                pathField.pathField.style.display = DisplayStyle.None;
                pixyzPrefabImport.style.display = DisplayStyle.Flex;
                DrawPixyzPrefabList(pixyzPrefabField.GetFile());
            }
            else
            {
                importModel.style.display = DisplayStyle.Flex;
                importUnit.style.display = DisplayStyle.Flex;
                pathField.pathField.style.display = DisplayStyle.Flex;
                pixyzPrefabImport.style.display = DisplayStyle.None;
            }
        }

        private void ImportPixyzPrefab(string path)
        {
            AssetDatabase.ImportPackage(path, false);
            AssetDatabase.Refresh();
        }

        #endregion PIXYZ IMPORT
    }

    public class PathField
    {
        public VisualElement pathField;

        TextField pathText;
        Button browse;

        string title;
        string filePath;
        string defaultPath;
        string fileType;
        string filename;

        public Action OnPathEmpty;
        public Action OnPathNotEmpty;
        public Action<string> OnPathChanged;
        public PathField(VisualElement pathField)
        {
            this.pathField = pathField;
            pathText = pathField.Q<TextField>();
            browse = pathField.Q<Button>();

            pathText.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                if (string.IsNullOrEmpty(pathText.value))
                {
                    if (OnPathEmpty != null) OnPathEmpty.Invoke();
                }
                OnPathChanged?.Invoke(evt.newValue);
            });

            browse.clicked += OnBrowseClicked;
        }

        private void OnBrowseClicked()
        {
            if (!Browse())
            {
                return;
            }

            pathText.value = filePath;
            OnPathNotEmpty?.Invoke();
        }
        private bool Browse()
        {
            if (string.IsNullOrEmpty(fileType))
            {
                return BrowsePath();
            }

            return BrowseFile();
        }
        private bool BrowsePath()
        {
            var path = filePath;

            if (string.IsNullOrEmpty(path))
                path = "";
            else
                path = filePath;

            var selectedPath = EditorUtility.OpenFolderPanel(title, path, "");

            if (!string.IsNullOrEmpty(selectedPath))
            {
                filePath = selectedPath;
                return true;
            }


            return false;
        }
        private bool BrowseFile()
        {
            var path = filePath;

            if (string.IsNullOrEmpty(path))
                path = defaultPath;
            else
                path = filePath;

            var selectedPath = EditorUtility.OpenFilePanel(title, path, fileType);

            if (!string.IsNullOrEmpty(selectedPath))
            {

                filePath = selectedPath;

                var selectedFile = filePath.Split("/");
                var fileName = selectedFile[selectedFile.Length - 1];
                var fileExtentsion = fileName.Split(".");
                this.filename = fileExtentsion[0];
                var type = fileExtentsion[fileExtentsion.Length - 1];

                if (type != fileType)
                {
                    var isOk = EditorUtility.DisplayDialog("Error",
                        string.Format("Wrong file type selected! Select file with extension : {0}", fileType),
                        "Ok", "Cancel");

                    if (!isOk)
                    {
                        pathText.value = string.Empty;
                        return false;
                    }
                    else
                    {
                        return BrowseFile();
                    }
                }
            }


            return true;
        }
        public void ImportSettings(string title, string defaultPath, string fileType)
        {
            this.title = title;
            this.defaultPath = defaultPath;
            this.fileType = fileType;
        }
        public string GetFile()
        {
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = pathText.text;
            }
            return filePath;
        }
        public void SetPathEmpty()
        {
            pathText.value = string.Empty;
        }
        public void SetEnabled(bool value)
        {
            pathText.SetEnabled(value);
            browse.SetEnabled(value);
        }
        public void SetValue(string path)
        {
            pathText.value = path;
        }

        //public string GetFileName()
        //{
        //    return filename;
        //}
    }

}
