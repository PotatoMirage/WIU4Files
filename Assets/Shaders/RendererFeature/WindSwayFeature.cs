using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class WindSwayFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings settings = new Settings();
    private Material      _material;
    private WindSwayPass  _pass;

    public override void Create()
    {
        if (settings.shader == null) settings.shader = Shader.Find("Hidden/Custom/WindSway");
        if (settings.shader != null) _material = CoreUtils.CreateEngineMaterial(settings.shader);
        _pass = new WindSwayPass(_material) { renderPassEvent = settings.renderPassEvent };
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

    private sealed class WindSwayPass : ScriptableRenderPass
    {
        private readonly Material _mat;

        private class PassData
        {
            public TextureHandle source;
            public Material      material;
            public float         intensity;
            public float         swaySpeed;
            public float         swayStrength;
            public float         swayScale;
            public float         chromaticDrift;
            public float         flickerStrength;
            public float         flickerSpeed;
            public float         time;
        }

        public WindSwayPass(Material mat) { _mat = mat; }
        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null) return;

            var stack  = VolumeManager.instance.stack;
            var volume = stack.GetComponent<WindSwayVolume>();
            if (volume == null || !volume.IsActive()) return;

            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;

            TextureHandle activeColor = resources.activeColorTexture;
            if (!activeColor.IsValid()) return;

            var desc = renderGraph.GetTextureDesc(activeColor);
            desc.name = "Wind_Temp"; desc.clearBuffer = false;
            TextureHandle temp = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Wind Copy", out var pd))
            {
                pd.source = activeColor;
                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(temp, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1,1,0,0), 0, false));
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Wind Apply", out var pd))
            {
                pd.source          = temp;
                pd.material        = _mat;
                pd.intensity       = volume.intensity.value;
                pd.swaySpeed       = volume.swaySpeed.value;
                pd.swayStrength    = volume.swayStrength.value;
                pd.swayScale       = volume.swayScale.value;
                pd.chromaticDrift  = volume.chromaticDrift.value;
                pd.flickerStrength = volume.flickerStrength.value;
                pd.flickerSpeed    = volume.flickerSpeed.value;
                pd.time            = Time.time;

                builder.UseTexture(temp, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                {
                    d.material.SetFloat("_Intensity",       d.intensity);
                    d.material.SetFloat("_SwaySpeed",       d.swaySpeed);
                    d.material.SetFloat("_SwayStrength",    d.swayStrength);
                    d.material.SetFloat("_SwayScale",       d.swayScale);
                    d.material.SetFloat("_ChromaticDrift",  d.chromaticDrift);
                    d.material.SetFloat("_FlickerStrength", d.flickerStrength);
                    d.material.SetFloat("_FlickerSpeed",    d.flickerSpeed);
                    d.material.SetFloat("_WindTime",        d.time);
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1,1,0,0), d.material, 0);
                });
            }
        }
    }
}
