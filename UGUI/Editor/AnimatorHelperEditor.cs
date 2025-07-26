using System;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEditorInternal;
using System.Collections.Generic;

[CanEditMultipleObjects]
[CustomEditor(typeof(animatorhelper))]
class AnimatorHelperEditor : Editor
{
   private SerializedProperty _animator;

   ReorderableList _callbackSwitchArray;

   ReorderableList _particleMatchArray;

   void OnEnable()
   {
       _animator = serializedObject.FindProperty("m_Animator");

       _callbackSwitchArray = new ReorderableList(serializedObject, serializedObject.FindProperty("SetCallbackSwitch")
           , true, true, true, true);
       _callbackSwitchArray.drawHeaderCallback = (Rect rect) =>
       {
           GUI.Label(rect, "回调设置");
       };
       _callbackSwitchArray.elementHeight = 20;
       _callbackSwitchArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
       {
           SerializedProperty item = _callbackSwitchArray.serializedProperty.GetArrayElementAtIndex(index);
           rect.height -= 4;
           rect.y += 1;
           EditorGUI.LabelField(rect, new GUIContent("动作" + (index + 1)));

           var nameProperty = item.FindPropertyRelative("name");
           var nameRect = new Rect(rect)
           {
               x = rect.x + 80,
           };
           EditorGUI.LabelField(nameRect, new GUIContent(nameProperty.stringValue));

           var openProperty = item.FindPropertyRelative("open");
           var openRect = new Rect(rect)
           {
               x = rect.x + 160,
           };
           openProperty.boolValue = EditorGUI.Toggle(openRect, "是否开启回调", openProperty.boolValue);
       };
       _callbackSwitchArray.onAddCallback = (ReorderableList list) =>
       {
           EditorUtility.DisplayDialog("Warnning", "Can not Add", "OK");
           //if (list.serializedProperty != null)
           //{
           //    list.serializedProperty.arraySize++;
           //    list.index = list.serializedProperty.arraySize - 1;
           //}
           //else
           //{
           //    ReorderableList.defaultBehaviours.DoAddButton(list);
           //}
       };
       _callbackSwitchArray.onRemoveCallback = (ReorderableList list) =>
       {
           EditorUtility.DisplayDialog("Warnning", "Can not Remove", "OK");
           //ReorderableList.defaultBehaviours.DoRemoveButton(list);
       };

       _particleMatchArray = new ReorderableList(serializedObject, serializedObject.FindProperty("SetMatchParticle")
           , true, true, true, true);
       _particleMatchArray.drawHeaderCallback = (Rect rect) =>
       {
           GUI.Label(rect, "配套特效");
       };
       _particleMatchArray.elementHeight = 20;
       _particleMatchArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
       {
           SerializedProperty item = _particleMatchArray.serializedProperty.GetArrayElementAtIndex(index);
           rect.height -= 4;
           rect.y += 1;
           EditorGUI.LabelField(rect, new GUIContent("特效" + (index + 1)));

           var nameProperty = item.FindPropertyRelative("key");
           var nameRect = new Rect(rect)
           {
               x = rect.x + 60,
           };
           EditorGUI.LabelField(nameRect, new GUIContent(nameProperty.stringValue));

           var particleProperty = item.FindPropertyRelative("value");
           var particleRect = new Rect(rect)
           {
               width = rect.width - 120,
               x = rect.x + 120,
           };
           particleProperty.objectReferenceValue = EditorGUI.ObjectField(particleRect, particleProperty.objectReferenceValue, typeof(ParticleSystem), true);
       };
       _particleMatchArray.onAddDropdownCallback = (Rect rect, ReorderableList list) =>
       {
           GenericMenu menu = new GenericMenu();
           for (int i = 0; i < _callbackSwitchArray.serializedProperty.arraySize; i++)
           {
               SerializedProperty item = _callbackSwitchArray.serializedProperty.GetArrayElementAtIndex(i);
               var nameProperty = item.FindPropertyRelative("name");
               menu.AddItem(new GUIContent(nameProperty.stringValue), false, particleMatchArrayClickHandler, nameProperty.stringValue);
           }
           menu.AddSeparator("");
           menu.ShowAsContext();
       };
       _particleMatchArray.onAddCallback = (ReorderableList list) =>
       {
           if (list.serializedProperty != null)
           {
               list.serializedProperty.arraySize++;
               list.index = list.serializedProperty.arraySize - 1;
           }
           else
           {
               ReorderableList.defaultBehaviours.DoAddButton(list);
           }
       };
       _particleMatchArray.onRemoveCallback = (ReorderableList list) =>
       {
           ReorderableList.defaultBehaviours.DoRemoveButton(list);
       };
   }

   private void particleMatchArrayClickHandler(object target)
   {
       string key = (string)target;
       int index = _particleMatchArray.serializedProperty.arraySize;
       _particleMatchArray.serializedProperty.arraySize++;
       _particleMatchArray.index = index;
       SerializedProperty element = _particleMatchArray.serializedProperty.GetArrayElementAtIndex(index);
       SerializedProperty nameProperty = element.FindPropertyRelative("key");
       nameProperty.stringValue = key;

       serializedObject.ApplyModifiedProperties();
   }

   void refresh()
   {
       var clips = (_animator.objectReferenceValue as Animator)?.runtimeAnimatorController.animationClips;
       if (clips != null)
       {
           _callbackSwitchArray.serializedProperty.ClearArray();
           for (int i = 0; i < clips.Length; i++)
           {
               _callbackSwitchArray.serializedProperty.arraySize++;
               _callbackSwitchArray.index = _callbackSwitchArray.serializedProperty.arraySize - 1;
               SerializedProperty element = _callbackSwitchArray.serializedProperty.GetArrayElementAtIndex(_callbackSwitchArray.index);
               var nameProperty = element.FindPropertyRelative("name");
               nameProperty.stringValue = clips[i].name;
               var openProperty = element.FindPropertyRelative("open");
               openProperty.boolValue = false;
           }
       }
   }

   public override void OnInspectorGUI()
   {
       serializedObject.Update();
       EditorGUILayout.PropertyField(_animator, new GUIContent("Animator"), true);
       _callbackSwitchArray.DoLayoutList();
       GUILayout.BeginHorizontal();
       if (GUILayout.Button("Reset"))
       {
           refresh();
       }
       GUILayout.EndHorizontal();
       _particleMatchArray.DoLayoutList();
       serializedObject.ApplyModifiedProperties();
   }
}