using YooAsset;
using UnityEngine;
using Cysharp.Threading.Tasks;

/// <summary>
/// 创建文件下载器
/// </summary>
public class FsmCreatePackageDownloader : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        FsmPatchEvent.PatchStatesChange.SendEventMessage("开始下载最新资源。。。");
        await CreateDownloader();
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }

    async UniTask CreateDownloader()
    {
        var downloader = YooAssets.CreateResourceDownloader(GameConfig.GameHotUpdatDdownMaxNum, GameConfig.GameHotUpdatFailedTryAgain);
        FsmPatchManager.Instance.Downloader = downloader;
        
        if (downloader.TotalDownloadCount == 0)
        {
            _machine.ChangeState<FsmLoadHotUpdateDll>();
        }
        else
        {
            // TODO 发现新更新文件后挂起流程系统 需要在下载前检测磁盘空间不足!
            var totalDownloadCount = downloader.TotalDownloadCount;
            var totalDownloadBytes = downloader.TotalDownloadBytes;
            FsmPatchEvent.FoundUpdateFiles.SendEventMessage(totalDownloadCount, totalDownloadBytes);
        }
    }
}