using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

[AddComponentMenu("UI/UISlider", 12)]
[RequireComponent(typeof(RectTransform))]
public class UISlider : Slider
{
    public bool SmoothSlider = false;
    public bool Right = true;
    public bool Left = true;

    public UserDelegate OnSmoothMoveComplete = new UserDelegate();

    public UserDelegate OnsmoothMoveUpdate = new UserDelegate();

    private Coroutine smoothCoroutine = null;
    private int m_lastTar;
    private float m_curTar;
    private float targetValue;
    private float m_smoothDuration = 0.5f;
    public float smoothDuration
    {
        get
        {
            return SmoothSlider ? m_smoothDuration : 0;
        }
        set
        {
            m_smoothDuration = value;
        }
    }

    public override float value
    {
        set
        {
            if (!Left && m_lastTar >= value) return;
            if (!Right && m_lastTar <= value) return;
            
            if (smoothCoroutine != null)
            {
                StopCoroutine(smoothCoroutine);
                smoothCoroutine = null;
            }
            m_curTar = value;
            targetValue = m_curTar - m_lastTar;
            if (this.isActiveAndEnabled && SmoothSlider)
            {
                smoothCoroutine = StartCoroutine(SmoothUpdate());
                if (OnsmoothMoveUpdate != null)
                    OnsmoothMoveUpdate.ExecuteCalls();
            }
            else
                InitValue(value);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        m_lastTar = 0;
        m_curTar = 0;
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }
    }

    public void StopMove()
    {
        m_lastTar = 0;
        m_curTar = 0;
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        } 

         Set(RemainderValue(0), true);
    }

    public void InitValue(float value)
    {
        m_lastTar = value <= maxValue ? 0 : (int)Math.Floor(value / maxValue);
        Set(RemainderValue(value), true);
    }
    private IEnumerator SmoothUpdate()
    {
        float f = 0;
        float v = value;

        bool direction = targetValue > value;
        while (f < smoothDuration)
        {
            f += Time.deltaTime;
            float curV = Mathf.Lerp(targetValue, v, maxValue - (f / smoothDuration));
            if (direction)
            {
                if(curV > maxValue) m_lastTar = m_curTar <= maxValue ? 0 : (int)Math.Floor(m_curTar / maxValue);
                curV = RemainderValue(curV);
            }
            else
            {
                if (curV < minValue) m_lastTar = m_curTar <= maxValue ? 0 : (int)Math.Floor(m_curTar / maxValue);
                curV = RemainderValue(curV);
                curV = curV <= minValue ? curV + maxValue : curV;
            }

            Set(curV, true);
            yield return null;
        }
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;           
        }

        m_lastTar = m_curTar <= maxValue ? 0:(int)Math.Floor(m_curTar / maxValue);
        Set(RemainderValue(targetValue), true);
        OnSmoothMoveComplete.ExecuteCalls();
    }
    private float RemainderValue(float value)
    {
        return (value > 0 && value % maxValue == 0) ? maxValue : value % maxValue;
    }
}