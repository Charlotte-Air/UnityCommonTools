using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace Framework.Manager
{
    public class GameManager
    {
        private static GameManager _instance;
        public static GameManager Instance
        {
            get
            {
                if (_instance == null) 
                    _instance = new GameManager();
                return _instance;
            }
        }
        
        public int m_GameSceneWidth { private set; get; }
        public int m_GameSceneHeight { private set; get; }
        public int m_GameSceneIndex { private set; get; } = -1;
        private SoundSetting soundSetting = new SoundSetting();

        public SoundSetting SoundSetting
        {
            get => soundSetting;
        }
        private SystemSetting systemSetting = new SystemSetting();
        public SystemSetting SystemSetting
        {
            get => systemSetting;
        }
        private GraphicsSetting graphicsSetting = new GraphicsSetting();
        public GraphicsSetting GraphicsSetting
        {
            get => graphicsSetting;
        }
        
        public void SetGameSceneIndex(int sceneIndex)
        {
            m_GameSceneIndex = sceneIndex;
        }
        
        public void FreshResolution()
        {
            double radio = Math.Sqrt(GameManager.Instance.graphicsSetting.GetResolutionRadio());
            int facWidth = (int)(m_GameSceneWidth * radio);
            int facHeight = (int)(facWidth * m_GameSceneHeight / m_GameSceneWidth);
            GameConfig.SetFacScreen(m_GameSceneWidth, facHeight);
            Screen.SetResolution(facWidth, facHeight, false);
        }
        

        public void SetResolutionFit()
        {
            m_GameSceneWidth = Screen.width;
            m_GameSceneHeight = Screen.height;
            float scRatio = (float)m_GameSceneHeight / m_GameSceneWidth;

            int d_width = GameConfig.GameInitWidth;
            int d_height = GameConfig.GameInitHeight;
            float d_ratio = (float)d_height / d_width;

            var Root = GameObject.Find("Root");
            if (scRatio >= d_ratio)
                Root.GetComponent<CanvasScaler>().matchWidthOrHeight = 0; //顶宽
            else
                Root.GetComponent<CanvasScaler>().matchWidthOrHeight = 1; //顶高

            double radio = Math.Sqrt(graphicsSetting.ResolutionList[Instance.graphicsSetting.Resolution]);
            int facWidth = (int)(m_GameSceneWidth * radio);
            int facHeight = (int)(facWidth * m_GameSceneHeight / m_GameSceneWidth);
            Debug.Log("Resolution radio = " + radio + ", facWidth = " + facWidth + ", facHeight = " + facHeight);
            Debug.Log("Resolution m_GameSceneWidth =" + (m_GameSceneWidth * radio));
            Debug.Log("Resolution m_GameSceneHeight =" + (m_GameSceneHeight * radio));
            Screen.SetResolution(facWidth, facHeight, false);

            // 显示的安全区域
            float yMax = Screen.safeArea.yMax;
            float yMin = Screen.safeArea.yMin;
            if (Screen.safeArea.width == d_width)
            {
                GameConfig.GameFullScrTopGap = d_width * scRatio - yMax;
                GameConfig.GameFullScrBottomGap = yMin;
            }
            else if (Screen.safeArea.width == m_GameSceneWidth)
            {
                GameConfig.GameFullScrTopGap = (m_GameSceneHeight - yMax) * d_width / m_GameSceneWidth;
                GameConfig.GameFullScrBottomGap = yMin * d_width / m_GameSceneWidth;
            }

#if !UNITY_EDITOR
        Debug.Log("*****GameManager SetResolutionFit****");
        Debug.Log("m_GameSceneWidth=" + m_GameSceneWidth);
        Debug.Log("m_GameSceneHeight=" + m_GameSceneHeight);
        Debug.Log("scRatio=" + scRatio);
        Debug.Log("yMax=" + yMax);
        Debug.Log("yMin=" + yMin);
        Debug.Log("CommonDefine.GameFullScrTopGap = "+ GameConfig.GameFullScrTopGap);
#endif
        }
    }
    
    
    /// <summary>
    /// 音效配置
    /// </summary>
    public class SoundSetting
    {
        /// <summary>
        /// 设置音乐音量
        /// </summary>
        public float MusicVolume
        {
            get { return PlayerPrefs.GetFloat("MusicVolume", 1f); }
            set
            {
                PlayerPrefs.SetFloat("MusicVolume", value);
                AudioManager.Instance.SetBGMVolume(value);
            }
        }

        /// <summary>
        /// 设置音效音量
        /// </summary>
        public float SoundVolume
        {
            get => PlayerPrefs.GetFloat("SoundVolume", 1f);
            set
            {
                PlayerPrefs.SetFloat("SoundVolume", value);
                AudioManager.Instance.SetSoundVolume(value);
            }
        }

        /// <summary>
        /// 设置音效静音
        /// </summary>
        public bool SoundMute
        {
            get => PlayerPrefs.GetInt("SoundMute", 0) == 0 ? false : true;
            set => PlayerPrefs.SetInt("SoundMute", value ? 1 : 0);
        }

        /// <summary>
        /// 设置音乐静音
        /// </summary>
        public bool MusicMute
        {
            get => PlayerPrefs.GetInt("MusicMute", 0) == 0 ? false : true;
            set => PlayerPrefs.SetInt("MusicMute", value ? 1 : 0);
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Restore()
        {
            MusicVolume = 0.5f;
            SoundVolume = 0.5f;
            SoundMute = false;
            MusicMute = false;
        }
    }

    /// <summary>
    /// 图形配置
    /// </summary>
    public class GraphicsSetting
    {
        /// <summary>
        /// 设置画质
        /// </summary>
        public enum EGraphicsQuality
        {
            low,
            high,
            veryhigh
        }

        /// <summary>
        /// 品质等级
        /// </summary>
        public int QualityLevel
        {
            get
            {
                var level = QualitySettings.GetQualityLevel(); //从出包的包体里面拿Quality配置
                var quality = PlayerPrefs.GetInt("GraphicsQuality", level); //从持久化配置数据里拿Quality配置
                return quality;
            }

            set
            {
                QualitySettings.SetQualityLevel(value, true);
                ShadowQuality = value;
                SetResolution(Resolution);
                PlayerPrefs.SetInt("GraphicsQuality", value);
            }
        }

        /// <summary>
        /// 抗锯齿
        /// </summary>
        public int AntiAliasing
        {
            get
            {
                var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt((int)QualityLevel) as UniversalRenderPipelineAsset;
                return PlayerPrefs.GetInt("AntiAliasing", pipelineAsset.msaaSampleCount);
            }
            set
            {
                var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt((int)QualityLevel) as UniversalRenderPipelineAsset;
                pipelineAsset.msaaSampleCount = value;
                PlayerPrefs.SetInt("AntiAliasing", value);
            }
        }

        /// <summary>
        /// 阴影质量
        /// </summary>
        public int ShadowQuality
        {
            get { return PlayerPrefs.GetInt("ShadowQuality", 1); }

            set
            {
                var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(value) as UniversalRenderPipelineAsset;
                if (pipelineAsset == null)
                    return;
                if (value == 0) // 低：关闭全部阴影
                {
                    QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
                    //pipelineAsset.shadowDistance = 0;
                }
                else if (value == 1) //高
                {
                    if (GameManager.Instance.m_GameSceneIndex == 6)
                    {
                        pipelineAsset.shadowDistance = 160;
                        pipelineAsset.shadowCascadeOption = ShadowCascadesOption.TwoCascades;
                        pipelineAsset.shadowDepthBias = 6.8f;
                        pipelineAsset.shadowNormalBias = 0;
                    }
                    else
                    {
                        pipelineAsset.shadowDistance = 60;
                        pipelineAsset.shadowCascadeOption = ShadowCascadesOption.NoCascades;
                        pipelineAsset.shadowDepthBias = 1.6f;
                        pipelineAsset.shadowNormalBias = 1;
                    }
                }
                else if (value == 2) //非常高
                {
                    if (GameManager.Instance.m_GameSceneIndex == 6)
                    {
                        pipelineAsset.shadowDistance = 160;
                        pipelineAsset.shadowCascadeOption = ShadowCascadesOption.TwoCascades;
                        pipelineAsset.shadowDepthBias = 5.61f;
                        pipelineAsset.shadowNormalBias = 0;
                    }
                    else
                    {
                        pipelineAsset.shadowDistance = 60;
                        pipelineAsset.shadowCascadeOption = ShadowCascadesOption.NoCascades;
                        pipelineAsset.shadowDepthBias = 1.6f;
                        pipelineAsset.shadowNormalBias = 1;
                    }
                }
                PlayerPrefs.SetInt("ShadowQuality", value);
            }
        }

        /// <summary>
        /// 设置高清特效
        /// </summary>
        public bool HDEffect 
        {
            get => PlayerPrefs.GetInt("HDEffect", 0) == 0 ? false : true; 
            set => PlayerPrefs.SetInt("HDEffect", value ? 1 : 0);
        }
        
        /// <summary>
        /// 重置
        /// </summary>
        public void RestoreShadow()
        {
            //var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt(0) as UniversalRenderPipelineAsset;
            var level = ShadowQuality;
            ShadowQuality = level; //重新根据目前Scene设置一遍
        }
        
        /// <summary>
        /// 设置分辨率
        /// </summary>
        public int Resolution
        {
            get => PlayerPrefs.GetInt("Resolution", 1);
            set
            {
                PlayerPrefs.SetInt("Resolution", value); //按照ResolutionList的配置刷新分辨率
                SetResolution(value);
            }
        }

        public float[] ResolutionList = { 0.5f, 0.6667f, 0.75f, 1f };
        
        public float GetResolutionRadio() 
        {
            return ResolutionList[Resolution];
        }
        
        public void SetRenderScale(float changeValue = 1)
        {
            var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt((int)QualityLevel) as UniversalRenderPipelineAsset;
            Debug.Log("SetRenderScale OLD:" + pipelineAsset.renderScale);
            pipelineAsset.renderScale = (float)Math.Sqrt(GetResolutionRadio()) * changeValue;
            Debug.Log("SetRenderScale: " + pipelineAsset.renderScale);
        }

        private void SetResolution(int index)
        {
#if !UNITY_EDITOR
            SetRenderScale();
            GameManager.Instance.FreshResolution();
#endif
            // var pipelineAsset = QualitySettings.GetRenderPipelineAssetAt((int)QualityLevel) as UniversalRenderPipelineAsset;
            // if (pipelineAsset != null)
            //     pipelineAsset.renderScale = (float)System.Math.Sqrt(ResolutionList[index]);
            // var radio = Math.Sqrt(GameManager.Instance.graphicsSetting.ResolutionList[GameManager.Instance.graphicsSetting.Resolution]);
            // var scHeight = GameManager.Instance.m_GameSceneHeight;
            // var scWidth = GameManager.Instance.m_GameSceneWidth;
            // var facWidth = (int)(GameManager.Instance.m_GameSceneWidth * radio);
            // var facHeight = (int)(facWidth * scHeight / scWidth);
            // Debug.Log("Resolution radio = " + radio + ", facWidth = " + facWidth + ", facHeight = " + facHeight);
            // Debug.Log("Resolution m_GameSceneWidth =" + (scWidth * radio));
            // Debug.Log("Resolution m_GameSceneHeight =" + (scHeight * radio));
            // Screen.SetResolution(facWidth, facHeight, false);
        }
    }
    
    /// <summary>
    /// 系统设置
    /// </summary>
    public class SystemSetting
    {
        /// <summary>
        /// 设置战斗震动
        /// </summary>
        public int BattleShake
        {
            get { return PlayerPrefs.GetInt("BattleShake", 1); }

            set { PlayerPrefs.SetInt("BattleShake", value); }
        }

        /// <summary>
        /// 设置FPS
        /// </summary>
        public int FPS
        {
            get { return PlayerPrefs.GetInt("FPS", GameConfig.BattleConfigFPS); }
            set
            {
                GameConfig.BattleConfigFPS = value;
                Application.targetFrameRate = GameConfig.BattleConfigFPS;
                PlayerPrefs.SetInt("FPS", value);
            }
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Restore()
        {
            
        }
    }
}