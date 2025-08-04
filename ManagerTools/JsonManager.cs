using System;
using System.IO;
using System.Text;
using UnityEngine;
using Framework.Manager;
using System.Collections.Generic;

public class JsonManager
{
    private static Dictionary<string, JSONObject> allJsonFileContent = new Dictionary<string, JSONObject>();
    
    public static void PreloadJsonFile(string filename, Action<int, bool> onfinishCallback)
    {
        if (JsonIsExisted(filename))
        {
            onfinishCallback(1, true);
            return;
        }
        var list = new List<string> { filename };
        PreloadJsonFile(list, onfinishCallback);
    }

    public static void PreloadJsonFile(List<string> filenames, Action<int, bool> onfinishCallback)
    {
        var count = filenames.Count;
        if (count <= 0)
        {
            onfinishCallback(1, true);
            return;
        }

        var loadedCount = 0;
        for (var i = 0; i < count; i++)
        {
            var key = filenames[i];
            if (!JsonIsExisted(key))
                continue;
            loadedCount++;
            onfinishCallback(loadedCount / count, loadedCount == count);
        }
    }
    
    public static string GetString(JSONObject obj)
    {
        return obj.type == JSONObject.Type.STRING ? obj.str : null;
    }

    public static Int32 GetInt(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToInt32(obj.n) : 0;
    }

    public static UInt32 GetUint(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToUInt32(obj.n) : (uint)0;
    }

    public static float GetFloat(JSONObject obj)
    {
        if (obj.type == JSONObject.Type.NUMBER)
            return obj.n;
        return 0;
    }

    public static UInt16 GetUshort(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToUInt16(obj.n) : (ushort)0;
    }

    public static Int16 GetShort(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToInt16(obj.n) : (short)0;
    }

    public static Byte GetByte(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToByte(obj.n) : (byte)0;
    }

    public static SByte GetSByte(JSONObject obj)
    {
        return obj.type == JSONObject.Type.NUMBER ? Convert.ToSByte(obj.n) : (sbyte)0;
    }

    public static string SafeGetString(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.STRING)
            return obj.str;
        return null;
    }

    public static Int32 SafeGetInt(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToInt32(obj.n);
        return 0;
    }

    public static UInt32 SafeGetUint(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToUInt32(Mathf.Abs(obj.n));
        return 0;
    }

    public static float SafeGetFloat(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return obj.n;
        return 0;
    }

    public static UInt16 SafeGetUshort(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToUInt16(Mathf.Abs(obj.n));
        return 0;
    }

    public static Int16 SafeGetShort(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToInt16(obj.n);
        return 0;
    }

    public static Byte SafeGetByte(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToByte(Mathf.Abs(obj.n));
        return 0;
    }

    public static SByte SafeGetSByte(JSONObject obj)
    {
        if (obj != null && obj.type == JSONObject.Type.NUMBER)
            return Convert.ToSByte(obj.n);
        return 0;
    }

    public static JSONObject LoadJson(string path)
    {
        if (!allJsonFileContent.ContainsKey(path) || allJsonFileContent[path] == null)
        {
            Debug.LogWarning("LoadJson Error!");
            allJsonFileContent[path] = new JSONObject(File.ReadAllText(path, Encoding.UTF8));
            return allJsonFileContent[path];
        }
        else
            return allJsonFileContent[path];
    }

    public static void SaveJson(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    public static bool JsonIsExisted(string path)
    {
        return allJsonFileContent.ContainsKey(path);
    }
}