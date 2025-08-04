using System;
using UnityEngine;

public class MaskFadeControler : MonoBehaviour
{
    public Material mat;
    private float value;   // alpha ctrl
    private string dir;    // + or -
    private int sign;
    void Start() { }

    private void OnEnable()
    {
        value = 0;
        dir = "+";
        sign = 60;
    }

    void Update()
    {
        if (dir == "+")
        {
            if (value >= 1)
            {
                dir = "-";
                sign = 0;
            }
            else if (value >= 0.9f) value += 0.006f;
            else value += 0.01f;
        }else if (dir == "-")
        {
            if (value <= 0)
            {
                dir = "+";
                sign = 0;
            }
            else if (value <= 0.1f) value -= 0.006f;
            else value -= 0.01f;
        }
        value = Math.Min(1, Math.Max(0, value));
        mat.SetFloat("_Intensity", value);
    }
}
