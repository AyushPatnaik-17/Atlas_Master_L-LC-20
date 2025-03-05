using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace InnovateLabs.Utilities
{
    [InitializeOnLoad]
    public class MeshCount : Editor
    {
        static int vertex = 0;
        static int triangle = 0;
        static int meshCount = 0;
        static Rect windowRect = new Rect(20, 20, 120, 50);

        static bool isVisible = false;

        private static void OnSelectionChanged()
        {

            if (!isVisible) return;

            vertex = 0;
            triangle = 0;
            meshCount = 0;

            var selections = Selection.objects.ToList();
            if (selections.Count == 0 || selections == null)
            {
                return;
            }
            for (int i = 0; i < selections.Count; i++)
            {
                if (selections[i].GetType() != typeof(GameObject)) continue;
                var objects = (GameObject)selections[i];
                var mesh = objects.GetComponent<MeshFilter>();
                if (mesh != null)
                {
                    vertex += mesh.sharedMesh.vertices.Length;
                    triangle += mesh.sharedMesh.triangles.Length/3;
                    meshCount++;
                }
            }
        }

        private static void UpdateDataDisplay(SceneView sceneView)
        {
            if (!isVisible) return;
            Handles.BeginGUI();
            windowRect = GUILayout.Window(0, windowRect, MeshLabel, "Mesh Data"); 
            Handles.EndGUI();
        }

        static void MeshLabel(int windowId)
        {
            GUILayout.Label($"Vertex Count : {vertex:n0}");
            GUILayout.Label($"Triangle Count : {triangle:n0}");
            GUILayout.Label($"Mesh Count : {meshCount:n0}");
        }

        [MenuItem("Innovate Labs/ Utilities/ Mesh Data")]
        public static void ToggleMeshData()
        {
            isVisible = !isVisible;
            if(isVisible)
            {
                Selection.selectionChanged += OnSelectionChanged;
                SceneView.duringSceneGui += UpdateDataDisplay;
            }
            else
            {
                Selection.selectionChanged -= OnSelectionChanged;
                SceneView.duringSceneGui -= UpdateDataDisplay;
            }
        }
    }

}