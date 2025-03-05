using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class AssemblyManager : MonoBehaviour
{

    [Header("Warning")]
    public GameObject PlacementWarning;
    public GameObject InterferenceWarning;
    public GameObject WarningIcon;
    public TextMeshProUGUI WarningText;

    public bool IsTriggerAvailable;
    public PressableButton ToggleCollision;
    public TransformProperties CylinderOne;
    public TransformProperties CylinderTwo;

    [Header("Misc")]

    public TextMeshProUGUI DescriptionText;
    public PressableButton NextButton, PreviousButton, ResetButton, HandInteractionButton, SkipButton;
    public HandCollisionChecker HandCollision;
    public MRTKTMPInputField InputField;
    public Material Shader;


    [Header("Parts")]
    [SerializeField] public List<GameObjectTriplet> Triplets = new();
    //public List<GameObject> Parts = new List<GameObject>();
    //[SerializeField] public List<ObjectMaterialPair> ObjMatPairs = new();
    public List<string> DescriptionTexts = new List<string>();
    public int CurrentStep = 0;
    public int PreviousStep = -1;
    public int TotalSteps = 0;
    //public GameObject FloorTwo;

    public GameObject DiameterOptions;
    //public GameObject ButtonCollection;
    public ViewManager ViewManager;
    //public DirectionalIndicator DirectionalIndicator;

    public void Start()
    {
        //FirstPhase();
        AddButtonListenrs();
        //NextButton.gameObject.SetActive(false);
        //PreviousButton.gameObject.SetActive(false);
        //ResetButton.gameObject.SetActive(false);
        //DescriptionText.text = "Place the part at the marked location";
        //TotalSteps = Triplets.Count;
        //ViewManager = GetComponent<ViewManager>();

        //for(int i = 0; i < Parts.Count; i++)
        //{
        //    ObjectMaterialPair pair = new ObjectMaterialPair();
        //    MeshRenderer meshRen = Parts[i].GetComponent<MeshRenderer>();
        //    List<Material> materials = meshRen.materials.ToList();

        //    pair.Part = Parts[i];
        //    pair.MeshRen = meshRen;
        //    pair.Materials = materials;

        //    ObjMatPairs.Add(pair);
        //}
        //DirectionalIndicator = FindObjectOfType<DirectionalIndicator>(true);
    }

    private void AddButtonListenrs()
    {
        //NextButton.OnClicked.AddListener(OnNext);
        //PreviousButton.OnClicked.AddListener(OnPrevious);
        ResetButton.OnClicked.AddListener(OnReset);
        HandInteractionButton.OnClicked.AddListener(delegate
        {
            HandCollision.isTriggerAvailable = HandInteractionButton.IsToggled.Active;
                
        });
        ToggleCollision.OnClicked.AddListener(delegate
        {
            IsTriggerAvailable = ToggleCollision.IsToggled.Active;
        });
        //SkipButton.OnClicked.AddListener(OnSkip);
    }
    public void FirstPhase()
    {
        CurrentStep = 0;
        SwitchStep();
        ResetButton.gameObject.SetActive(true);
        HandCollision.isTriggerAvailable = false;
    }
    

    public void SwitchStep()
    {
        for (int i = 0; i < TotalSteps; i++)
        {
            if (i == CurrentStep)
            {
                //DirectionalIndicator.DirectionalTarget = Triplets[i].OuterBox.transform;
                ObjectManipulator objMan = Triplets[i].Part.GetComponent<ObjectManipulator>();
                if (objMan != null)
                {
                    objMan.enabled = true;
                }
                Triplets[i].Part.GetComponent<TransformProperties>().SetDefaultProperties();
                Triplets[i].Visibility(locator: true, outer: true, part: true);
                DescriptionText.text = DescriptionTexts[i];
                //Interactable interactable = Triplets[i].Part.GetComponent<Interactable>();
                //if (interactable.InteractionType == InteractionTypes.Animatable)
                //{
                //    //DirectionalIndicator.DirectionalTarget = interactable.GetComponentInChildren<Animatable>().transform;
                //    Triplets[i].Part.SetActive(true);
                //    Triplets[i].OuterBox.SetActive(false);
                //    interactable.StartAnimation();
                //}
            }
            else
            {
                if (i < CurrentStep)
                {
                    ObjectManipulator objMan = Triplets[i].Part.GetComponent<ObjectManipulator>();
                    if (objMan != null)
                    {
                        objMan.enabled = false;
                    }
                    Triplets[i].Visibility(locator: false, outer: false, part: true);
                }
                else if (i > CurrentStep)
                {
                    Triplets[i].Visibility(locator: false, outer: false, part: false);
                }
            }

        }
    }

    public void ChangePhase()
    {
        if(CurrentStep >= 0)
        {
            NextButton.gameObject.SetActive(true);
            ResetButton.gameObject.SetActive(true);
        }

    }

    public void CloseApp()
    {
        Debug.Log("Close");
        Application.Quit();
    }
    private void OnNext()
    {
        if (CurrentStep <= TotalSteps - 1)
        {
            CurrentStep += 1;
            NextButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(true);
            //DirectionalIndicator.enabled = true;
            //DirectionalIndicator.gameObject.SetActive(true);
            SwitchStep();
        }
        else if (CurrentStep == 0)
        {
            NextButton.gameObject.SetActive(false);
            PreviousButton.gameObject.SetActive(false);
        }
            
        if (CurrentStep > 0)
        {
            PreviousButton.gameObject.SetActive(true);
        }
        if(CurrentStep == 1)
        {
            Triplets[2].Part.SetActive(true);
            Triplets[2].Part.GetComponent<ObjectManipulator>().enabled = false;
        }
        if(CurrentStep >= 2)
        {
            Triplets[1].Part.SetActive(false);
        }
        if(CurrentStep == 3)
        {
            DiameterOptions.SetActive(true);
            Triplets[3].OuterBox[0].SetActive(false);
            ViewManager.TurnOffDefault();
        }
        else
        {
            DiameterOptions.SetActive(false);
            Triplets[3].OuterBox[0].SetActive(false);
            AnimationManager animManager = FindObjectOfType<AnimationManager>(true);
            animManager.ClippingPlaneOne.SetActive(false);
            animManager.ClippingPlaneTwo.SetActive(false);
        }
        if(CurrentStep >= TotalSteps)
        {
            HandCollision.isTriggerAvailable = true;
            WarningText.text = "Interference Detected!";
            DescriptionText.text = "Assembly Complete";
            //DescriptionText.text = "Select View Options";
            HandInteractionButton.gameObject.SetActive(true);
            NextButton.gameObject.SetActive(false);
            PreviousButton.gameObject.SetActive(false);
            //ViewPanel.SetActive(true);
            SkipButton.gameObject.SetActive(false);
            //DirectionalIndicator.enabled = false;
            //DirectionalIndicator.gameObject.SetActive(false);
            HandInteractionButton.ForceSetToggled(true);
        }

        PreviousStep = CurrentStep;
    }

    private void OnPrevious()
    {
        if (CurrentStep > 0)
        {
            CurrentStep -= 1;
            PreviousButton.gameObject.SetActive(true);
            NextButton.gameObject.SetActive(false);
            SkipButton.gameObject.SetActive(true);
            SwitchStep();
        }
        else if (CurrentStep == 0)
            PreviousButton.gameObject.SetActive(false);
        if (CurrentStep == 1)
        {
            Triplets[2].Part.SetActive(true);
            Triplets[2].Part.GetComponent<ObjectManipulator>().enabled = false;
        }
        if (CurrentStep >= 2)
        {
            Triplets[1].Part.SetActive(false);
        }
        if (CurrentStep == 3)
        {
            DiameterOptions.SetActive(true);
            Triplets[3].OuterBox[0].SetActive(false);
            ViewManager.TurnOffDefault();
        }
        else
        {
            DiameterOptions.SetActive(false);
            Triplets[3].OuterBox[0].SetActive(false);
            ViewManager.TurnOffDefault();
        }

        PreviousStep = CurrentStep;
    }

    private void OnReset()
    {
        //DescriptionText.text = "Place the parts at the marked location";
        //PreviousButton.gameObject.SetActive(false);
        //NextButton.gameObject.SetActive(false);
        //FloorTwo.SetActive(false);
        //HandCollision.isTriggerAvailable = false;
        //HandInteractionButton.ForceSetToggled(false);
        //ViewManager.TurnOffVersions();
        //ViewManager.ER.SetActive(false);
        //ViewPanel.SetActive(false);
        //SkipButton.gameObject.SetActive(true);
        //CurrentStep = 0;
        //ViewManager.TurnOffDefault();
        SetToStartPositions();
        //ClearCoroutines();
        //SwitchStep();
    }

    private void SetToStartPositions()
    {
        //foreach(var t in Triplets)
        //{
        //    var tp = t.Part.GetComponent<TransformProperties>();
        //    tp.SetDefaultProperties();
        //}
        Debug.Log("Setting cylinders to default");
        CylinderOne.SetDefaultProperties();
        CylinderTwo.SetDefaultProperties();
    }

    private void OnSkip()
    {
        Transform requiredTransform = Triplets[CurrentStep].Locator.GetComponentInChildren<TriggerEvent>().RequiredTransform;
        Triplets[CurrentStep].Part.transform.SetPositionAndRotation(requiredTransform.position, requiredTransform.rotation);
        if(CurrentStep == 3)
        {
            ViewManager.OptionOneBtn.OnClicked.Invoke();
        }
        //Interactable interactable = Triplets[CurrentStep].InternalPart.GetComponent<Interactable>();
        //if(interactable.InteractionType == InteractionTypes.Regular)
        //{
        //    Transform requiredTransform = Triplets[CurrentStep].Indicator.GetComponent<TriggerEvent>().RequiredTransform;
        //    Triplets[CurrentStep].OuterBox.transform.SetPositionAndRotation(requiredTransform.position, requiredTransform.rotation);
        //}
        //else if(interactable.InteractionType == InteractionTypes.Animatable)
        //{
        //    interactable.StopAnimation();

        //}
        SkipButton.gameObject.SetActive(false);
    }

    private void ClearCoroutines()
    {
        List<Interactable> interactables = FindObjectsOfType<Interactable>().ToList();
        for(int i = 0; i < interactables.Count; i++)
        {
            if (interactables[i].InteractionType == InteractionTypes.Animatable)
            {
                interactables[i].Coroutines.Clear();
            }   
        }
    }

}

