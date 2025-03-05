using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class HoverUtil : Editor
{
    private static GameObject hoveredObject;
    private static bool isHovering = false;
    private static bool isKeyPressed = false;
    private static KeyCode activationKey = KeyCode.Z;

    static HoverUtil()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
        SceneView.onSceneGUIDelegate += HandleSceneViewGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        Event e = Event.current;

        if (e.type == EventType.KeyDown && e.keyCode == activationKey)
        {
            isKeyPressed = true;
        }
        else if (e.type == EventType.KeyUp && e.keyCode == activationKey)
        {
            isKeyPressed = false;
        }
    }

    private static void HandleSceneViewGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (isKeyPressed)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject hitObject = hit.collider.gameObject;
                if (hitObject != null && hitObject != hoveredObject)
                {
                    if (hoveredObject != null)
                    {
                        hoveredObject.SetActive(false);
                    }

                    hoveredObject = hitObject;
                    hoveredObject.SetActive(true);
                }
            }
        }
        else
        {
            if (hoveredObject != null)
            {
                hoveredObject.SetActive(false);
                hoveredObject = null;
            }
        }
    }
}
