using System;
using UnityEngine;
using System.Text;
using UnityEditorInternal;
using System.Collections.Generic;

namespace UnityEditor.AI
{
   public struct CreationCalculate
   {
       public int id;
       public GameObject obj;
       public int idx;
       public Vector3 pos;
   }

   [CanEditMultipleObjects]
   [CustomEditor(typeof(NavMeshBuildList))]
   public class NavMeshBuildListEditor : Editor
   {
       NavMeshBuildList source;

       ReorderableList _meshArray;

       int _selectIndex = -1;

       ReorderableList _pointArray;

       int _selectPointIndex = -1;

       static List<MeshPoint> tempPoints = new List<MeshPoint>();

       ReorderableList _EnvironmentArray;

       static List<EnvironmentValueEditor> tempEnvironment = new List<EnvironmentValueEditor>();

       StringBuilder builder = new StringBuilder();

       void OnEnable()
       {
           _meshArray = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Meshes")
           , true, true, true, true);
           _meshArray.drawHeaderCallback = (Rect rect) =>
           {
               GUI.Label(rect, "模块列表");
           };
           _meshArray.elementHeight = 20;
           _meshArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
           {
               SerializedProperty item = _meshArray.serializedProperty.GetArrayElementAtIndex(index);
               rect.height -= 4;
               rect.y += 1;
               EditorGUI.PropertyField(rect, item, new GUIContent("模块" + index));
           };
           _meshArray.onSelectCallback = (ReorderableList list) =>
           {
               SerializedProperty _element = list.serializedProperty.GetArrayElementAtIndex(list.index);
               if (_selectIndex == -1 || _selectIndex != list.index)
               {
                   _selectIndex = list.index;
                   EditorGUIUtility.PingObject(_element.objectReferenceValue);
               }
               else if (_selectIndex != -1 && _selectIndex == list.index)
               {
                   EditorGUIUtility.PingObject(_element.objectReferenceValue);
                   Selection.activeObject = _element.objectReferenceValue;
               }
           };
           _meshArray.onAddCallback = (ReorderableList list) =>
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
           _meshArray.onRemoveCallback = (ReorderableList list) =>
           {
               list.serializedProperty.DeleteArrayElementAtIndex(list.index);
               ReorderableList.defaultBehaviours.DoRemoveButton(list);
           };

