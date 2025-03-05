using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public InteractionTypes InteractionType;

    private Coroutine _co;
    public List<Coroutine> Coroutines = new List<Coroutine>();
    public void StartAnimation()
    {
        var animatables = GetComponentsInChildren<Animatable>(true);
        for(int i = 0; i < animatables.Length; i++)
        {
            //_co = StartCoroutine(animatables[i].LerpObject());
            Coroutines.Add(StartCoroutine(animatables[i].LerpObject()));
        }
    }
    public void StopAnimation()
    {
        Debug.Log("wuba luba dub dub");
        var animatables = GetComponentsInChildren<Animatable>(true);
        for (int i = 0; i < animatables.Length; i++)
        {
            if (Coroutines.Count != 0)
            {
                StopCoroutine(Coroutines[i]);
                animatables[i].SetPos();
            }
            else
                Debug.Log("Womp womp");
            
        }
        Coroutines.Clear();
    }
}
