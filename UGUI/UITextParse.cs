using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using System.Text.RegularExpressions;


public class UITextPicInfo
{
    public string id = "";
    public Vector2 size = Vector2.zero;
    public float width;
    public float scale = 1;
    public int charIndex;
    public int vertexIndex;
    public Vector2 textpos = Vector2.zero;
    public Vector3[] vertices = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
    public Vector2[] uv = new Vector2[4] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    public Vector2[] uv1 = new Vector2[4] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    public int[] triangles;
    public bool isDraw = false;
}

public class UITextRichtextInfo
{
    public int startChar;
    public int charLen;
    public int startIndex;
    public int endIndex;
}

public class UITextUrlInfo : UITextRichtextInfo
{
    public string url;
    public List<Rect> rects = new List<Rect>();

    public void UpdateRects(VertexHelper toFill)
    {
        rects.Clear();

        if (startIndex >= toFill.currentVertCount)
        {
            return;
        }

        UIVertex vert = new UIVertex();
        toFill.PopulateUIVertex(ref vert, startIndex);

        Vector3 pos = vert.position;
        Bounds bounds = new Bounds(pos, Vector3.zero);
        for (int i = startIndex, m = endIndex; i < m; i++)
        {
            if (i >= toFill.currentVertCount)
            {
                break;
            }

            toFill.PopulateUIVertex(ref vert, i);
            pos = vert.position;
            if (pos.x < bounds.min.x)
            {
                rects.Add(new Rect(bounds.min, bounds.size));
                bounds = new Bounds(pos, Vector3.zero);
            }
            else
            {
                bounds.Encapsulate(pos);
            }
        }

        rects.Add(new Rect(bounds.min, bounds.size));
    }
}

public class UITextUnderlineInfo : UITextRichtextInfo
{
    private UIVertex[] m_tempVerts = new UIVertex[4];

    private float GetLineBaseY(int c, IList<UILineInfo> lines, ref bool newline)
    {
        int found = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (c == lines[i].startCharIdx)
            {
                newline = true;
            }

            if (c < lines[i].startCharIdx)
            {
                break;
            }

            found = i;
        }

        return lines[found].topY - lines[found].height;
    }

    public void AddQuad(string s, Color sColor, VertexHelper toFill, IList<UnityEngine.UICharInfo> chars,
        IList<UILineInfo> lines, IList<UIVertex> verts, Vector2 roundingOffset, float unitsPerPixel,
        float letterSpacing)
    {
        UIVertex vert = new UIVertex();
        Color prevColor = sColor;
        float prevX = float.MinValue;
        float charExt = Mathf.Max(letterSpacing / 10, 10);

        for (int i = 0; i < charLen; i++)
        {
            int charIndex = startChar + i;
            int vertexIndex = startIndex + i * 4;

            if (charIndex >= chars.Count || vertexIndex > toFill.currentVertCount - 4)
            {
                break;
            }

            UnityEngine.UICharInfo c = chars[charIndex];
            if (c.charWidth == 0)
            {
                continue;
            }

            bool usePrevColor = false;
            char curc = s[charIndex];
            if (curc == ' ') usePrevColor = true;

            bool newline = false;
            float lineBaseY = unitsPerPixel * (GetLineBaseY(charIndex, lines, ref newline) + roundingOffset.y);
            float lineHeight = unitsPerPixel * 2;
            if (newline) prevX = float.MinValue;

            toFill.PopulateUIVertex(ref vert, startIndex + i * 4);
            m_tempVerts[0] = vert;
            m_tempVerts[0].position =
                new Vector3(prevX > float.MinValue ? prevX : vert.position.x, lineBaseY, vert.position.z);
            m_tempVerts[0].uv0 = verts[0].uv0;

            toFill.PopulateUIVertex(ref vert, vertexIndex + 1);
            m_tempVerts[1] = vert;
            m_tempVerts[1].position = new Vector3(vert.position.x, lineBaseY, vert.position.z);
            m_tempVerts[1].uv0 = verts[1].uv0;

            toFill.PopulateUIVertex(ref vert, vertexIndex + 2);
            m_tempVerts[2] = vert;
            m_tempVerts[2].position = new Vector3(vert.position.x, lineBaseY - lineHeight, vert.position.z);
            m_tempVerts[2].uv0 = verts[2].uv0;

            toFill.PopulateUIVertex(ref vert, vertexIndex + 3);
            m_tempVerts[3] = vert;
            m_tempVerts[3].position = new Vector3(prevX > float.MinValue ? prevX : vert.position.x,
                lineBaseY - lineHeight, vert.position.z);
            m_tempVerts[3].uv0 = verts[3].uv0;

            if (usePrevColor)
            {
                m_tempVerts[0].color = prevColor;
                m_tempVerts[1].color = prevColor;
                m_tempVerts[2].color = prevColor;
                m_tempVerts[3].color = prevColor;
            }

            prevColor = vert.color;
            prevX = m_tempVerts[2].position.x - charExt;

            toFill.AddUIVertexQuad(m_tempVerts);
        }
    }
}

