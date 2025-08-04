using System;
using UnityEngine;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HotUpdateScripts.Manager;
using System.Collections.Generic;
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行

/// <summary>
/// 窗口管理器
/// </summary>
public class WindowManager
{
    private static WindowManager _instance;
    public static WindowManager Instance
    {
        get
        {
            if (_instance == null) 
            {
                _instance = new WindowManager();
            }
            return _instance;
        }
    }
    
    private GameObject m_MaskWnd = null;
    private GameObject m_UiLayer = null;
    private GameObject m_BlockWnd = null;
    private GameObject m_WndLayer = null;
    private GameObject m_ScrollLayer = null;
    private NormalRoot m_NormalRootWnd = null;

    private delegate void PushWndCb();
    private delegate void ShowWndCb();
    private List<UIBaseWindow> m_VecWndStack = new List<UIBaseWindow>();
    private Dictionary<WindowType, object> m_RestoreWnd = new Dictionary<WindowType, object>();
    private Dictionary<WindowType, UIBaseWindow> m_DialogToWnd = new Dictionary<WindowType, UIBaseWindow>();
    private List<KeyValuePair<WindowType, object>> m_RestoreWndList = new List<KeyValuePair<WindowType, object>>();
    private Dictionary<WindowType, UIBaseWindow> m_DialogToWndInserting = new Dictionary<WindowType, UIBaseWindow>();
    
    
    public async UniTask Init()
    {
        m_WndLayer = GameObject.FindWithTag("WndLayer").gameObject;
        m_UiLayer = GameObject.FindWithTag("UILayer").gameObject;
        m_ScrollLayer = GameObject.FindWithTag("ScrollLayer").gameObject;
        m_MaskWnd = m_WndLayer.transform.Find("panel_maskwnd").gameObject; // 作为弹出窗口背后的蒙版
        m_BlockWnd = m_WndLayer.transform.Find("panel_blockwnd").gameObject; // 作为整个屏幕的点击事件的阻挡
        SetMaskLayerVisible(false);
        SetBlockLayerVisible(false);
        CreateNormalRoot();
    }
    
    
    #region 私有方法
    /// <summary>
    /// 创建货币栏
    /// </summary>
    private void CreateNormalRoot()
    {
        m_NormalRootWnd = new NormalRoot();
        m_NormalRootWnd.SetOnLoadedCallback(() =>
        {
            m_NormalRootWnd.SetWindowType(WindowType.NormalRoot);
            m_NormalRootWnd.GetTransform().SetParent(m_WndLayer.transform, false);
            m_NormalRootWnd.SetActive(false);
        });
    }
    
