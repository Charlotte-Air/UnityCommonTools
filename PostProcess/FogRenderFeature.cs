using Script.renderfeatures;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class FogRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public RenderPassEvent passEvent;
        public Matrix4x4 _Projection;
        public Matrix4x4 _HeightViewMatrix;

    }

    public Settings settings = new Settings();


    public FogPass fogPass;

    public override void Create()
    {
        fogPass = new FogPass(this) {renderPassEvent = settings.passEvent};
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        fogPass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(fogPass);
    }

    public class FogPass : ScriptableRenderPass
    {
        Settings settings;
        Matrix4x4 projection;
        Camera camera;
        private Transform camTrans;
        Matrix4x4 frustumCornors = Matrix4x4.identity;

        public RenderTargetIdentifier source;
        int fogID2 = Shader.PropertyToID("_Temp");
        // int tempRTID = Shader.PropertyToID("_RainTemp");

        public FogPass(FogRenderFeature feature)
        {
            settings = feature.settings;

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            camera = renderingData.cameraData.camera;
            if (!camera.CompareTag("MainCamera"))
                return;
            var cmd = CommandBufferPool.Get("Fog");
            camTrans = camera.transform;
            frustumCornors = Matrix4x4.identity;
            float fov = camera.fieldOfView;
            float near = camera.nearClipPlane;
            float far = camera.farClipPlane;
            float aspect = camera.aspect;

            float fovWHalf = fov * 0.5f;

            Vector3 toRight = camTrans.right * near * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * aspect;
            Vector3 toTop = camTrans.up * near * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            var forward = camTrans.forward;
            Vector3 topLeft = (forward * near - toRight + toTop);
            float camScale = topLeft.magnitude * far / near;

            topLeft.Normalize();
            topLeft *= camScale;

            Vector3 topRight = (forward * near + toRight + toTop);
            topRight.Normalize();
            topRight *= camScale;

            Vector3 bottomRight = (forward * near + toRight - toTop);
            bottomRight.Normalize();
            bottomRight *= camScale;

            Vector3 bottomLeft = (forward * near - toRight - toTop);
            bottomLeft.Normalize();
            bottomLeft *= camScale;
   
            frustumCornors.SetRow(0, bottomLeft);
            frustumCornors.SetRow(1, bottomRight);
            frustumCornors.SetRow(2, topRight);
            frustumCornors.SetRow(3, topLeft);
   
            settings.material.SetMatrix("_Ray", frustumCornors);
            settings.material.SetTexture("_MaskTex", FogMaskPass.maskRt);
            settings.material.SetVector("_CameraPos", camera.transform.position);
            settings.material.SetMatrix("_HeightViewMatrix",settings._HeightViewMatrix);
            settings.material.SetMatrix("_Projection",settings._Projection);

            var desc = renderingData.cameraData.cameraTargetDescriptor;
            cmd.GetTemporaryRT(fogID2, desc);
            cmd.Blit(source, fogID2);
            cmd.Blit(fogID2, source, settings.material);
            

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(fogID2);
        }
    }
}