public class UITextGradientInfo : UITextRichtextInfo
{
    public bool IsFlow { get; set; }

    private List<Color32> colorList = new List<Color32>
    {
        new Color(255, 0, 0),
        new Color(255, 150, 0),
        new Color(255, 255, 0),
        new Color(0, 150, 255),
        new Color(0, 255, 50),
        new Color(120, 210, 255),
        new Color(255, 0, 255)
    };

    
    public void AddGradientColor(VertexHelper toFill)
    {
        if (startIndex >= toFill.currentVertCount)
        {
            return;
        }
        int e = endIndex;
        if (endIndex >= toFill.currentVertCount)
        {
            e = toFill.currentVertCount - 1;
        }
        UIVertex vert = new UIVertex();
        toFill.PopulateUIVertex(ref vert, startIndex);

        UIVertex endVert = new UIVertex();
        toFill.PopulateUIVertex(ref endVert, e - 1);

        float left_x = vert.position.x;
        float right_x = endVert.position.x;
        float uiElementWidth = Mathf.Max((right_x - left_x), 0.01f) / 6;
        float verX_1 = left_x + uiElementWidth;
        float verX_2 = left_x + uiElementWidth * 2;
        float verX_3 = left_x + uiElementWidth * 3;
        float verX_4 = left_x + uiElementWidth * 4;
        float verX_5 = left_x + uiElementWidth * 5;
        float verX_6 = right_x;
        float width = right_x - left_x;

        Color32 color = new Color32(0, 0, 0, 255);
        Color32 white = new Color32(255, 255, 255, 255);

        for (int i = startIndex, m = endIndex; i <= m; i++)
        {
            if (i >= toFill.currentVertCount)
            {
                break;
            }

            toFill.PopulateUIVertex(ref vert, i);

            if (!this.IsFlow)
            {
                if (vert.position.x < verX_1)
                {
                    color = Color32.Lerp(colorList[1], colorList[0], (verX_1 - vert.position.x) / uiElementWidth);
                }
                else if (vert.position.x < verX_2)
                {
                    color = Color32.Lerp(colorList[2], colorList[1], (verX_2 - vert.position.x) / uiElementWidth);
                }
                else if (vert.position.x < verX_3)
                {
                    color = Color32.Lerp(colorList[3], colorList[2], (verX_3 - vert.position.x) / uiElementWidth);
                }
                else if (vert.position.x < verX_4)
                {
                    color = Color32.Lerp(colorList[4], colorList[3], (verX_4 - vert.position.x) / uiElementWidth);
                }
                else if (vert.position.x < verX_5)
                {
                    color = Color32.Lerp(colorList[5], colorList[4], (verX_5 - vert.position.x) / uiElementWidth);
                }
                else if (vert.position.x <= verX_6)
                {
                    color = Color32.Lerp(colorList[6], colorList[5], (verX_6 - vert.position.x) / uiElementWidth);
                }
            }
            else
            {
                color = white;
            }
            
            vert.color = color;

            float du = Mathf.Clamp01((vert.position.x - left_x) / width);
            vert.uv1 = new Vector2(du, this.IsFlow?1:0);
            
            toFill.SetUIVertex(vert, i);
        }
    }

    private void SetColor(VertexHelper vh, UIVertex vertex, Color32 color, int index)
    {
        vertex.color = color;
        vh.SetUIVertex(vertex, index);
    }
}

public class UITextParse
{
    private static readonly Regex s_regexCustom = new Regex(
         @"<url=([^>\n\s]+)>(.*?)(</url>)|<u>(.*?)(</u>)|<g>(.*?)(</g>)|<fg>(.*?)(</fg>)|<color=([^>\n\s]+)>(.*?)(</color>)|\[#(.+?)\]", RegexOptions.Singleline);

    private static StringBuilder s_sb = new StringBuilder();
    private static StringBuilder s_sb1 = new StringBuilder();