    /// <summary>
    /// 创建Window
    /// </summary>
    /// <param name="poWnd"></param>
    /// <param name="param"></param>
    /// <param name="isRestore"></param>
    private void InsertWindow(UIBaseWindow poWnd, object param, bool isRestore)
    {
        if (poWnd.IsEnabled)
        {
            poWnd.HideWindow(false);
            PopToWindow(poWnd);  //窗口已经打开，在队列中
        }
        else
        {
            PushWindow(poWnd, param, isRestore);
            poWnd.IsEnabled = true;
        }

        if (isRestore)
        {
            if (m_RestoreWndList.Count > 0)
                m_RestoreWndList.RemoveAt(0);
            
            RestoreWindow();
        }

        m_DialogToWndInserting.Remove(poWnd.GetWindowType());
    }

    
    /// <summary>
    /// Window入栈
    /// </summary>
    /// <param name="poWnd"></param>
    /// <param name="param"></param>
    /// <param name="isRestore"></param>
    private void PushWindow(UIBaseWindow poWnd, object param, bool isRestore)
    {
        m_VecWndStack.Add(poWnd);
        PushWndCb cb = () =>
        {
            switch (poWnd.GetWndShowType())
            {
                case WindowShowType.Popup_Wnd:
                {
                    SetMaskLayerVisible(true, poWnd.GetCloseTipsStatus());
                    poWnd.AddToWndLayer();
                    poWnd.GetTransform().SetAsLastSibling();
                    break;
                }
                case WindowShowType.Fullscreen_Wnd:
                {
                    poWnd.AddToWndLayer();
                    poWnd.GetTransform().SetAsLastSibling();
                    m_NormalRootWnd.AttachWnd = poWnd.GetWindowType();
                    SetRootLayerVisible(true, !isRestore);
                    SetMaskLayerVisible(false);
                    if (!WindowShowTypeExist(WindowShowType.Scroll_Wnd))
                    {
                        if (m_VecWndStack.Count > 1)
                        {
                            m_VecWndStack[m_VecWndStack.Count - 2].HideWindow(true);
                        }

                        var poCurScene = SceneManager.Instance.GetCurrentScene();
                        if (poCurScene.m_mainWindow != null)
                        {
                            poCurScene.m_mainWindow.HideWindow(true);
                        }
                    }

                    break;
                }
                case WindowShowType.Scroll_Wnd:
                    SetMaskLayerVisible(false);
                    poWnd.AddToWndLayer();
                    poWnd.GetTransform().SetAsLastSibling();
                    break;
                default:
                    break;
            }

            poWnd.HideWindow(false);
            poWnd.OnEnter(true);
        };

        var length = m_VecWndStack.Count;
        if (length > 0)
        {
            var backWnd = m_VecWndStack[length - 1];
            backWnd.OnExit(false);
            cb();
        }
        else if (!isRestore && poWnd.GetWndShowType() != WindowShowType.Popup_Wnd)
        {
            // 如果是全屏窗口的话需要播放一下mainWnd的退出动画
            var poCurScene = SceneManager.Instance.GetCurrentScene();
            if (poCurScene.m_mainWindow != null)
            {
                poCurScene.m_mainWindow.RegisterExitCallback(() => { cb(); });
                poCurScene.m_mainWindow.PlayExitAni(0.8f);
            }
        }
        else
        {
            cb();
        }

        // 用于窗口路径恢复
        if (SceneManager.Instance.GetCurrentSceneState() == (int) SceneStateDefines.NORMALSCENE_STATE)
        {
            if (m_RestoreWnd.ContainsKey(poWnd.GetWindowType()))
            {
                m_RestoreWnd[poWnd.GetWindowType()] = param;
            }
            else
            {
                m_RestoreWnd.Add(poWnd.GetWindowType(), param);
            }
        }
    }

    
    /// <summary>
    /// 弹出顶部Window
    /// </summary>
    /// <param name="poWnd"></param>
    private void PopToWindow(UIBaseWindow poWnd)
    {
        while (m_VecWndStack.Count > 0)
        {
            var length = m_VecWndStack.Count;
            if (m_VecWndStack[length - 1] == poWnd)
            {
                break;
            }

            var tmpWnd = m_VecWndStack[length - 1];
            tmpWnd.OnExit(true);
            
            RemoveWindow(tmpWnd);

            if (SceneManager.Instance.GetCurrentSceneState() == (int) SceneStateDefines.NORMALSCENE_STATE)
            {
                m_RestoreWnd.Remove(tmpWnd.GetWindowType());
            }
        }

        var popBaseWindow = m_VecWndStack[m_VecWndStack.Count - 1];
        popBaseWindow.OnEnter(false);
        if (popBaseWindow.GetWndShowType() == WindowShowType.Popup_Wnd)
        {
            SetMaskLayerVisible(true, popBaseWindow.GetCloseTipsStatus());
            popBaseWindow.GetTransform().SetAsLastSibling();
        }
        else if (popBaseWindow.GetWndShowType() == WindowShowType.Scroll_Wnd)
        {
            SetMaskLayerVisible(false);
            popBaseWindow.GetTransform().SetAsLastSibling();
        }
        else if (popBaseWindow.GetWndShowType() != WindowShowType.Fullscreen_Wnd)
        {
            m_NormalRootWnd.AttachWnd = poWnd.GetWindowType();
            SetRootLayerVisible(true, false);
        }

        SetBlockLayerVisible(false);
    }

    
    /// <summary>
    /// 销毁Window
    /// </summary>
    /// <param name="poWnd"></param>
    private void RealPopWnd(UIBaseWindow poWnd)
    {
        var type = poWnd.GetWndShowType();
        RemoveWindow(poWnd); //移除Window
        PopWndFinish(type); //移除Window以后需要处理的逻辑
        SetBlockLayerVisible(false); //隐藏底部蒙板
    }

    
    private void PopWndFinish(WindowShowType showType)
    {
        var length = m_VecWndStack.Count;
        if (length > 0)
        {
            if (m_VecWndStack[length - 1].GetWndShowType() == WindowShowType.Popup_Wnd)
            {
                var hasFullWnd = false;
                for (var i = length - 1; i >= 0; i--)
                {
                    if (m_VecWndStack[i].GetWndShowType() == WindowShowType.Popup_Wnd)
                        continue;
                    
                    hasFullWnd = true;
                    m_VecWndStack[i].HideWindow(false);
                    m_NormalRootWnd.AttachWnd = m_VecWndStack[i].GetWindowType();
                    SetRootLayerVisible(true, false);
                    break;
                }

                if (!hasFullWnd)
                {
                    var _poCurScene = SceneManager.Instance.GetCurrentScene();
                    if (_poCurScene.m_mainWindow != null)
                    {
                        _poCurScene.m_mainWindow.HideWindow(false);
                    }
                }

                SetMaskLayerVisible(true, m_VecWndStack[length - 1].GetCloseTipsStatus());
                m_VecWndStack[length - 1].GetTransform().SetAsLastSibling();
            }
            else if (m_VecWndStack[length - 1].GetWndShowType() != WindowShowType.Scroll_Wnd)
            {
                m_NormalRootWnd.AttachWnd = m_VecWndStack[length - 1].GetWindowType();
                SetRootLayerVisible(true, false);
            }

            m_VecWndStack[length - 1].HideWindow(false);
            m_VecWndStack[length - 1].OnEnter(false);
        }
        else
        {
            // 最后一个窗口pop的时候调一下mainWindow的OnEnter
            var poCurScene = SceneManager.Instance.GetCurrentScene();
            if (poCurScene.m_mainWindow != null)
            {
                if (showType != WindowShowType.Popup_Wnd)
                {
                    poCurScene.m_mainWindow.PlayEnterAni(true);
                }
                else
                {
                    poCurScene.m_mainWindow.PlayEnterAni(false);
                    // // 主城的新手引导，还有一处在 UIBaseWindow 的 EnterAniCallback 里
                    // GuideManager.getInstance().CheckWindowGuide((UINT16)_poCurScene.m_mainWindow.GetWindowType(), false, _poCurScene.m_mainWindow.m_gameObject);
                }

                poCurScene.m_mainWindow.HideWindow(false);
                poCurScene.m_mainWindow.OnEnter(false);
            }
        }
    }

    
    /// <summary>
    /// 是否显示Window底部蒙板
    /// </summary>
    /// <param name="bVisible"></param>
    /// <param name="closeTipsVisible"></param>
    private void SetMaskLayerVisible(bool bVisible, bool closeTipsVisible = false)
    {
        if (bVisible)
        {
            m_MaskWnd.SetActive(true);
            m_MaskWnd.transform.SetAsLastSibling();
            ShowMaskWndCloseTips(closeTipsVisible);
        }
        else
        {
            m_MaskWnd.SetActive(false);
        }
    }

    
    /// <summary>
    /// 是否显示Window底部蒙板Tips
    /// </summary>
    /// <param name="bVisible"></param>
    private void ShowMaskWndCloseTips(bool bVisible)
    {
        var closeTips = m_MaskWnd.transform.Find("Text_close_tips").gameObject;
        closeTips.SetActive(bVisible);
    }

