using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;
using System.Linq;

/// <summary>
/// Models a lever-like pivot interaction around a specified axis.
/// </summary>
public class PivotInteractable : MRTKBaseInteractable
{
    // Will constrain pivoting to the specified axis.
    // e.g., mode = X => only spin around X axis.
    public enum ConstrainMode
    {
        Free,
        X,
        Y,
        Z
    }

    [field: SerializeField, Tooltip("Which axis to constrain pivoting to. (e.g., Y results in only spinning around Y axis")]
    public ConstrainMode AxisConstraintMode { get; set; } = ConstrainMode.Free;

    [field: SerializeField, Tooltip("If set, the script manipulates this transform instead of the transform it's attached to.")]
    public Transform CustomTargetTransform { get; set; } = null;

    [field: SerializeField, Tooltip("The custom pivot point to use.")]
    public Transform CustomPivot { get; set; } = null;

    [SerializeField]
    [Tooltip("Which types of interactions are allowed to manipulate this object?")]
    private InteractionFlags allowedInteractionTypes = InteractionFlags.Near | InteractionFlags.Ray | InteractionFlags.Gaze | InteractionFlags.Generic;
    public InteractionFlags AllowedInteractionTypes
    {
        get => allowedInteractionTypes;
        set => allowedInteractionTypes = value;
    }

    private InteractionFlags GetInteractionFlagsFromInteractor(IXRInteractor interactor)
    {
        InteractionFlags flags = InteractionFlags.None;
        if (interactor is IGrabInteractor)
        {
            flags |= InteractionFlags.Near;
        }
        if (interactor is IRayInteractor)
        {
            flags |= InteractionFlags.Ray;
        }
        if (interactor is IGazeInteractor || interactor is IGazePinchInteractor)
        {
            flags |= InteractionFlags.Gaze;
        }

        // If none have been set, default to generic.
        if (flags == InteractionFlags.None)
        {
            flags = InteractionFlags.Generic;
        }

        return flags;
    }

    // Which pivot are we actually using?
    private Transform pivot => CustomPivot != null ? CustomPivot : target;

    // Which target are we actually using?
    private Transform target => CustomTargetTransform != null ? CustomTargetTransform : transform;

    // The vector from the pivot to the interaction point at the start of the interaction.
    private Vector3 fromVec;

    // The rotation of the target at the start of the interaction.
    private Quaternion fromRot;

    // The vector from the pivot to the interaction point at the start of the interaction.
    private Vector3 pivotOffset;

    //[Range(0,90)]
    public float minAngle = 0;
    //[Range(0,90)]
    public float maxAngle = 0;

    public bool b_shouldRestrict = false;
    public bool b_shouldPlaySound = false;
    public AudioSource fbSound;

    public delegate void D_pivInteractable();
    public event D_pivInteractable E_pivInteractable;

    private RotationInteraction rotationManager;

