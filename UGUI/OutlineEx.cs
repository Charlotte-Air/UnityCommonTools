using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


/// <summary>
/// 文本描边
/// </summary>
public class OutlineEx : BaseMeshEffect
{
    [Range(1, 2)] public int OutlineWidth = 1;

    public bool isOpenColor = false;
    public Color OutlineColor = Color.black;

    private static List<UIVertex> m_VetexList = new List<UIVertex>();

    [HideInInspector]
    public Shader shader;
    [HideInInspector]
    public Material material_x1;
    [HideInInspector]
    public Material material_x2;

    private static readonly int _OutlineColor = Shader.PropertyToID("_OutlineColor");
    private static readonly int _OutlineWidth = Shader.PropertyToID("_OutlineWidth");


    protected override void Start()
    {
        base.Start();

        Show();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        Show();
    }

    private void Show()
    {
        _RefreshModify();

        if (base.graphic.canvas != null)
        {
            var v1 = base.graphic.canvas.additionalShaderChannels;
            var v2 = AdditionalCanvasShaderChannels.TexCoord1;
            if ((v1 & v2) != v2)
            {
                base.graphic.canvas.additionalShaderChannels |= v2;
            }

            v2 = AdditionalCanvasShaderChannels.TexCoord2;
            if ((v1 & v2) != v2)
            {
                base.graphic.canvas.additionalShaderChannels |= v2;
            }
        }

        this._Refresh();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        base.graphic.material = null;
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (base.graphic.material != null)
        {
            this._RefreshModify();
            this._Refresh();
        }
    }
#endif

    private void _RefreshModify()
    {
        if (!isOpenColor)
        {
            if (OutlineWidth == 1)
            {
#if UNITY_EDITOR
                //var texMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Scripts/CHARLOTTE/OutlineMat_x1.mat");
                if (material_x1 != null)
                {
                    base.graphic.material = material_x1;
                }
                else
                {
                    Debug.LogError("没有找到材质OutlineMat_x1.mat");
                }
#else
            base.graphic.material = material_x1;
#endif
            }
            else if (OutlineWidth == 2)
            {
#if UNITY_EDITOR
                //var texMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Scripts/CHARLOTTE/OutlineMat_x2.mat");
                if (material_x2 != null)
                {
                    base.graphic.material = material_x2;
                }
                else
                {
                    Debug.LogError("没有找到材质OutlineMat_x2.mat");
                }
#else
            base.graphic.material = material_x2;
#endif
            }
        }
        else
        {
#if UNITY_EDITOR
            shader = Shader.Find("UI/OutlineEx");
#endif
            var material = new Material(shader);
            base.graphic.material = material;
            base.graphic.material.SetColor(_OutlineColor, this.OutlineColor);
            base.graphic.material.SetInt(_OutlineWidth, this.OutlineWidth);
        }
    }

    private void _Refresh()
    {
        base.graphic.SetVerticesDirty();
    }


    public override void ModifyMesh(VertexHelper vh)
    {
        vh.GetUIVertexStream(m_VetexList);

        this._ProcessVertices();

        vh.Clear();
        vh.AddUIVertexTriangleStream(m_VetexList);
    }


    private void _ProcessVertices()
    {
        for (int i = 0, count = m_VetexList.Count - 3; i <= count; i += 3)
        {
            var v1 = m_VetexList[i];
            var v2 = m_VetexList[i + 1];
            var v3 = m_VetexList[i + 2];
            // 计算原顶点坐标中心点
            //
            var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
            // 计算原始顶点坐标和UV的方向
            //
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right))
                > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right)))
            {
                triX = pos2 - pos1;
                triY = pos3 - pos2;
                uvX = v2.uv0 - v1.uv0;
                uvY = v3.uv0 - v2.uv0;
            }
            else
            {
                triX = pos3 - pos2;
                triY = pos2 - pos1;
                uvX = v3.uv0 - v2.uv0;
                uvY = v2.uv0 - v1.uv0;
            }

            // 计算原始UV框
            //
            var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);
            var uvOrigin = new Vector4(uvMin.x, uvMin.y, uvMax.x, uvMax.y);
            // 为每个顶点设置新的Position和UV，并传入原始UV框
            //
            v1 = _SetNewPosAndUV(v1, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v2 = _SetNewPosAndUV(v2, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            v3 = _SetNewPosAndUV(v3, this.OutlineWidth, posCenter, triX, triY, uvX, uvY, uvOrigin);
            // 应用设置后的UIVertex
            //
            m_VetexList[i] = v1;
            m_VetexList[i + 1] = v2;
            m_VetexList[i + 2] = v3;
        }
    }


    private static UIVertex _SetNewPosAndUV(UIVertex pVertex, int pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVX, Vector2 pUVY,
        Vector4 pUVOrigin)
    {
        // Position
        var pos = pVertex.position;
        var posXOffset = pos.x > pPosCenter.x ? pOutLineWidth : -pOutLineWidth;
        var posYOffset = pos.y > pPosCenter.y ? pOutLineWidth : -pOutLineWidth;
        pos.x += posXOffset;
        pos.y += posYOffset;
        pVertex.position = pos;
        // UV
        var uv = pVertex.uv0;
        var v2 = new Vector2(uv.x, uv.y);
        v2 += pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
        v2 += pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
        uv.x = v2.x;
        uv.y = v2.y;

        pVertex.uv0 = uv;
        // 原始UV框
        pVertex.uv1 = new Vector2(pUVOrigin.x, pUVOrigin.y);
        pVertex.uv2 = new Vector2(pUVOrigin.z, pUVOrigin.w);

        return pVertex;
    }


    private static float _Min(float pA, float pB, float pC)
    {
        return Mathf.Min(Mathf.Min(pA, pB), pC);
    }


    private static float _Max(float pA, float pB, float pC)
    {
        return Mathf.Max(Mathf.Max(pA, pB), pC);
    }


    private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
    }


    private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC)
    {
        return new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
    }
}