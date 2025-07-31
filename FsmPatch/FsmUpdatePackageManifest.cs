using YooAsset;
using Cysharp.Threading.Tasks;

/// <summary>
/// 更新资源清单
/// </summary>
public class FsmUpdatePackageManifest : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        FsmPatchEvent.PatchStatesChange.SendEventMessage("更新资源清单中。。。");
        await UpdateManifest();
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }

    async UniTask UpdateManifest()
    {
        var package = YooAssets.GetPackage(GameConfig.GameHotUpdatePackageName);
        var operation = package.UpdatePackageManifestAsync(FsmPatchManager.Instance.PackageVersion);
        await operation.ToUniTask();

        if(operation.Status == EOperationStatus.Succeed)
        {
            _machine.ChangeState<FsmCreatePackageDownloader>();
        }
        else
        {
            FsmPatchEvent.PatchManifestUpdateFailed.SendEventMessage();
        }
    }
}