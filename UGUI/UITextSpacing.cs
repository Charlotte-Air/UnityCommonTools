using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Charlotte.Dll
{
    [AddComponentMenu("UI/Effects/TextSpacing")]
    public class TextSpacing : BaseMeshEffect
    {
        public float Spacing = 1f;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0)
                return;
            Text component = GetComponent<Text>();
            if ((Object)component == (Object)null)
            {
                Debug.LogError("Missing Text component");
            }
            else
            {
                HorizontalAligmentType horizontalAligmentType =
                    component.alignment != TextAnchor.LowerLeft && component.alignment != TextAnchor.MiddleLeft &&
                    component.alignment != TextAnchor.UpperLeft
                        ? (component.alignment != TextAnchor.LowerCenter &&
                           component.alignment != TextAnchor.MiddleCenter &&
                           component.alignment != TextAnchor.UpperCenter
                            ? HorizontalAligmentType.Right
                            : HorizontalAligmentType.Center)
                        : HorizontalAligmentType.Left;
                List<UIVertex> stream = new List<UIVertex>();
                vh.GetUIVertexStream(stream);
                string[] strArray = component.text.Split('\n');
                Line[] lineArray = new Line[strArray.Length];
                for (int index = 0; index < lineArray.Length; ++index)
                {
                    string str = Regex.Replace(Regex.Replace(strArray[index], GlobalRegex.m_MatchColorTagHead, ""),
                        GlobalRegex.m_MatchColorTagEnd, "");
                    if (index == 0)
                    {
                        lineArray[index] = new Line(0, str.Length + 1);
                    }
                    else
                    {
                        int num = index <= 0 ? 0 : (index < lineArray.Length - 1 ? 1 : 0);
                        lineArray[index] = num == 0
                            ? new Line(lineArray[index - 1].EndVertexIndex + 1, str.Length)
                            : new Line(lineArray[index - 1].EndVertexIndex + 1, str.Length + 1);
                    }
                }

                for (int index = 0; index < lineArray.Length; ++index)
                {
                    for (int startVertexIndex = lineArray[index].StartVertexIndex;
                         startVertexIndex <= lineArray[index].EndVertexIndex;
                         ++startVertexIndex)
                    {
                        if (startVertexIndex >= 0 && startVertexIndex < stream.Count)
                        {
                            UIVertex vertex = stream[startVertexIndex];
                            int num1 = lineArray[index].EndVertexIndex - lineArray[index].StartVertexIndex;
                            if (index == lineArray.Length - 1)
                                num1 += 6;
                            switch (horizontalAligmentType)
                            {
                                case HorizontalAligmentType.Left:
                                    vertex.position +=
                                        new Vector3(
                                            Spacing *
                                            (float)((startVertexIndex - lineArray[index].StartVertexIndex) / 6), 0.0f,
                                            0.0f);
                                    break;
                                case HorizontalAligmentType.Center:
                                    float num2 = num1 / 6 % 2 == 0 ? 0.5f : 0.0f;
                                    vertex.position +=
                                        new Vector3(
                                            Spacing *
                                            ((float)((startVertexIndex - lineArray[index].StartVertexIndex) / 6 -
                                                     num1 / 12) + num2), 0.0f, 0.0f);
                                    break;
                                case HorizontalAligmentType.Right:
                                    vertex.position +=
                                        new Vector3(
                                            Spacing *
                                            (float)(-(num1 - startVertexIndex + lineArray[index].StartVertexIndex) / 6 +
                                                    1), 0.0f, 0.0f);
                                    break;
                            }

                            stream[startVertexIndex] = vertex;
                            if (startVertexIndex % 6 <= 2)
                                vh.SetUIVertex(vertex, startVertexIndex / 6 * 4 + startVertexIndex % 6);
                            if (startVertexIndex % 6 == 4)
                                vh.SetUIVertex(vertex, startVertexIndex / 6 * 4 + startVertexIndex % 6 - 1);
                        }
                    }
                }
            }
        }

        public enum HorizontalAligmentType
        {
            Left,
            Center,
            Right,
        }

        public class Line
        {
            private int _startVertexIndex = 0;
            private int _endVertexIndex = 0;
            private int _vertexCount = 0;

            public int StartVertexIndex => _startVertexIndex;

            public int EndVertexIndex => _endVertexIndex;

            public int VertexCount => _vertexCount;

            public Line(int startVertexIndex, int length)
            {
                _startVertexIndex = startVertexIndex;
                _endVertexIndex = length * 6 - 1 + startVertexIndex;
                _vertexCount = length * 6;
            }
        }
    }
    
    
      public class GlobalRegex
      {
        private const string strAudio = "\\{Audio=.*\\}";
        private const string strAllMatchs = "\\[(\\d)#(.+?)(:{1}(.+?))?\\]|\\{Audio=.*\\}|\\[url=[^\\]]*\\].*?\\[/url\\]";
        public const string strExpression = "\\[(\\d)#(.+?)(:{1}(.+?))?\\]";
        public const string strUrl = "\\[url=[^\\]]*\\].*?\\[/url\\]";
        public const string strUrlShell = "(?<=\\[url=[^\\]]*\\]\\[u\\](\\[c\\])*(\\[[0-9,a-f,A-F]{6}\\])+).*?(?=(\\[-\\])+(\\[\\/c\\])*\\[/u\\]\\[/url\\])";
        public static Regex face_matchs = new Regex("\\[(\\d)#(.+?)(:{1}(.+?))?\\]");
        public static Regex audio_matchs = new Regex("\\{Audio=.*\\}");
        public static Regex url_matchs = new Regex("\\[url=[^\\]]*\\].*?\\[/url\\]");
        public static Regex urlShell_matchs = new Regex("(?<=\\[url=[^\\]]*\\]\\[u\\](\\[c\\])*(\\[[0-9,a-f,A-F]{6}\\])+).*?(?=(\\[-\\])+(\\[\\/c\\])*\\[/u\\]\\[/url\\])");
        public static Regex all_matchs = new Regex("\\[(\\d)#(.+?)(:{1}(.+?))?\\]|\\{Audio=.*\\}|\\[url=[^\\]]*\\].*?\\[/url\\]");
        public static Regex NotFinishedColorPartten = new Regex("\\[[0-9,a-f,A-F]{6}\\]((?!\\[[0-9,a-f,A-F]{6}\\]).)*?\\[-\\]");
        public static Regex NumRegex = new Regex("^[0-9]*$");
        public static Regex NiceNameRegex = new Regex("^[\\u4E00-\\u9FA5A-Za-z0-9]+$");
        public const string FormtStringRegexPattern = "(#i)(\\[)(\\d+)(\\])";
        public static Regex FormtStringByMatch = new Regex("(#i)(\\[)(\\d+)(\\])", RegexOptions.IgnoreCase);
        private const string strReplaceItemPattern = "\\d+";
        public static Regex ReplaceItemRegex = new Regex("\\d+");
        public static readonly string m_MatchParenthesisContent = "[\\(].*[?=\\)]";
        public static readonly string m_MatchRidParenthesisHeadEnd = "[^(].*[^)$]";
        public static readonly string m_MatchTagFormat = "\\[(Inlet:|-)(.*)\\:]";
        public static readonly string m_MatchTagHeadEnd = "\\[Inlet:|\\[-|:\\]";
        public static readonly string m_MatchColorTagHead = "<color=#[0-9a-fA-F]{6}>";
        public static readonly string m_MatchColorTagEnd = "</color>";
        public static readonly string m_MatchUrlFormat = "\\[url=[^\\]]*\\]";
        public static readonly string m_MatchInputFormat = "(\\[[\\[#][^\\[\\]#][^\\]]*\\]|[@][^@][^\\s]*\\s)";
        public static readonly Regex _inputTagRegex = new Regex("\\[(\\-{0,1}\\d)#(.+?)(:{1}(.+?))?\\]", RegexOptions.Singleline);
        public static readonly string MatchRetractFormat = "\\[retract=[^\\]]*\\]";
        public static readonly string strNumber = "[1-9]\\d*";
        public static readonly Regex m_MatchNumber = new Regex(strNumber);
        public static readonly string m_MatchRichBlodHead = "<b>";
        public static readonly string m_MatchRichBlodEnd = "</b>";

        public static string RemoveRichText(string strContent)
        {
          return Regex.Replace(Regex.Replace(Regex.Replace(Regex.Replace(strContent, m_MatchColorTagHead, ""), m_MatchColorTagEnd, ""), m_MatchRichBlodHead, ""), m_MatchRichBlodEnd, "");
        }
      }
}