using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Rendering.Universal;

namespace UnityEditor.Rendering.Universal.ShaderGUI
{
    internal class URPPlayerShader : BaseShaderGUI
    {
        // Properties
        private LitGUI.LitProperties litProperties;
        protected MaterialProperty maskColorProp { get; set; }
        public MaterialProperty outlineColor;
        public MaterialProperty outlineColorWidth;
        public MaterialProperty UseoutlineColor;
        public MaterialProperty BombMap;
        public MaterialProperty UseBombMap;
        public MaterialProperty RimPower;
        public MaterialProperty DissolveNoise;
        public MaterialProperty ShaderDissolve;
        // collect properties from the material properties
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            maskColorProp = FindProperty("_MaskColor", properties, false);
            outlineColor = FindProperty("_OutlineColor", properties, false);
            outlineColorWidth = FindProperty("_OutlineColorWidth", properties, false);
            UseoutlineColor = FindProperty("_UseOutlineColor", properties, false);
            BombMap = FindProperty("_BombMap", properties, false);
            UseBombMap = FindProperty("_UseBombMap", properties, false);
            RimPower = FindProperty("_RimPower", properties, false);
            DissolveNoise = FindProperty("_DissolveNoise", properties, false);
            ShaderDissolve = FindProperty("_ShaderDissolve", properties, false);
    
            litProperties = new LitGUI.LitProperties(properties);
            
        }
    
        // material changed check
        public override void MaterialChanged(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
    
            SetMaterialKeywords(material, LitGUI.SetMaterialKeywords);
        }
    
        // material main surface options
        public override void DrawSurfaceOptions(Material material)
        {
            if (material == null)
                throw new ArgumentNullException("material");
    
            // Use default labelWidth
            EditorGUIUtility.labelWidth = 0f;
    
            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            if (litProperties.workflowMode != null)
            {
                DoPopup(LitGUI.Styles.workflowModeText, litProperties.workflowMode, Enum.GetNames(typeof(LitGUI.WorkflowMode)));
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendModeProp.targets)
                    MaterialChanged((Material)obj);
            }
            base.DrawSurfaceOptions(material);
        }
    
        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            BaseProperties(material);
            LitGUI.Inputs(litProperties, materialEditor, material);
            DrawEmissionProperties(material, true);
            DrawTileOffset(materialEditor, baseMapProp);
        }
        public void BaseProperties(Material material)
        {
            if (baseMapProp != null && baseColorProp != null) // Draw the baseMap, most shader will have at least a baseMap
            {
                materialEditor.TexturePropertySingleLine(Styles.baseMap, baseMapProp, baseColorProp, maskColorProp);
                // TODO Temporary fix for lightmapping, to be replaced with attribute tag.
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", baseMapProp.textureValue);
                    var baseMapTiling = baseMapProp.textureScaleAndOffset;
                    material.SetTextureScale("_MainTex", new Vector2(baseMapTiling.x, baseMapTiling.y));
                    material.SetTextureOffset("_MainTex", new Vector2(baseMapTiling.z, baseMapTiling.w));
                }
            }
        }
        // material main advanced options
        public override void DrawAdvancedOptions(Material material)
        {
            if (litProperties.reflections != null && litProperties.highlights != null)
            {
                EditorGUI.BeginChangeCheck();
                materialEditor.ShaderProperty(litProperties.highlights, LitGUI.Styles.highlightsText);
                materialEditor.ShaderProperty(litProperties.reflections, LitGUI.Styles.reflectionsText);
                if (EditorGUI.EndChangeCheck())
                {
                    MaterialChanged(material);
                }
            }
            EditorGUI.BeginChangeCheck();
            materialEditor.ShaderProperty(outlineColor, "OutlineColor");
            materialEditor.ShaderProperty(outlineColorWidth, "width");
            materialEditor.ShaderProperty(UseoutlineColor, "UseoutlineColor");
            materialEditor.ShaderProperty(UseBombMap, "UseBombMap");
            materialEditor.ShaderProperty(RimPower, "RimPower");
            materialEditor.TexturePropertySingleLine(new GUIContent("DissolveNoise"), DissolveNoise);
            materialEditor.ShaderProperty(ShaderDissolve, "ShaderDissolve");
            
            materialEditor.TexturePropertySingleLine(new GUIContent("BombMap"), BombMap);
           
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material);
            }
            base.DrawAdvancedOptions(material);
        }
    
        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");
    
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }
    
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
    
            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }
    
            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Surface", (float)surfaceType);
            material.SetFloat("_Blend", (float)blendMode);
    
            if (oldShader.name.Equals("Standard (Specular setup)"))
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Specular);
                Texture texture = material.GetTexture("_SpecGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
            else
            {
                material.SetFloat("_WorkflowMode", (float)LitGUI.WorkflowMode.Metallic);
                Texture texture = material.GetTexture("_MetallicGlossMap");
                if (texture != null)
                    material.SetTexture("_MetallicSpecGlossMap", texture);
            }
    
            MaterialChanged(material);
        }
    }
}
