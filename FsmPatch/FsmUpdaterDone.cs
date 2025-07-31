using YooAsset;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 流程更新完毕
/// </summary>
internal class FsmUpdaterDone : IStateNode
{
    private StateMachine _machine;
    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        JsonManager.PreloadInitSceneJsonFiles((progressJ, isLoadjson) =>
        {
            // UpdateProgress(progressJ * 0.1f);// json文件加载给10%的进度
            if (isLoadjson)
            {
                LoadHotLuncher().ToCoroutine();
            }
        });
    }

    async UniTask LoadHotLuncher()
    {
        var loadHandle = YooAssets.LoadAssetAsync<GameObject>($"{GameConfig.PackCommonPrefabsPath}/HotUpdateLuncher.prefab");
        await loadHandle.ToUniTask();
        if(loadHandle.Status == EOperationStatus.Succeed)
        {
            var instantiateHandle = loadHandle.InstantiateAsync();
            await instantiateHandle.ToUniTask();
        }
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }
}