using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Framework.GameConsole
{
    public class LogData
    {
        public ulong Id;
        public LogType Type;
        public string TimeStr;
        public string TypeStr;
        public string Message;
        public string StackTrace;
    }
    
    public partial class GameConsole
    {
        private bool _showInfoLog = true;
        private bool _showWarningLog = true;
        private bool _showErrorLog = true;
        private bool _showFatalLog = true;
        private int _infoLogCount = 0;
        private int _warningLogCount = 0;
        private int _errorLogCount = 0;
        private int _fatalLogCount = 0;
        private List<LogData> _logInformations = new();
        private ulong _logId = 0;

        private void Init_Log()
        {
            _logInformations.Capacity = GameConsoleConfig.Instance.MaxLogCount + 50;
            Application.logMessageReceivedThreaded += LogHandler;
            GameConsole.AddDebug("日志", OnGUI_Log, 0, () => _errorLogCount > 0 || _fatalLogCount > 0);
        }
        
        private Color btnHighlightedColor = Color.green;
        private Color btnNormalColor = new Color(0.8f, 0.8f, 0.8f);
        private Vector2 _scrollLogView = Vector2.zero;
        private Vector2 _scrollCurrentLogView = Vector2.zero;
        private ulong? _currentLogId = null;
        private void OnGUI_Log()
        {
            GUILayout.BeginHorizontal();
            
            GUI.contentColor = (_showInfoLog ? btnHighlightedColor : btnNormalColor);
            _showInfoLog = GUILayout.Toggle(_showInfoLog, "Info[" + _infoLogCount + "]");
            GUI.contentColor = (_showWarningLog ? btnHighlightedColor : btnNormalColor);
            _showWarningLog = GUILayout.Toggle(_showWarningLog, "Warning[" + _warningLogCount + "]");
            GUI.contentColor = (_showErrorLog ? btnHighlightedColor : btnNormalColor);
            _showErrorLog = GUILayout.Toggle(_showErrorLog, "Error[" + _errorLogCount + "]");
            GUI.contentColor = (_showFatalLog ? btnHighlightedColor : btnNormalColor);
            _showFatalLog = GUILayout.Toggle(_showFatalLog, "Fatal[" + _fatalLogCount + "]");
            GUI.contentColor = Color.white;
            
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            _scrollLogView = GUILayout.BeginScrollView(_scrollLogView, "Box", GUILayout.ExpandHeight(true));
            for (int i = 0; i < _logInformations.Count; i++)
            {
                var log = _logInformations[i];
                bool show = false;
                Color color = Color.white;
                switch (_logInformations[i].TypeStr)
                {
                    case "Fatal":
                        show = _showFatalLog;
                        color = Color.red;
                        break;
                    case "Error":
                        show = _showErrorLog;
                        color = Color.red;
                        break;
                    case "Info":
                        show = _showInfoLog;
                        color = Color.white;
                        break;
                    case "Warning":
                        show = _showWarningLog;
                        color = Color.yellow;
                        break;
                }

                if (show)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(_currentLogId == log.Id, ""))
                    {
                        _currentLogId = log.Id;
                    }
                    GUI.contentColor = color;
                    GUILayout.Label($"[{_logInformations[i].TypeStr}][{_logInformations[i].TimeStr}] {_logInformations[i].Message}");
                    GUILayout.FlexibleSpace();
                    GUI.contentColor = Color.white;
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndScrollView();

            if (_currentLogId.HasValue)
            {
                LogData currentLog = null;
                for (int i = 0; i < _logInformations.Count; i++)
                {
                    if (_logInformations[i].Id == _currentLogId.Value)
                    {
                        currentLog = _logInformations[i];
                        break;
                    }
                }

                if (currentLog != null)
                {
                    var cachedWordWrap = GUI.skin.textArea.wordWrap;
                    GUI.skin.textArea.wordWrap = false;
                    var logStr = $"消息：{currentLog.Message}{Environment.NewLine}" +
                                 $"堆栈：{Environment.NewLine}{currentLog.StackTrace}";
                    var logSize = GUI.skin.textArea.CalcSize(new GUIContent(logStr));
                    float currentLogViewHeight = Mathf.Min(300f, logSize.y+25);
                    _scrollCurrentLogView = GUILayout.BeginScrollView(_scrollCurrentLogView, GUILayout.Height(currentLogViewHeight));
                    GUILayout.TextArea(logStr, GUILayout.Width(logSize.x));
                    GUILayout.EndScrollView();
                    GUI.skin.textArea.wordWrap = cachedWordWrap;
                }
            }
        }

        private void LogHandler(string condition, string stackTrace, LogType type)
        {
            LogHandler2(condition, stackTrace, type).Forget();
        }
        
        private async UniTaskVoid LogHandler2(string condition, string stackTrace, LogType type)
        {
            await UniTask.SwitchToMainThread();

            var log = Lean.Pool.LeanClassPool<LogData>.Spawn() ?? new LogData();
            log.Id = _logId++;
            log.Type = type;
            log.TimeStr = DateTime.Now.ToString("HH:mm:ss");
            log.Message = condition;
            log.StackTrace = stackTrace;

            switch (type)
            {
                case LogType.Assert:
                    log.TypeStr = "Fatal";
                    _fatalLogCount += 1;
                    _logInformations.Add(log);
                    break;
                case LogType.Exception:
                case LogType.Error:
                    log.TypeStr = "Error";
                    _errorLogCount += 1;
                    _logInformations.Add(log);
                    break;
                case LogType.Warning:
                    log.TypeStr = "Warning";
                    _warningLogCount += 1;
                    _logInformations.Add(log);
                    break;
                case LogType.Log:
                    log.TypeStr = "Info";
                    _infoLogCount += 1;
                    _logInformations.Add(log);
                    break;
            }
            if (_infoLogCount > GameConsoleConfig.Instance.MaxLogCount) 
            {
                for (int i = 0; i < _logInformations.Count; i++)
                {
                    if (_logInformations[i].Type == LogType.Log)
                    {
                        var removeLog = _logInformations[i];
                        if(_currentLogId == removeLog.Id) _currentLogId = null;
                        Lean.Pool.LeanClassPool<LogData>.Despawn(removeLog);
                        _logInformations.RemoveAt(i);
                        _infoLogCount -= 1;
                        break;
                    }
                }
            }
        }
    }
}