using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class UIUnmask : UIBehaviour, IMaterialModifier
{
    [NonSerialized]
    private RectTransform m_RectTransform;
    public RectTransform rectTransform
    {
        get { return m_RectTransform ?? (m_RectTransform = GetComponent<RectTransform>()); }
    }

    [NonSerialized]
    private Graphic m_Graphic;
    public Graphic graphic
    {
        get { return m_Graphic ?? (m_Graphic = GetComponent<Graphic>()); }
    }

    [NonSerialized]
    private Material m_UnmaskMaterial = null;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (!IsActive())
            return;

        if (graphic != null)
            graphic.SetMaterialDirty();
    }
#endif

    public void SetUnmaskMaterial(Material unmaskMtl, int sortingOrder)
    {
        Canvas canvas = gameObject.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = sortingOrder;
        }
        
        m_UnmaskMaterial = unmaskMtl;
    }

    public virtual Material GetModifiedMaterial(Material baseMaterial)
    {
        if (m_UnmaskMaterial == null)
        {
            baseMaterial.SetInt("_ColorMask", 0);
            return baseMaterial;
        }

        return m_UnmaskMaterial;
    }
}