using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AtlasCopco.DS;

public class PartsToggler : MonoBehaviour
{
    public List<ButtonObjectPair> ButtonObjects = new();
    void Start()
    {
        foreach (var pair in ButtonObjects)
        {
            pair.SetupListener();
        }
    }

    [ContextMenu("Validate List")]
    public void Validate()
    {
        foreach (var pair in ButtonObjects) 
        {
            pair.Validate();
        }
    }

    public void QuitApp()
    {
        Application.Quit();
    }

}
