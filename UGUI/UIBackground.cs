using UnityEngine;
using UnityEngine.UI;

public class UIBackground : MonoBehaviour
{
    [SerializeField]
    public bool isCreateBackground = true;
    [SerializeField]
    public Color BackgroundColor = new Color(0, 0, 0, 0.65f);
    [SerializeField]
    public bool isClickClose = true;
    [SerializeField]
    public bool useClickCloseEvent = false;
    [SerializeField]
    public Button.ButtonClickedEvent onClickClose = new Button.ButtonClickedEvent();

    private UIImage m_imgClickCloseBg;

    private void Awake()
    {
        GameObject go = new GameObject();
        go.name = "_bg_";
        go.layer = 4;//GameLayer.Layer_UI;
        go.transform.SetParent(transform, false);
        go.transform.SetAsFirstSibling();
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.pivot = Vector2.zero;
        UIImage img = go.AddComponent<UIImage>();
        img.color = BackgroundColor;
        img.noGraphics = !isCreateBackground;
        m_imgClickCloseBg = img;

        if (isClickClose)
        {
            UIButton btn = m_imgClickCloseBg.gameObject.AddComponent<UIButton>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(OnClickClose);
        }
    }

    private void OnDestry()
    {
        if (m_imgClickCloseBg != null)
        {
            GameObject.Destroy(m_imgClickCloseBg.gameObject);
            m_imgClickCloseBg = null;
        }
    }

    private void OnClickClose()
    {
        if (useClickCloseEvent)
        {
            onClickClose.Invoke();
        }
        else
        {

        }
    }
}
