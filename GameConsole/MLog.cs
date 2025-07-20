using UnityEngine;

namespace Framework.GameConsole
{
    public static class MLog
    {
        public static void Log(string message, GameObject context = null)
        {
            if (GameConsoleConfig.Instance.IsDebug)
            {
                Debug.Log(message, context);
            }
        }

        public static void LogWarning( string message, GameObject context)
        {
            if (GameConsoleConfig.Instance.IsDebug)
            {
                Debug.LogWarning(message, context);
            }
        }
    }
}