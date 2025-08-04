using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class UnityExceptionUtils
{
    static int _UILayer = LayerMask.NameToLayer("UI");
    static int _HiddenLayer = LayerMask.NameToLayer("Hidden");
    static List<Canvas> _CanvasCacheList = new List<Canvas>();
    static List<CanvasRenderer> _RendersList = new List<CanvasRenderer>();
    static List<MaskableGraphic> _tempGraphics = new List<MaskableGraphic>();

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T comp = go.GetComponent<T>();
        if (!comp)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

    public static T GetOrAddComponent<T>(this Transform t) where T : Component
    {
        T comp = t.GetComponent<T>();
        if (!comp)
        {
            comp = t.gameObject.AddComponent<T>();
        }
        return comp;
    }
    
    /// <summary>
    /// 获取obj屏幕位置 Screen Space - Camera 模式
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Vector2 GetGameObjectScreenPos(this GameObject obj)
    {
        GameObject cameraNode = GameObject.Find("Root/UICamera");
        Vector2 pos;
        if (cameraNode == null)
        {
            pos = obj.transform.position;
        }
        else
        {
            Camera uiCamera = cameraNode.GetComponent<Camera>();
            pos = uiCamera.WorldToScreenPoint(obj.transform.position);
            pos *= GameConfig.GetGameRatio;
        }

        return pos;
    }

    /// <summary>
    /// 设置UGUI显隐
    /// </summary>
    /// <param name="go"></param>
    /// <param name="value"></param>
    public static void SetUIActiveFast(this GameObject go, bool value)
    {
        // if active false, must set active.
        if (!go.activeSelf)
        {
            go.SetActive(value);
            return;
        }

        // if game object is canvas, set layer fastest.
        if (go.GetComponent<Canvas>())
        {
            go.GetComponentsInChildren(false, _CanvasCacheList);
            foreach (var canvas in _CanvasCacheList)
            {
                canvas.gameObject.layer = value ? _UILayer : _HiddenLayer;
            }

            _CanvasCacheList.Clear();
            return;
        }

        //otherwise, set cull. unknownbug
        go.GetComponentsInChildren(false, _RendersList);
        foreach (var render in _RendersList)
        {
            render.cull = !value;
        }

        _RendersList.Clear();
        //Refresh the material's Cliprect
        go.GetComponentsInChildren(false, _tempGraphics);
        foreach (var render in _tempGraphics)
        {
            render.SetMaterialDirty();
        }

        _tempGraphics.Clear();
    }

    /// <summary>
    /// 设置UI显隐
    /// </summary>
    /// <param name="go"></param>
    /// <param name="value"></param>
    public static void SetUIActive(this GameObject go, bool value)
    {
        //if active false, must set active.
        if (!go.activeSelf)
        {
            go.SetActive(value);
            return;
        }

        //last way, set alpha.
        //the performance of the scheme setting alpha was one third of that of direct GameObject.SetActive 
        var group = go.GetOrAddComponent<CanvasGroup>();
        group.alpha = value ? 1f : 0f;
        group.blocksRaycasts = value;
    }


    public static void SetActiveSelf(this GameObject gameObject, bool real)
    {
        if (real && !gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!real && gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }


    public static GameObject FindChild(this GameObject gameObject, string name, GameObject root = null)
    {
        if (root == null)
        {
            root = gameObject;
        }

        var res = root.transform.Find(name);
        if (res != null)
            return res.gameObject;

        for (int i = 0; i < root.transform.childCount; ++i)
        {
            var child = root.transform.GetChild(i);
            var ob = FindChild(gameObject, name, child.gameObject);
            if (ob) return ob;
        }

        return null;
    }


    public static T FindChild<T>(this GameObject gameObject, string name, GameObject root = null)
    {
        GameObject child = FindChild(gameObject, name, root);
        if (child != null)
        {
            T componenet = child.GetComponent<T>();
            if (componenet != null)
            {
                return componenet;
            }
            else
            {
                LogHelper.Warning(string.Format("{0} is not has {1} component", child.name, typeof(T).ToString()));
                return componenet;
            }
        }
        else
        {
            return default(T);
        }
    }


    public static void SetLayerRecursively(this GameObject go, int layer, bool ignorEffect = false)
    {
        if (ignorEffect && (go.CompareTag("Effect") || go.GetComponent<ParticleSystem>() != null))
        {
            return;
        }

        go.layer = layer;

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform trans = go.transform.GetChild(i);
            if (ignorEffect && (go.CompareTag("Effect") || go.GetComponent<ParticleSystem>() != null))
            {
                continue;
            }

            SetLayerRecursively(trans.gameObject, layer, ignorEffect);
        }
    }


    public static void SetTagRecusively(this GameObject go, string tag, string exclude = null)
    {
        if (!string.IsNullOrEmpty(exclude))
        {
            if (!go.CompareTag(exclude))
            {
                go.tag = tag;
            }
        }
        else
        {
            go.tag = tag;
        }

        for (int i = 0; i < go.transform.childCount; i++)
        {
            Transform trans = go.transform.GetChild(i);
            SetTagRecusively(trans.gameObject, tag, exclude);
        }
    }


    /// <summary>
    /// 递归查找对象下第一个名字是name的的节点， 返回GameObject
    /// </summary>
    public static GameObject FindRecursively(this GameObject root, string name, bool isPath = false)
    {
        if (isPath)
        {
            Transform _tra = root.transform.Find(name);


            if (_tra != null)
            {
                return _tra.gameObject;
            }
            else
            {
                LogHelper.Warning("Path:" + name + "is not found in" + root.name);
            }

            return null;
        }

        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.transform.childCount; i++)
        {
            Transform trans = root.transform.GetChild(i);
            GameObject obj = FindRecursively(trans.gameObject, name);

            if (obj != null)
            {
                return obj;
            }
        }

        return null;
    }


    public static void ClearPRS(this Transform t)
    {
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;
        t.localScale = Vector3.one;
    }


    public static void AttachTo(this Transform t, Transform parent, bool clearPRS = true)
    {
        t.SetParent(parent);
        if (clearPRS) t.ClearPRS();
    }


    public static Transform FindTag(this Transform trans, string tag)
    {
        if (trans.CompareTag(tag)) return trans;
        for (int i = 0; i < trans.childCount; i++)
        {
            Transform childTag = FindTag(trans.GetChild(i), tag);
            if (childTag != null) return childTag;
        }

        return null;
    }


    /// <summary>
    /// 递归查找对象下第一个名字是name的的节点
    /// </summary>
    public static Transform FindRecursively(this Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (int i = 0; i < root.transform.childCount; ++i)
        {
            var child = root.transform.GetChild(i);
            Transform t = FindRecursively(child, name);

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }


    /// <summary>
    /// 递归查找对象下第一个包含name的节点
    /// </summary>
    public static Transform FindRecursivelyLike(this Transform root, string name)
    {
        if (root.name.Contains(name))
        {
            return root;
        }

        for (int i = 0; i < root.transform.childCount; ++i)
        {
            var child = root.transform.GetChild(i);
            Transform t = FindRecursivelyLike(child, name);

            if (t != null)
            {
                return t;
            }
        }

        return null;
    }


    public static List<Transform> FindChildsLike(this Transform root, string name)
    {
        List<Transform> temp = new List<Transform>();
        for (int i = 0; i < root.transform.childCount; ++i)
        {
            var child = root.transform.GetChild(i);
            if (child.name.Contains(name))
            {
                temp.Add(child);
            }
        }

        return temp;
    }


    public static T FindChild<T>(this Transform root, string name)
    {
        Transform child = root.Find(name);
        if (child != null)
        {
            T componenet = child.GetComponent<T>();
            if (componenet != null)
            {
                return componenet;
            }
            else
            {
                LogHelper.Warning(string.Format("{0} is not has {1} component", child.name, typeof(T).ToString()));
                return componenet;
            }
        }
        else
        {
            return default(T);
        }
    }


    public static int GetActiveChildCount(this Transform root)
    {
        int childCount = 0;
        for (int i = 0; i < root.childCount; i++)
        {
            if (root.GetChild(i).gameObject.activeSelf)
            {
                childCount++;
            }
        }

        return childCount;
    }
    
    
    public static void DelayDestroy(this Object obj, float delayTime = -1)
    {
        if (Application.isPlaying)
        {
            if (delayTime < 0)
                Object.Destroy(obj);
            else
                Object.Destroy(obj, delayTime);
        }
        else
            Object.DestroyImmediate(obj);
    }
    
    
    /// <summary>
    /// 强制刷新子节点 全部ContentSizeFitter
    /// </summary>
    public static void RefreshAllContentSizeFitter(this GameObject go)
    {
        ContentSizeFitter[] csfs = go.GetComponentsInChildren<ContentSizeFitter>();
        foreach (var item in csfs)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }
    }
    
    
    /// <summary>
    /// 移除所有子节点
    /// </summary>
    public static void RemoveAllChildren(this GameObject parent)
    {
        for (var i = parent.transform.childCount - 1; i >= 0; i--)
        {
            var transform = parent.transform.GetChild(i);
            if (transform != null && transform.gameObject != null)
            {
                GameObject.Destroy(transform.gameObject);
            }
        }
    }
    
    
    /// <summary>
    /// 置灰
    /// </summary>
    /// <param name="img"></param>
    /// <param name="val">true:灰化 false:原色</param>
    public static void Grey(this Image img, bool val = true)
    {
        if (val)
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        else
            img.color = new Color(img.color.r, img.color.g, img.color.b, 1);
    }

    
    public static byte[] EncodeToTGA(this Texture2D tex, bool hasAlpha)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(stream);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)2);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((byte)0);
        bw.Write((short)0);
        bw.Write((short)0);
        bw.Write((short)tex.width);
        bw.Write((short)tex.height);
        bw.Write((byte)(hasAlpha ? 32 : 24));
        bw.Write((byte)0);
        for (int i = 0; i < tex.height; i++)
        {
            for (int j = 0; j < tex.width; j++)
            {
                Color clr = tex.GetPixel(j, i);
                byte r = (byte)(clr.r * 255);
                byte g = (byte)(clr.g * 255);
                byte b = (byte)(clr.b * 255);
                byte a = (byte)(clr.a * 255);
                bw.Write(b);
                bw.Write(g);
                bw.Write(r);
                if (hasAlpha) bw.Write(a);
            }
        }

        return stream.ToArray();
    }
    
    
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopyByReflection<T>(this T obj)
    {
        if (obj is string || obj.GetType().IsValueType)
            return obj;

        var retval = Activator.CreateInstance(obj.GetType());
        var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        for (var i = 0; i < fields.Length; i++)
        {
            try
            {
                fields[i].SetValue(retval, DeepCopyByReflection(fields[i].GetValue(obj)));
            }
            catch
            {
                // ignored
            }
        }
        return (T)retval;
    }
}