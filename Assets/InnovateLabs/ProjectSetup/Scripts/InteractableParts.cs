using System.Collections.Generic;
using UnityEngine;

namespace InnovateLabs.Projects
{
    public class InteractableParts : ScriptableObject
    {
        public Interactables interactables = new Interactables();

        public void AddInteractable(string guid)
        {
            interactables.interactables.Add(guid); 
            MakeDirty();
        }

        
        public List<string> GetInteractables()
        {
            return interactables.interactables;
        }

        public string GetInteractable(int index)
        {
            return interactables.interactables[index];
        }
        public int Count()
        {
            return interactables.interactables.Count;
        }

        public void RemoveInteractable(string value)
        {
            interactables.interactables.Remove(value);
            MakeDirty();
        }
        public void RemoveAllInteractables(List<string> interactableParts)
        {
            interactables.interactables.RemoveAll(x=> !interactableParts.Contains(x));
            MakeDirty();
        }

        public void OnValidate()
        {
           
        }

        public void MakeDirty()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            this.SetDirty();
#pragma warning restore CS0618 // Type or member is obsolete
#if UNITY_EDITOR
            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }
    }
    [System.Serializable]
    public class Interactables
    {
        public List<string> interactables = new List<string>();
    }

}