           _pointArray = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Points")
           , true, true, true, true);
           _pointArray.drawHeaderCallback = (Rect rect) =>
           {
               GUI.Label(rect, "位置列表");
           };
           _pointArray.elementHeight = 65;
           _pointArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
           {
               SerializedProperty item = _pointArray.serializedProperty.GetArrayElementAtIndex(index);
               var labelRect = new Rect(rect)
               {
                   width = rect.width - 20,
                   height = EditorGUIUtility.singleLineHeight,
                   y = rect.y + 2
               };
               EditorGUI.LabelField(labelRect, new GUIContent("地表位置" + index));

               var pointRect = new Rect(labelRect)
               {
                   y = labelRect.y + EditorGUIUtility.singleLineHeight + 2,
               };
               var pointProperty = item.FindPropertyRelative("point");
               pointProperty.vector3Value = EditorGUI.Vector3Field(pointRect, "位置", pointProperty.vector3Value);

               var angleRect = new Rect(pointRect)
               {
                   y = pointRect.y + EditorGUIUtility.singleLineHeight
               };
               var angleProperty = item.FindPropertyRelative("angle");
               angleProperty.vector3Value = EditorGUI.Vector3Field(angleRect, "角度", angleProperty.vector3Value);

               var addPathRect = new Rect(labelRect)
               {
                   width = rect.width - 160,
                   x = labelRect.x + 140,
               };
               var objProperty = item.FindPropertyRelative("obj");
               objProperty.objectReferenceValue = EditorGUI.ObjectField(addPathRect, objProperty.objectReferenceValue, typeof(GameObject), true);

               if (objProperty.objectReferenceValue != null)
               {
                   var obj = objProperty.objectReferenceValue as GameObject;
                   bool get = false;
                   foreach (GameObject objt in Resources.FindObjectsOfTypeAll(typeof(GameObject)))
                   {
                       if (objt.name == obj.name)
                       {
                           get = true;
                           break;
                       }
                   }
                   if (get)
                   {
                       obj.transform.SetParent(source.transform);
                       var pos = pointProperty.vector3Value;
                       pos.x = obj.transform.localPosition.x;
                       pos.z = obj.transform.localPosition.z;
                       pointProperty.vector3Value = pos;
                       pos = obj.transform.localPosition;
                       pos.y = pointProperty.vector3Value.y;
                       obj.transform.localPosition = pos;
                   }
               }
                

               if (EditorGUI.DropdownButton(addPathRect, new GUIContent("添加路径"), FocusType.Keyboard))
               {
                  GenericMenu menu = new GenericMenu();
                  menu.AddItem(new GUIContent("新增"), false, AddPathClickHandler, new CreationCalculate() { id = 0, obj = null, idx = index, pos = pointProperty.vector3Value });
                  if (source.m_PathArray == null)
                  {
                      source.m_PathArray = new List<List<GameObject>>();
                      for (int i = 0; i < source.m_Points.Count; i++)
                      {
                          source.m_PathArray.Add(new List<GameObject>());
                      }
                  }
                  if (index < source.m_PathArray.Count)
                  {
                      for (int i = 0; i < source.m_PathArray[index].Count; i++)
                      {
                          if (source.m_PathArray[index][i] != null)
                          {
                              menu.AddItem(new GUIContent(source.m_PathArray[index][i].name), false, AddPathClickHandler, new CreationCalculate() { id = int.Parse(source.m_PathArray[index][i].name), obj = source.m_PathArray[index][i] });
                          }
                      }
                  }
                  menu.ShowAsContext();
               }
           };
           _pointArray.onSelectCallback = (ReorderableList list) =>
           {
               SerializedProperty _element = list.serializedProperty.GetArrayElementAtIndex(list.index);
               if (_selectPointIndex == -1 || _selectPointIndex != list.index)
               {
                   _selectPointIndex = list.index;
               }
           };
           _pointArray.onAddCallback = (ReorderableList list) =>
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
           _pointArray.onRemoveCallback = (ReorderableList list) =>
           {
               ReorderableList.defaultBehaviours.DoRemoveButton(list);
           };
           findSource();
           forEachGameObject();

