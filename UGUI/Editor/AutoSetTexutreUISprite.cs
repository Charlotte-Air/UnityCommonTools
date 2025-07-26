using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class AutoSetTexutreUISprite : AssetPostprocessor
{
    void OnPreprocessTexture()  
    {
        if (assetPath.IndexOf("ui/uiimage") >= 0) 
        {
            //自动设置类型;  
            TextureImporter textureImporter = (TextureImporter)assetImporter;  
            textureImporter.textureType = TextureImporterType.Sprite;  
        }
    }  
}
