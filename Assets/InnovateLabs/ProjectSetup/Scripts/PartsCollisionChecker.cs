using InnovateLabs.Projects;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PartsCollisionChecker : MonoBehaviour
{
    public Material collisionMaterial;

    public GameObject detectionFeedback;

    public Transform snapPoint;

    public List<CollidingPart> collidingParts = new List<CollidingPart>();
    public List<CollidingPart> collisionHistory = new List<CollidingPart>();

    float defaultAlpha = 0;

    public bool isTriggerAvailable = false;
    public bool isInteractionCompleted = false;

    private Vector3 defaultLocalPosition= Vector3.zero;
    private Quaternion defaultLocalRotation = Quaternion.identity;


    public bool visualizeFinalPosition = true;
    //public Action<Collider> onTriggerEnter;
    //public Action<Collider> onTriggerExit;
    //public Action<Collider> onTriggerStay;


    private void Awake()
    {
        var manipulator = GetComponent<ObjectManipulator>();
        defaultLocalPosition = manipulator.HostTransform.localPosition;
        defaultLocalRotation = manipulator.HostTransform.localRotation;

        ToggleForStep(true);

        var meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            var currentColor = meshRenderer.material.color;
            defaultAlpha = currentColor.a;
            meshRenderer.material.color = new Color(currentColor.r, currentColor.g, currentColor.b, 0f);
        }

    }
    public void TriggerEnter(Collider other)
    {
        if (!isTriggerAvailable) return;
        
        if (collidingParts.Exists(x => x.part == other.gameObject)) return;

        if (collidingParts.Count != 0) detectionFeedback.SetActive(true);

        if (other.transform.parent.GetComponent<PartPosition>() != null)
        {
            if (!isInteractionCompleted)
                OnInteractionCompleted(other);
            return;
        }

        CollidingPart collision = null;

        collision = collisionHistory.Find(x => x.part == other.gameObject);
        if (collision == null)
        {
            collision = new CollidingPart(other.gameObject);
            collisionHistory.Add(collision);

            Debug.Log($"Unable to find in history : {collision.part}");
        }

        //var collision = new CollidingPart(other.gameObject);
        Debug.Log($"Change function call : {collision.part}");

        collision.SetMaterial(collisionMaterial);
        collision.collisionCount++;
        collidingParts.Add(collision);

    }

    public void TriggerStay(Collider other)
    {
        if (!isTriggerAvailable) return;

        var collision = collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;

        detectionFeedback.SetActive(true);

        collision.SetMaterial(collisionMaterial);

    }

    

    public void TriggerExit(Collider other)
    {
        if (!isTriggerAvailable) return;

        var collision = collidingParts.Find(x => x.part == other.gameObject);

        if (collision == null) return;
        
        collision.SetDefaultMaterial();
        collidingParts.Remove(collision);

        if (collidingParts.Count == 0) detectionFeedback.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (!visualizeFinalPosition) return;
        if (snapPoint == null) return;

        var meshColliders = transform.GetComponentsInChildren<MeshFilter>(true);

        foreach (var mesh in meshColliders)
        {
            if (mesh.GetComponent<PartCollisionListener>() == null)
            {
                var color = Color.cyan;
                Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
                var localPose = transform.InverseTransformPoint(mesh.transform.position);
                Gizmos.matrix = Matrix4x4.TRS(localPose + snapPoint.transform.position, snapPoint.transform.rotation * mesh.transform.rotation, mesh.transform.lossyScale);
                Gizmos.DrawMesh(mesh.sharedMesh);
            }
        }

    }
    private void DrawBoundHandles(Vector3[] boundValues, float size = 0.0025f)
    {
#if UNITY_EDITOR
        //Bound Center
        Handles.color = Color.blue;
        Handles.DrawSolidDisc((boundValues[1] + boundValues[2]) / 2, Vector3.forward, size);
        //Bound Min
        Handles.color = Color.green;
        Handles.DrawSolidDisc(boundValues[1], Vector3.forward, size);
        //Bound Max
        Handles.color = Color.red;
        Handles.DrawSolidDisc(boundValues[2], Vector3.forward, size);
#endif
    }
    private Vector3[] CalculateBoundValue(List<Vector3> mins, List<Vector3> maxs)
    {
        if (mins.Count != maxs.Count) return null;

        Vector3 center = Vector3.zero;
        Vector3 min = mins[0];
        Vector3 max = maxs[0];

        for(int i  = 1; i < mins.Count; i++)
        {
            min = new Vector3(mins[i].x < min.x ? mins[i].x : min.x,
                mins[i].y < min.y ? mins[i].y : min.y,
                mins[i].z < min.z ? mins[i].z : min.z);
            max = new Vector3(maxs[i].x > max.x ? maxs[i].x : max.x,
                maxs[i].y > max.y ? maxs[i].y : max.y,
                maxs[i].z > max.z ? maxs[i].z : max.z);
        }
        center = (min + max)/2;
        return new Vector3[] {center, min, max};
    }
    void OnInteractionCompleted(Collider other)
    {
        var manipulator = GetComponent<ObjectManipulator>();
        manipulator.enabled = false;
        manipulator.HostTransform.position = snapPoint.position;
        manipulator.HostTransform.rotation = snapPoint.rotation;

        isTriggerAvailable = false;
        var feedbackPointer = transform.parent.GetComponentInChildren<HighlightSphere>(true);
        feedbackPointer.OnInteractionCompleted();
        isInteractionCompleted = true;
    }

    
    public void FitCollisionVolume()
    {
        Vector3 center = Vector3.zero;
        Vector3 min = Vector3.zero;
        Vector3 max = Vector3.zero;
        var bounds = new Bounds();
        for (int i = 1; i < transform.childCount; i++)
        {
            var partParent = transform.GetChild(i);
            bounds.Encapsulate(partParent.gameObject.GetBoundsWithCenter(ref center, ref min, ref max, transform.position));
        }

        Vector3 boundCenter = transform.parent.GetChild(1).TransformPoint(new Vector3(bounds.center.x, bounds.center.y + bounds.extents.y, bounds.center.z));

        transform.localPosition = boundCenter - transform.position + (max.y - min.y) * 0.5f * Vector3.up * 0.1f;
        transform.localScale = new Vector3(max.x - min.x, (max.y - min.y) * 0.5f, max.z - min.z) * 0.1f;
    }
    public void FitSnapVolume()
    {
#if UNITY_EDITOR
        var meshColliders = transform.GetComponentsInChildren<MeshFilter>(true);

        var minBound = new List<Vector3>();
        var maxBound = new List<Vector3>();

        foreach (var mesh in meshColliders)
        {
            //if (mesh.GetComponent<PartCollisionListener>() == null)
            //{
            //    var color = Color.cyan;
            //    Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
            //    var localPose = transform.InverseTransformPoint(mesh.transform.position);
            //    Gizmos.matrix = Matrix4x4.TRS(localPose + snapPoint.transform.position, snapPoint.transform.rotation * mesh.transform.rotation, mesh.transform.lossyScale);


            //    var bound = mesh.sharedMesh.bounds;
            //    minBound.Add(Gizmos.matrix.MultiplyPoint(bound.min));
            //    maxBound.Add(Gizmos.matrix.MultiplyPoint(bound.max));

            //}

            var color = Color.cyan;
            Gizmos.color = new Color(color.r, color.g, color.b, 0.25f);
            var localPose = transform.InverseTransformPoint(mesh.transform.position);
            Gizmos.matrix = Matrix4x4.TRS(localPose + snapPoint.transform.position, snapPoint.transform.rotation * mesh.transform.rotation, mesh.transform.lossyScale);


            var bound = mesh.sharedMesh.bounds;
            minBound.Add(Gizmos.matrix.MultiplyPoint(bound.min));
            maxBound.Add(Gizmos.matrix.MultiplyPoint(bound.max));
        }

        if (minBound.Count <= 0 || maxBound.Count <= 0) return;

        var boundValues = CalculateBoundValue(minBound, maxBound);

        snapPoint.GetChild(0).position = boundValues[0];
        snapPoint.GetChild(0).localScale = boundValues[2] - boundValues[1];


        var snapVolumeType = snapPoint.GetChild(0).name;
        if (snapVolumeType == "Cylinder")
            snapPoint.GetChild(0).localScale = Vector3.Scale(snapPoint.GetChild(0).localScale, new Vector3(1f, 0.5f, 1f));

#endif
        
    }
    public void ToggleVisibility(bool value)
    {
        visualizeFinalPosition = value;
        snapPoint.gameObject.SetActive(value); 
        var feedbackPointer = transform.parent.GetComponentInChildren<HighlightSphere>(true);
        feedbackPointer.gameObject.SetActive(value);
    }
    public void ToggleForStep(bool value)
    {
        snapPoint.gameObject.SetActive(value);
        isTriggerAvailable = value;
        var feedbackPointer = transform.parent.GetComponentInChildren<HighlightSphere>(true);
        feedbackPointer.gameObject.SetActive(value);
        var manipulator = GetComponent<ObjectManipulator>();
        manipulator.enabled = value;
        if(!value)
        {
            foreach(var collidingPart in collidingParts)
            {
                collidingPart.SetDefaultMaterial();
            }
        }

        detectionFeedback.SetActive(false);
    }

    [ContextMenu("ResetInteraction")]
    public void ResetCollisionInteraction()
    {
        var manipulator = GetComponent<ObjectManipulator>();
        manipulator.enabled = true;
        manipulator.HostTransform.localPosition = defaultLocalPosition;
        manipulator.HostTransform.localRotation = defaultLocalRotation;
        var feedbackPointer = transform.parent.GetComponentInChildren<HighlightSphere>(true);
        feedbackPointer.OnInteractionReset();
        isTriggerAvailable = true;
        isInteractionCompleted = false;
    }

}
