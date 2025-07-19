using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UIEraserTexture : MonoBehaviour
{

    public RawImage image;
    private int brushScale = 180;

    Texture2D texRender;
    RectTransform mRectTransform;
    public Canvas canvas;
    private const string timerKey = "EraserTextureDraw";
   
    public void Init()
    {
        mRectTransform = GetComponent<RectTransform>();
        texRender = new Texture2D((int)image.rectTransform.rect.width, (int)image.rectTransform.rect.height, TextureFormat.ARGB32,false);

        Reset();
       
    }


    void OnDestory()
    {
        //TimerManager.Destroy(timerKey);
    }

    //bool isMove = false;



    //private int tick = 0;
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            OnMouseMove();
        }
    }

    //Vector2 start = Vector2.zero;
    Vector2 end = Vector2.zero;

    Vector2 ConvertSceneToUI(Vector3 posi)
    {
        Vector2 postion;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, posi, canvas.worldCamera, out postion))
        {
            //LogHelper.Info("CanvasPosition:"+ postion);
            return postion;
        }
        return Vector2.zero;
    }

    private float x;
    private float y;
    void OnMouseMove()
    {

        end = ConvertSceneToUI(Input.mousePosition);
        x = end.x + image.rectTransform.rect.width/2;
        y = end.y + image.rectTransform.rect.height/2;
        Draw(new Rect(new Vector2(x, y), new Vector2(brushScale, brushScale)));
    }

    void Reset()
    {

        //for (int i = 0; i < texRender.width; i++)
        //{
        //    for (int j = 0; j < texRender.height; j++)
        //    {

        //        Color color = texRender.GetPixel(i, j);
        //        color.a = 1;
        //        texRender.SetPixel(i, j, color);
        //    }
        //}

        //texRender.Apply();
        image.material.SetTexture("_RendTex", texRender);

    }

    private Vector2 temp1;
    private Vector2 temp2;
    private float tempDis;
    
    void Draw(Rect rect)
    {
        temp1.x = rect.x;
        temp1.y = rect.y;
        for (int x = (int)(rect.x-rect.width/2); x < (int)(rect.x + rect.width / 2); x++)
        {
            for (int y = (int)(rect.y - rect.height / 2); y < (int)(rect.y + rect.height / 2); y++)
            {
                if (x < 0 || x > texRender.width || y < 0 || y > texRender.height)
                {
                    return;
                }
                temp2.x = x;
                temp2.y = y;

                tempDis = Vector2.Distance(temp1, temp2);
                if (tempDis > rect.width / 2)
                {
                    continue;
                }
                Color color = texRender.GetPixel(x, y);
              
                if (tempDis < rect.width/4)
                {
                    color.a = 0;
                }
                else
                {
                    color.a =Mathf.Min(color.a,1 - (rect.width/2 - tempDis)/(rect.width/4));
                }

                texRender.SetPixel(x, y, color);
            }
        }

        texRender.Apply();
        image.material.SetTexture("_RendTex", texRender);
    }


    public float GetTransparentPercent()
    {
        int count = 0;
        for (int x = 0; x < texRender.width; x++)
        {
            for (int y = 0; y < texRender.height; y++)
            {
                Color color = texRender.GetPixel(x, y);
                if (color.a == 0)
                {
                    count++;
                }
            }
        }
        return (float)count/(texRender.width* texRender.height);
    }

}