using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Halton
{
    public Vector2 Base;
   public Halton(Vector2 Base)
   {
       this.Base = Base;
   }
    public Vector2 GenerateHaltonSequence(int FrameID)
    {
        float a, b, c, d;
        a = b = 0;
        c = 1.0f / Base.x;
        d = 1.0f / Base.y;
        int i, j;
        i = j = FrameID;
        while (i > 0)
        {
            a += c * (FrameID % Base.x);
            i = Mathf.FloorToInt(i / Base.x);
            c /= Base.x;
        }
        while (j > 0)
        {
            b += d * (FrameID % Base.y);
            j = Mathf.FloorToInt(j / Base.y);
            d /= Base.y;
        }
        return new Vector2(c, d);
    }
}
