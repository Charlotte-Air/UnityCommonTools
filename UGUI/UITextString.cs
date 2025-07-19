using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

public class UITextString
{
    private static UITextString instance;
    public static UITextString Instance
    {
        get
        {
            if (instance == null || instance.Equals(null))
            {
                instance = new UITextString();
                instance.init();
            }
            return instance;
        }
    }

    //private static Dictionary<string, string> uiTextTempPool = new Dictionary<string, string>();
    private static Dictionary<string, string> uiTextTempMap = new Dictionary<string, string>();

    private XmlDocument xmlDoc;
    private XmlNode root;
    private string filePath = "/Data/Configs/Sources/UiString.xml";
    private string poolPath = "/Data/Configs/Sources/StrPool.dict";

    private void init()
    {
        filePath = Application.dataPath + filePath;
        poolPath = Application.dataPath + poolPath;
        ReLoadPool();
        ReLoadFile();
    }

    public void ReLoadPool()
    {
        try
        {
            //byte[] bytes = System.IO.File.ReadAllBytes(poolPath);
            //ByteReader br = new ByteReader(bytes);
            //uiTextTempPool = br.ReadDictionary();
//            uiTextTempPool.Clear();
//             xmlDoc = new XmlDocument();
//             xmlDoc.Load(poolPath);
//             root = xmlDoc.SelectSingleNode("//Object");
//             if (root != null)
//             {
//                 foreach(XmlElement ele in root.ChildNodes)
//                 {
//                     string id = ele.GetAttribute("ID");
//                     string content = ele.GetAttribute("Content");
//                     uiTextTempPool.Add(id, content);
//                 }
//             }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }
    public void ReLoadFile()
    {
        try
        {
            uiTextTempMap.Clear();
            xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            root = xmlDoc.SelectSingleNode("//Object");
            if (root != null)
            {
                foreach (XmlElement ele in root.ChildNodes)
                {
                    string id = ele.GetAttribute("Key");
                    string content = ele.GetAttribute("Value");
                    uiTextTempMap.Add(id, content);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning(ex);
        }
    }

    public string GetUIText(string id, out bool found)
    {
        if (string.IsNullOrEmpty(id))
        {
            found = true;
            return string.Empty;
        }

        string result = string.Empty;
        //string found = string.Empty;
        if (!uiTextTempMap.TryGetValue(id, out result))
        {
            found = false;
            Debug.LogWarningFormat("Can't found config id {0}.", id);
            return id;
        }
        //else
        //{
        //    if (!uiTextTempPool.TryGetValue(found, out result))
        //    {
        //        Debug.LogWarningFormat("Can't found Pool id {0}.", found);
        //    }
        //}
        found = true;
        if (result.Contains("[n]"))
        {
            result = result.Replace("[n]", "\n");
        }

        result = result.Replace("[color]", "</color>");
        result = result.Replace("[/color]", "</color>");

        Regex regexObj = new Regex(@"\[color=#(\w{8}|\w{6})]");
        Match matchResult = regexObj.Match(result);
        List<string> resultList = new List<string>();
        while (matchResult.Success)
        {
            resultList.Add(matchResult.Value);
            matchResult = matchResult.NextMatch();
        }

        for (int i = 0; i < resultList.Count; i++)
        {
            string s1 = resultList[i];
            string s2 = string.Copy(s1);
            s2 = s2.Replace("[", "<");
            s2 = s2.Replace("]", ">");
            result = result.Replace(s1, s2);
        }

        return result;
    }
}