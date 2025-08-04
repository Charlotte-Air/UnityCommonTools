using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public  int soundID = 0;
    public void OnPointerDown(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 0.95f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (soundID == 0)
        {
            
        }
    }
}