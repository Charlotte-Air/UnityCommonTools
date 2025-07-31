using YooAsset;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 更新资源版本号
/// </summary>
internal class FsmUpdatePackageVersion : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        FsmPatchEvent.PatchStatesChange.SendEventMessage("获取最新的资源版本!");
        await GetStaticVersion();
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }

    async UniTask GetStaticVersion()
    {
        var package = YooAssets.GetPackage(GameConfig.GameHotUpdatePackageName);
        var operation = package.UpdatePackageVersionAsync();
        await operation.ToUniTask();
        
        if (operation.Status == EOperationStatus.Succeed)
        {
            FsmPatchManager.Instance.PackageVersion = operation.PackageVersion;
            _machine.ChangeState<FsmUpdatePackageManifest>();
        }
        else
        {
            Debug.LogWarning(operation.Error);
            FsmPatchEvent.PackageVersionUpdateFailed.SendEventMessage();
        }
    }
}