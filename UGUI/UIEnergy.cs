using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Object = System.Object;

public class UIEnergy : MonoBehaviour
{
    private UIRawImage img;
    private Material instanceMaterial;
    private Material sharedMaterial;
        
    private float fill;
    private float range;

    private Tweener mEneryFillTweener;

    private void Awake()
    {
        img = this.GetComponent<UIRawImage>();
        sharedMaterial = img.material;
        instanceMaterial = new Material(sharedMaterial);
        img.material = instanceMaterial;
        fill = instanceMaterial.GetFloat("_Fill");
        range = instanceMaterial.GetFloat("_Range");
    }

    private void OnDestroy()
    {
        img.material = sharedMaterial;
        DestroyImmediate(instanceMaterial);
        instanceMaterial = null;
        sharedMaterial = null;
        img = null;
    }

    public void SetFill(float value)
    {
        if (instanceMaterial == null) return;

        fill = Mathf.Clamp(value, 0, 1);
        instanceMaterial.SetFloat("_Fill", fill);
    }

    public void SetSmoothFill(float curValue, float targetValue, float duringSec, Action<float> onUpdate = null, Action onComplete = null)
    {
        if (instanceMaterial == null) return;

        float _curValue = curValue;
        DOTween.Kill(mEneryFillTweener);
        mEneryFillTweener = DOTween.To(() => _curValue, x => _curValue = x, targetValue, duringSec)
            .SetEase(Ease.Linear).SetUpdate(true)
            .OnUpdate(() =>
            {
                if (onUpdate != null)
                    onUpdate.Invoke(_curValue);

                fill = Mathf.Clamp(_curValue, 0, 1);
                instanceMaterial.SetFloat("_Fill", fill);                                
            }
            ).OnComplete(()=> {
                if (onComplete != null)
                    onComplete.Invoke();
            });
    }

    public void SetColor(Color color)
    {
        if (instanceMaterial == null) return;
        instanceMaterial.SetColor("_Color", color);
    }

    public void SetRange(float value)
    {
        if (instanceMaterial == null) return;
        range = Mathf.Clamp(value, 0, 1);
        instanceMaterial.SetFloat("_Range", range);
    }
}
