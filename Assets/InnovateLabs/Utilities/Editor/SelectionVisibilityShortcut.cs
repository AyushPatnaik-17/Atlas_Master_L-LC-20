using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace InnovateLabs.Utilities
{
    public class SelectionVisibilityShortcut : Editor
    {
        [MenuItem("Innovate Labs/Utilities/Unhide All &H", false, 30)]
        public static void UnhideAll()
        {
            SceneVisibilityManager.instance.ShowAll();
        }
        [MenuItem("Innovate Labs/Utilities/Isolate %#H", false, 31)]
        public static void IsloateSelected()
        {
            var selections = Selection.gameObjects;

            if (!SceneVisibilityManager.instance.IsCurrentStageIsolated())
            {
                var s = SceneView.currentDrawingSceneView == null ? SceneView.lastActiveSceneView : SceneView.currentDrawingSceneView;
                s.FrameSelected();
                SceneVisibilityManager.instance.Isolate(selections, true);
            }
            else
            {
                SceneVisibilityManager.instance.ExitIsolation();
                var s = SceneView.currentDrawingSceneView == null ? SceneView.lastActiveSceneView : SceneView.currentDrawingSceneView;
                s.FrameSelected();
            }
        }
        [MenuItem("Innovate Labs/Utilities/Refresh",false, 44)]
        public static void Refresh()
        {
            AssetDatabase.Refresh();
        }
    }
}


public static class HideGameObject
{
    public static void HideSelected(List<GameObject> selection)
    {
        var selectionArray = selection.ToArray();
        
        SceneVisibilityManager.instance.Isolate(selectionArray, true);
    }

    public static void DisablePicking(List<GameObject> selection)
    {
        foreach(var selected in selection)
        {
            SceneVisibilityManager.instance.DisablePicking(selected, true);
        }
    }
}