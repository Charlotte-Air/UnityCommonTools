using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class UIDepth : MonoBehaviour, IMaskable 
{
    public bool maskable = false;
	public int order;
	public bool isUI = false;
    public bool isMatchOrder = false;
    public int matchOther = 1;
    public Canvas MatchCanvas = null;
    private Transform rootCanvas;

    private List<Material> m_mtls = null;
    
    private void Awake()
    {
        if (Application.isPlaying)
        {
            if (maskable)
            {
                m_mtls = new List<Material>();
                Renderer[] rnds = gameObject.GetComponentsInChildren<Renderer>(true);
                for (int i = 0; i < rnds.Length; i++)
                {
                    Renderer rnd = rnds[i];
                    if (rnd != null && rnd.material != null)
                    {
                        m_mtls.Add(rnd.material);
                    }
                }
            }
        }
    }

    private void Start()
    {
        Reset();
    }

    private void OnEnable()
	{
        Reset();
        if(maskable)
            RecalculateMasking();
    }

    public void Reset()
    {
        if (isUI)
        {
            Canvas canvas = GetComponent<Canvas>();

            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
            }

            canvas.overrideSorting = true;
            if (isMatchOrder)
            {
                if (MatchCanvas == null)
                {
                    if (transform.parent != null)
                    {
                        MatchCanvas = transform.parent.gameObject.GetComponentInParent<Canvas>();
                    }
                }
                if (MatchCanvas != null)
                {
                    canvas.sortingOrder = MatchCanvas.sortingOrder + matchOther;
                    order = MatchCanvas.sortingOrder + matchOther;
                }
                else
                {
                    canvas.sortingOrder = order;
                }
            }
            else
            {
                canvas.sortingOrder = order;
            }
        }
        else
        {
            Renderer[] renders = GetComponentsInChildren<Renderer>();

            foreach (Renderer render in renders)
            {
                if (isMatchOrder)
                {
                    if (MatchCanvas == null)
                    {
                        MatchCanvas = gameObject.GetComponentInParent<Canvas>();
                    }

                    int so = 0;
                    if (MatchCanvas != null)
                    {
                        so = MatchCanvas.sortingOrder;
                    }

                    render.sortingOrder = so + matchOther;
                    order = so + matchOther;
                }
                else
                {
                    render.sortingOrder = order;
                }
            }
        }
    }

    public void RecalculateMasking()
    {
        if (isUI)
        {
            return;
        }

        if (!maskable)
        {
            return;
        }

        if (m_mtls == null)
        {
            m_mtls = new List<Material>();
            Renderer[] rnds = gameObject.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < rnds.Length; i++)
            {
                Renderer rnd = rnds[i];
                if (rnd != null && rnd.material != null)
                {
                    m_mtls.Add(rnd.material);
                }
            }
        }

        rootCanvas = MatchCanvas == null ? MaskUtilities.FindRootSortOverrideCanvas(transform) : MatchCanvas.transform;
        int stencilValue = UIMask.GetStencilDepth(rootCanvas);
        //int stencilValue = MaskUtilities.GetStencilDepth(transform,rootCanvas);
        if (stencilValue > 0)
        {
            int stencil = 1 << stencilValue;
            for (int i = 0; i < m_mtls.Count; i++)
            {
                SetMaterial(m_mtls[i], stencil, StencilOp.Keep, CompareFunction.Equal, stencil, 0);
            }
        }
    }

    private void SetMaterial(Material mat, int stencilID, StencilOp operation, CompareFunction compareFunction, int readMask, int writeMask)
    {
        if (!mat.HasProperty("_Stencil") || !mat.HasProperty("_StencilOp") ||
            !mat.HasProperty("_StencilComp") || !mat.HasProperty("_StencilReadMask") ||
            !mat.HasProperty("_StencilWriteMask"))
        {
            return;
        }

        mat.SetInt("_Stencil", stencilID);
        mat.SetInt("_StencilOp", (int)operation);
        mat.SetInt("_StencilComp", (int)compareFunction);
        mat.SetInt("_StencilReadMask", readMask);
        mat.SetInt("_StencilWriteMask", writeMask);
    }
}
