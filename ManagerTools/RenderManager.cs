using UnityEngine;
using Framework.Utils;
using System.Collections.Generic;

public class RenderManager
{
    private static RenderManager _instance;
    public static RenderManager Instance
    {
        get
        {
            if (_instance == null) 
                _instance = new RenderManager();
            return _instance;
        }
    }

    private JSONObject _jsonObj = null;
    private Dictionary<string, RenderParam> _paramList = new Dictionary<string, RenderParam>();

    
    public void SetJsonObj(JSONObject obj)
    {
        _jsonObj = obj;
    }

    
    public RenderParam GetRenderParam(string key)
    {
        if (_paramList.ContainsKey(key))
        {
            return _paramList[key];
        }
        else
        {
            JSONObject jsonObj = _jsonObj.GetField(key);
            if (jsonObj == null)
            {
                Debug.LogError("renderparam key is null");
            }
            RenderParam renderP = JsonTool.ToObject<RenderParam>(jsonObj.ToString());
            renderP.Fog.Mode = FogMode.Linear;
            _paramList.Add(key, renderP);
            return renderP;
        }
    }

    
    public void RefreshEnvironmentParam(string key, Light _obj, Light _cha = null)
    {
        Debug.Log("RefreshEnvironmentParam:key = "+ key);
        RenderParam param = GetRenderParam(key);
        SetEnvironmentParam(param, _obj, _cha);
    }

    
    private void SetEnvironmentParam(RenderParam param, Light obj, Light cha)
    {
        // 设置灯光
        SetRenderLight(param.Light, obj);
        // 设置雾的参数
        SetRenderFog(param.Fog);
        // 设置角色光
        if (cha != null)
            SetRenderLight(param.Character, cha); 
    }

    
    private void SetRenderLight(RenderLight pLight, Light obj)
    {
        Vector3 rotation = new Vector3(pLight.Rotation[0], pLight.Rotation[1], pLight.Rotation[2]);
        obj.transform.localRotation = Quaternion.Euler(rotation);

        Color lightColor;
        ColorUtility.TryParseHtmlString(pLight.Color, out lightColor);
        obj.color = lightColor;

        obj.intensity = pLight.Intensity;
    }

    
    public void SetRenderFog(RenderFog pFog)
    {
        RenderSettings.fog = pFog.State;
        if (pFog.State)
        {
            Color fogColor;
            ColorUtility.TryParseHtmlString(pFog.Color, out fogColor);
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = pFog.Mode;
            if (pFog.Mode == FogMode.Linear)
            {
                RenderSettings.fogStartDistance = pFog.Start;
                RenderSettings.fogEndDistance = pFog.End;
            }
            else
            {
                RenderSettings.fogDensity = pFog.Density;
            }
        }
    }

    
    public bool UpdateProgressEnvironmentParam(float dt)
    {
        float t = progressTime > 0 ? curTime / progressTime : 1;
        if (t > 1) t = 1;

        if (isLightProgress && _light != null)
        {
            _light.transform.localRotation = Quaternion.Lerp(beforeLight.Rotation, afterLight.Rotation, t);
            _light.color = Color.Lerp(beforeLight.Color, afterLight.Color, t);
            _light.intensity = Mathf.Lerp(beforeLight.Intensity, afterLight.Intensity, t);
        }

        if (isFogProgress)
        {
            RenderSettings.fogColor = Color.Lerp(beforeFog.Color, afterFog.Color, t);
            RenderSettings.fogStartDistance = Mathf.Lerp(beforeFog.Start, afterFog.Start, t);
            RenderSettings.fogEndDistance = Mathf.Lerp(beforeFog.End, afterFog.End, t);
        }

        if (isCharacterProgress && _character != null)
        {
            _character.transform.localRotation = Quaternion.Lerp(beforeCharacter.Rotation, afterCharacter.Rotation, t);
            _character.color = Color.Lerp(beforeCharacter.Color, afterCharacter.Color, t);
            _character.intensity = Mathf.Lerp(beforeCharacter.Intensity, afterCharacter.Intensity, t);
        }

        if (isFogProgress || isLightProgress || isCharacterProgress)
        {
            curTime += dt;
        }

        if (t == 1)
        {
            isFogProgress = false;
            isLightProgress = false;
            isCharacterProgress = false;
            progressTime = 0;
            curTime = 0;
            return true;
        }
        return false;
    }

    
    float curTime = 0;
    float progressTime = 0;
    struct ProgressRenderFog
    {
        public Color Color;
        public float Start;
        public float End;
    }
    bool isFogProgress = false;
    ProgressRenderFog beforeFog;
    ProgressRenderFog afterFog;

