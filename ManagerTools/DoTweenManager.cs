using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections.Generic;

public class DoTweenManager
{
    private static DoTweenManager _instance;
    public static DoTweenManager Instance
    {
        get
        {
            if (_instance == null) 
            {
                _instance = new DoTweenManager();
            }
            return _instance;
        }
    }
    
    public void FlipVertical(Transform trans,float delay,float duration, Action cb = null,Action middleCb = null) 
    {
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(delay);
        seq.Append(trans.DOScale(new Vector3(0, 1, 1), duration));
        if (middleCb != null) 
        {
            seq.AppendCallback(() => 
            {
                middleCb();           
            });
        }        
        seq.Append(trans.DOScale(new Vector3(1, 1, 1), duration));
        if (cb != null) 
        {
            seq.AppendCallback(() => 
            {
                cb();
            });
        }
    }

    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="pos">绝对位置</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoMoveTo(GameObject obj, Vector3 pos, float time, Action callback = null) 
    {
        obj.transform.DOLocalMove(pos, time).OnComplete(() => 
        {
            if (callback != null) callback();
        }).SetUpdate(true);
    }

    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="pos">相对位置</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoMoveBy(GameObject obj, Vector3 pos, float time, Action callback = null) 
    {
        var localPosition = obj.transform.localPosition;
        Vector3 absPos = new Vector3(localPosition.x + pos.x, localPosition.y + pos.y, localPosition.z + pos.z);
        DoMoveTo(obj, absPos, time, callback);
    }

    public void DoMoveBy(GameObject obj, float posX, float posY, float posZ, float time, Action callback = null)
    {
        var localPosition = obj.transform.localPosition;
        Vector3 absPos = new Vector3(localPosition.x + posX, localPosition.y + posY, localPosition.z + posZ);
        DoMoveTo(obj, absPos, time, callback);
    }

    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="pos">绝对位置</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoAnchorMoveTo(GameObject obj, Vector2 pos, float time, Action callback = null) 
    {
        obj.transform.GetComponent<RectTransform>().DOAnchorPos(pos, time).OnComplete(()=>
        {
            callback?.Invoke();
        });
    }

    /// <summary>
    /// 移动到指定位置
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="pos">相对位置</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoAnchorMoveBy(GameObject obj, Vector2 pos, float time, Action callback = null) 
    {
        RectTransform rectTrans = obj.transform.GetComponent<RectTransform>();
        if (!rectTrans) Debug.Log(obj + "RectTransform 不存在");
        Vector2 absPos = new Vector2(rectTrans.anchoredPosition.x + pos.x, rectTrans.anchoredPosition.y + pos.y);
        DoAnchorMoveTo(obj, absPos, time, callback);
    }

    /// <summary>
    /// 对象沿着本地坐标数组做运动(匀速)
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="positions">本地坐标数组</param>
    /// <param name="allTime">通过所有点的总时间</param>
    /// <param name="delay">结束延迟</param>
    /// <param name="callback">结束回调</param>
    public void DoMoveLocalPositions(GameObject obj, Vector3[] positions, float allTime, float delay = 0, TweenCallback callback = null)
    {
        List<float> lengthList = new List<float>();
        float allLength = 0;
        Sequence seq = DOTween.Sequence();
        lengthList.Add(Vector3.Distance(positions[0], obj.transform.localPosition));
        allLength = lengthList[0];
        for (int i = 0; i < positions.Length - 1; i++) 
        {
            float length = Vector3.Distance(positions[i + 1], positions[i]);
            // 将两点间的距离保存下来
            lengthList.Add(length);
            allLength += length;
        }
        for (int i = 0; i < lengthList.Count; i++) 
        {
            seq.Append(obj.transform.DOLocalMove(positions[i], lengthList[i] / allLength * allTime).SetEase(Ease.Linear));
        }
        if (delay > 0) 
        {
            seq.AppendInterval(delay);
        }
        if (callback != null) 
        {
            seq.OnComplete(callback);
        }
    }

