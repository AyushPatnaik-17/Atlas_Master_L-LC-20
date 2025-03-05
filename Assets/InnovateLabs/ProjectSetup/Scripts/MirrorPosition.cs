using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorPosition : MonoBehaviour
{
    Vector3 offset = Vector3.zero;
    Quaternion offsetRot = Quaternion.identity;

    public ObjectManipulator objectManipulator;
    public PivotInteractable pivotInteractable;

    public enum WhatToMirror
    {
        objectManipulator,
        pivotInteractable
    };
    public WhatToMirror whatToMirror = WhatToMirror.objectManipulator;

    Transform hostTransform;

    Vector3 startHostPosition;
    Vector3 startFpPosition;
    Vector3 startHostScale;
    Vector3 startFpScale;

    Quaternion startHostRotation;
    Quaternion startFpRotation;

    Matrix4x4 hostMatrix;

    void Start()
    {
        
        if(whatToMirror == WhatToMirror.objectManipulator)
        {
            hostTransform = objectManipulator.HostTransform;
            pivotInteractable = null;
        }
        else if(whatToMirror == WhatToMirror.pivotInteractable)
        {
            hostTransform = pivotInteractable.CustomTargetTransform;
            objectManipulator = null;
        }

        startHostPosition = hostTransform.position;
        startHostRotation = hostTransform.rotation;
        startHostScale = hostTransform.lossyScale;
        
        startFpPosition = transform.position;
        startFpRotation = transform.rotation;
        startFpScale = transform.lossyScale;

        startFpPosition = DivideVectors(Quaternion.Inverse(hostTransform.rotation) * (startFpPosition - startHostPosition), startHostScale);
    }

    void Update()
    {
        hostMatrix = Matrix4x4.TRS(hostTransform.position, hostTransform.rotation, hostTransform.lossyScale);

        transform.position = hostMatrix.MultiplyPoint3x4(startFpPosition);

        transform.rotation = (hostTransform.rotation * Quaternion.Inverse(startHostRotation)) * startFpRotation;
    }

    Vector3 DivideVectors(Vector3 num, Vector3 den)
    {

        return new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);

    }
}
