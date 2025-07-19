using System;
using UnityEngine;
using System.Collections;

///<summary>
///多层血条类
///</summary>
public class UIBloodSlider : MonoBehaviour
{
    [System.Serializable]//序列化
    public class Bars
    {
        public Color first;
        public Color second;
    }
    public Bars[] healthBarArray;

    public UIImage HPImageTop;
    public UIImage HPImageTop_Shadow;
    public UIImage HPImageMid;
    public UIImage HPImageMid_Shadow;
    public UIImage HPImageBot;
    public UIImage HPImageBot_Shadow;

    public UIText TextHp;
    public UIText TextValue;

    private float MaxValue;
    private float MaxHP;

    public float smoothDuration;

    public float totalHP = 0;
    private float curHPValue = 0;

    private int totalCount;
    private int curCount;
    private bool direction;

    private bool isInit = false;
    private int m_MaxCount;
    private Coroutine smoothCoroutine = null;
    //private float ratio = 1;
    //private int index = 0;

    //void Start()
    //{
    //    Init(8329,500);
    //}

    private void OnDisable()
    {
        if(smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }
        m_MaxCount = 1;
        isInit = false;
    }

    public void InitByCount(float hp,int maxCount)
    {
        if (maxCount <= 0)
            maxCount = 1;

        m_MaxCount = maxCount;
        float mhp = 0;
        float maxvalue = 0;
        if (hp > maxCount)
        {
            mhp = hp - hp % maxCount;
            maxvalue = mhp / maxCount;
        }
        else
        {
            mhp = hp;
            maxvalue = hp;
        }
        Init(mhp, maxvalue);
    }
    public void Init(float hp, float maxValue,bool isShowPrecent = false)
    {
        if (maxValue == 0)
        {
            Debug.Log("Max Value is Zero");
            return;
        }

        MaxHP = hp;
        totalHP = hp;
        MaxValue = maxValue;

        curHPValue = totalHP % MaxValue;
        totalCount = curCount = Mathf.Abs((int)Math.Floor(totalHP / MaxValue));
        if (!isShowPrecent)
        {
            TextHp.text = string.Format("{0}/{1}", ConvertNumber((long)totalHP), ConvertNumber((long)MaxHP));
            TextValue.text = string.Format("x {0}", totalCount);
        }
        else
        {
            TextHp.text = string.Format("{0}%", totalHP.ToString("f2"));
            //TextValue.text = string.Format("x {0}", totalCount);
        }

        int index = (curCount) % healthBarArray.Length;
        HPImageTop.color = healthBarArray[index].first;
        HPImageTop.fillAmount = curHPValue / MaxValue;
        HPImageTop_Shadow.color = healthBarArray[index].second;
        HPImageTop_Shadow.fillAmount = curHPValue / MaxValue;

        index = (curCount + 1) % healthBarArray.Length;
        HPImageMid.color = healthBarArray[index].first;
        HPImageMid.fillAmount = curCount - 1 < 0 ? 0 : 1;
        HPImageMid_Shadow.color = healthBarArray[index].second;
        HPImageMid_Shadow.fillAmount = curCount - 1 < 0 ? 0 : 1;

        index = (curCount + 2) % healthBarArray.Length;
        HPImageBot.color = healthBarArray[index].first;
        HPImageBot.fillAmount = curCount - 2 < 0 ? 0 : 1;
        HPImageBot_Shadow.color = healthBarArray[index].second;
        HPImageBot_Shadow.fillAmount = curCount - 2 < 0 ? 0 : 1;

        isInit = true;
    }
    
    /// <summary>
    /// 转换数字
    /// 超过亿，以亿为单位，保留两位小数；
    /// 超过万，以万为单位，保留两位小数
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ConvertNumber(long value, bool isDecimal = true)
    {
        long hundredMillion = 100000000;
        //long hundredThousand = 10000;
        long tenThousand = 10000;

        if (value >= hundredMillion)
        {
            float m = (float)value / hundredMillion;
            return StringBuilder(isDecimal && m > Mathf.FloorToInt(m)
                ? m.ToString("F")
                : Mathf.FloorToInt(m).ToString(), "亿");
        }
        else if (value >= tenThousand)
        {
            float h = (float)value / 10000;
            return StringBuilder(isDecimal && h > Mathf.FloorToInt(h)
                ? h.ToString("F")
                : Mathf.FloorToInt(h).ToString(), "万");
        }
        return value.ToString();
    }
    
    private static System.Text.StringBuilder mstrbuilder = new System.Text.StringBuilder();
    /// <summary>
    /// 合并字符
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string StringBuilder(params object[] args)
    {
        mstrbuilder.Remove(0, mstrbuilder.Length);
        if (args != null)
        {
            int len = args.Length;
            for (int i = 0; i < len; ++i)
            {
                mstrbuilder.Append(args[i]);
            }
        }
        return mstrbuilder.ToString();
    }

    public void InitByPercent(float hp,float maxValue = 1)
    {
        Init(hp, maxValue, true);
    }

    public void ReSetHp(float value)
    {
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }

        if (MaxValue == 0)
            return;
        
        totalHP = value;
        totalHP = Mathf.Clamp(totalHP, 0, MaxHP);
        curHPValue = totalHP % MaxValue;
        totalCount = curCount = Mathf.Abs((int)Math.Floor(totalHP / MaxValue));
        TextHp.text = string.Format("{0}/{1}", ConvertNumber((long) totalHP), ConvertNumber((long) MaxHP));
        TextValue.text = string.Format("x {0}", totalCount);

