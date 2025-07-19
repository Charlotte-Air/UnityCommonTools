using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 自定义Image
/// </summary>
[AddComponentMenu("UI/UIRawImage")]
public class UIRawImage : RawImage
{
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
                SetMaterialDirty();
            }
        }
    }

    [SerializeField]
    protected bool m_loadRawFile = false;

    [SerializeField]
    public string m_rawFilePath;

    private bool m_curTextureNeedUnload = false;

    [SerializeField]
    protected bool m_noGraphics = false;

    [SerializeField]
    protected bool m_isEnabled = true;

    [SerializeField]
    protected bool m_isRandom = false;

    [SerializeField]
    public string m_path;

    [SerializeField]
    public string[] m_listProperty;

    public override Material material
    {
        get
        {
            if (!m_isEnabled)
            {
                if (m_grayMaterial == null)
                {
                    m_grayMaterial = new Material(CookShaders.Find("UI/Gray"));
                }
                return m_grayMaterial;
            }
            else
            {
                return base.material;
            }
        }
    }

    public bool noGraphics
    {
        get
        {
            return m_noGraphics;
        }
        set
        {
            if (m_noGraphics != value)
            {
                m_noGraphics = value;
                SetVerticesDirty();
            }
        }
    }

    protected Material m_grayMaterial;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (m_loadRawFile && Application.isPlaying)
        {
            LoadTexture(m_rawFilePath);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (m_loadRawFile && Application.isPlaying)
        {
            if (texture != null && m_curTextureNeedUnload)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }
    }

    protected override void OnDestroy()
    {
        if(m_grayMaterial != null)
            DestroyImmediate(m_grayMaterial);
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (!m_noGraphics)
        {
            base.OnPopulateMesh(vh);
        }
        else
        {
            vh.Clear();
        }
    }

    public void LoadTexture(string texPath)
    {
        if (!string.IsNullOrEmpty(texPath))
        {
            if (m_loadRawFile)
            {
                /*
                AssetManager.LoadRawAsync(texPath, AssetManager.TYPERES.ETE_TEXTURE2D_PNG , null, (ctx, asset) =>
                {
                    if (asset == null)
                    {
                        return;
                    }

                    byte[] bytes = asset as byte[];
                    if (bytes == null)
                    {
                        LogHelper.WarningFormat("rawimage tex:{0} invalid, use default", texPath);
                        return;
                    }

                    Texture2D tex = new Texture2D(2, 2);
                    if (tex != null)
                    {
                        if (tex.LoadImage(bytes))
                        {
                            if (texture != null && m_curTextureNeedUnload)
                            {
                                Texture2D.Destroy(texture);
                                texture = null;
                            }
                            texture = tex;
                            m_curTextureNeedUnload = true;
                        }
                        else
                        {
                            Texture2D.Destroy(tex);
                        }
                    }
                });
                */
            }
            else
            {
                /*
                AssetManager.LoadAsync(texPath, AssetManager.TYPERES.ETE_TEXTURE2D_PNG, null, (ctx, asset) =>
                {
                    if (asset == null)
                    {
                        return;
                    }
                    if (texture != null && m_curTextureNeedUnload)
                    {
                        AssetManager.Unload(texture);
                        texture = null;
                    }
                    texture = asset as Texture2D;
                    m_curTextureNeedUnload = true;
                });
                */
            }            
        }
    }
}


