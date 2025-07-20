using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;

public class BlurRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurRenderSettings
    {
        public string passTag = "BlurRender";
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingTransparents;
        public string uiCameraName = "UICamera";
        [Range(0.0f, 5.0f)] public float BlurRadius = 1.8f;
        [Range(1, 10)] public int Iteration = 6;
        [Range(1, 8)] public float RTDownScaling = 2f;
        public Material blurMat = null;
    }
    public BlurRenderSettings settings = new BlurRenderSettings();
    private RenderTargetHandle dest;
    BlurRenderPass m_ScriptablePass;
    private Camera UICamera = null;

    public override void Create()
    {
        m_ScriptablePass = new BlurRenderPass(settings);
        m_ScriptablePass.renderPassEvent = settings.Event;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (UICamera == null)
        {
            if (renderingData.cameraData.camera.name == settings.uiCameraName)
            {
                UICamera = renderingData.cameraData.camera;
            }
        }

        if (UICamera != null && renderingData.cameraData.camera == UICamera)
        {
            var src = renderer.cameraColorTarget;
            dest = RenderTargetHandle.CameraTarget;
            m_ScriptablePass.Setup(src, this.dest);
            renderer.EnqueuePass(m_ScriptablePass);
        }
    }
}


public class BlurRenderPass : ScriptableRenderPass
{
    public BlurRenderFeature.BlurRenderSettings settings;
    private CommandBuffer cmd;
    private string cmdname;
    private RenderTargetHandle dest;
    private Material m_blurMat;
    private RenderTargetIdentifier source { get; set; }

    private float blurRadius;
    private int iteration;
    private float rtDownSampling;
    RenderTargetHandle m_temporaryColorTexture;
    RenderTargetHandle blurredID;
    RenderTargetHandle blurredID2;
    private bool needSwitch = true;

    public BlurRenderPass(BlurRenderFeature.BlurRenderSettings param)
    {
        renderPassEvent = param.Event;
        m_blurMat = param.blurMat;
        cmdname = param.passTag;
        blurRadius = param.BlurRadius;
        iteration = param.Iteration;
        rtDownSampling = param.RTDownScaling;

        blurredID.Init("blurredID");
        blurredID2.Init("blurredID2");
    }

    public void Setup(RenderTargetIdentifier src, RenderTargetHandle _dest)
    {
        this.source = src;
        this.dest = _dest;
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (Application.isPlaying)
        {
            cmd = CommandBufferPool.Get(cmdname);

            // RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;

            // opaqueDesc.depthBufferBits = 0;

            // cmd.GetTemporaryRT(blurredID.id, opaqueDesc, FilterMode.Bilinear);
            // cmd.GetTemporaryRT(blurredID2.id, opaqueDesc, FilterMode.Bilinear);
            // cmd.Blit(source, blurredID.Identifier());

            int RTWidth = (int)(Screen.width / rtDownSampling);
            int RTHeight = (int)(Screen.height / rtDownSampling);

            cmd.GetTemporaryRT(blurredID.id, RTWidth, RTHeight, 0, FilterMode.Bilinear);
            cmd.GetTemporaryRT(blurredID2.id, RTWidth, RTHeight, 0, FilterMode.Bilinear);
            cmd.Blit(source, blurredID.Identifier());

            for (int i = 0; i < iteration; i++)
            {
                cmd.SetGlobalFloat("offset", i / rtDownSampling + blurRadius);
                cmd.Blit(needSwitch ? blurredID.Identifier() : blurredID2.Identifier(), needSwitch ? blurredID2.Identifier() : blurredID.Identifier(), m_blurMat);
                needSwitch = !needSwitch;
            }

            cmd.SetGlobalFloat("offset", iteration / rtDownSampling + blurRadius);
            cmd.Blit(needSwitch ? blurredID.Identifier() : blurredID2.Identifier(), source, m_blurMat);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
    public override void FrameCleanup(CommandBuffer cmd)
    {
        if (dest == RenderTargetHandle.CameraTarget)
        {
            cmd.ReleaseTemporaryRT(m_temporaryColorTexture.id);
            cmd.ReleaseTemporaryRT(blurredID.id);
            cmd.ReleaseTemporaryRT(blurredID2.id);
        }
    }
}
