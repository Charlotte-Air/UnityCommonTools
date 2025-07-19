using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 文本描边
/// </summary>
public class UIOutline : BaseMeshEffect 
{
    private static string _outlineShader = "UI/TextOutline";
    [Header("使用渐变")]
    [SerializeField]
    private bool _isUseTextGradient = false;
    public bool isUseTextGradient
    {
        get => _isUseTextGradient;
        set 
        {
            _isUseTextGradient = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    [Header("文字颜色")]
    [SerializeField]
    private Gradient _textGradient;
    public Gradient TextGradient
    {
        get => _textGradient;
        set 
        {
            _textGradient = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    [Header("描边颜色")]
    [SerializeField]
    private Color _outlineColor = Color.white;
    public Color OutlineColor
    {
        get => _outlineColor;
        set 
        {
            _outlineColor = value;
            if (base.graphic != null)
                _Refresh();
        }
    }

    [Header("描边宽度"), Range(0, 8)]
    [SerializeField]
    private float _outlineWidth = 0;
    public float OutlineWidth
    {
        get => _outlineWidth;
        set 
        {
            _outlineWidth = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("---[阴影参数]---")]
    [SerializeField]
    private bool _isShadow = false;
    public bool isUseShadow
    {
        get => _isShadow;
        set 
        {
            _isShadow = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("使用独立透明度")]
    [SerializeField]
    private bool _isShadowAlphaStand = false;
    public bool isShadowAlphaStand
    {
        get => _isShadowAlphaStand;
        set 
        {
            _isShadowAlphaStand = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("阴影文字过渡")]
    [SerializeField]
    private bool _isUseShadowGradient = false;
    public bool isUseShadowGradient
    {
        get => _isUseShadowGradient;
        set 
        {
            _isUseShadowGradient = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("阴影文字颜色")]
    [SerializeField]
    private Gradient _shadowGradient;
    public Gradient ShadowGradient
    {
        get => _shadowGradient;
        set 
        {
            _shadowGradient = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("阴影描边颜色")]
    [SerializeField]
    private Color _shadowOutlineColor = Color.white;
    public Color ShadowOutlineColor
    {
        get => _shadowOutlineColor;
        set 
        {
            _shadowOutlineColor = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("阴影偏移")]
    [SerializeField]
    private Vector2 _shadowOutlineOffset = new Vector2(0, 0);
    public Vector2 ShadowOutlineOffset
    {
        get => _shadowOutlineOffset;
        set 
        {
            _shadowOutlineOffset = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    
    [Header("阴影描边宽度"), Range(0, 8)]
    [SerializeField]
    private float _shadowOutlineWidth = 0;
    public float ShadowOutlineWidth
    {
        get => _shadowOutlineWidth;
        set 
        {
            _shadowOutlineWidth = value;
            if (base.graphic != null)
                _Refresh();
        }
    }
    
    

    int iMatHash = 0;
    bool bSetPreviewCanvas = false;
    
    protected override void Awake() 
    {
        base.Awake();
        if (Application.isPlaying) 
        {
            if (base.graphic.material.GetHashCode() != iMatHash) 
            {
                base.graphic.material = new Material(Shader.Find(_outlineShader));
                iMatHash = base.graphic.material.GetHashCode();
            }
        }
        if (CheckShader()) 
        {
            this.SetShaderChannels();
            this.SetShaderParams();
            this._Refresh();
        }
    }
    
    
    protected override void OnDestroy() 
    {
        base.OnDestroy();
        base.graphic.material = null;
    }
    
    
#pragma warning disable 0114
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    protected void OnValidate() 
    {
        if (!bSetPreviewCanvas && Application.isEditor && gameObject.activeInHierarchy) 
        {
            var can = GetComponentInParent<Canvas>();
            if (can != null) 
            {
                if ((can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord1) == 0 ||
                    (can.additionalShaderChannels & AdditionalCanvasShaderChannels.TexCoord2) == 0) 
                {
                    if (can.name.Contains("(Environment)"))
                    {   //处于Prefab预览场景中需要打开TexCoord[1,2,3]通道否则Scene场景上会有显示问题因为会用到[uv1,uv2.uv3]
                        can.additionalShaderChannels |=
                            AdditionalCanvasShaderChannels.TexCoord1 |
                            AdditionalCanvasShaderChannels.TexCoord2 |
                            AdditionalCanvasShaderChannels.TexCoord3;
                    } 
                    else
                    {   //不是处于Prefab预览场景中父级Canvas中的TexCoord1、TexCoord2通道没打开
                        #if UNITY_EDITOR || UNITY_STANDALONE_WIN 
                        UnityEngine.Debug.LogWarning("Text2DOutline may display incorrect if TexCoord1, TexCoord2 and TexCoord3 Channels are not open at Canvas where Text2DOutline object in.");
                        #endif
                    }
                }
                bSetPreviewCanvas = true;
            }
        }
        base.Invoke("OnValidate", 0);
        if (CheckShader()) 
        {
            this.SetShaderParams();
            this._Refresh();
        }
        if (base.graphic.material.GetHashCode() != iMatHash) 
        {
            base.graphic.material = new Material(Shader.Find(_outlineShader));
            iMatHash = base.graphic.material.GetHashCode();
        }
    }
    
    
    protected void Reset() 
    {
        // base.Reset();
        base.Invoke("Reset", 0);
        if (base.graphic.material.shader.name != _outlineShader) 
        {
            base.graphic.material = new Material(Shader.Find(_outlineShader));
            iMatHash = base.graphic.material.GetHashCode();
        }
    }
#endif
#pragma warning restore 0114

    
    
    private bool CheckShader() 
    {
        if (base.graphic == null) 
        {
            Debug.LogError("No Graphic Component Error!");
            return false;
        }
        if (base.graphic.material == null) 
        {
            Debug.LogError("No Material Error!");
            return false;
        }
        return true;
    }

    
    private void SetShaderParams() 
    {
        if (base.graphic.material != null) 
        {
            base.graphic.material.SetColor("_OutlineColor", OutlineColor);
            base.graphic.material.SetFloat("_OutlineWidth", OutlineWidth);
            base.graphic.material.SetVector("_OutlineOffset", ShadowOutlineOffset);
            base.graphic.material.SetColor("_ShadowOutlineColor", ShadowOutlineColor);
            base.graphic.material.SetFloat("_ShadowOutlineWidth", ShadowOutlineWidth);
        }
    }

    
    private void SetShaderChannels() 
    {
        if (base.graphic.canvas) 
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
            v2 = AdditionalCanvasShaderChannels.TexCoord3;
            if ((v1 & v2) != v2) 
            {
                base.graphic.canvas.additionalShaderChannels |= v2;
            }
        }
    }
    
    
    public override void ModifyMesh(VertexHelper vh) 
    {
        var lVetexList = new List<UIVertex>();
        vh.GetUIVertexStream(lVetexList);
        _ProcessVertices(lVetexList, this.OutlineWidth);

        List<UIVertex> lShadowVerts = new List<UIVertex>();
        if (isUseShadow) 
        {
            vh.GetUIVertexStream(lShadowVerts);
            _ProcessVertices(lShadowVerts, this.ShadowOutlineWidth);
            ApplayShadow(lShadowVerts, ShadowGradient.colorKeys[0].color);
            if (isUseShadowGradient)
            {
                ApplyGradient(lShadowVerts, ShadowGradient);
            }
        }
        if (isUseTextGradient) 
        {
            ApplyGradient(lVetexList, TextGradient);
        }
        vh.Clear();
        //lVetexList = lShadowVerts.Concat(lVetexList).ToList();
        vh.AddUIVertexTriangleStream(lShadowVerts.Concat(lVetexList).ToList());
        lShadowVerts.Clear();
        lVetexList.Clear();
    }

    
    private void ApplayShadow(List<UIVertex> _lShadowVerts, Color32 _color) 
    {
        for (var i = 0; i < _lShadowVerts.Count; i++) 
        {
            UIVertex vt = _lShadowVerts[i];
            vt.position += new Vector3(ShadowOutlineOffset.x, ShadowOutlineOffset.y, 0);
            var newColor = _color;
            if (!isShadowAlphaStand)
            {
                newColor.a = (byte)((newColor.a * vt.color.a) / 255);
            }
            vt.color = newColor;
            vt.uv3.x = -1;  //用来传给Shader判断是文字还是阴影
            _lShadowVerts[i] = vt;
        }
    }

    
    private void ApplyGradient(List<UIVertex> _verts, Gradient _gradient) 
    {
        if (_verts.Count == 0)
        {
            return;
        }
        float topY = _verts[0].position.y;
        float bottomY = _verts[0].position.y;
        for (var i = 0; i < _verts.Count; i++) 
        {
            var y = _verts[i].position.y;
            if (y > topY)
            {
                topY = y;
            }
            else if (y < bottomY)
            {
                bottomY = y;
            }
        }
        float height = topY - bottomY;
        for (var i = 0; i < _verts.Count; i++) 
        {
            var vt = _verts[i];
            Color32 color = _gradient.Evaluate((topY - vt.position.y) / height);
            // var color = Color32.Lerp (new Color(1, 1, 1, 0), new Color(1, 1, 1, 0), (vt.position.y - bottomY) / height);
            color.a = (byte)((color.a * vt.color.a) / 255);
            vt.color = color;
            _verts[i] = vt;
        }
    }

    
    private void _ProcessVertices(List<UIVertex> lVerts, float outlineWidth) 
    {
        // 添加描边后为防止描边被网格边框裁切需要将顶点外扩同时保持UV不变
        for (int i = 0, count = lVerts.Count - 3; i <= count; i += 3) 
        {
            var v1 = lVerts[i];
            var v2 = lVerts[i + 1];
            var v3 = lVerts[i + 2];
            
            // 计算原顶点坐标中心点
            var minX = _Min(v1.position.x, v2.position.x, v3.position.x);
            var minY = _Min(v1.position.y, v2.position.y, v3.position.y);
            var maxX = _Max(v1.position.x, v2.position.x, v3.position.x);
            var maxY = _Max(v1.position.y, v2.position.y, v3.position.y);
            var posCenter = new Vector2(minX + maxX, minY + maxY) * 0.5f;
            
            // 计算原始顶点坐标和UV的方向
            Vector2 triX, triY, uvX, uvY;
            Vector2 pos1 = v1.position;
            Vector2 pos2 = v2.position;
            Vector2 pos3 = v3.position;
            if (Mathf.Abs(Vector2.Dot((pos2 - pos1).normalized, Vector2.right)) > Mathf.Abs(Vector2.Dot((pos3 - pos2).normalized, Vector2.right))) 
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
            var uvMin = _Min(v1.uv0, v2.uv0, v3.uv0);
            var uvMax = _Max(v1.uv0, v2.uv0, v3.uv0);

            // 为每个顶点设置新的Position和UV，并传入原始UV框
            v1 = _SetNewPosAndUV(v1, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, this.ShadowOutlineOffset);
            v2 = _SetNewPosAndUV(v2, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, this.ShadowOutlineOffset);
            v3 = _SetNewPosAndUV(v3, outlineWidth, posCenter, triX, triY, uvX, uvY, uvMin, uvMax, this.ShadowOutlineOffset);

            // 应用设置后的UIVertex
            lVerts[i] = v1;
            lVerts[i + 1] = v2;
            lVerts[i + 2] = v3;
        }
    }


    private static UIVertex _SetNewPosAndUV
    (
        UIVertex pVertex,
        float pOutLineWidth,
        Vector2 pPosCenter,
        Vector2 pTriangleX, Vector2 pTriangleY,
        Vector2 pUVX, Vector2 pUVY,
        Vector2 pUVOriginMin, Vector2 pUVOriginMax, Vector2 offset) 
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
        var uvOffsetX = pUVX / pTriangleX.magnitude * posXOffset * (Vector2.Dot(pTriangleX, Vector2.right) > 0 ? 1 : -1);
        var uvOffsetY = pUVY / pTriangleY.magnitude * posYOffset * (Vector2.Dot(pTriangleY, Vector2.up) > 0 ? 1 : -1);
        uv.x += (uvOffsetX.x + uvOffsetY.x);
        uv.y += (uvOffsetX.y + uvOffsetY.y);
        pVertex.uv0 = uv;

        //uv1 uv2 可用  tangent  normal 在缩放情况 会有问题
        pVertex.uv1 = pUVOriginMin;
        pVertex.uv2 = pUVOriginMax;
        return pVertex;
    }
    

    private void _Refresh() 
    {
        SetShaderParams();
        base.graphic.SetVerticesDirty();
    }
    
    private static float _Min(float pA, float pB, float pC) => Mathf.Min(Mathf.Min(pA, pB), pC);
    private static float _Max(float pA, float pB, float pC) => Mathf.Max(Mathf.Max(pA, pB), pC);
    private static Vector2 _Min(Vector2 pA, Vector2 pB, Vector2 pC) => new Vector2(_Min(pA.x, pB.x, pC.x), _Min(pA.y, pB.y, pC.y));
    private static Vector2 _Max(Vector2 pA, Vector2 pB, Vector2 pC) => new Vector2(_Max(pA.x, pB.x, pC.x), _Max(pA.y, pB.y, pC.y));
}