using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using Microsoft.MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

public class AdjustPosition : MonoBehaviour
{
    public PressableButton[] actions;
    public Follow follow;
    public TextMeshProUGUI distance;
    private Vector3 startPosition;
    Vector3 offset;
    bool isActive = true;
    bool isFollowing = true;
    Vector3 defaultFollowPosition = Vector3.zero;
    public UpdateButtonData floatingState;

    [ContextMenu("Place")]
    public void Place()
    {
        var objects = FindObjectOfType<TapToPlaceManager>().GetObjects();
        offset = transform.position - startPosition;
        foreach (var obj in objects)
        {
            Debug.Log($"setting offset : {obj.name}");
            obj.position += offset;
        }

        SetStartTransform(transform);

        //gameObject.SetActive(false);
    }

    public void SetStartTransform(Transform transform)
    {
        startPosition = transform.position;
        GetComponent<TransformManipulator>().SetStart(transform);
    }

    public void ResetManipulator(Transform resetTransform)
    {
        GetComponent<TransformManipulator>().ResetManipulator(resetTransform);
        startPosition = resetTransform.position;
        transform.rotation = resetTransform.rotation;
    }
    private void ResetToMarker()
    {
        var resetTransform = FindObjectOfType<TapToPlaceManager>();
        ResetManipulator(resetTransform.transform);
        resetTransform.SetTransformOffset();
    }


    private void Awake()
    {
        actions[0].OnClicked.AddListener(ToggleManipulator);
        actions[1].OnClicked.AddListener(Place);
        actions[2].OnClicked.AddListener(ResetToMarker);
        actions[3].OnClicked.AddListener(ToggleFollow);
        actions[4].OnClicked.AddListener(CloseManipulator);
        defaultFollowPosition = follow.transform.localPosition;
        floatingState.Init();
        //CloseManipulator();
    }

    private void CloseManipulator()
    {
        gameObject.SetActive(false);
    }

    private void ToggleFollow()
    {
        isFollowing = !isFollowing;
        follow.IgnoreDistanceClamp = !isFollowing;
        floatingState.SetState(0);
        if(!isFollowing)
        {
            follow.transform.localPosition = defaultFollowPosition;
            floatingState.SetState(1);
        }
    }

    private void ToggleManipulator()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        for(int i = 0; i < TapToPlaceManager.colliders.Count; i++)
        {
            TapToPlaceManager.colliders[i].enabled = !gameObject.activeSelf;
        }
    }

    public void UpdateDistance(string dis)
    {
        distance.text = $"<size=8>{dis}</size><size=6>\n<alpha=#88>from previous position</size>";
    }

    private void Update()
    {
        
    }
}
