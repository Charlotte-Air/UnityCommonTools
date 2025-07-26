using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(UIAlphaGroup))]
public class UIAlphaGroupEditor : Editor
{
    float value = 0;
    public override void OnInspectorGUI()
    {
        UIAlphaGroup scrip = target as UIAlphaGroup;
        serializedObject.Update();

      

        value = serializedObject.FindProperty("alpha").floatValue;
        serializedObject.FindProperty("alpha").floatValue = EditorGUILayout.Slider("Slider", value, 0f, 1f);
        if (value != serializedObject.FindProperty("alpha").floatValue)
        {
            if (scrip != null)
                scrip.ChangeAlpha();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
