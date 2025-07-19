using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[AddComponentMenu("UI/UIImage")]
public class UIImage : Image
{
    public enum MirrorType
    {
        None,

        /// <summary>
        /// 水平
        /// </summary>
        Horizontal,

        /// <summary>
        /// 垂直
        /// </summary>
        Vertical,

        /// <summary>
        /// 四分之一
        /// 相当于水平，然后再垂直
        /// </summary>
        Quarter,
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
                    if (si.sprite.name == spriteName)
                    {
                        this.sprite = si.sprite;
                        if (si.mat != null)
                        {
                            this.material = si.mat;
                        }
                    }
                }
                catch
                { }
            });
            */
        }
    }

    public bool enableMask
    {
        get
        {
            return m_enableMask;
        }
        set
        {
            if (m_enableMask != value)
            {
                m_enableMask = value;
                SetVerticesDirty();
            }
        }
    }

    public Sprite maskSprite
    {
        get
        {
            return m_maskSprite;
        }
        set
        {
            if (m_maskSprite != value)
            {
                m_maskSprite = value;
                SetVerticesDirty();
            }
        }
    }

    public bool enableMirror
    {
        set
        {
            if (m_EnableMirror != value)
            {
                m_EnableMirror = value;
                SetVerticesDirty();
            }
        }
        get
        {
            return m_EnableMirror;
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

    public bool noAlphaTex
    {
        set
        {
            m_noAlphaTex = value;
            SetVerticesDirty();
        }
        get
        {
            return m_noAlphaTex;
        }
    }

    [SerializeField]
    private string m_spriteName;

    [SerializeField]
    private bool m_isEnabled = true;

    [SerializeField]
    private bool m_noGraphics = false;

    [SerializeField]
    private bool m_noAlphaTex = false;
    
    [SerializeField]
    private bool m_enableMask = false;
    
    [SerializeField]
    private Sprite m_maskSprite;

    /// <summary>
    /// 镜像类型
    /// </summary>
    [SerializeField]
    private MirrorType m_MirrorType = MirrorType.None;
    [SerializeField]
    private bool m_EnableMirror = false;

    [SerializeField]
    private bool m_EnableAdjust = false;
    [SerializeField]
    private int m_AdjustLeft = 0;
    [SerializeField]
    private int m_AdjustRight = 0;
    [SerializeField]
    private int m_AdjustTop = 0;
    [SerializeField]
    private int m_AdjustBottom = 0;

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


    protected override void OnDestroy()
    {
    }

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

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        if (m_noGraphics)
        {
            toFill.Clear();
            return;
        }

        base.OnPopulateMesh(toFill);

        float e = m_isEnabled ? 0 : 2;
        float a = !m_noAlphaTex ? 0 : 2;

        UIVertex vertex = new UIVertex();

        if (this.type == Type.Simple && (m_enableMask && m_maskSprite != null))
        {
            var uv1 = (m_maskSprite != null) ? UnityEngine.Sprites.DataUtility.GetOuterUV(m_maskSprite) : Vector4.zero;

            toFill.PopulateUIVertex(ref vertex, 0);
            vertex.uv1 = new Vector2(uv1.x + e, uv1.y + a);
            toFill.SetUIVertex(vertex, 0);

            toFill.PopulateUIVertex(ref vertex, 1);
            vertex.uv1 = new Vector2(uv1.x + e, uv1.w + a);
            toFill.SetUIVertex(vertex, 1);

            toFill.PopulateUIVertex(ref vertex, 2);
            vertex.uv1 = new Vector2(uv1.z + e, uv1.w + a);
            toFill.SetUIVertex(vertex, 2);

            toFill.PopulateUIVertex(ref vertex, 3);
            vertex.uv1 = new Vector2(uv1.z + e, uv1.y + a);
            toFill.SetUIVertex(vertex, 3);
        }
        else
        {
            int vcount = toFill.currentVertCount;
            for (int i = 0; i < vcount; i++)
            {
                toFill.PopulateUIVertex(ref vertex, i);
                vertex.uv1 = new Vector3(vertex.uv0.x + e, vertex.uv0.y + a);
                toFill.SetUIVertex(vertex, i);
            }
        }

        if (m_EnableAdjust)
        {
            AdjustUV(toFill);
        }

        if (m_EnableMirror)
        {
            DrawMirror(toFill);
        }
    }

    private void AdjustUV(VertexHelper vh)
    {
        float leftUV = 0;
        float rightUV = 0;
        float topUV = 0;
        float bottomUV = 0;

        float leftAdjust = (float)m_AdjustLeft / sprite.texture.width;
        float rightAdjust = (float)m_AdjustRight / sprite.texture.width;
        float topAdjust = (float)m_AdjustTop / sprite.texture.height;
        float bottomAdjust = (float)m_AdjustBottom / sprite.texture.height;

        UIVertex vertex = new UIVertex();
        int vcount = vh.currentVertCount;
        for (int i = 0; i < vcount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            if (vertex.uv0.x < leftUV)
            {
                leftUV = vertex.uv0.x;
            }

            if (vertex.uv0.x > rightUV)
            {
                rightUV = vertex.uv0.x;
            }

            if (vertex.uv0.y < bottomUV)
            {
                bottomUV = vertex.uv0.x;
            }

            if (vertex.uv0.y > topUV)
            {
                topUV = vertex.uv0.x;
            }
        }

        for (int i = 0; i < vcount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            if (m_AdjustLeft != 0 && Math.Abs(vertex.uv0.x - leftUV) < 0.001)
            {
                vertex.uv0.x += leftAdjust;
                vertex.uv1.x += leftAdjust;
                vh.SetUIVertex(vertex, i);
            }

            if (m_AdjustRight != 0 && Math.Abs(vertex.uv0.x - rightUV) < 0.001)
            {
                vertex.uv0.x -= rightAdjust;
                vertex.uv1.x -= rightAdjust;
                vh.SetUIVertex(vertex, i);
            }

            if (m_AdjustTop != 0 && Math.Abs(vertex.uv0.y - topUV) < 0.001)
            {
                vertex.uv0.y -= topAdjust;
                vertex.uv1.y -= topAdjust;
                vh.SetUIVertex(vertex, i);
            }

            if (m_AdjustBottom != 0 && Math.Abs(vertex.uv0.y - bottomUV) < 0.001)
            {
                vertex.uv0.y += bottomAdjust;
                vertex.uv1.y += bottomAdjust;
                vh.SetUIVertex(vertex, i);
            }

           
        }
    }

    #region Mirror
    public void DrawMirror(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        if (this.type == Image.Type.Filled || this.m_MirrorType == MirrorType.None)
        {
            return;
        }

        var output = new List<UIVertex>();
        vh.GetUIVertexStream(output);

        int count = output.Count;

        switch (this.type)
        {
            case Image.Type.Simple:
                DrawSimple(output, count);
                break;
            case Image.Type.Sliced:
                DrawSliced(output, count);
                break;
            case Image.Type.Tiled:
                DrawTiled(output, count);
                break;
            case Image.Type.Filled:
                break;
        }

        vh.Clear();
        vh.AddUIVertexTriangleStream(output);

    }

    /// <summary>
    /// 绘制Simple版
    /// </summary>
    /// <param name="output"></param>
    /// <param name="count"></param>
    protected void DrawSimple(List<UIVertex> output, int count)
    {
        Rect rect = GetPixelAdjustedRect();

        SimpleScale(rect, output, count);

        switch (m_MirrorType)
        {
            case MirrorType.Horizontal:
                ExtendCapacity(output, count);
                MirrorVerts(rect, output, count, true);
                break;
            case MirrorType.Vertical:
                ExtendCapacity(output, count);
                MirrorVerts(rect, output, count, false);
                break;
            case MirrorType.Quarter:
                ExtendCapacity(output, count * 3);
                MirrorVerts(rect, output, count, true);
                MirrorVerts(rect, output, count * 2, false);
                break;
        }
    }

    /// <summary>
    /// 绘制Sliced版
    /// </summary>
    /// <param name="output"></param>
    /// <param name="count"></param>
    protected void DrawSliced(List<UIVertex> output, int count)
    {
        if (!this.hasBorder)
        {
            DrawSimple(output, count);

            return;
        }

        Rect rect = GetPixelAdjustedRect();

        SlicedScale(rect, output, count);

        count = SliceExcludeVerts(output, count);

        switch (m_MirrorType)
        {
            case MirrorType.Horizontal:
                ExtendCapacity(output, count);
                MirrorVerts(rect, output, count, true);
                break;
            case MirrorType.Vertical:
                ExtendCapacity(output, count);
                MirrorVerts(rect, output, count, false);
                break;
            case MirrorType.Quarter:
                ExtendCapacity(output, count * 3);
                MirrorVerts(rect, output, count, true);
                MirrorVerts(rect, output, count * 2, false);
                break;
        }
    }

    /// <summary>
    /// 绘制Tiled版
    /// </summary>
    /// <param name="output"></param>
    /// <param name="count"></param>
    protected void DrawTiled(List<UIVertex> verts, int count)
    {
        Sprite overrideSprite = this.overrideSprite;

        if (overrideSprite == null)
        {
            return;
        }

        Rect rect = GetPixelAdjustedRect();

        //此处使用inner是因为Image绘制Tiled时，会把透明区域也绘制了。

        Vector4 inner = UnityEngine.Sprites.DataUtility.GetInnerUV(overrideSprite);

        float w = overrideSprite.rect.width / this.pixelsPerUnit;
        float h = overrideSprite.rect.height / this.pixelsPerUnit;

        int len = count / 3;

        for (int i = 0; i < len; i++)
        {
            UIVertex v1 = verts[i * 3];
            UIVertex v2 = verts[i * 3 + 1];
            UIVertex v3 = verts[i * 3 + 2];

            float centerX = GetCenter(v1.position.x, v2.position.x, v3.position.x);

            float centerY = GetCenter(v1.position.y, v2.position.y, v3.position.y);

            if (m_MirrorType == MirrorType.Horizontal || m_MirrorType == MirrorType.Quarter)
            {
                //判断三个点的水平位置是否在偶数矩形内，如果是，则把UV坐标水平翻转
                if (Mathf.FloorToInt((centerX - rect.xMin) / w) % 2 == 1)
                {
                    v1.uv0 = GetOverturnUV(v1.uv0, inner.x, inner.z, true);
                    v2.uv0 = GetOverturnUV(v2.uv0, inner.x, inner.z, true);
                    v3.uv0 = GetOverturnUV(v3.uv0, inner.x, inner.z, true);
                }
            }

            if (m_MirrorType == MirrorType.Vertical || m_MirrorType == MirrorType.Quarter)
            {
                //判断三个点的垂直位置是否在偶数矩形内，如果是，则把UV坐标垂直翻转
                if (Mathf.FloorToInt((centerY - rect.yMin) / h) % 2 == 1)
                {
                    v1.uv0 = GetOverturnUV(v1.uv0, inner.y, inner.w, false);
                    v2.uv0 = GetOverturnUV(v2.uv0, inner.y, inner.w, false);
                    v3.uv0 = GetOverturnUV(v3.uv0, inner.y, inner.w, false);
                }
            }

            verts[i * 3] = v1;
            verts[i * 3 + 1] = v2;
            verts[i * 3 + 2] = v3;
        }
    }

    /// <summary>
    /// 扩展容量
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="addCount"></param>
    protected void ExtendCapacity(List<UIVertex> verts, int addCount)
    {
        var neededCapacity = verts.Count + addCount;
        if (verts.Capacity < neededCapacity)
        {
            verts.Capacity = neededCapacity;
        }
    }

    /// <summary>
    /// Simple缩放位移顶点（减半）
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="verts"></param>
    /// <param name="count"></param>
    protected void SimpleScale(Rect rect, List<UIVertex> verts, int count)
    {
        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = verts[i];

            Vector3 position = vertex.position;

            if (m_MirrorType == MirrorType.Horizontal || m_MirrorType == MirrorType.Quarter)
            {
                position.x = (position.x + rect.x) * 0.5f;
            }

            if (m_MirrorType == MirrorType.Vertical || m_MirrorType == MirrorType.Quarter)
            {
                position.y = (position.y + rect.y) * 0.5f;
            }

            vertex.position = position;

            verts[i] = vertex;
        }
    }

    /// <summary>
    /// Sliced缩放位移顶点（减半）
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="verts"></param>
    /// <param name="count"></param>
    protected void SlicedScale(Rect rect, List<UIVertex> verts, int count)
    {
        Vector4 border = GetAdjustedBorders(rect);

        float halfWidth = rect.width * 0.5f;

        float halfHeight = rect.height * 0.5f;

        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = verts[i];

            Vector3 position = vertex.position;

            if (m_MirrorType == MirrorType.Horizontal || m_MirrorType == MirrorType.Quarter)
            {
                if (halfWidth < border.x && position.x >= rect.center.x)
                {
                    position.x = rect.center.x;
                }
                else if (position.x >= border.x)
                {
                    position.x = (position.x + rect.x) * 0.5f;
                }
            }

            if (m_MirrorType == MirrorType.Vertical || m_MirrorType == MirrorType.Quarter)
            {
                if (halfHeight < border.y && position.y >= rect.center.y)
                {
                    position.y = rect.center.y;
                }
                else if (position.y >= border.y)
                {
                    position.y = (position.y + rect.y) * 0.5f;
                }
            }

            vertex.position = position;

            verts[i] = vertex;
        }
    }

    /// <summary>
    /// 镜像顶点
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="verts"></param>
    /// <param name="count"></param>
    /// <param name="isHorizontal"></param>
    protected void MirrorVerts(Rect rect, List<UIVertex> verts, int count, bool isHorizontal = true)
    {
        for (int i = 0; i < count; i++)
        {
            UIVertex vertex = verts[i];

            Vector3 position = vertex.position;

            if (isHorizontal)
            {
                if (Mathf.Abs(position.x - rect.center.x) > 1)
                {
                    position.x = rect.center.x * 2 - position.x;
                }
            }
            else
            {
                if (Mathf.Abs(position.y - rect.center.y) > 1)
                {
                    position.y = rect.center.y * 2 - position.y;
                }
            }

            vertex.position = position;

            verts.Add(vertex);
        }
    }

    /// <summary>
    /// 清理掉不能成三角面的顶点
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    protected int SliceExcludeVerts(List<UIVertex> verts, int count)
    {
        int realCount = count;

        int i = 0;

        while (i < realCount)
        {
            UIVertex v1 = verts[i];
            UIVertex v2 = verts[i + 1];
            UIVertex v3 = verts[i + 2];

            if (v1.position == v2.position || v2.position == v3.position || v3.position == v1.position)
            {
                verts[i] = verts[realCount - 3];
                verts[i + 1] = verts[realCount - 2];
                verts[i + 2] = verts[realCount - 1];

                realCount -= 3;
                continue;
            }

            i += 3;
        }

        if (realCount < count)
        {
            verts.RemoveRange(realCount, count - realCount);
        }

        return realCount;
    }

    /// <summary>
    /// 返回矫正过的范围
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    protected Vector4 GetAdjustedBorders(Rect rect)
    {
        Sprite overrideSprite = this.overrideSprite;

        Vector4 border = overrideSprite.border;

        border = border / this.pixelsPerUnit;

        for (int axis = 0; axis <= 1; axis++)
        {
            float combinedBorders = border[axis] + border[axis + 2];
            if (rect.size[axis] < combinedBorders && combinedBorders != 0)
            {
                float borderScaleRatio = rect.size[axis] / combinedBorders;
                border[axis] *= borderScaleRatio;
                border[axis + 2] *= borderScaleRatio;
            }
        }

        return border;
    }

    /// <summary>
    /// 返回三个点的中心点
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <returns></returns>
    protected float GetCenter(float p1, float p2, float p3)
    {
        float max = Mathf.Max(Mathf.Max(p1, p2), p3);

        float min = Mathf.Min(Mathf.Min(p1, p2), p3);

        return (max + min) / 2;
    }

    /// <summary>
    /// 返回翻转UV坐标
    /// </summary>
    /// <param name="uv"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <param name="isHorizontal"></param>
    /// <returns></returns>
    protected Vector2 GetOverturnUV(Vector2 uv, float start, float end, bool isHorizontal = true)
    {
        if (isHorizontal)
        {
            uv.x = end - uv.x + start;
        }
        else
        {
            uv.y = end - uv.y + start;
        }

        return uv;
    }

    #endregion


}