    /// <summary>
    /// 对象沿着本地坐标数组做运动(变速，每两个点之间用时相等，距离长的两个点之间的运动速度就快)
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="positions">本地坐标数组</param>
    /// <param name="allTime">通过所有点的总时间</param>
    /// <param name="callback1"></param>
    /// <param name="delay">结束延迟</param>
    /// <param name="callback2"></param>
    public void DoMoveLocalPositionsWithAcc(GameObject obj, Vector3[] positions, float allTime, TweenCallback callback1 = null, float delay = 0, TweenCallback callback2 = null)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < positions.Length; i++) 
        {
            seq.Append(obj.transform.DOLocalMove(positions[i], allTime / positions.Length).SetEase(Ease.Linear));
        }
        if (callback1 != null) 
        {
            seq.AppendCallback(callback1);
        }
        if (delay > 0) 
        {
            seq.AppendInterval(delay);
        }
        if (callback2 != null) 
        {
            seq.OnComplete(callback2);
        }
    }

    /// <summary>
    /// 对象沿着世界坐标数组做运动(匀速)
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="positions">世界坐标数组</param>
    /// <param name="allTime">通过所有点的总时间</param>
    /// <param name="callback"></param>
    public void DoMovePositions(GameObject obj, Vector3[] positions, float allTime, TweenCallback callback = null)
    {
        List<float> lengthList = new List<float>();
        float allLength = 0;
        Sequence seq = DOTween.Sequence();
        lengthList.Add(Vector3.Distance(positions[0], obj.transform.position));
        allLength = lengthList[0];
        for (int i = 0; i < positions.Length - 1; i++) 
        {
            float length = Vector3.Distance(positions[i + 1], positions[i]);
            // 将两点间的距离保存下来
            lengthList.Add(length);
            allLength += length;
        }
        for (int i = 0; i < lengthList.Count; i++) 
        {
            seq.Append(obj.transform.DOMove(positions[i], lengthList[i] / allLength * allTime).SetEase(Ease.Linear));
        }
        if (callback != null) {
            seq.OnComplete(callback);
        }
    }

    /// <summary>
    /// 对象伸缩到指定大小
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="scale">绝对大小</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoScaleTo(GameObject obj, float scale, float time, Action callback = null) 
    {
        obj.transform.DOScale(scale, time).OnComplete(() => 
        {
            if (callback != null) callback();
        });
    }

    /// <summary>
    /// 对象伸缩到指定大小
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="scale">相对大小</param>
    /// <param name="time">时间</param>
    /// <param name="callback">结束回调</param>
    public void DoScaleBy(GameObject obj, float scale, float time, Action callback = null) 
    {
        float absScale = obj.transform.localScale.x * scale;
        DoScaleTo(obj, absScale, time, callback);
    }
    
    public void DoScaleTo(GameObject obj, Vector2 scale, float time, Action callback = null) 
    {
        int overNum = 0;
        obj.transform.DOScaleX(scale.x, time).OnComplete(()=>
        {
            if(++overNum == 2 && callback != null) callback();
        });
        obj.transform.DOScaleY(scale.y, time).OnComplete(()=>
        {
            if(++overNum == 2 && callback != null) callback();
        });
    }

    public void DoScaleTo(GameObject obj, Vector3 scale, float time, Action callback = null) 
    {
        int overNum = 0;
        obj.transform.DOScaleX(scale.x, time).OnComplete(()=>
        {
            if(++overNum == 3 && callback != null) callback();
        });
        obj.transform.DOScaleY(scale.y, time).OnComplete(()=>
        {
            if(++overNum == 3 && callback != null) callback();
        });
        obj.transform.DOScaleZ(scale.z, time).OnComplete(()=>
        {
            if(++overNum == 3 && callback != null) callback();
        });
    }

    public void DoText(GameObject obj,string str,float time,Action callback=null)
    {
        obj.GetComponent<Text>().DOText(str, time).OnComplete(() =>
        {
            if (callback != null) callback();
        });
    }

    /// <summary>
    /// 所有对象逐个FadeIn
    /// </summary>
    /// <param name="objs">对象数组</param>
    public void FadeInOneByOne(GameObject[] objs) 
    {
        for (int i = 0; i < objs.Length; i++) 
        {
            int index = i;
            CanvasGroup objCG = objs[i].GetComponent<CanvasGroup>();
            if (objCG == null) 
            {
                objCG = objs[i].AddComponent<CanvasGroup>();
            }
            KillTween(objCG);
            objCG.alpha = 0;
            Sequence seq = DOTween.Sequence();
            seq.SetDelay(index * 0.1f);
            seq.AppendCallback(()=>
            {
                FadeIn(objs[index], 0.3f, Ease.InQuad);
            });
        }
    }

    /// <summary>
    /// 渐显动画
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="time">时间</param>
    /// <param name="ease">EASE类型</param>
    public void FadeIn(GameObject obj, float time, Ease ease = Ease.Linear) 
    {
        CanvasGroup objCG = obj.GetComponent<CanvasGroup>();
        if (objCG == null) 
        {
            objCG = obj.AddComponent<CanvasGroup>();
        }
        KillTween(objCG);
        objCG.alpha = 0;
        objCG.DOFade(1, time).SetEase(ease).SetUpdate(true);
    }

    /// <summary>
    /// 渐隐动画
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="time">时间</param>
    /// <param name="ease">EASE类型</param>
    /// <param name="callback"></param>
    public void FadeOut(GameObject obj, float time, Ease ease = Ease.Linear, Action callback = null) 
    {
        CanvasGroup objCG = obj.GetComponent<CanvasGroup>();
        if (objCG == null) 
        {
            objCG = obj.AddComponent<CanvasGroup>();
        }
        KillTween(objCG);
        objCG.alpha = 1;
        objCG.DOFade(0, time).SetEase(ease).OnComplete(()=>
        {
            if (callback != null) callback();
        });
    }

    /// <summary>
    /// 渐隐动画(带透明度)
    /// </summary>
    /// <param name="obj">对象</param>
    /// <param name="time">时间</param>
    /// <param name="alpha">透明度</param>
    /// <param name="ease">ease类型</param>
    /// <param name="callback">回调</param>
    public void FadeOut(GameObject obj,float time, float alpha, Ease ease =Ease.Linear,Action callback = null)
    {
        CanvasGroup objCG = obj.GetComponent<CanvasGroup>();
        if (objCG == null)
        {
            objCG = obj.AddComponent<CanvasGroup>();
        }
        KillTween(objCG);
        objCG.alpha = 1;

        objCG.DOFade(alpha, time).SetEase(ease).OnComplete(() => 
        {
            if (callback != null) callback();
        });
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="objs">动画数组</param>
    public void KillTween(GameObject[] objs) 
    {
        var objEnumerator = objs.GetEnumerator();
        while(objEnumerator.MoveNext())
        {
            var obj = objEnumerator.Current as GameObject;
            KillTween(obj);
        }
    }

    public bool IsTweening(GameObject obj) 
    {
        return DOTween.IsTweening(obj);
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="obj">动画对象</param>
    public void KillTween(GameObject obj) 
    {
        if (DOTween.IsTweening(obj)) 
        {
            DOTween.Kill(obj, true);
        }
    }

    public void KillTween(CanvasGroup cg) 
    {
        if (DOTween.IsTweening(cg)) 
        {
            DOTween.Kill(cg, true);
        }
    }


    public void DoShakeCamera(Camera camera, float duration = 2f, float strengthPosition = 0.1f, int vibratoPosition = 10, float strengthRotation = 0.1f, int vibratoRotation = 10)
    {
        if (camera == null)
        {
            return;
        }
        camera.DOShakePosition(duration, strengthPosition, vibratoPosition);
        camera.DOShakeRotation(duration, strengthRotation, vibratoRotation);
    }
}

/// <summary>
/// 贝塞尔类
/// </summary>
public class BezierUtils
{
    /// <summary>
    /// 根据T值，计算贝塞尔曲线上面相对应的点
    /// </summary>
    /// <param name="t"></param>T值
    /// <param name="p0"></param>起始点
    /// <param name="p1"></param>控制点
    /// <param name="p2"></param>目标点
    /// <returns></returns>根据T值计算出来的贝赛尔曲线点（二阶）
    private static  Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        Vector3 p = uu * p0;
        p += 2 * u * t * p1;
        p += tt * p2;
        return p;
    }

    /// <summary>
    /// 获得贝塞尔曲线点的数组（二阶）
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>获得贝塞尔曲线点的数组（二阶）
    public static Vector3 [] GetBeizerList(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint,int segmentNum)
    {
        Vector3 [] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = CalculateCubicBezierPoint(t, startPoint, controlPoint, endPoint);
            path[i - 1] = pixel;
        }
        return path;
    }

    /// <summary>
    /// 获得贝塞尔曲线点的数组（二阶）
    /// </summary>
    /// <param name="startPoint"></param>起始点
    /// <param name="controlPoint"></param>控制点
    /// <param name="endPoint"></param>目标点
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>获得贝塞尔曲线点的数组（二阶）
    public static Vector3 [] GetBeizerListInQuad(Vector3 startPoint, Vector3 controlPoint, Vector3 endPoint,int segmentNum)
    {
        Vector3 [] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            t = t * t;
            Vector3 pixel = CalculateCubicBezierPoint(t, startPoint, controlPoint, endPoint);
            path[i - 1] = pixel;
        }
        return path;
    }

    /// <summary>
    /// 获取贝塞尔曲线点的数组（n阶，data.Length == n+1）
    /// </summary>
    /// <param name="points"></param>控制点数组
    /// <param name="segmentNum"></param>采样点的数量
    /// <returns></returns>获得贝塞尔曲线点的数组（n阶，data.Length == n+1）
    public static Vector3 [] GetBeizerList(Vector3[] points, int segmentNum)
    {
        Vector3 [] path = new Vector3[segmentNum];
        for (int i = 1; i <= segmentNum; i++)
        {
            float t = i / (float)segmentNum;
            Vector3 pixel = GetBezierPoint(points, t);
            path[i - 1] = pixel;
        }
        return path;
    }

    public static float GetBezierLength(Vector3[] data, int count = 20)
    {
        float length = 0.0f;
        Vector3 lastPoint = GetBezierPoint(data, 0);
        for (int i = 1; i <= count; i++)
        {
            Vector3 point = GetBezierPoint(data, (float)i / count);
            length += Vector3.Distance(point, lastPoint);
            lastPoint = point;
        }
        return length;
    }

    public static Vector3 GetBezierPoint(Transform[] data, float t)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 1; i <= data.Length; i++)
        {
            pos.x = pos.x + data[i - 1].position.x * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
            pos.y = pos.y + data[i - 1].position.y * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
            pos.z = pos.z + data[i - 1].position.z * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
        }
        return pos;
    }

    /// <summary>
    /// 获取贝塞尔曲线点（n阶，data.Length == n+1）
    /// </summary>
    /// <param name="data"></param>控制点数组
    /// <param name="t"></param>0-1
    /// <returns></returns>获取贝塞尔曲线点（n阶，data.Length == n+1）
    public static Vector3 GetBezierPoint(Vector3[] data, float t)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 1; i <= data.Length; i++)
        {
            pos.x = pos.x + data[i - 1].x * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
            pos.y = pos.y + data[i - 1].y * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
            pos.z = pos.z + data[i - 1].z * (Factorial(data.Length - 1) / (Factorial(data.Length - i) * Factorial(i - 1))) * Mathf.Pow(1 - t, data.Length - i) * Mathf.Pow(t, i - 1);
        }
        return pos;
    }

    private static float Factorial(int n)
    {
        if (n == 0)
        {
            return 1;
        }
        else
        {
            return n * Factorial(n - 1);
        }
    }
    
    
    /// <summary>
    /// 界面的切换动画
    /// </summary>
    /// <param name="panels">参与动画的节点列表</param>
    /// <param name="curIdx">当前的列表下标</param>
    /// <param name="nextIdx">需要切换到的节点下标</param>
    /// <param name="isForwardDir">正方向</param>
    public static void SwitchPanelAni(List<Transform> panels, int curIdx, int nextIdx, bool isForwardDir = true)
    {
        for (int i = 0; i < panels.Count; i++)
        {
            CanvasGroup canvas = panels[i].GetComponent<CanvasGroup>();
            if (canvas == null)
            {
                canvas = panels[i].gameObject.AddComponent<CanvasGroup>();
            }

            canvas.alpha = (i == curIdx || i == nextIdx) ? 1 : 0;
            canvas.interactable = (i == curIdx || i == nextIdx);
            canvas.blocksRaycasts = (i == curIdx || i == nextIdx);
            if (!panels[i].gameObject.activeSelf) panels[i].gameObject.SetActive(true);
            if (curIdx != nextIdx) panels[i].DOKill();
        }

        if (curIdx == nextIdx) return;
        float curPanelY = panels[curIdx].localPosition.y;
        float nextPanelY = panels[nextIdx].localPosition.y;
        panels[curIdx].localPosition = new Vector3(0, curPanelY);
        float x = curIdx < nextIdx ? -750 : 750;
        x = isForwardDir ? x : -x;
        panels[nextIdx].localPosition = new Vector3(x, nextPanelY);
        DoTweenManager.Instance.DoMoveTo(panels[curIdx].gameObject, new Vector3(-x, curPanelY), 0.3f, () =>
        {
            CanvasGroup canvasGroup = panels[curIdx].GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        });
        DoTweenManager.Instance.DoMoveTo(panels[nextIdx].gameObject, new Vector3(0, nextPanelY), 0.3f);
    }
}
