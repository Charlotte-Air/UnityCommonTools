using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(SpriteAtlas))]
public class UGUIAtlasInspector : Editor
{
    static public UGUIAtlasInspector instance;

    SpriteAtlas mAtlas;

	public override void OnInspectorGUI ()
	{
        mAtlas = target as SpriteAtlas;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel ("Texture2D");
        EditorGUILayout.ObjectField(mAtlas.texture, typeof(Texture2D), false);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("LoadSprite"))
        {
            if (mAtlas.texture != null)
            {
				if (mAtlas.alphaChanelMaterial == null) 
				{
					Debug.LogError("A SpriteAtlas Must Have a AlphaChanelMaterial!");
					EditorGUILayout.SelectableLabel("A SpriteAtlas Must Have a AlphaChanelMaterial!");
					return;
				}
                string path = AssetDatabase.GetAssetPath(mAtlas.texture);
                LoadSpriteData(mAtlas, path);
            }
        }
        DrawDefaultInspector ();
	}

    void LoadSpriteData (SpriteAtlas atlas, string path)
    {
        TextureImporter importer = TextureImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.spritesheet == null || importer.spritesheet.Length < 1)
        {
            Debug.Log("error:" + importer.spritesheet);
            EditorGUILayout.SelectableLabel("Selected Texture2D is not sprite!");
        }
        else
        {
            var frames = new List<Sprite>();
            Sprite[] all = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Select(x => x as Sprite).Where(x => x != null).ToArray();
            foreach(var data in importer.spritesheet)
            {
                foreach(var s in all) 
                {
                    if(s.name == data.name && s.texture == atlas.texture) 
                    {
                        frames.Add(s);
                        break;
                    }
                }
            }
            atlas.SetData(frames);
        }
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssets();
    }

}
