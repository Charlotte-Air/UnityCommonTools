using UnityEngine;
// using Spine.Unity;
using System.Collections.Generic;

public class ScreenFX : MonoBehaviour
{
    public GameObject fxSample;
    public float fxLifeTime = 1.0f;
    public RectTransform fxContainer;
    public Camera fxRenderCamera;

    private Queue<GameObject> pool = new Queue<GameObject>(20);
    private List<GameObject> activatedList = new List<GameObject>();
    // private SkeletonGraphic sgp = null;

    private void Awake()
    {
        this.enabled = false;
    }

    private void Update()
    {
        for (int i =activatedList.Count - 1; i >= 0; --i)
        {
            float fxTime = float.Parse(activatedList[i].name);
            if (Time.time - fxTime > fxLifeTime)
            {
                RecycleFX(activatedList[i]);
                activatedList.RemoveAt(i);
            }
        }

        if (Application.isMobilePlatform)
        {
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.GetTouch(i);
                if (touch.phase == TouchPhase.Began)
                {
                    PlayFX(touch.position);
                }
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                PlayFX(Input.mousePosition);
            }
        }
    }


    private void PlayFX(Vector2 tapPos)
    {
        GameObject fx = CreateFX();
        fx.name = Time.time.ToString();
        activatedList.Add(fx);

        RectTransform fxRectTrans = fx.GetComponent<RectTransform>();
        Vector2 fxLocalPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(fxContainer, tapPos, fxRenderCamera, out fxLocalPos);
        fxRectTrans.SetParent(fxContainer);
        fxRectTrans.anchoredPosition3D = new Vector3(fxLocalPos.x,fxLocalPos.y,0f);
        fxRectTrans.localScale = Vector3.one;
        // if (sgp != null)
        // {
        //     sgp.AnimationState.SetAnimation(0, sgp.startingAnimation,false);
        // }
        fx.SetActive(true);
        
    }


    private GameObject CreateFX()
    {
        GameObject newFX = null;
        if (pool.Count > 0)
        {
            newFX = pool.Dequeue();
        }
        else
        {
            newFX = Instantiate(fxSample);
        }
        return newFX;
    }

    private void RecycleFX(GameObject fx)
    {
        fx.SetActive(false);
        pool.Enqueue(fx);
    }

    float ParticleSystemsLength(GameObject obj)
    {
        ParticleSystem[] particleSystems = obj.GetComponentsInChildren<ParticleSystem>();
        float maxDuration = 0;
        for(int i = 0; i < particleSystems.Length; i++)
        {
            var tmp = particleSystems[i];
            if (tmp.emission.enabled)
            {
                if (tmp.main.loop) return -1f;
                float duration = 0;
                if (tmp.emission.rateOverTimeMultiplier <= 0) duration = tmp.main.startDelayMultiplier + tmp.main.startLifetimeMultiplier;
                else duration = tmp.main.startDelayMultiplier + Mathf.Max(tmp.main.duration, tmp.main.startLifetimeMultiplier);
                maxDuration = duration > maxDuration ? duration : maxDuration;
            }
        }
        return maxDuration;
    }
}
