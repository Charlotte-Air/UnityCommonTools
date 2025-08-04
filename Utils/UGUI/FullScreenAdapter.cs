using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FullScreenAdapter : MonoBehaviour
{
    public bool IsAdaptationLiuhai;
    public float FullScreenLimitWidth = 1468;
    public float FullScreenLimitHeight = 750;

    public float FullScrLeftGap = 88f;

    public float FullScrLeftGapProp
    {
        get { return FullScrLeftGap; }
        set
        {
            if ((int) (value * 100) == (int) (FullScrLeftGap * 100))
            {
                return;
            }

            FullScrLeftGap = value;
            Refresh();
        }
    }

    public float FullScrRightGap = 88f;

    public float FullScrRightGapProp
    {
        get => FullScrRightGap;
        set
        {
            if ((int) (value * 100) == (int) (FullScrRightGap * 100))
            {
                return;
            }

            FullScrRightGap = value;
            Refresh();
        }
    }

    public float D_UILimitWidth = 1650;
    public float D_UILimitHeight = 750;

    private Vector3 originPos;
    private RectTransform trans;
    void Awake()
    {
        trans = gameObject.GetComponent<RectTransform>();
        originPos = trans.localPosition;

        Refresh();
    }

    public void Refresh()
    {
        if (!IsAdaptationLiuhai)
        {
            return;
        }

        if (!CheckFullScreen()) {
            return;
        }

        // 监测全面屏 并对全面屏刘海适配
        // 贴左
        if(trans.anchorMax.x == 0 && trans.anchorMin.x == 0){
            if (CheckFullLongScreen())
            {
                trans.anchoredPosition3D = new Vector3(originPos.x + FullScrLeftGap, originPos.y, originPos.z);
            }
            else
            {
                trans.anchoredPosition3D = new Vector3(originPos.x, originPos.y, originPos.z);
            }
        }// 贴右
        else if (trans.anchorMax.x == 1 && trans.anchorMin.x == 1){
            if (CheckFullLongScreen())
            {
                trans.anchoredPosition3D = new Vector3(originPos.x - FullScrRightGap, originPos.y, originPos.z);
            }
            else
            {
                trans.anchoredPosition3D = new Vector3(originPos.x, originPos.y, originPos.z);
            }
        }
        else if(trans.anchorMax.x == 1)
        {
            trans.offsetMin = new Vector2(trans.offsetMin.x + FullScrLeftGap, trans.offsetMin.y);
            //trans.localPosition = new Vector3(originPos.x + CommonDefine.FullScrLeftGap, originPos.y, originPos.z);
        }
    }

    /// <summary>
    /// 检查当前是否是全面屏
    /// </summary>
    /// <returns></returns>
    public bool CheckFullScreen()
    {
        float width = GameConfig.ScreenWidth;
        float height = GameConfig.ScreenHeight;
        if (width / height >= FullScreenLimitWidth / FullScreenLimitHeight) // 17.6/9
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 检查当前是否是全面屏
    /// </summary>
    /// <returns></returns>
    public bool CheckFullLongScreen()
    {
        float width = GameConfig.ScreenWidth;
        float height = GameConfig.ScreenHeight;
        if (width / height >= D_UILimitWidth / D_UILimitHeight) // 19.8/9
        {
            return true;
        }
        return false;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(FullScreenAdapter))]
public class FullScreenAdapterEditor : Editor
{
    private FullScreenAdapter _fullScreenAdapter;
    public void OnEnable()
    {
        _fullScreenAdapter = target as FullScreenAdapter;
    }

    public void OnDisable()
    {
        _fullScreenAdapter = null;
    }

    public override void OnInspectorGUI()
    {
        _fullScreenAdapter.IsAdaptationLiuhai = EditorGUILayout.Toggle("需要适配屏幕刘海", _fullScreenAdapter.IsAdaptationLiuhai);

        if (_fullScreenAdapter.IsAdaptationLiuhai)
        {
            _fullScreenAdapter.FullScrLeftGapProp = EditorGUILayout.FloatField("FullScrLeftGap", _fullScreenAdapter.FullScrLeftGapProp);
            _fullScreenAdapter.FullScrRightGapProp = EditorGUILayout.FloatField("FullScrRightGap", _fullScreenAdapter.FullScrRightGapProp);

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("全屏尺寸比例下限：该尺寸以下不做刘海适配。");
            EditorGUILayout.LabelField("即:什么样的尺寸比例定义为全面屏。");
            EditorGUILayout.LabelField("全屏尺寸下限Width :         " + _fullScreenAdapter.FullScreenLimitWidth);
            EditorGUILayout.LabelField("全屏尺寸下限Height :        " + _fullScreenAdapter.FullScreenLimitHeight);
            EditorGUILayout.LabelField("全屏尺寸比例                  " + _fullScreenAdapter.FullScreenLimitWidth / _fullScreenAdapter.FullScreenLimitHeight);
        }

        EditorGUILayout.Space(30);
        EditorGUILayout.LabelField("适配尺寸限制比例：该尺寸以上的左右适配方案以该尺寸为标准。");
        EditorGUILayout.LabelField("即:1800/750的尺寸比例，按" + _fullScreenAdapter.D_UILimitWidth + "/" +
                                   _fullScreenAdapter.D_UILimitHeight + "的尺寸比例做。");
        EditorGUILayout.LabelField("适配尺寸限制Width :        " + _fullScreenAdapter.D_UILimitWidth);
        EditorGUILayout.LabelField("适配尺寸限制Height :       " + _fullScreenAdapter.D_UILimitHeight);
        EditorGUILayout.LabelField("适配尺寸限制比例              " + _fullScreenAdapter.D_UILimitWidth / _fullScreenAdapter.D_UILimitHeight);
        EditorGUILayout.Space();
    }
}
#endif