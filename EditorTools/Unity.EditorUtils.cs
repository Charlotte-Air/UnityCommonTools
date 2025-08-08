using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using Framework.Utils.Unity;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Text.RegularExpressions;

public static class UnityEditorUtils
{
    [MenuItem("Assets/Find References/Find References All", false, 10)]
    static private void FindAll()
    {
        Find(new List<string>(){".prefab",".unity",".mat",".asset"});
    }
 
    [MenuItem("Assets/Find References/Find References in .prefab", false, 11)]
    static private void FindPrefabs()
    {
        Find(new List<string>(){".prefab"});
    }

    [MenuItem("Assets/Find References/Find References in .unity", false, 12)]
    static private void FindScenes()
    {
        Find(new List<string>(){".unity"});
    }

    [MenuItem("Assets/Find References/Find References in .mat", false, 13)]
    static private void FindMaterials()
    {
        Find(new List<string>(){".mat"});
    }

    [MenuItem("Assets/Find References/Find References in .asset", false, 14)]
    static private void FindAssets()
    {
        Find(new List<string>(){".asset"});
    }

    static private void Find(List<string> withoutExtensions)
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (!string.IsNullOrEmpty(path))
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
                .Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;
 
            EditorApplication.update = delegate()
            {
                string file = files[startIndex];
            
                 bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);
 
