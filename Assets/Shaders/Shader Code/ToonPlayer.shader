Shader "Custom/ToonPlayer"
{
    Properties
    {
        [Header(Textures)]
        _MainTex               ("Base Map",              2D)           = "white" {}
        _Color                 ("Tint Color",            Color)        = (1,1,1,1)
        _NormalMap             ("Normal Map",            2D)           = "bump"  {}
        _NormalStrength        ("Normal Strength",       Range(0,2))   = 1.0
        _MetallicMap           ("Metallic Map",          2D)           = "black" {}
        _RoughnessMap          ("Roughness Map",         2D)           = "white" {}
        _OcclusionStrength     ("Occlusion Strength",    Range(0,1))   = 0.8
        _HeightMap             ("Height Map",            2D)           = "gray"  {}
        _ParallaxStrength      ("Parallax Strength",     Range(0,0.1)) = 0.015
        _HeightShadingStrength ("Height Shading Strength", Range(0,1)) = 0.3

        [Header(Toon Shading)]
        _LightThreshold        ("Light Threshold",       Range(0,1))   = 0.65
        _MidThreshold          ("Mid Threshold",         Range(0,1))   = 0.25
        _BandSoftness          ("Band Softness",         Range(0,0.2)) = 0.07
        _LightColor            ("Light Color",           Color)        = (1.00, 0.98, 0.95, 1)
        _MidColor              ("Mid Color",             Color)        = (0.75, 0.70, 0.65, 1)
        _ShadowColor           ("Shadow Color",          Color)        = (0.28, 0.22, 0.20, 1)

        [Header(Light Neutralization)]
        // Strips environment color tint from lighting so player always reads clean/white
        _LightDesaturation     ("Light Desaturation",    Range(0,1))   = 0.75
        _LightBrightness       ("Light Brightness Boost",Range(0,2))   = 1.25

        [Header(Specular)]
        _SpecularThreshold     ("Specular Threshold",    Range(0,1))   = 0.88
        _SpecularSoftness      ("Specular Softness",     Range(0,0.1)) = 0.03
        _SpecularColor         ("Specular Color",        Color)        = (1.00, 0.98, 0.96, 1)
        _SpecularStrength      ("Specular Strength",     Range(0,1))   = 0.5

        [Header(Rim Light)]
        _RimColor              ("Rim Color",             Color)        = (0.85, 0.10, 0.10, 1)
        _RimStrength           ("Rim Strength",          Range(0,3))   = 1.4
        _RimThreshold          ("Rim Threshold",         Range(0,1))   = 0.55
        _RimSoftness           ("Rim Softness",          Range(0,0.3)) = 0.08

        [Header(Outline)]
        _OutlineWidth          ("Outline Width",         Range(0,0.05)) = 0.013
        _OutlineColor          ("Outline Color",         Color)         = (0.35, 0.04, 0.04, 1)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" }

        // ── PASS 1: Toon Lit ─────────────────────────────────────────
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

            TEXTURE2D(_MainTex);      SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);    SAMPLER(sampler_NormalMap);
            TEXTURE2D(_MetallicMap);  SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_RoughnessMap); SAMPLER(sampler_RoughnessMap);
            TEXTURE2D(_HeightMap);    SAMPLER(sampler_HeightMap);

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

                float  _LightDesaturation;
                float  _LightBrightness;

                float  _SpecularThreshold;
                float  _SpecularSoftness;
                float4 _SpecularColor;
                float  _SpecularStrength;

                float4 _RimColor;
                float  _RimStrength;
                float  _RimThreshold;
                float  _RimSoftness;

                float  _OutlineWidth;
                float4 _OutlineColor;
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
            };

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
                float3x3 worldToTangent = float3x3(
                    normalize(o.tangentWS),
                    normalize(o.bitangentWS),
                    normalize(o.normalWS)
                );
                o.viewDirTS = mul(worldToTangent, viewWS);
                return o;
            }

            // ── Parallax offset ───────────────────────────────────────
            float2 ParallaxOffset(float2 uv, float3 viewDirTS, float strength)
            {
                float h      = SAMPLE_TEXTURE2D(_HeightMap, sampler_HeightMap, uv).r;
                float height = h - 0.5;
                float2 offset = (viewDirTS.xy / (viewDirTS.z + 0.42)) * height * strength;
                return uv + offset;
            }

            // ── Neutralize color tint ─────────────────────────────────
            float3 NeutralizeColor(float3 col, float amount)
            {
                float lum = dot(col, float3(0.299, 0.587, 0.114));
                return lerp(col, float3(lum, lum, lum), amount);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                // ── Parallax UV ──────────────────────────────────────
                float3 viewDirTS = normalize(i.viewDirTS);
                float2 uv        = ParallaxOffset(i.uv, viewDirTS, _ParallaxStrength);

                // ── Sample all maps ──────────────────────────────────
                half4 albedo    = SAMPLE_TEXTURE2D(_MainTex,      sampler_MainTex,      uv) * _Color;
                half  metallic  = SAMPLE_TEXTURE2D(_MetallicMap,  sampler_MetallicMap,  uv).r;
                // Roughness map: high roughness = less specular
                half  roughness = SAMPLE_TEXTURE2D(_RoughnessMap, sampler_RoughnessMap, uv).r;
                half  height    = SAMPLE_TEXTURE2D(_HeightMap,    sampler_HeightMap,    uv).r;

                // ── Normal map ───────────────────────────────────────
                half3 normalTS  = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, uv));
                normalTS.xy    *= _NormalStrength;
                float3x3 TBN    = float3x3(
                    normalize(i.tangentWS),
                    normalize(i.bitangentWS),
                    normalize(i.normalWS)
                );
                float3 normalWS = normalize(mul(normalTS, TBN));

                // ── Main light ───────────────────────────────────────
                float4 shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                Light  mainLight   = GetMainLight(shadowCoord);

                // ── Neutralize environment light color ───────────────
                // Strip the green/yellow/blue tint from scene lighting
                // so Red Riding Hood always reads under clean white light.
                // _LightDesaturation 0 = full scene color, 1 = pure white light
                float3 neutralLight = NeutralizeColor(mainLight.color, _LightDesaturation)
                                    * _LightBrightness;

                float NdotL   = dot(normalWS, mainLight.direction);
                float diffuse = NdotL * mainLight.shadowAttenuation;

                // ── Height shading ───────────────────────────────────
                float heightInfluence = (height - 0.5) * _HeightShadingStrength;
                diffuse += heightInfluence;

                // ── 3-tone bands ──────────────────────────────────────
                float lightMask  = smoothstep(_LightThreshold - _BandSoftness,
                                              _LightThreshold + _BandSoftness, diffuse);
                float midMask    = smoothstep(_MidThreshold   - _BandSoftness,
                                              _MidThreshold   + _BandSoftness, diffuse)
                                 * (1.0 - lightMask);
                float shadowMask = 1.0 - lightMask - midMask;

                float3 toonColor = _LightColor.rgb  * lightMask
                                 + _MidColor.rgb    * midMask
                                 + _ShadowColor.rgb * shadowMask;

                // Use neutralized light instead of raw mainLight.color
                float3 litColor = albedo.rgb * toonColor * neutralLight;

                // ── Roughness-gated toon specular ─────────────────────
                // Roughness inverted — smooth areas (low roughness) get specular
                float smoothness = 1.0 - roughness;
                float3 viewDirWS = normalize(GetCameraPositionWS() - i.positionWS);
                float3 halfDir   = normalize(mainLight.direction + viewDirWS);
                float  NdotH     = dot(normalWS, halfDir);
                float  specular  = smoothstep(_SpecularThreshold - _SpecularSoftness,
                                              _SpecularThreshold + _SpecularSoftness, NdotH);
                // Gate by smoothness AND metallic — shiny smooth areas get full spec
                specular *= lightMask * smoothness * metallic * _SpecularStrength
                          * mainLight.shadowAttenuation;

                float3 shadedColor = lerp(litColor, _SpecularColor.rgb * albedo.rgb, specular);

                // ── Soft red rim light ────────────────────────────────
                // Rim = how perpendicular the surface normal is to the view dir
                // Strong at silhouette edges, zero facing camera
                float  NdotV   = dot(normalWS, viewDirWS);
                float  rim     = 1.0 - saturate(NdotV);
                // Threshold so rim is a clean band, not a gradient wash
                float  rimMask = smoothstep(_RimThreshold - _RimSoftness,
                                            _RimThreshold + _RimSoftness, rim);
                // Only apply rim on surfaces facing the light, not in full shadow
                rimMask *= saturate(NdotL * 2.0);

                float3 rimLight = _RimColor.rgb * _RimStrength * rimMask;

                float3 finalColor = shadedColor + rimLight;
                return half4(finalColor, albedo.a);
            }
            ENDHLSL
        }

        // ── PASS 2: Inverted Hull Outline ────────────────────────────
        // Dark red outline instead of brown-black, reinforces her red cloak
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
                float  _LightDesaturation;
                float  _LightBrightness;
                float  _SpecularThreshold;
                float  _SpecularSoftness;
                float4 _SpecularColor;
                float  _SpecularStrength;
                float4 _RimColor;
                float  _RimStrength;
                float  _RimThreshold;
                float  _RimSoftness;
                float  _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float3 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings OutlineVert(Attributes i)
            {
                Varyings o;
                float3 posWS  = TransformObjectToWorld(i.positionOS);
                float4 posCS  = TransformWorldToHClip(posWS);
                float3 normWS = TransformObjectToWorldNormal(i.normalOS);
                float3 normCS = normalize(TransformWorldToHClipDir(normWS));

                // Work in NDC (clip.xy / clip.w) so the offset is in screen space,
                // giving a consistent pixel-width outline at any camera distance.
                float2 ndcOffset = normalize(normCS.xy);

                // Correct for aspect ratio — prevents oval outlines on wide screens
                float aspect  = _ScreenParams.x / _ScreenParams.y;
                ndcOffset.x  /= aspect;

                // No w multiply — keeping offset in pure clip space gives
                // consistent screen-size outline at any camera distance
                posCS.xy += ndcOffset * _OutlineWidth;
                o.positionCS = posCS;
                return o;
            }

            half4 OutlineFrag(Varyings i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // ── PASS 3: Depth Only ───────────────────────────────────
Pass
{
    Name "DepthOnly"
    Tags { "LightMode" = "DepthOnly" }
    ColorMask 0
    ZWrite On  // must be On so player blocks fog behind them
}

        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }
}
