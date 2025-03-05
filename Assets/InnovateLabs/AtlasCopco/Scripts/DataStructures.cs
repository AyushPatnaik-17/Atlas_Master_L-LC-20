using UnityEngine;
using UnityEngine.UI;
using System;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections.Generic;
using System.ComponentModel;

namespace AtlasCopco.DS
{
    [Serializable]
    public class ButtonObjectPair 
    {
        //public Button Button;
        public PressableButton Button;
        public List<GameObject> Objects = new();

        public void SetupListener()
        {
            int count = Objects.Count;
            if (count == 0 || Button == null)
            {
                Debug.LogError("Button or object not set");
                return;
            }

            Button.OnClicked.AddListener(delegate
            {
                ToggleObjects(count);   
            });
        }

        private void ToggleObjects(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (Objects[i] == null)
                    continue;

                Objects[i].SetActive(!Objects[i].activeSelf);
            }
        }

        public void Validate()
        {
            if (Objects == null) return;

            HashSet<GameObject> uniqueObjects = new();
            for (int i = Objects.Count - 1; i >= 0; i--)
            {
                if (Objects[i] == null || !uniqueObjects.Add(Objects[i]))
                {
                    Debug.LogWarning($"Duplicate or null GameObject removed: {Objects[i]?.name}");
                    Objects.RemoveAt(i);
                }
            }
        }
    }

}
