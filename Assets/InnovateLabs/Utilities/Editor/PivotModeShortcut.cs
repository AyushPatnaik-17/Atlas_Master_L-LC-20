using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.EditorTools;

public class PivotModeShortcut : Editor
{
    [MenuItem("Innovate Labs/Utilities/Change Pivot Mode &X")]
    public static void ChangePivot()
    {
        if(Tools.pivotMode == PivotMode.Pivot)
        {
            Tools.pivotMode = PivotMode.Center;
        }
        else
        {
            Tools.pivotMode = PivotMode.Pivot;
        }
    }
    [MenuItem("Innovate Labs/Utilities/Handle Mode &Z")]
    public static void ChangeHandle()
    {
        if(Tools.pivotRotation == PivotRotation.Global)
        {
            Tools.pivotRotation = PivotRotation.Local;
        }
        else
        {
            Tools.pivotRotation = PivotRotation.Global;
        }
    }
}
