using Framework.Manager;
using Cysharp.Threading.Tasks;

public class FsmLoadHotUpdateDll : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    async UniTask IStateNode.OnEnter()
    {
        GameConfig.GameHotUpdateVersion = FsmPatchManager.Instance.HotUpdateVersion;
        FsmPatchWindow.Instance.RefreshVersion();
        await ResourceManager.Instance.LoadMetadataForAOTAssemblies(GameConfig.GameHotUpdatePackageName);
        await ResourceManager.Instance.LoadHotUpdateAssemblies(GameConfig.GameHotUpdatePackageName);
        await YooAsset.YooAssets.GetPackage(GameConfig.GameHotUpdatePackageName).ClearUnusedCacheFilesAsync();
        _machine.ChangeState<FsmUpdaterDone>();
    }

    async UniTask IStateNode.OnExit()
    {
        
    }

    async UniTask IStateNode.OnUpdate()
    {
        
    }
}