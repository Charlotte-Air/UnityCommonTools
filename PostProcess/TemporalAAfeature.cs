using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class TemporalAAfeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent passEvent;

        public float blendAlpha;
    }
    public Settings settings = new Settings();
    class TemporalAAPass : ScriptableRenderPass
    {
        bool inited = false;
        Settings settings;
        public RenderTargetIdentifier source;

        RenderTexture historyBuffer;

        int FrameID = 0;
        int tempRT = Shader.PropertyToID("_Temp");

        Matrix4x4 CurrVP;
        Matrix4x4 LastVP;
        Matrix4x4 jitteredProjectionMatrix;

        Vector2 prevJitterOffset;
        Vector2 currJitterOffset;

        List<Vector2> offsets = new List<Vector2>();
        Halton halton = new Halton(new Vector2(2, 3));

        public TemporalAAPass(TemporalAAfeature feature)
        {
            settings = feature.settings;
        }

        private void SetUp(ScriptableRenderContext context,ref RenderingData renderingData)
        {
            Camera camera = renderingData.cameraData.camera;
            historyBuffer = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 0);
            historyBuffer.dimension = TextureDimension.Tex2D;
            historyBuffer.antiAliasing = 1;
            historyBuffer.format = UniversalRenderPipeline.asset.supportsHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            historyBuffer.filterMode = FilterMode.Bilinear;
            historyBuffer.memorylessMode = RenderTextureMemoryless.None;
            historyBuffer.Create();

            for(int i = 0; i < 16; i++)
            {
                offsets.Add(halton.GenerateHaltonSequence(i + 1));
            }

            inited = true;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (inited == false)
                SetUp(context, ref renderingData);
            Camera cam = renderingData.cameraData.camera;
            Material mat = settings.material;
            var cmd = CommandBufferPool.Get("TemporalAnti-Aliasing");

            if (FrameID == 0)
                prevJitterOffset = offsets[0];
            else
                prevJitterOffset = currJitterOffset;
            currJitterOffset = offsets[(FrameID + 1) % 16];

            jitteredProjectionMatrix = cam.projectionMatrix;
            jitteredProjectionMatrix.m02 += (currJitterOffset.x * 2 - 1) / cam.pixelWidth;
            jitteredProjectionMatrix.m12 += (currJitterOffset.y * 2 - 1) / cam.pixelHeight;

            cmd.SetViewProjectionMatrices(cam.worldToCameraMatrix, jitteredProjectionMatrix);

            CurrVP = GL.GetGPUProjectionMatrix(jitteredProjectionMatrix, false);
            CurrVP *= cam.worldToCameraMatrix;

            if (FrameID == 0)
                LastVP = jitteredProjectionMatrix * cam.worldToCameraMatrix;

            mat.SetMatrix("_InverseCurrVP", CurrVP.inverse);
            mat.SetMatrix("_LastVP", LastVP);
            mat.SetVector("_WorldCameraPos", cam.transform.position);
            mat.SetVector("_PrevJitterOffset", prevJitterOffset);
            mat.SetVector("_CurrJitterOffset", currJitterOffset);
            mat.SetTexture("_HistoryBuffer", historyBuffer);
            mat.SetFloat("_BlendAlpha", settings.blendAlpha);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(tempRT, desc);

            cmd.Blit(source, tempRT, mat, 0);
            cmd.Blit(tempRT, historyBuffer);
            cmd.Blit(tempRT, source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

            LastVP = jitteredProjectionMatrix * cam.worldToCameraMatrix;
            FrameID++;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempRT);
        }
    }
    TemporalAAPass temporalPass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        temporalPass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(temporalPass);
    }

    public override void Create()
    {
        temporalPass = new TemporalAAPass(this);
        temporalPass.renderPassEvent = settings.passEvent;
    }
}
