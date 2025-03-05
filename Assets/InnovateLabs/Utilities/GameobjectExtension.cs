using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameobjectExtension
{
    public static Bounds GetBoundsWithChildren(this GameObject gameObject)
    {
        // GetComponentsInChildren() also returns components on gameobject which you call it on
        // you don't need to get component specially on gameObject
        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();

        // If renderers.Length = 0, you'll get OutOfRangeException
        // or null when using Linq's FirstOrDefault() and try to get bounds of null
        Bounds bounds = renderers.Length > 0 ? renderers[0].bounds : new Bounds();

        // Or if you like using Linq
        // Bounds bounds = renderers.Length > 0 ? renderers.FirstOrDefault().bounds : new Bounds();

        // Start from 1 because we've already encapsulated renderers[0]
        for (int i = 1; i < renderers.Length; i++)
        {
            if (renderers[i].enabled)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
        }

        return bounds;
    }
    
    public static Bounds GetBoundsWithCenter(this GameObject gameObject, ref Vector3 worldCenter, ref Vector3 min, ref Vector3 max, Vector3 pivotPosition)
    {

        MeshFilter[] meshFilter = gameObject.GetComponentsInChildren<MeshFilter>();

        Bounds bounds = meshFilter.Length > 0 ? meshFilter[0].sharedMesh.bounds : new Bounds();

        List<Vector3> center = new List<Vector3>();


        center.Add(meshFilter[0].transform.TransformPoint(meshFilter[0].transform.localPosition) - pivotPosition) ;
         
        for (int i = 1; i < meshFilter.Length; i++)
        {
            if (meshFilter[i].gameObject.activeSelf)
            {
                center.Add(meshFilter[i].transform.TransformPoint(meshFilter[i].transform.localPosition) - pivotPosition);
                bounds.Encapsulate(meshFilter[i].sharedMesh.bounds);
            }
        }

        Vector3 centersum = Vector3.zero;

        foreach(var c in center)
        {
            centersum += c;
        }

        worldCenter = centersum/center.Count;
        min = bounds.min;
        max = bounds.max;
        return bounds;
    }

    public static bool HasAnyComponents(this GameObject target, Type[] componentTypes)
    {
        foreach (Type componentType in componentTypes)
        {
            if (target.GetComponent(componentType) != null)
            {
                return true;
            }
        }
        return false;
    }

    public static bool HasAnyComponentsInParent(this GameObject target, Type[] componentTypes)
    {

        foreach (Type componentType in componentTypes)
        {
            if (target.GetComponentInParent(componentType) != null)
            {
                return true;
            }
        }
        return false;
    }
}