    public static string Parse(UIText text)
    {
        s_sb.Clear();
        s_sb1.Clear();
        ParseCustom(text.text, text);
        text.charCount = s_sb1.Length;
        return s_sb.ToString();
    }

    public static void ParseCustom(string text, UIText uitext)
    {
        if (text == null || uitext == null)
            return;

        MatchCollection mc = s_regexCustom.Matches(text);

        if (mc.Count > 0)
        {
            string tmp;
            int txtIdx = 0;
            foreach (Match match in mc)
            {
                tmp = text.Substring(txtIdx, match.Index - txtIdx);
                s_sb.Append(tmp);
                s_sb1.Append(tmp.Replace("\n", ""));

                string cg = match.Groups[0].Value;
                if (cg.IndexOf("<url") == 0)
                {
                    int s = s_sb.Length;
                    int s1 = s_sb1.Length;
                    ParseCustom(match.Groups[2].Value, uitext);

                    UITextUrlInfo info = new UITextUrlInfo();
                    info.startChar = s;
                    info.charLen = s_sb.Length - s;
                    info.startIndex = s1 * 4;
                    info.endIndex = s_sb1.Length * 4 - 1;
                    info.url = match.Groups[1].Value;
                    uitext.m_urlInfos.Add(info);
                }
                else if (cg.IndexOf("<u") == 0)
                {
                    int s = s_sb.Length;
                    int s1 = s_sb1.Length;
                    ParseCustom(match.Groups[4].Value, uitext);

                    UITextUnderlineInfo info = new UITextUnderlineInfo();
                    info.startChar = s;
                    info.charLen = s_sb.Length - s;
                    info.startIndex = s1 * 4;
                    info.endIndex = s_sb1.Length * 4 - 1;
                    uitext.m_underlineInfos.Add(info);
                }
                else if (cg.IndexOf("<g") == 0)
                {
                    int s = s_sb.Length;
                    int s1 = s_sb1.Length;
                    ParseCustom(match.Groups[6].Value, uitext);

                    UITextGradientInfo info = new UITextGradientInfo();
                    info.startChar = s;
                    info.charLen = s_sb.Length - s;
                    info.startIndex = s1 * 4;
                    info.endIndex = s_sb1.Length * 4 - 1;
                    info.IsFlow = false;
                    uitext.m_gradientInfos.Add(info);
                }
                else if (cg.IndexOf("<fg") == 0)
                {
                    int s = s_sb.Length;
                    int s1 = s_sb1.Length;
                    ParseCustom(match.Groups[8].Value, uitext);

                    UITextGradientInfo info = new UITextGradientInfo();
                    info.startChar = s;
                    info.charLen = s_sb.Length - s;
                    info.startIndex = s1 * 4;
                    info.endIndex = s_sb1.Length * 4 - 1;
                    info.IsFlow = true;
                    uitext.m_gradientInfos.Add(info);
                }
                else if (cg.IndexOf("<color") == 0)
                {
                    s_sb.AppendFormat("<color={0}>", match.Groups[10].Value);

                    ParseCustom(match.Groups[11].Value, uitext);

                    s_sb.Append("</color>");
                }
                else if (cg.IndexOf("[#") == 0)
                {
                    if (uitext.m_font != null)
                    {
                        string sn = match.Groups[13].Value;

                        UIFontData dt = uitext.m_font.getTextImageData(sn);
                        if (dt != null)
                        {
                            float size = uitext.fontSize * uitext.picScale;
                            float width = dt.Image.rect.width / dt.Image.rect.height;

                            int s = s_sb.Length;
                            int s1 = s_sb1.Length;
                            s_sb.AppendFormat("<quad name={0} size={1} width={2}/>", sn, size, width);
                            s_sb1.Append("$");

                            UITextPicInfo finfo = new UITextPicInfo();
                            finfo.id = sn;
                            finfo.charIndex = s;
                            finfo.vertexIndex = s1 * 4;
                            finfo.size.x = size;
                            finfo.size.y = size;
                            finfo.width = width;
                            finfo.scale = uitext.picScale;
                            uitext.m_picInfos.Add(finfo);
                        }
                    }
                }
                else
                {
                    Debug.Assert(false, "uitext parse custom failed.");
                }

                txtIdx = match.Index + match.Length;
            }

            tmp = text.Substring(txtIdx, text.Length - txtIdx);
            s_sb.Append(tmp);
            s_sb1.Append(tmp.Replace("\n", ""));
        }
        else
        {
            s_sb.Append(text);
            s_sb1.Append(text.Replace("\n", ""));
        }
    }
}

