using UnityEngine;
using UnityEngine.UI;

public class HUDText : UIText, IHUDComponent
{
	private static int s_FontTexId = -1;
    private Vector3 m_targetPos;

    protected override void Awake()
    {
#if UNITY_EDITOR
        if (transform.localScale != Vector3.one)
        {
	        Debug.LogFormat(gameObject, "hud image {0} 的缩放不为1.", gameObject.name);
        }
#endif
        s_FontTexId = Shader.PropertyToID("_FontTex");
    }

    public void UpdateTargetPosition(Vector3 pos)
    {
        m_targetPos = pos;
        SetVerticesDirty();
    }

	protected override void OnPopulateMesh(VertexHelper toFill)
	{
        UnityEngine.Profiling.Profiler.BeginSample("hud text on populate mesh");
		base.OnPopulateMesh(toFill);

		UIVertex vertex = new UIVertex(); ;
		int vcount = toFill.currentVertCount;
		for (int i = 0; i < vcount; i++)
		{
			toFill.PopulateUIVertex(ref vertex, i);
			vertex.uv1 = new Vector2(1, 0);
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
        materialForRendering.SetTexture(s_FontTexId, mainTexture);
	}
}

