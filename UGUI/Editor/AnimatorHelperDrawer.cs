using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(animatorhelper))]
public class AnimatorHelperDrawer : PropertyDrawer
{
   private void OnEnable()
   {
      if (m_Animator != null && matchState == null && matchString == string.Empty)
      {
          var clips = m_Animator.runtimeAnimatorController.animationClips;
          matchState = new string[clips.Length];
          StringBuilder builder = new StringBuilder();
          for (int i = 0; i < clips.Length; i++)
          {
              matchState[i] = clips[i].name;
              if (i == 0)
              {
                  builder.Append(matchState[i]);
              }
              else
              {
                  builder.Append('|');
                  builder.Append(matchState[i]);
              }
          }
          matchString = builder.ToString();
      }
   }

   public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
   {
       using (new EditorGUI.PropertyScope(position, label, property))
       {
           EditorGUIUtility.labelWidth = 60;
           position.height = EditorGUIUtility.singleLineHeight;

           var animatorRect = new Rect(position)
           {
               width = position.width,
           };

           var matchRect = new Rect(animatorRect)
           {
               y = animatorRect.y + EditorGUIUtility.singleLineHeight + 5
           };

           var animatorProperty = property.FindPropertyRelative("m_Animator");
           var matchProperty = property.FindPropertyRelative("ClipsString");

           animatorProperty.objectReferenceValue = EditorGUI.ObjectField(animatorRect, animatorProperty.objectReferenceValue, typeof(Texture), false);
           matchProperty.stringValue = EditorGUI.TextField(matchRect, "值", matchProperty.stringValue);
       }
   }
}
