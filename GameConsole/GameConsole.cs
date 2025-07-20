using UnityEngine;

namespace Feamber.Framework.GameConsole
{
    public partial class GameConsole : MonoBehaviour
    {
        public static GameConsole Instance { get; private set; }
        private Vector2 _nativeResolution;

        [RuntimeInitializeOnLoadMethod]
        private static void Init()
        {
            if(GameConsoleConfig.Instance.IsDebug == false) 
                return;
            
            Instance = new GameObject("GameConsoleInstance").AddComponent<GameConsole>();
            DontDestroyOnLoad(Instance);
            Instance.IsShowFps = GameConsoleConfig.Instance.IsShowFps;
            Instance._nativeResolution = GameConsoleConfig.Instance.NativeResolution;
            Instance.InitWindow();
            Instance.Init_System();
            Instance.Init_Log();
        }
        
        private void OnGUI()
        {
            OnGUI_Entry();
            OnGUI_Window();
        }
    }
}