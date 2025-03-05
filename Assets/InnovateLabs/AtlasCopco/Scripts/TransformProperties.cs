using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TransformProperties : MonoBehaviour
{
    public Transform StartingPlace;
    public GameObject OuterPart;
    //public bool IsDoor;
    //public float ThresholdDistance = 4f;
    private AssemblyManager _assemblyManager;

    private void OnEnable()
    {
        _assemblyManager = FindObjectOfType<AssemblyManager>(true);
    }
    public void SetDefaultProperties()
    {
        transform.SetPositionAndRotation(StartingPlace.position, StartingPlace.rotation);
    }
    //public void Update()
    //{
    //    if (IsDoor)
    //    {
    //        var currDistance = Vector3.Distance(StartingPlace.position, transform.position);
    //        if(currDistance >= ThresholdDistance)
    //        {
    //            _assemblyManager.ChangePhase();
    //        }
    //        else
    //        {
    //            _assemblyManager.NextButton.gameObject.SetActive(false);
    //            _assemblyManager.SkipButton.gameObject.SetActive(true);
    //        }
    //        Debug.Log($"Current distance from start : {currDistance}");
    //    }
    //}
}
