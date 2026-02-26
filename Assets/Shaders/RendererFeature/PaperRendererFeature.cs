using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class PaperRenderFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings settings = new Settings();
    private Material _material;
    private PaperPass _pass;

    public override void Create()
    {
        if (settings.shader == null) settings.shader = Shader.Find("Hidden/Custom/WhimsicalPaper");
        if (settings.shader != null) _material = CoreUtils.CreateEngineMaterial(settings.shader);

        _pass = new PaperPass(_material)
        {
            renderPassEvent = settings.renderPassEvent
        };
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

    private sealed class PaperPass : ScriptableRenderPass
    {
        private readonly Material _mat;

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float intensity;
            public Color paperColor;
            public float grainAmount;
            public float inkBleed;
            public float sketchStrength;
            public float vignette;
            public float time;
        }

        public PaperPass(Material mat) { _mat = mat; }
        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null) return;

            var stack = VolumeManager.instance.stack;
            var volume = stack.GetComponent<PaperVolume>();
            if (volume == null || !volume.IsActive()) return;

            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;

            TextureHandle activeColor = resources.activeColorTexture;
            if (!activeColor.IsValid()) return;

            var desc = renderGraph.GetTextureDesc(activeColor);
            desc.name = "Paper_Temp";
            desc.clearBuffer = false;
            TextureHandle tempTexture = renderGraph.CreateTexture(desc);

            // 1. Copy
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Paper Copy", out var passData))
            {
                passData.source = activeColor;
                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(tempTexture, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), 0, false);
                });
            }

            // 2. Apply
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Paper Apply", out var passData))
            {
                passData.source = tempTexture;
                passData.material = _mat;
                passData.intensity = volume.intensity.value;
                passData.paperColor = volume.paperColor.value;
                passData.grainAmount = volume.grainAmount.value;
                passData.inkBleed = volume.inkBleed.value;
                passData.sketchStrength = volume.sketchStrength.value;
                passData.vignette = volume.vignette.value;
                passData.time = Time.time;

                builder.UseTexture(tempTexture, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    data.material.SetFloat("_Intensity", data.intensity);
                    data.material.SetColor("_PaperColor", data.paperColor);
                    data.material.SetFloat("_GrainAmount", data.grainAmount);
                    data.material.SetFloat("_InkBleed", data.inkBleed);
                    data.material.SetFloat("_SketchStrength", data.sketchStrength);
                    data.material.SetFloat("_Vignette", data.vignette);
                    Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }
        }
    }
}