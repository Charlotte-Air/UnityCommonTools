using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GridSizeFitter : MonoBehaviour
{
    System.Action fitCallBack = null;
    public Transform m_reference;

    public GridLayoutGroup m_content;

    //private bool m_bFit = false;
    private Vector2 m_size = Vector2.zero;

    public bool _mBInitInStart = true;
    //void LateUpdate()
    //{
    //    if (!m_bFit)
    //    {
    //        m_bFit = true;

    //        RectTransform refRect = m_reference.GetComponent<RectTransform>();
    //        RectTransform myRect = this.GetComponent<RectTransform>();
    //        if (refRect != null && myRect != null)
    //        {
    //            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, refRect.rect.width);
    //            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, refRect.rect.height);

    //            m_size = refRect.rect.size;

    //            if (m_size.x > 0)
    //            {
    //                m_bFit = true;
    //                FitGrid();
    //            }
    //        }
    //    }
    //}
    void Start()
    {
        if (_mBInitInStart)
        {
            StartFit();
        }
        if(fitCallBack != null)
        {
            fitCallBack();
            fitCallBack = null;

        }
    }

    public void StartFit()
    {
        fitCallBack = () =>
        {
            StartCoroutine(StartFitIE());
        };
    }


    private IEnumerator StartFitIE()
    {
        yield return 1;
        RectTransform refRect = m_reference.GetComponent<RectTransform>();
        RectTransform myRect = this.GetComponent<RectTransform>();
        if (refRect != null && myRect != null)
        {
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, refRect.rect.width);
            myRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, refRect.rect.height);

            m_size = refRect.rect.size;

            if (m_size.x > 0)
            {
                FitGrid();
            }
        }
    }

    private void FitGrid()
    {

        if (m_content.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            float width = m_content.cellSize.x * m_content.constraintCount + m_content.spacing.x * (m_content.constraintCount - 1);
            if (width > m_size.x)
            {
                m_content.constraintCount -= 1;
                FitGrid();
            }
            else
            {
                if (GetComponent<LoopScrollRect>() != null)
                {
                    GetComponent<LoopScrollRect>().enabled = true;
                }
            }
        }
    }
}
