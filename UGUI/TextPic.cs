using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TextPic : UIText, IPointerDownHandler ,IPointerUpHandler
{
    /// <summary>
    /// 图片池
    /// </summary>
    private readonly List<Image> m_ImagesPool = new List<Image>();

    /// <summary>
    /// 图片的最后一个顶点的索引
    /// </summary>
    private readonly List<int> m_ImagesVertexIndex = new List<int>();

    private List<string> listIconName = new List<string>();

    [SerializeField]
    private GameObject orgChatExpress = null;

    public float underLineOffset = 3.0f;

    /// <summary>
    /// 正则取出所需要的属性
    /// </summary>
    private static readonly Regex s_Regex = new Regex(@"<quad name=(.+?) sizex=(\d*\.?\d+%?) sizey=(\d*\.?\d+%?) width=(\d*\.?\d+%?) />", RegexOptions.Singleline);
    
    public override void SetVerticesDirty()
    {
        if (orgChatExpress == null)
            return;
        
        base.SetVerticesDirty();
        UpdateQuadImage();
    }

    /// <summary>
    /// 释放
    /// </summary>
    public void Close()
    {
       if(m_ImagesVertexIndex != null)
            m_ImagesVertexIndex.Clear();
        if (listIconName != null)
            listIconName.Clear();
        m_OrgText = string.Empty;
        m_OutputText = string.Empty;
        text = string.Empty;
        m_Text = string.Empty;
    }

    /// <summary>
    /// 解析完最终的文本
    /// </summary>
    private string m_OutputText;

    /// <summary>
    /// 未解析文本
    /// </summary>
    public string m_OrgText;

    protected void UpdateQuadImage()
    {
#if UNITY_EDITOR
        #if UNITY_5
        if (UnityEditor.PrefabUtility.GetPrefabType(this) == UnityEditor.PrefabType.Prefab)
        #else
        if (UnityEditor.PrefabUtility.GetPrefabAssetType(this) == UnityEditor.PrefabAssetType.Model)
        #endif
        {
            return;
        }
#endif
        if (string.IsNullOrEmpty(text))
            return;

        if (string.IsNullOrEmpty(m_OrgText) && !string.IsNullOrEmpty(text))
            m_OrgText = text;

        m_OutputText = GetOutputText(m_OrgText);
        m_ImagesVertexIndex.Clear();
        listIconName.Clear();
        foreach (Match match in s_Regex.Matches(m_OutputText))
        {
            var picIndex = match.Index;
            var endIndex = picIndex * 4 + 3;
            m_ImagesVertexIndex.Add(endIndex);

            if (m_ImagesPool.Count == 0)
            {
                GetComponentsInChildren<Image>(m_ImagesPool);
            }
            if (m_ImagesVertexIndex.Count > m_ImagesPool.Count)
            {
                GameObject go = GameObject.Instantiate(orgChatExpress);
                go.GetComponent<Image>().raycastTarget = false;
                go.layer = gameObject.layer;
                var rt = go.transform as RectTransform;
                if (rt)
                {
                    rt.SetParent(transform);
                    rt.localPosition = Vector3.zero;
                    rt.localRotation = Quaternion.identity;
                    rt.localScale = Vector3.one;
                }
                m_ImagesPool.Add(go.GetComponent<Image>());
            }

            var spriteName = match.Groups[1].Value;
            var sizex = float.Parse(match.Groups[2].Value);
            var sizey = float.Parse(match.Groups[3].Value);
            var img = m_ImagesPool[m_ImagesVertexIndex.Count - 1];
            listIconName.Add(spriteName);
            img.rectTransform.sizeDelta = new Vector2(sizex, sizey);
        }

        for (var i = m_ImagesVertexIndex.Count; i < m_ImagesPool.Count; i++)
        {
            if (m_ImagesPool[i])
            {
                m_ImagesPool[i].enabled = false;
            }
        }
    }

    private readonly Vector2 pivot1= new Vector2(0, 1);
    private readonly Vector2 pivot2 = new Vector2(1, 1);
    private readonly Vector2 pivot3 = new Vector2(0.5f, 1.0f);
    private readonly Vector2 pivot4 = new Vector2(0.5f, 0.5f);
    private readonly Vector2 pivot5 = new Vector2(0.0f, 0.0f);
    private readonly Vector2 pivot6 = new Vector2(1.0f, 0.0f);

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        m_Text = m_OutputText;
        base.OnPopulateMesh(toFill);

        UIVertex vert = new UIVertex();
        for (var i = 0; i < m_ImagesVertexIndex.Count; i++)
        {
            var endIndex = m_ImagesVertexIndex[i];
            var rt = m_ImagesPool[i].rectTransform;
            var size = rt.sizeDelta;
            if (endIndex < toFill.currentVertCount)
            {
                toFill.PopulateUIVertex(ref vert, endIndex);
                if (rectTransform.pivot.Equals(pivot1))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2 - rectTransform.sizeDelta.x / 2, vert.position.y + size.y * 0.28f + rectTransform.sizeDelta.y / 2);
                else if (rectTransform.pivot.Equals(pivot2))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2 + rectTransform.sizeDelta.x / 2, vert.position.y + size.y * 0.28f + rectTransform.sizeDelta.y / 2);
                else if (rectTransform.pivot.Equals(pivot3))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2 + rectTransform.sizeDelta.y / 2 - 2);
                else if (rectTransform.pivot.Equals(pivot4))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2, vert.position.y + size.y / 2);
                else if (rectTransform.pivot.Equals(pivot5))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2 - rectTransform.sizeDelta.x / 2, vert.position.y - rectTransform.sizeDelta.y / 2 + 0.3f * size.y);
                else if (rectTransform.pivot.Equals(pivot6))
                    rt.anchoredPosition = new Vector2(vert.position.x + size.x / 2 + rectTransform.sizeDelta.x / 2, vert.position.y - rectTransform.sizeDelta.y / 2 + 0.3f * size.y);

                toFill.PopulateUIVertex(ref vert, endIndex - 3);
                var pos = vert.position;
                for (int j = endIndex, m = endIndex - 3; j > m; j--)
                {
                    toFill.PopulateUIVertex(ref vert, endIndex);
                    vert.position = pos;
                    toFill.SetUIVertex(vert, j);
                }
            }
        }

        Vector2 extents = rectTransform.rect.size;
        var settings = GetGenerationSettings(extents);
        TextGenerator _UnderlineText = new TextGenerator();
        _UnderlineText.Populate("_", settings);
        IList<UIVertex> _TUT = _UnderlineText.verts;

        //处理超链接包围框
        var e = m_HrefInfos.GetEnumerator();
        while (e.MoveNext())
        {
            var hrefInfo = e.Current;
            hrefInfo.boxes.Clear();
            if (hrefInfo.startIndex >= toFill.currentVertCount)
            {
                continue;
            }

            // 将超链接里面的文本顶点索引坐标加入到包围框
            toFill.PopulateUIVertex(ref vert, hrefInfo.startIndex);
            var pos = vert.position;
            var bounds = new Bounds(pos, Vector3.zero);
            for (int i = hrefInfo.startIndex, m = hrefInfo.endIndex; i < m; i++)
            {
                if (i >= toFill.currentVertCount)
                {
                    break;
                }

                toFill.PopulateUIVertex(ref vert, i);
                pos = vert.position;
                if (pos.x < bounds.min.x) // 换行重新添加包围框
                {
                    hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));
                    bounds = new Bounds(pos, Vector3.zero);
                }
                else
                {
                    bounds.Encapsulate(pos); // 扩展包围框
                }
            }
            hrefInfo.boxes.Add(new Rect(bounds.min, bounds.size));

            for (int i = 0; i < hrefInfo.boxes.Count; i++)
            {
                Vector3 _StartBoxPos = new Vector3(hrefInfo.boxes[i].x, hrefInfo.boxes[i].y - underLineOffset, 0.0f);
                Vector3 _EndBoxPos = _StartBoxPos + new Vector3(hrefInfo.boxes[i].width, 0.0f, 0.0f);
                AddUnderlineQuad(toFill, _TUT, _StartBoxPos, _EndBoxPos, vert.color);
            }
        }
        bDirty = true;
    }

    private bool bDirty = false;
