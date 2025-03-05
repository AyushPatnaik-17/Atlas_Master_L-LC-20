using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationDetector : MonoBehaviour
{
    public float currentAngle = 0;
    public int number = 0;
    private Quaternion previousRotation;

    private void Start()
    {
        previousRotation = transform.rotation;  
    }
    // Update is called once per frame
    void Update()
    {
        Quaternion currentRotation = transform.rotation;
        float angleChange = Quaternion.Angle(previousRotation, currentRotation);
        if(angleChange == 0)
        {
            currentAngle += 0f;
        }
        else if (angleChange > 0)
        {
            currentAngle += currentRotation.eulerAngles.y;
        }
        else if(angleChange < 0)
        {
            currentAngle -= currentRotation.eulerAngles.y;
        }

        number = Mathf.FloorToInt(currentAngle/360f);

        Debug.Log($"current Angle : {currentAngle}, number of rotations : {number}");
    }
}
