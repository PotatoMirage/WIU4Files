Shader "Hidden/Custom/AtmosphericFog"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "AtmosphericFogPass"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float  _Intensity;
            float  _FogStart;
            float  _FogEnd;
            float  _FogDensity;
            float4 _FogColor;
            float  _HorizonBias;
            float  _UseHeightFog;
            float  _HeightFogStart;
            float  _HeightFogEnd;
            float  _HeightFogDensity;
            float3 _PlayerWorldPos;
            float  _PlayerShieldRadius;

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

            half4 Frag(Varyings i) : SV_Target
            {
                float2 uv    = i.uv;
                half4  scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                // ── Reconstruct world position from depth ────────────
                float  depth       = SampleSceneDepth(uv);
                float  linearDepth = LinearEyeDepth(depth, _ZBufferParams);

                float4 ndcPos = float4(uv * 2.0 - 1.0, depth, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    ndcPos.y = -ndcPos.y;
                #endif
                float4 worldPos = mul(UNITY_MATRIX_I_VP, ndcPos);
                worldPos.xyz   /= worldPos.w;

                // ── Distance fog (measured from player) ──────────────
                float distFromPlayer = distance(worldPos.xyz, _PlayerWorldPos);
                float fogFactor      = saturate((distFromPlayer - _FogStart)
                                       / max(_FogEnd - _FogStart, 0.001));
                fogFactor            = 1.0 - exp(-fogFactor * fogFactor * _FogDensity * 8.0);

                float3 viewDir     = normalize(worldPos.xyz - _WorldSpaceCameraPos);
                float  horizonMask = 1.0 - abs(viewDir.y);
                fogFactor          = lerp(fogFactor, saturate(fogFactor * 1.5),
                                         horizonMask * _HorizonBias);

                // ── Height fog (optional ground-hugging layer) ────────
                float heightFogFactor = 0.0;
                if (_UseHeightFog > 0.5)
                {
                    float worldY     = worldPos.y;
                    heightFogFactor  = 1.0 - saturate((worldY - _HeightFogStart)
                                       / max(_HeightFogEnd - _HeightFogStart, 0.001));
                    heightFogFactor  = pow(heightFogFactor, 2.0) * _HeightFogDensity;
                    heightFogFactor *= saturate(linearDepth * 0.1);
                }

                // ── Combine fog layers ────────────────────────────────
                float totalFog = saturate(fogFactor + heightFogFactor);

                // Sky pixels get no fog
                float isSky = step(0.9999, depth);
                #if defined(UNITY_REVERSED_Z)
                    isSky = step(depth, 0.0001);
                #endif
                totalFog *= (1.0 - isSky);

                // ── Player screen-space shield ────────────────────────
                // Project player into screen UV
                float4 playerClip = mul(UNITY_MATRIX_VP, float4(_PlayerWorldPos, 1.0));
                float2 playerUV   = (playerClip.xy / playerClip.w) * 0.5 + 0.5;
                #if UNITY_UV_STARTS_AT_TOP
                    playerUV.y = 1.0 - playerUV.y;
                #endif

                float playerCamDist = distance(_PlayerWorldPos, _WorldSpaceCameraPos);

                float aspect      = _ScreenParams.x / _ScreenParams.y;
                float2 screenDiff = (uv - playerUV) * float2(aspect, 1.0);
                float screenDist  = length(screenDiff);

                // Block fog on pixels that are:
                // 1. Behind the player (further from camera)
                // 2. Within _PlayerShieldRadius on screen
                // smoothstep goes 0 (full block) → 1 (no block) as screenDist grows
                float isBehindPlayer = step(playerCamDist, linearDepth);
                float screenShield   = smoothstep(0.0, _PlayerShieldRadius, screenDist);
                totalFog             = lerp(totalFog, totalFog * screenShield, isBehindPlayer);

                // ── Apply ─────────────────────────────────────────────
                float3 foggedColor = lerp(scene.rgb, _FogColor.rgb, totalFog * _Intensity);
                return half4(foggedColor, scene.a);
            }
            ENDHLSL
        }
    }
}