using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using Charlotte.Client.Logic;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine.Pool;

[AddComponentMenu("UI/UIDropdown")]
[RequireComponent(typeof(RectTransform))]
public class UIDropdown : Selectable, IPointerClickHandler, ISubmitHandler, ICancelHandler
{
    protected internal class UIDropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
    {
        [SerializeField]
        private UIText m_Text;
        [SerializeField]
        private UIImage m_Image;
        [SerializeField]
        private RectTransform m_RectTransform;
        [SerializeField]
        private UIToggle m_Toggle;

        public UIText text { get { return m_Text; } set { m_Text = value; } }
        public UIImage image { get { return m_Image; } set { m_Image = value; } }
        public RectTransform rectTransform { get { return m_RectTransform; } set { m_RectTransform = value; } }
        public UIToggle toggle { get { return m_Toggle; } set { m_Toggle = value; } }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            UIDropdown dropdown = GetComponentInParent<UIDropdown>();
            if (dropdown)
                dropdown.Hide();
        }
    }

    [Serializable]
    public class OptionData
    {
        [SerializeField]
        private string m_Text;
        [SerializeField]
        private Sprite m_Image;

        public string text { get { return m_Text; } set { m_Text = value; } }
        public Sprite image { get { return m_Image; } set { m_Image = value; } }

        public OptionData()
        {
        }

        public OptionData(string text)
        {
            this.text = text;
        }

        public OptionData(Sprite image)
        {
            this.image = image;
        }

        public OptionData(string text, Sprite image)
        {
            this.text = text;
            this.image = image;
        }
    }

    [Serializable]
    public class OptionDataList
    {
        [SerializeField]
        private List<OptionData> m_Options;
        public List<OptionData> options { get { return m_Options; } set { m_Options = value; } }


        public OptionDataList()
        {
            options = new List<OptionData>();
        }
    }

    [Serializable]
    public class DropdownEvent : UnityEvent<int> { }

    [SerializeField]
    private bool m_bTransferred;

    // Template used to create the dropdown.
    [SerializeField]
    private RectTransform m_Template;
    public RectTransform template { get { return m_Template; } set { m_Template = value; RefreshShownValue(); } }

    // UIText to be used as a caption for the current value. It's not required, but it's kept here for convenience.
    [SerializeField]
    private UIText m_CaptionText;
    public UIText captionText { get { return m_CaptionText; } set { m_CaptionText = value; RefreshShownValue(); } }

    [SerializeField]
    private UIImage m_CaptionImage;
    public UIImage captionImage { get { return m_CaptionImage; } set { m_CaptionImage = value; RefreshShownValue(); } }

    [Space]

    [SerializeField]
    private UIText m_ItemText;
    public UIText itemText { get { return m_ItemText; } set { m_ItemText = value; RefreshShownValue(); } }

    [SerializeField]
    private UIImage m_ItemImage;
    public UIImage itemImage { get { return m_ItemImage; } set { m_ItemImage = value; RefreshShownValue(); } }

    [Space]

    [SerializeField]
    private int m_Value;

    [Space]

    // Items that will be visible when the dropdown is shown.
    // We box this into its own class so we can use a Property Drawer for it.
    [SerializeField]
    private OptionDataList m_Options = new OptionDataList();
    public List<OptionData> options
    {
        get { return m_Options.options; }
        set { m_Options.options = value; RefreshShownValue(); }
    }

    [Space]

    // Notification triggered when the dropdown changes.
    [SerializeField]
    private DropdownEvent m_OnValueChanged = new DropdownEvent();
    public DropdownEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    private UnityAction m_OnEmptyClick = null;
    public UnityAction onEmptyClick { get { return m_OnEmptyClick; } set { m_OnEmptyClick = value; } }

    private GameObject m_Dropdown;
    private GameObject m_Blocker;
    private List<UIDropdownItem> m_Items = new List<UIDropdownItem>();
    private TweenRunner<FloatTween> m_AlphaTweenRunner;
    private bool validTemplate = false;

    private static OptionData s_NoOptionData = new OptionData();

    private RectTransform rectTransform;
    private float m_DefaultHeight;

    // Current value.
    public int value
    {
        get
        {
            return m_Value;
        }
        set
        {
            if (Application.isPlaying && (value == m_Value || options.Count == 0))
                return;

            m_Value = Mathf.Clamp(value, 0, options.Count - 1);
            RefreshShownValue();

            // Notify all listeners
            m_OnValueChanged.Invoke(m_Value);
        }
    }

    protected UIDropdown()
    { }

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        m_AlphaTweenRunner = new TweenRunner<FloatTween>();
        m_AlphaTweenRunner.Init(this);

        if (m_CaptionImage)
            m_CaptionImage.enabled = (m_CaptionImage.sprite != null);

        if (m_Template)
            m_Template.gameObject.SetActive(false);

        rectTransform = this.GetComponent<RectTransform>();
        m_DefaultHeight = rectTransform.sizeDelta.y;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!IsActive())
            return;

        RefreshShownValue();
    }

