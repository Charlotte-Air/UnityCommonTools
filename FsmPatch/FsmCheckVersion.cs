using System;
using YooAsset;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// 检查版本差异
/// </summary>
public class FsmCheckVersion : IStateNode
{
    private StateMachine _machine;

    public async UniTask OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public async UniTask OnEnter()
    {
        await CheckVersion();
    }

    async UniTask CheckVersion()
    {
        if (FsmPatchManager.Instance.PlayMode != EPlayMode.HostPlayMode)
        {
            _machine.ChangeState<FsmInitializePackage>();
            return;
        }
        
        var request = new UnityWebRequest($"{GameConfig.GameHttpHotUpdatePath}/VERSION.txt");
        var downloadHandlerBuffer = new DownloadHandlerBuffer();
        request.downloadHandler = downloadHandlerBuffer;
        var tokenSource = new CancellationTokenSource();
        tokenSource.CancelAfterSlim(TimeSpan.FromSeconds(3));
        try
        {
            Debug.Log("获取最新版本文件。。。");
            await request.SendWebRequest().WithCancellation(tokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken == tokenSource.Token)
            {
                FsmPatchWindow.Instance.ShowMessageBox("获取最新版本文件失败，请重新再试！", (() =>
                {
                    OnEnter();
                }));
            }
        }
        FsmPatchManager.Instance.HotUpdateVersion = request.downloadHandler.text;
        _machine.ChangeState<FsmInitializePackage>();
    }
    
    public async UniTask OnExit()
    {
        
    }

    public async UniTask OnUpdate()
    {
        
    }
}