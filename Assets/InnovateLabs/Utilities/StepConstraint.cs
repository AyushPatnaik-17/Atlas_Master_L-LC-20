using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Microsoft.MixedReality.Toolkit.SpatialManipulation
{
    //public class StepConstraint : TransformConstraint
    //{
    //    public float stepValue = 1f;

    //    public override TransformFlags ConstraintType => TransformFlags.Move;

    //    public override void ApplyConstraint(ref MixedRealityTransform transform)
    //    {
    //        transform.Position = Step(transform.Position);
    //    }

    //    private Vector3 Step(Vector3 position)
    //    {
    //        position.x = Mathf.Round(position.x / stepValue) * stepValue;
    //        position.y = Mathf.Round(position.y / stepValue) * stepValue;
    //        position.z = Mathf.Round(position.z / stepValue) * stepValue;

    //        return position;
    //    }
    //}

    public class StepConstraint : TransformConstraint
    {
        public float stepValue = 1f;

        private ObjectManipulator manipulator;

        private void Awake()
        {

        }



        public override TransformFlags ConstraintType => TransformFlags.Move;

        public override void ApplyConstraint(ref MixedRealityTransform transform)
        {
            transform.Position = Step(transform.Position);
        }

        private Vector3 Step(Vector3 position)
        {
            position.x = Mathf.Round(position.x / stepValue) * stepValue;
            position.y = Mathf.Round(position.y / stepValue) * stepValue;
            position.z = Mathf.Round(position.z / stepValue) * stepValue;

            return position;
        }


        
    }

}