#endif

    public void RefreshShownValue()
    {
        OptionData data = s_NoOptionData;

        if (options.Count > 0)
            data = options[Mathf.Clamp(m_Value, 0, options.Count - 1)];

        if (m_CaptionText)
        {
            if (data != null && data.text != null)
                m_CaptionText.text = data.text;
            else
                m_CaptionText.text = "";
        }

        if (m_CaptionImage)
        {
            if (data != null)
                m_CaptionImage.sprite = data.image;
            else
                m_CaptionImage.sprite = null;
            m_CaptionImage.enabled = (m_CaptionImage.sprite != null);
        }
    }

    public void AddOptions(List<OptionData> options)
    {
        this.options.AddRange(options);
        RefreshShownValue();
    }

    public void AddOptions(List<string> options)
    {
        for (int i = 0; i < options.Count; i++)
            this.options.Add(new OptionData(options[i]));
        RefreshShownValue();
    }

    public void AddOptions(List<Sprite> options)
    {
        for (int i = 0; i < options.Count; i++)
            this.options.Add(new OptionData(options[i]));
        RefreshShownValue();
    }

    public void ClearOptions()
    {
        options.Clear();
        RefreshShownValue();
    }

    private void SetupTemplate()
    {
        validTemplate = false;

        if (!m_Template)
        {
            Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a UIToggle component serving as the item.", this);
            return;
        }

        GameObject templateGo = m_Template.gameObject;
        templateGo.SetActive(true);
        UIToggle itemToggle = m_Template.GetComponentInChildren<UIToggle>();

        validTemplate = true;
        if (!itemToggle || itemToggle.transform == template)
        {
            validTemplate = false;
            Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a UIToggle component serving as the item.", template);
        }
        else if (!(itemToggle.transform.parent is RectTransform))
        {
            validTemplate = false;
            Debug.LogError("The dropdown template is not valid. The child GameObject with a UIToggle component (the item) must have a RectTransform on its parent.", template);
        }
        else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
        {
            validTemplate = false;
            Debug.LogError("The dropdown template is not valid. The Item UIText must be on the item GameObject or children of it.", template);
        }
        else if (itemImage != null && !itemImage.transform.IsChildOf(itemToggle.transform))
        {
            validTemplate = false;
            Debug.LogError("The dropdown template is not valid. The Item UIImage must be on the item GameObject or children of it.", template);
        }

        if (!validTemplate)
        {
            templateGo.SetActive(false);
            return;
        }

        UIDropdownItem item = itemToggle.gameObject.AddComponent<UIDropdownItem>();
        item.text = m_ItemText;
        item.image = m_ItemImage;
        item.toggle = itemToggle;
        item.rectTransform = (RectTransform)itemToggle.transform;

        Canvas popupCanvas = GetOrAddComponent<Canvas>(templateGo);
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = 30000;
        popupCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;

        GetOrAddComponent<GraphicRaycaster>(templateGo);
        GetOrAddComponent<CanvasGroup>(templateGo);
        templateGo.SetActive(false);

        validTemplate = true;
    }

    private static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (!comp)
            comp = go.AddComponent<T>();
        return comp;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        Show();
    }

    public virtual void OnSubmit(BaseEventData eventData)
    {
        Show();
    }

    public virtual void OnCancel(BaseEventData eventData)
    {
        Hide();
    }

    // Show the dropdown.
    //
    // Plan for dropdown scrolling to ensure dropdown is contained within screen.
    //
    // We assume the Canvas is the screen that the dropdown must be kept inside.
    // This is always valid for screen space canvas modes.
    // For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
    // We consider it a fair constraint that the canvas must be big enough to contains dropdowns.
    public void Show()
    {
        if (!IsActive() || !IsInteractable() || m_Dropdown != null)
            return;

        if (!validTemplate)
        {
            SetupTemplate();
            if (!validTemplate)
                return;
        }

        // Get root Canvas.
        var list = ListPool<Canvas>.Get();
        gameObject.GetComponentsInParent(false, list);
        if (list.Count == 0)
        {
            if (m_OnEmptyClick != null)
                m_OnEmptyClick.Invoke();
            return;
        }
        Canvas rootCanvas = list[0];
        ListPool<Canvas>.Release(list);

        m_Template.gameObject.SetActive(true);

        // Instantiate the drop-down template
        m_Dropdown = CreateDropdownList(m_Template.gameObject);
        m_Dropdown.name = "UIDropdown List";
        m_Dropdown.SetActive(true);

        // Make drop-down RectTransform have same values as original.
        RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
        dropdownRectTransform.SetParent(m_Template.transform.parent, false);

        // Instantiate the drop-down list items

        // Find the dropdown item and disable it.
        UIDropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<UIDropdownItem>();

        GameObject content = itemTemplate.rectTransform.parent.gameObject;
        RectTransform contentRectTransform = content.transform as RectTransform;
        itemTemplate.rectTransform.gameObject.SetActive(true);

        // Get the rects of the dropdown and item
        Rect dropdownContentRect = contentRectTransform.rect;
        Rect itemTemplateRect = itemTemplate.rectTransform.rect;

        // Calculate the visual offset between the item's edges and the background's edges
        Vector2 offsetMin = itemTemplateRect.min - dropdownContentRect.min + (Vector2)itemTemplate.rectTransform.localPosition;
        Vector2 offsetMax = itemTemplateRect.max - dropdownContentRect.max + (Vector2)itemTemplate.rectTransform.localPosition;
        Vector2 itemSize = itemTemplateRect.size;

        m_Items.Clear();

        UIToggle prev = null;
        for (int i = 0; i < options.Count; ++i)
        {
            OptionData data = options[i];
            UIDropdownItem item = AddItem(data, value == i, itemTemplate, m_Items);
            if (item == null)
                continue;

            // Automatically set up a toggle state change listener
            item.toggle.isOn = value == i;
            item.toggle.onValueChanged.AddListener(x => OnSelectItem(item.toggle));

            // Select current option
            if (item.toggle.isOn)
                item.toggle.Select();

            // Automatically set up explicit navigation
            if (prev != null)
            {
                Navigation prevNav = prev.navigation;
                Navigation toggleNav = item.toggle.navigation;
                prevNav.mode = Navigation.Mode.Explicit;
                toggleNav.mode = Navigation.Mode.Explicit;

                prevNav.selectOnDown = item.toggle;
                prevNav.selectOnRight = item.toggle;
                toggleNav.selectOnLeft = prev;
                toggleNav.selectOnUp = prev;

                prev.navigation = prevNav;
                item.toggle.navigation = toggleNav;
            }
            prev = item.toggle;
        }

        // Reposition all items now that all of them have been added
        Vector2 sizeDelta = contentRectTransform.sizeDelta;
        sizeDelta.y = itemSize.y * m_Items.Count + offsetMin.y - offsetMax.y;
        contentRectTransform.sizeDelta = sizeDelta;

        //float extraSpace = dropdownRectTransform.rect.height - contentRectTransform.rect.height;
        //if (extraSpace > 0)
        //    dropdownRectTransform.sizeDelta = new Vector2(dropdownRectTransform.sizeDelta.x, dropdownRectTransform.sizeDelta.y - extraSpace);

        // Invert anchoring and position if dropdown is partially or fully outside of canvas rect.
        // Typically this will have the effect of placing the dropdown above the button instead of below,
        // but it works as inversion regardless of initial setup.
        Vector3[] corners = new Vector3[4];
        dropdownRectTransform.GetWorldCorners(corners);

        RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
        Rect rootCanvasRect = rootCanvasRectTransform.rect;
        for (int axis = 0; axis < 2; axis++)
        {
            bool outside = false;
            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                if (corner[axis] < rootCanvasRect.min[axis] || corner[axis] > rootCanvasRect.max[axis])
                {
                    outside = true;
                    break;
                }
            }
            if (outside)
                RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, axis, false, false);
        }

        for (int i = 0; i < m_Items.Count; i++)
        {
            RectTransform itemRect = m_Items[i].rectTransform;
            itemRect.anchorMin = new Vector2(itemRect.anchorMin.x, 0);
            itemRect.anchorMax = new Vector2(itemRect.anchorMax.x, 0);
            itemRect.anchoredPosition = new Vector2(itemRect.anchoredPosition.x, offsetMin.y + itemSize.y * (m_Items.Count - 1 - i) + itemSize.y * itemRect.pivot.y);
            itemRect.sizeDelta = new Vector2(itemRect.sizeDelta.x, itemSize.y);
        }

        // Fade in the popup
        //AlphaFadeList(0.15f, 0f, 1f);

        // Make drop-down template and item template inactive
        m_Template.gameObject.SetActive(false);
        itemTemplate.gameObject.SetActive(false);

        m_Blocker = CreateBlocker(rootCanvas);

        //RectTransform rt = m_Dropdown.GetComponent<RectTransform>();
        //rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + rt.sizeDelta.y);

        //RebuildLayout();
    }

    protected virtual GameObject CreateBlocker(Canvas rootCanvas)
    {
        // Create blocker GameObject.
        GameObject blocker = new GameObject("Blocker");

        // Setup blocker RectTransform to cover entire root canvas area.
        RectTransform blockerRect = blocker.AddComponent<RectTransform>();
        blockerRect.SetParent(rootCanvas.transform, false);
        blockerRect.anchorMin = Vector3.zero;
        blockerRect.anchorMax = Vector3.one;
        blockerRect.sizeDelta = Vector2.zero;

        // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
        Canvas blockerCanvas = blocker.AddComponent<Canvas>();
        blockerCanvas.overrideSorting = true;
        blockerCanvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
        Canvas dropdownCanvas = m_Dropdown.GetComponent<Canvas>();
        blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
        blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

        // Add raycaster since it's needed to block.
        blocker.AddComponent<GraphicRaycaster>();

        // Add image since it's needed to block, but make it clear.
        UIImage blockerImage = blocker.AddComponent<UIImage>();
        blockerImage.color = Color.clear;

        // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
        Button blockerButton = blocker.AddComponent<Button>();
        blockerButton.onClick.AddListener(Hide);

        return blocker;
    }

    protected virtual void DestroyBlocker(GameObject blocker)
    {
        Destroy(blocker);
    }

    protected virtual GameObject CreateDropdownList(GameObject template)
    {
        return (GameObject)Instantiate(template);
    }

    protected virtual void DestroyDropdownList(GameObject dropdownList)
    {
        Destroy(dropdownList);
    }

    protected virtual UIDropdownItem CreateItem(UIDropdownItem itemTemplate)
    {
        return (UIDropdownItem)Instantiate(itemTemplate);
    }

    protected virtual void DestroyItem(UIDropdownItem item)
    {
        // No action needed since destroying the dropdown list destroys all contained items as well.
    }

    // Add a new drop-down list item with the specified values.
    private UIDropdownItem AddItem(OptionData data, bool selected, UIDropdownItem itemTemplate, List<UIDropdownItem> items)
    {
        // Add a new item to the dropdown.
        UIDropdownItem item = CreateItem(itemTemplate);
        item.rectTransform.SetParent(itemTemplate.rectTransform.parent, false);

        item.gameObject.SetActive(true);
        //item.gameObject.name = "Item " + items.Count + (data.text != null ? ": " + data.text : "");

        if (item.toggle != null)
        {
            item.toggle.isOn = false;
        }

        item.text.bTransferred = this.m_bTransferred;
        // Set the item's data
        if (item.text)
            item.text.text = data.text;
        if (item.image)
        {
            item.image.sprite = data.image;
            item.image.enabled = (item.image.sprite != null);
        }

        items.Add(item);
        return item;
    }

    private void AlphaFadeList(float duration, float alpha)
    {
        CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
        AlphaFadeList(duration, group.alpha, alpha);
    }

    private void AlphaFadeList(float duration, float start, float end)
    {
        if (end.Equals(start))
            return;

        FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
        tween.AddOnChangedCallback(SetAlpha);
        tween.ignoreTimeScale = true;
        m_AlphaTweenRunner.StartTween(tween);
    }

    private void SetAlpha(float alpha)
    {
        if (!m_Dropdown)
            return;
        CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
        group.alpha = alpha;
    }

    // Hide the dropdown.
    public void Hide()
    {
        if (m_Dropdown != null)
        {
            AlphaFadeList(0.15f, 0f);

            // User could have disabled the dropdown during the OnValueChanged call.
            if (IsActive())
                StartCoroutine(DelayedDestroyDropdownList(0.15f));
        }
        if (m_Blocker != null)
            DestroyBlocker(m_Blocker);
        m_Blocker = null;

        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, m_DefaultHeight);

        Select();
        RebuildLayout();
    }

    private IEnumerator DelayedDestroyDropdownList(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        for (int i = 0; i < m_Items.Count; i++)
        {
            if (m_Items[i] != null)
                DestroyItem(m_Items[i]);
        }
        m_Items.Clear();
        if (m_Dropdown != null)
            DestroyDropdownList(m_Dropdown);
        m_Dropdown = null;
    }

    // Change the value and hide the dropdown.
    private void OnSelectItem(UIToggle toggle)
    {
        if (!toggle.isOn)
            toggle.isOn = true;

        int selectedIndex = -1;
        Transform tr = toggle.transform;
        Transform parent = tr.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i) == tr)
            {
                // Subtract one to account for template child.
                selectedIndex = i - 1;
                break;
            }
        }

        if (selectedIndex < 0)
            return;

        value = selectedIndex;
        Hide();
    }

    private void RebuildLayout()
    {
        HorizontalOrVerticalLayoutGroup layout = transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
        if (layout)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layout.gameObject.GetComponent<RectTransform>());
        }
    }
}