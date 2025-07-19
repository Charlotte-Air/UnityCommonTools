using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUIObj : MonoBehaviour , IPointerDownHandler, IDragHandler, IPointerUpHandler,
    IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool reachTarget;
    public RectTransform canvas;          //得到canvas的ugui坐标
    private RectTransform imgRect;        //得到图片的ugui坐标

    public Action reachTargetAction;

    Vector2 startPos = new Vector2();
    Vector2 offset = new Vector2();    //用来得到鼠标和图片的差值
    Vector2 tempPos = new Vector2();    //用来得到鼠标和图片的差值
    public Vector3 target = new Vector3(99999,99999,0);
    public float nearDis = 5;
    void Start ()
    {
        imgRect = GetComponent<RectTransform>();
        startPos = imgRect.anchoredPosition;
        reachTarget = false;
    }

    //当鼠标按下时调用 接口对应  IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        imgRect.SetAsLastSibling();
        Vector2 mouseDown = eventData.position;    //记录鼠标按下时的屏幕坐标
        Vector2 mouseUguiPos = new Vector2();   //定义一个接收返回的ugui坐标
        //RectTransformUtility.ScreenPointToLocalPointInRectangle()：把屏幕坐标转化成ugui坐标
        //canvas：坐标要转换到哪一个物体上，这里img父类是Canvas，我们就用Canvas
        //eventData.enterEventCamera：这个事件是由哪个摄像机执行的
        //out mouseUguiPos：返回转换后的ugui坐标
        //isRect：方法返回一个bool值，判断鼠标按下的点是否在要转换的物体上
        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDown, eventData.enterEventCamera, out mouseUguiPos);
        if (isRect)   //如果在
        {
            //计算图片中心和鼠标点的差值
            offset = imgRect.anchoredPosition - mouseUguiPos;
        }
    }
    //当鼠标拖动时调用   对应接口 IDragHandler
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mouseDrag = eventData.position;   //当鼠标拖动时的屏幕坐标
        Vector2 uguiPos = new Vector2();   //用来接收转换后的拖动坐标
        //和上面类似
        bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, mouseDrag, eventData.enterEventCamera, out uguiPos);

        if (isRect) 
        {
            tempPos = offset + uguiPos;
            if (Vector2.Distance(tempPos, target) < nearDis)
            {
                tempPos = target;
            }
            //设置图片的ugui坐标与鼠标的ugui坐标保持不变
            imgRect.anchoredPosition = tempPos;
        }
    }
    //当鼠标抬起时调用  对应接口  IPointerUpHandler
    public void OnPointerUp(PointerEventData eventData)
    {
        offset = Vector2.zero;
        if (Vector2.Distance(imgRect.anchoredPosition, target) > nearDis)
        {
            imgRect.anchoredPosition = startPos;
        }
        else
        {
            if (reachTargetAction != null)
                reachTargetAction();
            reachTarget = true;
            imgRect.GetComponent<Image>().raycastTarget = false;
        }
    }
    //当鼠标结束拖动时调用   对应接口  IEndDragHandler
    public void OnEndDrag(PointerEventData eventData)
    {
        offset = Vector2.zero;
    }
    //当鼠标进入图片时调用   对应接口   IPointerEnterHandler
    public void OnPointerEnter(PointerEventData eventData)
    {
    }
    //当鼠标退出图片时调用   对应接口   IPointerExitHandler
    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
