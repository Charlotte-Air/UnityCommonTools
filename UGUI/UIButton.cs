using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteInEditMode]
[AddComponentMenu("UI/UIButton", 11)]
public class UIButton : Button
{
    public ButtonClickedEvent onClickUp
    {
        get { return this.m_OnClickUp; }
        set { this.m_OnClickUp = value; }
    }

    public ButtonClickedEvent onClickDown
    {
        get { return this.m_OnClickDown; }
        set { this.m_OnClickDown = value; }
    }

    public ButtonClickedEvent onPress
    {
        get { return this.m_OnPress; }
        set { this.m_OnPress = value; }
    }

    public ButtonClickedEvent onDisableClick
    {
        get { return this.m_onDisableClick; }
        set { this.m_onDisableClick = value; }
    }

    public RectTransform rectTransform
    {
        get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
    }

    [System.NonSerialized]
    private ButtonClickedEvent m_OnClickUp = new ButtonClickedEvent();
    [System.NonSerialized]
    private ButtonClickedEvent m_OnClickDown = new ButtonClickedEvent();
    [System.NonSerialized]
    private ButtonClickedEvent m_OnPress = new ButtonClickedEvent();

    [SerializeField]
    private ButtonClickedEvent m_onDisableClick = new ButtonClickedEvent();
        
    public float longPressTime = 1f;
    public UnityEvent OnLongPress;

    [System.NonSerialized]
    private bool isPress = false;

    [System.NonSerialized] 
    private RectTransform m_RectTransform;
    [System.NonSerialized]
    private UIImage m_clickProxy;

    [SerializeField]
    private bool m_customClickRect = false;
    [SerializeField]
    private Rect m_clickRect = new Rect(0, 0, 1, 1);

    public bool isEnabled
    {
        get
        {
            return m_isEnabled;
        }
        set
        {
            if (m_isEnabled != value)
            {
                m_isEnabled = value;
                SetEnableStatus();
            }
        }
    }
    [SerializeField]
    private bool m_isEnabled = true;

    public bool isDisableClickEnabled
    {
        get
        {
            return m_isDisableClickEnabled;
        }
        set
        {
            if (m_isDisableClickEnabled != value)
            {
                m_isDisableClickEnabled = value;
            }
        }
    }
    [SerializeField]
    private bool m_isDisableClickEnabled = false;

    private void SetEnableStatus()
    {
        if (this.targetGraphic != null)
        {
            UIImage img = this.targetGraphic as UIImage;
            if (img != null)
            {
                img.isEnabled = m_isEnabled;
            }
        }
        if (m_isEnabled)
        {
            this.interactable = true;
        }
        else
        {
            if (!m_isDisableClickEnabled)
            {
                this.interactable = false;
            }
        }
    }

    protected override void Awake()
    {
        SetEnableStatus();
        ProcessClickProxy();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        onClick.RemoveAllListeners();
        onClickUp.RemoveAllListeners();
        onClickDown.RemoveAllListeners();
        onPress.RemoveAllListeners();
        onDisableClick.RemoveAllListeners();

        if (m_clickProxy != null)
        {
            GameObject.Destroy(m_clickProxy.gameObject);
            m_clickProxy = null;
        }
    }
    
    protected void ProcessClickProxy()
    {
        UIImage baseImg = gameObject.GetComponent<UIImage>();
        if (baseImg != null)
        {
            baseImg.raycastTarget = !m_customClickRect;
        } 

        if (!Application.isPlaying)
        {
            return;
        }

        if (m_clickProxy != null)
        {
            GameObject.Destroy(m_clickProxy.gameObject);
            m_clickProxy = null;
        }

        if (m_customClickRect)
        {
            GameObject go = new GameObject();
            go.name = "_click_proxy_";
            go.layer = 5; //GameLayer.Layer_UI;
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.SetParent(transform, false);
            go.transform.SetAsFirstSibling();
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchoredPosition = m_clickRect.min;
            rect.sizeDelta = new Vector2(m_clickRect.width, m_clickRect.height);
            rect.pivot = new Vector2(0.5f, 0.5f);
            m_clickProxy = go.AddComponent<UIImage>();
            m_clickProxy.noGraphics = true;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        SetEnableStatus();

        if (!IsActive())
            return;
        
        ProcessClickProxy();
    }

#endif
    public override void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        this.Up();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;
        this.Down();
    }
    
    private void Up()
    {
        if (!this.IsActive() || !this.IsInteractable())
            return;
        if (this.m_OnClickUp != null)
            this.m_OnClickUp.Invoke();

        isPress = false;
        DoStateTransition(SelectionState.Normal, true);
        CancelInvoke("Press");
    }

    float pressTime = 0;
    bool longPressSuccess = false;

    private void Down()
    {
        if (!this.IsActive() || !this.IsInteractable())
            return;
        if (this.m_OnClickDown != null)
            this.m_OnClickDown.Invoke();

        isPress = true;
        pressTime = 0;
        longPressSuccess = false;
        DoStateTransition(SelectionState.Pressed, true);
        InvokeRepeating("Press", 0, 0.1f);
    }

    private void Update()
    {
        if (isPress&& !longPressSuccess)
        {
            pressTime += Time.unscaledDeltaTime;
            if (pressTime >= longPressTime)
            {
                longPressSuccess = true;
                OnLongPress.Invoke();
            }

        }
    }

    private void Press()
    {
        if (!isPress)
            return;

        if (this.m_OnPress != null)
            this.m_OnPress.Invoke();
    }

    private void Click()
    {
        if (!IsActive() || !IsInteractable())
            return;

        if (longPressSuccess)
            return;

        if (m_isEnabled)
        {
            onClick.Invoke();
        }
        else
        {
            m_onDisableClick.Invoke();
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        Click();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        Click();

        if (!IsActive() || !IsInteractable())
            return;

        DoStateTransition(SelectionState.Pressed, false);
        StartCoroutine(OnFinishSubmit());
    }

    private IEnumerator OnFinishSubmit()
    {
        var fadeTime = colors.fadeDuration;
        var elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        DoStateTransition(currentSelectionState, false);
    }
}