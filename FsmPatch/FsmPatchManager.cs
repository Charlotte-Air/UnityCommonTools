using YooAsset;
using UnityEngine;
using Framework.Utils;
using Cysharp.Threading.Tasks;

public class FsmPatchManager : SingletonInstance<FsmPatchManager>, ISingleton
{
	/// <summary>
	/// 运行模式
	/// </summary>
	public EPlayMode PlayMode { set; get; }

	/// <summary>
	/// 包裹的版本信息
	/// </summary>
	public string PackageVersion { set; get; }
	
	/// <summary>
	/// 热更最新版本
	/// </summary>
	public string HotUpdateVersion { set; get; }

	/// <summary>
	/// 下载器
	/// </summary>
	public ResourceDownloaderOperation Downloader { set; get; }
	
	private bool _isRun = false;
	private StateMachine _machine;
	private EventGroup _eventGroup = new EventGroup();
	
	void ISingleton.OnCreate(object createParam)
	{
		PlayMode = (EPlayMode)createParam;
	}
	
	void ISingleton.OnDestroy()
	{
		_eventGroup.RemoveAllListener();
	}
	
	void ISingleton.OnUpdate()
	{
		_machine?.Update();
	}

	/// <summary>
	/// 开启流程
	/// </summary>
	public void Run()
	{
		if (_isRun != false) 
			return;
		_isRun = true;
		HotUpdateVersion = GameConfig.GameHotUpdateVersion;
		Debug.Log("开启补丁更新流程...");
		_eventGroup.AddListener<FsmPatchEvent.UserTryInitialize>(OnHandleEventMessage);
		_eventGroup.AddListener<FsmPatchEvent.UserBeginDownloadWebFiles>(OnHandleEventMessage);
		_eventGroup.AddListener<FsmPatchEvent.UserTryUpdatePackageVersion>(OnHandleEventMessage);
		_eventGroup.AddListener<FsmPatchEvent.UserTryUpdatePatchManifest>(OnHandleEventMessage);
		_eventGroup.AddListener<FsmPatchEvent.UserTryDownloadWebFiles>(OnHandleEventMessage);
		_eventGroup.AddListener<FsmPatchEvent.UserTryCheckNetwork>(OnHandleEventMessage);
			
		_machine = new StateMachine(this);
		_machine.AddNode<FsmCheckNetwork>();
		_machine.AddNode<FsmCheckVersion>();
		_machine.AddNode<FsmInitializePackage>();
		_machine.AddNode<FsmUpdatePackageVersion>();
		_machine.AddNode<FsmUpdatePackageManifest>();
		_machine.AddNode<FsmCreatePackageDownloader>();
		_machine.AddNode<FsmDownloadPackageFiles>();
		_machine.AddNode<FsmLoadHotUpdateDll>();
		_machine.AddNode<FsmUpdaterDone>();
		_machine.Run<FsmCheckNetwork>();
	}

	/// <summary>
	/// 接收事件
	/// </summary>
	private void OnHandleEventMessage(IEventMessage message)
	{
		switch (message)
		{
			case FsmPatchEvent.UserTryInitialize:
				_machine.ChangeState<FsmInitializePackage>();		//初始化资源包
				break;
			case FsmPatchEvent.UserBeginDownloadWebFiles:
				_machine.ChangeState<FsmDownloadPackageFiles>();	//下载更新文件
				break;
			case FsmPatchEvent.UserTryUpdatePackageVersion:
				_machine.ChangeState<FsmUpdatePackageVersion>();	//更新资源版本号
				break;
			case FsmPatchEvent.UserTryUpdatePatchManifest:
				_machine.ChangeState<FsmUpdatePackageManifest>();	// 更新资源清单
				break;
			case FsmPatchEvent.UserTryDownloadWebFiles:
				_machine.ChangeState<FsmCreatePackageDownloader>(); //创建文件下载器
				break;
			case FsmPatchEvent.UserTryCheckNetwork:
				_machine.ChangeState<FsmCheckNetwork>();			// 重新检测网络
				break;
		}
	}
}