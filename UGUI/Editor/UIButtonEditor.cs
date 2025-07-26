using System;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

[CustomEditor(typeof(UIButton), true)]
[CanEditMultipleObjects]
public class UIButtonEditor : ButtonEditor
{
    private SerializedProperty m_customClickRect;
    private SerializedProperty m_clickRect;
    protected SerializedProperty m_isEnabled;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_customClickRect = serializedObject.FindProperty("m_customClickRect");
        m_clickRect = serializedObject.FindProperty("m_clickRect");
        m_isEnabled = serializedObject.FindProperty("m_isEnabled");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        UIButton btn = target as UIButton;

        EditorGUILayout.PropertyField(m_isEnabled);
        if (btn.isEnabled != m_isEnabled.boolValue)
        {
            btn.isEnabled = m_isEnabled.boolValue;
        }
        EditorGUILayout.PropertyField(m_customClickRect, true);

        if (m_customClickRect.boolValue)
        {
            EditorGUILayout.PropertyField(m_clickRect);
        }

        serializedObject.ApplyModifiedProperties();

        base.OnInspectorGUI();
    }

    public void OnSceneGUI()
    {
        if (m_customClickRect.boolValue)
        {
            UIButton btn = target as UIButton;
            RectTransform gui = btn.GetComponent<RectTransform>();
            Rect rect = m_clickRect.rectValue;
            // CanvasGroup objCG = btn.GetComponent<CanvasGroup>();
            // if (objCG == null)
            // {
            //     objCG = obj.AddComponent<CanvasGroup>();
            // }
            // objCG.alpha = 1;
            // objCG.DOFade(1,1);
            rect.center = rect.center - new Vector2(rect.width/2, rect.height/2);
            DrawRect(Color.magenta, rect, gui.transform);
        }
    }

    void DrawRect(Color col, Rect rect, Transform space)
    {
        Handles.color = col;
        Vector3 p0 = space.TransformPoint(new Vector2(rect.x, rect.y));
        Vector3 p1 = space.TransformPoint(new Vector2(rect.x, rect.yMax));
        Vector3 p2 = space.TransformPoint(new Vector2(rect.xMax, rect.yMax));
        Vector3 p3 = space.TransformPoint(new Vector2(rect.xMax, rect.y));
        Handles.DrawLine(p0, p1);
        Handles.DrawLine(p1, p2);
        Handles.DrawLine(p2, p3);
        Handles.DrawLine(p3, p0);
    }
}