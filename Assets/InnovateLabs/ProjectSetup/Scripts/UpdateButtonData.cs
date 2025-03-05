using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpdateButtonData : MonoBehaviour
{

    Image imageIcon;
    FontIconSelector fontIconSelector;
    TextMeshProUGUI labelItem;
    TextMeshProUGUI buttonText;

    public List<ButtonStateData> states = new List<ButtonStateData>();

    public void Init()
    {
        imageIcon = gameObject.GetComponentInChildren<Image>(true);
        imageIcon.gameObject.SetActive(false);
        fontIconSelector = gameObject.GetComponentInChildren<FontIconSelector>(true);
        fontIconSelector.gameObject.SetActive(false);
        labelItem = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Label");
        labelItem.gameObject.SetActive(false);
        buttonText = gameObject.GetComponentsInChildren<TextMeshProUGUI>(true).ToList().Find(x => x.gameObject.name == "Text");
        buttonText.gameObject.SetActive(false);

        SetState(0);
    }
    private void SetButtonData(ButtonStateData buttonStateData)
    {

        if (buttonStateData.spriteIcon != null)
        {
            imageIcon.sprite = buttonStateData.spriteIcon;
            imageIcon.gameObject.SetActive(true);
        }
        if (!string.IsNullOrEmpty(buttonStateData.fontIcon))
        {
            fontIconSelector.CurrentIconName = buttonStateData.fontIcon;
            fontIconSelector.gameObject.SetActive(true);
        }
        if (!string.IsNullOrEmpty(buttonStateData.label))
        {
            labelItem.text = buttonStateData.label;
            labelItem.gameObject.SetActive(true);
        }
        if (!string.IsNullOrEmpty(buttonStateData.text) || !string.IsNullOrEmpty(buttonStateData.metaText))
        {
            buttonText.text = $"<size=8>{buttonStateData.text}</size><size=6>\n<alpha=#88>{buttonStateData.metaText}</size>";
            buttonText.gameObject.SetActive(true);
        }
    }

    public void SetState(int index)
    {
        if(index >= 0  && index < states.Count)
        {
            SetButtonData(states[index]);
        }
    }
}


[System.Serializable]
public class ButtonStateData
{
    public Sprite spriteIcon;
    public string fontIcon;
    public string label;
    public string text;
    public string metaText;

}

