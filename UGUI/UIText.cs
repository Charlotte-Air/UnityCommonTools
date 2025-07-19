using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

[ExecuteInEditMode]
[AddComponentMenu("UI/UIText")]
public class UIText : Text, IPointerUpHandler, IPointerDownHandler, IPointerClickHandler
{
    [SerializeField] 
    public bool customFont = false;
    [SerializeField] 
    public UIFont m_UIFont;
    [SerializeField] 
    public bool supportPic = false;
    [SerializeField] 
    public float picScale = 1;
    [SerializeField] 
    public UIFont m_font;
    [SerializeField] 
    public float m_TextScale = 1;
    [SerializeField] 
    protected bool m_Gray = false;
    [SerializeField] 
    protected bool m_supportLetterSpacing = false;
    [SerializeField] 
    protected float m_letterSpacing = 0;
    [SerializeField] 
    public bool bTransferred = false;
    [SerializeField] 
    public bool isSetExtension = false;
    [TextArea(3, 10)] 
    [SerializeField] 
    public string m_UIString = String.Empty;
    
    public delegate string OnQueryString(string strText); //界面系统事件抛给用户管理
    public static OnQueryString moQuery = null;

    public override float preferredWidth
    {
        get
        {
            var settings = GetGenerationSettings(Vector2.zero);
            return cachedTextGeneratorForLayout.GetPreferredWidth(m_parsedText, settings) / pixelsPerUnit;
        }
    }

    public override float preferredHeight
    {
        get
        {
            var settings = GetGenerationSettings(new Vector2(rectTransform.rect.size.x, 0.0f));
            return cachedTextGeneratorForLayout.GetPreferredHeight(m_parsedText, settings) / pixelsPerUnit;
        }
    }

    public bool gray
    {
        set
        {
            if (m_Gray != value)
            {
                m_Gray = value;
                SetVerticesDirty();
            }
        }
        get { return m_Gray; }
    }

    public string parsedText { get => m_parsedText; }

    public float letterSpacing
    {
        get { return m_letterSpacing; }
        set
        {
            if (m_letterSpacing != value)
            {
                m_supportLetterSpacing = true;
                m_letterSpacing = value;
                SetVerticesDirty();
            }
        }
    }

    public int charCount = 0;
    public UITextPic m_textPic;
    public string m_parsedText = "";
    public List<UITextUrlInfo> m_urlInfos = new List<UITextUrlInfo>();
    public List<UITextPicInfo> m_picInfos = new List<UITextPicInfo>();
    public List<UITextGradientInfo> m_gradientInfos = new List<UITextGradientInfo>();
    public List<UITextUnderlineInfo> m_underlineInfos = new List<UITextUnderlineInfo>();

    [Serializable]
    public class UrlClickEvent : UnityEvent<string>
    {
        
    }

    [SerializeField] 
    private UrlClickEvent m_onUrlClick = new UrlClickEvent();
    public UrlClickEvent onUrlClick
    {
        get { return m_onUrlClick; }
        set { m_onUrlClick = value; }
    }

    [SerializeField] 
    private bool m_Interactable = false;
    
    [SerializeField] 
    private float m_HoldTimer = 0.3f;
    
    [SerializeField] 
    private UnityEvent m_OnHold = new UnityEvent();
    public UnityEvent onHold
    {
        get { return m_OnHold; }
        set { m_OnHold = value; }
    }

    [SerializeField] 
    private UnityEvent m_OnUp = new UnityEvent();
    public UnityEvent onUp
    {
        get { return m_OnUp; }
        set { m_OnUp = value; }
    }
    
