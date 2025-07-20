using UnityEngine;
using Unity.Collections;
using UnityEngine.Profiling;
using Framework.LocalizationHelper;

namespace Framework.GameConsole
{
    public partial class GameConsole
    {
        private void Init_System()
        {
            AddDebug("系统", OnGUI_System);
        }
        
        private void OnGUI_System()
        {
            IsShowFps = GUILayout.Toggle(IsShowFps, "显示FPS");
            
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("屏幕信息", "window");
                GUILayout.Label("DPI：" + Screen.dpi);
                GUILayout.Label("渲染分辨率：" + Camera.main.pixelWidth + "x" + Camera.main.pixelHeight);
                GUILayout.Label("渲染分辨率(scaled)：" + Camera.main.scaledPixelWidth + "x" + Camera.main.scaledPixelHeight);
                GUILayout.Label("分辨率：" + Screen.currentResolution);
                if (GUILayout.Button("全屏"))
                {
                    Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, !Screen.fullScreen);
                }
                GUILayout.EndVertical();
            }

            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("软件版本","window");
                GUILayout.Label("项目名称：" + Application.productName);
                GUILayout.Label("项目版本：" + Application.version);
                GUILayout.Label("Unity版本：" + Application.unityVersion);
                GUILayout.Label("公司名称：" + Application.companyName);
                GUILayout.EndVertical();
            }

            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("画质", "window");
                string value = "";
                if (QualitySettings.GetQualityLevel() == 0)
                {
                    value = " [最低]";
                }
                else if (QualitySettings.GetQualityLevel() == QualitySettings.names.Length - 1)
                {
                    value = " [最高]";
                }

                GUILayout.Label("图形质量：" + QualitySettings.names[QualitySettings.GetQualityLevel()] + value);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("降低一级图形质量"))
                {
                    QualitySettings.DecreaseLevel();
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("提升一级图形质量"))
                {
                    QualitySettings.IncreaseLevel();
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("语言", "window");

                var currentLanguageCode = LocalizationHelper.LocalizationHelper.CurrentLanguageCode;
                var selectedIndex = LocalizationConfig.Instance.EnableLanguages.IndexOf(currentLanguageCode);
                var newSelectedIndex = GUILayout.SelectionGrid(selectedIndex, LocalizationConfig.Instance.EnableLanguages.ToArray(), 2);
                if (newSelectedIndex != selectedIndex)
                {
                    LocalizationHelper.LocalizationHelper.CurrentLanguage = new System.Globalization.CultureInfo(LocalizationConfig.Instance.EnableLanguages[newSelectedIndex]);
                }

                GUILayout.EndVertical();
            }

            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("系统环境", "window");
                GUILayout.Label("操作系统：" + SystemInfo.operatingSystem);
                GUILayout.Label("语言：" + Application.systemLanguage);
                GUILayout.Label("系统内存：" + SystemInfo.systemMemorySize + "MB");
                GUILayout.Label("处理器：" + SystemInfo.processorType);
                GUILayout.Label("处理器数量：" + SystemInfo.processorCount);
                GUILayout.Label("显卡：" + SystemInfo.graphicsDeviceName);
                GUILayout.Label("显卡类型：" + SystemInfo.graphicsDeviceType);
                GUILayout.Label("API版本：" + SystemInfo.graphicsDeviceVersion);
                GUILayout.Label("显存：" + SystemInfo.graphicsMemorySize + "MB");
                GUILayout.Label("显卡标识：" + SystemInfo.graphicsDeviceID);
                GUILayout.Label("显卡供应商：" + SystemInfo.graphicsDeviceVendor);
                GUILayout.Label("显卡供应商标识码：" + SystemInfo.graphicsDeviceVendorID);
                GUILayout.Label("设备模式：" + SystemInfo.deviceModel);
                GUILayout.Label("设备名称：" + SystemInfo.deviceName);
                GUILayout.Label("设备类型：" + SystemInfo.deviceType);
                GUILayout.Label("设备标识：" + SystemInfo.deviceUniqueIdentifier);
                GUILayout.EndVertical();
            }
            
            {
                GUILayout.Space(10);
                GUILayout.BeginVertical("内存状态", "window");
                
                GUILayout.Label("总内存：" + Profiler.GetTotalReservedMemoryLong() / 1024 / 1024 + "MB");
                GUILayout.Label("已分配内存：" + Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024 + "MB");
                GUILayout.Label("未使用内存：" + Profiler.GetTotalUnusedReservedMemoryLong() / 1024 / 1024 + "MB");
                
                GUILayout.Label("Mono堆内存：" + Profiler.GetMonoHeapSizeLong() / 1024 / 1024 + "MB");
                GUILayout.Label("Mono已分配内存：" + Profiler.GetMonoUsedSizeLong() / 1024 / 1024 + "MB");

                GUILayout.BeginVertical("内存碎片状态", "window");
                if(GUILayout.Button("强制GC"))
                    System.GC.Collect();
                var freeStats = new NativeArray<int>(24, Allocator.Temp);
                var freeBlocks = Profiler.GetTotalFragmentationInfo(freeStats);
                GUILayout.Label($"空闲内存块总数量[{freeBlocks}]");
                for (int i = 0; i < 24; i++)
                {
                    if (freeStats[i] > 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label($"内存块大小[{Mathf.Pow(2, i)}]", GUILayout.Width(200));
                        GUILayout.Label($"数量[{freeStats[i]}]", GUILayout.Width(100));
                        GUILayout.EndHorizontal();
                    }
                }
                freeStats.Dispose();
                GUILayout.EndVertical();
                
                GUILayout.EndVertical();
            }
        }
    }
}