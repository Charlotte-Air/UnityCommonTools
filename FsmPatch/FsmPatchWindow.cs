using UnityEngine;
using UnityEngine.UI;
using Framework.Utils;

public class FsmPatchWindow : MonoBehaviour
{
    public static FsmPatchWindow Instance { get; private set; }= null;
    private Text m_Hotupdate_Des;
    private GameObject m_Node_Bg;
    private Image m_Hotupdate_Bar;
    private Text m_Hotupdate_Title;
    private Text m_Hotupdate_Version;
    private Text m_Hotupdate_BarNum;
    private Button m_Hotupdate_Confirm;
    private readonly EventGroup _eventGroup = new EventGroup();
    
    void Awake()
    {
        Instance = this;
        m_Node_Bg = transform.Find("Node_Bg").gameObject;
        m_Hotupdate_Des = transform.Find("Hotupdate_Des").GetComponent<Text>();
        m_Hotupdate_Bar = transform.Find("Hotupdate_Bar").GetComponent<Image>();
        m_Hotupdate_Title = transform.Find("Hotupdate_Title").GetComponent<Text>();
        m_Hotupdate_BarNum = transform.Find("Hotupdate_BarNum").GetComponent<Text>();
        m_Hotupdate_Version = transform.Find("Hotupdate_Version").GetComponent<Text>();
        m_Hotupdate_Confirm = transform.Find("Hotupdate_Confirm").GetComponent<Button>();
        RefreshVersion();
        ShowBox(false);
        
        _eventGroup.AddListener<FsmPatchEvent.InitializeFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.PatchStatesChange>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.FoundUpdateFiles>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.DownloadProgressUpdate>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.PackageVersionUpdateFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.PatchManifestUpdateFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.WebFileDownloadFailed>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.UserHutUpdateEnd>(OnHandleEventMessage);
        _eventGroup.AddListener<FsmPatchEvent.UserNoNetwork>(OnHandleEventMessage);
    }
    
    void OnDestroy()
    {
        _eventGroup.RemoveAllListener();
    }

    /// <summary>
    /// 接收事件
    /// </summary>
    private void OnHandleEventMessage(IEventMessage message)
    {
        if (message is FsmPatchEvent.InitializeFailed)
        {
            ShowMessageBox($"初始化包失败!", (() =>
            {
                FsmPatchEvent.UserTryInitialize.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.PatchStatesChange)
        {
            var msg = message as FsmPatchEvent.PatchStatesChange;
            m_Hotupdate_BarNum.text = msg.Tips;
        }
        else if (message is FsmPatchEvent.FoundUpdateFiles)
        {
            var msg = message as FsmPatchEvent.FoundUpdateFiles;
            float sizeMB = msg.TotalSizeBytes / 1048576f;
            sizeMB = Mathf.Clamp(sizeMB, 0.1f, float.MaxValue);
            string totalSizeMB = sizeMB.ToString("f1");
            ShowMessageBox($"需要更新文件总数:{msg.TotalCount} & 下载大小:{totalSizeMB}MB", (() =>
            {
                FsmPatchEvent.UserBeginDownloadWebFiles.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.DownloadProgressUpdate)
        {
            var msg = message as FsmPatchEvent.DownloadProgressUpdate;
            m_Hotupdate_Bar.fillAmount = Mathf.Clamp01((float)(msg.CurrentDownloadSizeBytes / 1048576f) / (msg.TotalDownloadSizeBytes / 1048576f));
            string currentSizeMB = (msg.CurrentDownloadSizeBytes / 1048576f).ToString("f1");
            string totalSizeMB = (msg.TotalDownloadSizeBytes / 1048576f).ToString("f1");
            m_Hotupdate_BarNum.text = $"{msg.CurrentDownloadCount}/{msg.TotalDownloadCount} {currentSizeMB}MB / {totalSizeMB}MB";
        }
        else if (message is FsmPatchEvent.PackageVersionUpdateFailed)
        {
            ShowMessageBox("获取最新版本失败，请检查网络状态！", (() =>
            {
                FsmPatchEvent.UserTryUpdatePackageVersion.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.PatchManifestUpdateFailed)
        {
            ShowMessageBox($"下载最新资源失败，请检查网络状态！", (() =>
            {
                FsmPatchEvent.UserTryUpdatePatchManifest.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.WebFileDownloadFailed)
        {
            var msg = message as FsmPatchEvent.WebFileDownloadFailed;
            ShowMessageBox($"下载文件失败:{msg.FileName}", (() =>
            {
                FsmPatchEvent.UserTryDownloadWebFiles.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.UserNoNetwork)
        {
            ShowMessageBox($"请检查网络状态！", (() =>
            {
                FsmPatchEvent.UserTryUpdatePatchManifest.SendEventMessage();
            }));
        }
        else if (message is FsmPatchEvent.UserHutUpdateEnd)
        {
            _eventGroup.RemoveAllListener();
            Destroy(gameObject);
        }
    }
    
    public void ShowMessageBox(string content, System.Action ok)
    {
        m_Hotupdate_Des.text = content;
        m_Hotupdate_Confirm.onClick.RemoveAllListeners();
        m_Hotupdate_Confirm.onClick.AddListener((() =>
        {
            ShowBox(false);
            ok?.Invoke();
        }));
        ShowBox(true);
    }

    private void ShowBox(bool isShow)
    {
        m_Node_Bg.gameObject.SetActive(isShow);
        m_Hotupdate_Des.gameObject.SetActive(isShow);
        m_Hotupdate_Title.gameObject.SetActive(isShow);
        m_Hotupdate_Confirm.gameObject.SetActive(isShow);
    }

    public void RefreshVersion()
    {
        m_Hotupdate_Version.text = GameConfig.GameHotUpdateVersion;
    }
}