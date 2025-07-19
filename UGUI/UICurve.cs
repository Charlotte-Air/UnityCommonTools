using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class UICurve : Image
{
    public enum CurveType
    {
        Bezier,
        CatmullRom,
    }

    public enum FillType
    {
        Simple,
        Repeat,
        Sliced,
    }


    public bool isEnabled
    {
        get
        {
            return m_isEnabled;
        }
        set
        {
            if (m_isEnabled != value)
            {
                m_isEnabled = value;
                SetVerticesDirty();
            }
        }
    }

    public string spriteName
    {
        get
        {
            return m_spriteName;
        }
        set
        {
            if (string.IsNullOrEmpty(value))
                return;

            m_spriteName = value;

            /*
            UISpriteManager.GetSprite(spriteName, (si) =>
            {
                try
                {
                    this.sprite = si.sprite;
                    this.material = si.mat;
                }
                catch
                { }
            });
            */
        }
    }

    public bool noGraphics
    {
        set
        {
            m_noGraphics = value;
            SetVerticesDirty();
        }
        get
        {
            return m_noGraphics;
        }
    }

    public Vector3[] controlPoints
    {
        get
        {
            return m_controlPoints;
        }
    }


    [SerializeField]
    private string m_spriteName;

    [SerializeField]
    private bool m_isEnabled = true;

    [SerializeField]
    private bool m_noGraphics = false;

    [SerializeField]
    private Vector3[] m_controlPoints = null;

    [SerializeField]
    private int m_lineSmooth = 6;

    [SerializeField]
    private float m_lineThickness = 20;

    [SerializeField]
    private CurveType m_curveType = CurveType.Bezier;

    [SerializeField]
    private FillType m_fillType = FillType.Sliced;

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (material == null || material.name == "Default UI Material")
        {
            material = GetMaterial();
        }
        if (canvas != null)
        {
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
        }
#endif
    }

#if UNITY_EDITOR
    public Material GetMaterial()
    {
        Material mat = null;
        string spath = AssetDatabase.GetAssetPath(sprite);
        if (spath != "")
        {
            string[] p = spath.Split('.');
            string mpath = p[0] + ".mat";
            p = p[0].Split('/');
            m_spriteName = p[p.Length - 1];
            mat = AssetDatabase.LoadAssetAtPath<Material>(mpath);
            if (mat == null)
            {
                LogHelper.WarningFormat(gameObject, "Can't find material in {0}", mpath);
            }
        }
        return mat;
    }
