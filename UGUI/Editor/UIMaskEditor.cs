using UnityEditor;
using UnityEngine.UI;

[CustomEditor(typeof(UIMask), true)]
[CanEditMultipleObjects]
public class UIMaskEditor : Editor
{
    SerializedProperty m_ShowMaskGraphic;
    SerializedProperty m_maskForEffect;
    //SerializedProperty m_unmask;

    protected virtual void OnEnable()
    {
        m_ShowMaskGraphic = serializedObject.FindProperty("m_ShowMaskGraphic");
        m_maskForEffect = serializedObject.FindProperty("m_maskForEffect");
        //m_unmask = serializedObject.FindProperty("m_unmask");
    }

    public override void OnInspectorGUI()
    {
        var graphic = (target as UIMask).GetComponent<Graphic>();

        if (graphic && !graphic.IsActive())
            EditorGUILayout.HelpBox("Masking disabled due to Graphic component being disabled.", MessageType.Warning);

        serializedObject.Update();
        EditorGUILayout.PropertyField(m_ShowMaskGraphic);

        EditorGUILayout.PropertyField(m_maskForEffect);
//         if (m_maskForEffect.boolValue)
//         {
//             EditorGUILayout.PropertyField(m_unmask);
//         }

        serializedObject.ApplyModifiedProperties();
    }
}
