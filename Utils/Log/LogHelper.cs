using System.IO;
using UnityEngine;
using System.Diagnostics;

public enum LogLevel
{
  Error,
  Warning,
  Info,
  Debug,
}

public static class LogHelper
{
  public static LogLevel logLevel = LogLevel.Info;
  public static bool enableFileLog = false;
  public static bool enableConsole = false;
  public static int MaxLogBytesToUpload = 5242880;

  public static void EnableStackTraceLog()
  {
    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
    Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
    Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
  }

  public static void DisableStackTraceLog()
  {
    Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
    Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
    Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
  }

  public static void StartLogToFile(string path)
  {
    LogWriter.instance.Setup(path);
    Application.logMessageReceivedThreaded -= new Application.LogCallback(LogHelper.HandleUnityLogThreaded);
    Application.logMessageReceivedThreaded += new Application.LogCallback(LogHelper.HandleUnityLogThreaded);
  }

  public static void StopLogToFile()
  {
    Application.logMessageReceivedThreaded -= new Application.LogCallback(LogHelper.HandleUnityLogThreaded);
    LogWriter.instance.Shutdown();
    LogUploader.StopAll();
  }

  private static void HandleUnityLogThreaded(string logString, string stackTrace, LogType type)
  {
    LogWriter.instance.AppendLog(logString, type);
    if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
      return;
    LogWriter.instance.AppendLog(stackTrace, type);
  }

  public static void UploadLog(string path, Uploader uploader, bool finalDelete = true)
  {
    if (!File.Exists(path) || uploader == null)
      return;
    LogUploader.Upload(path, uploader, finalDelete);
  }

  [Conditional("DEBUG_LOG")]
  public static void Debug(object msg)
  {
    if (LogHelper.logLevel < LogLevel.Debug)
      return;
    UnityEngine.Debug.Log(msg);
  }

  [Conditional("DEBUG_LOG")]
  public static void Debug(object msg, UnityEngine.Object context)
  {
    if (LogHelper.logLevel < LogLevel.Debug)
      return;
    UnityEngine.Debug.Log(msg, context);
  }

  [Conditional("DEBUG_LOG")]
  public static void DebugFormat(string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Debug)
      return;
    UnityEngine.Debug.LogFormat(format, args);
  }

  [Conditional("DEBUG_LOG")]
  public static void DebugFormat(UnityEngine.Object context, string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Debug)
      return;
    UnityEngine.Debug.LogFormat(context, format, args);
  }

  public static void Info(object msg)
  {
    if (LogHelper.logLevel < LogLevel.Info)
      return;
    UnityEngine.Debug.Log(msg);
  }

  public static void Info(object msg, UnityEngine.Object context)
  {
    if (LogHelper.logLevel < LogLevel.Info)
      return;
    UnityEngine.Debug.Log(msg, context);
  }

  public static void InfoFormat(string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Info)
      return;
    UnityEngine.Debug.LogFormat(format, args);
  }

  public static void InfoFormat(UnityEngine.Object context, string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Info)
      return;
    UnityEngine.Debug.LogFormat(context, format, args);
  }

  public static void Warning(object msg)
  {
    if (LogHelper.logLevel < LogLevel.Warning)
      return;
    UnityEngine.Debug.LogWarning(msg);
  }

  public static void Warning(object msg, UnityEngine.Object context)
  {
    if (LogHelper.logLevel < LogLevel.Info)
      return;
    UnityEngine.Debug.LogWarning(msg, context);
  }

  public static void WarningFormat(string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Warning)
      return;
    UnityEngine.Debug.LogWarningFormat(format, args);
  }

  public static void WarningFormat(UnityEngine.Object context, string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Warning)
      return;
    UnityEngine.Debug.LogWarningFormat(context, format, args);
  }

  public static void Error(object msg)
  {
    if (LogHelper.logLevel < LogLevel.Error)
      return;
    UnityEngine.Debug.LogError(msg);
  }

  public static void Error(object msg, UnityEngine.Object context)
  {
    if (LogHelper.logLevel < LogLevel.Error)
      return;
    UnityEngine.Debug.LogError(msg, context);
  }

  public static void ErrorFormat(string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Warning)
      return;
    UnityEngine.Debug.LogErrorFormat(format, args);
  }

  [Conditional("DEBUG_LOG")]
  public static void ErrorFormat(UnityEngine.Object context, string format, params object[] args)
  {
    if (LogHelper.logLevel < LogLevel.Warning)
      return;
    UnityEngine.Debug.LogErrorFormat(context, format, args);
  }

  [Conditional("DEBUG_LOG")]
  public static void Exception(System.Exception ex)
  {
    if (LogHelper.logLevel < LogLevel.Error)
      return;
    UnityEngine.Debug.LogException(ex);
  }

  [Conditional("DEBUG_LOG")]
  public static void Exception(System.Exception exception, UnityEngine.Object context)
  {
    if (LogHelper.logLevel < LogLevel.Error)
      return;
    UnityEngine.Debug.LogException(exception, context);
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition, UnityEngine.Object context)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition, object message)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition, string message)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition, object message, UnityEngine.Object context)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void Assert(bool condition, string message, UnityEngine.Object context)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void AssertFormat(bool condition, string format, params object[] args)
  {
    int logLevel = (int) LogHelper.logLevel;
  }

  [Conditional("UNITY_ASSERTIONS")]
  public static void AssertFormat(
    bool condition,
    UnityEngine.Object context,
    string format,
    params object[] args)
  {
    int logLevel = (int) LogHelper.logLevel;
  }
}
