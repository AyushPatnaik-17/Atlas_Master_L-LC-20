using UnityEngine;
using InnovateLabs.Utilities;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit;

namespace InnovateLabs.Projects
{
    public class RotateConstraint : MonoBehaviour
    {
        [SerializeField]
        float maxRotation = 25f;
        [SerializeField]
        Vector3 direction;
        public Vector3 startAngles;
        Vector3 finalAngles;

        ObjectManipulator objectManipulator;
        Transform host;

        [HideInInspector]public bool isReached = false;

        float min = 0f, max = 0f;

        bool isNegative = false;
        private void Start()
        {
            objectManipulator = GetComponent<ObjectManipulator>();

            host = objectManipulator.HostTransform;

            startAngles = transform.localRotation.eulerAngles;
            finalAngles = startAngles + maxRotation * direction;

            finalAngles = ConvertTo360(finalAngles);

            min = Vector3.Dot(startAngles, direction);
            max = Vector3.Dot(finalAngles, direction);

            if (maxRotation < 0) isNegative = true;

        }
        // Update is called once per frame
        void Update()
        {
            if (!objectManipulator.enabled) return;

            if (isReached)
            {
                objectManipulator.enabled = false;
                host.transform.localEulerAngles = finalAngles;
                //transform.localEulerAngles *= 1f;
                return;
            }

            var currentAngle = Vector3.Dot(host.transform.localEulerAngles, direction);

            //Debug.Log($"current : {currentAngle}, min : {min}, max : {max}");
            if(max - min  >= 0 )
            {
                if(isNegative)
                {
                    Debug.Log($"condtion 1 : {currentAngle}");
                    if (currentAngle > min && currentAngle < max)
                    {
                        if(currentAngle <max)
                        {
                            currentAngle = max;
                            InteractionCompleted();
                            isReached = true;
                            var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                            host.transform.localRotation = Quaternion.Euler(rotationAngle);
                        }
                        else
                        {
                            currentAngle = min;
                            var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                            host.transform.localRotation = Quaternion.Euler(rotationAngle);
                        }
                    }
                }
                else
                {

                    //if (currentAngle > min && currentAngle < max) return;

                    Debug.Log($"condtion 2 : {currentAngle}, min : {min}, max {max}"); 
                    //currentAngle = Mathf.Clamp(currentAngle, Vector3.Dot(startAngles, direction), Vector3.Dot(finalAngles, direction)); var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                    //host.transform.localRotation = Quaternion.Euler(rotationAngle);
                    //if (currentAngle < min )
                    //{

                        

                    //}
                    //else
                    if (currentAngle >= max && currentAngle <= max+20f)
                    {
                        currentAngle = max;
                        InteractionCompleted();
                        isReached = true;
                        var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                        Debug.Log($"rotation angle insert Value {rotationAngle}");
                        host.transform.localRotation = Quaternion.Euler(rotationAngle);
                    }
                }
            }
            else
            {
                Debug.Log($"condtion 3 : {currentAngle}");
                if (currentAngle > min && currentAngle <= max)
                {
                    if (currentAngle < max)
                    {
                        currentAngle = max;
                        InteractionCompleted();
                        isReached = true;
                        var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                        host.transform.localRotation = Quaternion.Euler(rotationAngle);
                    }
                    else
                    { 
                        currentAngle = min;
                        var rotationAngle = InsertValue(host.transform.localRotation.eulerAngles, direction, currentAngle);
                        host.transform.localRotation = Quaternion.Euler(rotationAngle);
                    }
                }
            }

        }

        private float ConvertTo360(float angle)
        {
            return angle < 0 ? 360 + angle : angle;
        }    

        private Vector3 ConvertTo360(Vector3 angle)
        {
            float x = angle.x < 0 ? 360 + angle.x : angle.x;
            float y = angle.y < 0 ? 360 + angle.y : angle.y;
            float z = angle.z < 0 ? 360 + angle.z : angle.z;
            return new Vector3(x, y, x);
        }    
        private void InteractionCompleted()
        {
            var fp = GetComponentInChildren<HighlightSphere>();
            if (fp == null) return;

            fp.OnInteractionCompleted();
        }
        private Vector3 InsertValue(Vector3 insert, Vector3 direction, float value)
        {
            var x = direction.x != 0 ? value : insert.x;
            var y = direction.y != 0 ? value : insert.y;
            var z = direction.z != 0 ? value : insert.z;

            return insert = new Vector3(x, y, z);
        }


        public void SetPivot(Transform pivot)
        {
            var objManipulator = GetComponent<ObjectManipulator>();
            if (objManipulator != null)
            {
                objManipulator.HostTransform = pivot;
            }
        }

        public void SetDirection(AxisFlags axisFlags)
        {
            var rotationAxisContraint = GetComponent<RotationAxisConstraint>();
            if (rotationAxisContraint != null)
            {
                switch (axisFlags)
                {
                    case AxisFlags.XAxis:
                        rotationAxisContraint.ConstraintOnRotation = AxisFlags.YAxis | AxisFlags.ZAxis;
                        direction = Vector3.right;//transform.right;
                        break;
                    case AxisFlags.YAxis:
                        rotationAxisContraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.ZAxis;
                        direction = Vector3.up;//transform.up;
                        break;
                    case AxisFlags.ZAxis:
                        rotationAxisContraint.ConstraintOnRotation = AxisFlags.XAxis | AxisFlags.YAxis;
                        direction = Vector3.forward;// transform.forward;
                        break;
                    default:
                        Debug.LogError($"Wrong direction chosen, select from X, Y, Z Axis : {axisFlags}");
                        break;
                }
            }
        }


        public Vector3 GetDirection()
        {
            return direction;
        }
        public void SetFinalRotation(Vector3 finalRotation)
        {
            finalAngles = finalRotation;
        }

        public void SetFinalValue(float value)
        {
            maxRotation = value;
        }
    }
}
