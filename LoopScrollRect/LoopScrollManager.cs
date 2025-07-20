using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class LoopScrollManager
{
    public delegate void LoopScrollCallBack(GameObject item, int index, bool bRefresh = false);
    public delegate void DespawnCallBack(Transform item, int index);
    public delegate GameObject SpawnCallBack(int index);
    public delegate void MoveCallBack(float value);
    public delegate void ScrollPageEndCallBack();

    public static void Start(LoopScrollRect component, int totalCount, LoopScrollCallBack callBack, DespawnCallBack despawnCall, SpawnCallBack spawnCall, MoveCallBack moveCall = null)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect == null) return;
        scrollRect.ClearCells();

        scrollRect.totalCount = totalCount;
        scrollRect.m_callBack = callBack;
        scrollRect.m_despawnCallBack= despawnCall;
        scrollRect.m_spawnCallBack = spawnCall;
        scrollRect.m_moveCallback = moveCall;

        component.enabled = true;

        scrollRect.RefillCells();
    }

    public static void StartFromEnd(LoopScrollRect component, int totalCount, LoopScrollCallBack callBack, DespawnCallBack despawnCall, SpawnCallBack spawnCall, MoveCallBack moveCall = null)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect == null) return;
        scrollRect.ClearCells();

        scrollRect.totalCount = totalCount;
        scrollRect.m_callBack = callBack;
        scrollRect.m_despawnCallBack = despawnCall;
        scrollRect.m_spawnCallBack = spawnCall;
        scrollRect.m_moveCallback = moveCall;

        component.enabled = true;

        scrollRect.RefillCellsFromEnd();
    }

    public static void Clear(LoopScrollRect component)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.ClearCells();
            scrollRect.enabled = false;
        }
    }

    /// <summary>
    /// 刷新
    /// </summary>
    /// <param name="component"></param>
    /// <param name="totalCount"></param>
    public static void RefreshData(LoopScrollRect component, int totalCount = -1, bool bSetRefresh = true)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            if (totalCount != -1)
            {
                scrollRect.totalCount = totalCount;
            }

            scrollRect.RefreshCells(totalCount != -1, bSetRefresh);
        }
    }

    public static void RefreshDataFromEnd(LoopScrollRect component, int totalCount = -1)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            if (totalCount != -1)
            {
                scrollRect.totalCount = totalCount;
            }

            scrollRect.RefillCellsFromEnd();
        }
    }

    /// <summary>
    /// 跳转
    /// </summary>
    /// <param name="component"></param>
    /// <param name="totalCount"></param>
    public static void GotoIndex(LoopScrollRect component, int index, int totalCount = -1)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.StopMovement();

            if (totalCount != -1)
            {
                scrollRect.totalCount = totalCount;
            }

            scrollRect.RefillCells(index);
        }
    }

    public static void GotoIndexWithSpeed(LoopScrollRect component, int index, int speed, int totalCount = -1)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.StopMovement();

            if (totalCount != -1)
            {
                scrollRect.totalCount = totalCount;
            }

            scrollRect.SrollToCell(index, speed);
        }
    }

    public static void GotoNextPage(LoopScrollRect component, int speed, ScrollPageEndCallBack callback)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.ScrollToNextPage(speed,callback);
        }
    }
    
    public static void GotoPrewPage(LoopScrollRect component, int speed, ScrollPageEndCallBack callback)
    {
        LoopScrollRect scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.StopMovement();
            scrollRect.ScrollToPrewPage(speed, callback);
        }
    }

    public static void Start(UIVerticalCollapseScroll component, int totalCount, Action<GameObject, CollapseData> refreshCallBack, Action<Transform> despawnCall, Func<CollapseData, GameObject> spawnCall)
    {
        UIVerticalCollapseScroll CG = component;
        if (CG == null) return;
        CG.ClearCells();
        CG.totalCount = totalCount;
        CG.refreshCall = refreshCallBack;
        CG.despawnCall = despawnCall;
        CG.spawnCall = spawnCall;
        CG.enabled = true;
        CG.GenRootData(totalCount);
        CG.RefillCells();
    }


    public static void Clear(UIVerticalCollapseScroll component)
    {
        UIVerticalCollapseScroll CG = component;
        if (CG != null)
        {
            CG.ClearCells();
            CG.ClearData();
            CG.enabled = false;
        }
    }

    public static void GotoIndex(UIVerticalCollapseScroll component, List<int> indexWithCountPaire, int offset = 0)
    {
        UIVerticalCollapseScroll scrollRect = component;
        if (scrollRect != null)
        {
            scrollRect.StopMovement();

            scrollRect.GotoIndex(indexWithCountPaire, offset);
        }
    }
}
