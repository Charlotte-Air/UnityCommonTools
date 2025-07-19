using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIToggle : Selectable, IPointerClickHandler, ISubmitHandler, ICanvasElement
{
    public enum ToggleTransition
    {
        None,
        Fade
    }

    [Serializable]
    public class ToggleEvent : UnityEvent<bool>
    { }

    /// <summary>
    /// Transition type.
    /// </summary>
    public ToggleTransition toggleTransition = ToggleTransition.Fade;

    /// <summary>
    /// Graphic the toggle should be working with.
    /// </summary>
    public Graphic[] graphic;
    public Graphic[] graphic2;

    //public bool graphicScale = false;
    //public float scaleVaule_on = 1;
    //public float scaleValue_off = 1;

    public bool UseCustomClickSound
    {
        get
        {
            return m_useCustomClickSound;
        }
        set
        {
            m_useCustomClickSound = value;
        }
    }

    public string CustomClickSound
    {
        get
        {
            return m_customClickSound;
        }
        set
        {
            m_customClickSound = value;
        }
    }

    public bool disable = false;

    [SerializeField]
    private bool m_useCustomClickSound = false;

    [SerializeField]
    private string m_customClickSound = string.Empty;

    // group that this toggle can belong to
    [SerializeField]
    private UIToggleGroup m_Group;

    public UIToggleGroup group
    {
        get { return m_Group; }
        set
        {
            m_Group = value;
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
            {
                SetToggleGroup(m_Group, true);
                PlayEffect(true);
            }
        }
    }

    public UnityEvent onDisableClick = new UnityEvent();
    /// <summary>
    /// Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    /// </summary>
    public ToggleEvent onValueChanged = new ToggleEvent();

    public delegate bool OnValueChangeHandler();
    public OnValueChangeHandler onValueChangeHandler;

    // Whether the toggle is on
    [FormerlySerializedAs("m_IsActive")]
    [Tooltip("Is the toggle currently on or off?")]
    [SerializeField]
    private bool m_IsOn;

    protected UIToggle()
    { }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        Set(m_IsOn, false);
        PlayEffect(toggleTransition == ToggleTransition.None);

        #if UNITY_5
        var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
        if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        #else
        var prefabType = UnityEditor.PrefabUtility.GetPrefabAssetType(this);
        if (prefabType != UnityEditor.PrefabAssetType.Model && !Application.isPlaying)
           CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this); 
        #endif
       
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
        if (executing == CanvasUpdate.Prelayout)
            onValueChanged.Invoke(m_IsOn);
