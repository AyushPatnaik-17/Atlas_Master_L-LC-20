using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine;

public class TriggerEvent : MonoBehaviour
{

    public enum CylinderType
    {
        CylOne,
        CylTwo
    }
    //public Phases Phase;
    private AssemblyManager _assemblyManager;
    public CylinderType CylType = CylinderType.CylOne;
    public float ThresholdValue;
    public float DistanceThreshold;
    public Transform RequiredTransform;
    public ObjectManipulator ObjMan;
    public bool IsOnePlaced = false;
    public bool IsTwoPlaced = false;

    public GameObject CylOneWarning;
    public GameObject CylTwoWarning;


    private void OnEnable()
    {
        _assemblyManager = FindObjectOfType<AssemblyManager>();
        //DistanceThreshold = 0.1f;
    }

    public void OnTriggerEnter(Collider other)
    {
        
        if (CylType == CylinderType.CylOne)
        {
            CheckPartAndOrientation(other, "CylOne");
        }
        else if(CylType == CylinderType.CylTwo)
        {
            CheckPartAndOrientation(other, "CylTwo");
        }
    }

    private void CheckPartAndOrientation(Collider other, string tag, bool handleMan = true)
    {
        if (other.CompareTag("Part") || other.CompareTag("finger"))
            return;
        if (!other.CompareTag(tag))
        {
            //_assemblyManager.PlacementWarning.SetActive(true);
            return;
        }

        Vector3 forwardA = new Vector3(RequiredTransform.forward.x, RequiredTransform.forward.y, 0).normalized;
        Vector3 forwardB = new Vector3(other.transform.forward.x, other.transform.forward.y, 0).normalized;


        float angleDifference = Vector3.Angle(forwardA, forwardB);
        var distance = Vector3.Distance(RequiredTransform.position, other.transform.position);

        Debug.Log($"distance : {distance}, angle: {angleDifference}");
        bool isValidOrientation = angleDifference <= ThresholdValue && distance <= DistanceThreshold;
        
        if (isValidOrientation)
        {
            if (handleMan)
            {
                other.transform.SetPositionAndRotation(RequiredTransform.position, RequiredTransform.rotation);
            }

            switch (CylType)
            {
                case CylinderType.CylOne:
                    CylOneWarning.SetActive(false);
                    break;
                case CylinderType.CylTwo:
                    CylTwoWarning.SetActive(false);
                    break;
            }
            //_assemblyManager.PlacementWarning.SetActive(false);
            //SetPlacedState(other, true);
            if (handleMan)
                HandleObjManipulator(other.gameObject);
        }
        else
        {
            //Debug.Log("Wrong Orientation");
            //SetPlacedState(other, false);
            //if(!IsOnePlaced || !IsTwoPlaced)
            //    _assemblyManager.PlacementWarning.SetActive(true);
            switch (CylType)
            {
                case CylinderType.CylOne:
                    CylOneWarning.SetActive(true);
                    break;
                case CylinderType.CylTwo:
                    CylTwoWarning.SetActive(true);
                    break;
            }
        }
    }
    
    private void WarningHandler()
    {

    }
    private void SetPlacedState(Collider other, bool set)
    {
        if (other.CompareTag("CylOne"))
        {
            IsOnePlaced = set;
        }
        else if (other.CompareTag("CylTwo"))
        {
            IsTwoPlaced = set;
        }
    }

    private void HandleObjManipulator(GameObject other)
    {
        ObjMan = other.GetComponent<ObjectManipulator>();
        if (ObjMan != null)
        {
            ObjMan.enabled = false;
            Invoke("EnableObjectManipulator", 1f);
        }
    }
    //public void OnTriggerStay(Collider other)
    //{
    //    if (CylType == CylinderType.CylOne)
    //    {
    //        CheckPartAndOrientation(other, "CylOne",false);
    //    }
    //    else if (CylType == CylinderType.CylTwo)
    //    {
    //        CheckPartAndOrientation(other, "CylTwo",false);
    //    }
    //}

    private void EnableObjectManipulator()
    {
        if(ObjMan != null)
            ObjMan.enabled = true;
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Part") || other.CompareTag("finger"))
            return;
        switch (CylType)
        {
            case CylinderType.CylOne:
                CylOneWarning.SetActive(false);
                break;
            case CylinderType.CylTwo:
                CylTwoWarning.SetActive(false);
                break;
        }
        //if (other.CompareTag("CylOne") || other.CompareTag("CylTwo"))
        //    _assemblyManager.PlacementWarning.SetActive(false);
        //if (CylType == CylinderType.CylOne)
        //{
        //    SetPlacedState(other, false);
        //}
        //else if (CylType == CylinderType.CylTwo)
        //{
        //    SetPlacedState(other, false);
        //}
    }
}
