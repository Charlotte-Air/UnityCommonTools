using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class CustomButton : MonoBehaviour, IPointerClickHandler
{
    public float Scale = -1;
    public bool ShowShadow = true;
    public string ShadowColor = "";
    // 音效类型
    public enum AudioType
    {
        CANCEL = 32,
        CONFIRM = 33,
        SWITCH = 53,
    }
    public AudioType Audio = AudioType.CONFIRM;                             // 点击音效
    // Mgr
    private AudioManager m_audioMgr = null;                                 // 音效管理
    // Data    
    private Vector2 m_originSize = Vector2.zero;                            // 根节点原大小(图片大小)
    private string m_strCurText = "";                                       // 子节点文本文字 String
    private string Blue_Text_Color = "ffffff";
    private string Orange_Text_Color = "fff3d7";
    private string Frame_Text_Color = "ffffff";
    private string Blue_Shadow_Color = "0076ab";
    private string Orange_Shadow_Color = "c55d17";
    
    // UI
    private RectTransform m_rectBtn = null;                                 // 根节点 RectTransform
    private Image m_imgBtn = null;                                          // 根节点图片 Image
    private GameObject m_oText = null;                                      // 子节点文本 GameObject
    private RectTransform m_rectText = null;                                // 子节点文本 RectTransform
    private Text m_txtText = null;                                          // 子节点文本 Text
    private Shadow m_shadow = null;                                         // 子节点文本阴影 Shadow
    
    
    void Start()
    {
        m_audioMgr = AudioManager.Instance;
        initUI();
        initSize();
        updateBtnScale();
        updateShadowTextColor();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            updateBtnScale();
            updateText();
            SetShowShadowTextState(ShowShadow);
        };
    }
#endif

    void initUI()
    {
        m_rectBtn = gameObject.GetComponent<RectTransform>();
        m_imgBtn = gameObject.GetComponent<Image>();
        if (transform.childCount == 0)
        {
            return;
        }
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<Text>() != null)
            {
                m_oText = child;
                m_rectText = m_oText.GetComponent<RectTransform>();
                m_txtText = m_oText.GetComponent<Text>();
                if (child.GetComponent<Shadow>() == null)
                {
                    m_shadow = child.AddComponent<Shadow>();
                }
                else
                {
                    m_shadow = child.GetComponent<Shadow>();
                }
                m_shadow.effectDistance = new Vector2(0, -2);
                break;
            }
        }
    }

    void initSize()
    {
        if (m_imgBtn == null) 
        {
            return;
        }
        m_imgBtn.SetNativeSize();
        m_originSize = m_rectBtn.sizeDelta;
    }

    void updateBtnScale()
    {
        if (m_imgBtn == null) 
        {
            return;
        }
        if (Scale == -1)
        {
            initSize();
            m_rectBtn.sizeDelta = m_originSize;
            if (m_rectText != null) 
            {
                m_rectText.localScale = Vector2.one;
            }
            return;
        }
        initSize();

        m_rectBtn.sizeDelta = new Vector2(m_originSize.x * Scale, m_originSize.y * Scale);
        if (m_rectText != null)
        {
            m_rectText.localScale = new Vector3(Scale, Scale, Scale);
        }
    }

    void updateText()
    {
        if (m_txtText == null || m_txtText.text == m_strCurText)
        {
            return;
        }

        if (m_imgBtn != null) 
        {
            if (m_imgBtn.mainTexture.name.StartsWith("btn_blue_"))
            {
                m_txtText.color = GetColorByHex(Blue_Text_Color);
                m_txtText.fontSize = m_imgBtn.mainTexture.name.EndsWith("l") ? 36
                    : m_imgBtn.mainTexture.name.EndsWith("m") ? 32
                    : m_imgBtn.mainTexture.name.EndsWith("_s") ? 30
                    : m_imgBtn.mainTexture.name.EndsWith("xs") ? 22 : m_txtText.fontSize;
            }
            else if (m_imgBtn.mainTexture.name.StartsWith("btn_yellow_"))
            {
                m_txtText.color = GetColorByHex(Orange_Text_Color);
                m_txtText.fontSize = m_imgBtn.mainTexture.name.EndsWith("l") ? 36
                    : m_imgBtn.mainTexture.name.EndsWith("m") ? 32
                    : m_imgBtn.mainTexture.name.EndsWith("_s") ? 30
                    : m_imgBtn.mainTexture.name.EndsWith("xs") ? 22 : m_txtText.fontSize;
            }
            else if (m_imgBtn.mainTexture.name.StartsWith("btn_frame"))
            {
                m_txtText.color = GetColorByHex(Frame_Text_Color);
                m_txtText.fontSize = 28;
            }
        }

        SetBtnText(m_txtText.text);
    }

    void updateShadowTextColor()
    {
        if (m_shadow == null || m_imgBtn == null || m_imgBtn.mainTexture == null)
        {
            return;
        }

        if (m_imgBtn.mainTexture.name.StartsWith("btn_frame"))
        {
            SetShowShadowTextState(false);
        }
        else if (m_imgBtn.mainTexture.name.StartsWith("btn_blue_"))
        {
            SetShadowTextColor(Blue_Shadow_Color);
        }
        else if (m_imgBtn.mainTexture.name.StartsWith("btn_yellow_"))
        {
            SetShadowTextColor(Orange_Shadow_Color);
        }
        else 
        {
            if (!string.IsNullOrEmpty(ShadowColor) && ShadowColor.Length >= 6) 
            {
                SetShadowTextColor(ShadowColor);
            }
            else 
            {
                SetShowShadowTextState(false);
            }
        }
    }

    public void SetBtnText(string str)
    {
        str = Lang.GetByString(str);
        if (m_txtText != null)
        {
            m_strCurText = str;
            m_txtText.text = str;
        }
    }

    public void SetShadowTextColor(string hex)
    {
        if (m_shadow == null)
        {
            return;
        }
        ShadowColor = hex;
        m_shadow.effectColor = GetColorByHex(hex);
    }

    public void SetShowShadowTextState(bool isActive)
    {
        ShowShadow = isActive;
        if (!isActive)
        {
            DestroyImmediate(m_shadow);
            m_shadow = null;
            ShadowColor = null;
        }
        else
        {
            if (m_shadow == null && m_oText != null)
            {
                m_shadow = m_oText.AddComponent<Shadow>();
                ShadowColor = "000000";
                m_shadow.effectDistance = new Vector2(0, -2);
                updateShadowTextColor();
            }
            else if (m_shadow != null && !m_shadow.enabled)
            {
                m_shadow.enabled = isActive;
            }
            if (!string.IsNullOrEmpty(ShadowColor) && ShadowColor.Length >= 6)
            {
                SetShadowTextColor(ShadowColor);
            }
        }
    }

    // 点击
    public void OnPointerClick(PointerEventData eventData)
    {
        PlayBtnAudio();
    }

    // 播放音乐
    private void PlayBtnAudio()
    {
        // m_audioMgr.PlayEffect((ushort)Audio);
    }
    
    
    public Color GetColorByHex(string hex)
    {
        byte r = System.Convert.ToByte("0x" + hex.Substring(0, 2), 16);
        byte g = System.Convert.ToByte("0x" + hex.Substring(2, 2), 16);
        byte b = System.Convert.ToByte("0x" + hex.Substring(4, 2), 16);
        byte a = 255;
        if (hex.Length >= 8)
            a = System.Convert.ToByte("0x" + hex.Substring(6, 2), 16);
        return new Color32(r, g, b, a);
    }
}