#if UNITY_EDITOR
    protected override void Update()
#else
    void Update()
#endif
    {
        if (bDirty && m_ImagesVertexIndex.Count == listIconName.Count)
        {
            for (int i = 0; i < m_ImagesVertexIndex.Count; i++)
            {
                m_ImagesPool[i].GetComponent<UIImage>().spriteName = listIconName[i];
                m_ImagesPool[i].GetComponent<UIImage>().enabled = true;
                m_ImagesPool[i].gameObject.SetActive(true);
            }
            bDirty = false;
        }
    }

    /// <summary>
    /// 超链接信息列表
    /// </summary>
    private readonly List<HrefInfo> m_HrefInfos = new List<HrefInfo>();

    /// <summary>
    /// 文本构造器
    /// </summary>
    private static readonly StringBuilder s_TextBuilder = new StringBuilder();

    /// <summary>
    /// 超链接正则
    /// </summary>
    private static readonly Regex s_HrefRegex =
        new Regex(@"<url=([\S\s]*)>(.*?)(</url>)", RegexOptions.Singleline);

    public class HrefClickEvent : UnityEvent<string>
    {
    }

    private HrefClickEvent m_OnHrefClick = new HrefClickEvent();

    /// <summary>
    /// 超链接点击事件
    /// </summary>
    public HrefClickEvent onHrefClick
    {
        get { return m_OnHrefClick; }
        set { m_OnHrefClick = value; }
    }

    /// <summary>
    /// 获取超链接解析后的最后输出文本
    /// </summary>
    /// <returns></returns>
    public string GetOutputText(string text)
    {
        s_TextBuilder.Length = 0;
        m_HrefInfos.Clear();
        var indexText = 0;
        foreach (Match match in s_HrefRegex.Matches(text))
        {
            s_TextBuilder.Append(text.Substring(indexText, match.Index - indexText));

            var group = match.Groups[1];
            var hrefInfo = new HrefInfo
            {
                startIndex = s_TextBuilder.Length*4, // 超链接里的文本起始顶点索引
                endIndex = (s_TextBuilder.Length + match.Groups[2].Length - 1)*4 + 3,
                name = group.Value
            };
            m_HrefInfos.Add(hrefInfo);

            s_TextBuilder.Append(match.Groups[2].Value);
            indexText = match.Index + match.Length;
        }
        s_TextBuilder.Append(text.Substring(indexText, text.Length - indexText));
        return s_TextBuilder.ToString();
    }

    private UIVertex[] m_TempVerts = new UIVertex[4];
    void AddUnderlineQuad(VertexHelper _VToFill, IList<UIVertex> _VTUT, Vector3 _VStartPos, Vector3 _VEndPos, Color urlColor)
    {
        Vector3[] _TUnderlinePos = new Vector3[4];
        _TUnderlinePos[0] = _VStartPos;
        _TUnderlinePos[1] = _VEndPos;
        _TUnderlinePos[2] = _VEndPos + new Vector3(0, fontSize * 0.2f, 0);
        _TUnderlinePos[3] = _VStartPos + new Vector3(0, fontSize * 0.2f, 0);

        for (int i = 0; i < 4; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = _VTUT[i % 4];
            m_TempVerts[tempVertsIndex].color = urlColor;

            m_TempVerts[tempVertsIndex].position = _TUnderlinePos[i];

            if (tempVertsIndex == 3)
                _VToFill.AddUIVertexQuad(m_TempVerts);
        }
    }

    /// <summary>
    /// 超链接信息类
    /// </summary>
    private class HrefInfo
    {
        public int startIndex;

        public int endIndex;

        public string name;

        public readonly List<Rect> boxes = new List<Rect>();
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
     
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        Vector2 lp;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out lp);

        var e = m_HrefInfos.GetEnumerator();
        while (e.MoveNext())
        {
            var item = e.Current;
            List<Rect> boxes = item.boxes;
            for (var i = 0; i < boxes.Count; ++i)
            {
                if (boxes[i].Contains(lp))
                {
                    m_OnHrefClick.Invoke(item.name);
                    return;
                }
            }
        }
    }

