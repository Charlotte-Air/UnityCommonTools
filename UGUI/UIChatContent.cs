using UnityEngine;
using UnityEngine.UI;

namespace Soda
{
    public class UIChatContent : ScrollRect
    {
        public delegate void SetDataCallBack(GameObject item, int index);
        public delegate GameObject SpawnItemCallBack(int index);
        public delegate void DespawnItemCallBack(GameObject item, int index);

        private SetDataCallBack m_onShow;
        private SpawnItemCallBack m_onSpawn;
        private DespawnItemCallBack m_onDespawn;

        private bool m_autoScrollToEnd = false;
        //设置是否需要在内容变动时自动滚动到底部
        public bool AutoScrollToEnd
        {
            get
            {
                return m_autoScrollToEnd; 
            }
            set
            {
                m_autoScrollToEnd = value;
                if (m_autoScrollToEnd)
                {
                    ScrollToBottom();
                }
            }
        }
        private Bounds viewBounds;
        //可视区域第一个实例的编号
        protected int m_StartIndex;
        //可视区域最后一个实例的编号
        protected int m_EndIndex;

        public int totalCount;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="onShow">描述对应元素的回调</param>
        /// <param name="onSpawn">加载对应元素的回调</param>
        /// <param name="onDespawn">销毁对应元素的回调</param>
        public void Init(SetDataCallBack onShow, SpawnItemCallBack onSpawn, DespawnItemCallBack onDespawn)
        {
            m_onShow = onShow;
            m_onSpawn = onSpawn;
            m_onDespawn = onDespawn;
            m_StartIndex = 0;
            m_EndIndex = 0;
            totalCount = 0;
        }
        /// <summary>
        /// 新增元素
        /// </summary>
        /// <param name="index">第一个新增元素所在位置</param>
        /// <param name="count">新增元素个数</param>
        public void AddRows(int index, int count = 1)
        {
            StopMovement();
            for (int i = 0; i < count; i++)
            {
                if (IsInRange(index + i))
                {
                    SpawnItem(index + i);
                    totalCount++;
                }
            }
            UpdateContent();
            if (m_autoScrollToEnd)
                ScrollToBottom();
        }
        /// <summary>
        /// 刷新元素
        /// </summary>
        /// <param name="index">第一个刷新元素所在位置</param>
        /// <param name="count">刷新元素个数</param>
        public void RefreshRows(int index, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                ShowItem(index + i, null);
            }
        }
        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="index">第一个删除元素所在位置</param>
        /// <param name="count">删除元素个数</param>
        public void DelRows(int index, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                DespawnItem(index + i);
                totalCount--;
            }
            UpdateContent();
        }

        public void CleanAll()
        {
            while(content.childCount != 0)
            {
                DespawnItem(0);
            }

            m_StartIndex = 0;
            m_EndIndex = 0;
            totalCount = 0;
        }

        /// <summary>
        /// 滚动到指定位置元素
        /// </summary>
        /// <param name="index">指定元素的位置</param>
        public void ScrollToIndex(int index)
        {
            //TODO smooth move
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
            int i = index - m_StartIndex;
            RectTransform item = (RectTransform)this.content.GetChild(i);
            Rect contentRect = GetContentRect();
            Rect viewRect = GetViewRect();
            float normalizePos = (0 - item.anchoredPosition.y - item.rect.height / 2) / (contentRect.height - viewRect.height);
            this.verticalNormalizedPosition = 1 - normalizePos;
        }

        /// <summary>
        /// 滚动到底部
        /// </summary>
        public void ScrollToBottom()
        {
            //TODO smooth move
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
            this.verticalNormalizedPosition = 0;
        }

        protected Rect GetContentRect()
        {
            if (this.content != null)
            {
                return this.content.rect;
            }

            return Rect.zero;
        }
        protected Rect GetViewRect()
        {
            if (this.viewport != null)
            {
                return this.viewport.rect;
            }

            return Rect.zero;
        }
        protected bool IsInRange(int index)
        {
            //TODO 检查新增元素是否在可视区域
            //if (index <= m_StartIndex)
            //{
            //    if(GetViewBounds().Intersects(GetItemBounds(index)))
            //    {

            //    }
            //}

            return true;
        }
        protected void UpdateContent()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.content);
        }
         
        protected GameObject SpawnItem(int index)
        {
            GameObject item = m_onSpawn(index);
            if (item == null)
            {
                Debug.LogFormat("UIChatContent spwan item error:the index {0}", index);
                //TODO spwam default item
            }
            else
            {
                ShowItem(index, item);
                item.transform.SetParent(this.content, false);
                if (index - m_StartIndex < totalCount)
                    item.transform.SetSiblingIndex(index - m_StartIndex);
            }
            return item;
        }
        protected void ShowItem(int index, GameObject item)
        {
            if (item == null)
            {
                int i = index - m_StartIndex;
                item = this.content.GetChild(i).gameObject;
            }
            m_onShow(item, index);
        }
        protected void DespawnItem(int index)
        {
            int i = index - m_StartIndex;
            GameObject item = this.content.GetChild(i).gameObject;
            m_onDespawn(item, index);
        }
        protected Bounds GetViewBounds()
        {
            if(viewBounds == null)
            {
                viewBounds = new Bounds(viewport.position, viewport.sizeDelta);
            }
            return viewBounds;
        }
        protected Bounds GetItemBounds(int index)
        {
            int i = index - m_StartIndex;
            RectTransform item = (RectTransform)content.GetChild(i);
            Bounds b = new Bounds(item.position, item.sizeDelta);
            return b;
        }
    }
}