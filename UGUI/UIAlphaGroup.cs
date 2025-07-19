using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;

[ExecuteInEditMode]
public class UIAlphaGroup : MonoBehaviour
{
    public float alpha;

    private Image[] imgs;
    private Text[] labels;
    private RawImage[] rawImgs;

    private Dictionary<MaskableGraphic, float> maxAlpha;
    
    private Tweener fadeInTweener = null;

    public Tweener FadeInTweener
    {
        get { return fadeInTweener; }
        set { fadeInTweener = value; }
    }
    
    private Tweener fadeOutTweener = null;

    public Tweener FadeOutTweener
    {
        get { return fadeOutTweener; }
        set { fadeOutTweener = value; }
    }

    private void Start()
    {
        Init();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        if (fadeInTweener != null)
            DOTween.Kill(fadeInTweener);

        if (fadeOutTweener != null)
            DOTween.Kill(fadeOutTweener);
    }

    public void Init()
    {
        try
        {
            imgs = this.GetComponentsInChildren<Image>(true);
            labels = this.GetComponentsInChildren<Text>(true);
            rawImgs = this.GetComponentsInChildren<RawImage>(true);
        }
        catch (Exception e)
        {
            Debug.LogFormat("UIAlphaGroup is error, e : {0}", e);
        }
       
        if (maxAlpha == null)
            maxAlpha = new Dictionary<MaskableGraphic, float>();

        maxAlpha.Clear();


        if (imgs != null)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                maxAlpha.Add(imgs[i], imgs[i].color.a);
            }
        }

        if (rawImgs != null)
        {
            for (int i = 0; i < rawImgs.Length; i++)
            {
                maxAlpha.Add(rawImgs[i], rawImgs[i].color.a);
            }
        }
    }

    public void ChangeAlpha()
    {
        if (imgs != null)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                imgs[i].color =new Color(imgs[i].color.r, imgs[i].color.g, imgs[i].color.b, alpha);
            }
        }

        if (labels != null)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].color = new Color(labels[i].color.r, labels[i].color.g, labels[i].color.b, alpha);
            }
        }

        if (rawImgs != null)
        {
            for (int i = 0; i < rawImgs.Length; i++)
            {
                rawImgs[i].color = new Color(rawImgs[i].color.r, rawImgs[i].color.g, rawImgs[i].color.b, alpha);
            }
        }
    }

    public void SetAlpha(float tempA)
    {
        if (imgs != null && maxAlpha != null)
        {
            for (int i = 0; i < imgs.Length; i++)
            {
                if (imgs[i] == null)
                    continue;
                
                float alpha = maxAlpha == null || maxAlpha.Count == 0 ? tempA : Mathf.Min(tempA, maxAlpha[imgs[i]]);
                imgs[i].color = new Color(imgs[i].color.r, imgs[i].color.g, imgs[i].color.b, alpha);
            }
        }

        if (labels != null)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i] == null)
                    continue;
                
                labels[i].color = new Color(labels[i].color.r, labels[i].color.g, labels[i].color.b, tempA);
            }
        }

        if (rawImgs != null && maxAlpha != null)
        {
            for (int i = 0; i < rawImgs.Length; i++)
            {
                if (rawImgs[i] == null)
                    continue;
                
                float alpha = maxAlpha == null || maxAlpha.Count == 0 ? tempA : Mathf.Min(tempA, maxAlpha[rawImgs[i]]);
                rawImgs[i].color = new Color(rawImgs[i].color.r, rawImgs[i].color.g, rawImgs[i].color.b, alpha);
            }
        }
    }

    /// <summary>
    /// 渐显
    /// </summary>
  
    public static void FadeIn(GameObject target, float dAlpha, float tAlpha, float time,
        Action<float> updateAction = null, Action<float> completeAction = null)
    {
        if (target == null)
            return;
        
        UIAlphaGroup ag = target.GetComponent<UIAlphaGroup>();
        if (ag == null)
            ag = target.AddComponent<UIAlphaGroup>();

        FadeIn(ag, dAlpha, tAlpha, time, updateAction, completeAction);
    }

    public static void FadeIn(UIAlphaGroup target, float dAlpha, float tAlpha, float time,
        Action<float> updateAction = null, Action<float> completeAction = null)
    {
        if (target == null)
            return;

        if (dAlpha >= tAlpha)
            return;

        if (target.FadeInTweener != null)
            DOTween.Kill(target.FadeInTweener);
        
        float alpha = dAlpha;
        target.SetAlpha(alpha);
        target.FadeInTweener = DOTween.To(() => alpha, x => alpha = x, tAlpha,
                time).OnUpdate(() =>
            {
                target.SetAlpha(alpha);
                
                if (updateAction != null)
                    updateAction.Invoke(alpha);
            })
            .OnComplete(() =>
            {
                target.SetAlpha(tAlpha);
                target.FadeInTweener = null;
                
                if (completeAction != null)
                    completeAction.Invoke(alpha);
            }); 
    }

    /// <summary>
    /// 渐隐
    /// </summary>
    public static void FadeOut(GameObject target, float dAlpha, float tAlpha, float time,
        Action<float> updateAction = null, Action<float> completeAction = null)
    {
        if (target == null)
            return;
        
        UIAlphaGroup ag = target.GetComponent<UIAlphaGroup>();
        if (ag == null)
            ag = target.AddComponent<UIAlphaGroup>();

        FadeOut(ag, dAlpha, tAlpha, time, updateAction, completeAction);
    }

    public static void FadeOut(UIAlphaGroup target, float dAlpha, float tAlpha, float time,
        Action<float> updateAction = null, Action<float> completeAction = null)
    {
        if (target == null)
            return;
        
        if (dAlpha <= tAlpha)
            return;

        if (target.FadeOutTweener != null)
            DOTween.Kill(target.FadeOutTweener);
        
        float alpha = dAlpha;
        target.SetAlpha(alpha);
        target.FadeOutTweener = DOTween.To(() => alpha, x => alpha = x, tAlpha,
                time).OnUpdate(() =>
            {
                target.SetAlpha(alpha);

                if (updateAction != null)
                    updateAction.Invoke(alpha);
            })
            .OnComplete(() =>
            {
                target.SetAlpha(tAlpha);
                target.FadeOutTweener = null;

                if (completeAction != null)
                    completeAction.Invoke(alpha);
            });
    }

    public static void SetAlpha(GameObject target, float alpha)
    {
        if (target == null)
            return;
        
        UIAlphaGroup ag = target.GetComponent<UIAlphaGroup>();
        if (ag == null)
            ag = target.AddComponent<UIAlphaGroup>();

        SetAlpha(ag, alpha);
    }

    public static void SetAlpha(UIAlphaGroup target, float alpha)
    {
        if (target == null)
            return;
        
        if (target.FadeInTweener != null)
            DOTween.Kill(target.FadeInTweener);

        if (target.FadeOutTweener != null)
            DOTween.Kill(target.FadeOutTweener);
        
        target.SetAlpha(alpha);
    }

    public static void Fade(GameObject target, float dAlpha, float tAlpha, float time,
        Action<float> updateAction = null, Action<float> completeAction = null)
    {
        if (target == null)
            return;

        CanvasGroup cGroup = target.GetOrAddComponent<CanvasGroup>();
        float alpha = dAlpha;
        cGroup.alpha = dAlpha;
        DOTween.To(() => alpha, x => alpha = x, tAlpha,
                time).OnUpdate(() =>
            {
                if (cGroup != null)
                    cGroup.alpha = alpha;
                if (updateAction != null)
                    updateAction.Invoke(alpha);
            })
            .OnComplete(() =>
            {
                if (cGroup != null)
                    cGroup.alpha = tAlpha;
                if (completeAction != null)
                    completeAction.Invoke(alpha);
            }); 
    }
}
