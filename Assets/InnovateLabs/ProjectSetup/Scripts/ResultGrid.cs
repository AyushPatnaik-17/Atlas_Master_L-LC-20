using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class ResultGrid : MonoBehaviour
{
    public GameObject resultElementPrefab;
    public GameObject resultHolder;
    public void ClearGrid()
    {
        var childCount = transform.childCount;
        for(int i = childCount-1; i >=0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
    public void CreateGrid(List<StepResult> stepResults)
    {
        resultHolder.SetActive(true);
        if (transform.childCount != 0) ClearGrid();

        foreach(var result in stepResults)
        {
            var resultElement = Instantiate(resultElementPrefab, this.transform);
            resultElement.transform.localScale = Vector3.one;
            resultElement.name = $"Step - {result.stepNo}";
            resultElement.GetComponent<ResultElement>().UpdateResult(result);
        }
    }
    public void HideResult()
    {
        resultHolder.SetActive(false);
    }
}
