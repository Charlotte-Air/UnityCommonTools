using UnityEngine;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine.UI;
using System.Collections;

[CustomEditor(typeof(UIText), true)]
[CanEditMultipleObjects]
public class UITextEditor : UnityEditor.UI.GraphicEditor
{
    SerializedProperty m_supportImage;
    SerializedProperty m_Text;
    SerializedProperty m_UIFont;
    SerializedProperty m_TextScale;

    SerializedProperty m_supportLetterSpacing;
    SerializedProperty m_letterSpacing;

    SerializedProperty m_FontData;


    SerializedProperty m_supportPic;
    SerializedProperty m_font;
    SerializedProperty m_picScale;

    SerializedProperty m_Gray;

    SerializedProperty m_OnUrlClickProperty;

    SerializedProperty m_Interactable;
    SerializedProperty m_OnHold;
    SerializedProperty m_OnUp;

    private SerializedProperty mText;
    private SerializedProperty mUIString;
    private SerializedProperty bTransferred;
    private SerializedProperty isSetExtension;

    private bool bLastTrans;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_supportImage = serializedObject.FindProperty("customFont");
        m_UIFont = serializedObject.FindProperty("m_UIFont");
        m_TextScale = serializedObject.FindProperty("m_TextScale");

        m_letterSpacing = serializedObject.FindProperty("m_letterSpacing");
        m_supportLetterSpacing = serializedObject.FindProperty("m_supportLetterSpacing");

        m_Text = serializedObject.FindProperty("m_Text");

        m_FontData = serializedObject.FindProperty("m_FontData");

        m_font = serializedObject.FindProperty("m_font");
        m_supportPic = serializedObject.FindProperty("supportPic");
        m_picScale = serializedObject.FindProperty("picScale");

        m_Gray = serializedObject.FindProperty("m_Gray");

        m_OnUrlClickProperty = serializedObject.FindProperty("m_onUrlClick");

        m_Interactable = serializedObject.FindProperty("m_Interactable");
        m_OnHold = serializedObject.FindProperty("m_OnHold");
        m_OnUp = serializedObject.FindProperty("m_OnUp");

        mText = serializedObject.FindProperty("m_Text");
        mUIString = serializedObject.FindProperty("m_UIString");
        bTransferred = serializedObject.FindProperty("bTransferred");
        isSetExtension = serializedObject.FindProperty("isSetExtension");
        bLastTrans = bTransferred.boolValue;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(m_supportImage);
        EditorGUILayout.PropertyField(m_supportPic);

        bool bChanged1 = EditorGUI.EndChangeCheck();

        if (m_supportImage.boolValue)
        {
            EditorGUILayout.PropertyField(m_UIFont);
            EditorGUILayout.PropertyField(m_TextScale);
        }
        else if (m_supportPic.boolValue)
        {
            if (m_font.objectReferenceValue == null)
            {
                string defaultFaceFont = "Assets/DataRes/UI/Font/UIFont.prefab";
                Object faceFont = AssetDatabase.LoadAssetAtPath(defaultFaceFont, typeof(Object));
                m_font.objectReferenceValue = faceFont;
            }
            EditorGUILayout.PropertyField(m_picScale);
            EditorGUILayout.PropertyField(m_font);
        }

        EditorGUILayout.PropertyField(bTransferred, true);

        if (bTransferred.boolValue)
        {
            if (bLastTrans != bTransferred.boolValue)
            {
                mUIString.stringValue = mText.stringValue;
                mText.stringValue = string.Empty;
                bLastTrans = bTransferred.boolValue;
            }

            GUIStyle style = new GUIStyle(EditorStyles.textField);
            style.wordWrap = true;

            Rect rect = EditorGUILayout.GetControlRect(GUILayout.Height(15));

            EditorGUI.LabelField(rect, new GUIContent("Text"));
            EditorGUILayout.LabelField(mText.stringValue, style, GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            EditorGUILayout.PropertyField(mUIString);
        }
        else
        {
            if (bLastTrans != bTransferred.boolValue)
            {
                mText.stringValue = mUIString.stringValue;
                bLastTrans = bTransferred.boolValue;
            }
            EditorGUILayout.PropertyField(mText);
        }

        EditorGUILayout.PropertyField(m_supportLetterSpacing);
        if (m_supportLetterSpacing.boolValue)
        {
            EditorGUILayout.PropertyField(m_letterSpacing);
        }

        EditorGUILayout.PropertyField(m_Gray);

       

        EditorGUILayout.PropertyField(m_FontData);

        EditorGUILayout.PropertyField(m_Interactable);

        if (!Application.isPlaying)
        {
            if (bTransferred.boolValue)
            {
                if (GUI.changed)
                {
                    bool found = false;
                    mText.stringValue = UITextString.Instance.GetUIText(mUIString.stringValue, out found);
                }
            }
        }

        AppearanceControlsGUI();
        RaycastControlsGUI();

        if (m_Interactable.boolValue)
        {
            EditorGUILayout.PropertyField(m_OnHold);
            EditorGUILayout.PropertyField(m_OnUp);
        }

        EditorGUILayout.PropertyField(m_OnUrlClickProperty);

        EditorGUILayout.PropertyField(isSetExtension);

        serializedObject.ApplyModifiedProperties();
    }
}