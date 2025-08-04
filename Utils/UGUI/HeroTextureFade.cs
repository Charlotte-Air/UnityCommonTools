using System;
using UnityEngine;

public class HeroTextureFade : MonoBehaviour
{
    public Material mat;
    private float _dir;
    private float _appearOffset;
    private int _appearOffsetState;
    private float _appearOffsetMin, _appearOffsetMax;
    private float _appearOffsetValue;
    private int _sign;
    private Action _callback;
    void Start() { }

    private void OnEnable()
    {
        _dir = -1;
        _appearOffset = 0;
        _appearOffsetState = 0;
        _appearOffsetMin = 0;
        _appearOffsetMax = 7;
        _appearOffsetValue = 0.2f;
        _sign = 0;
    }

    void FixedUpdate()
    {
        if (_sign == 1 && _appearOffsetValue > 0)
        {
            if (_appearOffsetState == 1)
            {
                _appearOffset = _appearOffset + _appearOffsetValue;
                if (_appearOffset >= _appearOffsetMax)
                {
                    _appearOffset = _appearOffsetMax;
                    _sign = 0;
                    if (_callback != null)
                    {
                        _callback();
                        _callback = null;
                    }
                }
            }else if (_appearOffsetState == -1)
            {
                _appearOffset = _appearOffset - _appearOffsetValue;
                if (_appearOffset <= _appearOffsetMin)
                {
                    _appearOffset = _appearOffsetMin;
                    _sign = 0;

                    if (_callback != null)
                    {
                        _callback();
                        _callback = null;
                    }
                }
            }

            // Debug.Log("_appearOffset ：  " + _appearOffset);
            mat.SetFloat("_AppearOffset", _appearOffset);
        }
    }

    public void SetHeroDir(float dir)
    {
        if (dir == -1) _dir = 0;
        else if (dir == 1) _dir = 1;
        mat.SetFloat("_ChangeDirection", _dir);
        // Debug.Log("SetHeroDir dir : " + _dir);
    }

    public void HideHeroTextureFade(float duration, Action callback)
    {
        _callback = callback;
        _appearOffset = 2;
        _appearOffsetState = 1;
        _sign = 1;
        _appearOffsetValue = (_appearOffsetMax-2)/(duration*Application.targetFrameRate);
        // Debug.Log(" value : " + _appearOffsetValue);
    }

    public void ShowHeroTextureFade(float duration, Action callback)
    {
        _callback = callback;
        _appearOffset = 7;
        _appearOffsetState = -1;
        _sign = 1;
        _appearOffsetValue = (_appearOffsetMax-2)/(duration*Application.targetFrameRate);
        // Debug.Log(" value : " + _appearOffsetValue);
    }

    public void ShowHeroTexture()
    {
        _sign = 0;
        _appearOffset = 0;
        mat.SetFloat("_AppearOffset", _appearOffset);
    }
}
