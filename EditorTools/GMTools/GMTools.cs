using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
namespace Charlotte.Client.GM
{
    /// <summary>
    /// GM命令工具
    /// </summary>
    public class GMTools : EditorWindow
    {
        [MenuItem("Tools/Game GM面板")]
        public static void GMtoolWindow()
        {
            GMTools window = (GMTools)GMTools.GetWindow(typeof(GMTools), false, "GM面板", true);
            window.Show();
        }

        private GMCommand delCommand;
        private GMvo.GMType SelectType = GMvo.GMType.None;
        private List<GMCommand> gmCommands = new List<GMCommand>();
        private static List<GMCommand> realCommands = new List<GMCommand>();
        private const string GmFiles = "Assets/Editor/GM/Files";
        private const string AssetsGmFiles = "Assets/Editor/GM/Files";
        private const string GmFilesXml = "Assets/Editor/GM/Files/GM.xml";


        private Vector2 mScroll;
        private Vector2 mScrollt;
        private Vector2 mScrolltt;
        private List<CGFile> listPath = new List<CGFile>();

        private static GMTools instance;
        public static GMTools Instance => instance;

        public GMTools()
        {
            instance = this;
            Load();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        void Load()
        {
            if (LoadCommand(GmFilesXml))
            {
                LoadCommands();
            }
        }

        /// <summary>
        /// 加载GM-XML文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        bool LoadCommand(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
            TextReader tr = sr;
            string text = tr.ReadToEnd();
            XMLParser parser = new XMLParser();
            XMLNode node = parser.Parse(text);
            XMLNodeList list = node.GetNodeList("Objects>0>GM");
            gmCommands.Clear();
            for (int i = 0; i < list.Count; i++)
            {
                XMLNode n = list[i] as XMLNode;
                GMvo item = new GMvo();
                item.id = int.Parse(n.GetValue("@Id"));
                item.type = (GMvo.GMType)int.Parse(n.GetValue("@Type"));
                item.command = n.GetValue("@Command");
                item.desc = n.GetValue("@Desc");

                gmCommands.Add(new GMCommand(item));
            }

            sr.Close();
            fs.Close();
            tr.Close();
            return true;
        }

        /// <summary>
        /// 加载GM-文本文件
        /// </summary>
        void LoadCommands()
        {
            listPath.Clear();
            string[] paths = Directory.GetFiles(AssetsGmFiles, "*.txt");
            string[] paths1 = Directory.GetFileSystemEntries(AssetsGmFiles, "*.txt");
            if (paths1.Length > 0)
            {
                FileInfo info;
                for (int i = 0; i < paths.Length; i++)
                {
                    string path = paths[i];
                    string name = path.Remove(0, 23);
                    info = new FileInfo(path);
                    listPath.Add(new CGFile(name, path, i, info.CreationTimeUtc.Ticks));
                }

                listPath.Sort(CompareTicks);
            }
        }

        private int CompareTicks(CGFile file1, CGFile file2)
        {
            if (file1.ticks > file2.ticks)
                return -1;
            else if (file1.ticks < file2.ticks)
                return 1;
            else
                return 0;
        }

        private string gmStr = string.Empty;

        void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.ExpandHeight(true);
            GUILayout.ExpandWidth(true);

            /****************************************************************************/
            mScrolltt = GUILayout.BeginScrollView(mScrolltt);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open GM Folder", GUILayout.Width(120)))
            {
                System.Diagnostics.Process.Start($"{Application.dataPath}/{GmFiles}");
            }

            if (GUILayout.Button("Update", GUILayout.Width(100)))
            {
                Load();
            }

            gmStr = TextField(gmStr, GUILayout.Width(300));
            if (GUILayout.Button("Send"))
            {
                ServerCommand.Send(gmStr);
            }

            GUILayout.EndHorizontal();

            foreach (CGFile path in listPath)
            {
                path.OnGUI();
            }

            GUILayout.EndScrollView();

            /****************************************************************************/

            GUILayout.BeginHorizontal();

