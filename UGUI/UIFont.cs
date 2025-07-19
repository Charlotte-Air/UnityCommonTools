using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIFontData
{
    public string Text;
    public Sprite Image;
}

public class UIFont : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] public UIFontData[] m_Data;
    private Dictionary<string, UIFontData> m_txtToFontData = new Dictionary<string, UIFontData>();

    [SerializeField]
    public Material m_matrial;

    public float textureWidth
    {
        get
        {
            if (m_Data == null || m_Data[0] == null) return 1;
            return m_Data[0].Image.texture.width;
        }
    }

    public float textureHeight
    {
        get
        {
            if (m_Data == null || m_Data[0] == null) return 1;
            return m_Data[0].Image.texture.height;
        }
    }

    public void OnAfterDeserialize()
    {
        m_txtToFontData.Clear();
        if (m_Data != null)
        {
            for (int i = 0; i < m_Data.Length; i++)
            {
                UIFontData dt = m_Data[i];
                m_txtToFontData[dt.Text] = dt;
            }
        }
    }

    public Texture2D mainTexture
    {
        get
        {
            if (m_Data == null || m_Data[0] == null) return null;
            return m_Data[0].Image.texture;
        }
    }

    public void OnBeforeSerialize()
    {
    }

    public void PopulateMesh(VertexHelper toFill, string text, float scale, Color32 color, RectTransform rect,
        TextGenerationSettings m_TextSettings) //,FontData fd
    {
        Vector2 txtSize = Vector2.zero;
        List<UIVertex> ls = Populate(text, color, scale, m_TextSettings.lineSpacing, ref txtSize);

        toFill.Clear();

        UpdateMesh(m_TextSettings, toFill, ls, txtSize, rect);
    }

    readonly List<UIVertex> ls = new List<UIVertex>();

    private List<UIVertex> Populate(string text, Color32 color, float scale, float lineSpacing, ref Vector2 txtSize)
    {
        float wd = textureWidth;
        float ht = textureHeight;
        ls.Clear();
        Vector4 tpos;
        Vector4 v;
        for (int i = 0; i < text.Length; i++)
        {
            UIFontData dt = getTextImageData(text[i].ToString());
            if (dt != null)
            {
                Rect rect = dt.Image.rect;
                v = new Vector4(rect.x, rect.y, rect.x + rect.width, rect.y + rect.height);

                var x = txtSize.x;
                tpos = new Vector4(x, 0, x + rect.width * scale, rect.height * scale);
                txtSize.x = txtSize.x + rect.width * scale + lineSpacing;

                if (txtSize.y < tpos.w) txtSize.y = tpos.w;
            }
            else
            {
                var x = txtSize.x;
                tpos = new Vector4(x, 0, x + 10 * scale, 1 * scale);
                txtSize.x = txtSize.x + 10;

                v = Vector4.zero;
            }

            ls.Add(CreateUIVertex(new Vector3(tpos.x, tpos.y), color, new Vector2(v.x / wd, v.y / ht)));
            ls.Add(CreateUIVertex(new Vector3(tpos.x, tpos.w), color, new Vector2(v.x / wd, v.w / ht)));
            ls.Add(CreateUIVertex(new Vector3(tpos.z, tpos.w), color, new Vector2(v.z / wd, v.w / ht)));
            ls.Add(CreateUIVertex(new Vector3(tpos.z, tpos.y), color, new Vector2(v.z / wd, v.y / ht)));
        }

        return ls;
    }

    private UIVertex CreateUIVertex(Vector3 pos, Color32 color, Vector2 uv)
    {
        var uiv0 = new UIVertex();
        uiv0.position = pos;
        uiv0.color = color;
        uiv0.uv0 = uv;
        return uiv0;
    }

    readonly UIVertex[] m_TempVerts = new UIVertex[4];

    private void UpdateMesh(TextGenerationSettings m_TextSettings, VertexHelper toFill, List<UIVertex> ls,
        Vector2 txtSize, RectTransform rect)
    {
        float width = rect.rect.size.x;
        float height = rect.rect.size.y;

        float currX = m_TextSettings.pivot.x * width;
        float currY = m_TextSettings.pivot.y * height;

        Vector3 offset = GetOffset(m_TextSettings, width, height, txtSize.x, txtSize.y, currX, currY);

        for (int i = 0; i < ls.Count; ++i)
        {
            int tempVertsIndex = i & 3;
            m_TempVerts[tempVertsIndex] = ls[i];
            m_TempVerts[tempVertsIndex].position += offset;
            if (tempVertsIndex == 3)
                toFill.AddUIVertexQuad(m_TempVerts);
        }

    }

    private Vector3 GetOffset(TextGenerationSettings m_TextSettings, float wd, float ht, float txtWd, float txtHt,
        float currX, float currY)
    {
        float tx = 0;
        float ty = 0;
        switch (m_TextSettings.textAnchor)
        {
            case TextAnchor.UpperLeft:
                tx = 0 - currX;
                ty = (ht - txtHt) - currY;
                break;
            case TextAnchor.UpperCenter:
                tx = (wd - txtWd) / 2 - currX;
                ty = (ht - txtHt) - currY;
                break;
            case TextAnchor.UpperRight:
                tx = (wd - txtWd) - currX;
                ty = (ht - txtHt) - currY;
                break;
            case TextAnchor.MiddleLeft:
                tx = 0 - currX;
                ty = (ht - txtHt) / 2 - currY;
                break;
            case TextAnchor.MiddleCenter:
                tx = (wd - txtWd) / 2 - currX;
                ty = (ht - txtHt) / 2 - currY;
                break;
            case TextAnchor.MiddleRight:
                tx = (wd - txtWd) - currX;
                ty = (ht - txtHt) / 2 - currY;
                break;
            case TextAnchor.LowerLeft:
                tx = 0 - currX;
                ty = 0 - currY;
                break;
            case TextAnchor.LowerCenter:
                tx = (wd - txtWd) / 2 - currX;
                ty = 0 - currY;
                break;
            case TextAnchor.LowerRight:
                tx = (wd - txtWd) - currX;
                ty = 0 - currY;
                break;
        }

        Vector3 offset = new Vector3(tx, ty, 0);
        return offset;
    }

    public UIFontData getTextImageData(string value)
    {
        if (m_txtToFontData.ContainsKey(value))
            return m_txtToFontData[value];
        return null;
    }
}

