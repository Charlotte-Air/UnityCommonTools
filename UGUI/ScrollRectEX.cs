using System;
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollRectEX : ScrollRect
{
    public Action verticalTopUpAction;
    private bool startCheckverticalTopUpAction;

    public Action verticalButtomDownAction;
    private bool startCheckverticalButtomDownAction;


    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        lasrVerticalNormalizedPosition = verticalNormalizedPosition;
        if (base.verticalNormalizedPosition > 1)
        {
            startCheckverticalTopUpAction = true;
        }
        else
        {
            startCheckverticalTopUpAction = false;
        }

        if (base.verticalNormalizedPosition <0)
        {
            startCheckverticalButtomDownAction = true;
        }
        else
        {
            startCheckverticalButtomDownAction = false;
        }
        
    }

    private float lasrVerticalNormalizedPosition;
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
 
        if (startCheckverticalTopUpAction&&
            verticalNormalizedPosition- lasrVerticalNormalizedPosition > 0.1f )

        {
            if (verticalTopUpAction != null)
                verticalTopUpAction();
            startCheckverticalTopUpAction = false;
            verticalNormalizedPosition = 1;
        }

        if (startCheckverticalButtomDownAction &&
            verticalNormalizedPosition - lasrVerticalNormalizedPosition < -0.1f)

        {
            if (verticalButtomDownAction != null)
                verticalButtomDownAction();
            startCheckverticalButtomDownAction = false;
            verticalNormalizedPosition = 0;
        }

        lasrVerticalNormalizedPosition = verticalNormalizedPosition;
    }
}
