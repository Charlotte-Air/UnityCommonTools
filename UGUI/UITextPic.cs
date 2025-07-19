using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UITextPic : MaskableGraphic
{
    [SerializeField] public UIFont m_font;

    public float textureWidth
    {
        get { return m_font.textureWidth; }
    }

    public float textureHeight
    {
        get { return m_font.textureHeight; }
    }

    public override Texture mainTexture
    {
        get
        {
            if (m_font.mainTexture == null)
                return s_WhiteTexture;
            return m_font.mainTexture;
        }
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
        if (m_toFill.currentVertCount >= 4)
        {
            int c = m_toFill.currentVertCount / 4;
            UIVertex vert = new UIVertex();
            for (int i = 0; i < c; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    m_toFill.PopulateUIVertex(ref vert, i * 4 + j);
                    toFill.AddVert(vert);
                }

                int num = i;
                toFill.AddTriangle(0 + 4 * num, 1 + 4 * num, 2 + 4 * num);
                toFill.AddTriangle(1 + 4 * num, 0 + 4 * num, 3 + 4 * num);
            }
        }
    }

    public void Clear()
    {
        m_toFill.Clear();
    }

    private readonly VertexHelper m_toFill = new VertexHelper();

    private void SetPicInfo(List<UITextPicInfo> picInfo)
    {
        workerMesh.Clear();
        if (picInfo.Count > 0)
        {
            m_toFill.Clear();
            UIVertex vert = new UIVertex();
            for (int i = 0; i < picInfo.Count; i++)
            {
                UITextPicInfo info = picInfo[i];
                if (!info.isDraw)
                {
                    break;
                }

                for (int j = 0; j < 4; j++)
                {
                    vert.position = info.vertices[j];
                    vert.color = color;
                    vert.uv0 = info.uv[j];
                    vert.uv1 = info.uv1[j];
                    m_toFill.AddVert(vert);
                }

                int num = i;
                m_toFill.AddTriangle(0 + 4 * num, 1 + 4 * num, 2 + 4 * num);
                m_toFill.AddTriangle(1 + 4 * num, 0 + 4 * num, 3 + 4 * num);
            }

            m_toFill.FillMesh(workerMesh);

        }

        canvasRenderer.SetMesh(workerMesh);
    }

    public Sprite GetSpriteByKey(string key)
    {
        UIFontData dt = m_font.getTextImageData(key);
        if (dt != null)
        {
            return dt.Image;
        }

        return null;
    }

    private void GetLineInfo(int c, IList<UILineInfo> lines, float unitsPerPixel, ref float topY, ref float lineHeight)
    {
        int found = 0;
        for (int i = 0; i < lines.Count; i++)
        {
            if (c == lines[i].startCharIdx)
            {
                //newline = true;
            }
            if (c < lines[i].startCharIdx)
            {
                break;
            }
            found = i;
        }
        topY = lines[found].topY * unitsPerPixel;
        lineHeight = lines[found].height * unitsPerPixel;
    }

    public void AddPicRects(VertexHelper toFill, List<UITextPicInfo> picInfo, IList<UILineInfo> lines, Vector2 roundingOffset, float unitsPerPixel, bool isEnabled)
    {
        float e = isEnabled ? 0 : 2;
        Vector3 txtPos = Vector3.zero;
        Vector2 texSize = new Vector2(textureWidth, textureHeight);
        UIVertex tempVertex = new UIVertex();

        for (int i = 0; i < picInfo.Count; i++)
        {
            UITextPicInfo info = picInfo[i];
            if (info.vertexIndex >= toFill.currentVertCount)
            {
                info.isDraw = false;
                break;
            }

            info.isDraw = true;

            for (int m = info.vertexIndex; m < info.vertexIndex + 4; m++)
            {
                toFill.PopulateUIVertex(ref tempVertex, m);
                tempVertex.uv0 = Vector2.zero;
                toFill.SetUIVertex(tempVertex, m);
            }

            toFill.PopulateUIVertex(ref tempVertex, info.vertexIndex);
            info.textpos = tempVertex.position;

            float y = 0;
            float h = 0;
            GetLineInfo(info.charIndex, lines, unitsPerPixel, ref y, ref h);

            float cw = info.size.x;
            float ch = Mathf.Min(h, info.size.y);

            txtPos.x = info.textpos.x;
            txtPos.y = y + roundingOffset.y;
            txtPos.z = 0;

            info.vertices[0].x = txtPos.x;
            info.vertices[0].y = txtPos.y - ch;
            info.vertices[0].z = txtPos.z;

            info.vertices[1].x = info.width * cw + txtPos.x;
            info.vertices[1].y = txtPos.y;
            info.vertices[1].z = txtPos.z;

            info.vertices[2].x = info.width * cw + txtPos.x;
            info.vertices[2].y = txtPos.y - ch;
            info.vertices[2].z = txtPos.z;

            info.vertices[3].x = txtPos.x;
            info.vertices[3].y = txtPos.y;
            info.vertices[3].z = txtPos.z;

            Sprite sp = GetSpriteByKey(info.id);
            Rect spriteRect = sp != null ? sp.rect : new Rect(0, 0, 0, 0);

            info.uv[0].x = spriteRect.x / texSize.x;
            info.uv[0].y = spriteRect.y / texSize.y;

            info.uv[1].x = (spriteRect.x + spriteRect.width) / texSize.x;
            info.uv[1].y = (spriteRect.y + spriteRect.height) / texSize.y;
            info.uv[2].x = (spriteRect.x + spriteRect.width) / texSize.x;
            info.uv[2].y = spriteRect.y / texSize.y;
            info.uv[3].x = spriteRect.x / texSize.x;
            info.uv[3].y = (spriteRect.y + spriteRect.height) / texSize.y;

            info.uv1[0].x = info.uv[0].x + e;
            info.uv1[0].y = info.uv[0].y;
            info.uv1[1].x = info.uv[1].x + e;
            info.uv1[1].y = info.uv[1].y;
            info.uv1[2].x = info.uv[2].x + e;
            info.uv1[2].y = info.uv[2].y;
            info.uv1[3].x = info.uv[3].x + e;
            info.uv1[3].y = info.uv[3].y;
        }

        SetPicInfo(picInfo);
    }
}

