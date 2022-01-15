/*#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

// This script simply allows users to be given [ReadOnly] property, which makes them visible in the editor, but unable to be edited.
public class ReadOnlyAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}*/
