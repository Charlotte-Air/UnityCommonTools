using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UICanvasScaler), true)]
    [CanEditMultipleObjects]
    public class UICanvasScalerEditor : Editor
    {
        SerializedProperty m_UiReferenceResolutionMode;
        SerializedProperty m_ReferenceResolution;
        SerializedProperty m_CanvasScaler;

        protected virtual void OnEnable()
        {
            m_UiReferenceResolutionMode = serializedObject.FindProperty("m_ReferenceResolutionMode");
            m_ReferenceResolution = serializedObject.FindProperty("m_ReferenceResolution");
            m_CanvasScaler = serializedObject.FindProperty("m_CanvasScaler");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_UiReferenceResolutionMode);

            EditorGUILayout.Space();

            if (m_UiReferenceResolutionMode.enumValueIndex == (int)UICanvasScaler.ReferenceResolutionMode.Custom)
            {
                EditorGUILayout.PropertyField(m_ReferenceResolution);
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_CanvasScaler);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
