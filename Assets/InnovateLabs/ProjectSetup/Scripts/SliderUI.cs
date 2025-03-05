using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using System;

public class SliderUI : MonoBehaviour
{
    public Slider minSlider;
    public Slider maxSlider;

    public TextMeshProUGUI minValTxt;
    public TextMeshProUGUI maxValTxt;

    public void Awake() => minSlider.Value = maxSlider.Value = 0;
    public void OnEnable()
    {
        minSlider.OnValueUpdated.AddListener(value => minValTxt.text = $"Min Angle: -{String.Format("{0:0.00}", minSlider.Value * 90f)}");
        maxSlider.OnValueUpdated.AddListener(value => maxValTxt.text = $"Max Angle: {String.Format("{0:0.00}", maxSlider.Value * 90f)}");
    }
}
