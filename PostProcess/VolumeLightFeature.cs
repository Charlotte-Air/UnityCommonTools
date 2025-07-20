using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VolumeLightFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public int sampleCount = 8;
        public float density = 4.0f;
        public float lightIntensity = 2.0f;
        public float g = 0.22f;
        public float noiseDensity = 0.01f;
        public float fogFactor = 10.0f;
        public Vector2 fogDirection = new Vector2(1, 1);
        public float fogHeight = 2.0f;
        public float fogNoise = 0.6f;

        public bool dither = true;
        public bool volumeFog = false;

        public Material material;
        public RenderPassEvent passEvent;
    }
    public Settings settings = new Settings();
    class VolumeLightPass : ScriptableRenderPass
    {
        Settings settings;
        public RenderTargetIdentifier source;
        Matrix4x4 InvProjectionViewMatrix;

        int tempRTID = Shader.PropertyToID("_Temp");
        public VolumeLightPass(VolumeLightFeature feature)
        {
            settings = feature.settings;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Camera cam = renderingData.cameraData.camera;
            //VP矩阵的逆矩阵，用于利用深度以及屏幕坐标重建世界坐标
            InvProjectionViewMatrix = (GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix).inverse;

            UpdateMaterialParams(settings.material, settings);

            var cmd = CommandBufferPool.Get("体积光");
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tempRTID, desc);

            cmd.Blit(source, tempRTID, settings.material, 0);
            cmd.Blit(tempRTID, source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        void UpdateMaterialParams(Material material,Settings settings)
        {
            material.SetMatrix("_InverseViewProjectionMatrix", InvProjectionViewMatrix);
            material.SetInt("_SampleCount", settings.sampleCount);
            material.SetFloat("_Density", settings.density);
            material.SetFloat("_G", settings.g);
            material.SetVector("_MoveDir", settings.fogDirection);
            material.SetFloat("_LightIntensity", settings.lightIntensity);
            if (settings.volumeFog)
            {
                material.EnableKeyword("volume_fog");
                material.SetFloat("_NoiseIntensity", settings.noiseDensity);
                material.SetFloat("_FogHeight", settings.fogHeight);
                material.SetFloat("_StrongFog", settings.fogFactor);
                material.SetFloat("_FogNoise", settings.fogNoise);
            }
            else
                material.DisableKeyword("volume_fog");
            if (settings.dither)
                material.EnableKeyword("sample_dither");
            else
                material.DisableKeyword("sample_dither");

        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempRTID);
        }
    }
    VolumeLightPass lightPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        lightPass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(lightPass);
    }

    public override void Create()
    {
        lightPass = new VolumeLightPass(this);
        lightPass.renderPassEvent = settings.passEvent;
    }
}
