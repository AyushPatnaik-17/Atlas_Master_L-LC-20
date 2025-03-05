using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animatable : MonoBehaviour
{
    public Transform FromTransform;
    public Transform ToTransform;
    public float LerpTime = 0f;
    public List<GameObject> AssociatedParts = new List<GameObject>();

    public IEnumerator LerpObject()
    {
        float elapsedTime = 0;

        while (elapsedTime < LerpTime)
        {
            transform.position = Vector3.Lerp(FromTransform.position, ToTransform.position, elapsedTime / LerpTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = ToTransform.position;
    }

    public void SetPos()
    {
        transform.position = ToTransform.position;
        Debug.Log($"Setting the position of this object : {this.gameObject.name}");
    }
    //public void CompleteLerp()
    //{
    //    Debug.Log($"Stopping coroutuine on {this.gameObject}");
    //    StopCoroutine(LerpObject());
    //    transform.position = ToTransform.position;
    //}
    public void ToggleAssociatedParts(bool toggle)
    {
        if (AssociatedParts.Count == 0 || AssociatedParts == null) return;
        for (int i = 0; i < AssociatedParts.Count; i++)
        {
            AssociatedParts[i].SetActive(true);
        }
    }
}
