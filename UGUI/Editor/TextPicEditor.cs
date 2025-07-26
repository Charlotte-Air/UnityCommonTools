using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

[CustomEditor(typeof(TextPic), true)]
[CanEditMultipleObjects]
public class TextPicEditor : GraphicEditor
{
    private SerializedProperty underLineOffSet;
    private SerializedProperty mText;
    private SerializedProperty mFontData;
    //private SerializedProperty mOrgText;

    //private float _underLineOffSet;

    protected override void OnEnable()
    {
        base.OnEnable();
        underLineOffSet = serializedObject.FindProperty("underLineOffset");
        mText = serializedObject.FindProperty("m_Text");
        mFontData = serializedObject.FindProperty("m_FontData");
        //mOrgText = serializedObject.FindProperty("m_OrgText");
        //_underLineOffSet = underLineOffSet.floatValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUILayout.PropertyField(underLineOffSet);
        EditorGUILayout.PropertyField(mText);
        EditorGUILayout.PropertyField(mFontData);

        AppearanceControlsGUI();
        RaycastControlsGUI();
        serializedObject.ApplyModifiedProperties();

        if (GUILayout.Button("Clear"))
        {
            ((TextPic)serializedObject.targetObject).Close();
        }
    }
}