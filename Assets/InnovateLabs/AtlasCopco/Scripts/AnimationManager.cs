using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public ViewManager ViewManager;
    public PressableButton SkipAnimBtn;
    public GameObject ClippingPlaneOne;
    public GameObject ClippingPlaneTwo;
    public GameObject AnimationGroupOne;
    public GameObject AnimationGroupTwo;
    private void Start()
    {
        ViewManager = FindObjectOfType<ViewManager>(true);
        //SkipAnimBtn.OnClicked.AddListener(delegate
        //{
        //    SkipAnimBtn.gameObject.SetActive(false);
        //    SkipAnimation();
        //});
        //SkipAnimBtn.gameObject.SetActive(false);

    }

    private void SkipAnimation()
    {
        Debug.Log("Skipping animation");
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Detector"))
        {
            ToggleOpitons(true);
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Detector"))
        {
            ToggleOpitons(true);
        }
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Detector"))
        {
            ToggleOpitons(false);
        }
    }

    private void ToggleOpitons(bool enable)
    {
        //SkipAnimBtn.gameObject.SetActive(enable);
        switch (ViewManager.CurrentOption)
        {
            case Options.OptionOne:
                //ManageObjectStates(AnimationGroupTwo, false);
                //ManageObjectStates(AnimationGroupOne, enable);
                AnimationGroupTwo.SetActive(false);
                ClippingPlaneTwo.SetActive(false);
                ClippingPlaneOne.SetActive(enable);
                AnimationGroupOne.SetActive(enable);
                break;
            case Options.OptionTwo:
                //ManageObjectStates(AnimationGroupOne, false);
                //ManageObjectStates(AnimationGroupTwo, enable);
                AnimationGroupTwo.SetActive(enable);
                ClippingPlaneTwo.SetActive(enable);
                ClippingPlaneOne.SetActive(false);
                AnimationGroupOne.SetActive(false);
                break;
            default:
                Debug.Log("Idhar kahan bhai?");
                break;
        }
    }

    //private void ManageObjectStates(List<GameObject> objectList, bool toggle)
    //{
    //    //group.SetActive(toggle);
    //    foreach (var anim in objectList)
    //    {
    //        anim.SetActive(toggle);
    //    }
    //}

    public void TurnOffAll()
    {
        //ManageObjectStates(AnimationGroupOne, false);
        //ManageObjectStates(AnimationGroupTwo, false);
        AnimationGroupTwo.SetActive(false);
        AnimationGroupOne.SetActive(false);
        ClippingPlaneOne.SetActive(false);
        ClippingPlaneTwo.SetActive(false);
    }

}