    /// <summary>
    /// 设置Window层级
    /// </summary>
    /// <param name="bVisible"></param>
    /// <param name="bPlayAni"></param>
    /// <param name="cb"></param>
    private void SetRootLayerVisible(bool bVisible, bool bPlayAni, Action cb = null)
    {
        if (bVisible)
        {
            m_NormalRootWnd.SetSelfActiveState(true);
            m_NormalRootWnd.RegisterEvent();
            m_NormalRootWnd.OnEnter(bPlayAni);
            m_NormalRootWnd.GetTransform().SetAsLastSibling();

            var dEnumerator = m_DialogToWnd.GetEnumerator();
            while (dEnumerator.MoveNext())
            {
                var it = dEnumerator.Current.Value;
                if (it.GetWindowType() == m_NormalRootWnd.AttachWnd)
                {
                    m_NormalRootWnd.SetTopTreasureNode(it.GetWndShowType());
                    m_NormalRootWnd.SetTopMoneyShowTypeStr(it.GetWndTopBarShowTypeList());
                    break;
                }
            }
        }
        else
        {
            m_NormalRootWnd.RegisterExitCallback(() =>
            {
                m_NormalRootWnd.AttachWnd = WindowType.DefaultWnd;
                m_NormalRootWnd.ResetState();
                cb?.Invoke();
            });
            m_NormalRootWnd.OnExit(bPlayAni);
            m_NormalRootWnd.SetSelfActiveState(false);
        }
    }

