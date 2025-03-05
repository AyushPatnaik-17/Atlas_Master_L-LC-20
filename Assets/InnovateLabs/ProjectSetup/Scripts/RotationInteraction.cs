using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using GUID = InnovateLabs.Projects.GUID;
using System;
using UnityEditor;
using InnovateLabs.Projects;
using System.Threading.Tasks;

public class RotationInteraction : MonoBehaviour
{
    public List<GameObject> rotationParts = new List<GameObject>();

    public List<GameObject> grabParts = new List<GameObject>();

    private static Vector3 initRot = Vector3.zero;

    public delegate void D_rotationInteraction();
    public event D_rotationInteraction E_rotationInteraction;

    private List<GameObject> duplicateParts;

    public void OnEnable() => InstantiateRotationObjects();

    [SerializeField]bool keepActivated;
    public bool KeepActivated
    {
        get { return keepActivated; }
        set { keepActivated = value; }
    }


    private void Awake()
    {
        var pivotInteractable = transform.GetComponentInChildren<PivotInteractable>(true);
        pivotInteractable.AxisConstraintMode = PivotInteractable.ConstrainMode.X;
        pivotInteractable.enabled = false;
        this.gameObject.SetActive(false);
    }

    public void InstantiateRotationObjects()
    {
        List<GameObject> instantiatedParts = new List<GameObject>();
        List<GameObject> instantiatedGrabParts = new List<GameObject>();
        var grabPoint = transform.GetComponentInChildren<PivotInteractable>(true);

        for (int i = 0; i < rotationParts.Count; i++)
        {
            GameObject duplicatePart = Instantiate(rotationParts[i], grabPoint.gameObject.transform);
            duplicatePart.transform.position = rotationParts[i].transform.position;
            duplicatePart.transform.rotation = rotationParts[i].transform.rotation;
            duplicatePart.transform.localScale = rotationParts[i].transform.lossyScale;
            duplicatePart.name = rotationParts[i].name;
            instantiatedParts.Add(duplicatePart);
            if (grabParts.Contains(rotationParts[i]))
            {
                instantiatedGrabParts.Add(duplicatePart);
            }
        }
        for (int i = 0; i < instantiatedGrabParts.Count; i++)
        {
            if (!instantiatedGrabParts[i].HasComponent<BoxCollider>())
            {
                instantiatedGrabParts[i].AddComponent<BoxCollider>();
            }
            grabPoint.colliders.Add(instantiatedGrabParts[i].GetComponent<BoxCollider>());
        }

        for (int i = 0; i < rotationParts.Count; i++)
        {
            rotationParts[i].SetActive(false);
        }

        duplicateParts = instantiatedParts;
    }

    /// <summary>
    /// If the a rotation step is activated, then the corresponding original objects are set
    /// inactive and the duplicate ones are set active and vice versa
    /// </summary>
    /// <param name="setActive"></param>
    /// <param name="isKeepActiveCase"></param>
    public void SetObjectsActive(bool setActive, bool isKeepActiveCase = false)
    {
        #region old code
        /*if (!isKeepActiveCase)
        {
            for (int i = 0; i < rotationParts.Count; i++)
            {
                rotationParts[i].SetActive(setActive);
                if (duplicateParts == null)
                    return;
                duplicateParts[i].SetActive(!setActive);
            }
        }
        else
        {
            for (int i = 0; i < rotationParts.Count; i++)
            {
                rotationParts[i].SetActive(setActive);
                if (duplicateParts == null)
                    return;
                duplicateParts[i].SetActive(setActive);
            }
        }*/
        #endregion

        for (int i = 0; i < rotationParts.Count; i++)
        {
            rotationParts[i].SetActive(setActive);
            if (duplicateParts == null)
                return;
            duplicateParts[i].SetActive(isKeepActiveCase ? setActive : !setActive);
        }


    }

    public void ToggleFbPointer(bool setActive)
    {
        var feedBackPointer = transform.GetComponentInChildren<HighlightSphere>(true);
        var followAlong = transform.GetComponentInChildren<MirrorPosition>(true);
        followAlong.pivotInteractable = transform.GetComponentInChildren<PivotInteractable>();
        feedBackPointer.gameObject.SetActive(setActive);
        TransformManipulator gizmo = transform.GetComponentInChildren<TransformManipulator>(true);

        gizmo.gameObject.SetActive(setActive);
    }

    /// <summary>
    /// Sets the properties in the pivot interactable component of the respective rotation manager
    /// </summary>
    /// <param name="shouldReset"> </param>
    /// <param name="shouldRestrict"> </param>
    /// <param name="shouldPlaySound"> </param>
    public void SetRotationProperties(bool shouldReset, bool shouldRestrict, bool shouldPlaySound)
    {
        var pivInteractable = transform.GetComponentInChildren<PivotInteractable>();

        if (shouldReset)
            pivInteractable.CustomTargetTransform.localRotation = Quaternion.Euler(initRot);

        pivInteractable.minAngle = 330f;//setting default angles
        pivInteractable.maxAngle = 30f;

        pivInteractable.b_shouldRestrict = shouldRestrict;
        pivInteractable.b_shouldPlaySound = shouldPlaySound;
    }
    
    /// <summary>
    /// Sets the min and max values for the respective pivot interactable components
    /// </summary>
    /// <param name="minAngle"></param>
    /// <param name="maxAngle"></param>
    /// <param name="shouldReset"></param>
    public void SetAngleValues(float minAngle, float maxAngle, bool shouldReset)
    {
        var pivInteractable = transform.GetComponentInChildren<PivotInteractable>();

        if (shouldReset)
            pivInteractable.CustomTargetTransform.localRotation = Quaternion.Euler(initRot);

        pivInteractable.minAngle = minAngle;
        pivInteractable.maxAngle = maxAngle;
    }
   
    public void SetAxisConstraints(PivotInteractable.ConstrainMode constraint)
    {
        PivotInteractable pivInteractable = transform.GetComponentInChildren<PivotInteractable>();
        pivInteractable.AxisConstraintMode = constraint;
        E_rotationInteraction.Invoke();
    }

    public void InteractionCompleted()
    {
        var fp = GetComponentInChildren<HighlightSphere>();
        if (fp == null) return;

        fp.OnInteractionCompleted(1);
    }
}



