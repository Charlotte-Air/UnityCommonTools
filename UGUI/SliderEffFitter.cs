using System;
using UnityEngine;
using UnityEngine.UI;

public class SliderEffFitter : MonoBehaviour
{
    public Slider slider;
    public float r;
    private Vector2 temp = new Vector2(1,1);
    private RectTransform eff;

    public bool isShanBi = false;
    void Awake()
    {
        eff = (RectTransform)this.transform;
    }

    void Update()
    {
        if (slider == null || eff == null)
            return;
        if (isShanBi)
        {
            temp.x = -Mathf.Sin( Mathf.PI*0.5f * slider.value);
            temp.y = Mathf.Cos(Mathf.PI*0.5f * slider.value);
            eff.anchoredPosition =new Vector2( (temp * r).x+45,(temp * r).y-45);
        }
        else
        {
            temp.x = Mathf.Sin(2 * Mathf.PI * slider.value);
            temp.y = Mathf.Cos(2 * Mathf.PI * slider.value);
            eff.anchoredPosition = temp * r;
        }       
    }
}
