using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultElement : MonoBehaviour
{
    public TextMeshProUGUI stepNo;
    public TextMeshProUGUI interactionCount;
    public TextMeshProUGUI totalInteractions;

    public void UpdateResult(StepResult stepResult)
    {
        this.stepNo.text = stepResult.stepNo.ToString("D2");
        this.interactionCount.text = stepResult.interactionCount.ToString("D2");
        this.totalInteractions.text = "/" + stepResult.totalInteractions.ToString("D2");
    }
}


[System.Serializable]
public class StepResult
{
    public int stepNo = 0;
    public int interactionCount = 0;
    public int totalInteractions = 0;

    public StepResult(int stepNo, int interactionCount, int totalInteractions)
    {
        this.stepNo = stepNo;
        this.interactionCount=interactionCount;
        this.totalInteractions = totalInteractions;
    }

    
}
