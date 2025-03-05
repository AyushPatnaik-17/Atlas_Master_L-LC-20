using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using Microsoft.MixedReality.Toolkit;
using InnovateLabs.Utilities;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.Build.Editor;
using System.Threading.Tasks;
using MeshProcess;

namespace InnovateLabs.Projects
{
    public class ErgonomicEvaluation : EditorWindow
    {
        string isErgonmicActive = "false";

        public static ErgonomicEvaluation instance;

        public static List<GameObject> stepInteractables = new List<GameObject>();

        private static List<StepData> steps = new List<StepData>();
        private static List<GameObject> selections = new List<GameObject>();

        static readonly string myDocumentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        ListView touchListView;

        VisualElement stepDetail;

        VisualElement root, touchInteractionInfo, movementInteractionInfo, rotateInteractionInfo, partCollisionInfo;

        public static StepData currentStepData;

        InteractionTypes previousInteraction, currentInteraction;

        GameObject currentFeedbackPointer;

        private static readonly string modelAssetLocation = "/InnovateLabs/Models/";

        private static readonly string pointerPrefabPath = "Assets/InnovateLabs/ProjectSetup/Prefabs/FeedbackPointer.prefab";


        [MenuItem("Innovate Labs/ Ergonomic Evalutaion %#R", false, 24)]
        public static void ErgonomicEvaluationWindow()
        {
            instance = GetWindow<ErgonomicEvaluation>();
            instance.titleContent = new GUIContent("Ergonomic Evaluation");
            instance.Show();
        }

        public void CreateGUI()
        {
            //if (Application.isPlaying) return;
            root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ErgonomicEvaluation.uxml");
            VisualElement UIFromXML = visualTree.Instantiate();

            root.Add(UIFromXML);

            Init();

        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            BuildDeployWindow.OnErgonomicEvaluation += LoadInteractables;
            BuildDeployWindow.OnErgoEvaluation += RefreshItem;
            //CustomEditorEventSystem.onClearErgoData += ClearErgonomicData;
        }

        private void OnDisable()
        {
            //CustomEditorEventSystem.onClearErgoData -= ClearErgonomicData;
        }

        private void OnSceneGUI(SceneView scene)
        {
            if (currentFeedbackPointer != null)
            {
                if (currentFeedbackPointer.GetComponent<HighlightSphere>().isMoving)
                {
                    FeedbackPointer.MovePointer(currentFeedbackPointer.transform, Event.current, IsCollisionInteraction());
                }
            }

            DrawTranslateDistance();
        }



        private void Init()
        {
            #region Interactable Parts

            var addInteraction = root.Q<Button>("AddSelected");
            addInteraction.clicked += AddSelectedInteractable;

            var removeInteraction = root.Q<Button>("RemoveSelected");
            removeInteraction.clicked += RemoveSelectedInteractable;

            var removeAll = root.Q<Button>("RemoveAll");
            removeAll.clicked += RemoveAllInteractable;

            LoadInteractables();

            #endregion Interactable Parts

            stepDetail = root.Q<VisualElement>("StepDetailHolder");

            #region Step List

            var addStep = root.Q<Button>("AddStep");
            addStep.clicked += OnAddStepClicked;

            LoadAllSteps();

            var deleteStep = root.Q<Button>("DeleteStep");
            deleteStep.clicked += OnDeleteStepClicked;

            #endregion Step List

            #region Step Details

            var stepDescription = root.Q<TextField>("StepDescription");

            var interactionType = root.Q<EnumField>("InteractionType");

            interactionType.RegisterCallback<ChangeEvent<string>>(e =>
            {
                #region unable to delete implementation - causes mutliple intantiation problem. changes also made in the OnStepDeleted method
                //OnInteractionTypeChanged();

                var value = (InteractionTypes)interactionType.value;
                if (value != InteractionTypes.PartCollision && currentStepData.interactionTypes == InteractionTypes.PartCollision && !string.IsNullOrEmpty(currentStepData.collisionManager))
                {
                    if (value == InteractionTypes.Translate && currentStepData.interactionTypes == InteractionTypes.PartCollision)
                    {
                        EditorUtility.DisplayDialog("Warning", "Unable to Switch to Translate", "Ok");
                        interactionType.value = InteractionTypes.Rotate;
                        return;
                    }
                    var option = EditorUtility.DisplayDialog("Warning", "Collision data will be deleted", "Ok, Delete", "Don't Delete");
                    if (!option)
                    {
                        interactionType.value = InteractionTypes.PartCollision;
                        return;
                    }
                    else
                    {
                        RemoveCollisionChecker();
                    }
                }
                if (value != InteractionTypes.Rotate && currentStepData.interactionTypes == InteractionTypes.Rotate && !string.IsNullOrEmpty(currentStepData.rotationManager))
                {
                    if(value == InteractionTypes.Translate && currentStepData.interactionTypes == InteractionTypes.Rotate)
                    {
                        EditorUtility.DisplayDialog("Warning", "Unable to Switch to Translate", "Ok");
                        interactionType.value = InteractionTypes.Rotate;
                        return;
                    }
                    var option = EditorUtility.DisplayDialog("Warning", "Rotation data will be deleted", "Ok, Delete", "Don't Delete");
                    if (!option)
                    {
                        interactionType.value = InteractionTypes.Rotate;
                        return;
                    }
                    else
                    {

                        RemoveRotationManager();
                    }
                }
                //previousInteraction = currentInteraction;

                //the other implementation solves the issue but creates another one
                #endregion
                currentInteraction = (InteractionTypes)interactionType.value;
                OnInteractionTypeChanged();
            });

            touchInteractionInfo = root.Q<VisualElement>("Touch");
            movementInteractionInfo = root.Q<VisualElement>("Translate");
            rotateInteractionInfo = root.Q<VisualElement>("Rotate");
            partCollisionInfo = root.Q<VisualElement>("PartCollision");

            #region Touch Interactions

            GenerateTouchInteractionList();

            #endregion Touch Interactions


            #region Translate Interaction

            var translateView = root.Q<VisualElement>("Translate");

            var translatePartField = translateView.Q<TextField>("MovementPart");
            translatePartField.RegisterCallback<MouseDownEvent>(evt => { Selection.activeObject = FindPartWithGUID(currentStepData.movementPart); });

            var addTranslatePart = translateView.Q<Button>("AddMovementPart");
            addTranslatePart.clicked += AddTranslatePart;

            var removeTranslatePart = translateView.Q<Button>("RemoveMovementPart");
            removeTranslatePart.clicked += RemoveTranslatePart;

            var pivotField = translateView.Q<TextField>("PivotHolder");
            pivotField.RegisterCallback<MouseDownEvent>(evt => { Selection.activeObject = FindPartWithGUID(currentStepData.movementPart).transform.parent; });

            var addTranslatePivot = translateView.Q<Button>("AddTranslatePivot");
            addTranslatePivot.clicked += AddTranslatePivot;

            var removeTranslatePivot = translateView.Q<Button>("RemoveTranslatePivot");
            removeTranslatePivot.clicked += RemoveTranslatePivot;

            translateView.Q<Button>("Select").clicked += () =>
            {

                var fp = FindPartWithGUID(currentStepData.movementPart).GetComponentInChildren<HighlightSphere>();
                currentFeedbackPointer = fp.gameObject;
                Selection.activeObject = fp.gameObject;
            };

            translateView.Q<Button>("Move").clicked += () =>
            {
                var fp = FindPartWithGUID(currentStepData.movementPart).GetComponentInChildren<HighlightSphere>();
                currentFeedbackPointer = fp.gameObject;
                Selection.activeObject = fp.gameObject;
                fp.isMoving = true;
            };

            var translateDirection = translateView.Q<EnumField>("MovementConstraint");
            translateDirection.RegisterCallback<ChangeEvent<string>>(e =>
            {
                var direction = (AxisFlags)Enum.Parse(typeof(AxisFlags), translateDirection.value.ToString());
                var mp = FindPartWithGUID(currentStepData.movementPart);
                if (mp != null)
                {
                    var tp = mp.GetComponent<MoveConstraint>();
                    if (tp != null)
                        tp.SetDirection(direction);
                }
            });

            translateDistanceInput = new LengthInputElement(translateView.Q<VisualElement>("MovementDistanceInputHolder"));

            translateDistanceInput.OnValueChanged += SetDistanceValue;

            #endregion Translate Interaction


            #region Rotation Interaction

            var rotationView = root.Q<VisualElement>("Rotate");
            rotationView.style.color = Color.white;

            var keepVisibleToggle = rotateInteractionInfo.Q<Toggle>("KeepActivatedToggle");

            keepVisibleToggle.RegisterValueChangedCallback(evt =>
            {
                var manager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
                manager.GetComponent<RotationInteraction>().KeepActivated = evt.newValue;
            });

            var addRotationPart = rotateInteractionInfo.Q<Button>("AddRotPart");
            addRotationPart.clicked += () =>
            {
                AddRotationPart();
            };
            var removeRotationPart = rotateInteractionInfo.Q<Button>("RemRotPart");
            removeRotationPart.clicked += () =>
            {
                RemoveRotationPart();
            };
            var rotationPartList = rotateInteractionInfo.Q<ListView>("PartsRotation");
            var grabPartList = rotateInteractionInfo.Q<ListView>("GrabPartsList");

            UpdateRotationPartList(rotationPartList);
            UpdateGrabPartList(grabPartList);
            
            var addGrabPart = rotationView.Q<Button>("AddGrabPart");
            addGrabPart.clicked += AddGrabPart;

            var removeGrabPart = rotationView.Q<Button>("RemoveGrabPart");
            removeGrabPart.clicked += RemoveGrabPart;

            rotationView.Q<Button>("Select").clicked += () =>
            {
                var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
                var fp = currentManager.GetComponentInChildren<HighlightSphere>();

                currentFeedbackPointer = fp.gameObject;
                Selection.activeObject = fp.gameObject;
            };

            rotationView.Q<Button>("Move").clicked += () =>
            {
                var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
                var fp = currentManager.GetComponentInChildren<HighlightSphere>(); 
                currentFeedbackPointer = fp.gameObject;
                Selection.activeObject = fp.gameObject;
                fp.isMoving = true;
            };
            #endregion Rotation Interaction


            #region Part Collision Interaction

            var partList = partCollisionInfo.Q<ListView>("Parts");

            var addCollisionPart = partCollisionInfo.Q<Button>("AddCollisionPart");
            addCollisionPart.clicked += () =>
            {
                OnCollisionPartAdded();
            };

            var removeCollisionPart = partCollisionInfo.Q<Button>("RemoveCollisionPart");
            removeCollisionPart.clicked += () =>
            {

                OnCollisionPartRemoved();
            };


            UpdateCollisionPartList(partList);


            //Snap Position Label Part Selection
            var snapPositionLabel = partCollisionInfo.Q<Label>("SnapPositionLabel");
            snapPositionLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                Selection.activeObject = PartCollision.GetSnapTransform(currentStepData.collisionManager).gameObject;
            });

