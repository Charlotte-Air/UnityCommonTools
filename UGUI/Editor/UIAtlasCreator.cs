using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class UIAtlasCreator : EditorWindow
{
    private static string m_UIAtlasResourceFolder = "";
    public static string UIAtlasResourceFolder
    {
        get
        {
            if (string.IsNullOrEmpty(m_UIAtlasResourceFolder))
            {
                m_UIAtlasResourceFolder = Application.dataPath + "/../../resource/ui/atlas";
                m_UIAtlasResourceFolder = System.IO.Path.GetFullPath(m_UIAtlasResourceFolder);
            }
            return m_UIAtlasResourceFolder;
        }
    }

    public static string UIAtlasTexFolder
    {
        get
        {
            string prefix = "Assets/DataRes/UI";
            return prefix + "/Atlas";
        }
    }

    public static string UIAtlasFolder
    {
        get
        {
            string prefix = "Assets/Data/UI";
            return prefix + "/Atlas";
        }
    }


    [MenuItem("Tools/图集创建/创建UI图集(Editor)")]
    public static void Create()
    {
        string title = "UI Atlas Creator";
        UIAtlasCreator window = EditorWindow.GetWindow<UIAtlasCreator>(true, title, true);
        window.Show();
    }

    [MenuItem("Tools/图集创建/创建UI图集(所有)")]
    public static void RebuildUIAtlas()
    {
        RebuildAllUIAtlas(true);
    }

    private static void RebuildAllUIAtlas(bool showProgressbar)
    {
        string[] dirs = System.IO.Directory.GetDirectories(UIAtlasResourceFolder);
        for (int i = 0; i < dirs.Length; i++)
        {
            string atlasName = FileUtils.GetLastDir(dirs[i]);

            if (showProgressbar)
            {
                string title = string.Format("创建UI图集中 ({0}/{1})", i, dirs.Length);
                string prompt = string.Format("创建UI图集: {0}", atlasName);
                EditorUtility.DisplayProgressBar(title, prompt, (float)i / dirs.Length);
            }

            CreateUIAtlas(UIAtlasResourceFolder, atlasName);
        }

        if (showProgressbar)
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private Vector2 scrollPosition = Vector2.zero;

    private void OnAtlasGUI(string atlasName)
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField(atlasName);

        string atlasPath = string.Format("{0}/{1}Atlas.prefab", UIAtlasFolder, atlasName);
        GameObject atlasObj = (GameObject)AssetDatabase.LoadAssetAtPath(atlasPath, typeof(GameObject));
        EditorGUILayout.ObjectField(atlasObj, typeof(GameObject), false);

        string resPath = string.Format("{0}/{1}/{1}.png", UIAtlasTexFolder, atlasName);
        Texture2D atlasTex = (Texture2D)AssetDatabase.LoadAssetAtPath(resPath, typeof(Texture2D));
        EditorGUILayout.ObjectField(atlasTex, typeof(Texture2D), false);

        int m = EditorPrefs.GetInt(atlasName + "_margin", 0);
        int mv = EditorGUILayout.IntField(m);
        if (m != mv)
        {
            EditorPrefs.SetInt(atlasName + "_margin", mv);
        }

        int b = EditorPrefs.GetInt(atlasName + "_border", 2);
        int bv = EditorGUILayout.IntField(b);
        if (b != bv)
        {
            EditorPrefs.SetInt(atlasName + "_border", bv);
        }

        if (GUILayout.Button("重建"))
        {
            CreateUIAtlas(UIAtlasResourceFolder, atlasName);
        }

        EditorGUILayout.EndHorizontal();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("图集列表:");

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        string[] dirs = System.IO.Directory.GetDirectories(UIAtlasResourceFolder);
        for (int i = 0; i < dirs.Length; i++)
        {
            string atlasName = FileUtils.GetLastDir(dirs[i]);
            OnAtlasGUI(atlasName);
        }

        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("重建所有"))
        {
            RebuildAllUIAtlas(true);
        }
    }

    public static bool CreateAtlasTexture(string name, Texture2D sourceTex, TextAsset manifest)
    {
        string path = string.Format("{0}/{1}/{1}.png", UIAtlasTexFolder, name);

        try
        {
            Dictionary<string, SpriteMetaData> old = new Dictionary<string, SpriteMetaData>();
            if (System.IO.File.Exists(path))
            {
                TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
                SpriteMetaData[] sheets = ti.spritesheet;
                if (sheets != null && sheets.Length > 0)
                {
                    for (int i = 0; i < sheets.Length; i++)
                    {
                        SpriteMetaData sma = sheets[i];
                        old[sma.name] = sma;
                    }
                }
            }

            WriteAtlasTexture(path, sourceTex);

            TextureImporter texImporter = AssetImporter.GetAtPath(path) as TextureImporter;

            texImporter.textureType = TextureImporterType.Sprite;
            texImporter.spriteImportMode = SpriteImportMode.Multiple;
            texImporter.maxTextureSize = 2048;
            texImporter.isReadable = false;
            texImporter.mipmapEnabled = false;
            texImporter.filterMode = FilterMode.Bilinear;

            List<SpriteMetaData> spriteMetaData = BuildAtlasList(name, sourceTex, manifest, old);
            texImporter.spritesheet = spriteMetaData.ToArray();

            AssetDatabase.ImportAsset(path);

            Debug.LogFormat("Create atlas tex {0} successful.", path);

            old.Clear();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogFormat("Create atlas tex {0} failed.", path);
            return false;
        }
    }

    public static void CreateAtlas(string name)
    {
        string resPath = string.Format("{0}/{1}/{1}.png", UIAtlasTexFolder, name);
        string path = string.Format("{0}/{1}Atlas.prefab", UIAtlasFolder, name);

        GameObject go = new GameObject("temp");

        try
        {
            Texture2D atlasTex = (Texture2D)AssetDatabase.LoadAssetAtPath(resPath, typeof(Texture2D));
            if (atlasTex == null)
            {
                throw new System.Exception("load atlas " + resPath + " failed.");
            }

            Object[] obj = AssetDatabase.LoadAllAssetRepresentationsAtPath(resPath);
            List<Sprite> sprites = new List<Sprite>();
            for (int i = 0; i < obj.Length; i++)
            {
                Sprite sprite = obj[i] as Sprite;
                if (sprite != null)
                {
                    sprites.Add(sprite);
                }
            }

            TextureImporter importer1 = (TextureImporter)TextureImporter.GetAtPath(resPath);
		    importer1.alphaSource = TextureImporterAlphaSource.FromInput;
            var pts = importer1.GetPlatformTextureSettings(EditorUserBuildSettings.selectedBuildTargetGroup.ToString());
            TextureImporterFormat old = pts.format;
            pts.format = TextureImporterFormat.RGBA32;
            importer1.SetPlatformTextureSettings(pts);
            AssetDatabase.ImportAsset(resPath);

            //UGUIAtlasAlphaChanel.SplitAlpha(atlasTex);

            TextureImporter importer2 = (TextureImporter)TextureImporter.GetAtPath(resPath);
            importer2.alphaSource = TextureImporterAlphaSource.None;
            var pts2 = importer1.GetPlatformTextureSettings(EditorUserBuildSettings.selectedBuildTargetGroup.ToString());
            pts2.format = old;
            importer2.SetPlatformTextureSettings(pts2);
            AssetDatabase.ImportAsset(resPath);

			string materialPath = System.IO.Path.ChangeExtension(resPath, "mat");
            Material material = AssetDatabase.LoadAssetAtPath(materialPath, typeof(Material)) as Material;
            material.SetTexture("_MainTex", null);

            SpriteAtlas atlas = go.AddComponent<SpriteAtlas>();
            atlas.SetData(sprites);
            atlas.alphaChanelMaterial = material;

            Object prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            if (prefab == null)
            {
                FileUtils.CreateDirectory(UIAtlasFolder);
                prefab = PrefabUtility.CreateEmptyPrefab(path);
            }
            PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ReplaceNameBased);

            Debug.LogFormat("Create atlas {0} successful.", path);
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogFormat("Create {0} failed.", path);
        }

        GameObject.DestroyImmediate(go);
    }

    private static void WriteAtlasTexture(string path, Texture2D sourceTex)
    {
        string assetPath = AssetDatabase.GetAssetPath(sourceTex);
        UnityEditorUtils.SetTextureAssetFormat(assetPath, TextureImporterCompression.Uncompressed, true, false);
        string fullPath = System.IO.Path.GetFullPath(path);
        fullPath = FileUtils.RemovePathFileName(fullPath);
        FileUtils.CreateDirectory(fullPath);
        System.IO.File.WriteAllBytes(path, sourceTex.EncodeToPNG());
        UnityEditorUtils.ReimportAsset(path);
    }

    private static List<SpriteMetaData> BuildAtlasList(string name, Texture2D sourceTex, TextAsset manifest, Dictionary<string, SpriteMetaData> old)
    {
        List<SpriteMetaData> descList = new List<SpriteMetaData>();
        descList.Clear();

        ByteReader reader = new ByteReader(manifest);
        char[] separator = new char[] { '|' };
        int lineCount = 0;

        while (reader.canRead)
        {
            string line = reader.ReadLine();
            lineCount++;

            if (line == null) break;
            if (line.StartsWith("//")) continue;

            string[] split = line.Split(separator);

            //attack1|000000|128|128|23|59|77|121|0.3085938|0.07568359|0.02685547|0.03076172
            if (split.Length == 12)
            {
                SpriteMetaData desc = new SpriteMetaData();
                string g = split[0].Trim();
                if (string.IsNullOrEmpty(g))
                {
                    desc.name = string.Format("{0}_{1}", name, split[1].Trim());
                }
                else
                {
                    desc.name = string.Format("{0}_{1}_{2}", name, g, split[1].Trim());
                }
                desc.pivot = new Vector2(0, 0);
                desc.rect = new Rect();
                desc.rect.x = float.Parse(split[8].Trim()) * sourceTex.width;
                desc.rect.y = float.Parse(split[9].Trim()) * sourceTex.height;
                desc.rect.width = float.Parse(split[10].Trim()) * sourceTex.width;
                desc.rect.height = float.Parse(split[11].Trim()) * sourceTex.height;
                desc.rect.y = sourceTex.height - desc.rect.height - desc.rect.y;
                if (desc.rect.y < 0) desc.rect.y = 0;

                if (old.ContainsKey(desc.name))
                {
                    SpriteMetaData smd = old[desc.name];
                    desc.border = smd.border;
                    desc.alignment = smd.alignment;
                    desc.pivot = smd.pivot;
                }

                descList.Add(desc);
            }
            else
            {
                Debug.LogWarningFormat("Read line {0} failed.", lineCount);
            }
        }

        return descList;
    }

    private static string s_TexturePackerPath
    {
        get
        {
            return string.Format("{0}/../../tools/bin/TexturePacker.exe", Application.dataPath);
        }
    }

    public static void CreateUIAtlas(string sourcePath, string atlasName)
    {
        Debug.LogFormat("------------------------------------");

        
        FileUtils.DeleteFiles(sourcePath, "log");
        FileUtils.DeleteFiles(sourcePath, "png");
        FileUtils.DeleteFiles(sourcePath, "txt");

        Debug.LogFormat("create {0} under {1}", atlasName, sourcePath);

        string fullSourcePath = FileUtils.RemoveLastPathSep(System.IO.Path.GetFullPath(sourcePath));

        string cmd = string.Format("{0} {1}\\{2} -c", s_TexturePackerPath, fullSourcePath, atlasName);

        int m = EditorPrefs.GetInt(atlasName + "_margin", 0);
        if (m != 0)
        {
            cmd += string.Format(" -m {0}", m);
        }
        int b = EditorPrefs.GetInt(atlasName + "_border", 2);
        if (b != 2)
        {
            cmd += string.Format(" -b {0}", b);
        }

        Debug.LogFormat("  texture packing ...\n  {0}", cmd);

        if (ShellHelper.ProcessCommand(cmd, fullSourcePath))
        {
            Debug.Log("  texture pack finished.");

            string atlasFileName = atlasName + "_atlas.png";
            string manifestFileName = atlasName + "_manifest.txt";

            string[] atlasAndManifest = new string[3];
            atlasAndManifest[0] = sourcePath + "\\" + atlasFileName;
            atlasAndManifest[1] = sourcePath + "\\" + manifestFileName;

            UnityEditorUtils.CopyAssetTo(atlasAndManifest, "");

            Texture2D tex = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/" + atlasFileName, typeof(Texture2D));
            TextAsset manifest = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/" + manifestFileName, typeof(TextAsset));


            if (CreateAtlasTexture(atlasName, tex, manifest))
            {
                CreateAtlas(atlasName);
            }

            string[] atlasAndManifestDel = new string[3];
            atlasAndManifestDel[0] = atlasFileName;
            atlasAndManifestDel[1] = manifestFileName;

            UnityEditorUtils.DeleteAsset(atlasAndManifestDel);

            FileUtils.DeleteFiles(sourcePath, "log");
            FileUtils.DeleteFiles(sourcePath, "png");
            FileUtils.DeleteFiles(sourcePath, "txt");

            AssetDatabase.Refresh();
        }
        else
        {
            Debug.LogError("  texture pack failed.");
        }
    }
}
