using YooAsset;
using Cysharp.Threading.Tasks;

/// <summary>
/// 下载更新文件
/// </summary>
public class FsmDownloadPackageFiles : IStateNode
{
    private StateMachine _machine;

    async UniTask IStateNode.OnCreate(StateMachine machine)
    {
        _machine = machine;
    }
    
    async UniTask IStateNode.OnEnter()
    {
        await BeginDownload();
    }
    
    async UniTask IStateNode.OnUpdate()
    {
        
    }
    
    async UniTask IStateNode.OnExit()
    {
        
    }

    async UniTask BeginDownload()
    {
        // 注册下载回调
        var downloader = FsmPatchManager.Instance.Downloader;
        downloader.OnDownloadErrorCallback = FsmPatchEvent.WebFileDownloadFailed.SendEventMessage;
        downloader.OnDownloadProgressCallback = FsmPatchEvent.DownloadProgressUpdate.SendEventMessage;
        downloader.BeginDownload();
        await downloader.ToUniTask();
        // 检测下载结果
        if (downloader.Status != EOperationStatus.Succeed)
            return;
        
        _machine.ChangeState<FsmLoadHotUpdateDll>();
    }
}