                if (Regex.IsMatch(File.ReadAllText(file), guid))
                {
                    Debug.Log(file, AssetDatabase.LoadAssetAtPath<Object>(GetRelativeAssetsPath(file)));
                }
 
                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                }
 
            };
        }
    }

    static private bool VFind()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        return (!string.IsNullOrEmpty(path));
    }
 
    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
    
    public static void GizmosDrawCircle(Vector3 center, Vector3 up, float redius)
    {
        Quaternion R = Quaternion.FromToRotation(Vector3.up, up);
        float stepa = 0.3f;
        Vector3 o = center + R * new Vector3(Mathf.Sin(0) * redius, 0, Mathf.Cos(0) * redius);
        Vector3 p = o;
        for (float a = stepa; a < Mathf.PI * 2f; a += stepa)
        {
            Vector3 c = center + R * new Vector3(Mathf.Sin(a) * redius, 0, Mathf.Cos(a) * redius);
            Gizmos.DrawLine(p, c);
            p = c;
        }

        Gizmos.DrawLine(p, o);
    }

    public static void GizmosDrawArc(Vector3 from, Vector3 to, Vector3 up, float height = 0f)
    {
        var dis = Vector3.Distance(to, from);
        if (height == 0f)
        {
            height = dis / 2f;
        }

        var dir = (to - from).normalized;
        var normal = Vector3.Cross(dir, up);
        var down = -Vector3.Cross(normal, dir);
        var c1 = from + (to - from) * 0.5f;
        Vector3 center = c1 + down * height;
        var redius = Vector3.Distance(from, center);
        Vector3 p = from;
        for (int i = 0; i < 24; ++i)
        {
            var o = from + (to - from).normalized * dis / 24f * i;
            var c = center + (o - center).normalized * redius;
            Gizmos.DrawLine(p, c);
            p = c;
        }

        Gizmos.DrawLine(p, to);
    }

    public static string HandleCopyPaste(int controlID)
    {
        if (controlID == GUIUtility.keyboardControl)
        {
            if (Event.current.type == UnityEngine.EventType.KeyUp &&
                (Event.current.modifiers == EventModifiers.Control ||
                 Event.current.modifiers == EventModifiers.Command))
            {
                if (Event.current.keyCode == KeyCode.C)
                {
                    Event.current.Use();
                    UnityEngine.TextEditor editor =
                        (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                            GUIUtility.keyboardControl);
                    editor.Copy();
                }
                else if (Event.current.keyCode == KeyCode.V)
                {
                    Event.current.Use();
                    UnityEngine.TextEditor editor =
                        (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                            GUIUtility.keyboardControl);
                    editor.Paste();
#if UNITY_5_3_OR_NEWER || UNITY_5_3
                    return editor.text; //以及更高的unity版本中editor.content.text已经被废弃，需使用editor.text代替
#else
                    return editor.content.text;
#endif
                }
                else if (Event.current.keyCode == KeyCode.A)
                {
                    Event.current.Use();
                    UnityEngine.TextEditor editor =
                        (UnityEngine.TextEditor)GUIUtility.GetStateObject(typeof(UnityEngine.TextEditor),
                            GUIUtility.keyboardControl);
                    editor.SelectAll();
                }
            }
        }

        return null;
    }

    public static string TextField(string value, params GUILayoutOption[] options)
    {
        int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
        if (textFieldID == 0)
            return value;

        //处理复制粘贴的操作
        value = HandleCopyPaste(textFieldID) ?? value;

        return GUILayout.TextField(value, options);
    }

    public static string TextField(Rect rect, string value)
    {
        int textFieldID = GUIUtility.GetControlID("TextField".GetHashCode(), FocusType.Keyboard) + 1;
        if (textFieldID == 0)
            return value;

        //处理复制粘贴的操作
        value = HandleCopyPaste(textFieldID) ?? value;

        return GUI.TextField(rect, value);
    }

    public static List<Object> LoadAssetsAtPath(string root, System.Type type)
    {
        List<Object> ret = new List<Object>();

        string fullPath = Application.dataPath + "/../" + root;

        List<string> files = FileUtils.GetFilesInDirectory(fullPath, false);
        foreach (string file in files)
        {
            string f = file.Replace(Application.dataPath, "Assets");
            Object asset = AssetDatabase.LoadAssetAtPath(f, type);
            if (asset != null)
            {
                ret.Add(asset);
            }
        }

        return ret;
    }

    private static System.Type GetType(string typeName)
    {
        System.Reflection.Assembly[] ass = System.AppDomain.CurrentDomain.GetAssemblies();
        foreach (var a in ass)
        {
            System.Type[] types = a.GetTypes();
            foreach (var t in types)
            {
                if (t.Name.Equals(typeName))
                    return t;
            }
        }

        return null;
    }

    private static Dictionary<string, MethodInfo> funcs = new Dictionary<string, MethodInfo>();

    public static object Invoke(string className, string funcName, object obj, object[] args)
    {
        string key = className + "." + funcName;

        MethodInfo mi;
        if (!funcs.TryGetValue(key, out mi))
        {
            System.Type t = UnityEditorUtils.GetType(className);
            if (t != null)
            {
                mi = t.GetMethod(funcName,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                funcs.Add(key, mi);
            }
        }

        if (mi != null)
        {
            return mi.Invoke(obj, args);
        }
        else
        {
            LogHelper.WarningFormat("Cant find {0} in {1}.", funcName, className);
            return null;
        }
    }

    public static T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
        }

        return existingAsset;
    }

    public static Camera GetSceneViewCamera()
    {
        if (SceneView.lastActiveSceneView != null)
        {
            return SceneView.lastActiveSceneView.camera;
        }

        if (SceneView.currentDrawingSceneView != null)
        {
            return SceneView.currentDrawingSceneView.camera;
        }

        return null;
    }

    public static bool GetTextureOriginalSize(Texture asset, out int width, out int height)
    {
        if (asset != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer != null)
            {
                object[] args = new object[2] { 0, 0 };
                Invoke("TextureImporter", "GetWidthAndHeight", importer, args);
                width = (int)args[0];
                height = (int)args[1];

                return true;
            }
        }

        height = width = 0;
        return false;
    }

    public static void SaveTexture(Texture2D texture, string path, TextureImporterCompression comporession,
        bool readable, bool mipmapEnabled)
    {
        string fullPath = System.IO.Path.GetFullPath(path);
        fullPath = FileUtils.RemovePathFileName(fullPath);
        FileUtils.CreateDirectory(fullPath);

        int w = texture.width;
        int h = texture.height;
        RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(texture, rt);
        RenderTexture old = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D dest = new Texture2D(w, h, TextureFormat.ARGB32, true, false);
        dest.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        dest.Apply();

        System.IO.File.WriteAllBytes(path, dest.EncodeToPNG());

        RenderTexture.active = old;
        RenderTexture.ReleaseTemporary(rt);
        Texture2D.DestroyImmediate(dest);

        SetTextureAssetFormat(path, comporession, readable, mipmapEnabled);
    }

    public static bool TextureSetReadWriteEnabled(Texture2D texture, bool enabled, bool force)
    {
        return AssetSetReadWriteEnabled(AssetDatabase.GetAssetPath(texture), enabled, force);
    }

    public static bool AssetSetReadWriteEnabled(string path, bool enabled, bool force)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        if (ti == null)
        {
            return false;
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        if (force || settings.readable != enabled)
        {
            settings.readable = enabled;
            ti.SetTextureSettings(settings);
            ReimportAsset(path);
        }

        return true;
    }

    public static bool SetTextureAssetFormat(string path, TextureImporterCompression comporession, bool readable,
        bool mipmapEnabled)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;

        if (ti == null)
        {
            return false;
        }

        TextureImporterSettings settings = new TextureImporterSettings();
        ti.ReadTextureSettings(settings);

        ti.textureType = TextureImporterType.Default;
        ti.textureCompression = comporession;

        settings.npotScale = TextureImporterNPOTScale.None;
        settings.readable = readable;
        settings.mipmapEnabled = mipmapEnabled;
        ti.SetTextureSettings(settings);

        ReimportAsset(path);

        return true;
    }

    public static void ReimportAsset(string path, ImportAssetOptions options =
        ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport)
    {
        try
        {
            AssetDatabase.StartAssetEditing();
            AssetDatabase.ImportAsset(path, options);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }
    }

    public static void CopyAssetTo(string[] fullpaths, string relativePathToAssets)
    {
        for (int i = 0; i < fullpaths.Length; i++)
        {
            string source = fullpaths[i];
            if (string.IsNullOrEmpty(source))
            {
                continue;
            }

            string fileName = FileUtils.GetFileName(source);
            string dest = "";
            if (string.IsNullOrEmpty(relativePathToAssets))
            {
                dest = string.Format("{0}/{1}", Application.dataPath, fileName);
            }
            else
            {
                dest = string.Format("{0}/{1}/{2}", Application.dataPath, relativePathToAssets, fileName);
            }

            FileUtils.CopyFile(source, dest);
        }

        AssetDatabase.Refresh();
    }

    public static void DeleteAsset(string[] assets)
    {
        for (int i = 0; i < assets.Length; i++)
        {
            if (string.IsNullOrEmpty(assets[i]))
            {
                continue;
            }

            string file = string.Format("{0}/{1}", Application.dataPath, assets[i]);
            FileUtils.DeleteFile(file);
            FileUtils.DeleteFile(file + ".meta");
        }

        AssetDatabase.Refresh();
    }

    public static T AddComponent<T, T1>(this GameObject go, T1 other) where T : Component
    {
        return go.AddComponent<T>().GetCopyOf<T, T1>(other) as T;
    }

    public static T GetCopyOf<T, T1>(this Component comp, T1 other) where T : Component
    {
        System.Type sourceType = other.GetType();
        System.Type type = comp.GetType();

        if (type != sourceType && !type.IsSubclassOf(sourceType))
        {
            Debug.LogWarningFormat("Can't copy component, {0} isn't sub class of {1}.", type.Name, sourceType);
            return null;
        }

        var fields = type.GetFields();
        foreach (var field in fields)
        {
            if (field.IsStatic) continue;
            var fi = sourceType.GetField(field.Name);
            if (fi == null) continue;
            try
            {
                field.SetValue(comp, field.GetValue(other));
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (!prop.CanWrite || prop.Name == "name") continue;
            var pi = sourceType.GetProperty(prop.Name);
            if (pi == null || !pi.CanRead) continue;
            try
            {
                prop.SetValue(comp, pi.GetValue(other, null), null);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        return comp as T;
    }

    private static Material blitMaterial = null;

    public static bool HasAlpha(Texture2D tex2d)
    {
        try
        {
            if (tex2d == null)
                return false;

            switch (tex2d.format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.ARGB4444:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.DXT5:
                case TextureFormat.RGBA4444:
                case TextureFormat.BGRA32:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                {
                    return true;
                }
                case TextureFormat.DXT1:
                case TextureFormat.RGB24:
                case TextureFormat.RGB565:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.EAC_RG:
                case TextureFormat.EAC_RG_SIGNED:
                case TextureFormat.ETC2_RGB:
                    break;
            }

            return false;
        }
        catch (System.Exception ex)
        {
            LogHelper.Exception(ex);
            return false;
        }
    }

    public static Texture2D LoadTexture(string texPath, bool keepAlpha = true)
    {
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
        if (HasAlpha(tex))
        {
            if (!keepAlpha)
            {
                Texture2D n = CopyTexture(tex, false);
                Resources.UnloadAsset(tex);
                tex = n;
            }
        }

        return tex;
    }

    public static Texture2D CopyTexture(Texture2D tex, bool keepAlpha = true)
    {
        int width = tex.width;
        int height = tex.height;

        if (blitMaterial == null)
        {
            blitMaterial = new Material(Shader.Find("Editor/CopyTexture"));
        }

        RenderTexture rtt = new RenderTexture(width, height, 0);
        Graphics.SetRenderTarget(rtt);
        GL.LoadPixelMatrix(0, 1, 1, 0);
        GL.Clear(true, true, new Color(1, 1, 1, 1));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), tex, blitMaterial);

        TextureFormat format = TextureFormat.RGB24;
        if (HasAlpha(tex) && keepAlpha)
        {
            format = TextureFormat.RGBA32;
        }

        Texture2D result = new Texture2D(width, height, format, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        result.Apply();

        Graphics.SetRenderTarget(null);
        RenderTexture.DestroyImmediate(rtt);

        return result;
    }

    public static Texture2D CopyTexture(Texture2D tex, int destWidth, int destHeight, bool keepAlpha = true,
        bool forceAlpha = false)
    {
        int width = destWidth;
        int height = destHeight;

        if (blitMaterial == null)
        {
            blitMaterial = new Material(Shader.Find("Editor/CopyTexture"));
        }

        RenderTexture rtt = new RenderTexture(width, height, 0);
        Graphics.SetRenderTarget(rtt);
        GL.LoadPixelMatrix(0, 1, 1, 0);
        GL.Clear(true, true, new Color(1, 1, 1, 1));
        Graphics.DrawTexture(new Rect(0, 0, 1, 1), tex, blitMaterial);

        TextureFormat format = TextureFormat.RGB24;
        if ((HasAlpha(tex) && keepAlpha) || forceAlpha)
        {
            format = TextureFormat.RGBA32;
        }

        Texture2D result = new Texture2D(width, height, format, false);
        result.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
        result.Apply();

        Graphics.SetRenderTarget(null);
        RenderTexture.DestroyImmediate(rtt);

        return result;
    }


    static Texture2D s_ContrastTex;

    // Returns a usable texture that looks like a high-contrast checker board.
    static Texture2D contrastTexture
    {
        get
        {
            if (s_ContrastTex == null)
                s_ContrastTex = CreateCheckerTex(
                    new Color(0f, 0.0f, 0f, 0.5f),
                    new Color(1f, 1f, 1f, 0.5f));
            return s_ContrastTex;
        }
    }

    static Texture2D CreateCheckerTex(Color c0, Color c1)
    {
        Texture2D tex = new Texture2D(16, 16);
        tex.name = "[Generated] Checker Texture";
        tex.hideFlags = HideFlags.DontSave;

        for (int y = 0; y < 8; ++y)
        for (int x = 0; x < 8; ++x)
            tex.SetPixel(x, y, c1);
        for (int y = 8; y < 16; ++y)
        for (int x = 0; x < 8; ++x)
            tex.SetPixel(x, y, c0);
        for (int y = 0; y < 8; ++y)
        for (int x = 8; x < 16; ++x)
            tex.SetPixel(x, y, c0);
        for (int y = 8; y < 16; ++y)
        for (int x = 8; x < 16; ++x)
            tex.SetPixel(x, y, c1);

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return tex;
    }

    static Texture2D CreateGradientTex()
    {
        Texture2D tex = new Texture2D(1, 16);
        tex.name = "[Generated] Gradient Texture";
        tex.hideFlags = HideFlags.DontSave;

        Color c0 = new Color(1f, 1f, 1f, 0f);
        Color c1 = new Color(1f, 1f, 1f, 0.4f);

        for (int i = 0; i < 16; ++i)
        {
            float f = Mathf.Abs((i / 15f) * 2f - 1f);
            f *= f;
            tex.SetPixel(0, i, Color.Lerp(c0, c1, f));
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    // Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
    static void DrawTiledTexture(Rect rect, Texture tex)
    {
        float u = rect.width / tex.width;
        float v = rect.height / tex.height;

        Rect texCoords = new Rect(0, 0, u, v);
        TextureWrapMode originalMode = tex.wrapMode;
        tex.wrapMode = TextureWrapMode.Repeat;
        GUI.DrawTextureWithTexCoords(rect, tex, texCoords);
        tex.wrapMode = originalMode;
    }

    public static void DrawSprite(Texture tex, Rect drawArea, Vector4 padding, Rect outer, Rect inner, Rect uv,
        Color color, Material mat)
    {
        // Create the texture rectangle that is centered inside rect.
        Rect outerRect = drawArea;
        outerRect.width = Mathf.Abs(outer.width);
        outerRect.height = Mathf.Abs(outer.height);

        if (outerRect.width > 0f)
        {
            float f = drawArea.width / outerRect.width;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (drawArea.height > outerRect.height)
        {
            outerRect.y += (drawArea.height - outerRect.height) * 0.5f;
        }
        else if (outerRect.height > drawArea.height)
        {
            float f = drawArea.height / outerRect.height;
            outerRect.width *= f;
            outerRect.height *= f;
        }

        if (drawArea.width > outerRect.width)
            outerRect.x += (drawArea.width - outerRect.width) * 0.5f;

        // Draw the background
        EditorGUI.DrawTextureTransparent(outerRect, null, ScaleMode.ScaleToFit, outer.width / outer.height);

        // Draw the Image
        GUI.color = color;

        Rect paddedTexArea = new Rect(
            outerRect.x + outerRect.width * padding.x,
            outerRect.y + outerRect.height * padding.w,
            outerRect.width - (outerRect.width * (padding.z + padding.x)),
            outerRect.height - (outerRect.height * (padding.w + padding.y))
        );

        if (mat == null)
        {
            GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
            GUI.DrawTextureWithTexCoords(paddedTexArea, tex, uv, true);
            GL.sRGBWrite = false;
        }
        else
        {
            // NOTE: There is an issue in Unity that prevents it from clipping the drawn preview
            // using BeginGroup/EndGroup, and there is no way to specify a UV rect...
            EditorGUI.DrawPreviewTexture(paddedTexArea, tex, mat);
        }

        // Draw the border indicator lines
        GUI.BeginGroup(outerRect);
        {
            tex = contrastTexture;
            GUI.color = Color.white;

            if (inner.xMin != outer.xMin)
            {
                float x = (inner.xMin - outer.xMin) / outer.width * outerRect.width - 1;
                DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
            }

            if (inner.xMax != outer.xMax)
            {
                float x = (inner.xMax - outer.xMin) / outer.width * outerRect.width - 1;
                DrawTiledTexture(new Rect(x, 0f, 1f, outerRect.height), tex);
            }

            if (inner.yMin != outer.yMin)
            {
                // GUI.DrawTexture is top-left based rather than bottom-left
                float y = (inner.yMin - outer.yMin) / outer.height * outerRect.height - 1;
                DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
            }

            if (inner.yMax != outer.yMax)
            {
                float y = (inner.yMax - outer.yMin) / outer.height * outerRect.height - 1;
                DrawTiledTexture(new Rect(0f, outerRect.height - y, outerRect.width, 1f), tex);
            }
        }

        GUI.EndGroup();
    }

    public class EditorCoroutine
    {
        public static EditorCoroutine start(IEnumerator _routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.start();
            return coroutine;
        }

        readonly IEnumerator routine;

        EditorCoroutine(IEnumerator _routine)
        {
            routine = _routine;
        }

        void start()
        {
            EditorApplication.update += update;
        }

        public void stop()
        {
            EditorApplication.update -= update;
        }

        void update()
        {
            if (!routine.MoveNext())
            {
                stop();
            }
        }
    }
}



public class CreateFont : EditorWindow
{
    [MenuItem("Tools/创建数字字体")]
    public static void Open()
    {
        GetWindow<CreateFont>("创建数字标签");
    }
 
    private Texture2D tex;
    private string fontName;
    private string fontPath;
    private int advanceValue;
    private int asciiStart;
 
    private void OnGUI()
    {
        GUILayout.BeginVertical();
 
        GUILayout.BeginHorizontal();
        GUILayout.Label("Num Image：");
        tex = (Texture2D)EditorGUILayout.ObjectField(tex, typeof(Texture2D), true);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Advance：");
        advanceValue = int.Parse(EditorGUILayout.TextField(advanceValue.ToString()));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Ascii Start:");
        asciiStart = int.Parse(EditorGUILayout.TextField(asciiStart.ToString()));
        GUILayout.EndHorizontal();
 
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建"))
        {
            Create();
        }
        GUILayout.EndHorizontal();
 
        GUILayout.EndVertical();
    }
 
    private void Create()
    {
        if (tex == null)
        {
            Debug.LogError("创建失败，图片为空！");
            return;
        }
        
        fontPath = "/HotUpdateAssets/Fonts/";
        fontName = tex.name;

        string selectionPath = AssetDatabase.GetAssetPath(tex);
        Debug.Log("loadPath="+selectionPath);

        string selectionExt = Path.GetExtension(selectionPath);
        if (selectionExt.Length == 0)
        {
            Debug.LogError("创建失败！");
            return;
        }
        
        float lineSpace = 0.1f;
        string fontPathName = fontPath + fontName + ".fontsettings";
        string matPathName = fontPath + fontName + ".mat";

        // string loadPath = selectionPath.Replace(selectionExt, "").Substring(selectionPath.IndexOf("/Resources/") + "/Resources/".Length);
        // Sprite[] sprites = Resources.LoadAll<Sprite>(loadPath);

        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath(selectionPath);
        
        if (sprites.Length > 1)
        {
            Material mat = new Material(Shader.Find("GUI/Text Shader"));
            mat.SetTexture("_MainTex", tex);
            
            Font m_myFont = new Font();
            m_myFont.material = mat;

            CharacterInfo[] characterInfo = new CharacterInfo[sprites.Length];
            for (int i = 1; i < sprites.Length; i++)
            {
                Sprite sprite = sprites[i] as Sprite;
                if (sprite.rect.height > lineSpace)
                {
                    lineSpace = sprite.rect.height;
                }
            }
            for (int i = 0; i < sprites.Length-1; i++)
            {
                Sprite spr = sprites[i+1] as Sprite;
                CharacterInfo info = new CharacterInfo();
                info.index = i + asciiStart;

                Rect rect = spr.rect;
                float pivot = spr.pivot.y / rect.height - 0.5f;
                if (pivot > 0)
                {
                    pivot = -lineSpace / 2 - spr.pivot.y;
                }
                else if (pivot < 0)
                {
                    pivot = -lineSpace / 2 + rect.height - spr.pivot.y;
                }
                else
                {
                    pivot = -lineSpace / 2;
                }
                int offsetY = (int)(pivot + (lineSpace - rect.height) / 2);
                info.uvBottomLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y) / tex.height);
                info.uvBottomRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y) / tex.height);
                info.uvTopLeft = new Vector2((float)rect.x / tex.width, (float)(rect.y + rect.height) / tex.height);
                info.uvTopRight = new Vector2((float)(rect.x + rect.width) / tex.width, (float)(rect.y + rect.height) / tex.height);
                info.minX = 0;
                info.minY = -(int)rect.height - offsetY;
                info.maxX = (int)rect.width;
                info.maxY = -offsetY;
                info.advance = advanceValue;
                characterInfo[i] = info;
            }
            AssetDatabase.CreateAsset(mat, "Assets" + matPathName);
            AssetDatabase.CreateAsset(m_myFont, "Assets" + fontPathName);
            m_myFont.characterInfo = characterInfo;
            EditorUtility.SetDirty(m_myFont);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();//刷新资源
            Debug.Log("创建字体成功"); 
        }
        else
        {
            Debug.LogError("图集错误！");
        }
    }
}