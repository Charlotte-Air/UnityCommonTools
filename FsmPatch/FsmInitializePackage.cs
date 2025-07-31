using YooAsset;
using Cysharp.Threading.Tasks;

/// <summary>
/// 初始化资源包
/// </summary>
internal class FsmInitializePackage : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        FsmPatchEvent.PatchStatesChange.SendEventMessage("初始化资源包中。。。");
        await InitPackage();
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }

    private async UniTask InitPackage()
    {
        // 运行模式
        var playMode = FsmPatchManager.Instance.PlayMode;
        
        // 创建默认的资源包
        var packageName = GameConfig.GameHotUpdatePackageName;
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(packageName);
            YooAssets.SetDefaultPackage(package);
        }

        // 编辑器下的模拟模式
        InitializationOperation initializationOperation = null;
        if (playMode == EPlayMode.EditorSimulateMode)
        {
            var createParameters = new EditorSimulateModeParameters();
            createParameters.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(GameConfig.GameHotUpdateBuildPipeline, packageName);
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 单机运行模式
        if (playMode == EPlayMode.OfflinePlayMode)
        {
            var createParameters = new OfflinePlayModeParameters();
            createParameters.DecryptionServices = new FileStreamDecryption();
            initializationOperation = package.InitializeAsync(createParameters);
        }

        // 联机运行模式
        if (playMode == EPlayMode.HostPlayMode)
        {
            // 版本一致对比差异
            if (GameConfig.GameHotUpdateVersion == FsmPatchManager.Instance.HotUpdateVersion)
            {
                FsmPatchManager.Instance.PlayMode = EPlayMode.OfflinePlayMode;
                var createParameters = new OfflinePlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                initializationOperation = package.InitializeAsync(createParameters);
            }
            else
            {
                //下载最新资源包
                var hostUrl = GetHostServerURL();
                var createParameters = new HostPlayModeParameters();
                createParameters.DecryptionServices = new FileStreamDecryption();
                createParameters.BuildinQueryServices = new GameQueryServices();
                createParameters.RemoteServices = new RemoteServices(hostUrl, hostUrl);
                initializationOperation = package.InitializeAsync(createParameters);
            }
        }
        
        await initializationOperation.ToUniTask();
        if (package.InitializeStatus == EOperationStatus.Succeed)
        {
            _machine.ChangeState<FsmUpdatePackageVersion>();
        }
        else
        {
            // 版本一致对比差异且是差异模式 文件出现问题则重新下载最新资源包
            if (FsmPatchManager.Instance.PlayMode == EPlayMode.OfflinePlayMode && GameConfig.GameHotUpdateVersion == FsmPatchManager.Instance.HotUpdateVersion)
            {
                FsmPatchManager.Instance.PlayMode = EPlayMode.HostPlayMode;
                DownloadServicesPack();
            }
            else
            {
                FsmPatchEvent.InitializeFailed.SendEventMessage();
            }
        }
    }

    /// <summary>
    /// 从服务器下载最新资源包
    /// </summary>
    private async UniTask DownloadServicesPack()
    {
        var packageName = GameConfig.GameHotUpdatePackageName;
        var package = YooAssets.TryGetPackage(packageName);
        if (package == null)
        {
            package = YooAssets.CreatePackage(packageName);
            YooAssets.SetDefaultPackage(package);
        }

        var hostUrl = GetHostServerURL();
        var createParameters = new HostPlayModeParameters();
        createParameters.DecryptionServices = new FileStreamDecryption();
        createParameters.BuildinQueryServices = new GameQueryServices();
        createParameters.RemoteServices = new RemoteServices(hostUrl, hostUrl);
        var initializationOperation = package.InitializeAsync(createParameters);
        await initializationOperation.ToUniTask();
        if (package.InitializeStatus == EOperationStatus.Succeed)
        {
            _machine.ChangeState<FsmUpdatePackageVersion>();
        }
        else
        {
            FsmPatchEvent.InitializeFailed.SendEventMessage();
        }
    }
    
    /// <summary>
    /// 获取热更包地址
    /// </summary>
    public static string GetHostServerURL()
    {
#if UNITY_EDITOR
        if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android)
            return $"{GameConfig.GameHttpHotUpdatePath}/Android/{FsmPatchManager.Instance.HotUpdateVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.iOS)
            return $"{GameConfig.GameHttpHotUpdatePath}/IPhone/{FsmPatchManager.Instance.HotUpdateVersion}";
        else if (UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.WebGL)
            return $"{GameConfig.GameHttpHotUpdatePath}/WebGL/{FsmPatchManager.Instance.HotUpdateVersion}";
        else
            return $"{GameConfig.GameHttpHotUpdatePath}/PC/{FsmPatchManager.Instance.HotUpdateVersion}";
#else
        if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.Android)
            return $"{GameConfig.GameHttpHotUpdatePath}/Android/{FsmPatchManager.Instance.HotUpdateVersion}";
        else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.IPhonePlayer)
            return $"{GameConfig.GameHttpHotUpdatePath}/IPhone/{FsmPatchManager.Instance.HotUpdateVersion}";
        else if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WebGLPlayer)
            return $"{GameConfig.GameHttpHotUpdatePath}/WebGL/{FsmPatchManager.Instance.HotUpdateVersion}";
        else
            return $"{GameConfig.GameHttpHotUpdatePath}/PC/{FsmPatchManager.Instance.HotUpdateVersion}";
#endif
    }
}