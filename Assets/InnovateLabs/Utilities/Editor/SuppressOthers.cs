using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class SuppressOthers : Editor
{
    [MenuItem("Innovate Labs/ Utilities/ Suppress Other")]
    public static void Suppress()
    {

        var rootparents = new List<GameObject>();

        var selectedGameObjects = new List<GameObject>();

        var selections = Selection.objects;
        foreach(var selection in selections)
        {
            if(selection.GetType() == typeof(GameObject))
            {
                selectedGameObjects.Add((GameObject)selection);
            }
        }

        foreach(var selectedGameObject in selectedGameObjects)
        {
            var currentParent = selectedGameObject;
            while(currentParent.transform.parent != null)
            {
                currentParent = currentParent.transform.parent.gameObject;
            }
            if(!rootparents.Exists(x => x ==currentParent))
            {
                rootparents.Add(currentParent);
            }
        }

        foreach(var parent in rootparents)
        {
            var children = parent.GetComponentsInChildren<MeshRenderer>();
            foreach(var child in children)
            {
                if(selectedGameObjects.Exists(x=>x == child.gameObject))
                {
                    continue;
                }
                child.gameObject.SetActive(false);
            }
        }
    }
    [MenuItem("Innovate Labs/ Utilities/ UnSuppress All &S")]
    public static void UnSuppress()
    {
        if (EditorApplication.isPlaying) return;
        var rootparents = new List<GameObject>();

        var selectedGameObjects = new List<GameObject>();

        var selections = Selection.objects;

        foreach(var selection in selections)
        {
            if(selection.GetType() == typeof(GameObject))
            {
                selectedGameObjects.Add((GameObject)selection);
            }
        }

        foreach(var selectedGameObject in selectedGameObjects)
        {
            var currentParent = selectedGameObject;
            while(currentParent.transform.parent != null)
            {
                currentParent = currentParent.transform.parent.gameObject;
            }
            if(!rootparents.Exists(x => x ==currentParent))
            {
                rootparents.Add(currentParent);
            }
        }

        foreach(var parent in rootparents)
        {
            var children = parent.GetComponentsInChildren<Transform>(true);
            foreach(var child in children)
            {
                //if(selectedGameObjects.Exists(x=>x == child.gameObject))
                //{
                //    continue;
                //}
                child.gameObject.SetActive(true);
            }
        }
    }

    [MenuItem("Innovate Labs/ Utilities/ Suprress Selected _S")]
    public static void SuppressSelection()
    {
        if (EditorApplication.isPlaying) return;
        var rootparents = new List<GameObject>();

        var selectedGameObjects = new List<GameObject>();

        var selections = Selection.objects;

        foreach (var selection in selections)
        {
            if (selection.GetType() == typeof(GameObject))
            {
                selectedGameObjects.Add((GameObject)selection);
            }
        }

        foreach (var selectedGameObject in selectedGameObjects)
        {
            selectedGameObject.gameObject.SetActive(false);
        }

    }
}