[Serializable]
public struct GameObjectTriplet
{
    public GameObject Locator;
    public GameObject Part;
    public List<GameObject> OuterBox;

    public void Visibility(bool locator, bool outer, bool part)
    {
        if (Locator != null)
            Locator.SetActive(locator);

        if (OuterBox.Count != 0 || OuterBox != null)
        {
            foreach(var box in OuterBox)
            {
                box.SetActive(outer);
            }
        }

        if (Part != null)
            Part.SetActive(part);
    }

    //public void ToggleOff()
    //{
    //    if (Locator != null)
    //        Locator.SetActive(false);

    //    if (OuterBox != null)
    //        OuterBox.SetActive(false);

    //    if (Part != null)
    //        Part.SetActive(false);
    //}
}

[System.Serializable]
public struct ObjectMaterialPair
{
    public GameObject Part;
    public MeshRenderer MeshRen;
    public List<Material> Materials;

    public ObjectMaterialPair(GameObject part,MeshRenderer meshRen, List<Material> materials)
    {
        Part = part;
        MeshRen = meshRen;
        Materials = materials;
    }
    public void SetDefaultMaterials()
    { 
        MeshRen.materials = Materials.ToArray();
    }

    public void SetMaterial(Material material)
    {
        Material[] newMaterials = new Material[MeshRen.sharedMaterials.Length];
        for (int i = 0; i < newMaterials.Length; i++)
        {
            newMaterials[i] = material;
        }
        MeshRen.materials = newMaterials;
    }
}

public enum InteractionTypes
{
    Animatable,
    Regular
}
