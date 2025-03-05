using UnityEngine;
using InnovateLabs.Utilities;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit;
using System.Linq;
using System.Collections.Generic;

namespace InnovateLabs.Projects
{
    public class MoveConstraint : MonoBehaviour
    {
        [ReadValueAtInspector]
        [SerializeField]
        Vector3 direction;

        [HideInInspector]public Vector3 startPosition;

        [ReadValueAtInspector]
        [SerializeField]
        public Vector3 finalPosition;

        [ReadValueAtInspector]
        public float inputDistanceValue;

        Transform host;

        float distanceMoved = 0;

        LengthUnit modelUnit;
        [HideInInspector]public bool reached = false;

        //[ReadValueAtInspector]
        public bool showEndPosition;

        private Vector3 previousHostPosition = Vector3.zero;

        ObjectManipulator objectManipulator;

        #region Unity Method

        private void Start()
        {
            objectManipulator = GetComponent<ObjectManipulator>();

            host = objectManipulator.HostTransform;

            startPosition = host.localPosition;

            distanceMoved = Vector3.Distance(finalPosition, startPosition);

            modelUnit = GetComponentInParent<ModelInfo>().GetCurrentUnit();

            previousHostPosition = startPosition;

        }

        private void Update()
        {
            if (!objectManipulator.enabled) return;

            if (reached)
            {
                objectManipulator.enabled = false;
                return;
            }
            var currentDistance = Vector3.Distance(startPosition, host.localPosition);

            var moveDirection = Vector3.Angle(finalPosition - startPosition, host.localPosition - startPosition);
            moveDirection /= 90f;

            if (currentDistance > distanceMoved)
            {
                objectManipulator.enabled = false;
                host.localPosition = finalPosition;
                InteractionCompleted();
                reached = true;
                return;
            }

            if (!(moveDirection >= 0 && moveDirection <= 1))
            {
                host.localPosition = startPosition;
                return;
            }

        }

        private void OnDrawGizmos()
        {
            if (!showEndPosition) return;

            DrawTranslateEndPositionMesh();
        }

        #endregion Unity Method

        private void InteractionCompleted()
        {
            var fp = GetComponentInChildren<HighlightSphere>();
            if (fp == null) return;

            fp.OnInteractionCompleted();
        }
        public void SetFinalPosition(Vector3 finalPosition)
        {
            this.finalPosition = transform.InverseTransformPoint(finalPosition);
        }
        public void SetDirection(AxisFlags axisFlags)
        {
            var moveAxisConstraint = GetComponent<MoveAxisConstraint>();
            if (moveAxisConstraint != null)
            {
                switch (axisFlags)
                {
                    case AxisFlags.XAxis:
                        moveAxisConstraint.ConstraintOnMovement = AxisFlags.YAxis | AxisFlags.ZAxis;
                        direction = Vector3.right;//transform.right;
                        break;
                    case AxisFlags.YAxis:
                        moveAxisConstraint.ConstraintOnMovement = AxisFlags.XAxis | AxisFlags.ZAxis;
                        direction = Vector3.up;//transform.up;
                        break;
                    case AxisFlags.ZAxis:
                        moveAxisConstraint.ConstraintOnMovement = AxisFlags.XAxis | AxisFlags.YAxis;
                        direction = Vector3.forward;// transform.forward;
                        break;
                    default:
                        moveAxisConstraint.ConstraintOnMovement = AxisFlags.YAxis | AxisFlags.ZAxis;
                        direction = Vector3.right;//transform.right;
                        break;
                }
            }
        }
        public void SetPivot(Transform pivot)
        {
            var objManipulator = GetComponent<ObjectManipulator>();
            if (objManipulator != null)
            {
                objManipulator.HostTransform = pivot;
            }
        }
        private Vector3 Direction(Vector3 direction)
        {
            var dir = Vector3.zero;
            if (direction == Vector3.right)
            {
                dir = transform.right;
            }
            else if (direction == Vector3.up)
            {
                dir = transform.up;
                Debug.Log($"direction : {dir} transform : {transform.up} and Vector {Vector3.up}");
            }
            else if (direction == Vector3.forward)
            {
                dir = transform.forward;
            }
            else
            {

            }
            return dir;
        }

        private void DrawTranslateEndPositionMesh()
        {
            var host = GetComponent<ObjectManipulator>().HostTransform;

            List<MeshFilter> meshes = new List<MeshFilter>();
            meshes = host.GetComponentsInChildren<MeshFilter>().ToList().Where(x => x.gameObject.name != "Sphere").ToList();

            var meshInhost = host.GetComponent<MeshFilter>();
            if (meshInhost != null)
            {
                meshes.Add(meshInhost);
            }

            foreach (var mesh in meshes)
            {
                var startPosition = host.position;
                var offset = mesh.transform.position - startPosition;

                Vector3 worldPosition = host.parent.TransformPoint(finalPosition) + offset;

                Quaternion rotation = mesh.transform.rotation;
                Vector3 scale = new Vector3(mesh.transform.lossyScale.x, mesh.transform.lossyScale.y, mesh.transform.lossyScale.z);

                Gizmos.color = new Color(135f / 255f, 206f / 255f, 235f / 255f);
                Gizmos.DrawWireMesh(mesh.sharedMesh, worldPosition, rotation, scale);

            }
            InnovateGizmos.DrawText($"{host.name}\n{inputDistanceValue}mm", host.parent.TransformPoint(finalPosition) + Vector3.left * 0.010f, Color.blue, Vector2.left, 0.25f);
        }
    }
}
