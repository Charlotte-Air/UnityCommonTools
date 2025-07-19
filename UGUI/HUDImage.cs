using UnityEngine;
using UnityEngine.UI;

public interface IHUDComponent
{
    void UpdateTargetPosition(Vector3 pos);
}

public class HUDImage : Image, IHUDComponent
{
    private Vector3 m_targetPos;

    [SerializeField]
    private string m_spriteName;

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (transform.localScale != Vector3.one)
        {
            Debug.LogFormat(gameObject, "hud image {0} 的缩放不为1.", gameObject.name);
        }
#endif
    }

    public void UpdateTargetPosition(Vector3 pos)
    {
        m_targetPos = pos;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        UnityEngine.Profiling.Profiler.BeginSample("hud image on populate mesh");
        base.OnPopulateMesh(toFill);

        UIVertex vertex = new UIVertex(); ;
        int vcount = toFill.currentVertCount;
        for (int i = 0; i < vcount; i++)
        {
            toFill.PopulateUIVertex(ref vertex, i);
            vertex.uv1 = new Vector2(0, 0);
            vertex.normal = m_targetPos;
            toFill.SetUIVertex(vertex, i);
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }


    protected override void UpdateMaterial()
    {
        if (!IsActive())
            return;

        canvasRenderer.materialCount = 1;
        canvasRenderer.SetMaterial(materialForRendering, 0);
        canvasRenderer.SetTexture(mainTexture);
    }
    
    public string spriteName
    {
        get
        {
            return m_spriteName;
        }
        set
        {
            if (m_spriteName != value)
            {
                m_spriteName = value;
                // UISpriteManager.GetHUDSprite(spriteName, (si) =>
                // {
                //     this.sprite = si.sprite;
                //     this.material = si.mat;
                // });
            }
        }
    }
}