    /// <summary>
    /// 移除Window
    /// </summary>
    /// <param name="poWnd"></param>
    private void RemoveWindow(UIBaseWindow poWnd)
    {
        var wndType = poWnd.GetWindowType();
        var vecEnumerator = m_VecWndStack.GetEnumerator();
        while (vecEnumerator.MoveNext())
        {
            var item = vecEnumerator.Current;
            if (item.GetWindowType() == wndType)
            {
                m_VecWndStack.Remove(item);
                break;
            }
        }

        // 窗口退出就从路径恢复里面删除
        m_RestoreWnd.Remove(wndType);

        // 不需要每次销毁的窗口
        // if ()
        // {
        //     poWnd.IsEnabled = false;
        //     poWnd.HideWindow(true);
        //     return;
        // }

        poWnd.DestroyWindow();
        m_DialogToWnd.Remove(wndType);
    }
    #endregion

    
    #region 公有方法
    
    public GameObject GetWndParentLayer() => m_WndLayer;


    public GameObject GetMainWndParentLayer() => m_UiLayer;


    public GameObject GetScrollParentLayer() => m_ScrollLayer;
    
    
    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <param name="eType">窗口类型</param>
    /// <param name="param">打开窗口的参数</param>
    /// <param name="isRestore">是否是通过恢复路径打开</param>
    /// <returns></returns>
    public UIBaseWindow ShowWindow(WindowType eType, object param = null, bool isRestore = false)
    {
        var topWindow = GetTopWindow();
        if (topWindow != null && topWindow.GetWindowType() == eType)
            return topWindow;
        if (m_DialogToWndInserting.ContainsKey(eType))
            return null;

        UIBaseWindow poWnd = null;
        var enumerator = m_DialogToWnd.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Value.GetWindowType() == eType)
            {
                poWnd = enumerator.Current.Value;
            }
        }

        if (poWnd == null)
        {
            switch (eType)
            {
                case WindowType.E_DT_NORMAL_DIALOG_WND:
                    poWnd = new DialogWnd(param);
                    break;
                case WindowType.E_DT_NORMAL_DIALOG_WND2:
                    poWnd = new DialogWnd(param);
                    break;
                default:
                    poWnd = Assembly.GetExecutingAssembly().CreateInstance(eType.ToString(), true, BindingFlags.Default,
                        null, new[] {param}, null, null) as UIBaseWindow;
                    break;
            }
            
            if (poWnd == null)
                return null;

            m_DialogToWndInserting.Add(eType, poWnd);
            Action poMustCb = () =>
            {
                poWnd.SetWindowType(eType);
                poWnd.HideWindow(true);
                poWnd.RegisterEvent();
                
                m_DialogToWnd.Add(eType, poWnd);

                if (poWnd.GetIsPreLoadWnd())
                {
                    poWnd.RequestData(() =>
                    {
                        InsertWindow(poWnd, param, isRestore);
                    });
                }
                else
                {
                    InsertWindow(poWnd, param, isRestore);
                }

                m_DialogToWndInserting.Remove(eType);
            };
            
            //m_RestoreWnd.Remove(eType);
            poWnd.SetOnLoadedCallback(poMustCb);
        }
        else
        {
            poWnd.SetWindowType(eType);
            poWnd.HideWindow(true);
            poWnd.RegisterEvent();

            if (poWnd.GetIsPreLoadWnd())
            {
                poWnd.RequestData(() => { InsertWindow(poWnd, param, isRestore); });
                m_DialogToWndInserting.Remove(eType);
            }
            else
            {
                InsertWindow(poWnd, param, isRestore);
            }
        }
        return poWnd;
    }

    
    /// <summary>
    /// 移除顶部窗口
    /// </summary>
    public void PopTopWindow()
    {
        int length = m_VecWndStack.Count;
        if (length > 0)
        {
            UIBaseWindow poWnd = m_VecWndStack[length - 1];
            switch (poWnd.GetWndShowType())
            {
                case WindowShowType.Popup_Wnd:
                {
                    SetMaskLayerVisible(false);
                    RealPopWnd(poWnd);
                    break;
                }
                case WindowShowType.Fullscreen_Wnd:
                {
                    SetRootLayerVisible(false, true, () => { RealPopWnd(poWnd); });
                    break;
                }
                case WindowShowType.Scroll_Wnd:
                    RealPopWnd(poWnd);
                    break;
            }
        }
    }

    
    /// <summary>
    /// 传入当前UIBaseWindow后，将关闭当前窗口和当前窗口之上的全部窗口
    /// </summary>
    /// <param name="poWnd"></param>
    public void CloseWindowsFromThis(UIBaseWindow poWnd)
    {
        PopToWindow(poWnd);
        poWnd.CloseWindow();
    }

    
    /// <summary>
    /// 设置遮挡层的显隐
    /// </summary>
    /// <param name="bVisible"></param>
    public void SetBlockLayerVisible(bool bVisible)
    {
        if (bVisible)
        {
            m_BlockWnd.SetActive(true);
            m_BlockWnd.transform.SetAsLastSibling();
        }
        else
        {
            m_BlockWnd.SetActive(false);
        }
    }

    
    /// <summary>
    /// 获取NormalRootWnd
    /// </summary>
    /// <returns></returns>
    public NormalRoot GetNormalRootWnd()
    {
        return m_NormalRootWnd;
    }

    
    /// <summary>
    /// 获取NormalRootWnd依附的窗口类型
    /// </summary>
    /// <returns></returns>
    public WindowType GetNormalRootAttachWndType()
    {
        return m_NormalRootWnd.AttachWnd;
    }

    
    /// <summary>
    /// 获取顶部的窗口
    /// </summary>
    /// <returns></returns>
    public UIBaseWindow GetTopWindow()
    {
        var length = m_VecWndStack.Count;
        if (length > 0)
        {
            return m_VecWndStack[length - 1];
        }
        return null;
    }

    
    /// <summary>
    /// 根据窗口类型获取窗口
    /// </summary>
    /// <param name="eType"></param>
    /// <returns></returns>
    public UIBaseWindow GetWindowByType(WindowType eType)
    {
        for (var i = 0; i < m_VecWndStack.Count; i++)
        {
            if (m_VecWndStack[i].GetWindowType() == eType)
            {
                return m_VecWndStack[i];
            }
        }
        return null;
    }

    
    /// <summary>
    /// 用于回UIMain时还原路径
    /// </summary>
    /// <param name="isFirst"></param>
    /// <returns></returns>
    public bool RestoreWindow(bool isFirst = false)
    {
        if (isFirst)
        {
            m_RestoreWndList.Clear();
            var enumerator = m_RestoreWnd.GetEnumerator();
            while (enumerator.MoveNext())
            {
                m_RestoreWndList.Add(enumerator.Current);
            }
        }

        if (m_RestoreWndList.Count > 0)
        {
            ShowWindow(m_RestoreWndList[0].Key, m_RestoreWndList[0].Value, true);
            return true;
        }

        return false;
    }

    
    /// <summary>
    /// 清除恢复路径
    /// </summary>
    public void ClearRestoreWnd()
    {
        m_RestoreWnd.Clear();
    }
    

    /// <summary>
    /// 根据窗口类型清除恢复路径中的窗口
    /// </summary>
    /// <param name="eType"></param>
    public void ClearRestoreWndByType(WindowType eType)
    {
        if (SceneManager.Instance.GetCurrentSceneState() == (int) SceneStateDefines.NORMALSCENE_STATE)
        {
            m_RestoreWnd.Remove(eType);
        }
    }

    
    /// <summary>
    /// 添加恢复路径
    /// </summary>
    /// <param name="eType">窗口类型</param>
    /// <param name="param">窗口参数</param>
    /// <param name="clearPre">是否清除之前的路径</param>
    public void AddRestoreWnd(WindowType eType, object param = null, bool clearPre = true)
    {
        if (clearPre)
        {
            m_RestoreWnd.Clear();
        }
        m_RestoreWnd.Add(eType, param);
    }

    
    /// <summary>
    /// 清除所有的窗口
    /// </summary>
    public void CleanUp()
    {
        while (m_VecWndStack.Count > 0) //清除WndLayer上受管理的窗口
        {
            int length = m_VecWndStack.Count;
            var tmpWnd = m_VecWndStack[length - 1];
            var eType = tmpWnd.GetWindowType();
            tmpWnd.IsEnabled = false;
            tmpWnd.OnExit(true);
            tmpWnd.DestroyWindow();
            
            m_VecWndStack.RemoveAt(length - 1);
            m_DialogToWnd.Remove(eType);
            m_DialogToWndInserting.Remove(eType);
        }

        m_VecWndStack.Clear();
        m_DialogToWnd.Clear();

        // 清除正在Insert但还未正式被管理的窗口
        var wndEnumerator = m_DialogToWndInserting.GetEnumerator();
        while (wndEnumerator.MoveNext())
        {
            wndEnumerator.Current.Value.DestroyWindow();
        }

        m_DialogToWndInserting.Clear();

        // 隐藏蒙版和normalroot
        SetMaskLayerVisible(false);
        m_NormalRootWnd.SetSelfActiveState(false);

        //清除UILayer上的窗口
        var poCurScene = SceneManager.Instance.GetCurrentScene();
        if (poCurScene.m_mainWindow != null)
        {
            poCurScene.m_mainWindow.OnExit(true);
        }

        for (var i = 0; i < m_UiLayer.transform.childCount; i++)
        {
            var childObj = m_UiLayer.transform.GetChild(i).gameObject;
            if (childObj != null)
            {
                GameObject.Destroy(childObj);
            }
        }
    }

    
    /// <summary>
    /// 移除所有窗口
    /// </summary>
    public void PopAllWindow()
    {
        var showType = WindowShowType.Fullscreen_Wnd;
        while (m_VecWndStack.Count > 0)
        {
            var length = m_VecWndStack.Count;
            var tmpWnd = m_VecWndStack[length - 1];
            tmpWnd.OnExit(true);
            RemoveWindow(tmpWnd);

            if (SceneManager.Instance.GetCurrentSceneState() == (int) SceneStateDefines.NORMALSCENE_STATE)
            {
                var type = tmpWnd.GetWindowType();
                showType = tmpWnd.GetWndShowType();
                m_RestoreWnd.Remove(type);
            }
        }

        SetMaskLayerVisible(false);
        SetRootLayerVisible(false, true);
        SetBlockLayerVisible(false);

        var poCurScene = SceneManager.Instance.GetCurrentScene();
        if (poCurScene.m_mainWindow != null)
        {
            if (showType != WindowShowType.Popup_Wnd)
            {
                poCurScene.m_mainWindow.PlayEnterAni(true);
            }
            else
            {
                poCurScene.m_mainWindow.PlayEnterAni(false);
            }

            poCurScene.m_mainWindow.HideWindow(false);
            poCurScene.m_mainWindow.OnEnter(false);
        }
    }

    
    /// <summary>
    /// 根据类型移除窗口
    /// </summary>
    /// <param name="eType"></param>
    public void PopWindowByType(WindowType eType)
    {
        for (var i = 0; i < m_VecWndStack.Count; i++)
        {
            var poWnd = m_VecWndStack[i];
            if (poWnd.GetWindowType() == eType)
            {
                if (poWnd.GetWndShowType() == WindowShowType.Popup_Wnd)
                {
                    SetMaskLayerVisible(false);
                    RealPopWnd(poWnd);
                }
                else
                {
                    SetRootLayerVisible(false, true, () =>
                    {
                        RealPopWnd(poWnd);
                    });
                }
            }
        }
    }

    
    /// <summary>
    /// 判断是否存在指定窗口类型的窗口
    /// </summary>
    /// <param name="windowType"></param>
    /// <returns></returns>
    public bool WindowExist(WindowType windowType)
    {
        return m_DialogToWnd.ContainsKey(windowType);
    }

    
    /// <summary>
    /// 判断是否存在指定窗口展示类型的窗口
    /// </summary>
    /// <param name="windowShowType"></param>
    /// <returns></returns>
    private bool WindowShowTypeExist(WindowShowType windowShowType)
    {
        for (var i = 0; i < m_VecWndStack.Count; i++)
        {
            if (m_VecWndStack[i].GetWndShowType() == windowShowType)
                return true;
        }
        return false;
    }
    
    #endregion
}