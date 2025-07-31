using UnityEditor;
using UnityEngine;
using UnityEditor.UI;

namespace UiParticles.Editor
{
    /// <summary>
    /// Custom editor for UI Particles component
    /// </summary>
    [CustomEditor(typeof(UiParticles))]
    public class UiParticlesEditor : GraphicEditor
    {

        private SerializedProperty m_RenderMode;
        private SerializedProperty m_StretchedSpeedScale;
        private SerializedProperty m_StretchedLenghScale;
        private SerializedProperty m_IgnoreTimescale;
        private SerializedProperty m_Mesh;
        static readonly GUIContent s_ContentmMesh = new GUIContent ("Mesh", "The mesh for rendering particles");
        static readonly GUILayoutOption GUILayoutButtonHeight = GUILayout.Height(38);
        protected override void OnEnable()
        {
            base.OnEnable();

            m_RenderMode = serializedObject.FindProperty("m_RenderMode");
            m_StretchedSpeedScale = serializedObject.FindProperty("m_StretchedSpeedScale");
            m_StretchedLenghScale = serializedObject.FindProperty("m_StretchedLenghScale");
            m_IgnoreTimescale = serializedObject.FindProperty("m_IgnoreTimescale");
            m_Mesh = serializedObject.FindProperty("m_Mesh");
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            UiParticles uiParticleSystem = (UiParticles) target;

            if (GUILayout.Button("Apply to nested particle systems"))
            {
                var nested = uiParticleSystem.gameObject.GetComponentsInChildren<ParticleSystem>();
                foreach (var particleSystem in nested)
                {
                    if (particleSystem.GetComponent<UiParticles>() == null)
                        particleSystem.gameObject.AddComponent<UiParticles>();
                }
            }

            EditorGUILayout.PropertyField(m_RenderMode);

            switch(uiParticleSystem.RenderMode)
            {
                case UiParticleRenderMode.StreachedBillboard:
                {
                    EditorGUILayout.PropertyField(m_StretchedSpeedScale);
                    EditorGUILayout.PropertyField(m_StretchedLenghScale);
                    break;
                }
                case UiParticleRenderMode.Mesh:
                {
                    EditorGUILayout.PropertyField(m_Mesh, s_ContentmMesh);
                    if (uiParticleSystem.mesh != null && !uiParticleSystem.mesh.isReadable)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.HelpBox ("该网格模型的 Read/Write Enbaled 开启才能读取数据并渲染", MessageType.Warning);
                        if (GUILayout.Button("开启", GUILayoutButtonHeight))
                        {
                            var assetPath = UnityEditor.AssetDatabase.GetAssetPath(uiParticleSystem.mesh.GetInstanceID());
                            Debug.Log("修改FBX文件的可读写属性：" + assetPath);
                            var importer = UnityEditor.ModelImporter.GetAtPath(assetPath) as UnityEditor.ModelImporter;
                            importer.isReadable = true;
                            importer.SaveAndReimport();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
                }
            }

            
            if (uiParticleSystem.ParticleSystem.customData.enabled)
            {
                if (uiParticleSystem.material != null)
                {
                    // if (!uiParticleSystem.material.HasProperty("_UseUICustomData"))
                    // {
                    //     EditorGUILayout.HelpBox ("该材质 shader 不支持 CustomData 在 UiParticles 上使用！", MessageType.Warning);
                    // }
                    // else if (uiParticleSystem.material.GetFloat("_UseUICustomData") == 0)
                    // {
                    //     EditorGUILayout.BeginHorizontal();
                    //     EditorGUILayout.HelpBox ("shader 上的 UseUICustomData 开启后才能使用 CustomData", MessageType.Warning);
                    //     if (GUILayout.Button("开启", GUILayoutButtonHeight))
                    //     {
                    //         Undo.RecordObject(uiParticleSystem.material, "Enabled UseUICustomData");
                    //         uiParticleSystem.material.SetFloat("_UseUICustomData", 1);
                    //     }
                    //     EditorGUILayout.EndHorizontal();
                    // }
                }
            }

            EditorGUILayout.PropertyField(m_IgnoreTimescale);
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
