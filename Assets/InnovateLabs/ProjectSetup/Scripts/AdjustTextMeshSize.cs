using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustTextMeshSize : MonoBehaviour
{
    [ContextMenu("Adjust Text Bounds")]
    public void AdjustTextBound()
    {
        var bounds = transform.parent.GetComponent<MeshFilter>().sharedMesh.bounds;
        var rect = GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(bounds.size.x * 0.5f, bounds.size.y * 0.5f);
        Debug.Log($"new bound size : width = {bounds.size.x}, height = {bounds.size.y}" );
    }
}
