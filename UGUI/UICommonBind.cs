using System;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

#if UNITY_EDITOR
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

public class UICommonBind : MonoBehaviour
{
    public enum BindUnitType
    {
        Component,
        GameObject,
    }

    [Serializable]
    public struct BindUnit
    {
        public string tag;
        public Component com;
        public BindUnitType type;
    }

    [HideInInspector]
    public BindUnit[] units = new BindUnit[0];
    private Dictionary<string, Component> dic = null;

#if UNITY_EDITOR

    public static bool bUpdate = true;
    [OnInspectorGUI]
    private void OnInspectorGUI1()
    {
        if (Application.isPlaying)
        {
            UnityEditor.EditorGUILayout.HelpBox("运行时无法保存！", UnityEditor.MessageType.Warning);
        }

        SirenixEditorGUI.Title("Bind System For UI", string.Format("total count is {0}", units == null ? 0 : units.Length), TextAlignment.Center, true, true);
        bUpdate = GUILayout.Toggle(bUpdate, "Update");
    }

    StringBuilder sbCache = new StringBuilder();
    string tempStr = string.Empty;
    [HorizontalGroup("CodeHelp", 0.5f)]
    [Button("变量获取", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    public void CopyVarGetCode()
    {

        sbCache.Clear();
        sbCache.Append(@"bind = gameObject.GetComponent<UICommonBind>();");
        sbCache.Append("\n");
        BindUnit unit;
        for (int i = 0; i < units.Length; i++)
        {
            unit = units[i];
            if (unit.type == BindUnitType.Component)
            {
                tempStr = unit.com.GetType().ToString();
            }
            else
            {
                tempStr = typeof(GameObject).ToString();
            }
            if (tempStr.IndexOf(".") > 0)
            {
                tempStr = tempStr.Substring(tempStr.LastIndexOf(".") + 1);
            }
            string code = string.Format("{0} = bind.Get<{1}>(\"{2}\");", unit.tag, tempStr, unit.tag);
            sbCache.Append(code);
            sbCache.Append("\n");
        }
        GUIUtility.systemCopyBuffer = sbCache.ToString();
    }
    [HorizontalGroup("CodeHelp", 0.5f)]
    [Button("变量定义", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    public void CopyVarDefineCode()
    {
        sbCache.Clear();
        sbCache.Append(@"UICommonBind bind;");
        sbCache.Append("\n");
        BindUnit unit;
        for (int i = 0; i < units.Length; i++)
        {
            unit = units[i];
            if (unit.type == BindUnitType.Component)
            {
                tempStr = unit.com.GetType().ToString();
            }
            else
            {
                tempStr = typeof(GameObject).ToString();
            }
            if (tempStr.IndexOf(".") > 0)
            {
                tempStr = tempStr.Substring(tempStr.LastIndexOf(".") + 1);
            }

            string code = string.Format(@"private {0} {1};", tempStr, unit.tag);
            sbCache.Append(code);
            sbCache.Append("\n");
        }
        GUIUtility.systemCopyBuffer = sbCache.ToString();
    }

#endif

    [DisableInPlayMode]
    [HorizontalGroup("Split", 0.5f)]
    [Button("+", ButtonSizes.Small), GUIColor(0.4f, 0.8f, 1)]
    private void OnClickBtnAdd()
    {
        AddField(null);
    }

    [DisableInPlayMode]
    [VerticalGroup("Split/right")]
    [Button("-", ButtonSizes.Small), GUIColor(1, 0, 0)]
    private void OnClickBtnRemove()
    {
        Array.Resize(ref units, Mathf.Clamp(units.Length - 1, 0, units.Length - 1));
    }

    public void AddField(GameObject go)
    {
        BindUnit unit = new BindUnit();
        unit.com = go == null ? null : go.GetComponent<RectTransform>();
        unit.tag = go == null ? string.Empty : go.name;
        unit.type = BindUnitType.Component;
        Array.Resize(ref units, units.Length + 1);
        units[units.Length - 1] = unit;
    }

    bool inited = false;

    void Init()
    {
        if (!inited)
        {
            if (dic == null)
                dic = new Dictionary<string, Component>();
            if (units != null)
            {
                for (int i = 0; i < units.Length; i++)
                {
                    if (!dic.ContainsKey(units[i].tag))
                        dic.Add(units[i].tag, units[i].com);
                }
            }
            inited = true;
        }
    }

    public void ChangeFieldName(string newName, string oldName)
    {
        for (int i = 0; i < units.Length; i++)
        {
            if (oldName == units[i].tag)
                units[i].tag = newName;
        }
    }

    public bool ContainField(GameObject go, ref string name)
    {
        if (units == null)
            return false;

        for (int i = 0; i < units.Length; i++)
        {
            object o = units[i].com;
            if (o == null)
                continue;
            if (o is MonoBehaviour)
            {
                MonoBehaviour m = o as MonoBehaviour;
                GameObject tmpGo = m.gameObject;
                if (tmpGo == go)
                {
                    name = units[i].tag;
                    return true;
                }
            }
            else if (o is GameObject)
            {
                GameObject tmpGo = o as GameObject;
                if (tmpGo == go)
                {
                    name = units[i].tag;
                    return true;
                }
            }
            else if (o is Component)
            {
                Component c = o as Component;
                if (c != null && c.gameObject == go)
                {
                    name = units[i].tag;
                    return true;
                }
            }
        }
        return false;
    }

    public void RemoveFieldByGo(GameObject go)
    {
        for (int i = 0; i < units.Length; i++)
        {
            List<BindUnit> list = new List<BindUnit>(units);
            object o = units[i].com;
            if (o is GameObject)
            {
                GameObject tmpGo = o as GameObject;
                if (tmpGo == go)
                {
                    list.RemoveAt(i);
                    Array.Resize(ref units, units.Length - 1);
                    units = list.ToArray();
                    break;
                }
            }
            else if (o is MonoBehaviour)
            {
                MonoBehaviour tmpGo = o as MonoBehaviour;
                if (tmpGo == null || tmpGo.gameObject == go)
                {
                    list.RemoveAt(i);
                    Array.Resize(ref units, units.Length - 1);
                    units = list.ToArray();
                    break;
                }
            }
            else if (o is Component)
            {
                Component tmpGo = o as Component;

                if (tmpGo == null || tmpGo.gameObject == go)
                {
                    list.RemoveAt(i);
                    Array.Resize(ref units, units.Length - 1);
                    units = list.ToArray();
                    break;
                }
            }
        }
    }

    public T Get<T>(string key) where T : UnityEngine.Object
    {
        Init();
        if (dic.ContainsKey(key))
        {
            if (typeof(T) == typeof(GameObject))
            {
                return dic[key].gameObject as T;
            }
            else
            {
                return dic[key] as T;
            }
        }
        return null;
    }

#if UNITY_EDITOR


    [OnInspectorGUI]
    private void OnInspectorGUI2()
    {
        EditorGUI.BeginChangeCheck();
        int toRemove = -1;

        Color blueColor = new Color(0.4f, 0.8f, 1);

        SirenixEditorGUI.BeginVerticalList(false, false);


        for (int i = 0; i < units.Length; i++)
        {
            if (i % 2 == 0)
            {
                GUIHelper.PushColor(Color.white);
                GUIHelper.PushContentColor(blueColor);
            }
            else
            {
                GUIHelper.PushColor(Color.cyan);
                GUIHelper.PushContentColor(Color.cyan);
            }

            var unit = units[i];

            SirenixEditorGUI.BeginBox();

            EditorGUILayout.BeginHorizontal();

            if (SirenixEditorGUI.IconButton(EditorIcons.Minus, 20, 20))
            {
                toRemove = i;
            }

            EditorGUILayout.LabelField(string.Format("[{0}]:{1}", i, unit.tag), GUILayout.MinWidth(60), GUILayout.MaxWidth(180));

            unit.tag = SirenixEditorFields.TextField(unit.tag);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (unit.com == null)
                unit.com = EditorGUILayout.ObjectField(unit.com, typeof(Component), true) as Component;
            else
                unit.com = EditorGUILayout.ObjectField(unit.com, typeof(Component), true, GUILayout.MinWidth(80), GUILayout.MaxWidth(200)) as Component;

            if (unit.com != null)
            {
                Component[] coms = unit.com.GetComponents<Component>();
                string[] comsName = new string[coms.Length + 2];
                UnityEngine.Object[] objs = new UnityEngine.Object[coms.Length + 2];
                for (var j = 0; j < coms.Length; j++)
                {
                    if (coms[j] != null)
                    {
                        comsName[j] = coms[j].GetType().Name;
                        objs[j] = coms[j];
                    }
                    else
                    {
                        comsName[j] = "missing";
                        objs[j] = null;
                    }
                }
                objs[objs.Length - 1] = unit.com.gameObject;
                comsName[objs.Length - 1] = "GameObject";
                //				unit.com = SirenixEditorFields.Dropdown<Component> (unit.com, coms, comsName);
                UnityEngine.Object selected = null;
                if (unit.type == BindUnitType.Component)
                {
                    selected = unit.com;
                }
                else
                {
                    selected = unit.com.gameObject;
                }
                selected = SirenixEditorFields.Dropdown<UnityEngine.Object>(selected, objs, comsName);

                if (selected is GameObject)
                {
                    unit.type = BindUnitType.GameObject;
                }
                else
                {
                    unit.type = BindUnitType.Component;
                    unit.com = (Component)selected;
                }

            }


            EditorGUILayout.EndHorizontal();

            units[i] = unit;

            GUIHelper.PopColor();
            GUIHelper.PopContentColor();

            SirenixEditorGUI.EndBox();

            EditorGUILayout.Space();

            if (i < (units.Length - 1))
            {
                SirenixEditorGUI.HorizontalLineSeparator();

                EditorGUILayout.Space();
            }
        }

        SirenixEditorGUI.EndVerticalList();

        if (toRemove >= 0)
        {
            ArrayUtility.Remove<BindUnit>(ref units, units[toRemove]);
        }

        if (EditorGUI.EndChangeCheck())
            EditorUtility.SetDirty(this.gameObject);
    }

    [ContextMenu("--检查器--")]
    public void CheckValid()
    {
        foreach (var unit in units)
        {
            if (unit.com == null)
            {
#if UNITY_5
				UnityEngine.Object root = PrefabUtility.FindPrefabRoot (gameObject);
#else
                UnityEngine.Object root = gameObject.transform.root.gameObject;
#endif

                LogHelper.ErrorFormat(gameObject, "UICommonBind of {0}, has invalid element named {1}", root == gameObject ? gameObject.name : string.Format("{0} : {1}", root.name, gameObject.name), unit.tag);
            }
        }
    }
#endif


}