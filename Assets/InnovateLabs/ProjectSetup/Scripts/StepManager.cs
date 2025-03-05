using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InnovateLabs.Utilities;
using TMPro;
using Microsoft.MixedReality.Toolkit.UX;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit;
using System;
using UnityEngine.SceneManagement;

namespace InnovateLabs.Projects
{
    public class StepManager : MonoBehaviour
    {
        public static StepManager instance;


        #region UI
        [SerializeField] TextMeshProUGUI stepNoField;

        [SerializeField] TextMeshProUGUI descriptionField;

        [SerializeField] PressableButton previousButton;
        [SerializeField] PressableButton nextButton;

        [SerializeField] PressableButton closeButton;
        [SerializeField] PressableButton resetInteraction;
        public ResultGrid resultGrid;

        [Header("Rotation UI")]
        [SerializeField] GameObject rotationPanel;
        [SerializeField] PressableButton ltlButton;
        [SerializeField] PressableButton freeRotFbButton;
        [SerializeField] PressableButton freeRotButton;
        [SerializeField] PressableButton xAxisButton;
        [SerializeField] PressableButton yAxisButton;
        [SerializeField] PressableButton zAxisButton;

        [SerializeField] SliderUI slidersLtl;
        [SerializeField] SliderUI sliderFreeRotFb;

        #endregion

        [ReadValueAtInspector] public List<StepData> steps;
        [ReadValueAtInspector] public List<GameObject> interactables;


        [ReadValueAtInspector]
        [SerializeField] private int currentStep = -1, totalSteps = -1, totalStepsInStep = 0;

        private AudioSource audioSource;
        private bool isResults = false;
        public bool isLastStepReset = false;
        private bool isAllPointerOn = false;

        void Start()
        {
            Init();

            totalSteps = steps.Count;

            rotationPanel.SetActive(false);

            ToggleAllFeedbackPointer(false);

            closeButton.OnClicked.AddListener(CloseApplication);
            nextButton.OnClicked.AddListener(NextStep);
            previousButton.OnClicked.AddListener(PreviousStep);
            resetInteraction.OnClicked.AddListener(ResetStep);
            audioSource = GetComponent<AudioSource>();
            Debug.Log("Current Step : " + currentStep);
            ToggleStepPointer(true, currentStep);

            InitStepResult();

            UpdateStepDescription();
        }


