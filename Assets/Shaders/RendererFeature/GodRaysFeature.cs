using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class GodRaysFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings settings = new Settings();
    private Material   _material;
    private GodRayPass _pass;

    public override void Create()
    {
        if (settings.shader == null) settings.shader = Shader.Find("Hidden/Custom/GodRays");
        if (settings.shader != null) _material = CoreUtils.CreateEngineMaterial(settings.shader);
        _pass = new GodRayPass(_material) { renderPassEvent = settings.renderPassEvent };
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

    private sealed class GodRayPass : ScriptableRenderPass
    {
        private readonly Material _mat;

        private class PassData
        {
            public TextureHandle source;
            public Material      material;
            public float         intensity;
            public Vector2       lightPosition;
            public Color         rayColor;
            public float         rayLength;
            public int           samples;
            public float         density;
            public float         decay;
            public float         weight;
            public float         exposure;
            public float         brightnessThreshold;
        }

        public GodRayPass(Material mat) { _mat = mat; }
        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null) return;

            var stack  = VolumeManager.instance.stack;
            var volume = stack.GetComponent<GodRaysVolume>();
            if (volume == null || !volume.IsActive()) return;

            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;

            TextureHandle activeColor = resources.activeColorTexture;
            if (!activeColor.IsValid()) return;

            var desc = renderGraph.GetTextureDesc(activeColor);
            desc.name = "GodRay_Temp"; desc.clearBuffer = false;
            TextureHandle temp = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("GodRay Copy", out var pd))
            {
                pd.source = activeColor;
                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(temp, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1,1,0,0), 0, false));
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("GodRay Apply", out var pd))
            {
                pd.source              = temp;
                pd.material            = _mat;
                pd.intensity = GodRaysVolume.RuntimeIntensity;
                pd.lightPosition = GodRaysVolume.RuntimeLightPosition;
                pd.rayColor            = volume.rayColor.value;
                pd.rayLength           = volume.rayLength.value;
                pd.samples             = volume.samples.value;
                pd.density             = volume.density.value;
                pd.decay               = volume.decay.value;
                pd.weight              = volume.weight.value;
                pd.exposure            = volume.exposure.value;
                pd.brightnessThreshold = volume.brightnessThreshold.value;

                builder.UseTexture(temp, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                {
                    d.material.SetFloat("_Intensity",           d.intensity);
                    d.material.SetVector("_LightPosition",      new Vector4(d.lightPosition.x, d.lightPosition.y, 0, 0));
                    d.material.SetColor("_RayColor",            d.rayColor);
                    d.material.SetFloat("_RayLength",           d.rayLength);
                    d.material.SetInt("_Samples",               d.samples);
                    d.material.SetFloat("_Density",             d.density);
                    d.material.SetFloat("_Decay",               d.decay);
                    d.material.SetFloat("_Weight",              d.weight);
                    d.material.SetFloat("_Exposure",            d.exposure);
                    d.material.SetFloat("_BrightnessThreshold", d.brightnessThreshold);
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1,1,0,0), d.material, 0);
                });
            }
        }
    }
}