            mScroll = GUILayout.BeginScrollView(mScroll);

            SelectType = (GMvo.GMType)EditorGUILayout.EnumPopup(SelectType, GUILayout.Width(150));

            foreach (GMCommand command in gmCommands)
            {
                if (SelectType == GMvo.GMType.None || command.data.type == SelectType)
                {
                    command.OnGUI();
                }
            }

            GUILayout.EndScrollView();

            /****************************************************************************/

            GUILayout.BeginHorizontal();

            mScrollt = GUILayout.BeginScrollView(mScrollt);

            GUILayout.BeginHorizontal();
            if (realCommands.Count > 0)
            {
                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    realCommands.Clear();
                }

                if (GUILayout.Button("Save", GUILayout.Width(100)))
                {
                    ProcessSave();
                    AssetDatabase.Refresh();
                }

                if (GUILayout.Button("Execute", GUILayout.Width(100)))
                {
                    OnExecuteClick();
                }
            }

            GUILayout.EndHorizontal();

            foreach (GMCommand command in realCommands)
            {
                command.OnDraw();
            }

            if (delCommand != null)
            {
                realCommands.Remove(delCommand);
                delCommand = null;
            }

            GUILayout.EndScrollView();

            /****************************************************************************/

            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// 添加命令
        /// </summary>
        /// <param name="command"></param>
        public void AddCommand(GMCommand command)
        {
            realCommands.Add(command);
        }

        /// <summary>
        /// 删除命令
        /// </summary>
        /// <param name="command"></param>
        public void RemoveCommand(GMCommand command)
        {
            delCommand = command;
        }

        /// <summary>
        /// 通过命令名称-获得命令
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public GMCommand GetCommand(string value)
        {
            string[] str = value.Split(new char[] { '=' }, System.StringSplitOptions.RemoveEmptyEntries);
            GMvo vo;
            for (int i = 0; i < gmCommands.Count; i++)
            {
                vo = gmCommands[i].data;
                if (value.IndexOf(vo.realCommand) > -1)
                {
                    vo = vo.Clone();
                    if (str.Length >= 2)
                    {
                        vo.param_0 = str[1];
                    }

                    if (str.Length >= 3)
                    {
                        vo.param_1 = str[2];
                    }

                    if (str.Length >= 4)
                    {
                        vo.param_2 = str[3];
                    }

                    return new GMCommand(vo);
                }
            }

            return null;
        }

        /// <summary>
        /// 执行GM命令
        /// </summary>
        private void OnExecuteClick()
        {
            if (Application.isPlaying)
            {
                GMCommand command;
                for (int i = 0; i < realCommands.Count; i++)
                {
                    command = realCommands[i];
                    ServerCommand.Send(command.ToString());
                }
            }
        }

        /// <summary>
        /// 保存
        /// </summary>
        private void ProcessSave()
        {
            string path = EditorUtility.SaveFilePanel("Save GM", AssetsGmFiles, "", "txt");
            if (string.IsNullOrEmpty(path))
                return;
            if (File.Exists(path))
                File.Delete(path);

            FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sr = new StreamWriter(fs, System.Text.Encoding.UTF8);
            TextWriter tr = sr;
            GMCommand[] coss = realCommands.ToArray();
            for (int i = 0; i < coss.Length; i++)
            {
                tr.WriteLine(coss[i].ToSaveString());
            }

            sr.Close();
            fs.Close();
            tr.Close();
            Debug.Log("保存 " + path + " 成功");
        }

        #region GMVo

        public class GMvo
        {
            public enum GMType
            {
                None = 0,
                System = 1,
                Player = 2,
                Item = 3,
                Task = 4,
            }

            public GMvo Clone()
            {
                GMvo v = new GMvo();
                v.id = id;
                v.command = command;
                v.desc = desc;
                v.type = type;
                v.param_0 = param_0;
                v.param_1 = param_1;
                v.param_2 = param_2;
                return v;
            }

            public int id;
            public string desc;
            public GMType type;
            public string command;
            public string param_0 = "";
            public string param_1 = "";
            public string param_2 = "";

