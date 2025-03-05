using InnovateLabs.Projects;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InnovateLabs.Projects
{
    public class HighlightSphere : MonoBehaviour
    {

        Vector3 minScale;

        [SerializeField]
        private float maxScale;

        [SerializeField]
        private bool repeatable;

        [SerializeField]
        private float speed = 2f;

        [SerializeField]
        private float duration = 1.25f;

        [HideInInspector]
        public bool isMoving = false;

        StatefulInteractable statefulInteractable;
        [HideInInspector] public bool isInteractionComplete = false;
        void Awake()
        {
            minScale = transform.localScale;

            statefulInteractable = GetComponentInParent<StatefulInteractable>();
            if (statefulInteractable == null) return;
            
            statefulInteractable.IsPokeHovered.OnEntered.AddListener(
                delegate
                {
                    if (statefulInteractable.enabled)
                    {
                        OnInteractionCompleted();
                    }
                });
        }


        private void OnEnable()
        {
            StartCoroutine(StartPulse()); 
            if (isInteractionComplete)
            {
                if(statefulInteractable != null)
                {
                    statefulInteractable.enabled = false;
                }
            } 
                
        }


        private void OnDisable()
        {
            StopCoroutine(StartPulse());
        }
        private IEnumerator StartPulse()
        {
            while (repeatable)
            {
                yield return RepeatLerp(minScale, maxScale * Vector3.one, duration);
                yield return RepeatLerp(maxScale * Vector3.one, minScale, duration);
            }
        }
        public IEnumerator RepeatLerp(Vector3 a, Vector3 b, float time)
        {
            float i = 0.0f;
            float rate = (1.0f / time) * speed;
            while (i < 1.0f)
            {
                i += Time.deltaTime * rate;
                transform.localScale = Vector3.Lerp(a, b, i);
                yield return null;

            }

        }



        #region Rim Color Changing Script

        private void ChangeRimColor(Material rimColor)
        {
            GetComponentInChildren<MeshRenderer>().sharedMaterial = rimColor;
        }

        public void SetHighlight()
        {
            ChangeRimColor(RimColorManager.instance.highlightColor);
        }
        private void SetCorrect()
        {
            ChangeRimColor(RimColorManager.instance.CorrectColor);
        }
        private void SetWrongColor()
        {
            ChangeRimColor(RimColorManager.instance.WrongColor);
        }


        public void OnLastStep()
        {
            if (isInteractionComplete == false)
            {
                SetWrongColor();
                //Debug.Log("OnLastStep");
            }
                
        }
        public void OnInteractionCompleted()
        {
            SetCorrect();
            StepManager.instance.InteractedAudio();
            StepManager.instance.AddResult();
            StepManager.instance.UpdateStepDescription();
            if(statefulInteractable != null)
            {
                statefulInteractable.enabled = false;
            }
            isInteractionComplete = true;
        }
        
        public void OnInteractionCompleted(int stepCount)
        {
            SetCorrect();
            StepManager.instance.AddResult(stepCount);
            StepManager.instance.UpdateStepDescription();
            if(statefulInteractable != null)
            {
                statefulInteractable.enabled = false;
            }
            isInteractionComplete = true;
        }


        public void OnInteractionReset()
        {
            if (!isInteractionComplete) return;
            SetHighlight();
            StepManager.instance.RemoveResult();
            if (statefulInteractable != null) statefulInteractable.enabled = true;
            isInteractionComplete = false;
        }
        #endregion Rim Color Changing Script

    }

}

