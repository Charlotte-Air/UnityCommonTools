using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Text;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

[ExecuteInEditMode]
public class UIRotator : MonoBehaviour
{
    public AnimationCurve curve;

    private Quaternion m_Rotation;
    private Action m_OnFin;
    private Tweener m_Tweener;

    private void Awake()
    {
        m_Rotation = transform.localRotation;
    }
    
    public void StartRotate(float degree, float during, Action onFin)
    {
        if (m_Tweener != null)
        {
            m_Tweener.Kill();
            m_Tweener = null;
        }

        m_OnFin = onFin;
        transform.DOLocalRotate(new Vector3(0, 0, degree), during, RotateMode.FastBeyond360).
            SetEase(curve).
            OnComplete(() =>
            {
                if (m_OnFin != null)
                    m_OnFin();
            });
    }

    public void StopRotate(bool reset = true)
    {
        if (m_Tweener != null)
        {
            m_Tweener.Kill();
            m_Tweener = null;
        }

        if (reset)
        {
            Reset();
        }

        if (m_OnFin != null)
            m_OnFin();
    }

    public void Reset()
    {
        transform.localRotation = m_Rotation;
    }

#if UNITY_EDITOR

    private float degree = 7200;
    private float during = 5;

    [OnInspectorGUI]
    private void OnInspectorGUI1()
    {
        degree = EditorGUILayout.FloatField("Degree:", degree);
        during = EditorGUILayout.FloatField("During:", during);

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Start"))
        {
            StartRotate(degree, during, ()=> { Debug.Log("Done"); });
        }

        if (GUILayout.Button("Stop"))
        {
            StopRotate();
        }

        EditorGUILayout.EndHorizontal();
    }
#endif
}