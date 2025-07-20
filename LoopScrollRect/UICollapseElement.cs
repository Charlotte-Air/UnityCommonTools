using UnityEngine.Events;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(LayoutElement))]
    public class UICollapseElement : MonoBehaviour
    {
        [HideInInspector]
        public UIVerticalCollapseScroll scroll;
        [SerializeField]
        [HideInInspector]
        private UnityEvent OnFocusStateChange = new UnityEvent();
        [SerializeField]
        private CollapseData groupData;//把数据放在element里，不用在业务外面传参数
        private RectTransform rect;
        private LayoutElement layout;
        
        private void Awake()
        {
            rect = gameObject.GetComponent<RectTransform>();
            layout = gameObject.GetComponent<LayoutElement>();
        }
        
        
        /// <summary>
        /// 设置尺寸
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(Vector2 size)
        {
            rect.sizeDelta = size;
            layout.preferredHeight = size.y;
        }
        
        
        /// <summary>
        /// 设置某一个节点被选中
        /// </summary>
        public void SetFocus()
        {
            if (groupData.IsOnFocus == true)
            {
                return;
            }
            groupData.ChangeFocus(true);
            if (scroll != null)
            {
                scroll.SetFocusData(groupData);
                scroll.ClearFocusState(groupData);
            }
        }
        
        
        /// <summary>
        /// 获取是否聚焦(被点击)的状态，用来初始化
        /// </summary>
        /// <returns></returns>
        public bool GetFocusState()
        {
            return groupData.IsOnFocus;
        }
        
        
        /// <summary>
        /// 获取当前item所在的index和层级
        /// </summary>
        /// <returns></returns>
        public List<int> GetDepthList()
        {
            return groupData.DepthIndex;
        }
        
        
        /// <summary>
        /// 获得当前被选中的item所在的index和层级
        /// </summary>
        public List<int> GetSelectedItemDepthList()
        {
            CollapseData node = scroll.GetFocusDataItem();
            if (node != null)
            {
                return node.DepthIndex;
            }
            return null;
        }
        
        
        /// <summary>
        /// 刷新子物体的数量
        /// </summary>
        /// <param name="count"></param>
        public void RefreshElements(int count)
        {
            if (scroll != null)
            {
                scroll.NoticeChildCntChange(groupData, count);
            }
            else
            {
                Debug.LogFormat("ShowElements_Error:collapseGroup == null");
            }
        }
        
        
        /// <summary>
        /// 获取子物体的数量
        /// </summary>
        /// <returns></returns>
        public int GetChildCount()
        {
            return groupData.Children.Count;
        }


        #region 这边由scroll控制
        
        public void BindData(CollapseData data)
        {
            OnFocusStateChange.RemoveAllListeners();
            data.Element = this;
            groupData = data;
        }

        
        public void UnbindData()
        {
            OnFocusStateChange.RemoveAllListeners();
            if (groupData != null)
            {
                groupData.Element = null;
                groupData = null;
            }
        }
        
        
        /// <summary>
        /// 通知数据变化
        /// </summary>
        public void OnNoticeFocusStateChange()
        {
            OnFocusStateChange?.Invoke();
        }
        
        #endregion
    }
}
