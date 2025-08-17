using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModifyTexture
{

   [MenuItem("Tools/设置Packingtag")]
   static void SetPackingtag()
   {
       List<string> allTexturePaths = new List<string>();
       string textureType = "*.jpg,*.png";
       string[] textureTypeArray = textureType.Split(',');

       for (int i = 0; i < textureTypeArray.Length; i++) {
           //string[] texturePath = Directory.GetFiles(Application.dataPath,"(*.jpg|*.bmp)");
           string[] texturePath = Directory.GetFiles(SKY.CommonDefine.ASSETBUNDLE_UITEXTURE_DIR, textureTypeArray[i], SearchOption.AllDirectories);
           for (int j = 0; j < texturePath.Length; j++)
           {
               allTexturePaths.Add(texturePath[j]);
           }
       }

       for (int k = 0; k < allTexturePaths.Count; k++)
       {
           TextureImporter textureImporter = TextureImporter.GetAtPath(allTexturePaths[k]) as TextureImporter;
           string atlasName = new DirectoryInfo(Path.GetDirectoryName(allTexturePaths[k])).Name;
           textureImporter.spritePackingTag = atlasName;
           textureImporter.spriteImportMode = SpriteImportMode.Multiple;
           textureImporter.mipmapEnabled = false;
           textureImporter.SaveAndReimport();
           AssetDatabase.ImportAsset(allTexturePaths[k]);
       }
   }

}