            public string realCommand
            {
                get => command.Substring(1);
            }
        }

        #endregion


        #region File

        public class CGFile
        {
            public static CGFile SelectPath = null;
            public long ticks;
            public string mName;
            public string mPath;
            public int index;

            public CGFile(string name, string path, int index, long time)
            {
                mName = name;
                mPath = path;
                this.index = index;
                this.ticks = time;
            }

            public void OnGUI()
            {
                Color color = Color.grey;
                if (SelectPath == this)
                {
                    color = Color.red;
                }
                else
                {
                    if (index % 2 == 0)
                    {
                        color = Color.green;
                    }
                }

                GUI.backgroundColor = color;
                GUILayout.BeginHorizontal("Button");

                EditorGUILayout.LabelField(mName.Remove(mName.Length - 4, 4), GUILayout.Width(250));

                if (GUILayout.Button("执行", GUILayout.Width(60)))
                {
                    ExecuteCommand(mPath);
                }

                if (GUILayout.Button("选择", GUILayout.Width(60)))
                {
                    SelectPath = this;
                    SelectCommand(mPath);
                }

                GUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;
            }

            private void ExecuteCommand(string path)
            {
                realCommands.Clear();
                if (string.IsNullOrEmpty(path))
                    return;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
                TextReader tr = sr;
                string text = tr.ReadToEnd();
                string[] strLines = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                string line = string.Empty;
                for (int i = 0; i < strLines.Length; i++)
                {
                    line = strLines[i];

                    if (line.StartsWith("|"))
                    {
                        ServerCommand.Send(line.Substring(1));
                    }
                    else
                    {
                        GMCommand command = Instance.GetCommand(line);
                        if (command != null)
                        {
                            ServerCommand.Send(command.ToString());
                        }
                    }
                }

                sr.Close();
                fs.Close();
                tr.Close();
            }

            /// <summary>
            /// Open 按钮
            /// </summary>
            private void SelectCommand(string path)
            {
                realCommands.Clear();

                if (string.IsNullOrEmpty(path)) return;

                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8);
                TextReader tr = sr;
                string text = tr.ReadToEnd();
                string[] strLines = text.Split(new string[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < strLines.Length; i++)
                {
                    GMCommand command = Instance.GetCommand(strLines[i]);
                    if (command != null)
                    {
                        realCommands.Add(command);
                    }
                }

                sr.Close();
                fs.Close();
                tr.Close();
            }
        }

        #endregion


        #region

        public class ServerCommand
        {
            private static MethodInfo myMethodInfo;

            static MethodInfo GetMethod()
            {
                if (myMethodInfo == null)
                {
                    Assembly[] assemblys = System.AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly assem in assemblys)
                    {
//                    if (assem.ManifestModule.Name == "Assembly-CSharp.dll")
//                    {
//                        System.Type command = assem.GetType("GameCommand");
//                        myMethodInfo = command.GetMethod("SendGM");
//                    }
                        System.Type command = assem.GetType("GameCommand");
                        if (command != null)
                        {
                            myMethodInfo = command.GetMethod("SendGM");
                        }
                    }
                }

                return myMethodInfo;
            }

