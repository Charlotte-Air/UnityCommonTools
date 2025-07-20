using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Loop Horizontal Scroll Rect", 50)]
    [DisallowMultipleComponent]
    public class LoopHorizontalScrollRect : LoopScrollRect
    {
        public int visibleStartItemIdx
        {
            get
            {
                if (content.childCount != 0)
                {
                    RectTransform oldItem = content.GetChild(0) as RectTransform;
                    int idx = Mathf.FloorToInt((-content.anchoredPosition.x + contentSpacing) / GetSize(oldItem));

                    if (idx >= 0)
                    {
                        return idx;
                    }
                }

                return 0;
            }
        }

        public int visibleEndItemIdx
        {
            get
            {
                if (content.childCount != 0)
                {
                    RectTransform oldItem = content.GetChild(0) as RectTransform;
                    int idx = Mathf.FloorToInt((content.rect.width + content.anchoredPosition.x - this.GetComponent<RectTransform>().rect.width + contentSpacing) / GetSize(oldItem));

                    if (idx >= 0)
                    {
                        return content.childCount - 1 - idx;
                    }
                }

                return 0;
            }
        }

        protected override float GetSize(RectTransform item)
        {
            float size = contentSpacing;
            if (m_GridLayout != null)
            {
                size += m_GridLayout.cellSize.x;
            }
            else
            {
                size += LayoutUtility.GetPreferredWidth(item);
            }
            return size;
        }

        protected override float GetDimension(Vector2 vector)
        {
            return -vector.x;
        }

        protected override Vector2 GetVector(float value)
        {
            return new Vector2(-value, 0);
        }

        protected override void Awake()
        {
            base.Awake();
            directionSign = 1;

//             GridLayoutGroup layout = content.GetComponent<GridLayoutGroup>();
//             if (layout != null && layout.constraint != GridLayoutGroup.Constraint.FixedRowCount)
//             {
//                 Debug.LogError("[LoopHorizontalScrollRect] unsupported GridLayoutGroup constraint");
//             }
        }

        protected override bool UpdateItems(Bounds viewBounds, Bounds contentBounds)
        {
            bool changed = false;
            if (viewBounds.max.x > contentBounds.max.x)
            {
                float size = NewItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x > contentBounds.max.x + totalSize)
                {
                    size = NewItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }
            else if (viewBounds.max.x < contentBounds.max.x - threshold)
            {
                float size = DeleteItemAtEnd(), totalSize = size;
                while (size > 0 && viewBounds.max.x < contentBounds.max.x - threshold - totalSize)
                {
                    size = DeleteItemAtEnd();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }

            if (viewBounds.min.x < contentBounds.min.x)
            {
                float size = NewItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x < contentBounds.min.x - totalSize)
                {
                    size = NewItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }
            else if (viewBounds.min.x > contentBounds.min.x + threshold)
            {
                float size = DeleteItemAtStart(), totalSize = size;
                while (size > 0 && viewBounds.min.x > contentBounds.min.x + threshold + totalSize)
                {
                    size = DeleteItemAtStart();
                    totalSize += size;
                }
                if (totalSize > 0)
                    changed = true;
            }
            return changed;
        }
    }
}