using System;
using UnityEngine;
using Script.renderfeatures;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class FogMaskSettings
{
    public Material maskMat;
    public Material BlurMat;
    public RenderPassEvent passEvent;
    [Range(1, 5)] public int num;
}

public class FogMaskTexFeature : ScriptableRendererFeature
{
    public FogMaskSettings settings = new FogMaskSettings();
    public FogMaskPass fogMaskPass;

    public override void Create()
    {
        fogMaskPass = new FogMaskPass(this) {renderPassEvent = settings.passEvent};
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(fogMaskPass);
    }
}

public class FogMaskPass : ScriptableRenderPass
{
    private FogMaskSettings fogMaskSettings;
    public static RenderTexture maskRt;
    public FogPlane _fogPlane;
    Matrix4x4 projection;
    private RenderTargetHandle tempTexture0;
    public FogMaskPass(FogMaskTexFeature feature)
    {
        fogMaskSettings = feature.settings;
        _fogPlane = GameObject.FindObjectOfType<FogPlane>();
        tempTexture0.Init("_TempRTexture0");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!renderingData.cameraData.camera.CompareTag("MaskCamera"))
            return;
        if (_fogPlane == null)
            return;
        if (_fogPlane.status != FogPlane.MaskStatus.start)
            return;

        var cmd = CommandBufferPool.Get("FogMask");
        if (!maskRt)
        {
            maskRt = new RenderTexture(512, 512, 24, RenderTextureFormat.RFloat);
        }
        cmd.SetRenderTarget(maskRt);

        cmd.ClearRenderTarget(true, true, Color.black);
        
        foreach (var pair in _fogPlane.maskMessage)
        {
            cmd.DrawMeshInstanced(_fogPlane._mesh, 0, fogMaskSettings.maskMat, 0, pair.ToArray(),
                pair.Count);
        }
   
        cmd.GetTemporaryRT(tempTexture0.id, 512, 512, 0);

        Blit(cmd, maskRt, tempTexture0.Identifier());

        
        for (int i = 0; i < fogMaskSettings.num; i++)
        {
            cmd.Blit(maskRt,tempTexture0.Identifier(),fogMaskSettings.BlurMat,0);
            cmd.Blit(tempTexture0.Identifier(), maskRt, fogMaskSettings.BlurMat, 1);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
        _fogPlane.status = FogPlane.MaskStatus.done;
    }
}