        void Init()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(this);
            }
        }

        private void CloseApplication()
        {
#if UNITY_EDITOR
            Debug.Log($"Quitting Application at {System.DateTime.Now}");
#endif
            Application.Quit();
        }

        private void NextStep()
        {
            if (currentStep < totalSteps)
            {
                ToggleStepPointer(false, currentStep);
                currentStep++;
                //Debug.Log($"aagey badh ke abhi is step pe : {currentStep}");
                isResults = false;
            }

            if (currentStep >= totalSteps)
            {
                var interactionResult = CalculateTotal();
                stepNoField.text = FormatHeader(true);//$"Result";
                descriptionField.text = FormatDescription(true);
                ToggleAtEnd();
                ToggleStepNavigator(currentStep);

                var rotationInteractions = FindObjectsOfType<RotationInteraction>(true);
                for (int i = 0; i < rotationInteractions.Length; i++)
                {
                    rotationInteractions[i].SetObjectsActive(true, false);
                }

                currentStep = totalSteps - 1;
                if (rotationPanel.activeSelf == true) rotationPanel.SetActive(false);
                isResults = true;
                return;
            }

            UpdateStepDescription();
            ToggleStepPointer(true, currentStep);
            ToggleStepNavigator(currentStep);

        }

        private void PreviousStep()
        {

            if (currentStep > 0)
            {
                ToggleStepPointer(false, currentStep);
                currentStep--;
            }

            if (currentStep == totalSteps - 1 && stepNoField.text == "Result")
            {
                currentStep++;
            }

            if (isAllPointerOn)
            {
                var fps = FindObjectsOfType<HighlightSphere>();
                foreach (var fp in fps)
                {
                    fp.gameObject.SetActive(false);
                }
                ToggleStepPointer(true, currentStep);
                isAllPointerOn = false;
            }

            UpdateStepDescription();
            ToggleStepPointer(true, currentStep);
            ToggleStepNavigator(currentStep);
            isResults = false;
        }


        private void ToggleStepNavigator(int stepIndex)
        {
            #region old code
            /*if (stepIndex == 0)
            {
                previousButton.gameObject.SetActive(false);
            }
            if (stepIndex == 1 && !previousButton.gameObject.activeSelf)
            {
                previousButton.gameObject.SetActive(true);
            }


            if (stepIndex == (totalSteps - 2) && stepNoField.text == $"Step No : {currentStep} / {steps.Count}")
            {
                nextButton.gameObject.SetActive(true);
            }
            if (stepIndex == totalSteps)
            {
                nextButton.gameObject.SetActive(false);
            }*/
            #endregion

            if (stepIndex == 0)
            {
                previousButton.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(true);
            }
            else if (stepIndex > 0)
            {
                previousButton.gameObject.SetActive(true);
                nextButton.gameObject.SetActive(true);
            }
            if (stepIndex == totalSteps)
            {
                nextButton.gameObject.SetActive(false);
            }
        }

        public void UpdateStepDescription()
        {

            if (currentStep == 0)
            {
                stepNoField.text = FormatHeader();
                if (stepResult.Count != 0)
                    descriptionField.text = FormatDescription();

            }
            else
            {
                stepNoField.text = FormatHeader();
                if (stepResult.Count != 0)
                    descriptionField.text = FormatDescription();
            }


        }


        public void ResetSingleStep(int index)
        {
            if (steps[index].interactionTypes == InteractionTypes.Translate)
            {
                var part = FindPart(steps[index].movementPart);
                var statefulInteractable = part.GetComponent<StatefulInteractable>();
                var feedbackPointer = part.GetComponentInChildren<HighlightSphere>();

                if (feedbackPointer != null)
                {
                    feedbackPointer.SetHighlight();
                    feedbackPointer.isInteractionComplete = false;
                }

                if (statefulInteractable != null)
                {
                    statefulInteractable.enabled = false;
                }
                var objectManipulator = part.GetComponent<ObjectManipulator>();
                var moveConstraint = part.GetComponent<MoveConstraint>();
                moveConstraint.reached = false;
                objectManipulator.enabled = true;
                objectManipulator.HostTransform.localPosition = moveConstraint.startPosition;



            }

            if (steps[index].interactionTypes == InteractionTypes.Rotate)
            {
                var part = RotationUtil.FindManagerWithGUID(steps[index].rotationManager);
                var feedbackPointer = part.GetComponentInChildren<HighlightSphere>();
                var pivotInteractable = part.GetComponentInChildren<PivotInteractable>(true);
                pivotInteractable.enabled = true;

                if (slidersLtl.gameObject.activeSelf == true)
                {
                    slidersLtl.minSlider.Value = slidersLtl.maxSlider.Value = 0.33333333f;
                }
                else if (sliderFreeRotFb.gameObject.activeSelf)
                {
                    sliderFreeRotFb.minSlider.Value = sliderFreeRotFb.maxSlider.Value = 0.33333333f;
                }

                pivotInteractable.CustomTargetTransform.localRotation = Quaternion.Euler(Vector3.zero);
                var rotDisplayBehaviour = part.GetComponentInChildren<RotationDisplayBehaviour>();
                rotDisplayBehaviour.UpdateRotationDisplay();

                if (feedbackPointer != null)
                {
                    feedbackPointer.SetHighlight();
                    feedbackPointer.isInteractionComplete = false;
                }

            }

            if (steps[index].interactionTypes == InteractionTypes.PartCollision)
            {
                var collisionChecker = PartCollision.GetCollisionChecker(steps[index].collisionManager);
                collisionChecker.ResetCollisionInteraction();
            }
            if (steps[index].interactionTypes == InteractionTypes.Touch)
            {

                foreach (var interaction in steps[index].touchInteractions)
                {
                    var part = FindPart(interaction);
                    var statefulInteractable = part.GetComponent<StatefulInteractable>();
                    var feedbackPointer = part.GetComponentInChildren<HighlightSphere>();

                    if (feedbackPointer != null)
                    {
                        feedbackPointer.SetHighlight();
                        feedbackPointer.isInteractionComplete = false;
                    }
                    if (statefulInteractable != null)
                    {
                        statefulInteractable.enabled = true;
                    }

                }

            }

        }
        public void ResetStep()
        {

            if (isResults == true)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    ResetSingleStep(i);

                    ToggleFeedBackPointers(false);
                }
                var fps = FindObjectsOfType<HighlightSphere>(true);
                foreach (var fp in fps)
                {
                    fp.isInteractionComplete = false;
                }
                isLastStepReset = true;
                stepResult.Clear();
                InitStepResult();
                stepNoField.text = FormatHeader(true);
                descriptionField.text = FormatDescription(true);
                return;
            }
            else
            {
                ResetSingleStep(currentStep);
                isLastStepReset = false;
                stepResult[currentStep] = 0;
                UpdateStepDescription();

            }


        }


        private void ToggleAllFeedbackPointer(bool isActive)
        {
            var fps = FindObjectsOfType<HighlightSphere>(true);

            foreach (var fp in fps)
            {
                fp.gameObject.SetActive(isActive);
            }

            if (isActive == false)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i].interactionTypes == InteractionTypes.Translate)
                    {
                        var part = FindPart(steps[i].movementPart);
                        var statefulInteractable = part.GetComponent<StatefulInteractable>();
                        if (statefulInteractable != null)
                        {
                            statefulInteractable.enabled = false;
                        }
                        var objectManipulator = part.GetComponent<ObjectManipulator>();
                        objectManipulator.enabled = false;
                    }
                    //this part could be problematic. this hasn't been tested yet
                    else if (steps[i].interactionTypes == InteractionTypes.Rotate)
                    {
                        GameObject part = RotationUtil.FindManagerWithGUID(steps[i].rotationManager);
                        part.GetComponent<RotationInteraction>().ToggleFbPointer(false);
                    }
                    else if (steps[i].interactionTypes == InteractionTypes.PartCollision)
                    {
                        var checker = PartCollision.GetCollisionChecker(steps[i].collisionManager);
                        checker.ToggleForStep(false);
                    }
                    else
                    {
                        foreach (var interaction in steps[i].touchInteractions)
                        {
                            var part = FindPart(interaction);
                            var statefulInteractable = part.GetComponent<StatefulInteractable>();
                            if (statefulInteractable != null)
                            {
                                statefulInteractable.enabled = false;
                            }
                        }
                    }
                }
            }
        }

        private void ToggleAtEnd()
        {
            ToggleAllFeedbackPointer(true);
            var fps = FindObjectsOfType<HighlightSphere>();
            foreach (var fp in fps)
            {
                fp.OnLastStep();
            }
            isAllPointerOn = true;
        }

        public float minAngle = 0f;
        public float maxAngle = 0f;

        void ToggleStepPointer(bool isActive, int stepIndex)
        {
            if (steps.Count <= 0) return;
            var step = steps[stepIndex];
            var fps = new List<HighlightSphere>();

            rotationPanel.SetActive(false);
            slidersLtl.gameObject.SetActive(false);

            var rotationInteractions = FindObjectsOfType<RotationInteraction>(true);
            for (int i = 0; i < rotationInteractions.Length; i++)
            {
                if (rotationInteractions[i].KeepActivated == true)
                {
                    //rotationInteractions[i].GetComponent<RotationInteraction>().SetObjectsActive(true);
                    rotationInteractions[i].SetObjectsActive(true, false);
                }
                else
                {
                    rotationInteractions[i].SetObjectsActive(false, true);
                }
            }


            if (step.interactionTypes == InteractionTypes.Translate)
            {
                var part = FindPart(step.movementPart);
                fps.Add(part.GetComponentInChildren<HighlightSphere>(true));
                var objectManipulator = part.GetComponent<ObjectManipulator>();
                var statefulInteractable = part.GetComponent<StatefulInteractable>();

                objectManipulator.enabled = isActive;
                statefulInteractable.enabled = !isActive;

                var col = GetComponent<Collider>();
                objectManipulator.colliders.Add(col);
                statefulInteractable.colliders.Clear();
            }

            else if (step.interactionTypes == InteractionTypes.Rotate)
            {
                GameObject part = RotationUtil.FindManagerWithGUID(step.rotationManager);
                part.GetComponent<RotationInteraction>().ToggleFbPointer(isActive);
                part.GetComponent<RotationInteraction>().SetObjectsActive(false, false);
                var pivotInteractable = part.GetComponentInChildren<PivotInteractable>(true);

                pivotInteractable.enabled = isActive;

                rotationPanel.SetActive(true);

                freeRotFbButton.enabled = freeRotButton.enabled = yAxisButton.enabled = zAxisButton.enabled = true;
                ltlButton.ForceSetToggled(true);
                xAxisButton.ForceSetToggled(true);

                RemoveAllListeners();

                AddUIListeners(part);
            }

            else if (step.interactionTypes == InteractionTypes.PartCollision)
            {
                var checker = PartCollision.GetCollisionChecker(step.collisionManager);
                checker.ToggleForStep(isActive);
            }

            else
            {
                foreach (var interaction in step.touchInteractions)
                {
                    var part = FindPart(interaction);
                    fps.Add(part.GetComponentInChildren<HighlightSphere>(true));
                    var objectManipulator = part.GetComponent<ObjectManipulator>();
                    var statefulInteractable = part.GetComponent<StatefulInteractable>();

                    if (objectManipulator != null)
                    {
                        objectManipulator.enabled = !isActive;
                        objectManipulator.colliders.Clear();
                    }

                    statefulInteractable.enabled = isActive;

                    var col = GetComponent<Collider>();
                    statefulInteractable.colliders.Add(col);
                }
            }

            foreach (var fp in fps)
            {
                fp.gameObject.SetActive(isActive);
            }
        }

        /// <summary>
        /// Removes all the listeners from the sliders and buttons
        /// </summary>
        public void RemoveAllListeners()
        {
            slidersLtl.minSlider.OnValueUpdated.RemoveAllListeners();
            slidersLtl.maxSlider.OnValueUpdated.RemoveAllListeners();
            sliderFreeRotFb.minSlider.OnValueUpdated.RemoveAllListeners();
            sliderFreeRotFb.maxSlider.OnValueUpdated.RemoveAllListeners();
            ltlButton.OnClicked.RemoveAllListeners();
            freeRotFbButton.OnClicked.RemoveAllListeners();
            freeRotButton.OnClicked.RemoveAllListeners();
            xAxisButton.OnClicked.RemoveAllListeners();
            yAxisButton.OnClicked.RemoveAllListeners();
            zAxisButton.OnClicked.RemoveAllListeners();

        }

        public void AddUIListeners(GameObject part)
        {
            slidersLtl.minSlider.OnValueUpdated.AddListener(value =>
            {
                minAngle = (slidersLtl.minSlider.Value * -90f) + 360f;
                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, false);
            });
            slidersLtl.maxSlider.OnValueUpdated.AddListener(value =>
            {
                maxAngle = slidersLtl.maxSlider.Value * 90f;
                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, false);
            });

            ltlButton.OnClicked.AddListener(() =>
            {
                slidersLtl.gameObject.SetActive(true);
                sliderFreeRotFb.gameObject.SetActive(false);

                slidersLtl.minSlider.Value = slidersLtl.maxSlider.Value = 0.33333333f;

                part.GetComponent<RotationInteraction>().SetRotationProperties(true, true, true);
                ltlButton.enabled = false;
                freeRotFbButton.enabled = freeRotButton.enabled = true;
                UpdateRotationDisplay(part);
            });
            ltlButton.OnClicked.Invoke();

            sliderFreeRotFb.minSlider.OnValueUpdated.AddListener(value =>
            {
                minAngle = (sliderFreeRotFb.minSlider.Value * -90f) + 360f;
                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, false);
            });
            sliderFreeRotFb.maxSlider.OnValueUpdated.AddListener(value =>
            {
                maxAngle = sliderFreeRotFb.maxSlider.Value * 90f;
                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, false);
            });

            freeRotFbButton.OnClicked.AddListener(() =>
            {
                slidersLtl.gameObject.SetActive(false);
                sliderFreeRotFb.gameObject.SetActive(true);

                sliderFreeRotFb.minSlider.Value = sliderFreeRotFb.maxSlider.Value = 0.33333333f;

                part.GetComponent<RotationInteraction>().SetRotationProperties(true, false, true);
                UpdateRotationDisplay(part);
                ltlButton.enabled = freeRotButton.enabled = true;
                freeRotFbButton.enabled = false;
            });

            freeRotButton.OnClicked.AddListener(() =>
            {
                slidersLtl.gameObject.SetActive(false);
                sliderFreeRotFb.gameObject.SetActive(false);
                part.GetComponent<RotationInteraction>().SetRotationProperties(true, false, false);
                UpdateRotationDisplay(part);
                ltlButton.enabled = freeRotFbButton.enabled = true;
                freeRotButton.enabled = false;
            });

            xAxisButton.OnClicked.AddListener(() =>
            {
                part.GetComponent<RotationInteraction>().SetAxisConstraints(PivotInteractable.ConstrainMode.X);

                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, true);

                xAxisButton.enabled = false;
                yAxisButton.enabled = zAxisButton.enabled = true;
            });
            xAxisButton.OnClicked.Invoke();

            yAxisButton.OnClicked.AddListener(() =>
            {
                part.GetComponent<RotationInteraction>().SetAxisConstraints(PivotInteractable.ConstrainMode.Y);

                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, true);

                xAxisButton.enabled = zAxisButton.enabled = true;
                yAxisButton.enabled = false;
            });
            zAxisButton.OnClicked.AddListener(() =>
            {
                part.GetComponent<RotationInteraction>().SetAxisConstraints(PivotInteractable.ConstrainMode.Z);

                part.GetComponent<RotationInteraction>().SetAngleValues(minAngle, maxAngle, true);

                xAxisButton.enabled = yAxisButton.enabled = true;
                zAxisButton.enabled = false;
            });
        }

        void UpdateRotationDisplay(GameObject part)
        {
            try
            {
                var rotDisplayBehaviour = part.GetComponentInChildren<RotationDisplayBehaviour>();
                rotDisplayBehaviour.UpdateRotationDisplay();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
        GameObject FindPart(string guid)
        {
            try
            {
                return interactables.Find(x => x.GetComponent<GUID>().GetGUID() == guid);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                Debug.Log("This object is not aprt of the interactables");

            }
            return TapToPlaceManager.Instance.designElements.Find(x => x.GetComponent<GUID>().GetGUID() == guid);
        }


        public void InteractedAudio()
        {
            audioSource.Stop();
            audioSource.Play();
        }



        List<int> stepResult = new List<int>();


        private void InitStepResult()
        {
            for (int i = 0; i < totalSteps; i++)
            {
                stepResult.Add(0);
            }
        }

        public void AddResult()
        {
            stepResult[currentStep]++;
        }
        public void AddResult(int count)
        {
            stepResult[currentStep] = count;
        }

        public void RemoveResult()
        {
            stepResult[currentStep]--;
        }
        string FormatHeader(bool isResultPage = false)
        {
            string header = "";
            if (isResultPage)
            {
                var interactionResult = CalculateTotal();
                header = $"<size=11>  Result</size>" +
                        $" \n<size=11><align=right>Interactions : {interactionResult[0]} / {interactionResult[1]}</align></size>";
            }
            else
            {
                header = $"<size=11>  Step No - {currentStep + 1} / {steps.Count}</size>" +
                            $" \n<size=11><align=right>Interactions : {(steps.Count <= 0 ? 0 : stepResult[currentStep])} / {ReturnInteractionCount(currentStep)}</align></size>";
            }

            return header;
        }

        string FormatDescription(bool isResultPage = false)
        {
            string description = "";

            if (isResultPage)
            {
                descriptionField.gameObject.SetActive(false);
                resultGrid.CreateGrid(CalculateStepWise());
            }
            else
            {
                resultGrid.HideResult();
                descriptionField.gameObject.SetActive(true);
                description = $"<size=11><b>Description</b></size>\n<size=4> </size>\n  " +
                            $"{steps[currentStep].stepDescription}";
            }

            return description;
        }
        private string FormatResult()
        {
            string result = "\n";
            if (isLastStepReset == true)
            {
                for (int i = 0; i < stepResult.Count; i++)
                {
                    result += $" Step - {i + 1} = {0} / \n";

                }
                isLastStepReset = false;
            }
            else
            {

                for (int i = 0; i < stepResult.Count; i++)
                {
                    result += $" Step - {i + 1} = {stepResult[i]} / {ReturnInteractionCount(i)}\n";
                }
            }

            int totalInteractions = 0;
            foreach (var step in steps)
            {
                if (step.interactionTypes != InteractionTypes.Touch)
                {
                    totalInteractions++;
                }
                else
                {
                    totalInteractions += step.touchInteractions.Count;
                }
            }
            return result;
        }
        private List<StepResult> CalculateStepWise()
        {
            var results = new List<StepResult>();
            if (isLastStepReset == true)
            {
                for (int i = 0; i < stepResult.Count; i++)
                {
                    results.Add(new StepResult(i + 1, 0, ReturnInteractionCount(i)));

                }
                isLastStepReset = false;
            }
            else
            {

                for (int i = 0; i < stepResult.Count; i++)
                {
                    results.Add(new StepResult(i + 1, stepResult[i], ReturnInteractionCount(i)));
                }
            }
            return results;
        }
        private int[] CalculateTotal()
        {
            int[] result = { 0, 0 }; // interacted/totalinteraction

            //Total Interaction Count
            foreach (var step in steps)
            {
                if (step.interactionTypes != InteractionTypes.Touch)
                {
                    result[1]++;
                }
                else
                {
                    result[1] += step.touchInteractions.Count;
                }
            }

            if (isLastStepReset) return result;

            //InteractionCompleted

            for (int i = 0; i < stepResult.Count; i++)
            {
                result[0] += stepResult[i];
            }

            return result;
        }
        private int ReturnInteractionCount(int index)
        {
            if (steps.Count <= 0) return totalStepsInStep = 1;
            if (steps[index].interactionTypes == InteractionTypes.Translate)
            {
                totalStepsInStep = 1;

            }
            if (steps[index].interactionTypes == InteractionTypes.Rotate)
            {
                totalStepsInStep = 1;

            }
            if (steps[index].interactionTypes == InteractionTypes.PartCollision)
            {
                totalStepsInStep = 1;

            }
            if (steps[index].interactionTypes == InteractionTypes.Touch)
            {

                totalStepsInStep = steps[index].touchInteractions.Count;

            }
            return totalStepsInStep;

        }

        #region Dirty code needs fixing
        private void ToggleFeedBackPointers(bool isActive)
        {
            var fps = FindObjectsOfType<HighlightSphere>();

            foreach (var fp in fps)
            {
                fp.gameObject.SetActive(isActive);
                fp.SetHighlight();
            }

            if (isActive == false)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    if (steps[i].interactionTypes == InteractionTypes.Translate)
                    {
                        var part = FindPart(steps[i].movementPart);
                        var statefulInteractable = part.GetComponent<StatefulInteractable>();
                        if (statefulInteractable != null)
                        {
                            statefulInteractable.enabled = false;
                        }
                        var objectManipulator = part.GetComponent<ObjectManipulator>();
                        objectManipulator.enabled = false;
                    }
                    else if (steps[i].interactionTypes == InteractionTypes.Rotate)
                    {
                        /*var part = FindPart(steps[i].rotationManager);
                        var statefulInteractable = part.GetComponent<StatefulInteractable>();
                        if (statefulInteractable != null)
                        {
                            statefulInteractable.enabled = false;
                        }
                        var objectManipulator = part.GetComponent<ObjectManipulator>();
                        objectManipulator.enabled = false;*/
                    }
                    else
                    {
                        foreach (var interaction in steps[i].touchInteractions)
                        {
                            var part = FindPart(interaction);
                            var statefulInteractable = part.GetComponent<StatefulInteractable>();
                            if (statefulInteractable != null)
                            {
                                statefulInteractable.enabled = false;
                            }
                        }
                    }
                }
            }

        }

        #endregion
    }
}
