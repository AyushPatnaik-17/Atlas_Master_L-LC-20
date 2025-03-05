using UnityEngine;
using UnityEditor;
using InnovateLabs.Projects;

namespace InnovateLabs.Utilities
{
    public static class FeedbackPointer
    {
        //public static void MovePointer(Transform pointer, Event current)
        //{
        //    var ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
        //    Physics.Raycast(ray, out var rayCastHit, Mathf.Infinity);
        //    if (rayCastHit.collider)
        //    {
        //        var collider = rayCastHit.collider.gameObject.GetComponent<MeshRenderer>();
        //        if (collider != null)
        //        {
        //            pointer.position = rayCastHit.point;
        //            if (current.type == EventType.MouseDown && current.button == 0)
        //            {
        //                pointer.GetComponent<HighlightSphere>().isMoving = false;
        //            }
        //        }
        //    }

        //}

        public static void MovePointer(Transform pointer, Event current, bool isCollision = true)
        {
            var ray = HandleUtility.GUIPointToWorldRay(current.mousePosition);
            Physics.Raycast(ray, out var rayCastHit, Mathf.Infinity);
            if (rayCastHit.collider)
            {
                if(isCollision)
                {
                    var collision = rayCastHit.collider.gameObject.GetComponentInParent<PartsCollisionChecker>(true);
                    if (collision != null)
                    {
                        pointer.position = rayCastHit.point;
                        if (current.type == EventType.MouseDown && current.button == 0)
                        {
                            pointer.GetComponent<HighlightSphere>().isMoving = false;
                        }
                    } 
                }
                else
                {
                    var collider = rayCastHit.collider.gameObject.GetComponent<MeshRenderer>();
                    if (collider != null)
                    {
                        pointer.position = rayCastHit.point;
                        if (current.type == EventType.MouseDown && current.button == 0)
                        {
                            pointer.GetComponent<HighlightSphere>().isMoving = false;
                        }
                    }
                }
                
            }

        }
    }
}
