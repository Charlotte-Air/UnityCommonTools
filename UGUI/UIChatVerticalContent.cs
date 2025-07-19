using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class UIChatVerticalContent : LoopChatVerticalScrollRect
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="onShow">描述对应元素的回调</param>
        /// <param name="onSpawn">加载对应元素的回调</param>
        /// <param name="onDespawn">销毁对应元素的回调</param>
        public void Init(LoopScrollManager.LoopScrollCallBack onShow, LoopScrollManager.SpawnCallBack onSpawn, LoopScrollManager.DespawnCallBack onDespawn, LoopScrollManager.MoveCallBack onMove)
        {
            ClearCells();

            m_callBack = onShow;
            m_spawnCallBack = onSpawn;
            m_despawnCallBack = onDespawn;
            m_moveCallback = onMove;
            this.enabled = true;
        }
        public void RefreshDataFromEnd(int totalCount)
        {
            this.totalCount = totalCount;
            RefillCellsFromEnd();
        }
        /// <summary>
        /// 新增元素
        /// </summary>
        /// <param name="index">第一个新增元素所在位置</param>
        /// <param name="count">新增元素个数</param>
        public void AddRows(int index, int count = 1)
        {
            totalCount += count;
            AddCells(index, count);
        }
        /// <summary>
        /// 刷新元素
        /// </summary>
        /// <param name="index">第一个刷新元素所在位置</param>
        /// <param name="count">刷新元素个数</param>
        public void RefreshRows(int index, int count = 1)
        {
            RefreshCells(index, count);
        }
        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="index">第一个删除元素所在位置</param>
        /// <param name="count">删除元素个数</param>
        public void DelRows(int index, int count = 1)
        {
            totalCount -= count;
            DelCells(index, count);
        }

        public void CleanAll()
        {
            ClearCells();
        }

        /// <summary>
        /// 滚动到指定位置元素
        /// </summary>
        /// <param name="index">指定元素的位置</param>
        public void ScrollToIndex(int index)
        {
            SrollToCell(index, 3000);
        }
        /// <summary>
        /// 滚动到底部
        /// </summary>
        public void ScrollToBottom(float speed = 6000)
        {
            // if (speed <= 0)
            //     speed = 2000000;
            // if (speed == -1)
            // {
            //     if (totalCount > 0)
            //     {
            //         RefillCells(totalCount - 1);
            //         Debug.Log("ScrollToBottom");
            //     }
            // }
            // else
            {
                if (totalCount > 0)
                    SrollToCell(totalCount - 1, speed);

            }

        }
    }
}