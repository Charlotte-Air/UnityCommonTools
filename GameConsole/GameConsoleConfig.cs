using UnityEngine;
using Sirenix.OdinInspector;
using Framework.ConfigHelper;
using UnityEngine.Serialization;

namespace Framework.GameConsole
{
    [ConfigMenu(category = "不常用/GM面板")]
    public class GameConsoleConfig : GameConfig<GameConsoleConfig>
    {
        [FormerlySerializedAs("IsEnable")]
        [LabelText("Debug模式,启用GM面板,启用debugLog")]
        public bool IsDebug = true;
        
        [LabelText("最大缓存日志数量")]
        public int MaxLogCount = 200;
        
        [LabelText("目标分辨率")]
        public Vector2 NativeResolution = new Vector2(1080, 1920);
        
        [LabelText("默认是否显示FPS")]
        public bool IsShowFps = true;
    }
}