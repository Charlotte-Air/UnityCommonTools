using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class CommonUtils
{
    #region Common

    const int D_Width = 1920;
    const int D_Height = 1080;
    static bool isMatchHeight = true;

    public static float D_H_Ratio
    {
        get
        {
            if (isMatchHeight)
            {
                return (float)D_Height / Screen.height;
            }
            else
            {
                return (float)D_Width / Screen.width;
            }
        }
    }

    public static float canvasWidth
    {
        get
        {
            if (isMatchHeight)
            {
                return D_H_Ratio * Screen.width;
            }
            else
            {
                return D_Width;
            }
        }
    }

    public static float canvasHeight
    {
        get
        {
            if (isMatchHeight)
            {
                return D_Height;
            }
            else
            {
                return D_H_Ratio * Screen.height;
            }
        }
    }
    
    /// <summary>
    /// 屏幕坐标转世界坐标
    /// </summary>
    /// <param name="screenPosition"></param>
    /// <returns></returns>
    public static Vector3 GetScreenPosToWorldPos(Vector3 screenPosition)
    {
        Camera camera = GameObject.FindWithTag("UICamera")?.GetComponent<Camera>();
        if (camera == null)
        {
            Debug.LogError("UICamera is not exist.");
            return Vector3.zero;
        }
        screenPosition = new Vector3(screenPosition.x, screenPosition.y, Mathf.Abs(-camera.transform.position.z));
        Vector3 pos = camera.ScreenToWorldPoint(screenPosition);
        return pos;
    }

    
    

    /// <summary>
    /// ��ȡobj��Ļλ�� Screen Space - Camera ģʽ
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Vector2 GetGameObjectScreenPos(GameObject obj)
    {
        GameObject cameraNode = GameObject.Find("gmraiden/UICamera");
        Vector2 pos;
        if (cameraNode == null)
        {
            pos = GetGameObjectScreenPosForOverlay(obj);
        }
        else
        {
            Camera uiCamera = cameraNode.GetComponent<Camera>();
            pos = uiCamera.WorldToScreenPoint(obj.transform.position);
            pos *= D_H_Ratio;
        }

        return pos;
    }

    /// <summary>
    /// ��ȡobj��Ļλ�� Screen Space - Overlay ģʽ
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Vector2 GetGameObjectScreenPosForOverlay(GameObject obj)
    {
        // Overlay ģʽ�£�UGUI�������������Ļ����
        return obj.transform.position;
    }


    public static void SetItemMsgPosition(Hashtable itemiconData, GameObject itemNode)
    {
        RectTransform itemMsgRectTransform = itemNode.GetComponent<RectTransform>();
        var descText = itemNode.transform.Find("Image_Bg/Text_Desc1").GetComponent<Text>();
        RectTransform itemMsgRectChildTransform = itemNode.transform.Find("Image_Bg").GetComponent<RectTransform>();
        itemMsgRectChildTransform.sizeDelta =
            new Vector2(itemMsgRectChildTransform.sizeDelta.x, 188 + descText.preferredHeight);
        itemMsgRectTransform.sizeDelta = itemMsgRectChildTransform.sizeDelta;
        float itemiconX = (float)itemiconData["x"];
        float itemiconY = (float)itemiconData["y"];
        float itemiconW = (float)itemiconData["w"] / 2;
        float itemiconH = (float)itemiconData["h"] / 2;
        float itemMsgOffX = itemMsgRectTransform.rect.width;
        float itemMsgOffY = itemMsgRectTransform.rect.height;
        float screenOffX = canvasWidth / 2;
        float screenOffY = canvasHeight / 2;
        float x, y;
        if (itemiconX <= canvasWidth / 2)
        {
            x = itemiconX + itemMsgOffX / 2 + itemiconW - screenOffX + 10;
        }
        else
        {
            x = itemiconX - itemMsgOffX / 2 - itemiconW - screenOffX - 10;
        }

        if (itemiconY <= canvasHeight / 2)
        {
            y = itemiconY + itemMsgOffY / 2 + itemiconH - screenOffY;
        }
        else
        {
            y = itemiconY - itemMsgOffY / 2 - itemiconH - screenOffY;
        }

        itemMsgRectTransform.anchoredPosition = new Vector2(x, y);
    }

    public static void SetItemMsgPosForOverlay(Hashtable itemiconData, GameObject itemNode)
    {
        RectTransform itemMsgRectTransform = itemNode.GetComponent<RectTransform>();
        float itemiconX = (float)itemiconData["x"];
        float itemiconY = (float)itemiconData["y"];
        float itemiconW = (float)itemiconData["w"] / 2;
        float itemiconH = (float)itemiconData["h"] / 2;
        float itemMsgOffX = itemMsgRectTransform.rect.width;
        float itemMsgOffY = itemMsgRectTransform.rect.height;
        float screenOffX = canvasWidth / 2;
        float screenOffY = canvasHeight / 2;
        float x, y;
        x = -itemiconX - screenOffX;
        if (x <= screenOffX)
        {
            x = x - itemMsgOffX / 2 - itemiconW;
        }
        else
        {
            x = x + itemMsgOffX / 2 + itemiconW;
        }

        y = itemiconY;
        if (y >= screenOffY)
        {
            y = y - itemMsgOffY / 2 - itemiconH;
        }
        else
        {
            y = y + itemMsgOffY / 2 + itemiconH;
        }

        itemMsgRectTransform.anchoredPosition = new Vector2(x, y);
    }

    /// <summary>
    /// ǿ��ˢ��ָ�������µ�����������UI����
    /// </summary>
    /// <param name="go">ָ��������</param>
    public static void RefreshLayout(GameObject go)
    {
        ContentSizeFitter[] csfs = go.GetComponentsInChildren<ContentSizeFitter>();
        foreach (var item in csfs)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(item.GetComponent<RectTransform>());
        }
    }
    
    /// <summary>
    /// 将一个数各个位数提取出来
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    public static List<int> GetListByInt(int num)
    {
        List<int> list = new List<int>();
        while (num != 0)
        {
            list.Add(num % 10);
            num /= 10;
        }
        return list;
    }
    
    
    /// <summary>
    /// 深拷贝
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static T DeepCopyByReflection<T>(T obj)
    {
        if (obj is string || obj.GetType().IsValueType)
        {
            return obj;
        }
        object retval = Activator.CreateInstance(obj.GetType());
        FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++)
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

    #endregion

    
    #region Image Extension

    /// <summary>
    /// 
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

    #endregion

    
    #region Transform Extension

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

    #endregion

    
    #region GameObject Extension

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

    public static T GetOrAddComponent<T>(this GameObject go) where T : Component
    {
        T com = go.GetComponent<T>();
        if (null == com)
        {
            return go.AddComponent<T>();
        }

        return com;
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

    public static void SetActiveSelf(this GameObject gameObject, bool real)
    {
        if (real && !gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        if (!real && gameObject.activeInHierarchy)
            gameObject.SetActive(false);
    }

    public static void Destroy(Object obj, float t = -1)
    {
        if (Application.isPlaying)
        {
            if (t < 0)
            {
                Object.Destroy(obj);
            }
            else
            {
                Object.Destroy(obj, t);
            }
        }
        else
        {
            Object.DestroyImmediate(obj);
        }
    }

    #endregion
    
    
}