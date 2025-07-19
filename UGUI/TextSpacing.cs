using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class Line
{

    private int _startVertexIndex = 0;
    /// <summary>
    /// 起点索引
    /// </summary>
    public int StartVertexIndex
    {
        get
        {
            return _startVertexIndex;
        }
    }

    private int _endVertexIndex = 0;
    /// <summary>
    /// 终点索引
    /// </summary>
    public int EndVertexIndex
    {
        get
        {
            return _endVertexIndex;
        }
    }

    private int _vertexCount = 0;
    /// <summary>
    /// 该行占的点数目
    /// </summary>
    public int VertexCount
    {
        get
        {
            return _vertexCount;
        }
    }

    public Line(int startVertexIndex, int length)
    {
        _startVertexIndex = startVertexIndex;
        _endVertexIndex = length * 6 - 1 + startVertexIndex;
        _vertexCount = length * 6;
    }
}


[AddComponentMenu("UI/Effects/TextSpacing")]
public class TextSpacing : BaseMeshEffect
{
    public float _textSpacing = 1f;

    private Text text = null;

    protected override void Awake()
    {
        text = GetComponent<Text>();
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive() || vh.currentVertCount == 0)
        {
            return;
        }

        if (text == null)
        {
            return;
        }

        AdjustLetterSpacing(text, vh);

//         List<UIVertex> vertexs = new List<UIVertex>();
//         vh.GetUIVertexStream(vertexs);
//         int indexCount = vh.currentIndexCount;
// 
//         string[] lineTexts = text.text.Split('\n');
// 
//         Line[] lines = new Line[lineTexts.Length];
// 
//         //根据lines数组中各个元素的长度计算每一行中第一个点的索引，每个字、字母、空母均占6个点
//         for (int i = 0; i < lines.Length; i++)
//         {
//             //除最后一行外，vertexs对于前面几行都有回车符占了6个点
//             if (i == 0)
//             {
//                 lines[i] = new Line(0, lineTexts[i].Length + 1);
//             }
//             else if (i > 0 && i < lines.Length - 1)
//             {
//                 lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length + 1);
//             }
//             else
//             {
//                 lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
//             }
//         }
// 
//         UIVertex vt;
// 
//         for (int i = 0; i < lines.Length; i++)
//         {
//             for (int j = lines[i].StartVertexIndex + 6; j <= lines[i].EndVertexIndex; j++)
//             {
//                 if (j < 0 || j >= vertexs.Count)
//                 {
//                     continue;
//                 }
//                 vt = vertexs[j];
//                 vt.position += new Vector3(_textSpacing * ((j - lines[i].StartVertexIndex) / 6), 0, 0);
//                 vertexs[j] = vt;
//                 //以下注意点与索引的对应关系
//                 if (j % 6 <= 2)
//                 {
//                     vh.SetUIVertex(vt, (j / 6) * 4 + j % 6);
//                 }
//                 if (j % 6 == 4)
//                 {
//                     vh.SetUIVertex(vt, (j / 6) * 4 + j % 6 - 1);
//                 }
//             }
//         }
    }

    private static UIVertex s_tempVertex1 = new UIVertex();
    private static UIVertex s_tempVertex2 = new UIVertex();

    protected void AdjustLetterSpacing(Text text, VertexHelper vh)
    {
        float alignmentFactor = 0;

        switch (text.alignment)
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

        float letterOffset = _textSpacing;
        float lastx = float.MinValue;
        float letterSpaceIncr = 0;

        int curLineStart = 0;
        int charIdx = 0;
        int curLineCharCount = 0;

        Vector3 pos;

        for (charIdx = 0; charIdx < text.text.Length; charIdx++)
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

    private static UIVertex s_tempVertex = new UIVertex();

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
}