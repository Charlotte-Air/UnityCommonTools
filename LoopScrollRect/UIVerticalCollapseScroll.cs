using System;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/UIVerticalCollapseScroll", 55)]
    public class UIVerticalCollapseScroll : LoopVerticalScrollRect
    {
        public Action<Transform> despawnCall;
        public Func<CollapseData, GameObject> spawnCall;
        public Action<GameObject, CollapseData> refreshCall;
        private HashSet<CollapseData> repeatCheckList = new HashSet<CollapseData>();//辅助检查重复项
        private List<CollapseData> showDataList = new List<CollapseData>();//实际显示的item,按照顺序排列的数据
        
        protected override void Awake()
        {
            base.Awake();
            m_callBack = OnScrollRefresh;
            m_despawnCallBack = OnScrollDespawn;
            m_spawnCallBack = OnScrollSpawn;
        }

        
        private void OnScrollRefresh(GameObject go, int index, bool bRefresh = false)
        {
            CollapseData itemData = showDataList[index];
            UICollapseElement ele = go.GetOrAddComponent<UICollapseElement>();
            ele.BindData(itemData);
            refreshCall?.Invoke(go, itemData);
        }

        
        private GameObject OnScrollSpawn(int index)
        {
            CollapseData itemData = showDataList[index];
            GameObject go = spawnCall?.Invoke(itemData);
            go.GetComponent<UICollapseElement>().scroll = this;
            return go;

        }
        
        
        private void OnScrollDespawn(Transform trans, int index)
        {
            UICollapseElement ele = trans.gameObject.GetComponent<UICollapseElement>();
            if (ele != null)
            {
                ele.UnbindData();
            }
            despawnCall?.Invoke(trans);
        }
        
        
        public void NoticeChildCntChange(CollapseData data, int count, bool foucus = true)
        {
            if (data.Children.Count == count)
                return;
            
            if (count == 0)
            {
                for (int i = 0; i < data.Children.Count; i++)
                {
                    showDataList.Remove(data.Children[i]);
                    UIVerticalCollapseScroll.Put(data.Children[i]);
                }
                data.Children.Clear();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    CollapseData tempData = UIVerticalCollapseScroll.Get();
                    tempData.Parent = data;
                    tempData.DepthIndex.Clear();
                    tempData.DepthIndex = new List<int>(data.DepthIndex.ToArray());
                    tempData.DepthIndex.Add(i);
                    data.Children.Add(tempData);
                }
            }

            RebuildShowDataList();
            //int jumpIndex = count == 0 ? -1 : FindJumpIndex(data);
            int jumpIndex = FindJumpIndex(data);
            data.IsOnFocus = foucus;
            if (foucus)
            {
                SetFocusData(data);
            }
            totalCount = showDataList.Count;
            RefreshCells(true, true);

            if (jumpIndex != -1)
            {
                StopMovement();
                RefillCells(jumpIndex);
                // if (count == 0)
                // {
                //     RefillCells(jumpIndex);
                // }
                // else
                // {
                //     SrollToCell(jumpIndex, showDataList.Count * 120);
                // }
            }
        }

        
        private int FindJumpIndex(CollapseData data)
        {
            int index = -1;
            for (int i = 0; i < showDataList.Count; i++)
            {
                if (showDataList[i] == data)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }

        
        private void RebuildShowDataList()
        {
            List<CollapseData> newList = new List<CollapseData>();
            repeatCheckList.Clear();
            for (int i = 0; i < showDataList.Count; i++)
            {
                CollapseData temp = showDataList[i];
                temp.IsOnFocus = false;
                if (repeatCheckList.Contains(temp))
                {
                    continue;
                }
                else
                {
                    newList.Add(temp);
                    repeatCheckList.Add(temp);
                    for (int j = 0; j < temp.Children.Count; j++)//这边就偷懒不递归只写两级菜单了
                    {
                        temp.Children[j].IsOnFocus = false;
                        if (repeatCheckList.Contains(temp.Children[j]))
                        {
                            continue;
                        }
                        else
                        {
                            newList.Add(temp.Children[j]);
                            repeatCheckList.Add(temp.Children[j]);
                        }
                    }
                }
            }
            showDataList = newList;
        }

        
        public void GenRootData(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CollapseData virtualData = UIVerticalCollapseScroll.Get();
                virtualData.IsOnFocus = false;
                virtualData.Parent = null;
                virtualData.DepthIndex.Clear();
                virtualData.DepthIndex.Add(i);
                showDataList.Add(virtualData);
            }
        }
        
        
        /// <summary>
        /// 获取当前选中的数据
        /// </summary>
        /// <returns></returns>
        public CollapseData GetFocusDataItem()
        {
            return currentFocusData;
        }
        
        
        public void ClearFocusState()
        {
            for (int i = 0; i < showDataList.Count; i++)
            {
                showDataList[i].ChangeFocus(false);
            }
        }
        
        
        CollapseData currentFocusData = null;
        public void SetFocusData(CollapseData data)
        {
            currentFocusData = data;
        }
        
        
        public void ClearFocusState(CollapseData exceptData)
        {
            for (int i = 0; i < showDataList.Count; i++)
            {
                if (showDataList[i] != exceptData)
                {
                    showDataList[i].ChangeFocus(false);
                }
            }
        }
        
        
        public void ClearData()
        {
            for (int i = 0; i < showDataList.Count; i++)
            {
                if (showDataList[i].DepthIndex.Count == 1)
                {
                    UIVerticalCollapseScroll.Put(showDataList[i]);
                }
            }
            showDataList.Clear();
            repeatCheckList.Clear();
            currentFocusData = null;
        }

        
        /// <summary>
        /// 当前层index,当前index子物体总数量，重复这个
        /// </summary>
        /// <param name="indexWithCountPaire"></param>
        public void GotoIndex(List<int> indexWithCountPaire, int offset = 0)
        {
            int len = indexWithCountPaire.Count;
            CollapseData tempData = null;
            for (int i = 0; i < showDataList.Count; i++)
            {
                if (showDataList[i].DepthIndex.Count == 1 && showDataList[i].DepthIndex[0] == indexWithCountPaire[0])
                {
                    tempData = showDataList[i];
                    break;
                }
            }
            if (tempData != null)
            {
                int cnt = indexWithCountPaire[1];
                bool hasNext = len > 2 && cnt > 0;
                NoticeChildCntChange(tempData, cnt, !hasNext);
                if (hasNext)
                {
                    CollapseData tempSubData = null;
                    for (int i = 0; i < tempData.Children.Count; i++)
                    {
                        if (tempData.Children[i].DepthIndex[1] == indexWithCountPaire[2])
                        {
                            tempSubData = tempData.Children[i];
                            break;
                        }
                    }
                    if (tempSubData != null)
                    {

                        int jumpIndex = FindJumpIndex(tempSubData) + offset;
                        if (jumpIndex >= 0)
                        {
                            StopMovement();
                            SrollToCell(jumpIndex, 1000);
                        }
                        tempSubData.ChangeFocus(true);
                        tempSubData.Parent?.ChangeFocus(true);
                    }
                }
            }
        }

        
        private void Destroy()
        {
            ClearData();
            currentFocusData = null;
        }

        
        #region 这边用来回收数据
        
        public static Queue<CollapseData> pool = new Queue<CollapseData>();
        
        public static void Put(CollapseData unuseData)
        {
            unuseData.OnUnuse();
            pool.Enqueue(unuseData);
        }
        
        
        public static CollapseData Get()
        {
            if (pool.Count > 0)
            {
                return pool.Dequeue();
            }
            else
            {
                return new CollapseData();
            }
        }
        
        #endregion
    }


    public class CollapseData
    {
        public bool IsOnFocus = false;//是否被选中
        public CollapseData Parent;//树状结构中的父亲
        public List<CollapseData> Children = new List<CollapseData>();//树状结构中的孩子
        public List<int> DepthIndex = new List<int>();//深度数据，例如[2,4,6] 表示层级为3，第一层级的第二个节点，第二层级的第四个节点，第三层级的第六个节点
        public UICollapseElement Element;//数据绑定的Element对象
        
        public string GetHierarchyStr()//测试用，打印层级
        {
            return string.Join(",", DepthIndex);
        }
        
        public void OnUnuse()
        {
            IsOnFocus = false;
            Element = null;
            Parent = null;
            for (int i = 0; i < Children.Count; i++)
            {
                UIVerticalCollapseScroll.Put(Children[i]);
            }
            Children.Clear();
            DepthIndex.Clear();
        }

        public void ChangeFocus(bool isFocus)
        {
            IsOnFocus = isFocus;
            if (Element != null)
            {
                Element.OnNoticeFocusStateChange();
            }
        }
    }
}