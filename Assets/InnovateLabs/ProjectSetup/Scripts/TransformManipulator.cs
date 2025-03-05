using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using InnovateLabs.Utilities;
public class TransformManipulator : MonoBehaviour
{
   
    [Header("Manipulator")] 
    public Vector2 aspectRatio = new Vector3(1f, 0.04f);
    [Range(0.1f, 3f)] 
    public float scale = 0.5f;

    [Header("Line")]
    [Range(0.005f, 1f)]
    public float lineWidth = 0.005f;


    public Transform display;

    private LineRenderer line;
    private Transform startPoint;
    private Transform endPoint;
    private Transform textAnchor;

    private TextMeshProUGUI[] texts;

    private Vector3 defaultPosition;

    private float previousScale = 0.5f;
    private Vector2 previousAspectRatio = new Vector3(1f, 0.04f);
    private float previousLineWidth = 0.005f;

    public bool isRotationManipulator;


    private AdjustPosition adjustPosition;

    #region Utility
    public void SetHandleScale(Transform value)
    {
        switch (value.name)
        {
            case "Right":
                value.localScale = new Vector3(aspectRatio.x, aspectRatio.y, aspectRatio.y) * scale;
                value.localPosition = new Vector3(value.localScale.x / 2, 0f, 0f);
                return;
            case "Up":
                value.localScale = new Vector3(aspectRatio.y, aspectRatio.x, aspectRatio.y) * scale;
                value.localPosition = new Vector3(0f, value.localScale.y / 2, 0f);
                return;
            case "Forward":
                value.localScale = new Vector3(aspectRatio.y, aspectRatio.y, aspectRatio.x) * scale;
                value.localPosition = new Vector3(0f, 0f, value.localScale.z / 2);
                return;
            case "Free":
                value.localScale = Vector3.one * aspectRatio.y * scale * 2f;
                value.localPosition = Vector3.zero;
                return;
            default:
                return;
        }

    }

    private void SetLineSize()
    {
        var lineRenderer = GetComponentInChildren<LineRenderer>(true);
        lineRenderer.startWidth = lineWidth;
        var hieght = lineWidth * 12f;
        display.GetChild(0).transform.localScale = Vector3.one * hieght;
        display.GetChild(1).transform.localScale = Vector3.one * hieght;
    }
    #endregion Utility

    private void UpdateDisplay()
    {
        if (!isRotationManipulator)
        {
            UpdateStart();
            SetEnd(transform);
            UpdateText(startPoint.position, endPoint.position);
        }
        
    }


    private void UpdateText(Vector3 start, Vector3 final)
    {
        var distance = Vector3.Distance(start, final);
        var tetx = $"{ (distance * 1000).ToString("N0")} mm";
        texts[0].text = tetx;

        texts[1].text = $"X : { ((final.x - start.x) * 1000).ToString("N0")} mm";
        texts[2].text = $"Y : { ((final.y - start.y) * 1000).ToString("N0")} mm";
        texts[3].text = $"Z : { ((final.z - start.z) * 1000).ToString("N0")} mm";

        if(!isRotationManipulator) adjustPosition.UpdateDistance(tetx);
        PositionText(final);
    }
    private void PositionText(Vector3 currentPosition)
    {
        textAnchor.position = Vector3.Lerp(defaultPosition, currentPosition, 0.5f);
        var anchorPose = (textAnchor.position - Camera.main.transform.position).normalized * 0.025f;
        anchorPose = new Vector3(anchorPose.x, anchorPose.y + 0.025f, -anchorPose.z);
        textAnchor.localPosition += anchorPose;//+ Vector3.forward * -0.0025f + Vector3.up * 0.025f;
        textAnchor.LookAt(Camera.main.transform);
        textAnchor.localEulerAngles = new Vector3(0f, textAnchor.localEulerAngles.y + 180f, 0f);
    }

    public void SetStart(Transform transform)
    {
        defaultPosition = transform.position;
        startPoint.position = defaultPosition;
        line.SetPosition(0, startPoint.position);
    }
    private void UpdateStart()
    {
        startPoint.position = defaultPosition;
        line.SetPosition(0, startPoint.position);
    }
    
    private void SetEnd(Transform transform)
    {
        endPoint.position = transform.position;
        line.SetPosition(1, endPoint.position);
    }
    public void ResetManipulator(Transform transform)
    {
        this.transform.position = transform.position;
        SetStart(transform);
        SetEnd(transform);
    }
    private void InitDisplay()
    {
        if(line == null) line = GetComponentInChildren<LineRenderer>(true);
        if(texts == null) texts = display.GetComponentsInChildren<TextMeshProUGUI>(true);
        if(startPoint == null) startPoint = display.GetChild(0);
        if(endPoint == null) endPoint = display.GetChild(1);
        if(textAnchor == null) textAnchor = display.GetChild(2);
        if (!isRotationManipulator) if (adjustPosition == null) adjustPosition = GetComponent<AdjustPosition>();
        SetStart(transform);
        SetEnd(transform);
    }



    #region Monobehaviour
    private void OnValidate()
    {
        if (previousScale != scale || previousAspectRatio != aspectRatio || previousLineWidth != lineWidth)
        {
            foreach (Transform handle in transform.GetChild(0))
            {
                SetHandleScale(handle);
            }
            SetLineSize();
        }
    }

    private void Start()
    {
        InitDisplay();
        if (!isRotationManipulator)
            gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        InitDisplay();
    }

    private void Update()
    {
        UpdateDisplay();
    }
    #endregion Monobehaviour
}
