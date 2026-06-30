using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class GlitchRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public RenderPassEvent passEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings settings = new Settings();
    private Material _material;
    private GlitchPass _pass;

    public override void Create()
    {
        if (settings.shader == null) settings.shader = Shader.Find("Hidden/Custom/Glitch");
        if (settings.shader == null) return;

        _material = CoreUtils.CreateEngineMaterial(settings.shader);
        _pass = new GlitchPass(_material);
        _pass.renderPassEvent = settings.passEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_material == null) return;
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing)
    {
        _pass?.Dispose();
        CoreUtils.Destroy(_material);
    }

    private class GlitchPass : ScriptableRenderPass
    {
        private Material _material;
        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");
        private static readonly int ScanJitterID = Shader.PropertyToID("_ScanlineJitter");
        private static readonly int ColorDriftID = Shader.PropertyToID("_ColorDrift");
        private static readonly int TimeXID = Shader.PropertyToID("_TimeX");

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public GlitchVolume volume;
        }

        public GlitchPass(Material material)
        {
            _material = material;
        }

        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_material == null) return;

            var cameraData = frameData.Get<UniversalCameraData>();
            var resourceData = frameData.Get<UniversalResourceData>();

            if (cameraData.isPreviewCamera) return;

            var volume = VolumeManager.instance.stack.GetComponent<GlitchVolume>();
            if (volume == null || !volume.IsActive()) return;

            TextureHandle source = resourceData.activeColorTexture;
            if (!source.IsValid()) return;

            TextureDesc desc = renderGraph.GetTextureDesc(source);
            desc.name = "Glitch_Temp";
            desc.clearBuffer = false;
            TextureHandle tempTexture = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Glitch Apply", out var data))
            {
                data.source = source;
                data.material = _material;
                data.volume = volume;

                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData pd, RasterGraphContext ctx) =>
                {
                    pd.material.SetFloat(IntensityID, pd.volume.intensity.value);
                    pd.material.SetFloat(ScanJitterID, pd.volume.scanlineJitter.value);
                    pd.material.SetFloat(ColorDriftID, pd.volume.colorDrift.value);
                    pd.material.SetFloat(TimeXID, Time.time);

                    Blitter.BlitTexture(ctx.cmd, pd.source, new Vector4(1, 1, 0, 0), pd.material, 0);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Glitch Copy Back", out var data))
            {
                data.source = tempTexture;
                builder.UseTexture(tempTexture, AccessFlags.Read);
                builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData pd, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, pd.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }
        }
    }
}