    struct ProgressRenderLight
    {
        public Quaternion Rotation;
        public Color Color;
        public float Intensity;
    }
    Light _light;
    bool isLightProgress = false;
    ProgressRenderLight beforeLight;
    ProgressRenderLight afterLight;

    Light _character;
    bool isCharacterProgress = false;
    ProgressRenderLight beforeCharacter;
    ProgressRenderLight afterCharacter;

    public void SetProgressEnvironmentParam(Light obj, Light cha, string key, float time = 0)
    {
        Debug.Log("RefreshEnvironmentParam:key = " + key);
        RenderParam _param = GetRenderParam(key);

        curTime = 0;
        progressTime = time;

        RenderLight pLight = _param.Light;
        if (obj != null)
        {
            _light = obj;
            if (progressTime > 0)
            {
                beforeLight.Rotation = obj.transform.localRotation;
                beforeLight.Color = obj.color;
                beforeLight.Intensity = obj.intensity;

                afterLight.Rotation = Quaternion.Euler(new Vector3(pLight.Rotation[0], pLight.Rotation[1], pLight.Rotation[2]));
                ColorUtility.TryParseHtmlString(pLight.Color, out afterLight.Color);
                afterLight.Intensity = pLight.Intensity;

                isLightProgress = true;
            }
            else
            {
                isLightProgress = false;

                Vector3 rotation = new Vector3(pLight.Rotation[0], pLight.Rotation[1], pLight.Rotation[2]);
                obj.transform.localRotation = Quaternion.Euler(rotation);

                Color lightColor;
                ColorUtility.TryParseHtmlString(pLight.Color, out lightColor);
                obj.color = lightColor;

                obj.intensity = pLight.Intensity;
            }
        }
        else
        {
            _light = null;
            isLightProgress = false;
        }

        RenderFog pFog = _param.Fog;
        RenderSettings.fog = pFog.State;
        if (pFog.State)
        {
            if (RenderSettings.fogMode == pFog.Mode && progressTime > 0)
            {
                beforeFog.Color = RenderSettings.fogColor;
                beforeFog.Start = RenderSettings.fogStartDistance;
                beforeFog.End = RenderSettings.fogEndDistance;

                ColorUtility.TryParseHtmlString(pFog.Color, out afterFog.Color);
                afterFog.Start = pFog.Start;
                afterFog.End = pFog.End;

                isFogProgress = true;
            }
            else
            {
                isFogProgress = false;
                Color fogColor;
                ColorUtility.TryParseHtmlString(pFog.Color, out fogColor);
                RenderSettings.fogColor = fogColor;
                RenderSettings.fogMode = pFog.Mode;
                if (pFog.Mode == FogMode.Linear)
                {
                    RenderSettings.fogStartDistance = pFog.Start;
                    RenderSettings.fogEndDistance = pFog.End;
                }
                else
                {
                    RenderSettings.fogDensity = pFog.Density;
                }
            }
        }
        else
        {
            isFogProgress = false;
        }

        RenderLight pCharacter = _param.Character;
        if (cha != null)
        {
            _character = cha;
            if (progressTime > 0)
            {
                beforeCharacter.Rotation = _character.transform.localRotation;
                beforeCharacter.Color = _character.color;
                beforeCharacter.Intensity = _character.intensity;

                afterCharacter.Rotation = Quaternion.Euler(new Vector3(pCharacter.Rotation[0], pCharacter.Rotation[1], pCharacter.Rotation[2]));
                ColorUtility.TryParseHtmlString(pCharacter.Color, out afterCharacter.Color);
                afterCharacter.Intensity = pCharacter.Intensity;

                isCharacterProgress = true;
            }
            else
            {
                isCharacterProgress = false;

                Vector3 rotation = new Vector3(pCharacter.Rotation[0], pCharacter.Rotation[1], pCharacter.Rotation[2]);
                _character.transform.localRotation = Quaternion.Euler(rotation);

                Color lightColor;
                ColorUtility.TryParseHtmlString(pCharacter.Color, out lightColor);
                _character.color = lightColor;

                _character.intensity = pCharacter.Intensity;
            }
        }
        else
        {
            _character = null;
            isCharacterProgress = false;
        }
    }
    
    
}

public struct RenderParam
{
    public RenderLight Light;
    public RenderFog Fog;
    public RenderLight Character;
}

public struct RenderLight
{
    public float[] Rotation;
    public string Color;
    public float Intensity;
}

public struct RenderFog
{
    public bool State;
    public FogMode Mode;
    public string Color;
    public float Density;
    public float Start;
    public float End;
}