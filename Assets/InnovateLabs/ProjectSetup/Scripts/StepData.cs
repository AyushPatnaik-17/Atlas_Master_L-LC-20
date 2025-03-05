using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Microsoft.MixedReality.Toolkit;

using InnovateLabs.Utilities;


#if UNITY_EDITOR

using UnityEditor;

#endif

namespace InnovateLabs.Projects
{
    [CreateAssetMenu(fileName = "Step Data", menuName = "Innovate Labs/Step Data", order = 31)]
    public class StepData : ScriptableObject
    {
        private static string prefabPath = "Assets/InnovateLabs/ProjectSetup/Prefabs/FeedbackPointer.prefab";

        public string ID = Guid.NewGuid().ToString().ToUpper();

        public int stepNo;
        public int interactionsInStep;
        public string stepDescription;
        public InteractionTypes interactionTypes;

        public List<string> touchInteractions = new List<string>();

        public string movementPart;
        public string movementPivot;
        public AxisFlags movementDirection;
        public float distance;
        public float distanceToLocal;

        //public string grabPart;
        //public string rotationPivot;
        //public AxisFlags rotationAxis;
        //public float rotationAngle;
        public string rotationManager;


        public string collisionManager;
        //public string collisionPart;

#if UNITY_EDITOR

        #region Touch Interaction
        public void AddTouchInteraction(GameObject touchPart)
        {
            var fp = touchPart.GetComponentInChildren<HighlightSphere>();
            if(fp == null)
            {
                InstantiateFeedbackPointer(touchPart);
            }
            touchInteractions.Add(touchPart.GetComponent<GUID>().GetGUID());
        }

        public void RemoveTouchInteraction(GameObject touch)
        {
            var guid = touch.GetComponent<GUID>();
            if(guid != null)
            {
                var id = guid.GetGUID();
                if(touchInteractions.Contains(id))
                {
                    var hs = touch.GetComponentInChildren<HighlightSphere>();

                    if (hs != null)
                    {
                        DestroyImmediate(hs.gameObject);
                    }
                    touchInteractions.Remove(id);
                }
                else
                {
                    Debug.LogError($"Do not contain '{touch.name}' having id '{id}' as touchable interaction");
                }
            }
            else
            {
                Debug.LogError($"'{touch.name}' is not  a touchable interaction");
            }
        }

        public bool ContainsTouchInteraction(GameObject interactable)
        {
            return touchInteractions.Contains(interactable.GetComponent<GUID>().GetGUID());
        }

        #endregion Touch Interaction

        #region Translate Interaction

        
        public Vector3 TranslatedPosition(Transform part)
        {
            var translatedPos = DistanceInDirection(movementDirection, part);
            return translatedPos;
        }

        public Vector3 RotatedPosition(Transform part)
        {
            var rotatedAngle = Vector3.zero;
            return rotatedAngle;
        }

        public void AddTranslatePivot(GameObject pivot)
        {
            movementPivot = pivot.GetComponent<GUID>().GetGUID();
        }

        public void RemoveTranslatePivot()
        {
            movementPivot = "";
        }

        #endregion Translate Interaction

        #region Rotation Interaction

        /*public void AddRotationPivot(GameObject pivot)
        {
            rotationPivot = pivot.GetComponent<GUID>().GetGUID();
        }

        public void RemoveRotationPivot()
        {
            rotationPivot = "";
        }*/



        #endregion Rotation Interaction

        public GameObject InstantiateFeedbackPointer(GameObject touchPart)
        {
            Debug.Log("Instantiating Feedback Pointer");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            var feedbackPointer = Instantiate(prefab, touchPart.transform.position, Quaternion.identity);
            feedbackPointer.transform.parent = touchPart.transform;

            return feedbackPointer;
        }

        public Vector3 DistanceInDirection(AxisFlags axisFlags, Transform part)
        {
            Vector3 dir = Vector3.zero;
            switch(axisFlags)
            {
                case AxisFlags.XAxis:
                    dir = part.position + part.right * distanceToLocal;
                    break;
                case AxisFlags.YAxis:
                    //dir = new Vector3(part.position.x, part.position.y, part.position.y) + new
                    dir = part.position + part.up * distanceToLocal;
                    //Debug.Log($"Distance in direction {dir} part {part.position} up {part.up} distanceToLocal {distanceToLocal}");
                    break;
                case AxisFlags.ZAxis:
                    dir = part.position + part.forward * distanceToLocal;
                    break;
                default:
                    dir = part.position + part.right * distanceToLocal;
                    break;
            }
            //Debug.Log($"during return {dir}");
            return dir;
        }

        #region Make This Dirty

        public void OnValidate()
        {
            MakeDirty();
        }
        public void MakeDirty()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            this.SetDirty();
#pragma warning restore CS0618 // Type or member is obsolete
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        #endregion Make This Dirty
#endif
    }

}
