using System;
using UnityEngine;
using Framework.Manager;
using UnityEngine.EventSystems;

public class EventListener : EventTrigger
{
    public delegate void VoidDelegate<T>(T t);
    public VoidDelegate<PointerEventData> onUp;
    public VoidDelegate<BaseEventData> onCancel;
    public VoidDelegate<PointerEventData> onDown;
    public VoidDelegate<PointerEventData> onDrag;
    public VoidDelegate<PointerEventData> onClick;
    public VoidDelegate<PointerEventData> onEndDrag;
    public VoidDelegate<PointerEventData> onStartDrag;
    
    public override void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData);
    public override void OnCancel(BaseEventData eventData) => onCancel?.Invoke(eventData);
    public override void OnPointerUp(PointerEventData eventData) => onUp?.Invoke(eventData);
    public override void OnEndDrag(PointerEventData eventData) => onEndDrag?.Invoke(eventData);
    public override void OnPointerDown(PointerEventData eventData) => onDown?.Invoke(eventData);
    public override void OnPointerClick(PointerEventData eventData) => onClick?.Invoke(eventData);
    public override void OnBeginDrag(PointerEventData eventData) => onStartDrag?.Invoke(eventData);
    
    public static void TouchEnd(GameObject obj, VoidDelegate<PointerEventData> ac) => GetEventListener(obj).onUp = ac;
    public static void TouchMove(GameObject obj, VoidDelegate<PointerEventData> ac) => GetEventListener(obj).onDrag = ac;
    public static void TouchStart(GameObject obj, VoidDelegate<PointerEventData> ac) => GetEventListener(obj).onDown = ac;
    public static void TouchCancel(GameObject obj, VoidDelegate<BaseEventData> ac) => GetEventListener(obj).onCancel = ac;
    public static void TouchMoveEnd(GameObject obj, VoidDelegate<PointerEventData> ac) => GetEventListener(obj).onEndDrag = ac;
    public static void TouchMoveStart(GameObject obj, VoidDelegate<PointerEventData> ac) => GetEventListener(obj).onStartDrag = ac;
    
    public static EventListener GetEventListener(GameObject go)
    {
        var listener = go.GetComponent<EventListener>();
        if (listener == null)
            listener = go.AddComponent<EventListener>();
        return listener;
    }
    
    public static void LongTouch(GameObject obj, string timerKey, Func<bool> intervalCallback, float startTime = 0.5f, float intervalTime = 1f, Action finishCallback = null)
    {
        var ispress = false;
        var clickOneTime = true;
        var timerKey1 = timerKey;
        var timerKey2 = $"{timerKey}_2";
        GetEventListener(obj).onDown = eve =>
        {
            ispress = true;
            clickOneTime = true;
            var returnBool = false;
            TimerManager.Instance.Destroy(timerKey1);
            TimerManager.Instance.AddTimer(timerKey1, startTime, (args =>
            {
                if (!ispress)
                    return;
                
                clickOneTime = false;
                TimerManager.Instance.Destroy(timerKey2);
                TimerManager.Instance.AddTimerRepeat(timerKey2, intervalTime, (objects =>
                {
                    if (!ispress || returnBool)
                    {
                        finishCallback?.Invoke();
                        TimerManager.Instance.Destroy(timerKey2);
                        TimerManager.Instance.Destroy(timerKey1);
                        return;
                    }
                    returnBool = intervalCallback();
                }));
            }));
        };

        GetEventListener(obj).onUp = eve =>
        {
            ispress = false;
            if (clickOneTime)
                finishCallback?.Invoke();
        };
        
        GetEventListener(obj).onCancel = eve =>
        {
            ispress = false;
            if (clickOneTime)
                finishCallback?.Invoke();
        };
    }
}