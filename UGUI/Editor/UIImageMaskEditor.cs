using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Collections;
using UnityEditor.AnimatedValues;


[CustomEditor(typeof(UIImageMask), true)]
[CanEditMultipleObjects]
public class UIImageMaskEditor : ImageEditor
{
    GUIContent m_SpriteContent2;

    SerializedProperty m_Type2;
    SerializedProperty m_Sprite2;
    SerializedProperty m_PreserveAspect2;
    AnimBool m_ShowType2;

    SerializedProperty m_maskTexture;
    SerializedProperty m_positionScale;

    protected override void OnEnable()
    {
        base.OnEnable();

        m_SpriteContent2 = new GUIContent("Source Image");
        m_Sprite2 = serializedObject.FindProperty("m_Sprite");
        m_Type2 = serializedObject.FindProperty("m_Type");
        m_PreserveAspect2 = serializedObject.FindProperty("m_PreserveAspect");

        m_ShowType2 = new AnimBool(m_Sprite2.objectReferenceValue != null);
        m_ShowType2.valueChanged.AddListener(Repaint);

        m_maskTexture = serializedObject.FindProperty("m_maskTexture");
        m_positionScale = serializedObject.FindProperty("m_positionScale");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(m_maskTexture);
        EditorGUILayout.PropertyField(m_positionScale, true);

        SpriteGUI();
        AppearanceControlsGUI();
        RaycastControlsGUI();

        m_ShowType2.target = m_Sprite2.objectReferenceValue != null;
        if (EditorGUILayout.BeginFadeGroup(m_ShowType2.faded))
            TypeGUI();
        EditorGUILayout.EndFadeGroup();

        SetShowNativeSize(false);
        if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_PreserveAspect2);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndFadeGroup();
        NativeSizeButtonGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void SetShowNativeSize(bool instant)
    {
        Image.Type type = (Image.Type)m_Type2.enumValueIndex;
        bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled);
        base.SetShowNativeSize(showNativeSize, instant);
    }

    protected new void SpriteGUI()
    {
        EditorGUILayout.PropertyField(m_Sprite2, m_SpriteContent2);
    }
}