using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
[CustomEditor(typeof(CurvedUISettings))]
public class CurvedUISettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CurvedUISettings myTarget = (CurvedUISettings) target;

        if (target == null) return;

        //initial settings
        GUI.changed = false;
        EditorGUIUtility.labelWidth = 150;

        //shape settings
        GUILayout.Label("Shape", EditorStyles.boldLabel);
        myTarget.Shape = (CurvedUISettings.CurvedUIShape) EditorGUILayout.EnumPopup("Canvas Shape", myTarget.Shape);
        switch (myTarget.Shape)
        {
            case CurvedUISettings.CurvedUIShape.Bezier:
            {
                myTarget.Angle = EditorGUILayout.IntSlider("Angle", myTarget.Angle, 0, 360);
                myTarget.PreserveAspect = EditorGUILayout.Toggle("Preserve Aspect", myTarget.PreserveAspect);

                break;
            }
        }

        //advanced settings
        GUILayout.Space(10);

        //final settings
        if (GUI.changed && myTarget != null)
            EditorUtility.SetDirty(myTarget);
    }
}