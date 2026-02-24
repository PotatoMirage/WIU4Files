Shader "Hidden/Custom/WindSway"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "WindSwayPass"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float _Intensity;
            float _SwaySpeed;
            float _SwayStrength;
            float _SwayScale;
            float _ChromaticDrift;
            float _FlickerStrength;
            float _FlickerSpeed;
            float _WindTime;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(i.vertexID);
                o.uv         = GetFullScreenTriangleTexCoord(i.vertexID);
                return o;
            }

            // ── Value noise ───────────────────────────────────────────
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }

            float valueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(
                    lerp(hash(i),             hash(i + float2(1,0)), u.x),
                    lerp(hash(i+float2(0,1)), hash(i + float2(1,1)), u.x),
                    u.y
                );
            }

            // ── Wind warp UV ──────────────────────────────────────────
            // Two layers of noise moving in slightly different directions
            // simulates the organic, non-uniform way wind moves through a canopy
            float2 WindWarp(float2 uv, float time)
            {
                float2 flow1 = float2(time * _SwaySpeed * 0.7, time * _SwaySpeed * 0.3);
                float2 flow2 = float2(time * _SwaySpeed * 0.4, time * _SwaySpeed * 0.6);

                float wx = valueNoise(uv * _SwayScale + flow1) * 2.0 - 1.0;
                float wy = valueNoise(uv * _SwayScale + flow2) * 2.0 - 1.0;

                // Second octave for more organic feel
                wx += (valueNoise(uv * _SwayScale * 2.1 + flow2) * 2.0 - 1.0) * 0.4;
                wy += (valueNoise(uv * _SwayScale * 2.1 + flow1) * 2.0 - 1.0) * 0.4;

                return float2(wx, wy) * _SwayStrength;
            }

            // ── Canopy light flicker ──────────────────────────────────
            // High frequency noise simulating light flickering through moving leaves
            float CanopyFlicker(float2 uv, float time)
            {
                float f1 = valueNoise(uv * 8.0  + time * _FlickerSpeed * 0.7);
                float f2 = valueNoise(uv * 15.0 + time * _FlickerSpeed * 1.3);
                float f3 = valueNoise(uv * 30.0 + time * _FlickerSpeed * 0.9);
                // Combine — gives irregular dappled light pattern
                return (f1 * 0.5 + f2 * 0.3 + f3 * 0.2);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv   = i.uv;
                float  time = _WindTime;

                // ── UV warp ──────────────────────────────────────────
                float2 warp    = WindWarp(uv, time) * _Intensity;
                float2 warpedUV = uv + warp;

                // ── Sample with chromatic drift ───────────────────────
                // R, G, B sampled at slightly offset UVs along wind direction
                // Subtle RGB split like light refracting through moving leaves
                float2 driftDir = normalize(warp + float2(0.001, 0.001));
                float  drift    = _ChromaticDrift * _Intensity;

                float r = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture,
                              warpedUV - driftDir * drift).r;
                float g = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture,
                              warpedUV).g;
                float b = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture,
                              warpedUV + driftDir * drift).b;

                float3 warped = float3(r, g, b);

                // ── Canopy flicker ────────────────────────────────────
                float flicker     = CanopyFlicker(uv, time);
                // Flicker is centered at 0 — brightens and darkens slightly
                float flickerMult = 1.0 + (flicker - 0.5) * _FlickerStrength * _Intensity * 2.0;
                float3 flickered  = warped * flickerMult;

                // ── Blend with original ───────────────────────────────
                float3 scene  = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).rgb;
                float3 final  = lerp(scene, flickered, _Intensity);
                return half4(final, 1.0);
            }
            ENDHLSL
        }
    }
}
