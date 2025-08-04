using System;
using System.IO;
using UnityEngine;

namespace Framework.Utils
{
    /// <summary>
    /// 象转换JSON字符串
    //  struct与class均支持
    //  公开的成员变量
    //  公开的属性,包括只读属性
    //  对数组,List,Dictionary等都有良好的支持
    //  不会转换的内容:函数,静态变量,GameObject对象
    /// </summary>
    public class JsonTool
    {
        public static T ToObject<T>(string jsonTxt)
        {
            try
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonTxt);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                Debug.LogError($"解析Json字符串格式错误,原始字符串[{jsonTxt}],出错行号:{1},出错列号:{e.LineNumber},原始错误信息:{e.ToString()}");
                return default(T);
            }
            catch (Newtonsoft.Json.JsonSerializationException e)
            {
                Debug.LogError($"解析Json字符串格式错误,错误原因:将列表解析为单个对象,原始字符串[{jsonTxt}],原始错误信息:{e.ToString()}");
                return default(T);
            }
        }

        public static string ToJson<T>(T obj)
        {
#if UNITY_EDITOR
            try
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
            catch (Newtonsoft.Json.JsonReaderException e)
            {
                var rv = string.Format("ToJson错误,对象类型[{0}],出错行号:{1},出错列号:{2},错误信息:{3}", (obj == null ? "null" : obj.GetType().Name), e.LineNumber, e.LinePosition, e.ToString());
                Debug.LogError(rv);
                return rv;
            }
            catch (Newtonsoft.Json.JsonSerializationException e)
            {
                var rv = string.Format("ToJson错误,对象类型[{0}],错误信息:{1}", (obj == null ? "null" : obj.GetType().Name), e.ToString());
                Debug.LogError(rv);
                return rv;
            }
#else
            try 
            {
                return JsonUtility.ToJson(obj);
            } 
            catch (Exception e) 
            {
                string rv = string.Format("ToJson错误,对象类型[{0}],错误信息:{1}", (obj == null ? "null" : obj.GetType().Name), e.ToString());
                Debug.LogError(rv);
                return rv;
            }
#endif
        }

        // 将对象转换为Bson
        public static byte[] ToBson<T>(T obj)
        {
#if UNITY_EDITOR
            using (var stream = new MemoryStream())
            {
                using (var writer = new Newtonsoft.Json.Bson.BsonWriter(stream))
                {
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    serializer.Serialize(writer, obj);
                }

                return stream.ToArray();
                //var serialized = Convert.ToBase64String(serializedData);
            }
#else
            return StringTool.byteEncoding.GetBytes(ToJson(obj));
#endif
        }

        // 将Bson转换为对象
        public static T ToObject<T>(byte[] serializedData)
        {
#if UNITY_EDITOR
            using (var stream = new MemoryStream(serializedData))
            {
                using (var reader = new Newtonsoft.Json.Bson.BsonReader(stream))
                {
                    var serializer = new Newtonsoft.Json.JsonSerializer();
                    return serializer.Deserialize<T>(reader);
                }
            }
#else
            return ToObject<T>(StringTool.byteEncoding.GetString(serializedData));
#endif
        }
    }
}