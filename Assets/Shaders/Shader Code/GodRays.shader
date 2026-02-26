Shader "Hidden/Custom/GodRays"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "GodRaysPass"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float  _Intensity;
            float4 _LightPosition;   // xy = screen UV of light source
            float4 _RayColor;
            float  _RayLength;
            int    _Samples;
            float  _Density;
            float  _Decay;
            float  _Weight;
            float  _Exposure;
            float  _BrightnessThreshold;

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

            // Extract bright spots — only sky gaps and highlights cast rays
            float3 BrightPass(float2 uv)
            {
                float3 col = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv).rgb;
                float  lum = dot(col, float3(0.299, 0.587, 0.114));
                // Hard threshold — only pixels brighter than threshold contribute
                float  mask = smoothstep(_BrightnessThreshold - 0.05,
                                         _BrightnessThreshold + 0.05, lum);
                return col * mask;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv      = i.uv;
                half4  scene   = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                // ── Radial light scattering ───────────────────────────
                // March from pixel toward light source, accumulating bright samples.
                // Each step decays so rays fade naturally with distance.
                float2 lightUV  = _LightPosition.xy;
                float2 delta    = (uv - lightUV) * (1.0 / _Samples) * _Density * _RayLength;

                float2 sampleUV = uv;
                float3 accumulated = 0;
                float  decayAccum  = 1.0;

                for (int s = 0; s < _Samples; s++)
                {
                    sampleUV   -= delta;
                    float3 bright = BrightPass(sampleUV);
                    // Accumulate, weighted by decay
                    accumulated += bright * decayAccum * _Weight;
                    decayAccum  *= _Decay;
                }

                accumulated *= _Exposure;

                // Tint rays with the light color
                float3 rays = accumulated * _RayColor.rgb;

                // Fade rays out when light source is off screen or behind camera
                float2 lightCenter = lightUV - 0.5;
                float  offscreen   = saturate(1.0 - length(lightCenter) * 1.5);
                rays *= offscreen;

                // Additive blend onto scene
                float3 finalColor = scene.rgb + rays * _Intensity;
                return half4(finalColor, scene.a);
            }
            ENDHLSL
        }
    }
}