#endif
    }

    public virtual void LayoutComplete()
    { }

    public virtual void GraphicUpdateComplete()
    { }

    protected override void OnEnable()
    {
        //if (!graphicScale)
        //{
        //    scaleVaule_on = 1;
        //    scaleValue_off = 1;
        //}
        base.OnEnable();
        SetToggleGroup(m_Group, false);
        PlayEffect(true);
    }

    protected override void OnDisable()
    {
        SetToggleGroup(null, false);
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        // Check if isOn has been changed by the animation.
        // Unfortunately there is no way to check if we don�t have a graphic.
        if (graphic.Length > 0)
        {
            bool oldValue = !Mathf.Approximately(graphic[0].canvasRenderer.GetColor().a, 0);
            if (m_IsOn != oldValue)
            {
                m_IsOn = oldValue;
                Set(!oldValue);
            }
        }

        base.OnDidApplyAnimationProperties();
    }

    private void SetToggleGroup(UIToggleGroup newGroup, bool setMemberValue)
    {
        UIToggleGroup oldGroup = m_Group;

        // Sometimes IsActive returns false in OnDisable so don't check for it.
        // Rather remove the toggle too often than too little.
        if (m_Group != null)
            m_Group.UnregisterToggle(this);

        // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
        // That's why we use the setMemberValue parameter.
        if (setMemberValue)
            m_Group = newGroup;

        // Only register to the new group if this Toggle is active.
        if (m_Group != null && IsActive())
            m_Group.RegisterToggle(this);

        // If we are in a new group, and this toggle is on, notify group.
        // Note: Don't refer to m_Group here as it's not guaranteed to have been set.
        if (newGroup != null && newGroup != oldGroup && isOn && IsActive())
            m_Group.NotifyToggleOn(this);
    }

    /// <summary>
    /// Whether the toggle is currently active.
    /// </summary>
    public bool isOn
    {
        get { return m_IsOn; }
        set
        {
            Set(value);
        }
    }

    void Set(bool value)
    {
        Set(value, true);
    }

    void Set(bool value, bool sendCallback)
    {
        if (value && onValueChangeHandler != null && onValueChangeHandler())
        {
            return;
        }

        if (m_IsOn == value)
            return;

        // if we are in a group and set to true, do group logic
        m_IsOn = value;
        if (m_Group != null && IsActive())
        {
            if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
            {
                m_IsOn = true;
                m_Group.NotifyToggleOn(this);
            }
        }

        // Always send event when toggle is clicked, even if value didn't change
        // due to already active toggle in a toggle group being clicked.
        // Controls like Dropdown rely on this.
        // It's up to the user to ignore a selection being set to the same value it already was, if desired.
        PlayEffect(toggleTransition == ToggleTransition.None);
        if (sendCallback)
            onValueChanged.Invoke(m_IsOn);
    }

    /// <summary>
    /// Play the appropriate effect.
    /// </summary>
    private void PlayEffect(bool instant)
    {
        if (graphic == null && graphic2 == null)// && graphicScale == false)
            return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (graphic != null)
            {
                for (int i = 0; i < graphic.Length; i++)
                {
                    if (graphic[i])
                    {
                        graphic[i].canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
                    }
                }
            }
            if (graphic2 != null)
            {
                for (int i = 0; i < graphic2.Length; i++)
                {
                    if (graphic2[i])
                    {
                        graphic2[i].canvasRenderer.SetAlpha(m_IsOn ? 0f : 1f);
                    }
                }
            }
            //if (graphicScale)
            //{
            //    if (m_IsOn) this.transform.localScale = new Vector3(scaleVaule_on, scaleVaule_on, scaleVaule_on);
            //    else this.transform.localScale = new Vector3(scaleValue_off, scaleValue_off, scaleValue_off);
            //}
        }
        else
#endif
        {
            if (graphic != null)
            {
                for (int i = 0; i < graphic.Length; i++)
                {
                    if (graphic[i])
                    {
                        graphic[i].CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
                    }
                }
            }
            if (graphic2 != null)
            {
                for (int i = 0; i < graphic2.Length; i++)
                {
                    if (graphic2[i])
                    {
                        graphic2[i].CrossFadeAlpha(m_IsOn ? 0f : 1f, instant ? 0f : 0.1f, true);
                    }
                }
            }
            //if (graphicScale)
            //{
            //    if (m_IsOn) this.transform.DOScale(scaleVaule_on, 0.2f);
            //    else this.transform.DOScale(scaleValue_off, 0.2f);
            //}
        }
    }

    /// <summary>
    /// Assume the correct visual state.
    /// </summary>
    protected override void Start()
    {
        PlayEffect(true);
    }

    private void InternalToggle()
    {
        if (!IsActive() || !IsInteractable())
            return;

        isOn = !isOn;
    }

    /// <summary>
    /// React to clicks.
    /// </summary>
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!disable)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
        }
        else
        {
            onDisableClick.Invoke();
        }
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        InternalToggle();
    }

    public void SwitchToggleOn()
    {
        if (onValueChangeHandler != null && onValueChangeHandler())
        {
            return;
        }

        if (isOn)
        {
            onValueChanged.Invoke(m_IsOn);
        }
        else
        {
            isOn = true;
        }
    }
    
    public void SwitchToggleOff()
    {
        if (onValueChangeHandler != null && onValueChangeHandler())
        {
            return;
        }

        if (!isOn)
        {
            onValueChanged.Invoke(m_IsOn);
        }
        else
        {
            isOn = false;
        }
    } 
}
