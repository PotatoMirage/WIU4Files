Shader "Custom/ToonEnemy"
{
    Properties
    {
        [Header(Textures)]
        _MainTex               ("Base Map",              2D)           = "white" {}
        _Color                 ("Tint Color",            Color)        = (1,1,1,1)
        _NormalMap             ("Normal Map",            2D)           = "bump"  {}
        _NormalStrength        ("Normal Strength",       Range(0,2))   = 1.0
        _MetallicMap           ("Metallic Map",          2D)           = "black" {}
        _OcclusionMap          ("Occlusion Map",         2D)           = "white" {}
        _OcclusionStrength     ("Occlusion Strength",    Range(0,1))   = 0.8
        _HeightMap             ("Height Map",            2D)           = "gray"  {}
        _ParallaxStrength      ("Parallax Strength",     Range(0,0.1)) = 0.02
        _HeightShadingStrength ("Height Shading Strength", Range(0,1)) = 0.4

        [Header(Toon Shading)]
        _LightThreshold        ("Light Threshold",       Range(0,1))   = 0.75
        _MidThreshold          ("Mid Threshold",         Range(0,1))   = 0.35
        _BandSoftness          ("Band Softness",         Range(0,0.2)) = 0.04
        _LightColor            ("Light Color",           Color)        = (1.00, 0.97, 0.88, 1)
        _MidColor              ("Mid Color",             Color)        = (0.72, 0.65, 0.52, 1)
        _ShadowColor           ("Shadow Color",          Color)        = (0.22, 0.18, 0.14, 1)

        [Header(Specular)]
        _SpecularThreshold     ("Specular Threshold",    Range(0,1))   = 0.85
        _SpecularSoftness      ("Specular Softness",     Range(0,0.1)) = 0.02
        _SpecularColor         ("Specular Color",        Color)        = (1.00, 0.95, 0.80, 1)
        _SpecularStrength      ("Specular Strength",     Range(0,1))   = 0.6

        [Header(Emissive)]
        _EmissiveMap           ("Emissive Map",          2D)           = "black" {}
        _EmissiveColor         ("Emissive Color",        Color)        = (0.4, 0.0, 1.0, 1)
        _EmissiveIntensity     ("Emissive Intensity",    Range(0,5))   = 2.0
        _PulseSpeed            ("Pulse Speed",           Range(0,5))   = 1.2
        _PulseMin              ("Pulse Min Brightness",  Range(0,1))   = 0.2
        _PulseMax              ("Pulse Max Brightness",  Range(0,1))   = 1.0

        [Header(Outline)]
        _OutlineWidth          ("Outline Width",         Range(0,0.05)) = 0.015
        _OutlineColor          ("Outline Color",         Color)         = (0.06, 0.04, 0.03, 1)

        [Header(Dissolve)]
        _DissolveAmount        ("Dissolve Amount",       Range(0,1))   = 0.0
        _DissolveNoiseScale    ("Noise Scale",           Range(1,20))  = 6.0
        _EdgeWidth             ("Edge Glow Width",       Range(0,0.15))= 0.05
        _EdgeColorLow          ("Edge Color Low",        Color)        = (1.0, 0.15, 0.0, 1)
        _EdgeColorHigh         ("Edge Color High",       Color)        = (1.0, 0.85, 0.2, 1)
        _EdgeEmission          ("Edge Emission",         Range(0,10))  = 5.0
        _BurnNoiseStrength     ("Burn Noise Strength",   Range(0,1))   = 0.3
    }

    SubShader
    {
        // TransparentCutout so clip() works correctly for dissolve
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }

        // ── PASS 1: Toon Lit + Dissolve ──────────────────────────────
        Pass
        {
            Name "ToonForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);     SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MetallicMap);   SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_OcclusionMap);  SAMPLER(sampler_OcclusionMap);
            TEXTURE2D(_HeightMap);     SAMPLER(sampler_HeightMap);
            TEXTURE2D(_EmissiveMap);   SAMPLER(sampler_EmissiveMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _NormalStrength;
                float  _OcclusionStrength;
                float  _ParallaxStrength;
                float  _HeightShadingStrength;
                float  _LightThreshold;
                float  _MidThreshold;
                float  _BandSoftness;
                float4 _LightColor;
                float4 _MidColor;
                float4 _ShadowColor;
                float  _SpecularThreshold;
                float  _SpecularSoftness;
                float4 _SpecularColor;
                float  _SpecularStrength;
                float4 _EmissiveColor;
                float  _EmissiveIntensity;
                float  _PulseSpeed;
                float  _PulseMin;
                float  _PulseMax;
                float  _OutlineWidth;
                float4 _OutlineColor;
                float  _DissolveAmount;
                float  _DissolveNoiseScale;
                float  _EdgeWidth;
                float4 _EdgeColorLow;
                float4 _EdgeColorHigh;
                float  _EdgeEmission;
                float  _BurnNoiseStrength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirTS   : TEXCOORD5;
                float  meshHeight  : TEXCOORD6;
            };

            // ── Noise ─────────────────────────────────────────────────
            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }
            float valueNoise(float2 p)
            {
                float2 i = floor(p); float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i), hash(i+float2(1,0)), u.x),
                            lerp(hash(i+float2(0,1)), hash(i+float2(1,1)), u.x), u.y);
            }
            float burnNoise(float2 uv, float scale)
            {
                float n  = valueNoise(uv * scale)       * 0.500;
                      n += valueNoise(uv * scale * 2.1) * 0.250;
                      n += valueNoise(uv * scale * 4.3) * 0.125;
                      n += valueNoise(uv * scale * 8.7) * 0.0625;
                return n;
            }

            // ── Parallax ──────────────────────────────────────────────
            float2 ParallaxOffset(float2 uv, float3 viewDirTS, float strength)
            {
                float h = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r - 0.5;
                return uv + (viewDirTS.xy / (viewDirTS.z + 0.42)) * h * strength;
            }

            Varyings Vert(Attributes i)
            {
                Varyings o;
                o.positionWS  = TransformObjectToWorld(i.positionOS);
                o.positionCS  = TransformWorldToHClip(o.positionWS);
                o.uv          = TRANSFORM_TEX(i.uv, _MainTex);
                o.normalWS    = TransformObjectToWorldNormal(i.normalOS);
                o.tangentWS   = TransformObjectToWorldDir(i.tangentOS.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS)
                                * (i.tangentOS.w * GetOddNegativeScale());
                float3 viewWS = GetCameraPositionWS() - o.positionWS;
                float3x3 W2T  = float3x3(normalize(o.tangentWS),
                                         normalize(o.bitangentWS),
                                         normalize(o.normalWS));
                o.viewDirTS   = mul(W2T, viewWS);
                // Pass object-space height to fragment for burn direction
                // Assumes pivot at base — adjust +1.0 offset if pivot is centered
                o.meshHeight  = saturate((i.positionOS.y + 1.0) * 0.5);
                return o;
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // ── Dissolve clip ────────────────────────────────────
                // Do this first — cheapest early exit for clipped pixels
                float noise         = burnNoise(i.uv, _DissolveNoiseScale);
                float dissolveNoise = lerp(i.meshHeight, noise, _BurnNoiseStrength);
                float dissolveMask  = dissolveNoise - _DissolveAmount;
                clip(dissolveMask);

                // ── Glowing burn edge ────────────────────────────────
                float edgeMask  = 1.0 - saturate(dissolveMask / _EdgeWidth);
                float3 edgeColor = lerp(_EdgeColorHigh.rgb, _EdgeColorLow.rgb, edgeMask);

                // ── Parallax UV ──────────────────────────────────────
                float3 viewDirTS = normalize(i.viewDirTS);
                float2 uv        = ParallaxOffset(i.uv, viewDirTS, _ParallaxStrength);

                // ── Sample all maps ──────────────────────────────────
                half4 albedo       = SAMPLE_TEXTURE2D(_MainTex,      sampler_MainTex,      uv) * _Color;
                half  metallic     = SAMPLE_TEXTURE2D(_MetallicMap,  sampler_MetallicMap,  uv).r;
                half  occlusion    = SAMPLE_TEXTURE2D(_OcclusionMap, sampler_OcclusionMap, uv).r;
                half  height       = SAMPLE_TEXTURE2D(_HeightMap,    sampler_HeightMap,    uv).r;
                half3 emissiveMask = SAMPLE_TEXTURE2D(_EmissiveMap,  sampler_EmissiveMap,  uv).rgb;
                occlusion          = lerp(1.0, occlusion, _OcclusionStrength);

                // ── Normal map ───────────────────────────────────────
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));
                normalTS.xy   *= _NormalStrength;
                float3x3 TBN   = float3x3(normalize(i.tangentWS),
                                          normalize(i.bitangentWS),
                                          normalize(i.normalWS));
                float3 normalWS = normalize(mul(normalTS, TBN));

                // ── Main light ───────────────────────────────────────
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light  mainLight   = GetMainLight(shadowCoord);
                float  NdotL       = dot(normalWS, mainLight.direction);
                float  diffuse     = NdotL * mainLight.shadowAttenuation;
                diffuse           += (height - 0.5) * _HeightShadingStrength;

                // ── 3-tone bands ──────────────────────────────────────
                float lightMask  = smoothstep(_LightThreshold - _BandSoftness,
                                              _LightThreshold + _BandSoftness, diffuse);
                float midMask    = smoothstep(_MidThreshold   - _BandSoftness,
                                              _MidThreshold   + _BandSoftness, diffuse)
                                 * (1.0 - lightMask);
                float shadowMask = 1.0 - lightMask - midMask;

                shadowMask = saturate(shadowMask + (1.0 - occlusion) * 0.5);
                lightMask  = saturate(lightMask  * occlusion);
                midMask    = saturate(1.0 - lightMask - shadowMask);

                float3 toonColor = _LightColor.rgb  * lightMask
                                 + _MidColor.rgb    * midMask
                                 + _ShadowColor.rgb * shadowMask;
                float3 litColor  = albedo.rgb * toonColor * mainLight.color;

                // ── Specular ─────────────────────────────────────────
                float3 viewDirWS = normalize(GetCameraPositionWS() - i.positionWS);
                float3 halfDir   = normalize(mainLight.direction + viewDirWS);
                float  NdotH     = dot(normalWS, halfDir);
                float  specular  = smoothstep(_SpecularThreshold - _SpecularSoftness,
                                              _SpecularThreshold + _SpecularSoftness, NdotH);
                specular *= lightMask * metallic * _SpecularStrength
                          * mainLight.shadowAttenuation;
                float3 shadedColor = lerp(litColor, _SpecularColor.rgb * albedo.rgb, specular);

                // ── Emissive pulse ────────────────────────────────────
                float pulse    = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                pulse          = lerp(_PulseMin, _PulseMax, pulse);
                float3 emissive = emissiveMask * _EmissiveColor.rgb * _EmissiveIntensity * pulse;

                // ── Combine ───────────────────────────────────────────
                float3 finalColor = shadedColor + emissive;
                // Burn edge overrides shading near dissolve front
                finalColor = lerp(finalColor, edgeColor * _EdgeEmission, edgeMask * 0.85);

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }

        // ── PASS 2: Inverted Hull Outline + Dissolve ─────────────────
        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float  _NormalStrength;
                float  _OcclusionStrength;
                float  _ParallaxStrength;
                float  _HeightShadingStrength;
                float  _LightThreshold;
                float  _MidThreshold;
                float  _BandSoftness;
                float4 _LightColor;
                float4 _MidColor;
                float4 _ShadowColor;
                float  _SpecularThreshold;
                float  _SpecularSoftness;
                float4 _SpecularColor;
                float  _SpecularStrength;
                float4 _EmissiveColor;
                float  _EmissiveIntensity;
                float  _PulseSpeed;
                float  _PulseMin;
                float  _PulseMax;
                float  _OutlineWidth;
                float4 _OutlineColor;
                float  _DissolveAmount;
                float  _DissolveNoiseScale;
                float  _EdgeWidth;
                float4 _EdgeColorLow;
                float4 _EdgeColorHigh;
                float  _EdgeEmission;
                float  _BurnNoiseStrength;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float  meshHeight : TEXCOORD1;
            };

            float hash2(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 19.19);
                return frac(p.x * p.y);
            }
            float valueNoise2(float2 p)
            {
                float2 i = floor(p); float2 f = frac(p);
                float2 u = f*f*(3.0-2.0*f);
                return lerp(lerp(hash2(i),hash2(i+float2(1,0)),u.x),
                            lerp(hash2(i+float2(0,1)),hash2(i+float2(1,1)),u.x),u.y);
            }
            float burnNoise2(float2 uv, float scale)
            {
                float n  = valueNoise2(uv*scale)       * 0.500;
                      n += valueNoise2(uv*scale*2.1)   * 0.250;
                      n += valueNoise2(uv*scale*4.3)   * 0.125;
                      n += valueNoise2(uv*scale*8.7)   * 0.0625;
                return n;
            }

            Varyings OutlineVert(Attributes i)
            {
                Varyings o;
                o.meshHeight  = saturate((i.positionOS.y + 1.0) * 0.5);
                o.uv          = TRANSFORM_TEX(i.uv, _MainTex);
                float3 posWS  = TransformObjectToWorld(i.positionOS);
                float4 posCS  = TransformWorldToHClip(posWS);
                float3 normWS = TransformObjectToWorldNormal(i.normalOS);
                float3 normCS = normalize(TransformWorldToHClipDir(normWS));
                float2 ndcOffset = normalize(normCS.xy);
                float aspect  = _ScreenParams.x / _ScreenParams.y;
                ndcOffset.x  /= aspect;
                posCS.xy     += ndcOffset * _OutlineWidth;
                o.positionCS  = posCS;
                return o;
            }

            half4 OutlineFrag(Varyings i) : SV_Target
            {
                // Outline clips in sync with the mesh
                float noise       = burnNoise2(i.uv, _DissolveNoiseScale);
                float dissolveNoise = lerp(i.meshHeight, noise, _BurnNoiseStrength);
                clip(dissolveNoise - _DissolveAmount);
                return _OutlineColor;
            }
            ENDHLSL
        }

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
