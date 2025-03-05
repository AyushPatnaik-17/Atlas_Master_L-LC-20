using UnityEngine;

public class RimColorManager : MonoBehaviour
{
    public static RimColorManager instance;
    void Start()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public Material CorrectColor;
    public Material WrongColor;

    public Material highlightColor;
}
