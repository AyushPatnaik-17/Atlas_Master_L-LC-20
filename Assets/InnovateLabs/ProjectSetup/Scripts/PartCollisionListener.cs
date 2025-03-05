using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartCollisionListener : MonoBehaviour
{
    private PartsCollisionChecker collisionChecker;
    void Start()
    {
        collisionChecker = GetComponentInParent<PartsCollisionChecker>(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PartsCollisionChecker>() == collisionChecker 
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<HandCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<PartsCollisionChecker>() == collisionChecker
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<HandCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PartsCollisionChecker>() == collisionChecker
            || other.GetComponent<MeshRenderer>() == null
            || other.GetComponent<SpatialMesh>() != null
            || other.GetComponent<HandCollisionListener>() != null)
            return;
        else
            collisionChecker.TriggerExit(other);
    }
}