           _EnvironmentArray = new ReorderableList(serializedObject, serializedObject.FindProperty("m_Environments")
           , true, true, true, true);
           _EnvironmentArray.drawHeaderCallback = (Rect rect) =>
           {
               GUI.Label(rect, "环境变量");
           };
           _EnvironmentArray.elementHeight = 200;
           _EnvironmentArray.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
           {
               SerializedProperty element = _EnvironmentArray.serializedProperty.GetArrayElementAtIndex(index);

               var labelRect = new Rect(rect)
               {
                   width = rect.width - 20,
                   height = EditorGUIUtility.singleLineHeight,
                   y = rect.y + 2
               };
               EditorGUI.LabelField(labelRect, new GUIContent("变量 " + index));

               // 描述
               var describeProperty = element.FindPropertyRelative("Describe");
               var describeRect = new Rect(labelRect)
               {
                   width = rect.width - 20,
                   height = EditorGUIUtility.singleLineHeight,
                   y = labelRect.y + EditorGUIUtility.singleLineHeight + 4
               };
               describeProperty.stringValue = EditorGUI.TextField(describeRect, "描述", describeProperty.stringValue);

               //环境变量类型
               var typeProperty = element.FindPropertyRelative("Type");
               var typeRect = new Rect(describeRect)
               {
                   width = rect.width - 20,
                   height = EditorGUIUtility.singleLineHeight,
                   y = describeRect.y + EditorGUIUtility.singleLineHeight + 4
               };
               List<string> typeStrList = new List<string>() { "空", "剧情对话", "跳下一个wavestep", "中断循环", "播放animator动画", "播放animation动画", "播放特效", "位移", "场景刷怪器" };
               typeProperty.intValue = EditorGUI.Popup(typeRect, "环境变量类型", typeProperty.intValue, typeStrList.ToArray());
               EnvironmentType display = (EnvironmentType)typeProperty.intValue;
               var stateProperty = element.FindPropertyRelative("State");
               var animatorProperty = element.FindPropertyRelative("AnimatorPart");
               var matchStringProperty = element.FindPropertyRelative("MatchString");
               var monsterProperty = element.FindPropertyRelative("MonsterPart");
               var groupsProperty = element.FindPropertyRelative("Groups");
               var motionEntityProperty = element.FindPropertyRelative("MotionEntity");
               var transformProperty = element.FindPropertyRelative("TransformPart");
               var motionTypeProperty = element.FindPropertyRelative("MotionType");
               var positionModeProperty = element.FindPropertyRelative("PositionMode");
               var positionPointProperty = element.FindPropertyRelative("PositionPoint");
               var positionTimeProperty = element.FindPropertyRelative("PositionTime");
               var rotateStyleProperty = element.FindPropertyRelative("RotateStyle");
               var rotateAngleProperty = element.FindPropertyRelative("RotateAngle");
               var rotateTimeProperty = element.FindPropertyRelative("RotateTime");
               var targetRotateAngleProperty = element.FindPropertyRelative("TargetRotate");

               switch (display)
               {
                   case EnvironmentType.animator:
                       {
                           //stateProperty.intValue = 0;

                           var animatorRect = new Rect(typeRect)
                           {
                               y = typeRect.y + EditorGUIUtility.singleLineHeight + 4
                           };
                           animatorProperty.objectReferenceValue = EditorGUI.ObjectField(animatorRect, animatorProperty.objectReferenceValue, typeof(animatorhelper), true);

                           builder.Clear();
                           List<string> options = new List<string>();
                           var helper = animatorProperty.objectReferenceValue as animatorhelper;
                           if (helper != null)
                           {
                               for (int i = 0; i < helper.SetCallbackSwitch.Count; i++)
                               {
                                   if (builder.Length > 0)
                                   {
                                       builder.Append('|');
                                   }
                                   builder.Append(i);
                                   builder.Append(',');
                                   builder.Append(helper.SetCallbackSwitch[i].name);
                                   options.Add(helper.SetCallbackSwitch[i].name);
                               }
                           }
                           matchStringProperty.stringValue = builder.Length > 0 ? builder.ToString() : string.Empty;
                           var matchStringRect = new Rect(animatorRect)
                           {
                               y = animatorRect.y + EditorGUIUtility.singleLineHeight + 4
                           };
                           EditorGUI.TextField(matchStringRect, "状态值", matchStringProperty.stringValue);

                           var stateRect = new Rect(matchStringRect)
                           {
                               y = matchStringRect.y + EditorGUIUtility.singleLineHeight + 4
                           };

                           if (stateProperty.intValue >= options.Count)
                           {
                               stateProperty.intValue = 0;
                           }
                           stateProperty.intValue = EditorGUI.Popup(stateRect, stateProperty.intValue, options.ToArray());
                       }
                       break;
                   case EnvironmentType.motion:
                       {
                           stateProperty.intValue = 0;

                           var motionEntityRect = new Rect(typeRect)
                           {
                               y = typeRect.y + EditorGUIUtility.singleLineHeight + 4
                           };
                           List<string> motionEntityStrList = new List<string>() { "场景", "模块", "相机" };
                           EnvironmentMotion.MotionEntity entity = (EnvironmentMotion.MotionEntity)EditorGUI.Popup(motionEntityRect, "位移对象", motionEntityProperty.intValue, motionEntityStrList.ToArray());
                           motionEntityProperty.intValue = (int)entity;

                           bool showTransform = false;
                           if (entity == EnvironmentMotion.MotionEntity.module)
                           {
                               showTransform = true;
                               var transformRect = new Rect(motionEntityRect)
                               {
                                   y = motionEntityRect.y + EditorGUIUtility.singleLineHeight + 4
                               };
                               transformProperty.objectReferenceValue = EditorGUI.ObjectField(transformRect, transformProperty.objectReferenceValue, typeof(Transform), true);
                           }

                           var motionTypeRect = new Rect(motionEntityRect)
                           {
                               y = showTransform ? motionEntityRect.y + (EditorGUIUtility.singleLineHeight + 4) * 2 : motionEntityRect.y + EditorGUIUtility.singleLineHeight + 4,
                           };
                           List<string> motionTypeStrList = new List<string>() {"空","位移运动","圆周运动"};
                           EnvironmentMotion.MotionType effect = (EnvironmentMotion.MotionType)EditorGUI.Popup(motionTypeRect, "运动轨迹", motionTypeProperty.intValue, motionTypeStrList.ToArray());
                           motionTypeProperty.intValue = (int)effect;
                           stateProperty.intValue = (int)effect;
                           switch (effect)
                           {
                               case EnvironmentMotion.MotionType.position:
                                   {
                                       var positionModeRect = new Rect(motionTypeRect)
                                       {
                                           y = motionTypeRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };
                                       List<string> posStrList = new List<string>(){"直接到终点", "匀速","正弦","加速","减速"};
                                       MotionAlgPosition.PositionMode mode = (MotionAlgPosition.PositionMode)EditorGUI.Popup(positionModeRect, "运动方式", positionModeProperty.intValue, posStrList.ToArray());
                                       positionModeProperty.intValue = (int)mode;

                                       var positionPointRect = new Rect(positionModeRect)
                                       {
                                           y = positionModeRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };
                                       positionPointProperty.vector3Value = EditorGUI.Vector3Field(positionPointRect, "位置", positionPointProperty.vector3Value);

                                       var positionTimeRect = new Rect(positionPointRect)
                                       {
                                           y = positionPointRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };
                                       positionTimeProperty.intValue = EditorGUI.IntField(positionTimeRect, "时间", positionTimeProperty.intValue);
                                   }
                                   break;
                               case EnvironmentMotion.MotionType.rotation:
                                   {
                                       var rotateStyleRect = new Rect(motionTypeRect)
                                       {
                                           y = motionTypeRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };
                                       List<string> rotateStrList = new List<string>() { "直接设置角度", "匀速旋转", "正弦旋转", "角速度旋转","加速到当前速度旋转", "减速旋转" };
                                       MotionAlgRotate.FixMode mode = (MotionAlgRotate.FixMode)EditorGUI.Popup(rotateStyleRect, "运动方式", rotateStyleProperty.intValue, rotateStrList.ToArray());
                                       rotateStyleProperty.intValue = (int)mode;

                                       var rotateAngleRect = new Rect(rotateStyleRect)
                                       {
                                           y = rotateStyleRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };
                                       rotateAngleProperty.vector3Value = EditorGUI.Vector3Field(rotateAngleRect, "角度", rotateAngleProperty.vector3Value);

                                       var rotateTimeRect = new Rect(rotateAngleRect)
                                       {
                                           y = rotateAngleRect.y + EditorGUIUtility.singleLineHeight + 4
                                       };

                                       if (mode == MotionAlgRotate.FixMode.DECELOMEGA)
                                       {
                                           rotateTimeProperty.floatValue = EditorGUI.FloatField(rotateTimeRect, "提前回调的系数", rotateTimeProperty.floatValue);
                                           var targetRotateAngleRect = new Rect(rotateTimeRect)
                                           {
                                               y = rotateTimeRect.y + EditorGUIUtility.singleLineHeight + 4
                                           };
                                           targetRotateAngleProperty.vector3Value = EditorGUI.Vector3Field(targetRotateAngleRect, "目标角度", targetRotateAngleProperty.vector3Value);
                                       }
                                       else
                                       {
                                           rotateTimeProperty.floatValue = EditorGUI.FloatField(rotateTimeRect, "时间", rotateTimeProperty.floatValue);
                                       }
                                   }
                                   break;
                               default:
                                   {
                                       positionModeProperty.intValue = 0;
                                       positionPointProperty.vector3Value = Vector3.zero;
                                       positionTimeProperty.intValue = 0;    
                                       rotateStyleProperty.intValue = 0;
                                       rotateAngleProperty.vector3Value = Vector3.zero;
                                       rotateTimeProperty.floatValue = 0;
                                       targetRotateAngleProperty.vector3Value = Vector3.zero;
                                   }
                                   break;
                           }
                       }
                       break;
                   case EnvironmentType.monster:
                       {
                           stateProperty.intValue = 1;

                           var monsterRect = new Rect(typeRect)
                           {
                               y = typeRect.y + EditorGUIUtility.singleLineHeight + 4
                           };
                           monsterProperty.objectReferenceValue = EditorGUI.ObjectField(monsterRect, monsterProperty.objectReferenceValue, typeof(Transform), true);

                           var groupsRect = new Rect(monsterRect)
                           {
                               y = monsterRect.y + EditorGUIUtility.singleLineHeight + 4
                           };
                           groupsProperty.stringValue = EditorGUI.TextField(groupsRect, "阵型组", groupsProperty.stringValue);
                       }
                       break;
                   default:
                       {
                           stateProperty.intValue = 0;
                           animatorProperty.objectReferenceValue = null;
                           matchStringProperty.stringValue = string.Empty;
                           monsterProperty.objectReferenceValue = null;
                           groupsProperty.stringValue = string.Empty;
                           motionEntityProperty.intValue = 0;
                           transformProperty.objectReferenceValue = null;
                           motionTypeProperty.intValue = 0;
                           positionModeProperty.intValue = 0;
                           positionPointProperty.vector3Value = Vector3.zero;
                           positionTimeProperty.intValue = 0;
                           rotateStyleProperty.intValue = 0;
                           rotateAngleProperty.vector3Value = Vector3.zero;
                           rotateTimeProperty.floatValue = 0;
                           targetRotateAngleProperty.vector3Value = Vector3.zero;
                       }
                       break;
               }
           };
           _EnvironmentArray.onAddCallback = (ReorderableList list) =>
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
           _EnvironmentArray.onRemoveCallback = (ReorderableList list) =>
           {
               ReorderableList.defaultBehaviours.DoRemoveButton(list);
           };
       }

       //private void AddPathClickHandler(object target)
       //{
       //    CreationCalculate creation = (CreationCalculate)target;

       //    if (creation.id != 0)
       //    {
       //        var obj = source.transform.Find(creation.id.ToString());
       //        if (obj != null)
       //        {
       //            EditorGUIUtility.PingObject(obj.gameObject);
       //            Selection.activeObject = obj.gameObject;
       //        }
       //    }
       //    else
       //    {
       //        var window = MainWindow.Popup("新增路径");
       //        window.SetParent(source, source.transform, creation.idx, creation.pos);
       //    }
            
       //    serializedObject.ApplyModifiedProperties();
       //}

       public override void OnInspectorGUI()
       {
           serializedObject.Update();
           EditorGUILayout.Space();
           _pointArray.DoLayoutList();
           GUILayout.BeginHorizontal();
           //GUILayout.Space(EditorGUIUtility.labelWidth);
           if (GUILayout.Button("save"))
           {
               savePoints();
           }
           if (GUILayout.Button("load"))
           {
               loadPoints();
           }
           GUILayout.EndHorizontal();
           EditorGUILayout.Space();
           _EnvironmentArray.DoLayoutList();
           GUILayout.BeginHorizontal();
           //GUILayout.Space(EditorGUIUtility.labelWidth);
           if (GUILayout.Button("SaveEnvironment"))
           {
               saveEnvironment();
           }
           if (GUILayout.Button("LoadEnvironment"))
           {
               loadEnvironment();
           }
           GUILayout.EndHorizontal();
           EditorGUILayout.Space();
           _meshArray.DoLayoutList();
           GUILayout.BeginHorizontal();
           //GUILayout.Space(EditorGUIUtility.labelWidth);
           if (GUILayout.Button("Reload"))
           {
               forEachGameObject();
           }
           GUILayout.EndHorizontal();
           EditorGUILayout.Space();
           serializedObject.ApplyModifiedProperties();
           EditorGUILayout.Space();
       }

       void findSource()
       {
           foreach (NavMeshBuildList navSource in targets)
           {
               source = navSource;
               break;
           }
       }

       void forEachGameObject()
       {
           source.clear();
           source.forEachGameObject(source.transform.parent);
       }

       public void OnSceneGUI()
       {
           Handles.color = Color.red;
           if (Event.current.type == EventType.Repaint)
           {
               for (int i = 0; i < _pointArray.serializedProperty.arraySize; i++)
               {
                   int id = GUIUtility.GetControlID(FocusType.Passive);
                   SerializedProperty item = _pointArray.serializedProperty.GetArrayElementAtIndex(i);
                   SerializedProperty pointValue = item.FindPropertyRelative("point");
                   Vector3 pos = pointValue.vector3Value + source.transform.position;
                   SerializedProperty angleValue = item.FindPropertyRelative("angle");
                   Vector3 angle = angleValue.vector3Value;
                   GUIStyle style = new GUIStyle("HeaderLabel");
                   style.fontStyle = FontStyle.Bold;
                   style.richText = true;
                   if (_selectPointIndex == i) 
                   {
                       style.fontSize = 40;
                       Handles.Label(pos, "<color=#FF0000>" + i + "</color>", style);
                       pos.y += 27f;
                       Handles.color = Color.magenta;
                       Handles.ArrowHandleCap(id, pos, Quaternion.Euler(new Vector3(90, 0, 0)), 24f, EventType.Repaint);
                       pos.y -= 27f;
                       Handles.color = Color.cyan;
                       Handles.ArrowHandleCap(id, pos, Quaternion.Euler(angle), 16f, EventType.Repaint);
                   }
                   else
                   {
                       style.fontSize = 20;
                       Handles.Label(pos, "<color=#FFB428>" + i + "</color>", style);
                       pos.y += 13.5f;
                       Handles.color = Color.yellow;
                       Handles.ArrowHandleCap(id, pos, Quaternion.Euler(new Vector3(90, 0, 0)), 12f, EventType.Repaint);
                       pos.y -= 13.5f;
                       Handles.color = Color.white;
                       Handles.ArrowHandleCap(id, pos, Quaternion.Euler(angle), 8f, EventType.Repaint);
                   }
               }
           }
           if (Event.current.alt)
           {
               addCollider();
           }
           else
           {
               delCollider();
           }
           if (Event.current.type == EventType.MouseDown && Event.current.alt)
           {
               var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
               RaycastHit hit;
               if (Physics.Raycast(ray, out hit, 1000f, LayerMask.GetMask("GroundLayer")))
               {
                   addPoint(hit.point);
               }
           }
       }

       public void addPoint(Vector3 point)
       {
           //_pointArray.serializedProperty.arraySize++;
           //_pointArray.index = _pointArray.serializedProperty.arraySize - 1;
           //SerializedProperty element = _pointArray.serializedProperty.GetArrayElementAtIndex(_pointArray.index);
           //element.vector3Value = point;
           source.addPoint(point);
       }

       public void loadPoints()
       {
           _pointArray.serializedProperty.ClearArray();
           for (int i = 0; i < tempPoints.Count; i++)
           {
               _pointArray.serializedProperty.arraySize++;
               _pointArray.index = _pointArray.serializedProperty.arraySize - 1;
               SerializedProperty element = _pointArray.serializedProperty.GetArrayElementAtIndex(_pointArray.index);
               var pointProperty = element.FindPropertyRelative("point");
               pointProperty.vector3Value = tempPoints[i].point;
               var angleProperty = element.FindPropertyRelative("angle");
               angleProperty.vector3Value = tempPoints[i].angle;
           }
       }

       public void savePoints()
       {
           tempPoints.Clear();
           for (int i = 0; i < _pointArray.serializedProperty.arraySize; i++)
           {
               SerializedProperty element = _pointArray.serializedProperty.GetArrayElementAtIndex(i);
               var pointProperty = element.FindPropertyRelative("point");
               var angleProperty = element.FindPropertyRelative("angle");
               tempPoints.Add(new MeshPoint(pointProperty.vector3Value, angleProperty.vector3Value));
           }
       }

       public void addCollider()
       {
           source.addMeshCollider();
       }

       public void delCollider()
       {
           source.delMeshCollider();
       }

       public void loadEnvironment()
       {
           _EnvironmentArray.serializedProperty.ClearArray();
           for (int i = 0; i < tempEnvironment.Count; i++)
           {
               var _editor = tempEnvironment[i];
               _EnvironmentArray.serializedProperty.arraySize++;
               _EnvironmentArray.index = _EnvironmentArray.serializedProperty.arraySize - 1;
               SerializedProperty element = _EnvironmentArray.serializedProperty.GetArrayElementAtIndex(_EnvironmentArray.index);
               var typeProperty = element.FindPropertyRelative("Type");
               typeProperty.intValue = _editor.Type;
               var describeProperty = element.FindPropertyRelative("Describe");
               describeProperty.stringValue = _editor.Describe;
               var stateProperty = element.FindPropertyRelative("State");
               stateProperty.intValue = (int)_editor.State;
               var animatorProperty = element.FindPropertyRelative("AnimatorPart");
               animatorProperty.objectReferenceValue = _editor.AnimatorPart;
               var matchStringProperty = element.FindPropertyRelative("MatchString");
               matchStringProperty.stringValue = _editor.MatchString;
               var monsterProperty = element.FindPropertyRelative("MonsterPart");
               monsterProperty.objectReferenceValue = _editor.MonsterPart;
               var groupsProperty = element.FindPropertyRelative("Groups");
               groupsProperty.stringValue = _editor.Groups;
               var motionEntityProperty = element.FindPropertyRelative("MotionEntity");
               motionEntityProperty.intValue = _editor.MotionEntity;
               var transformProperty = element.FindPropertyRelative("TransformPart");
               transformProperty.objectReferenceValue = _editor.TransformPart;
               var motionTypeProperty = element.FindPropertyRelative("MotionType");
               motionTypeProperty.intValue = _editor.MotionType;
               var positionModeProperty = element.FindPropertyRelative("PositionMode");
               positionModeProperty.intValue = _editor.PositionMode;
               var positionPointProperty = element.FindPropertyRelative("PositionPoint");
               positionPointProperty.vector3Value = _editor.PositionPoint;
               var positionTimeProperty = element.FindPropertyRelative("PositionTime");
               positionTimeProperty.intValue = (int)_editor.PositionTime;
               var rotateStyleProperty = element.FindPropertyRelative("RotateStyle");
               rotateStyleProperty.intValue = _editor.RotateStyle;
               var rotateAngleProperty = element.FindPropertyRelative("RotateAngle");
               rotateAngleProperty.vector3Value = _editor.RotateAngle;
               var rotateTimeProperty = element.FindPropertyRelative("RotateTime");
               rotateTimeProperty.floatValue = _editor.RotateTime;
               var targetRotateAngleProperty = element.FindPropertyRelative("TargetRotate");
               targetRotateAngleProperty.vector3Value = _editor.TargetRotate;
           }
       }

       public void saveEnvironment()
       {
           tempEnvironment.Clear();
           for (int i = 0; i < _EnvironmentArray.serializedProperty.arraySize; i++)
           {
               SerializedProperty element = _EnvironmentArray.serializedProperty.GetArrayElementAtIndex(i);
               EnvironmentValueEditor _editor = new EnvironmentValueEditor();
               var typeProperty = element.FindPropertyRelative("Type");
               _editor.Type = typeProperty.intValue;
               var describeProperty = element.FindPropertyRelative("Describe");
               _editor.Describe = describeProperty.stringValue;
               var stateProperty = element.FindPropertyRelative("State");
               _editor.State = (uint)stateProperty.intValue;
               var animatorProperty = element.FindPropertyRelative("AnimatorPart");
               _editor.AnimatorPart = animatorProperty.objectReferenceValue as animatorhelper;
               var matchStringProperty = element.FindPropertyRelative("MatchString");
               _editor.MatchString = matchStringProperty.stringValue;
               var monsterProperty = element.FindPropertyRelative("MonsterPart");
               _editor.MonsterPart = monsterProperty.objectReferenceValue as Transform;
               var groupsProperty = element.FindPropertyRelative("Groups");
               _editor.Groups = groupsProperty.stringValue;
               var motionEntityProperty = element.FindPropertyRelative("MotionEntity");
               _editor.MotionEntity = motionEntityProperty.intValue;
               var transformProperty = element.FindPropertyRelative("TransformPart");
               _editor.TransformPart = transformProperty.objectReferenceValue as Transform;
               var motionTypeProperty = element.FindPropertyRelative("MotionType");
               _editor.MotionType = motionTypeProperty.intValue;
               var positionModeProperty = element.FindPropertyRelative("PositionMode");
               _editor.PositionMode = positionModeProperty.intValue;
               var positionPointProperty = element.FindPropertyRelative("PositionPoint");
               _editor.PositionPoint = positionPointProperty.vector3Value;
               var positionTimeProperty = element.FindPropertyRelative("PositionTime");
               _editor.PositionTime = (uint)positionTimeProperty.intValue;
               var rotateStyleProperty = element.FindPropertyRelative("RotateStyle");
               _editor.RotateStyle = rotateStyleProperty.intValue;
               var rotateAngleProperty = element.FindPropertyRelative("RotateAngle");
               _editor.RotateAngle = rotateAngleProperty.vector3Value;
               var rotateTimeProperty = element.FindPropertyRelative("RotateTime");
               _editor.RotateTime = rotateTimeProperty.floatValue;
               var targetRotateAngleProperty = element.FindPropertyRelative("TargetRotate");
               _editor.TargetRotate = targetRotateAngleProperty.vector3Value;
               tempEnvironment.Add(_editor);
           }
       }
   }
}