    protected override void Awake()
    {
        // We're not poke-able.
        DisableInteractorType(typeof(IPokeInteractor));
        rotationManager = this.GetComponentInParent<RotationInteraction>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        RecordFromVector();
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);
        //Debug.Log($"min angle pivInt : {minAngle}, max : {maxAngle}")
        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
        {
            Vector3 toVec = ProjectToConstraintPlane(interactorsSelecting[0].GetAttachTransform(this).position - pivot.position);

            // If we've rotated far enough, let's reset our fromVec to avoid
            // wrap-around-the-sphere error.
            
            if (Vector3.Angle(fromVec, toVec) > 45)
            {
                RecordFromVector();
            }
            Quaternion rot = Quaternion.FromToRotation(fromVec, toVec);

            // If pivot == transform, then this pivot offset math is no-op.
            target.position = Vector3.Lerp(target.position, rot * (pivotOffset) + pivot.position, 0.5f);


            target.localRotation = Quaternion.Slerp(target.localRotation, rot * fromRot, 0.5f);

            if (b_shouldRestrict)
                ClampRotation();
            if (!b_shouldRestrict && b_shouldPlaySound)
                PlaySoundAtAngles();

            E_pivInteractable.Invoke();

        }
    }

    private void PlaySoundAtAngles(float rot = 0f)
    {

        switch (AxisConstraintMode)
        {
            case ConstrainMode.X:
                rot = target.localRotation.eulerAngles.x;
                break;
            case ConstrainMode.Y:
                rot = target.localRotation.eulerAngles.y;
                break;
            case ConstrainMode.Z:
                rot = target.localRotation.eulerAngles.z;
                break;
            default:
                return;
        }

        if (Mathf.Round(rot) == Mathf.Round(maxAngle) || Mathf.Round(rot) == Mathf.Round(minAngle))
        {

            fbSound.Stop();
            fbSound.Play();
            rotationManager.InteractionCompleted();
        }
    }

    private void RecordFromVector()
    {
        fromVec = ProjectToConstraintPlane(interactorsSelecting[0].GetAttachTransform(this).position - pivot.position);
        fromRot = target.localRotation;
        pivotOffset = target.position - pivot.position;
    }

    private void ClampRotation()
    {
        switch (AxisConstraintMode)
        {
            case ConstrainMode.X:
                #region old way
                /*var rotX = target.localRotation.eulerAngles.x;

                if(rotX > maxAngle && rotX < minAngle)
                {
                    if(target.localRotation.eulerAngles.x > 180f)
                    {
                        rotX = Mathf.Clamp(rotX, minAngle, 360f);
                    }
                    if (target.localRotation.eulerAngles.x < 180f)
                    {
                        rotX = Mathf.Clamp(rotX, 0f, maxAngle);
                    }
                }
                target.localRotation = Quaternion.Euler(new Vector3(rotX, 0f,0f));*/
                #endregion
                ConstrainRotation("X");
                break;
            case ConstrainMode.Y:
                ConstrainRotation("Y");
                break;
            case ConstrainMode.Z:
                ConstrainRotation("Z");
                break;
            default:
                break;
        }
    }

    public void ConstrainRotation(string axis)
    {
        float rot = 0f;

        switch (axis)
        {
            case "X":
                rot = target.localRotation.eulerAngles.x;
                break;
            case "Y":
                rot = target.localRotation.eulerAngles.y;
                break;
            case "Z":
                rot = target.localRotation.eulerAngles.z;
                break;
            default:
                return; // Invalid axis
        }

        if (rot > maxAngle && rot < minAngle)
        {
            if (b_shouldPlaySound)
            {
                PlaySoundAtAngles(rot);
            }

            if (rot > 180f)
            {
                rot = Mathf.Clamp(rot, minAngle, 360f);
            }
            if (rot < 180f)
            {
                rot = Mathf.Clamp(rot, 0f, maxAngle);
            }

            rotationManager.InteractionCompleted();
        }

        switch (axis)
        {
            case "X":
                target.localRotation = Quaternion.Euler(rot * Vector3.right);
                break;
            case "Y":
                target.localRotation = Quaternion.Euler(rot * Vector3.up);
                break;
            case "Z":
                target.localRotation = Quaternion.Euler(rot * Vector3.forward);
                break;
        }
    }


    private Vector3 ProjectToConstraintPlane(Vector3 vec)
    {
        
        switch (AxisConstraintMode)
        {
            case ConstrainMode.Free: return vec;
            case ConstrainMode.X: return new Vector3(0, vec.y, vec.z);
            case ConstrainMode.Y: return new Vector3(vec.x, 0, vec.z);
            case ConstrainMode.Z: return new Vector3(vec.x, vec.y, 0);
            default: return vec;
        }
    }

    public override bool IsSelectableBy(IXRSelectInteractor interactor)
    {
        return base.IsSelectableBy(interactor) && AllowedInteractionTypes.IsMaskSet(GetInteractionFlagsFromInteractor(interactor));
    }
}