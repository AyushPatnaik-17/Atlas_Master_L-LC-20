using UnityEditor;
using UnityEngine;

namespace InnovateLabs.Utilities
{
    [CustomPropertyDrawer(typeof(ReadValueAtInspector))]
    public class ReadValueAtInspector_Editor : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label);
            GUI.enabled = true;
        }
    }
}
