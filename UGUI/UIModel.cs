using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIModel : MonoBehaviour
{
    public GameObject TargetModel;
    private GameObject Model;
    public Transform AttachTo;
    public Vector3 OffSet;
    public Vector3 Scale = Vector3.one;

    private List<GameObject> _clearWaitingList = new List<GameObject>();
    private int loadIndex = 0;
    private int panelType;
    private string UniqueKey;
    
    public void Init(int panelType)
    {
        this.panelType = panelType;
        UniqueKey = transform.GetHashCode().ToString();
        if (TargetModel != null)
        {
            GameObject model = Instantiate(TargetModel);
            if (model != null)
            {
                if (AttachTo != null) model.transform.SetParent(AttachTo, false);
                else model.transform.SetParent(transform,false);
                model.transform.localScale = Scale;
                model.transform.localPosition += OffSet;
                Model = model;
                _clearWaitingList.Add(Model);
                SetModel(Model);
            }
        }
    }

    private const string UICamera = "UICamera";
    private Canvas GetHighestCanvas()
    {
        Transform high = transform;
        while (high.parent != null && !high.parent.CompareTag(UICamera)) high = high.parent;
        return high == null ? null : high.GetComponent<Canvas>();
    }

    public void LoadModel(object createArgs, Transform parent = null, Action<GameObject> loadCall = null)
    {
        loadIndex++;
        Action<GameObject, object> call = (o, context) =>
        {
            if (o == null || context == null) return;
            //TODO commonshow回调多次
            if (null == this || (int)context != loadIndex)
            {
                return;
            }
            ClearOtherModel(o);
            Model = o;
            SetModel(Model);
            if(Model!= null) _clearWaitingList.Add(Model);
            if (loadCall != null) loadCall(o);
        };
    }

    public void SetModel(GameObject model)
    {
        model.SetLayerRecursively(LayerMask.GetMask("UI"));
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
        foreach (var render in renderers)
        {
            foreach (var mat in render.materials)
            {
                mat.renderQueue = 3000;
            }
        }
        UpdatePos();
        AttachUIDepth();
    }
    
    public void UpdatePos(string modelName = "")
    {

    }
    

    private void CheckParticleScalingMode()
    {
        ParticleSystem[] particles = Model.GetComponentsInChildren<ParticleSystem>();
        foreach (var particle in particles)
        {
            var particleMain = particle.main;
            particleMain.scalingMode = ParticleSystemScalingMode.Hierarchy;
        }
    }

    public void AttachUIDepth()
    {
        UIDepth depth = Model.GetOrAddComponent<UIDepth>();
        depth.MatchCanvas = MaskUtilities.FindRootSortOverrideCanvas(transform).GetComponent<Canvas>();
        depth.isMatchOrder = true;
        depth.maskable = true;
        depth.RecalculateMasking();
    }

    private void SetSortingOrder()
    {
        Canvas canvas = GetHighestCanvas();
        int sortingOrder = 1000;
        if (canvas != null) sortingOrder = canvas.sortingOrder;
        SetSortingOrder(sortingOrder);
    }

    public void SetSortingOrder(int order)
    {
        if (order != -1)
        {
            Renderer[] renderers = Model.GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                render.sortingOrder = order;
                foreach (var mat in render.materials)
                {
                    mat.renderQueue = 3000;
                }
            }
        }
        else
        {
            Renderer[] renderers = Model.GetComponentsInChildren<Renderer>();
            foreach (var render in renderers)
            {
                render.sortingOrder = 0;
                foreach (var mat in render.materials)
                {
                    mat.renderQueue = -1;
                }
            }
        }
    }

    public void ModelActive(bool active)
    {
        if(Model.activeSelf != active) Model.SetActive(active);
    }

    private void ClearOtherModel(GameObject except = null)
    {
        for (var i = 0; i < _clearWaitingList.Count; i++)
        {
            GameObject temp = _clearWaitingList[i];
            if (temp != null && temp != except)
            {
            }
        }
        _clearWaitingList.Clear();
    }

    public void DestoryModel()
    {
        if (Model == null) return;
        SetSortingOrder(-1);
    }
    
    private void OnDestroy()
    {
        ClearOtherModel();
        if (Model != null)
        {
        }
    }

    public GameObject GetModel()
    {
        return Model;
    }
}
