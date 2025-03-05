using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandCollisionListener : MonoBehaviour
{
    private HandCollisionChecker collisionChecker;
    void Start()
    {
        collisionChecker = GetComponentInParent<HandCollisionChecker>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<HandCollisionChecker>() == collisionChecker
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<PartCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<HandCollisionChecker>() == collisionChecker
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<PartCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<HandCollisionChecker>() == collisionChecker
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<PartCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerExit(other);
    }
}