            /// <summary>
            /// 发送GM命令
            /// </summary>
            /// <param name="value"></param>
            public static void Send(string value)
            {
                if (GetMethod() != null)
                {
                    GetMethod().Invoke(null, new object[]
                    {
                        value
                    });
                }
            }
        }

        #endregion


        #region GMCommand

        public class GMCommand
        {
            public GMvo data;

            public GMCommand(GMvo vo)
            {
                data = vo;
            }

            /// <summary>
            /// 保存gm命令
            /// </summary>
            /// <returns></returns>
            public string ToSaveString()
            {
                StringBuilder str = new StringBuilder();
                str.Append(data.realCommand);
                if (!string.IsNullOrEmpty(data.param_0))
                {
                    str.Append("=");
                    str.Append(data.param_0);
                }

                if (!string.IsNullOrEmpty(data.param_1))
                {
                    str.Append("=");
                    str.Append(data.param_1);
                }

                if (!string.IsNullOrEmpty(data.param_2))
                {
                    str.Append("=");
                    str.Append(data.param_2);
                }

                return str.ToString();
            }

            /// <summary>
            /// 获得完成gm命令
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                StringBuilder str = new StringBuilder();
                str.Append(data.realCommand);
                if (!string.IsNullOrEmpty(data.param_0))
                {
                    str.Append(" ");
                    str.Append(data.param_0);
                }

                if (!string.IsNullOrEmpty(data.param_1))
                {
                    str.Append(" ");
                    str.Append(data.param_1);
                }

                if (!string.IsNullOrEmpty(data.param_2))
                {
                    str.Append(" ");
                    str.Append(data.param_2);
                }

                return str.ToString();
            }

            /// <summary>
            /// 选择队列
            /// </summary>
            public void OnGUI()
            {
                GUILayout.BeginVertical("Button");
                GUILayout.ExpandHeight(true);
                GUILayout.ExpandWidth(true);
                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(data.command, GUILayout.Width(250));
                if (GUILayout.Button("执行", GUILayout.Width(75)))
                {
                    ServerCommand.Send(ToString());
                }

                if (GUILayout.Button("添加", GUILayout.Width(75)))
                {
                    GMTools.Instance.AddCommand(new GMCommand(data.Clone()));
                }

                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(data.desc, GUILayout.Width(400));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();

                GUI.backgroundColor = Color.white;
            }

            /// <summary>
            /// 命令队列
            /// </summary>
            public void OnDraw()
            {
                GUILayout.BeginHorizontal("Button");
                EditorGUILayout.LabelField("命令:", GUILayout.Width(40));
                EditorGUILayout.TextField(data.command, GUILayout.Width(150));
                EditorGUILayout.LabelField("参数1:", GUILayout.Width(40));
                data.param_0 = EditorGUILayout.TextField(data.param_0, GUILayout.Width(80));
                EditorGUILayout.LabelField("参数2:", GUILayout.Width(40));
                data.param_1 = EditorGUILayout.TextField(data.param_1, GUILayout.Width(80));
                EditorGUILayout.LabelField("参数3:", GUILayout.Width(40));
                data.param_2 = EditorGUILayout.TextField(data.param_2, GUILayout.Width(80));
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    GMTools.Instance.RemoveCommand(this);
                }

                GUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
            }
        }

        #endregion


        #region Tool

        static string TextField(string value, params GUILayoutOption[] options)
        {
            int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
            if (textFieldID == 0)
                return value;

            value = HandleCopyPaste(textFieldID) ?? value;
            return GUILayout.TextField(value, options);
        }

        static string HandleCopyPaste(int controlID)
        {
            if (controlID == GUIUtility.keyboardControl)
            {
                if (Event.current.type == UnityEngine.EventType.KeyUp &&
                    (Event.current.modifiers == EventModifiers.Control ||
                     Event.current.modifiers == EventModifiers.Command))
                {
                    if (Event.current.keyCode == KeyCode.C)
                    {
                        Event.current.Use();
                        UnityEngine.TextEditor editor =
                            (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                                GUIUtility.keyboardControl);
                        editor.Copy();
                    }
                    else if (Event.current.keyCode == KeyCode.V)
                    {
                        Event.current.Use();
                        UnityEngine.TextEditor editor =
                            (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                                GUIUtility.keyboardControl);
                        editor.Paste();
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                        return editor.text; //以及更高的unity版本中editor.content.text已经被废弃，需使用editor.text代替
#else
                    return editor.content.text;
#endif
                    }
                    else if (Event.current.keyCode == KeyCode.A)
                    {
                        Event.current.Use();
                        UnityEngine.TextEditor editor =
                            (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                                GUIUtility.keyboardControl);
                        editor.SelectAll();
                    }
                }
            }

            return null;
        }

        #endregion
    }
}