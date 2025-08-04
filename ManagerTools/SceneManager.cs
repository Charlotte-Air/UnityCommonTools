using System;
using YooAsset;
using UnityEngine;
using Framework.Utils;
using Framework.Manager;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace HotUpdateScripts.Manager
{
    /// <summary>
    /// 场景管理器
    /// </summary>
    public class SceneManager : SingletonInstance<SceneManager>, ISingleton
    {
        /// <summary>
        /// 当前场景索引
        /// </summary>
        private int m_nCurSceneState = -1;
        
        /// <summary>
        /// 即将跳转场景的索引
        /// </summary>
        private int m_nNextSceneState = -1;
        
        /// <summary>
        /// 当前场景
        /// </summary>
        private SceneBase m_poCurScene = null;
        
        private bool m_isFromLoginScene = false;
        
        /// <summary>
        /// 是否第一次启动
        /// </summary>
        private bool m_isFirstBoost = true;
        
        /// <summary>
        /// 预加载预制体列表
        /// </summary>
        private List<string> m_preloadPrefabList = new List<string>();
        
        /// <summary>
        /// 预加载场景
        /// </summary>
        private Dictionary<string, SceneHandle> m_dicAsyncScene = new Dictionary<string, SceneHandle>();
        
        
        void ISingleton.OnCreate(object createParam)
        {

        }
        
        void ISingleton.OnDestroy()
        {

        }
        
        void ISingleton.OnUpdate()
        {
            if (m_poCurScene == null)
                return;
            
            m_poCurScene.ScheduleUpdate();
            if (m_poCurScene.m_mainWindow != null && m_poCurScene.m_mainWindow.IsLoaded())
            {
                m_poCurScene.m_mainWindow.Update();
            }
            
            if (m_poCurScene.m_mainWindow != null && WindowManager.Instance.GetTopWindow() != null && m_poCurScene.m_mainWindow.IsLoaded())
            {
                WindowManager.Instance.GetTopWindow().Update();
            }
        }
        
        
        /// <summary>
        /// 前往场景
        /// </summary>
        /// <param name="nextState">下一个场景</param>
        /// <param name="callback">回调</param>
        /// <param name="sceneMode">下一个场景加载模式</param>
        public void GotoScene(int nextState, Action callback = null, LoadSceneMode sceneMode = LoadSceneMode.Single)
        {
            if (nextState == m_nCurSceneState)
                return;

            Debug.Log("当前场景:" + (SceneStateDefines) m_nCurSceneState + "=>" + "下个场景:" + (SceneStateDefines) nextState);
            if (m_nCurSceneState == (int) SceneStateDefines.LOGIN_STATE || m_nCurSceneState == (int) SceneStateDefines.CREATECHAR_STATE)
                m_isFromLoginScene = true;
            else
                m_isFromLoginScene = false;

            m_nNextSceneState = nextState;

            if (nextState == (int) SceneStateDefines.NORMALSCENE_STATE || nextState == (int) SceneStateDefines.BATTLESCENE_STATE)
            {
                nextState = (int) SceneStateDefines.LOADING_STATE;
            }

            DirectGotoScene(nextState, callback, sceneMode).ToCoroutine();
        }


        /// <summary>
        /// 处理前往场景
        /// </summary>
        /// <param name="nextSceneState">下一个场景加载模式</param>
        /// <param name="callback">回调</param>
        /// <param name="nextSceneMode">下一个场景加载模式</param>
        public async UniTask DirectGotoScene(int nextSceneState, Action callback = null, LoadSceneMode nextSceneMode = LoadSceneMode.Single)
        {
            var nextSceneName = GetSceneStateDefinesPackName(nextSceneState);
            if (nextSceneName == string.Empty)
            {
                Debug.LogError($"DirectGotoScene GetSceneStateDefinesPackName ID:{nextSceneState} Error!");
                return;
            }

            Action endCall = () =>
            {
                GameManager.Instance.SetGameSceneIndex(nextSceneState);
                //Resources.UnloadUnusedAssets(); // 场景切换的时候会自动调用一次，不需要在这里手动调用
                GC.Collect();
                
                if (m_poCurScene != null)
                {
                    m_poCurScene.ReleaseScene();
                    m_poCurScene = null;
                }
            
                switch (nextSceneState)
                {
                    case (int) SceneStateDefines.LOGIN_STATE:
                        m_poCurScene = new LoginScene();
                        break;
                    case (int) SceneStateDefines.CREATECHAR_STATE:
                        m_poCurScene = new CreateRoleScene();
                        break;
                    case (int) SceneStateDefines.LOADING_STATE:
                        m_poCurScene = new LoadingScene();
                        break;
                    case (int) SceneStateDefines.NORMALSCENE_STATE:
                        m_poCurScene = new MainScene();
                        break;
                }

                if (m_poCurScene == null)
                {
                    Debug.LogError($"Scene Load :{nextSceneName} InitScene SceneBase Null Error!");
                    return;
                }
            
                m_poCurScene.SceneLoadedCallback();
                m_nCurSceneState = nextSceneState; //此行代码必须在InitScene前调用!!
                m_poCurScene.InitScene();
                m_poCurScene.m_mainWindow.RegisterEvent();
                callback?.Invoke();
            };
            
            // 已经预加载过场景
            if (m_dicAsyncScene.ContainsKey(nextSceneName) && m_dicAsyncScene[nextSceneName].Progress >= 0.89f)
            {
                var handle = m_dicAsyncScene[nextSceneName];
                m_dicAsyncScene.Remove(nextSceneName);
                handle.UnSuspend();
                endCall();
            }
            else
            {
                Debug.Log($"LoadSceneAsync sceneName:{nextSceneName}");
                var package = YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName);
                if (package == null)
                {
                    Debug.LogError($"LoadSceneAsync Get PackageName:{GameConfig.GameHotUpdatePackageName} Error!");
                    return;
                }
                var loadHandle = package.LoadSceneAsync(nextSceneName, nextSceneMode);
                await loadHandle.ToUniTask();
                if (loadHandle.Status != EOperationStatus.Succeed)
                {
                    Debug.LogError($"ScenenManaer -> DirectGotoScene LoadSceneAsync Error!");
                    return;
                }
                Debug.LogError($"场景加载OK：{loadHandle.SceneName} State:{loadHandle.Status.ToString()}");
                loadHandle.ActivateScene();
                endCall();
            }
        }
        
        
        /// <summary>
        /// 预加载场景
        /// </summary>
        /// <param name="sceneState">场景</param>
        /// <param name="onFinishCallback">加载进度回调</param>
        public void PreloadScene(int sceneState, Action<float, bool> onFinishCallback)
        {
            PreloadScenePrefabs(sceneState, onFinishCallback, () =>
            {
                PreloadSceneValue(sceneState, onFinishCallback).ToCoroutine();
            }).ToCoroutine();
        }
        
        /// <summary>
        /// 加载场景前预加载资源,初始化
        /// </summary>
        /// <param name="sceneState">场景</param>
        /// <param name="onFinishCallback">加载进度回调</param>
        /// <param name="preloadSceneCb">场景加载完成回调</param>
        private async UniTask PreloadScenePrefabs(int sceneState, Action<float, bool> onFinishCallback, Action preloadSceneCb)
        {
            // 下一个场景需要预加载的部分预制体
            m_preloadPrefabList.Clear();

            switch (sceneState)
            {
                case (int) SceneStateDefines.LOGIN_STATE:
                    break;
                case (int) SceneStateDefines.CREATECHAR_STATE:
                    break;
                case (int) SceneStateDefines.LOADING_STATE:
                    break;
                case (int) SceneStateDefines.NORMALSCENE_STATE:
                    break;
                case (int) SceneStateDefines.BATTLESCENE_STATE:
                    break;
            }

            Debug.Log($"PreloadScenePrefabs SceneID:{sceneState} Count:{m_preloadPrefabList.Count}");
            if (m_preloadPrefabList.Count > 0)
            {
                var prefabLoadedCount = 0; // 加载完的预制体数量
                var resMgr = ResourceManager.Instance;
                for (var i = 0; i < m_preloadPrefabList.Count; i++)
                {
                    await resMgr.PreLoadUGUIPrefabAsync(m_preloadPrefabList[i], () =>
                    {
                        prefabLoadedCount++;
                        // 加载进度
                        onFinishCallback(0.5f * prefabLoadedCount / m_preloadPrefabList.Count, false);
                        if (prefabLoadedCount == m_preloadPrefabList.Count)
                        {
                            preloadSceneCb();
                        }
                    });
                }
            }
            else
            {
                onFinishCallback(0.5f, false);
                preloadSceneCb();
            }
        }

        private async UniTask PreloadSceneValue(int sceneState, Action<float, bool> onFinishCallback)
        {
            var sceneName = GetSceneStateDefinesPackName(sceneState);
            if (sceneName == string.Empty)
            {
                Debug.LogError($"SceneManager PreloadScene -> PreloadSceneValue ID:{sceneState} Error!");
                return;
            }
            var package = YooAssets.TryGetPackage(GameConfig.GameHotUpdatePackageName);
            if (package == null)
            {
                Debug.LogError($"SceneManager PreloadScene -> PreloadSceneValue Get PackageName:{GameConfig.GameHotUpdatePackageName} Error!");
                return;
            }
            var handle = m_dicAsyncScene.ContainsKey(sceneName) ? m_dicAsyncScene[sceneName] : package.LoadSceneAsync(sceneName, suspendLoad: true);
            TimerManager.Instance.AddTimerRepeat("SceneManager_PreloadSceneValue", 1, (args =>
            {
                if (handle.Progress >= 0.89f)
                {
                    TimerManager.Instance.Destroy("SceneManager_PreloadSceneValue");
                    if (!m_dicAsyncScene.ContainsKey(sceneName))
                    {
                        m_dicAsyncScene.Add(sceneName, handle);
                        onFinishCallback(1f, true);
                    }
                }
                else
                {
                    onFinishCallback(0.5f + handle.Progress * 0.5f, false);
                }
            }));
        }

        /// <summary>
        /// 是否是从登录场景跳转的
        /// </summary>
        /// <returns></returns>
        public bool IsFromLoginScene()
        {
            return m_isFromLoginScene;
        }

        /// <summary>
        /// 获取当前场景
        /// </summary>
        /// <returns></returns>
        public SceneBase GetCurrentScene()
        {
            return m_poCurScene;
        }

        /// <summary>
        /// 获取下一个场景
        /// </summary>
        /// <returns></returns>
        public int GetNextScene()
        {
            return m_nNextSceneState;
        }

        /// <summary>
        /// 获取当前场景的索引
        /// </summary>
        /// <returns></returns>
        public int GetCurrentSceneState()
        {
            return m_nCurSceneState;
        }

        /// <summary>
        /// 获取当前场景的主窗口
        /// </summary>
        /// <returns></returns>
        public UIBaseWindow GetCurrentSceneMainWindow()
        {
            return m_poCurScene.m_mainWindow;
        }

        
        public static string GetSceneStateDefinesPackName(int sceneStateDefines)
        {
            SceneStateDefines stateDefines = (SceneStateDefines)sceneStateDefines;
            switch (stateDefines)
            {
                case SceneStateDefines.HOTUPDATE_STATE:
                    return "HotUpdateScene";
                case SceneStateDefines.LOGIN_STATE:
                    return "loginscene";
                case SceneStateDefines.CREATECHAR_STATE:
                    return "createchar";
                case SceneStateDefines.LOADING_STATE:
                    return "loadingscene";
                case SceneStateDefines.NORMALSCENE_STATE:
                    return "normalscene";
                case SceneStateDefines.BATTLESCENE_STATE:
                    return "battlescene";
                case SceneStateDefines.HERODISPLAY_STATE:
                    return "herodisplayscene";
                default:
                    return string.Empty;
            }
        }
        
        public GameObject GetTargetSceneGameObject(int sceneState, string targetName)
        {
            var sceneName = GetSceneStateDefinesPackName(sceneState);
            if (sceneName == string.Empty)
            {
                Debug.LogError($"SceneManager PreloadScene -> PreloadSceneValue ID:{sceneState} Error!");
                return null;
            }
            if (!m_dicAsyncScene.ContainsKey(sceneName))
                return null;
            var scene = m_dicAsyncScene[sceneName].SceneObject;
            var sceneObjs = scene.GetRootGameObjects();
            for (var i = 0; i < sceneObjs.Length; i++)
            {
                var result = CommonFunctions.SeekTransformByName(sceneObjs[i].transform, targetName);
                if (result != null)
                    return result.gameObject;
            }
            return null;
        }
    }
}