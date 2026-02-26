using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class AtmosphericFogFeature : ScriptableRendererFeature
{
    [Serializable]
    public class Settings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Shader shader;
    }

    [SerializeField] private Settings settings = new Settings();
    private Material _material;
    private FogPass _pass;

    public override void Create()
    {
        if (settings.shader == null) settings.shader = Shader.Find("Hidden/Custom/AtmosphericFog");
        if (settings.shader != null) _material = CoreUtils.CreateEngineMaterial(settings.shader);
        _pass = new FogPass(_material) { renderPassEvent = settings.renderPassEvent };
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

    private sealed class FogPass : ScriptableRenderPass
    {
        private readonly Material _mat;

        private class PassData
        {
            public TextureHandle source;
            public Material material;
            public float intensity;
            public float fogStart;
            public float fogEnd;
            public float fogDensity;
            public Color fogColor;
            public float horizonBias;
            public float useHeightFog;
            public float heightFogStart;
            public float heightFogEnd;
            public float heightFogDensity;
            public Vector3 playerWorldPos;
            public float playerShieldRadius;
        }

        public FogPass(Material mat) { _mat = mat; }
        public void Dispose() { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (_mat == null) return;

            var stack = VolumeManager.instance.stack;
            var volume = stack.GetComponent<AtmosphericFogVolume>();
            if (volume == null || !volume.IsActive()) return;

            var resources = frameData.Get<UniversalResourceData>();
            if (resources.isActiveTargetBackBuffer) return;

            TextureHandle activeColor = resources.activeColorTexture;
            if (!activeColor.IsValid()) return;

            Vector3 playerPos = Vector3.zero;
            var playerGO = GameObject.FindWithTag(volume.playerTag);
            if (playerGO != null)
            {
                playerPos = playerGO.transform.position;
                playerPos.y += 0.5f; // adjust to roughly match character height center
            }

            var desc = renderGraph.GetTextureDesc(activeColor);
            desc.name = "Fog_Temp"; desc.clearBuffer = false;
            TextureHandle temp = renderGraph.CreateTexture(desc);

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Fog Copy", out var pd))
            {
                pd.source = activeColor;
                builder.UseTexture(activeColor, AccessFlags.Read);
                builder.SetRenderAttachment(temp, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), 0, false));
            }

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Fog Apply", out var pd))
            {
                pd.source = temp;
                pd.material = _mat;
                pd.intensity = volume.intensity.value;
                pd.fogStart = volume.fogStart.value;
                pd.fogEnd = volume.fogEnd.value;
                pd.fogDensity = volume.fogDensity.value;
                pd.fogColor = volume.fogColor.value;
                pd.horizonBias = volume.horizonBias.value;
                pd.useHeightFog = volume.useHeightFog.value ? 1f : 0f;
                pd.heightFogStart = volume.heightFogStart.value;
                pd.heightFogEnd = volume.heightFogEnd.value;
                pd.heightFogDensity = volume.heightFogDensity.value;
                pd.playerWorldPos = playerPos;
                pd.playerShieldRadius = volume.playerShieldRadius.value;

                builder.UseTexture(temp, AccessFlags.Read);
                builder.SetRenderAttachment(activeColor, 0);
                builder.SetRenderFunc((PassData d, RasterGraphContext ctx) =>
                {
                    d.material.SetFloat("_Intensity", d.intensity);
                    d.material.SetFloat("_FogStart", d.fogStart);
                    d.material.SetFloat("_FogEnd", d.fogEnd);
                    d.material.SetFloat("_FogDensity", d.fogDensity);
                    d.material.SetColor("_FogColor", d.fogColor);
                    d.material.SetFloat("_HorizonBias", d.horizonBias);
                    d.material.SetFloat("_UseHeightFog", d.useHeightFog);
                    d.material.SetFloat("_HeightFogStart", d.heightFogStart);
                    d.material.SetFloat("_HeightFogEnd", d.heightFogEnd);
                    d.material.SetFloat("_HeightFogDensity", d.heightFogDensity);
                    d.material.SetVector("_PlayerWorldPos", d.playerWorldPos);
                    d.material.SetFloat("_PlayerShieldRadius", d.playerShieldRadius);
                    Blitter.BlitTexture(ctx.cmd, d.source, new Vector4(1, 1, 0, 0), d.material, 0);
                });
            }
        }
    }
}