    private int m_fontTexId = -1;
    private static UIVertex s_tempVertex = new UIVertex();
    private readonly UIVertex[] m_TempVerts = new UIVertex[4];
    private static UIVertex s_tempVertex1 = new UIVertex();
    private static UIVertex s_tempVertex2 = new UIVertex();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_textPic != null)
        {
            GameObject.DestroyImmediate(m_textPic.gameObject);
            m_textPic = null;
        }
    }

    protected override void OnEnable()
    {
        if (bTransferred)
        {
            if (!string.IsNullOrEmpty(m_UIString.Trim()) && !m_UIString.Equals("/n"))
            {
                if (moQuery != null)
                {
                    m_Text = moQuery(m_UIString);
                }
            }

            if (!string.IsNullOrEmpty(m_UIString))
            {
                if (moQuery != null)
                {
                    m_Text = moQuery(m_UIString);
                }
            }
        }

        if (supportPic)
        {
            CheckTextPic();
            if (m_textPic != null)
            {
                m_textPic.enabled = true;
            }
        }

        base.OnEnable();
    }

    protected override void OnDisable()
    {
        if (supportPic)
        {
            if (m_textPic != null)
            {
                m_textPic.enabled = false;
            }
        }

        base.OnDisable();
    }

    public override string text
    {
        get { return m_Text; }
        set
        {
            if (String.IsNullOrEmpty(value))
            {
                if (String.IsNullOrEmpty(m_Text))
                    return;
                m_Text = "";
                SetVerticesDirty();
            }
            else if (m_Text != value)
            {
                if (moQuery != null && bTransferred)
                {
                    m_Text = moQuery(value);
                }
                else
                {
                    if (isSetExtension)
                    {
                        m_Text = SetTextWithEllipsis(value);
                    }
                    else
                    {
                        m_Text = value;
                    }

                }

                SetVerticesDirty();
                SetLayoutDirty();
            }
        }
    }
    
    protected void OnBasePopulateMesh(VertexHelper toFill)
    {
        if (font == null)
            return;
        
        //垂直溢出的网格生成错误
        // 当我们进行更新时，我们不关心字体纹理是否改变。
        // CachedTextGenerator的最终结果将对这个实例有效。
        m_DisableFontTextureRebuiltCallback = true;
        
        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        settings.verticalOverflow = VerticalWrapMode.Overflow;
        cachedTextGenerator.PopulateWithErrors(text, settings, gameObject);

        // 将偏移量应用于顶点
        IList<UIVertex> verts = cachedTextGenerator.verts;
        float unitsPerPixel = 1 / pixelsPerUnit;
        int vertCount = verts.Count;

        // We have no verts to process just return (case 1037923)
        if (vertCount <= 0)
        {
            toFill.Clear();
            return;
        }

        Vector2 roundingOffset = new Vector2(verts[0].position.x, verts[0].position.y) * unitsPerPixel;
        roundingOffset = PixelAdjustPoint(roundingOffset) - roundingOffset;
        toFill.Clear();
        if (roundingOffset != Vector2.zero)
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                m_TempVerts[tempVertsIndex].position.x += roundingOffset.x;
                m_TempVerts[tempVertsIndex].position.y += roundingOffset.y;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }
        else
        {
            for (int i = 0; i < vertCount; ++i)
            {
                int tempVertsIndex = i & 3;
                m_TempVerts[tempVertsIndex] = verts[i];
                m_TempVerts[tempVertsIndex].position *= unitsPerPixel;
                if (tempVertsIndex == 3)
                    toFill.AddUIVertexQuad(m_TempVerts);
            }
        }

        m_DisableFontTextureRebuiltCallback = false;
    }
    
    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (customFont && m_UIFont != null)
        {
            Vector2 extents = rectTransform.rect.size;
            var settings = GetGenerationSettings(extents);
            m_UIFont.PopulateMesh(toFill, m_Text, m_TextScale, color, rectTransform, settings); //font
            AdjustLetterSpacing(toFill);
            return;
        }
        
        if (supportRichText)
        {
            string oldText = m_Text;
            m_Text = m_parsedText;
            OnBasePopulateMesh(toFill);
            m_Text = oldText;
            AdjustLetterSpacing(toFill);
            FillExtraMesh(toFill);
        }
        else
        {
            OnBasePopulateMesh(toFill);
            AdjustLetterSpacing(toFill);
        }

        if (m_Gray)
        {
            UIVertex vert = new UIVertex();
            int n = toFill.currentVertCount;
            for (int i = 0; i < n; i++)
            {
                toFill.PopulateUIVertex(ref vert, i);
                vert.uv1 = new Vector2(1, 0);
                toFill.SetUIVertex(vert, i);
            }
        }
    }

    protected override void UpdateMaterial()
    {
        if (!IsActive())
        {
            return;
        }

        if (customFont && m_UIFont != null)
        {
            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            canvasRenderer.SetTexture(m_UIFont.mainTexture);
            return;
        }

        if (m_fontTexId == -1)
        {
            if (materialForRendering.HasProperty("_FontTex"))
            {
                m_fontTexId = Shader.PropertyToID("_FontTex");
            }
        }

        if (m_fontTexId != -1)
        {
            canvasRenderer.materialCount = 1;
            canvasRenderer.SetMaterial(materialForRendering, 0);
            materialForRendering.SetTexture(m_fontTexId, mainTexture);
        }
        else
        {
            base.UpdateMaterial();
        }
    }

    public override void SetVerticesDirty()
    {
        base.SetVerticesDirty();

        if (supportRichText)
        {
            m_urlInfos.Clear();
            m_underlineInfos.Clear();
            m_gradientInfos.Clear();
            m_picInfos.Clear();
            if (m_textPic != null) m_textPic.Clear();
            m_parsedText = UITextParse.Parse(this);
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (supportPic)
        {
            if (m_textPic != null)
            {
                RectTransform src = rectTransform;
                RectTransform dest = m_textPic.rectTransform;
                dest.anchorMin = Vector2.zero;
                dest.anchorMax = Vector2.one;
                dest.pivot = src.pivot;
                dest.sizeDelta = Vector2.zero;
            }
        }

        base.OnRectTransformDimensionsChange();
    }
    
    protected void AdjustLetterSpacing(VertexHelper vh)
    {
        if (!m_supportLetterSpacing)
            return;

        float alignmentFactor = 0;

        switch (this.alignment)
        {
            case TextAnchor.LowerLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.UpperLeft:
                alignmentFactor = 0f;
                break;

            case TextAnchor.LowerCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.UpperCenter:
                alignmentFactor = 0.5f;
                break;

            case TextAnchor.LowerRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.UpperRight:
                alignmentFactor = 1f;
                break;
        }

        float letterOffset = m_letterSpacing * (float) this.fontSize / 100f;
        float lastx = float.MinValue;
        float letterSpaceIncr = 0;

        int curLineStart = 0;
        int charIdx = 0;
        int curLineCharCount = 0;

        Vector3 pos;

        for (charIdx = 0; charIdx < parsedText.Length; charIdx++)
        {
            if (charIdx * 4 + 3 > vh.currentVertCount - 1)
                return;

            vh.PopulateUIVertex(ref s_tempVertex1, charIdx * 4 + 0);
            vh.PopulateUIVertex(ref s_tempVertex2, charIdx * 4 + 1);

            if (s_tempVertex1.position.x < lastx)
            {
                // is new line
                letterSpaceIncr = 0;

                pos = -Vector3.right * (curLineCharCount - 1) * letterOffset * alignmentFactor;
                for (int c = charIdx - 1; c >= curLineStart; c--)
                {
                    SetCharVertexOffset(vh, c, pos);
                }

                curLineStart = charIdx;
                curLineCharCount = 0;
            }

            pos = Vector3.right * letterSpaceIncr;
            SetCharVertexOffset(vh, charIdx, pos);

            // skip invisiable char
            if (s_tempVertex2.position.x - s_tempVertex1.position.x > 0.1f)
            {
                letterSpaceIncr += letterOffset;
                curLineCharCount++;
                lastx = s_tempVertex1.position.x;
            }
        }

        pos = -Vector3.right * (curLineCharCount - 1) * letterOffset * alignmentFactor;
        for (int c = charIdx - 1; c >= curLineStart; c--)
        {
            SetCharVertexOffset(vh, c, pos);
        }
    }
    
    private void SetCharVertexOffset(VertexHelper vh, int charIdx, Vector3 offset)
    {
        for (int i = 0; i < 4; i++)
        {
            int idx = charIdx * 4 + i;
            if (idx > vh.currentVertCount - 1)
                return;

            vh.PopulateUIVertex(ref s_tempVertex, idx);
            s_tempVertex.position += offset;
            vh.SetUIVertex(s_tempVertex, idx);
        }
    }

    private void FillExtraMesh(VertexHelper toFill)
    {
        Vector2 roundingOffset = PixelAdjustPoint(Vector2.zero);
        float unitsPerPixel = 1 / pixelsPerUnit;
        float letterSpacing = (m_supportLetterSpacing && m_letterSpacing > 0)   ? m_letterSpacing  : 0;

        // pic
        if (supportPic && m_textPic != null)
        {
            m_textPic.Clear();
            m_textPic.AddPicRects(toFill, m_picInfos, this.cachedTextGenerator.lines, roundingOffset,
                unitsPerPixel, !gray);
        }

        // url rect
        for (int i = 0; i < m_urlInfos.Count; i++)
        {
            m_urlInfos[i].UpdateRects(toFill);
        }

        // underline
        if (m_underlineInfos.Count > 0)
        {
            // TODO opt later
            Vector2 extents = rectTransform.rect.size;
            var settings = GetGenerationSettings(extents);
            TextGenerator underlineTextGenerator = new TextGenerator();
            underlineTextGenerator.Populate("■", settings);

            IList<UnityEngine.UIVertex> verts = underlineTextGenerator.verts;
            IList<UnityEngine.UICharInfo> chars = cachedTextGenerator.characters;
            IList<UnityEngine.UILineInfo> lines = cachedTextGenerator.lines;

            for (int i = 0; i < m_underlineInfos.Count; i++)
            {
                UITextUnderlineInfo info = m_underlineInfos[i];
                info.AddQuad(m_parsedText, color, toFill, chars, lines, verts, roundingOffset, unitsPerPixel,
                    letterSpacing);
            }
        }

        // gradient color
        for (int i = 0; i < m_gradientInfos.Count; i++)
        {
            m_gradientInfos[i].AddGradientColor(toFill);
        }
    }

    private void CheckTextPic()
    {
        if (supportPic && m_font)
        {
            if (m_textPic == null)
            {
                GameObject gm = null;

                Transform pic = transform.Find("pic_dontsave");
                if (pic != null)
                {
                    gm = pic.gameObject;
                }

                if (gm == null)
                {
                    gm = new GameObject();
                    //gm.hideFlags = HideFlags.HideAndDontSave | HideFlags.NotEditable;
                    gm.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
                    gm.name = "pic_dontsave";
                    gm.transform.SetParent(transform, false);
                }

                gm.transform.localPosition = Vector2.zero;
                gm.transform.localScale = Vector3.one;

                gm.GetOrAddComponent<CanvasRenderer>();

                m_textPic = gm.GetOrAddComponent<UITextPic>();
                m_textPic.m_font = m_font;
                if (m_font != null && m_font.m_matrial != null)
                {
                    m_textPic.material = m_font.m_matrial;
                }
                m_textPic.raycastTarget = false;

                RectTransform src = rectTransform;
                RectTransform dest = m_textPic.rectTransform;
                dest.anchorMin = Vector2.zero;
                dest.anchorMax = Vector2.one;
                dest.pivot = src.pivot;
                dest.sizeDelta = Vector2.zero;
            }
        }
        else
        {
            m_font = null;
            if (m_textPic != null)
            {
                GameObject.DestroyImmediate(m_textPic.gameObject);
                m_textPic = null;
            }
        }
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        if (m_Interactable)
        {
            CancelInvokeHold();
            m_OnUp.Invoke();
        }
        else
        {
            if (transform.parent != null)
            {
                GameObject gm = ExecuteEvents.GetEventHandler<IPointerUpHandler>(transform.parent.gameObject);
                if (gm != null) gm.GetComponent<IPointerUpHandler>().OnPointerUp(eventData);
            }

            return;
        }
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (m_Interactable)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            DelayInvokeHold();
        }
        else
        {
            if (transform.parent != null)
            {
                GameObject gm = ExecuteEvents.GetEventHandler<IPointerDownHandler>(transform.parent.gameObject);
                if (gm != null) gm.GetComponent<IPointerDownHandler>().OnPointerDown(eventData);
            }

            return;
        }
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (m_urlInfos.Count > 0)
        {
            Vector2 lp;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out lp);

            for (int i = 0; i < m_urlInfos.Count; i++)
            {
                UITextUrlInfo info = m_urlInfos[i];
                for (int j = 0; j < info.rects.Count; j++)
                {
                    if (info.rects[j].Contains(lp))
                    {
                        m_onUrlClick.Invoke(info.url);
                        return;
                    }
                }
            }
        }

        if (m_Interactable)
        {
            CancelInvokeHold();
        }
        else
        {
            if (transform.parent != null)
            {
                GameObject gm = ExecuteEvents.GetEventHandler<IPointerClickHandler>(transform.parent.gameObject);
                if (gm != null) gm.GetComponent<IPointerClickHandler>().OnPointerClick(eventData);
            }
        }
    }

    private void CancelInvokeHold()
    {
        if (IsInvoking("InvokeHold"))
        {
            CancelInvoke("InvokeHold");
        }
    }

    private void DelayInvokeHold()
    {
        CancelInvokeHold();
        Invoke("InvokeHold", m_HoldTimer);
    }

    private void InvokeHold()
    {
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            m_OnHold.Invoke();
        }
    }

    public string SetTextWithEllipsis(string value, float maxWidth = 400)
    {
        // 用值和当前矩形创建生成器
        var generator = new TextGenerator();
        var rectTransform = this.GetComponent<RectTransform>();

        var settings = this.GetGenerationSettings(new Vector2(maxWidth, 200));
        generator.Populate(value, settings);

        // 截断可见值并添加省略号
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = value;
        if (value.Length > characterCountVisible)
        {
            updatedText = value.Substring(0, characterCountVisible - 1);
            updatedText += "…";
        }
        return updatedText;
    }

#if UNITY_EDITOR
    private bool m_needCheckTextPic = false;

    protected virtual void Update()
    {
        if (m_needCheckTextPic)
        {
            CheckTextPic();
            m_needCheckTextPic = false;
        }
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        m_needCheckTextPic = true;
    }
#endif
}