using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RotationDisplayBehaviour : MonoBehaviour
{

    public TextMeshProUGUI anglesText;
    private PivotInteractable pivotInteractable;
    private RotationInteraction rotationManager;

    private void Awake()
    {
        rotationManager = transform.GetComponentInParent<RotationInteraction>(true);
        pivotInteractable = rotationManager.transform.GetComponentInChildren<PivotInteractable>(true);
        pivotInteractable.E_pivInteractable += UpdateRotationDisplay;
        rotationManager.E_rotationInteraction += UpdateRotationDisplay;
    }
    private void LateUpdate()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }

    public void UpdateRotationDisplay()
    {
        var angles = pivotInteractable.CustomTargetTransform.localRotation.eulerAngles;
        switch (pivotInteractable.AxisConstraintMode)
        {
            case PivotInteractable.ConstrainMode.X:
                anglesText.text = $"X : {(angles.x < 180 ? angles.x : angles.x - 360):F2}°";
                break; 
            case PivotInteractable.ConstrainMode.Y:
                anglesText.text = $"Y : {(angles.y < 180 ? angles.y : angles.y - 360):F2}°";
                break; 
            case PivotInteractable.ConstrainMode.Z:
                anglesText.text = $"Z : {(angles.z < 180 ? angles.z : angles.z - 360):F2}°";
                break;
        }
    }
}
