using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;

public class ShaderKeywords : EditorWindow
{
    string MatPathName; //分割后路径
    static string MatPath_k; //总路径

    [MenuItem("Tools/剔除材质无用Keywords")]
    public static void OpenWindow()
    {
        if (Application.isPlaying)
        {
            return;
        }

        ShaderKeywords window = GetWindow<ShaderKeywords>("剔除材质无用Keywords工具");
        window.minSize = new Vector2(380, 700);
        window.maxSize = new Vector2(380, 700);
        window.Show();

        //RemoveRedundantMaterialShaderKeywords();
    }

    private void OnEnable()
    {
    }

    private void OnGUI()
    {
        GUILayout.Space(20);
        if (GUILayout.Button("选择材质球路径"))
        {
            //根据Unity Editor 内置接口，我们打开夹获取文件夹
            MatPath_k = EditorUtility.OpenFolderPanel("选择材质球路径", "", "");

            //string s= Application.dataPath.Remove(Application.dataPath.Length - 6);
            MatPath_k = MatPath_k.Remove(0, Application.dataPath.Length - 6);
            string[] ids = AssetDatabase.FindAssets("t:Material", new string[] { MatPath_k });
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                Debug.Log(path);
                Material material = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                RemoveRedundantMaterialShaderKeywords(material);
            }
        }

        GUILayout.Space(20);
        if (GUILayout.Button("遍历所有材质"))
        {
            string[] ids = AssetDatabase.FindAssets("t:Material", new string[] { "Assets" });
            for (int i = 0; i < ids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(ids[i]);
                Debug.Log(path);
                Material material = AssetDatabase.LoadMainAssetAtPath(path) as Material;
                RemoveRedundantMaterialShaderKeywords(material);
            }
        }
    }

    public static void RemoveRedundantMaterialShaderKeywords(Material material)
    {
        if (material == null) return;
        if (material.shader.name == "Hidden/InternalErrorShader") return;
        List<string> materialKeywordsLst = new List<string>(material.shaderKeywords);
        List<string> shaderKeywordsLst = new List<string>();
        var getKeywordsMethod = typeof(ShaderUtil).GetMethod("GetShaderGlobalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
        string[] keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { material.shader });
        shaderKeywordsLst.AddRange(keywords);

        getKeywordsMethod = typeof(ShaderUtil).GetMethod("GetShaderLocalKeywords", BindingFlags.Static | BindingFlags.NonPublic);
        keywords = (string[])getKeywordsMethod.Invoke(null, new object[] { material.shader });
        shaderKeywordsLst.AddRange(keywords);

        List<string> notExistKeywords = new List<string>();
        foreach (var each in materialKeywordsLst)
        {
            if (!shaderKeywordsLst.Contains(each))
            {
                notExistKeywords.Add(each);
            }
        }

        foreach (var each in notExistKeywords)
        {
            materialKeywordsLst.Remove(each);
        }

        if (materialKeywordsLst.Count > 0)
        {
            material.shaderKeywords = materialKeywordsLst.ToArray();
        }
    }
}