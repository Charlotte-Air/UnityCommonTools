using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Events;
using System.Collections;
using Framework.Utils.Unity;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class UIImageMask : Image
{
    public Texture2D maskTexture
    {
        get
        {
            return m_maskTexture;
        }
        set
        {
            if (m_maskTexture != value)
            {
                m_maskTexture = value;
                UpdateMaskObj();
                if (m_maskMaterial != null)
                {
                    m_maskMaterial.SetTexture("_MaskTex", maskTexture);
                }
            }
        }
    }

    public Vector4 positionScale
    {
        get
        {
            return m_positionScale;
        }
        set
        {
            if (m_positionScale != value)
            {
                m_positionScale = value;
                UpdateMaskObj();
                if (m_maskMaterial != null)
                {
                    m_maskMaterial.SetVector("_PositionScale", m_positionScale);
                }
            }
        }
    }

    [SerializeField]
    private Texture2D m_maskTexture;

    [SerializeField]
    private Vector4 m_positionScale = new Vector4(0, 0, 1, 1);

    private Material m_maskMaterial;

    public override Material material
    {
        get
        {
            if (m_maskMaterial == null)
            {
                Shader sd = CookShaders.Find("UI/Mask");
                if (sd != null)
                {
                    m_maskMaterial = new Material(sd);
                    MaterialSetup();
                }
            }
            return m_maskMaterial;
        }
        set
        {
            LogHelper.DebugFormat("{0} set mat", m_maskMaterial);
            m_maskMaterial = value;
        }
    }

    private RectTransform m_maskObj;

    protected override void Awake()
    {
        base.Awake();
        GameObject maskObj = new GameObject("maskObj", typeof(RectTransform));
        maskObj.hideFlags = HideFlags.HideAndDontSave;
        maskObj.transform.SetParent(transform);
        maskObj.transform.ClearPRS();
        m_maskObj = maskObj.GetComponent<RectTransform>();
        UpdateMaskObj();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (m_maskObj != null)
        {
            GameObject.DestroyImmediate(m_maskObj.gameObject);
            m_maskObj = null;
        }
        if (m_maskMaterial != null)
        {
            Material.DestroyImmediate(m_maskMaterial);
            m_maskMaterial = null;
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        MaterialSetup();
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

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        if (m_maskMaterial != null)
        {
            Vector4 sizeInfo = Vector4.one;
            sizeInfo.x = rectTransform.rect.width;
            sizeInfo.y = rectTransform.rect.height;
            sizeInfo.z = sizeInfo.x / sizeInfo.y;
            sizeInfo.w = sizeInfo.y / sizeInfo.x;
            m_maskMaterial.SetVector("_SizeInfo", sizeInfo);
        }
    }

    public override bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        if (!isActiveAndEnabled)
            return true;

        return !RectTransformUtility.RectangleContainsScreenPoint(m_maskObj, sp, eventCamera);
    }


    private void UpdateMaskObj()
    {
        if (m_maskObj != null)
        {
            m_maskObj.anchoredPosition = new Vector2(m_positionScale.x, m_positionScale.y);
            m_maskObj.sizeDelta = new Vector2(maskTexture.width * m_positionScale.z, maskTexture.height * m_positionScale.w);
        }
    }

    private void MaterialSetup()
    {
        if (m_maskMaterial != null)
        {
            Vector4 sizeInfo = Vector4.one;
            sizeInfo.x = rectTransform.rect.width;
            sizeInfo.y = rectTransform.rect.height;
            sizeInfo.z = sizeInfo.x / sizeInfo.y;
            sizeInfo.w = sizeInfo.y / sizeInfo.x;
            m_maskMaterial.SetVector("_SizeInfo", sizeInfo);
            m_maskMaterial.SetVector("_PositionScale", m_positionScale);
            m_maskMaterial.SetTexture("_MaskTex", maskTexture);
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        UpdateMaskObj();
        MaterialSetup();
    }
#endif
}