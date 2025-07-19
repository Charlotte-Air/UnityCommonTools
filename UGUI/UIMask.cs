using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class UIMask : Mask
{
    public bool maskForEffect
    {
        get
        {
            return m_maskForEffect;
        }
        set
        {
            if (m_maskForEffect == value)
                return;

            m_maskForEffect = value;

            if (graphic != null)
                graphic.SetMaterialDirty();
        }
    }

    [SerializeField]
    private bool m_maskForEffect = true;

    [SerializeField]
    private UIUnmask m_unmask;

    public Material unmaskMaterial = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_unmask != null)
        {
            m_unmask.gameObject.SetActive(true);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_unmask != null)
        {
            m_unmask.gameObject.SetActive(false);
        }
    }

    public override Material GetModifiedMaterial(Material baseMaterial)
    {
        if (!MaskEnabled())
            return baseMaterial;
        Material mat = baseMaterial;
        var rootSortCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform);
        //var stencilDepth = MaskUtilities.GetStencilDepth(transform, rootSortCanvas);
        var stencilDepth = GetStencilDepth(rootSortCanvas);
        if (stencilDepth >= 8)
        {
            Debug.LogWarning("Attempting to use a stencil mask with depth > 8", gameObject);
            return baseMaterial;
        }
        int desiredStencilBit = 1 << stencilDepth;

        if (desiredStencilBit == 1)
        {
            var maskMaterial = StencilMaterial.Add(baseMaterial, 1, StencilOp.Replace, CompareFunction.Always,
                showMaskGraphic ? ColorWriteMask.All : 0);
            StencilMaterial.Remove(mat);
            mat = maskMaterial;
        }
        
        var maskMaterial2 = StencilMaterial.Add(baseMaterial, desiredStencilBit | (desiredStencilBit - 1), StencilOp.Replace, CompareFunction.GreaterEqual, 
            showMaskGraphic ? ColorWriteMask.All : 0, desiredStencilBit - 1, desiredStencilBit | (desiredStencilBit - 1));
        StencilMaterial.Remove(mat);
        mat = maskMaterial2;

        if (maskForEffect)
        {
            unmaskMaterial = graphic.canvasRenderer.GetPopMaterial(0);
            graphic.canvasRenderer.hasPopInstruction = false;
            if (m_unmask != null)
            {
                m_unmask.SetUnmaskMaterial(unmaskMaterial, graphic.canvas.sortingOrder);
            }
        }
        else
        {
            graphic.canvasRenderer.hasPopInstruction = true;
        }

        return mat;
    }

    protected override void OnDestroy()
    {
        ReturnStencilDepth(transform);
        base.OnDestroy();
    }

    private static Dictionary<Transform, int> _transToDepthDict = new Dictionary<Transform, int>();
    private static int depth = 0;
    public static int GetStencilDepth(Transform transform)
    {
        if (transform == null) return 0;
        if (_transToDepthDict.TryGetValue(transform, out int dep)) return dep;
        if (++depth >= 8)
        {
            LogHelper.Warning("StencilDepth 超过最大值,已按最大值处理");
            depth = 7;
        }
        _transToDepthDict.Add(transform,depth);
        return depth;
    }

    public static void ReturnStencilDepth(Transform transform)
    {
        Transform root = GetRootCanvas(transform);
        if (root == null) return;
        if (_transToDepthDict.ContainsKey(root))
        {
            _transToDepthDict.Remove(root);
            depth--;
            return;
        }
        LogHelper.Warning($"Return StencilDepth fail, Not Contains transform: {root.name}");
    }

    private const string UICamera = "UICamera";
    public static Transform GetRootCanvas(Transform transform)
    {
        if (transform == null) return null;
        Transform t = transform;
        while (t.parent != null && !t.parent.CompareTag(UICamera)) t = t.parent;
        return t;
    }
}
