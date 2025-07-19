using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUI : MonoBehaviour , IPointerDownHandler, IDragHandler, IPointerUpHandler, IEndDragHandler
{
    public Action<bool> DragAction;
    private Vector2 startPos = new Vector2();
    public float offsetDis = 0;
    public float moveDis = 0;
    private RectTransform rect = null;
    private Vector2 pos = Vector2.zero;

    void Start()
    {
        rect = gameObject.GetComponent<RectTransform>();
        pos = rect.anchoredPosition;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        startPos = eventData.position;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        CheckDis(eventData);
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        CheckDis(eventData);
        rect.anchoredPosition = pos;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        startPos = Vector2.zero;
    }

    private void CheckDis(PointerEventData eventData)
    {
        if (startPos == Vector2.zero)
            return;
        
        Vector2 mouseDrag = eventData.position;
        float dis = startPos.x - mouseDrag.x;
        float absDis = Mathf.Abs(dis);

        if (moveDis != 0)
        {
            float move = Mathf.Lerp(0, moveDis, absDis / offsetDis);
            if (dis < 0)
                rect.anchoredPosition = new Vector2(pos.x + move, pos.y);
            else
                rect.anchoredPosition = new Vector2(pos.x - move, pos.y);
        }
        
        if (absDis >= offsetDis && DragAction != null)
        {
            DragAction.Invoke(dis > 0);
            startPos = Vector2.zero;
        }
    }
}
