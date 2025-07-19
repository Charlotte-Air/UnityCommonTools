using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[AddComponentMenu("UI/UIObject3D")]
public class UIObject3D : MaskableGraphic
{
    public GameObject[] preGos;

    private Dictionary<string, GameObject> objects = new Dictionary<string, GameObject>();

    protected override void Start()
    {
        if (Application.isPlaying)
        {
            if (preGos != null && preGos.Length > 0)
            {
                for (int i = 0; i < preGos.Length; i++)
                {
                    AddObject(preGos[i]);
                }
            }
        }
    }

    protected override void OnDestroy()
    {
        objects.Clear();
    }

    public void AddObject(GameObject go, string name = null, int sortingOrder = -1)
    {
        string key = string.IsNullOrEmpty(name) ? go.name : name;

        if (objects.ContainsKey(key))
        {
            LogHelper.WarningFormat("add 3d object {0} to ui failed, object already exist.", key);
            return;
        }
        
        go.SetLayerRecursively(LayerMask.GetMask("UI")/*GameLayer.Layer_UI*/);
        if (go.transform.parent != transform)
        {
            go.transform.SetParent(transform, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
        }
        objects[key] = go;

        int so = sortingOrder == -1 ? canvas.sortingOrder + objects.Count : sortingOrder;

        SetSortingOrder(go, so);
    }

    public void RemoveObject(string name)
    {
        objects.Remove(name);
    }

    protected override void OnCanvasHierarchyChanged()
    {
        base.OnCanvasHierarchyChanged();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
    }

    protected void SetSortingOrder(GameObject go, int sortingOrder)
    {
        Renderer[] renders = go.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer render in renders)
        {
            Material mat = render.material;
            int rq = mat.renderQueue;
            mat.renderQueue = rq < 3000 ? 3000 : rq;
            mat.SetInt("_ZWrite", 0);
            render.sortingOrder = sortingOrder;
            render.material = mat;
        }
    }
}