        int index = (curCount) % healthBarArray.Length;
        HPImageTop.color = healthBarArray[index].first;
        HPImageTop.fillAmount = curHPValue / MaxValue;
        HPImageTop_Shadow.color = healthBarArray[index].second;
        HPImageTop_Shadow.fillAmount = curHPValue / MaxValue;

        index = (curCount + 1) % healthBarArray.Length;
        HPImageMid.color = healthBarArray[index].first;
        HPImageMid.fillAmount = curCount - 1 < 0 ? 0 : 1;
        HPImageMid_Shadow.color = healthBarArray[index].second;
        HPImageMid_Shadow.fillAmount = curCount - 1 < 0 ? 0 : 1;

        index = (curCount + 2) % healthBarArray.Length;
        HPImageBot.color = healthBarArray[index].first;
        HPImageBot.fillAmount = curCount - 2 < 0 ? 0 : 1;
        HPImageBot_Shadow.color = healthBarArray[index].second;
        HPImageBot_Shadow.fillAmount = curCount - 2 < 0 ? 0 : 1;

        isInit = true;
    }

    public void RefreshPercent(float value)
    {
        if (!isInit)
            Init(value, m_MaxCount,true);

        float v = value - totalHP;
        HurtOrRecoverValue(v,true);
    }

    public void RefreshValue(float value)
    {
        if (!isInit)
            Init(value, m_MaxCount);

        float v = value - totalHP;
        HurtOrRecoverValue(v);
    }

    public void HurtOrRecoverValue(float value,bool isShowPrecent = false)
    {
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }

        if (MaxValue == 0)
            return;

        direction = value < 0;
        totalHP += value;

        if (!isShowPrecent)
        {
            totalHP = Mathf.Clamp(totalHP, 0, MaxHP);
            TextHp.text = string.Format("{0}/{1}", ConvertNumber((long)totalHP), ConvertNumber((long)MaxHP));
            //TextValue.text = string.Format("x {0}", totalCount);
        }
        else
        {
            TextHp.text = string.Format("{0}%", totalHP.ToString("f2"));
            TextValue.text = "";
        }


        int count = Mathf.Abs((int)Math.Floor(totalHP / MaxValue));

        if (!isActiveAndEnabled) return;
        if (count == curCount)
        {
            curHPValue = totalHP % MaxValue;
            HPImageTop.fillAmount = curHPValue / MaxValue;
            smoothCoroutine = StartCoroutine(SmoothUpdate());
        }
        else// if (count < curCount)
        {
            curCount = count;
            if (!isShowPrecent)
                TextValue.text = string.Format("x {0}", curCount);
            ResetTop();
            smoothCoroutine = StartCoroutine(SmoothUpdate());
        }
    }
    private IEnumerator SmoothUpdate()
    {
        float f = 0;
        if(smoothDuration == 0)
        {
            HPImageTop_Shadow.fillAmount = HPImageTop.fillAmount;
            if(HPImageTop_Shadow.fillAmount == 0)
            {
                ResetTop();
            }
        }
        while (f < smoothDuration && !EqualFillMount(HPImageTop_Shadow.fillAmount, HPImageTop.fillAmount))
        {
            f += Time.deltaTime;
            HPImageTop_Shadow.fillAmount = Mathf.Lerp(HPImageTop_Shadow.fillAmount, HPImageTop.fillAmount, f / smoothDuration);
            yield return null;
        }
        if (smoothCoroutine != null)
        {
            StopCoroutine(smoothCoroutine);
            smoothCoroutine = null;
        }
        if (HPImageTop_Shadow.fillAmount == 0)// && !EqualFillMount(HPImageTop_Shadow.fillAmount,0))
        {
            ResetTop();
            if (!EqualFillMount(HPImageTop_Shadow.fillAmount, HPImageTop.fillAmount))
                smoothCoroutine = StartCoroutine(SmoothUpdate());
        }
    }
    private void ResetTop()
    {
        if (MaxValue == 0)
            return;
        
        curHPValue = totalHP % MaxValue;

        int index = (curCount) % healthBarArray.Length;
        if (index >= healthBarArray.Length)
        {
            Debug.LogFormat("index : {0}, array count : {1} : ", index, healthBarArray.Length);
            return;
        }
        HPImageTop.color = healthBarArray[index].first;
        HPImageTop.fillAmount = curHPValue / MaxValue;
        HPImageTop_Shadow.color = healthBarArray[index].second;
        HPImageTop_Shadow.fillAmount = direction ? 1 : 0;

        index = (curCount + 1) % healthBarArray.Length;
        if (index >= healthBarArray.Length)
        {
            Debug.LogFormat("index : {0}, array count : {1} : ", index, healthBarArray.Length);
            return;
        }
        HPImageMid.color = healthBarArray[index].first;
        HPImageMid.fillAmount = curCount - 1 < 0 ? 0 : 1;
        HPImageMid_Shadow.color = healthBarArray[index].second;
        HPImageMid_Shadow.fillAmount = curCount - 1 < 0 ? 0 : 1;

        index = (curCount + 2) % healthBarArray.Length;
        if (index >= healthBarArray.Length)
        {
            Debug.LogFormat("index : {0}, array count : {1} : ", index, healthBarArray.Length);
            return;
        }
        HPImageBot.color = healthBarArray[index].first;
        HPImageBot.fillAmount = curCount - 2 < 0 ? 0 : 1;
        HPImageBot_Shadow.color = healthBarArray[index].second;
        HPImageBot_Shadow.fillAmount = curCount - 2 < 0 ? 0 : 1;
    }

    private bool EqualFillMount(float a,float b)
    {
        bool result = Mathf.Abs(a - b) <= 0.001f ? true : false;
        return result;
    }
}