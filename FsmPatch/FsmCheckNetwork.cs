using System;
using UnityEngine;
using System.Threading;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;

/// <summary>
/// 检测网络流程
/// </summary>
public class FsmCheckNetwork : IStateNode
{
    private StateMachine _machine;

    public async UniTask OnCreate(StateMachine machine)
    {
        _machine = machine;
    }

    public async UniTask OnEnter()
    {
        // 编译器模式
        if (FsmPatchManager.Instance.PlayMode == YooAsset.EPlayMode.EditorSimulateMode)
        {
            _machine.ChangeState<FsmCheckVersion>();
            return;
        }
        // 用户無网络
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            FsmPatchEvent.UserNoNetwork.SendEventMessage();
            return;
        }
        
        var request = new UnityWebRequest(GameConfig.GameHttpHotUpdatePath);
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfterSlim(TimeSpan.FromSeconds(3));
        try
        {
            Debug.Log($"UnityWebRequest -> {GameConfig.GameHttpHotUpdatePath}");
            await request.SendWebRequest().WithCancellation(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException ex)
        {
            if (ex.CancellationToken == cancellationTokenSource.Token)
            {
                request.Dispose();
                FsmPatchWindow.Instance.ShowMessageBox("连接超时，请重新再试！", (() =>
                {
                    OnEnter();
                }));
                return;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            FsmPatchWindow.Instance.ShowMessageBox(e.ToString(), (() =>
            {
                OnEnter();
            }));
            return;
        }
        
        if (request.result != UnityWebRequest.Result.Success)
        {
            request.Dispose();
            FsmPatchWindow.Instance.ShowMessageBox("无法连接服务器，请重新再试！", (() =>
            {
                OnEnter();
            }));
            return;
        }
        _machine.ChangeState<FsmCheckVersion>();
    }

    public async UniTask OnExit()
    {

    }

    public async UniTask OnUpdate()
    {

    }
}