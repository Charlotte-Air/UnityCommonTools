using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DissloveControler : MonoBehaviour
{
    //1.开关Particlesystem（On Enable）,能够让vapewave材质的control值进行一个变化最好能依照一个曲线变化(AnimationCurve)
    // Start is called before the first frame update
    public AnimationCurve amc_1, amc_2;
    public float time;
    float t;
    public Material mat;
    bool starordown;

    // public float _disslove_Intensity;
    // public float _disslove_Intensity
    // {
    //     get { return _disslove_Intensity; }
    //     set
    //     {
    //         _disslove_Intensity = value;
    //         mat.SetFloat("_Disslove_Intensity", _disslove_Intensity);
    //     }
    // }
   // public string MatName = "";
    void Start()
    {
        
    }
    private void OnEnable()
    {
        starordown = true;
        t = 0;
        
    }
    // Update is called once per frame
    void Update()
    {
        float amcvalue_1 = amc_1.Evaluate(t);//括号中需要一个从0变化到1的值
        float amcvalue_2 = amc_2.Evaluate(t);//括号中需要一个从0变化到1的值

        if (starordown)
        {
            t += Time.deltaTime/time;//time.delatTime获取每一帧的延迟，如果我们的游戏为30帧，或为60帧，那么1s的时间就= time。deltatime*30或者60
                                //t += time.deltatime 必定会在1s钟的时候走到1；
            t=Mathf.Clamp01(t);
            if (t>=1)
            {
                starordown = false;
            }
        }

        // float amcvalue = (amcvalue_1 + amcvalue_2) / 2;
        mat.SetFloat("_Disslove_Intensity", amcvalue_1);
    }
}

#if UNITY_EDITOR
// [CustomEditor(typeof(FullScreenAdapter))]
// public class DissloveControlerEditor : Editor
// {
//     // private FullScreenAdapter _fullScreenAdapter;
//     private DissloveControler _dissloveControler;
//     public void OnEnable()
//     {
//         _dissloveControler = target as DissloveControler;
//     }
//
//     public void OnDisable()
//     {
//         _dissloveControler = null;
//     }
//
//     public override void OnInspectorGUI()
//     {
//
//         _dissloveControler._disslove_Intensity = EditorGUILayout.FloatField("_disslove_Intensity", _dissloveControler._disslove_Intensity);
//     }
// }
#endif
