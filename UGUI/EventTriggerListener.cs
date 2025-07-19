using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventTriggerListener : EventTrigger{
	public delegate void CallDelegate (GameObject go, PointerEventData eventData);
	public CallDelegate onClick;
	public CallDelegate onDown;
	public CallDelegate onEnter;
	public CallDelegate onExit;
	public CallDelegate onUp;
	public CallDelegate onSelect;
	public CallDelegate onUpdateSelect;
	public CallDelegate onDrag;
 
	static public EventTriggerListener RegisterListener (GameObject go)
	{
		EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
		if (listener == null)
			listener = go.AddComponent<EventTriggerListener>();
		listener.enabled = true;
		
		return listener;
	}

	static public void UnregisterListener(GameObject go)
	{
		EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
		if (listener != null)
			listener.enabled = false;
	}
    
	public override void OnPointerClick(PointerEventData eventData)
	{
		if(onClick != null) 	
			onClick(gameObject, eventData);
	}
    
	public override void OnPointerDown (PointerEventData eventData)
	{
		if(onDown != null) 
			onDown(gameObject, eventData);
	}
    
	public override void OnPointerEnter (PointerEventData eventData)
	{
		if(onEnter != null) 
			onEnter(gameObject, eventData);
	}
    
	public override void OnPointerExit (PointerEventData eventData)
	{
		if(onExit != null) 
			onExit(gameObject, eventData);
	}
    
	public override void OnPointerUp (PointerEventData eventData)
	{
		if(onUp != null) 
			onUp(gameObject, eventData);
	}

	public override void OnDrag(PointerEventData eventData)
	{
		if (onDrag != null)
			onDrag(gameObject, eventData);
	}

	public override void OnSelect (BaseEventData eventData)
	{
		if(onSelect != null) 
			onSelect(gameObject, null);
	}
    
	public override void OnUpdateSelected (BaseEventData eventData)
	{
		if(onUpdateSelect != null) 
			onUpdateSelect(gameObject, null);
	}
}