            //Snap Position Transform tool part selection
            var snapPositionButton = partCollisionInfo.Q<Label>("AdjustSnapPosition");
            snapPositionLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                Selection.activeObject = PartCollision.GetSnapTransform(currentStepData.collisionManager).gameObject;
                Tools.current = Tool.Move;
            });


            //Snap Volume Label Selection

            var snapVolumeLabel = partCollisionInfo.Q<Label>("SnapVolumeLabel");
            snapVolumeLabel.RegisterCallback<MouseDownEvent>(evt =>
            {
                var snapVol = PartCollision.GetSnapTransform(currentStepData.collisionManager)
                    .GetChild(0)
                    .gameObject;

                if (snapVol == null) return;

                Selection.activeObject = snapVol;

            });

            var snapAdjust = partCollisionInfo.Q<Button>("AdjustSnapPosition");
            snapAdjust.clicked += () =>
            {
                var snapVol = PartCollision.GetSnapTransform(currentStepData.collisionManager)
                    .GetChild(0)
                    .gameObject;

                if (snapVol == null) return;

                Selection.activeObject = snapVol;

            };

            //Snap Volume Dropdown type selection

            var snapVolumeType = partCollisionInfo.Q<DropdownField>("SnapVolumeType");


            snapVolumeType.RegisterCallback<ChangeEvent<string>>(evt =>
            {
                var snapVol = PartCollision.GetSnapTransform(currentStepData.collisionManager)
                    .GetChild(0)
                    .gameObject;

                if (snapVol != null)
                {
                    if (snapVol.name == evt.newValue) return;
                    DestroyImmediate(snapVol);
                }

                switch (snapVolumeType.value)
                {
                    case "Cube":
                        snapVol = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        break;
                    case "Cylinder":
                        snapVol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        break;
                    case "Sphere":
                        snapVol = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        break;
                    default:
                        break;
                }

                var primitiveCol = snapVol.GetComponent<Collider>();
                DestroyImmediate(primitiveCol);
                snapVol.AddComponent<MeshCollider>();
                snapVol.transform.parent = PartCollision.GetSnapTransform(currentStepData.collisionManager);

                snapVol.transform.localPosition = Vector3.zero;
                snapVol.transform.localRotation = Quaternion.identity;
                var snapVolMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/InnovateLabs/Material/DXS_Rim_Position.mat");
                snapVol.GetComponent<MeshRenderer>().material = snapVolMat;
            });

            //Snap volume fit volume

            var fitSnapVolume = partCollisionInfo.Q<Button>("FitSnapVolume");
            fitSnapVolume.clicked += () =>
            {
                PartCollision.FitSnapVolume(currentStepData.collisionManager);
            };

            //Feedback pointer

            var selectFP = partCollisionInfo.Q<Button>("Select");
            selectFP.clicked += () =>
            {
                var currentCollisionManager = PartCollision.FindManagerWithGUID(currentStepData.collisionManager);
                currentFeedbackPointer = currentCollisionManager.GetComponentInChildren<HighlightSphere>(true).gameObject;
                Selection.activeObject = currentFeedbackPointer;
            };

            var moveFP = partCollisionInfo.Q<Button>("Move");
            moveFP.clicked += () =>
            {
                var currentCollisionManager = PartCollision.FindManagerWithGUID(currentStepData.collisionManager);
                currentFeedbackPointer = currentCollisionManager.GetComponentInChildren<HighlightSphere>(true).gameObject;
                Selection.activeObject = currentFeedbackPointer;
                currentFeedbackPointer.GetComponent<HighlightSphere>().isMoving = true;
            };

            #endregion Part Collision Interaction

            #endregion Step Details


            GenerateStepList();


            #region Update Window

            OnInteractionTypeChanged();

            ContextMenuRefresh(root);

            #endregion Upadte Window
        }



        #region Interactable List

        public static List<GameObject> interactables;
        public static InteractableParts interactableParts;
        public ListView interactableListView;

        public void LoadInteractables()
        {
            interactableParts = (InteractableParts)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/Interactables/InteractableList.asset", typeof(InteractableParts));

            if (interactableParts == null)
            {
                interactableParts = CreateInstance<InteractableParts>();

                AssetDatabase.CreateAsset(interactableParts, $"Assets/InnovateLabs/ProjectSetup/Data/Interactables/InteractableList.asset");

            }

            var allGUID = FindObjectsOfType<GUID>(true);
            var loadedModel = new List<GameObject>();
            var loadedModelGUID = new List<string>();


            for (int i = 0; i < allGUID.Length; i++)
            {
                var meshCollider = allGUID[i].GetComponent<MeshCollider>();
                var statefulInteractable = allGUID[i].GetComponent<StatefulInteractable>();
                if (meshCollider != null && statefulInteractable != null)
                {
                    var model = allGUID[i].gameObject;
                    loadedModel.Add(model);
                    loadedModelGUID.Add(allGUID[i].GetGUID());

                }

            }

            interactables = loadedModel;

            if (interactables.Count < interactableParts.Count())
            {
                interactableParts.RemoveAllInteractables(loadedModelGUID);
            }

            GenerateInteractabelList();
        }
        void GenerateInteractabelList()
        {
            var interactionVisualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/InteractiveParts.uxml");

            try
            {
                interactableListView = root.Q<ListView>("InteractiveParts");
                interactableListView.makeItem = interactionVisualAsset.Instantiate;
                interactableListView.bindItem = (e, i) =>
                {
                    if (e != null)
                    {
                        e.Q<ObjectField>().value = interactables[i];
                        e.Q<ObjectField>().label = $"Interactive Part : {i + 1}";
                    }
                };

                interactableListView.onSelectionChange += OnInteractableSelectionChanged;
                interactableListView.itemsSource = interactables;
                interactableListView.headerTitle = "Interactable Parts";
                interactableListView.Q<TextField>("unity-list-view__size-field").SetEnabled(false);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            FindObjectOfType<StepManager>().interactables = interactables;
        }

        [MenuItem("Innovate Labs/ Utilities/ Update Interactables")]
        public static void UpdateInteractables()
        {
            interactableParts = (InteractableParts)AssetDatabase.LoadAssetAtPath("Assets/InnovateLabs/ProjectSetup/Data/Interactables/InteractableList.asset", typeof(InteractableParts));

            if (interactableParts == null)
            {
                interactableParts = CreateInstance<InteractableParts>();

                AssetDatabase.CreateAsset(interactableParts, $"Assets/InnovateLabs/ProjectSetup/Data/Interactables/InteractableList.asset");

            }

            var allGUID = FindObjectsOfType<GUID>(true).ToList();
            var loadedModel = new List<GameObject>();
            var loadedModelGUID = new List<string>();

            foreach (var guid in allGUID)
            {
                var meshCollider = guid.GetComponent<MeshCollider>();
                var statefulInteractable = guid.GetComponent<StatefulInteractable>();
                if (meshCollider != null && statefulInteractable != null)
                {
                    var model = guid.gameObject;
                    loadedModel.Add(model);

                }
            }

        }

        void AddSelectedInteractable()
        {
            var selection = Selection.transforms;
            var errorSelection = new List<string>();
            for (int i = 0; i < selection.Length; i++)
            {
                var current = selection[i].gameObject;
                var mesh = current.GetComponent<MeshFilter>();
                if (mesh != null && current.GetComponent<MeshCollider>() == null)
                {
                    current.AddComponent<MeshCollider>();
                    current.AddComponent<StatefulInteractable>();
                    var guid = current.AddComponent<GUID>();
                    interactableParts.AddInteractable(guid.GetGUID());
                    interactables.Add(current);
                }
                else
                {
                    errorSelection.Add(current.name);
                }
            }
            if (errorSelection.Count != 0)
            {
                EditorUtility.DisplayDialog("Error", $"Unable to create interaction with \n" +
                    $" {ListToText.Convert(errorSelection)} ", "OK");
            }

            interactableListView.RefreshItems();
            interactableListView.Rebuild();

            FindObjectOfType<StepManager>().interactables = interactables;
        }
        void RemoveSelectedInteractable()
        {
            var stepManager = FindObjectOfType<StepManager>();
            var notDeleted = new List<string>();
            for (int i = 0; i < selections.Count; i++)
            {
                var guid = selections[i].GetComponent<GUID>().GetGUID();
                bool donotdelete = false;
                if (stepManager != null)
                {
                    foreach (var step in stepManager.steps)
                    {
                        if (step.ContainsTouchInteraction(selections[i]))
                        {
                            notDeleted.Add(selections[i].name);
                            donotdelete = true;
                            continue;
                        }
                        if (step.movementPart == guid)
                        {
                            notDeleted.Add(selections[i].name);
                            donotdelete = true;
                            continue;
                        }
                        if (step.rotationManager == guid)
                        {
                            notDeleted.Add(selections[i].name);
                            donotdelete = true;
                            continue;
                        }
                    }
                }
                if (donotdelete) continue;
                DestroyImmediate(selections[i].GetComponent<MeshCollider>());
                DestroyImmediate(selections[i].GetComponent<StatefulInteractable>());
                interactableParts.RemoveInteractable(selections[i].GetComponent<GUID>().GetGUID());
                DestroyImmediate(selections[i].GetComponent<GUID>());
                interactables.Remove(selections[i]);
            }
            interactableListView.Rebuild();
            interactableListView.RefreshItems();

            FindObjectOfType<StepManager>().interactables = interactables;

            if (notDeleted.Count != 0)
            {
                EditorUtility.DisplayDialog("Error", $"Unable to delete interactions \n" +
                    $" {ListToText.Convert(notDeleted)} ", "OK");
            }

        }
        void RemoveAllInteractable()
        {
            var stepManager = FindObjectOfType<StepManager>();
            var notDeleted = new List<GameObject>();
            foreach (var interaction in interactables)
            {
                var guid = interaction.GetComponent<GUID>().GetGUID();
                bool donotdelete = false;
                if (stepManager != null)
                {
                    foreach (var step in stepManager.steps)
                    {
                        if (step.ContainsTouchInteraction(interaction))
                        {
                            notDeleted.Add(interaction);
                            donotdelete = true;
                            continue;
                        }
                        if (step.movementPart == guid)
                        {
                            notDeleted.Add(interaction);
                            donotdelete = true;
                            continue;
                        }
                        if (step.rotationManager == guid)
                        {
                            notDeleted.Add(interaction);
                            donotdelete = true;
                            continue;
                        }
                    }
                }
                if (donotdelete) continue;
                DestroyImmediate(interaction.GetComponent<MeshCollider>());
                DestroyImmediate(interaction.GetComponent<StatefulInteractable>());
                interactableParts.RemoveInteractable(interaction.GetComponent<GUID>().GetGUID());
                DestroyImmediate(interaction.GetComponent<GUID>());

            }
            interactables.RemoveAll(x => !notDeleted.Contains(x));
            //interactables = notDeleted;
            interactableListView.Rebuild();
            interactableListView.RefreshItems();

            FindObjectOfType<StepManager>().interactables = interactables;
            var notDeletedNames = new List<string>();
            foreach (var name in notDeleted)
            {
                notDeletedNames.Add(name.name);
            }

            if (notDeleted.Count != 0)
            {
                EditorUtility.DisplayDialog("Error", $"Unable to delete interactions \n" +
                    $" {ListToText.Convert(notDeletedNames)} ", "OK");
            }

        }
        private void OnInteractableSelectionChanged(IEnumerable<object> list)
        {
            List<GameObject> selection = new List<GameObject>();

            var listarray = list.ToArray();

            for (int i = 0; i < listarray.Length; i++)
            {
                var go = (GameObject)listarray[i];
                selection.Add(go);
            }

            Selection.objects = selection.ToArray();

            selections.Clear();
            selections = selection;

        }

        #endregion Interactable List



        #region Step List

        ListView stepListView;
        void OnAddStepClicked()
        {

            ReloadErgonomicWindow();
            StepData newStep = CreateInstance<StepData>();
            newStep.stepNo = steps.Count + 1;

            AssetDatabase.CreateAsset(newStep, $"Assets/InnovateLabs/ProjectSetup/Data/ErgonomicInteractionData/{newStep.ID}.asset");

            steps.Add(newStep);

            stepListView.Rebuild();
            stepListView.RefreshItems();
            RefreshItem();
            currentStepData = steps.Last();

            newStep.movementDirection = AxisFlags.XAxis;


            FindObjectOfType<StepManager>().steps = steps;
        }
        
        void OnDeleteStepClicked()
        {
            #region Delete Interactions
            //touch
            #region Experimental
            /*if (!string.IsNullOrEmpty(steps[currentStepData.stepNo - 1].rotationManager) && !string.IsNullOrEmpty(steps[currentStepData.stepNo - 2].rotationManager))
            {
                //Debug.Log("et voila!");
                rotationCounter = 1;
            }
            else if (!string.IsNullOrEmpty(steps[currentStepData.stepNo - 1].collisionManager) && !string.IsNullOrEmpty(steps[currentStepData.stepNo - 2].collisionManager))
            {
                collisionCounter = 1;
            }
            else
            {
                rotationCounter = 0;
                collisionCounter = 0;
            }*/
            #endregion


            while (currentStepData.touchInteractions.Count != 0)
            {
                var touchPart = FindPartWithGUID(currentStepData.touchInteractions[currentStepData.touchInteractions.Count - 1]);

                currentStepData.RemoveTouchInteraction(touchPart);
            }

            //translate
            if (!string.IsNullOrEmpty(currentStepData.movementPart))
            {
                if (!string.IsNullOrEmpty(currentStepData.movementPivot))
                {
                    RemoveTranslatePivot();
                }
                RemoveTranslatePart();

                currentTranslationPart = null;
            }
            //rotate
            if (!string.IsNullOrEmpty(currentStepData.rotationManager))
            {
                EditorUtility.DisplayDialog("Error", "Unable to delete", "Ok");
                return;
                //RemoveRotationManager();
            }
            //collision parts
            if (!string.IsNullOrEmpty(currentStepData.collisionManager))
            {

                EditorUtility.DisplayDialog("Error", "Unable to delete", "Ok");
                return;
                //RemoveCollisionChecker();

            }
            #endregion Delete Interactions


            string path = AssetDatabase.GetAssetPath(currentStepData);
            AssetDatabase.DeleteAsset(path);

            steps.Remove(currentStepData);

            stepListView.Rebuild();
            touchListView.Rebuild();

            RefreshItem();

            ReloadErgonomicWindow();


            currentFeedbackPointer = null;

            stepDetail.style.visibility = Visibility.Hidden;
            stepDetail.visible = false;

            touchInteractionInfo.style.display = DisplayStyle.None;
            movementInteractionInfo.style.display = DisplayStyle.None;
            rotateInteractionInfo.style.display = DisplayStyle.None;
            partCollisionInfo.style.display = DisplayStyle.None;

            if (steps.Count != 0)
            {
                currentStepData = steps.Last();
                GenerateTouchInteractionList();

            }
            else
            {
                currentStepData = null;
            }

            FindObjectOfType<StepManager>().steps = steps;
            EditorSceneManager.MarkAllScenesDirty();
        }

        private void ReloadErgonomicWindow()
        {
            GetWindow<ErgonomicEvaluation>("Ergonomic Evaluation", true, typeof(ErgonomicEvaluation)).Close();
            GetWindow<ErgonomicEvaluation>("Ergonomic Evaluation", true, typeof(ImportModel)).Show();
            /* currentStepData = steps.Last();*/
            /*stepListView.Rebuild();
            stepListView.RefreshItems();*/
            //RefreshItem();

        }
        void LoadAllSteps()
        {
            steps.Clear();

            var loadingStep = new List<StepData>();

            string[] allPaths = Directory.GetFiles("Assets/InnovateLabs/ProjectSetup/Data/ErgonomicInteractionData", "*.asset", SearchOption.AllDirectories);

            foreach (string path in allPaths)
            {
                string cleanedPath = path.Replace("\\", "/");
                loadingStep.Add((StepData)AssetDatabase.LoadAssetAtPath(cleanedPath, typeof(StepData)));
            }

            steps = loadingStep.OrderBy(l => l.stepNo).ToList();

            FindObjectOfType<StepManager>().steps = steps;
        }

        void GenerateStepList()
        {
            var stepListVisualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/ErgonomicSteps.uxml");

            stepListView = root.Q<ListView>("StepList");

            stepListView.makeItem = stepListVisualAsset.CloneTree;
            stepListView.bindItem = (e, i) =>
            {
                if (e != null)
                {
                    steps[i].stepNo = i + 1;
                    e.Q<Label>().text = $"Step No {steps[i].stepNo}";
                }
            };

            stepListView.onSelectionChange += OnStepSelected;
            stepListView.itemsSource = steps;
            stepListView.Rebuild();
            stepListView.RefreshItems();

            if (steps.Count != 0)
            {
                stepListView.SetSelection(0);
                currentStepData = steps[0];
            }
            else
            {
                currentStepData = new StepData();
            }
            FindObjectOfType<StepManager>().steps = steps;
        }
        private void OnStepSelected(IEnumerable<object> list)
        {

            currentStepData = (StepData)list.Last();

            if (currentStepData == null) return;

            try
            {
                SerializedObject so = new SerializedObject(currentStepData);
                stepDetail.Bind(so);
            }
            catch (ArgumentException e)
            {
                Debug.Log($"Missing Data : {e.Message}");
            }

            /*SerializedObject so = new SerializedObject(currentStepData);
            stepDetail.Bind(so);*/

            stepDetail.style.visibility = Visibility.Visible;
            stepDetail.style.display = DisplayStyle.Flex;
            stepDetail.visible = true;

            if (currentStepData != null)
                currentInteraction = currentStepData.interactionTypes;


            GenerateTouchInteractionList();

            OnInteractionTypeChanged();

            if (currentTranslationPart != null)
            {
                currentTranslationPart.GetComponent<MoveConstraint>().showEndPosition = false;
                currentCollisionParts = null;
            }

            PartCollision.ToggleOffVisaulization();
            RotationUtil.ToggleOffAllFbPointers();

            if (currentStepData.interactionTypes == InteractionTypes.Translate && !string.IsNullOrEmpty(currentStepData.movementPart))
            {
                currentTranslationPart = FindPartWithGUID(currentStepData.movementPart);
                if (currentTranslationPart.GetComponent<MoveConstraint>().showEndPosition == false)
                {
                    currentTranslationPart.GetComponent<MoveConstraint>().showEndPosition = true;
                }
                currentRotationParts = null;
                currentGrabParts = null;
                currentCollisionParts = null;
            }
            else if (currentStepData.interactionTypes == InteractionTypes.Rotate && !string.IsNullOrEmpty(currentStepData.rotationManager))
            {
                Debug.Log("Rotation Interaciton");

                #region debugging zone
                /* if (currentRotationParts != null)
                     currentRotationParts.Clear();*/

                /*if (RotationUtil.GetRotationParts(currentStepData.rotationManager).Count != 0)
                {
                    foreach (var part in RotationUtil.GetRotationParts(currentStepData.rotationManager))
                    {
                        Debug.Log($"Rotation Parts : {part}");
                    }
                }
                else
                {
                    Debug.Log("rotation parts not found!");
                }*/
                #endregion
                var currRotManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
                Selection.activeObject = currRotManager as GameObject;
                RotationUtil.ToggleFbPointers(true, currentStepData.rotationManager);
                currentRotationParts = RotationUtil.GetRotationParts(currentStepData.rotationManager);
                UpdateRotationPartList(rotateInteractionInfo.Q<ListView>("PartsRotation"));
                UpdateGrabPartList(rotateInteractionInfo.Q<ListView>("GrabPartsList"));
                currentTranslationPart = null;
                currentCollisionParts = null;
                rotateInteractionInfo.Q<Toggle>("KeepActivatedToggle").value = currRotManager.GetComponent<RotationInteraction>().KeepActivated;
            }
            else if (currentStepData.interactionTypes == InteractionTypes.PartCollision && !string.IsNullOrEmpty(currentStepData.collisionManager))
            {
                PartCollision.ToggleVisaulization(currentStepData, true);
                if (currentCollisionParts != null)
                    currentCollisionParts.Clear();
                currentCollisionParts = PartCollision.GetCollisionParts(currentStepData.collisionManager);
                UpdateSnapVolume();
                UpdateCollisionPartList(partCollisionInfo.Q<ListView>("Parts"));
            }
            else
            {
                currentRotationParts = null;
                currentGrabParts = null;
                currentTranslationPart = null;
                currentCollisionParts = null;
            }
        }

        #endregion Step List

        private void OnInteractionTypeChanged()
        {
            #region old code
            /*switch (currentInteraction)
            {
                case InteractionTypes.Touch:

                    touchInteractionInfo.style.display = DisplayStyle.Flex;
                    movementInteractionInfo.style.display = DisplayStyle.None;
                    rotateInteractionInfo.style.display = DisplayStyle.None;
                    partCollisionInfo.style.display = DisplayStyle.None;
                    break;
                case InteractionTypes.Translate:
                    touchInteractionInfo.style.display = DisplayStyle.None;
                    movementInteractionInfo.style.display = DisplayStyle.Flex;
                    rotateInteractionInfo.style.display = DisplayStyle.None;
                    partCollisionInfo.style.display = DisplayStyle.None;
                    break;
                case InteractionTypes.Rotate:
                    touchInteractionInfo.style.display = DisplayStyle.None;
                    movementInteractionInfo.style.display = DisplayStyle.None;
                    rotateInteractionInfo.style.display = DisplayStyle.Flex;
                    partCollisionInfo.style.display = DisplayStyle.None;
                    UpdateRotationManager();
                    break;
                case InteractionTypes.PartCollision:
                    touchInteractionInfo.style.display = DisplayStyle.None;
                    movementInteractionInfo.style.display = DisplayStyle.None;
                    rotateInteractionInfo.style.display = DisplayStyle.None;
                    partCollisionInfo.style.display = DisplayStyle.Flex;
                    UpdateCollisionManager();
                    UpdateSnapVolume();
                    break;
                default:
                    break;
            }*/
            #endregion
            UpdateDisplayStyle(currentInteraction);
        }

        /*static int rotationCounter = 0;
        static int collisionCounter = 0;*/
        void UpdateDisplayStyle(InteractionTypes interactionType)
        {

            //Debug.Log($"Rotation coutner valeu : {rotationCounter}");
            touchInteractionInfo.style.display = interactionType == InteractionTypes.Touch ? DisplayStyle.Flex : DisplayStyle.None;
            movementInteractionInfo.style.display = interactionType == InteractionTypes.Translate ? DisplayStyle.Flex : DisplayStyle.None;
            rotateInteractionInfo.style.display = interactionType == InteractionTypes.Rotate ? DisplayStyle.Flex : DisplayStyle.None;
            partCollisionInfo.style.display = interactionType == InteractionTypes.PartCollision ? DisplayStyle.Flex : DisplayStyle.None;
            switch (interactionType)
            {
                case InteractionTypes.Rotate:
                    /*if(rotationCounter <= 0)
                        UpdateRotationManager();*/
                    UpdateRotationManager();
                    break;
                case InteractionTypes.PartCollision:
                    /*if(collisionCounter <= 0)
                    {
                        UpdateCollisionManager();
                        UpdateSnapVolume();
                    }*/
                    UpdateCollisionManager();
                    UpdateSnapVolume(); 
                    break;
                default:
                    break;
            }
        }


        #region Touch Interaction Parts

        private void GenerateTouchInteractionList()
        {
            var touchInteractionVisualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/InnovateLabs/ProjectSetup/Scripts/Editor/TouchPartElements.uxml");

            touchListView = root.Q<ListView>("TouchInteractions");

            var addElement = root.Q<Button>("AddElement");
            addElement.clicked += OnTouchElementAdded;
            var deleteElement = root.Q<Button>("RemoveElement");
            deleteElement.clicked += OnTouchElementRemoved;

            touchListView.makeItem = touchInteractionVisualAsset.CloneTree;
            touchListView.bindItem = (e, i) =>
            {
                if (e != null && currentStepData != null && currentStepData.touchInteractions.Count != 0)
                {
                    e.Q<TextField>().label = $"Touch Part {i + 1}";
                    e.Q<TextField>().value = currentStepData.touchInteractions[i];

                    e.Q<Button>("Select").clicked += () =>
                    {
                        OnFeedbackPointerSelected(i);
                    };
                    e.Q<Button>("Move").clicked += () =>
                    {
                        OnFeedbackPointerMoved(i);
                    };
                }
            };
            if (currentStepData != null)
            {
                touchListView.itemsSource = currentStepData.touchInteractions;
                touchListView.onSelectionChange += OnTouchElementSelected;
            }
        }
        private void OnFeedbackPointerSelected(int index)
        {
            var selectedPart = interactables.Find(x => x.GetComponent<GUID>().GetGUID() == currentStepData.touchInteractions[index]);
            var currentFP = selectedPart.GetComponentInChildren<HighlightSphere>();
            if (currentFP == null)
            {
                currentFP = currentStepData.InstantiateFeedbackPointer(selectedPart).GetComponent<HighlightSphere>();
            }
            currentFeedbackPointer = currentFP.gameObject;
            Selection.activeObject = currentFP.gameObject;
        }
        private void OnFeedbackPointerMoved(int index)
        {
            var selectedPart = interactables.Find(x => x.GetComponent<GUID>().GetGUID() == currentStepData.touchInteractions[index]);
            var currentFP = selectedPart.GetComponentInChildren<HighlightSphere>();

            currentFeedbackPointer = currentFP.gameObject;
            Selection.activeObject = currentFP;


            currentFP.isMoving = true;

        }
        private void OnTouchElementAdded()
        {
            var selection = Selection.objects;
            var errorSelection = new List<string>();

            var selectedGameObject = new List<GameObject>();

            foreach (var selected in selection)
            {
                if (selected.GetType() == typeof(GameObject))
                {
                    selectedGameObject.Add((GameObject)selected);
                }
            }

            for (int i = 0; i < selectedGameObject.Count; i++)
            {
                var interactable = selectedGameObject[i];

                if (interactable.GetComponent<StatefulInteractable>() != null && interactable.GetComponent<MeshCollider>() != null)
                {
                    if (!currentStepData.ContainsTouchInteraction(interactable)  && interactables.Contains(interactable))
                    {
                        currentStepData.AddTouchInteraction(interactable);
                    }
                    else
                        errorSelection.Add(interactable.name);
                }
                else
                {
                    errorSelection.Add(interactable.name);
                }
            }

            if (errorSelection.Count != 0)
            {
                EditorUtility.DisplayDialog("Error", $"Unable to add interaction(s) is this\n" +
                    $" {ListToText.Convert(errorSelection)} ", "OK");
            }

            //selections.Clear();

            //touchListView.Rebuild();
            //touchListView.RefreshItems();
            touchListView.SetSelection(interactables.Count - 1);
        }
        private void OnTouchElementRemoved()
        {
            if (selections.Count != 0)
            {
                for (int i = 0; i < selections.Count; i++)
                {
                    currentStepData.RemoveTouchInteraction(selections[i]);
                }
            }

            selections.Clear();
            touchListView.Rebuild();
            touchListView.RefreshItems();
        }
        private void OnTouchElementSelected(IEnumerable<object> list)
        {
            if (list.Count() == 0) return;

            List<GameObject> selection = new List<GameObject>();

            var selectedGUID = new List<string>();

            foreach (var id in list)
            {
                if (id != null)
                {
                    if (id.GetType() == typeof(string))
                    {
                        selectedGUID.Add((string)id);
                    }
                }
            }

            for (int i = 0; i < selectedGUID.Count; i++)
            {
                var go = interactables.Find(x => x.GetComponent<GUID>().GetGUID() == selectedGUID[i]);
                selection.Add(go);
            }

            Selection.objects = selection.ToArray();

            selections.Clear();
            selections = selection;

        }


        #endregion

        #region Translation Interaction

        GameObject currentTranslationPart;
        LengthInputElement translateDistanceInput;
        public void AddTranslatePart()
        {
            var currentSelection = (GameObject)Selection.activeObject;

            if (interactables.Contains(currentSelection))
            {
                
                if (currentSelection.gameObject.HasComponent<ObjectManipulator>() || currentSelection.gameObject.HasComponent<MoveAxisConstraint>() || currentSelection.gameObject.HasComponent<MoveConstraint>())
                {
                    EditorUtility.DisplayDialog("Part Already added!", $"'{currentSelection.name}' has already been added as a translation part.", "Ok");
                    return;
                }
                else
                {
                    currentStepData.movementPart = currentSelection.GetComponent<GUID>().GetGUID();
                    var objectManipulator = currentSelection.AddComponent<ObjectManipulator>();
                    var moveAxisConstraint = currentSelection.AddComponent<MoveAxisConstraint>();
                    var movementConstraint = currentSelection.AddComponent<MoveConstraint>();
                    objectManipulator.AllowedManipulations = TransformFlags.Move;
                    objectManipulator.AllowedInteractionTypes = InteractionFlags.Near;
                    moveAxisConstraint.UseLocalSpaceForConstraint = true;
                    movementConstraint.SetDirection(AxisFlags.XAxis);
                }

                var fp = currentSelection.GetComponentInChildren<HighlightSphere>();

                if (fp == null)
                {
                    var instFp = currentStepData.InstantiateFeedbackPointer(currentSelection);
                    fp = instFp.GetComponent<HighlightSphere>();
                }
                currentFeedbackPointer = fp.gameObject;
                currentTranslationPart = currentSelection;

            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Unable to add :'{currentSelection.name}' as translation part." +
                    $"\nTry selecting part from interactable parts", "Ok");
            }
        }

        public void RemoveTranslatePart()
        {
            if (currentTranslationPart == null)
            {
                currentTranslationPart = FindPartWithGUID(currentStepData.movementPart);
            }

            DestroyImmediate(currentTranslationPart.GetComponent<MoveConstraint>());
            DestroyImmediate(currentTranslationPart.GetComponent<MoveAxisConstraint>());
            DestroyImmediate(currentTranslationPart.GetComponent<ObjectManipulator>());
            DestroyImmediate(currentTranslationPart.GetComponent<ConstraintManager>());
            DestroyImmediate(currentTranslationPart.GetComponentInChildren<HighlightSphere>().gameObject);

            currentStepData.movementPart = "";
            currentTranslationPart = null;
        }

        public void AddTranslatePivot()
        {
            var selectedPivot = Selection.activeGameObject;
            var components = selectedPivot.GetComponents<Component>();

            if (currentTranslationPart == null)
            {
                currentTranslationPart = FindPartWithGUID(currentStepData.movementPart);
            }

            if (!(components.Length > 1) && selectedPivot.transform.childCount == 0)
            {
                selectedPivot.AddComponent<GUID>();

                var siblingIndex = currentTranslationPart.transform.GetSiblingIndex();
                selectedPivot.transform.parent = currentTranslationPart.transform.parent;
                selectedPivot.transform.SetSiblingIndex(siblingIndex);
                currentTranslationPart.transform.parent = selectedPivot.transform;
                var moveConstraint = currentTranslationPart.GetComponent<MoveConstraint>();
                moveConstraint.SetPivot(selectedPivot.transform);
                currentStepData.AddTranslatePivot(selectedPivot);

                SetDistanceValue();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", $"Unable to set '{selectedPivot.name}' as pivot of '{currentTranslationPart.name}'","Ok");
            }
        }

        public void RemoveTranslatePivot()
        {
            if (currentTranslationPart == null)
            {
                currentTranslationPart = FindPartWithGUID(currentStepData.movementPart);
            }

            if (!string.IsNullOrEmpty(currentStepData.movementPivot) && !string.IsNullOrEmpty(currentStepData.movementPart))
            {
                var pivotGUID = FindPartWithGUID(currentStepData.movementPart).transform.parent.GetComponentInParent<GUID>();
                if (pivotGUID != null && pivotGUID.GetGUID() == currentStepData.movementPivot)
                {
                    var siblingIndex = currentTranslationPart.transform.GetSiblingIndex();
                    currentTranslationPart.transform.parent = pivotGUID.transform.parent;
                    currentTranslationPart.transform.SetSiblingIndex(siblingIndex);
                    pivotGUID.transform.parent = null;
                    pivotGUID.transform.SetAsLastSibling();
                    DestroyImmediate(pivotGUID);
                    var moveConstraint = currentTranslationPart.GetComponent<MoveConstraint>();
                    moveConstraint.SetPivot(currentTranslationPart.transform);
                    currentStepData.RemoveTranslatePivot();

                    SetDistanceValue();
                }
            }
        }

        public void SetDistanceValue()
        {
            var value = translateDistanceInput.value;
            var tp = FindPartWithGUID(currentStepData.movementPart);

            if (tp == null && currentTranslationPart == null) return;

            currentTranslationPart = tp;
            if (currentTranslationPart != null)
            {
                /*var modelInfo = currentTranslationPart.GetComponentInParent<ModelInfo>();
                var modelUnit = modelInfo.GetCurrentUnit();*/
                float valueInLocal = translateDistanceInput.ValueInMeter();


                var moveConstraint = currentTranslationPart.GetComponent<MoveConstraint>();

                var origin = currentTranslationPart.transform;

                if (!string.IsNullOrEmpty(currentStepData.movementPivot))
                {
                    origin = origin.parent;
                }

                moveConstraint.SetFinalPosition(currentStepData.TranslatedPosition(origin));
                moveConstraint.inputDistanceValue = value;


                currentStepData.distanceToLocal = valueInLocal;
            }
        }


        #endregion Translation Interaction

        #region Rotation Interaction

        //GameObject currentRotationPart;
        LengthInputElement rotationAngleInput;
        private List<GameObject> currentRotationParts = new List<GameObject>();
        private List<GameObject> currentGrabParts = new List<GameObject>();

        RotationInteraction rotationInteraction;
        public void AddRotationPart()
        {
            var selection = Selection.activeObject;
            if (selection.GetType() != typeof(GameObject))
                return;
            var currentSelection = (GameObject)selection;
            var rootGameObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();

            var heirarchyDivider = FindObjectOfType<StepManager>().gameObject;

            if( rootGameObjects.Contains(currentSelection) && currentSelection.transform.GetSiblingIndex() <= heirarchyDivider.transform.GetSiblingIndex())
            {
                EditorUtility.DisplayDialog("Invalid Selection!", $"{currentSelection.name} cannot be added as a rotation part due to internal dependencies", "Ok");
                return;
            }

            var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);

            if(RotationUtil.ContainsRotationPart(currentSelection, currentStepData.rotationManager))
            {
                EditorUtility.DisplayDialog("Part already exists!", "The selected object has already been defined as rotation part", "Ok");
                return;
            }
            RotationUtil.AddRotationPart(currentSelection, currentStepData.rotationManager);
            
            currentRotationParts = RotationUtil.GetRotationParts(currentStepData.rotationManager);

            UpdateRotationPartList(rotateInteractionInfo.Q<ListView>("PartsRotation"));
            rotateInteractionInfo.Q<Button>("AddRotationPart").SetEnabled(true);

            EditorUtility.SetDirty(currentManager);

        }

        public void RemoveRotationPart()
        {
            //Debug.Log("Removed Rotation Part");
            var selection = Selection.activeObject;
            if (selection.GetType() != typeof(GameObject))
                return;
            var currentSelection = (GameObject)selection;

            var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);

            if (!RotationUtil.ContainsRotationPart(currentSelection, currentStepData.rotationManager))
                return;

            RotationUtil.RemoveRotationPart(currentSelection, currentStepData.rotationManager);

            UpdateRotationPartList(rotateInteractionInfo.Q<ListView>("PartsRotation"));
            EditorUtility.SetDirty(currentManager);

            #region SHUTUP

            /*if(currentRotationPart == null)
            {
                currentRotationPart = FindPartWithGUID(currentStepData.grabPart);
            }*/

            /*DestroyImmediate(currentRotationPart.GetComponent<RotateConstraint>());
            DestroyImmediate(currentRotationPart.GetComponent<RotationAxisConstraint>());
            DestroyImmediate(currentRotationPart.GetComponent<ObjectManipulator>());
            DestroyImmediate(currentRotationPart.GetComponent<ConstraintManager>());
            DestroyImmediate(currentRotationPart.GetComponentInChildren<HighlightSphere>().gameObject);

            currentStepData.grabPart = "";
            currentRotationPart = null;*/
            #endregion
        }

        private void RemoveRotationManager()
        {
            //Debug.Log("Removing Rotation Manager");
            RemoveRotation();   
        }

        private async void RemoveRotation()
        {
            await RotationUtil.RemoveManager(currentStepData.rotationManager);
            await Task.Yield();
            currentStepData.rotationManager = null;
            currentRotationParts = null;
            currentGrabParts = null;
        }
        public void AddGrabPart()
        {
            //Debug.Log("Add Grab Part");
            int maxParts = 1;
            var selection = Selection.activeObject;
            if (selection.GetType() != typeof(GameObject))
                return;
            var currentSelection = (GameObject)selection;

            if (!RotationUtil.ContainsRotationPart(currentSelection, currentStepData.rotationManager))
            {
                EditorUtility.DisplayDialog("Not a Rotation Part!", "The Desired Grab Part should be part of the rotation parts list", "Ok");
                return;
            }
            if(RotationUtil.ContainsGrabPart(currentSelection, currentStepData.rotationManager))
            {
                EditorUtility.DisplayDialog("Part already exists!", "The selected grab part has already been defined", "Ok");
                return;
            }
            var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
            RotationUtil.AddGrabPart(maxParts, currentSelection, currentStepData.rotationManager);

            currentGrabParts = RotationUtil.GetGrabParts(currentStepData.rotationManager);
            UpdateGrabPartList(rotateInteractionInfo.Q<ListView>("GrabPartsList"));
            EditorUtility.SetDirty(currentManager);

            //Debug.Log("grab parts count : " + currentGrabParts.Count);
            if (!RotationUtil.ContainsGrabPart(currentSelection, currentStepData.rotationManager))
            {
                //Debug.Log("Yahan atak raha hai bhai ye");
                return;
            }
            //Debug.Log("Ye part toh hai grab part me");
            HighlightSphere fp = null;

            if (fp == null)
            {
                fp = currentManager.GetComponentInChildren<HighlightSphere>(true);
            }
            currentFeedbackPointer = fp.gameObject;

        }

        public void RemoveGrabPart()
        {
            //Debug.Log("Remove Grab Part");

            var selection = Selection.activeObject;
            if (selection.GetType() != typeof(GameObject))
                return;
            var currentSelection = (GameObject)selection;

            if (!RotationUtil.ContainsGrabPart(currentSelection, currentStepData.rotationManager))
            {
                EditorUtility.DisplayDialog("Not a Rotation Part!", "The Desired Grab Part should be part of the rotation parts list", "Ok");
                //Debug.Log("ye part toh hai hi nahi grab part me so no bueno");
                return;
            }
            Debug.Log("Yahan tak pohonch gaya code");
            var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);
            DestroyImmediate(currentFeedbackPointer);
            RotationUtil.RemoveGrabPart(currentSelection, currentStepData.rotationManager);
            UpdateGrabPartList(rotateInteractionInfo.Q<ListView>("GrabPartsList"));

            EditorUtility.SetDirty(currentManager);

            #region SHUTUP
            /*if (currentRotationPart == null)
            {
                currentRotationPart = FindPartWithGUID(currentStepData.grabPart);
            }*/

            /*if(!string.IsNullOrEmpty(currentStepData.rotationPivot) && !string.IsNullOrEmpty(currentStepData.rotationPart))
            {
                var pivotGUID = FindPartWithGUID(currentStepData.rotationPart).transform.parent.GetComponentInParent<GUID>();
                if (pivotGUID != null && pivotGUID.GetGUID() == currentStepData.rotationPivot)
                {
                    var siblingIndex = currentRotationPart.transform.GetSiblingIndex();
                    currentRotationPart.transform.parent = pivotGUID.transform.parent;
                    currentRotationPart.transform.SetSiblingIndex(siblingIndex);
                    pivotGUID.transform.parent = null;
                    pivotGUID.transform.SetAsLastSibling();
                    DestroyImmediate(pivotGUID);
                    var moveConstraint = currentRotationPart.GetComponent<RotateConstraint>();
                    moveConstraint.SetPivot(currentRotationPart.transform);
                    currentStepData.RemoveRotationPivot();

                    //SetAngleValue();
                }
            }*/
            #endregion
        }

        private async void UpdateRotationManager()
        {
            //Debug.Log("Creating rotation manager");
            string currentRotationManager = currentStepData.rotationManager; ;


            if (string.IsNullOrEmpty(currentRotationManager))
            {
                currentRotationManager = RotationUtil.InstantiateManager(currentStepData.stepNo);
                await Task.Yield();
                currentStepData.rotationManager = currentRotationManager;
                currentRotationParts = null;
                if (currentRotationParts != null)
                {
                    if (currentRotationParts.Count > 0)
                        currentRotationParts.Clear();
                }
            }
            //var currentManager = RotationUtil.FindManagerWithGUID(currentStepData.rotationManager);

            //currentRotationParts = currentManager.GetComponent<RotationInteraction>().rotationParts;
            currentRotationParts = RotationUtil.GetRotationParts(currentStepData.rotationManager);
            currentGrabParts = RotationUtil.GetGrabParts(currentStepData.rotationManager);
            try
            {
                UpdateRotationPartList(rotateInteractionInfo.Q<ListView>("PartsRotation"));
                UpdateGrabPartList(rotateInteractionInfo.Q<ListView>("GrabPartsList"));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
        private void UpdateRotationPartList(ListView rotationPartList)
        {

            if (currentStepData != null)
            {

                if (!string.IsNullOrEmpty(currentStepData.rotationManager))
                {
                    rotationPartList.makeItem = () => new Label();
                    rotationPartList.itemsSource = currentRotationParts;
                    rotationPartList.bindItem = (e, i) =>
                    {
                        if (currentRotationParts == null) 
                        {
                            //Debug.Log("L lag rahe hain bhai");
                            return;
                        }
                        
                        if (i >= currentRotationParts.Count && i < 0) return;
                        try
                        {
                            if (e != null && currentRotationParts[i] != null)
                            {
                                e.Q<Label>().text = $"{i + 1}-{currentRotationParts[i].name}";
                            }
                        }
                        catch(Exception ex)
                        {
                            Debug.Log(ex.Message);
                        }
                        
                    };
                    rotationPartList.selectionType = SelectionType.Multiple;
                    rotationPartList.showBorder = true;
                    rotationPartList.showFoldoutHeader = true;
                    //rotationPartList.Q<TextField>("unity-list-view__size-field").SetEnabled(false);
                    rotationPartList.onSelectionChange += objects =>
                    {
                        var torray = objects.First();
                        Selection.activeObject = (GameObject)torray;
                    };

                }
            }
        }

        private void UpdateGrabPartList(ListView grabPartList)
        {
            if (currentStepData != null)
            {

                if (!string.IsNullOrEmpty(currentStepData.rotationManager))
                {
                    grabPartList.makeItem = () => new Label();
                    grabPartList.itemsSource = currentGrabParts;
                    grabPartList.bindItem = (e, i) =>
                    {
                        if (currentRotationParts == null)
                        {
                            Debug.Log("L lag rahe hain bhai");
                            return;
                        }

                        if (i >= currentGrabParts.Count && i < 0) return;
                        try
                        {
                            if (e != null && currentGrabParts[i] != null)
                            {
                                e.Q<Label>().text = $"{i + 1}-{currentGrabParts[i].name}";
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.Log(ex.Message);
                        }

                    };
                    grabPartList.selectionType = SelectionType.Multiple;
                    grabPartList.showBorder = true;
                    grabPartList.showFoldoutHeader = true;
                    //rotationPartList.Q<TextField>("unity-list-view__size-field").SetEnabled(false);
                    grabPartList.onSelectionChange += objects =>
                    {
                        var torray = objects.First();
                        Selection.activeObject = (GameObject)torray;
                    };

                }
            }
        }

        /*public void SetAngleValue()
        {
            var value = rotationAngleInput.value;
            var tp = FindPartWithGUID(currentStepData.rotationPart);

            if (tp == null && currentRotationPart == null) return;

            currentRotationPart = tp;

            var rotateConstraint = currentRotationPart.GetComponent<RotateConstraint>();

            var origin = currentRotationPart.transform;

            if (!string.IsNullOrEmpty(currentStepData.rotationPivot))
            {
                origin = origin.parent;
            }

            rotateConstraint.SetFinalValue(value);


            currentStepData.rotationAngle = value;
        }*/


        #endregion Rotation Interaction

        #region Part Collision Interaction

        private List<GameObject> currentCollisionParts = new List<GameObject>();

        async void OnCollisionPartAdded()
        {
            var selected = Selection.activeObject;
            if (selected.GetType() != typeof(GameObject)) return;

            var selectedPart = (GameObject)selected;

            var componentsToCheck = new Type[]
                                            {
                                                typeof(StatefulInteractable),
                                                typeof(HighlightSphere),
                                                typeof(PartsCollisionChecker),
                                                typeof(GUID),
                                                typeof(PartCollisionListener),
                                                typeof(PartPosition),
                                                typeof(TapToPlaceManager),
                                                typeof(StepManager),
                                                typeof(TransformManipulator)
                                            };
            if (selectedPart.HasAnyComponents(componentsToCheck) || selectedPart.HasAnyComponentsInParent(componentsToCheck))
            {
                Debug.Log("Unsuitable Part Selected");
                return;
            }

            var childs = selectedPart.GetComponentsInChildren<Transform>(true);
            foreach (var child in childs)
            {
                if (child.gameObject.HasAnyComponents(componentsToCheck) || child.gameObject.HasAnyComponentsInParent(componentsToCheck))
                {
                    Debug.Log("Unsuitable child in selection");
                    return;
                }
            }

            if (selectedPart.transform.parent == PartCollision.GetCollisionChecker(currentStepData.collisionManager).transform.parent) return;

            selectedPart.transform.parent = PartCollision.GetCollisionChecker(currentStepData.collisionManager).transform;

            var convexmeshGenerator = selectedPart.AddComponent<VHACD>();
            convexmeshGenerator.GenerateMeshCollider();

            await Task.Yield();



            currentCollisionParts.Clear();
            currentCollisionParts = PartCollision.GetCollisionParts(currentStepData.collisionManager);
            UpdateCollisionPartList(partCollisionInfo.Q<ListView>("Parts"));

            DestroyImmediate(convexmeshGenerator);


            partCollisionInfo.Q<Button>("AddCollisionPart").SetEnabled(true);
        }

        async void OnCollisionPartRemoved()
        {
            var selected = Selection.activeObject;
            if (selected.GetType() != typeof(GameObject)) return;

            var selectedPart = (GameObject)selected;

            if (selectedPart.transform.parent != PartCollision.GetCollisionChecker(currentStepData.collisionManager).transform) return;
            selectedPart.transform.parent = null;
            selectedPart.transform.SetAsLastSibling();

            var collisionListeners = selectedPart.GetComponentsInChildren<PartCollisionListener>(true);
            foreach (var cl in collisionListeners)
            {
                var col = cl.GetComponent<Collider>();
                if (col)
                {
                    if (col.GetType() != typeof(MeshCollider))
                    {
                        cl.GetComponent<Collider>().isTrigger = false;
                        DestroyImmediate(cl.GetComponent<Rigidbody>());
                        DestroyImmediate(cl);
                        continue;
                    }
                }
                DestroyImmediate(cl.gameObject);
            }

            await Task.Yield();

            currentCollisionParts.Clear();
            currentCollisionParts = PartCollision.GetCollisionParts(currentStepData.collisionManager);
            UpdateCollisionPartList(partCollisionInfo.Q<ListView>("Parts"));

        }
        async void UpdateCollisionManager()
        {
            string currentCollisionManager = currentStepData.collisionManager; ;


            if (string.IsNullOrEmpty(currentCollisionManager))
            {
                currentCollisionManager = PartCollision.InstantiateManager(currentStepData.stepNo);
                await Task.Yield();
                currentStepData.collisionManager = currentCollisionManager;
                currentCollisionParts = null;
                //if (currentCollisionParts != null)
                //{
                //    if (currentCollisionParts.Count > 0)
                //        currentCollisionParts.Clear();
                //}
            }

            if (currentCollisionParts != null)
            {
                if (currentCollisionParts.Count > 0)
                    currentCollisionParts.Clear();
            }
            currentCollisionParts = PartCollision.GetCollisionParts(currentStepData.collisionManager);
            try
            {
                UpdateCollisionPartList(partCollisionInfo.Q<ListView>("Parts"));
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }

        private void UpdateCollisionPartList(ListView partList)
        {
            if (currentStepData != null)
            {

                if (!string.IsNullOrEmpty(currentStepData.collisionManager))
                {
                    partList.makeItem = () => new Label();
                    partList.itemsSource = currentCollisionParts;
                    partList.bindItem = (e, i) =>
                    {
                        if (currentCollisionParts == null) return;
                        if (i >= currentCollisionParts.Count && i < 0) return;
                        try
                        {
                            if (e != null && currentCollisionParts[i] != null)
                            {
                                e.Q<Label>().text = $"{i + 1}-{currentCollisionParts[i].name}";
                            }
                        }
                        catch(Exception ex)
                        {
                            Debug.Log(ex.Message);
                        }
                        
                    };
                    partList.selectionType = SelectionType.Single;
                    partList.showBorder = true;
                    partList.showFoldoutHeader = true;
                    partList.Q<TextField>("unity-list-view__size-field").SetEnabled(false);
                    partList.onSelectionChange += objects =>
                    {
                        var torray = objects.First();
                        Selection.activeObject = (GameObject)torray;
                    };

                }
            }
        }


        private void RemoveCollisionChecker()
        {
            _RemoveCollisionChecker();
        }

        private async void _RemoveCollisionChecker()
        {
            await PartCollision.RemoveStepCollision(currentStepData);
            await Task.Yield();
            currentStepData.collisionManager = null;
            currentCollisionParts = null;
        }

        private async void UpdateSnapVolume()
        {
            var snapVolumeType = partCollisionInfo.Q<DropdownField>("SnapVolumeType");
            GameObject snapVol = null;
            await Task.Yield(); var snapTransform = PartCollision.GetSnapTransform(currentStepData.collisionManager);

            if (snapTransform != null && snapTransform.GetChild(0) != null)
            {
                snapVol = snapTransform
                .GetChild(0)
                .gameObject;
            }


            if (snapVol == null) return;

            switch (snapVol.name)
            {
                case "Cylinder":
                    snapVolumeType.value = "Cylinder";
                    break;
                case "Cube":
                    snapVolumeType.value = "Cube";
                    break;
                case "Sphere":
                    snapVolumeType.value = "Sphere";
                    break;
                default:
                    snapVolumeType.value = "";
                    break;

            }
        }

        #endregion Part Collision Interaction

        private GameObject FindPartWithGUID(string guid)
        {
            GameObject part;

            part = interactables.Find(x => x.GetComponent<GUID>().GetGUID() == guid);

            return part;
        }

        private void ContextMenuRefresh(VisualElement root)
        {

            root.AddManipulator(new ContextualMenuManipulator((ContextualMenuPopulateEvent evt) =>
            {
                evt.menu.AppendAction("Refresh", (e) =>
                {


                    var selected = new List<StepData>();
                    selected.Add(currentStepData);

                    RefreshItem();

                    OnStepSelected(selected);



                });

                #region Menu to load interactables
                /* evt.menu.AppendAction("Reload Interactables", (e) =>
                 {
                     LoadInteractables();
                 });*/
                #endregion

            }));
        }

        private async void RefreshItem()
        {

            int stepIndex = 0;

            try
            {
                if (currentStepData != null)
                    stepIndex = currentStepData.stepNo <= 1 ? 0 : currentStepData.stepNo - 1;
            }
            catch (NullReferenceException e)
            {
                if (currentStepData == null)
                    stepIndex = 0;
                Debug.Log($"Null Reference : {e.Message}");
            }

            stepListView.SetSelection(stepIndex);

            stepListView.RefreshItems();
            interactableListView.RefreshItems();
            touchListView.RefreshItems();
            await Task.Yield();
        }

        /*private async void RefreshItem()
        {

            int stepIndex = 0;

            try
            {
                if (currentStepData != null)
                    stepIndex = currentStepData.stepNo <= 1 ? 0 : currentStepData.stepNo - 1;
            }
            catch (NullReferenceException e)
            {
                if (currentStepData == null)
                    stepIndex = 0;
                Debug.Log($"Null Reference : {e.Message}");
            }

            stepListView.SetSelection(stepIndex);

            stepListView.RefreshItems();
            interactableListView.RefreshItems();
            touchListView.RefreshItems();
            await Task.Yield();
        }*/


        public async void ClearErgonomicData()
        {
            int stepCount = steps.Count;
            for (int i = stepCount - 1; i >= 0; i--)
            {
                currentStepData = steps[i];
                OnDeleteStepClicked();
                await Task.Yield();
            }
            RemoveAllInteractable();
            currentStepData = null;
            currentFeedbackPointer = null;
            //currentRotationPart = null;
            currentTranslationPart = null;
        }


        public void DrawTranslateDistance()
        {
            if (currentTranslationPart != null)
            {
                var origin = currentTranslationPart.transform;
                if (!string.IsNullOrEmpty(currentStepData.movementPivot))
                {
                    origin = origin.parent;
                }

                Handles.color = Color.blue;
                Handles.DrawLine(origin.position, currentStepData.TranslatedPosition(origin), 5);
            }
        }

        /*public void DrawRotationAngle()
        {
            if (currentRotationPart != null)
            {
                var origin = currentRotationPart.transform;
                if (!string.IsNullOrEmpty(currentStepData.rotationPivot))
                {
                    origin = origin.parent;
                }

                var handleColor  = Color.blue;
                Handles.color = new Color(handleColor.r, handleColor.g, handleColor.b, 0.5f);
                var direction = origin.up;
                var normal = origin.right;
                switch(currentStepData.rotationAxis)
                {
                    case AxisFlags.XAxis:

                        break;
                    case AxisFlags.YAxis:
                        direction = origin.right;
                        normal = origin.up;
                        break;
                    case AxisFlags.ZAxis:
                        direction = origin.up;
                        normal = origin.forward;
                        break;
                }
                //Handles.DrawSolidArc(origin.position,normal ,direction ,  currentStepData.rotationAngle, 0.5f);
            }
        }*/

        private bool IsCollisionInteraction()
        {
            return currentStepData.interactionTypes == InteractionTypes.PartCollision;
        }

    }

}
