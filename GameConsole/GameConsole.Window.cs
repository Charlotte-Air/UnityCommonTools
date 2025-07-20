using System;
using Lean.Touch;
using UnityEngine;
using UnityEngine.UI;
using Framework.Utility;
using System.Collections.Generic;

namespace Framework.GameConsole
{
    class GameConsoleCategory
    {
        public string Name;
        public List<GameConsoleState> States = new();
        public Vector2 ScrollPosition;
    }
    
    public class GameConsoleState
    {
        public int Priority;
        public Action OnGUI;
        public Func<bool> IsShowNotice;
    }

    public struct GameConsoleCloseWindowEvent{}

    public partial class GameConsole
    {
        public bool IsOpenWindow { get; private set; }
        private GameObject _windowBackground;
        private GameConsoleCategory _currentCategory;
        private List<GameConsoleCategory> _debugStates = new();
        private Rect _windowRect;

        private void InitWindow()
        {
            _windowBackground = new GameObject("GM面板背景");
            _windowBackground.layer = LayerMask.NameToLayer("UI");
            var canvas = _windowBackground.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;
            var graphicRaycaster = _windowBackground.AddComponent<GraphicRaycaster>();
            graphicRaycaster.ignoreReversedGraphics = true;
            graphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.All;
            var image = _windowBackground.AddComponent<Image>();
            image.color = new Color(0, 0, 0, 0.3f);
            DontDestroyOnLoad(_windowBackground);
            _windowBackground.SetActive(false);

            _windowRect = new Rect(60, 10, 0.4f * _nativeResolution.x, 0.45f * _nativeResolution.y);
            
            // Test();
        }

        private void Test()
        {
            for (int i = 0; i < 100; i++)
            {
                AddDebug($"模拟经营{i}", () =>
                {
                    for (int i = 0; i < 100; i++)
                        GUILayout.Label($"Test{i}");
                });
                if(i%2 == 0)
                    AddDebug($"模拟经营{i}", () =>
                    {
                        for (int i = 0; i < 100; i++)
                            GUILayout.Label($"Test2{i}");
                    });
            }
        }
        
        public void OpenWindow()
        {
            IsOpenWindow = true;
            _windowBackground.SetActive(true);
            LeanTouch.Instance.UseTouch = false;
            LeanTouch.Instance.UseMouse = false;
        }
        
        public void CloseWindow()
        {
            IsOpenWindow = false;
            _windowBackground.SetActive(false);
            LeanTouch.Instance.UseTouch = true;
            LeanTouch.Instance.UseMouse = true;
            MessageSystem.Send(new GameConsoleCloseWindowEvent());
        }

        /// <summary> priority越小越先绘制 </summary>
        public static void AddDebug(string category, Action onGuiFun, int priority = 0, Func<bool> isShowNoticeFun = null)
        {
            if(GameConsoleConfig.Instance.IsDebug == false) 
                return;
            
            var list = Instance._debugStates.Find(x => x.Name == category);
            if (list == null)
            {
                list = new GameConsoleCategory() {Name = category};
                Instance._debugStates.Add(list);
            }
            list.States.Add(new GameConsoleState() {Priority = priority, OnGUI = onGuiFun, IsShowNotice = isShowNoticeFun});
            list.States.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }
        
        public static void RemoveDebug(Action onGuiFun)
        {
            if(GameConsoleConfig.Instance.IsDebug == false) 
                return;
            
            foreach (var category in Instance._debugStates)
            {
                category.States.RemoveAll(x => x.OnGUI == onGuiFun);
            }
        }

        private void OnGUI_Window()
        {
            if (IsOpenWindow)
            {
                UIResizing();
                _windowRect = GUI.Window(0, _windowRect, OnGUI_Window2, "GM面板");
            }
        }

        private Vector2 _scrollCategoryPosition;
        private void OnGUI_Window2(int windowId)
        {
            var windowAvailableWidth = _windowRect.width - 25;
            var cachedColor = GUI.color;
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
            
            // 绘制所有category按钮列表
            var spacing = 0;
            var currentAvailableWidth = windowAvailableWidth;
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.BeginHorizontal();
            foreach (var debugState in _debugStates)
            {
                var buttonWidth = GUI.skin.button.CalcSize(new GUIContent(debugState.Name)).x + spacing;
                if (currentAvailableWidth < buttonWidth)
                {
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    currentAvailableWidth = windowAvailableWidth;
                }
                currentAvailableWidth -= buttonWidth;
    
                var isCurrentCategory = debugState == _currentCategory;
                GUI.color = isCurrentCategory ? Color.green : Color.white;
                
                var isShowNotice = debugState.States.Exists(x => x.IsShowNotice?.Invoke() ?? false);
                if (isShowNotice && !isCurrentCategory)
                {
                    GUI.color = Color.Lerp(Color.red, Color.white, Mathf.PingPong(Time.time, 0.5f));
                }
                
                if (GUILayout.Button(debugState.Name))
                {
                    _currentCategory = debugState;
                }
                GUI.color = cachedColor;
                GUILayout.Space(spacing);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            
            // 绘制当前category下的所有debug
            if (_currentCategory != null)
            {
                _currentCategory.ScrollPosition = GUILayout.BeginScrollView(_currentCategory.ScrollPosition, "Box", GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                foreach (var debugState in _currentCategory.States)
                {
                    debugState.OnGUI?.Invoke();
                }
                GUILayout.EndScrollView();
            }
        }
        
        private float _windowScaleFactor = -1.0f;
        private void UIResizing()
        {
            Matrix4x4 m = new Matrix4x4();
            _windowScaleFactor = Mathf.Min(Screen.width / _nativeResolution.x, Screen.height / _nativeResolution.y)*2;
            m.SetTRS(Vector3.one, Quaternion.identity, Vector3.one * _windowScaleFactor);
            GUI.matrix *= m;
        }
    }
}