#region textPic

    /// <summary>
    /// 正则取出所需要的属性
    /// </summary>
    private static readonly Regex c_Regex =
        new Regex(@"<color=#\S{6,8}>", RegexOptions.Singleline);
    
    public static void GetPicTextContent(TextPic text, string content, float orgWidth, float orgHeight,  float expressSize, ref float fWidth, ref float fHeight, float offset = 0)
    {
        string tempStr = text.GetOutputText(content);
        int index = 0;
        int expressionCount = 0;
        string str = string.Empty;
        while (tempStr.IndexOf("<quad", index) != -1)
        {
            str += tempStr.Substring(index, tempStr.IndexOf("<quad", index) - index);
            index = tempStr.IndexOf("/>", index) + 2;
            expressionCount++;
        }
        str += tempStr.Substring(index, tempStr.Length - index);
        if (string.IsNullOrEmpty(str))
        {
            fWidth += expressionCount * (expressSize + 1.0f);
            fWidth += offset;
            fWidth += 1.0f;
            
            if (fWidth > orgWidth)
                fWidth = orgWidth;
            fHeight = Mathf.CeilToInt(fWidth / orgWidth) * orgHeight;
            return;
        }

        tempStr = str;
        tempStr = tempStr.Replace("</color>", string.Empty);
        foreach (Match match in c_Regex.Matches(str))
            tempStr = tempStr.Replace(match.Value, string.Empty);
        
        tempStr = tempStr.Replace("\u3000\u3000\u3000", string.Empty);

        text.font.RequestCharactersInTexture(tempStr, text.fontSize);
        CharacterInfo characterInfo = new CharacterInfo();
        for (int i = 0; i < tempStr.Length; i++)
        {
            text.font.GetCharacterInfo(tempStr[i], out characterInfo, text.fontSize);
            fWidth += characterInfo.advance;
        }

        fWidth += expressionCount * (expressSize + 1.0f);
        fWidth += offset;
        fWidth += 1.0f;

        fHeight = Mathf.CeilToInt(fWidth / orgWidth) * orgHeight;

        if (fWidth > orgWidth)
            fWidth = orgWidth;
    }

#endregion
}