#endif

    public override void OnBeforeSerialize()
    {
        base.OnBeforeSerialize();
        if (string.IsNullOrEmpty(m_spriteName))
        {
            m_spriteName = sprite != null ? sprite.name : ""; ;
        }
        else
        {
            if (sprite != null && m_spriteName != sprite.name)
            {
                m_spriteName = sprite.name;
            }
        }
    }

    protected override void UpdateMaterial()
    {
        if (!IsActive())
        {
            return;
        }

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
        canvasRenderer.SetTexture(mainTexture);
    }

    private static Vector2[] s_uvs = new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    private static Vector2[] s_startUvs = new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    private static Vector2[] s_middleUvs = new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    private static Vector2[] s_endUvs = new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    private static Vector2[] s_repeatUvs = new[] { Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero };
    private static Vector2 s_uv1Adj = Vector2.zero;

    private void GenerateUvs()
    {
        Vector4 outer = Vector4.zero;
        Vector4 inner = Vector4.zero;

        if (sprite != null)
        {
            outer = UnityEngine.Sprites.DataUtility.GetOuterUV(sprite);
            inner = UnityEngine.Sprites.DataUtility.GetInnerUV(sprite);
        }

        s_uvs[0] = new Vector2(outer.x, outer.y);
        s_uvs[1] = new Vector2(inner.x, inner.y);
        s_uvs[2] = new Vector2(inner.z, inner.w);
        s_uvs[3] = new Vector2(outer.z, outer.w);

        s_startUvs[0] = new Vector2(s_uvs[0].x, s_uvs[0].y);
        s_startUvs[1] = new Vector2(s_uvs[0].x, s_uvs[3].y);
        s_startUvs[2] = new Vector2(s_uvs[1].x, s_uvs[3].y);
        s_startUvs[3] = new Vector2(s_uvs[1].x, s_uvs[0].y);

        s_middleUvs[0] = new Vector2(s_uvs[1].x, s_uvs[0].y);
        s_middleUvs[1] = new Vector2(s_uvs[1].x, s_uvs[3].y);
        s_middleUvs[2] = new Vector2(s_uvs[1].x, s_uvs[3].y);
        s_middleUvs[3] = new Vector2(s_uvs[1].x, s_uvs[0].y);

        s_endUvs[0] = new Vector2(s_uvs[2].x, s_uvs[0].y);
        s_endUvs[1] = new Vector2(s_uvs[2].x, s_uvs[3].y);
        s_endUvs[2] = new Vector2(s_uvs[3].x, s_uvs[3].y);
        s_endUvs[3] = new Vector2(s_uvs[3].x, s_uvs[0].y);

        s_repeatUvs[0] = new Vector2(s_uvs[0].x, s_uvs[0].y);
        s_repeatUvs[1] = new Vector2(s_uvs[0].x, s_uvs[3].y);
        s_repeatUvs[2] = new Vector2(s_uvs[3].x, s_uvs[3].y);
        s_repeatUvs[3] = new Vector2(s_uvs[3].x, s_uvs[0].y);

        s_uv1Adj.x = m_isEnabled ? 0 : 2;
        s_uv1Adj.y = 2;// !m_noAlphaTex ? 0 : 2; 
    }

    public IEnumerable<Vector3> GetPoints()
    {
        if (m_curveType == CurveType.Bezier)
        {
            return Interpolate.NewBezier(Interpolate.Ease(Interpolate.EaseType.Linear), m_controlPoints, m_lineSmooth*2);
        }
        else
        {
            return Interpolate.NewCatmullRom(m_controlPoints, m_lineSmooth, false, 1);
        }
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (m_noGraphics || m_controlPoints == null || m_controlPoints.Length <= 1)
        {
            return;
        }

        if (!hasBorder && m_fillType == FillType.Sliced)
            m_fillType = FillType.Simple;

        GenerateUvs();

        m_segmentCount = 0;

        Vector3[] points = GetPoints().ToArray();

        for (int i = 1; i < points.Length; i++)
        {
            Vector3 s0 = points[i - 1];
            Vector3 s1 = points[i];
            if (i == 1)
            {
                CreateLineSegment(s0, s1, 0);
                AddSegment(vh);
            }
            else if (i == points.Length - 1)
            {
                CreateLineSegment(s0, s1, 2);
                AddSegment(vh, true);
            }
            else
            {
                CreateLineSegment(s0, s1, 1);
                AddSegment(vh);
            }
        }

        if (m_fillType == FillType.Simple)
        {
            ApplyFillTypeSimple(vh);
        }
    }
    
    private void ApplyFillTypeSimple(VertexHelper vh)
    {
        int vCount = vh.currentVertCount;
        if (vCount <= 4)
            return;

        float f0 = 0;
        float f1 = 0;

        UIVertex v0 = UIVertex.simpleVert;
        UIVertex v1 = UIVertex.simpleVert;
        UIVertex v2 = UIVertex.simpleVert;
        UIVertex v3 = UIVertex.simpleVert;

        for (int i = 0; i < vCount; i+= 4)
        {
            vh.PopulateUIVertex(ref v0, i);
            vh.PopulateUIVertex(ref v1, i + 1);
            vh.PopulateUIVertex(ref v2, i + 2);
            vh.PopulateUIVertex(ref v3, i + 3);

            v0.uv0.x = f0;
            v1.uv0.x = f1;

            f0 += Vector3.Distance(v0.position, v3.position);
            f1 += Vector3.Distance(v1.position, v2.position);
            
            v2.uv0.x = f1;
            v3.uv0.x = f0;

            vh.SetUIVertex(v0, i);
            vh.SetUIVertex(v1, i + 1);
            vh.SetUIVertex(v2, i + 2);
            vh.SetUIVertex(v3, i + 3);
        }


        float dx0 = (s_uvs[3].x - s_uvs[0].x) / f0;
        float dx1 = (s_uvs[3].x - s_uvs[0].x) / f1;

        for (int i = 0; i < vCount; i += 4)
        {
            vh.PopulateUIVertex(ref v0, i);
            vh.PopulateUIVertex(ref v1, i + 1);
            vh.PopulateUIVertex(ref v2, i + 2);
            vh.PopulateUIVertex(ref v3, i + 3);

            v0.uv0.x = s_uvs[0].x + dx0 * v0.uv0.x;
            v0.uv0.y = s_uvs[0].y;
            v0.uv1.x = v0.uv0.x + s_uv1Adj.x;
            v0.uv1.y = v0.uv0.y + s_uv1Adj.y;

            v1.uv0.x = s_uvs[0].x + dx1 * v1.uv0.x;
            v1.uv0.y = s_uvs[3].y;
            v1.uv1.x = v1.uv0.x + s_uv1Adj.x;
            v1.uv1.y = v1.uv0.y + s_uv1Adj.y;

            v2.uv0.x = s_uvs[0].x + dx1 * v2.uv0.x;
            v2.uv0.y = s_uvs[3].y;
            v2.uv1.x = v2.uv0.x + s_uv1Adj.x;
            v2.uv1.y = v2.uv0.y + s_uv1Adj.y;

            v3.uv0.x = s_uvs[0].x + dx0 * v3.uv0.x;
            v3.uv0.y = s_uvs[0].y;
            v3.uv1.x = v3.uv0.x + s_uv1Adj.x;
            v3.uv1.y = v3.uv0.y + s_uv1Adj.y;

            vh.SetUIVertex(v0, i);
            vh.SetUIVertex(v1, i + 1);
            vh.SetUIVertex(v2, i + 2);
            vh.SetUIVertex(v3, i + 3);
        }
    }
    
    private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;

    private int m_segmentCount = 0;
    private static UIVertex[] s_segmentJoinVerts = new[] { UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert };
    private static UIVertex[] s_segmentLastVerts = new[] { UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert };
    private static UIVertex[] s_segmentVerts = new[] { UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert, UIVertex.simpleVert };

    private void AddSegment(VertexHelper vh, bool isLast = false)
    {
        if (m_segmentCount >= 1)
        {
            if (m_fillType == FillType.Repeat)
            {
                vh.AddUIVertexQuad(s_segmentLastVerts);
            }
            else
            {
                var vec1 = s_segmentLastVerts[1].position - s_segmentLastVerts[2].position;
                var vec2 = s_segmentVerts[2].position - s_segmentVerts[1].position;
                var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                var miterDistance = m_lineThickness / (2 * Mathf.Tan(angle / 2));
                var miterPointA = s_segmentLastVerts[2].position - vec1.normalized * miterDistance * sign;
                var miterPointB = s_segmentLastVerts[3].position + vec1.normalized * miterDistance * sign;

                if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                {
                    if (sign < 0)
                    {
                        s_segmentLastVerts[2].position = miterPointA;
                        s_segmentVerts[1].position = miterPointA;
                    }
                    else
                    {
                        s_segmentLastVerts[3].position = miterPointB;
                        s_segmentVerts[0].position = miterPointB;
                    }
                }

                s_segmentJoinVerts[0] = s_segmentLastVerts[3];
                s_segmentJoinVerts[1] = s_segmentLastVerts[2];
                s_segmentJoinVerts[2] = s_segmentVerts[1];
                s_segmentJoinVerts[3] = s_segmentVerts[0];

                vh.AddUIVertexQuad(s_segmentLastVerts);
                vh.AddUIVertexQuad(s_segmentJoinVerts);
            }            
        }
        
        s_segmentLastVerts[0] = s_segmentVerts[0];
        s_segmentLastVerts[1] = s_segmentVerts[1];
        s_segmentLastVerts[2] = s_segmentVerts[2];
        s_segmentLastVerts[3] = s_segmentVerts[3];

        if (isLast)
        {
            vh.AddUIVertexQuad(s_segmentLastVerts);
        }

        m_segmentCount++;
    }

    private void CreateLineSegment(Vector2 start, Vector2 end, int type)
    {
        var uvs = s_repeatUvs;
        if (m_fillType != FillType.Repeat)
        {
            uvs = s_middleUvs;
            if (type == 0)
                uvs = s_startUvs;
            else if (type == 2)
                uvs = s_endUvs;
        }

        Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * m_lineThickness / 2;
        var v0 = start - offset;
        var v1 = start + offset;
        var v2 = end + offset;
        var v3 = end - offset;

        s_segmentVerts[0].color = color;
        s_segmentVerts[0].position = v0;
        s_segmentVerts[0].uv0 = uvs[0];
        s_segmentVerts[0].uv1 = uvs[0] + s_uv1Adj;

        s_segmentVerts[1].color = color;
        s_segmentVerts[1].position = v1;
        s_segmentVerts[1].uv0 = uvs[1];
        s_segmentVerts[1].uv1 = uvs[1] + s_uv1Adj;

        s_segmentVerts[2].color = color;
        s_segmentVerts[2].position = v2;
        s_segmentVerts[2].uv0 = uvs[2];
        s_segmentVerts[2].uv1 = uvs[2] + s_uv1Adj;

        s_segmentVerts[3].color = color;
        s_segmentVerts[3].position = v3;
        s_segmentVerts[3].uv0 = uvs[3];
        s_segmentVerts[3].uv1 = uvs[3] + s_uv1Adj;
    }
}


