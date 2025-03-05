using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public class SetObjectOffset : Editor
{
    [MenuItem("Innovate Labs/Utilities/Set Object Offset &V", false, 0)]
    public static void SetOffset()
    {
        var tapToPlace = FindObjectOfType<TapToPlaceManager>();
        if (tapToPlace != null)
        {
            tapToPlace.GetTransformOffset();
        }
        else
        {
            Debug.Log("Not Found");
        }
    }
}
