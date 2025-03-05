using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Options
{
    none, OptionOne, OptionTwo
}
public class ViewManager : MonoBehaviour
{
    public AnimationManager AnimationManager;
    public PressableButton OptionOneBtn, OptionTwoBtn;
    public GameObject FirstOption, SecondOption;
    public GameObject ViewBox;
    public Options CurrentOption = Options.none;

    private void Awake()
    {
        OptionOneBtn.OnClicked.AddListener(delegate
        {
            CurrentOption = Options.OptionOne;
            ToggleViewModes(true);
        });
        OptionTwoBtn.OnClicked.AddListener(delegate
        {
            CurrentOption = Options.OptionTwo;
            ToggleViewModes(false);
        });
    }

    public void ToggleViewModes(bool toggle)
    {
        FirstOption.SetActive(toggle);
        SecondOption.SetActive(!toggle);
        ViewBox.SetActive(true);
    }

    public void TurnOffDefault()
    {
        FirstOption.SetActive(false);
        SecondOption.SetActive(false);
        AnimationManager.TurnOffAll();
    }

    //public void TurnOffVersions()
    //{
    //    AirCooled.SetActive(false);

    //    LiquidCooled.SetActive(false);

    //    foreach (var part in ToDisable)
    //        part.SetActive(true);
    //}
}
