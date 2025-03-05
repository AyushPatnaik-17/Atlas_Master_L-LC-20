using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine;

public enum SizeFactor
{
    FullScale,
    SmallScale
}
public class ScaleOptions : MonoBehaviour
{
    public PressableButton OneX;
    public PressableButton HalfX;
    public PressableButton QuarterX;

    public SizeFactor Size;

    public Transform ObjectToScale;
    private void Awake()
    {
        Size = SizeFactor.FullScale;
        OneX.OnClicked.AddListener(delegate
        {
            ObjectToScale.localScale = Vector3.one;
            Size = SizeFactor.FullScale;
        });
        HalfX.OnClicked.AddListener(delegate
        {
            ObjectToScale.localScale = Vector3.one * 0.5f;
            Size = SizeFactor.SmallScale;
        });
        QuarterX.OnClicked.AddListener(delegate
        {
            ObjectToScale.localScale = Vector3.one * 0.25f;
            Size = SizeFactor.SmallScale;
        });
    }
}
