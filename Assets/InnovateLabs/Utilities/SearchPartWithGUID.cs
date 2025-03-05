using InnovateLabs.Projects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using GUID = InnovateLabs.Projects.GUID;
public class SearchPartWithGUID : MonoBehaviour
{
    public string guidToSearch;
    [ContextMenu("Search")]
    public void SearchPart()
    {
        var rootObjects = gameObject.scene.GetRootGameObjects().ToList();

        var guids = new List<GUID>();

        foreach(var root in rootObjects)
        {
            var guidinParent = root.GetComponent<GUID>();

            if(guidinParent!= null)
            {
                guids.Add(guidinParent);
            }
            var guidInChildren = root.GetComponentsInChildren<GUID>(true);
            if(guidInChildren != null)
            {
                guids.AddRange(guidInChildren.ToList());
            }
        }
        if (string.IsNullOrEmpty(guidToSearch)) return;
        var part = guids.Find(x => x.GetGUID() == guidToSearch);
#if UNITY_EDITOR
        Selection.activeObject = part.gameObject;
#endif
    }
}
