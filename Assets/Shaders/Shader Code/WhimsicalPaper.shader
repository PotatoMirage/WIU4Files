Shader "Hidden/Custom/WhimsicalPaper"
{
    Properties { _MainTex ("Texture", 2D) = "white" {} }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "WhimsicalPaperPass"
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            float  _Intensity;
            float4 _PaperColor;
            float  _GrainAmount;
            float  _InkBleed;
            float  _SketchStrength;
            float  _Vignette;

            struct Attributes { uint vertexID : SV_VertexID; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings o;
                o.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                o.uv         = GetFullScreenTriangleTexCoord(input.vertexID);
                return o;
            }

            // ── Helpers ───────────────────────────────────────────────

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

            float lum(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

            // ── Wobble ────────────────────────────────────────────────
            float2 wobble(float2 uv, float str)
            {
                float wx  = valueNoise(uv * 25.0 + float2(0.0, 1.7)) - 0.5;
                float wy  = valueNoise(uv * 25.0 + float2(3.3, 0.0)) - 0.5;
                      wx += (valueNoise(uv * 55.0 + float2(1.1, 2.2)) - 0.5) * 0.5;
                      wy += (valueNoise(uv * 55.0 + float2(4.4, 0.9)) - 0.5) * 0.5;
                return float2(wx, wy) * str;
            }

            // ── Sobel edge on wobbled UVs ─────────────────────────────
            float inkEdge(float2 uv, float wobbleStr, float px)
            {
                float2 offsets[8] = {
                    float2(-px,  px), float2(0,  px), float2( px,  px),
                    float2(-px,  0),                  float2( px,  0),
                    float2(-px, -px), float2(0, -px), float2( px, -px)
                };

                float s[8];
                for (int i = 0; i < 8; i++)
                {
                    float2 wob = wobble(uv + offsets[i], wobbleStr);
                    s[i] = lum(SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv + offsets[i] + wob).rgb);
                }

                float gx = -s[0] + s[2] - 2*s[3] + 2*s[4] - s[5] + s[7];
                float gy = -s[0] - 2*s[1] - s[2] + s[5] + 2*s[6] + s[7];
                return saturate(sqrt(gx*gx + gy*gy) * 4.5);
            }

            // ── Shadow sketch hatching ────────────────────────────────
            // Three angle passes build up like real cross-hatching.
            // darkness 0 = bright (no lines), 1 = deep shadow (dense lines).
            float shadowHatch(float2 uv, float darkness)
            {
                float2 wob1 = wobble(uv,                        0.0008);
                float2 wob2 = wobble(uv + float2(0.3, 0.7),    0.0006);
                float2 wob3 = wobble(uv + float2(0.6, 0.2),    0.0005);

                float2 wuv1 = uv + wob1;
                float2 wuv2 = uv + wob2;
                float2 wuv3 = uv + wob3;

                float scale = 160.0;

                // Layer 1: ~45 degrees — appears in mid shadows
                float d1 = frac((wuv1.x + wuv1.y) * scale);
                float t1 = 1.0 - saturate((darkness - 0.25) / 0.75) * 0.88;
                float line1 = step(t1, d1) * (1.0 - step(t1 + 0.035, d1));

                // Layer 2: ~-45 degrees — appears in darker shadows
                float d2 = frac((wuv2.x - wuv2.y) * scale);
                float t2 = 1.0 - saturate((darkness - 0.50) / 0.50) * 0.82;
                float line2 = step(t2, d2) * (1.0 - step(t2 + 0.035, d2));

                // Layer 3: near-horizontal — deepest shadows only
                float d3 = frac(wuv3.y * scale * 1.1);
                float t3 = 1.0 - saturate((darkness - 0.72) / 0.28) * 0.75;
                float line3 = step(t3, d3) * (1.0 - step(t3 + 0.03, d3));

                return saturate(line1 * 0.55 + line2 * 0.35 + line3 * 0.25);
            }

            // ── Brushstroke texture ───────────────────────────────────
            float brushStroke(float2 uv)
            {
                float2 stretched = float2(uv.x * 3.0, uv.y * 120.0);
                float stroke  = valueNoise(stretched             ) * 0.50;
                      stroke += valueNoise(stretched * 2.1 + 1.3 ) * 0.25;
                      stroke += valueNoise(stretched * 4.3 + 2.7 ) * 0.125;
                float2 diag   = float2((uv.x + uv.y) * 2.0, (uv.x - uv.y) * 80.0);
                      stroke += valueNoise(diag) * 0.125;
                return stroke;
            }

            // ── Posterization ─────────────────────────────────────────
            float3 posterize(float3 col, float steps)
            {
                return floor(col * steps + 0.5) / steps;
            }

            // ── Paper fiber ───────────────────────────────────────────
            float paperFiber(float2 uv)
            {
                float n  = valueNoise(uv * 350.0)  * 0.50;
                      n += valueNoise(uv * 700.0)  * 0.25;
                      n += valueNoise(uv * 1400.0) * 0.125;
                return n;
            }

            // ── Fragment ──────────────────────────────────────────────
            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                half4  scene    = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                float  sceneLum = lum(scene.rgb);

                // ── 1. Posterize ─────────────────────────────────────
                float posterSteps = lerp(255.0, 5.0, _Intensity * 0.85);
                float3 posted = posterize(scene.rgb, posterSteps);

                // ── 2. Grimm color grade ─────────────────────────────
                float3 graded = posted;
                float  lumVal = lum(graded);
                float  desat  = smoothstep(0.2, 0.8, lumVal) * 0.4;
                graded = lerp(graded, float3(lumVal, lumVal, lumVal), desat * _Intensity);
                float shadowStr = (1.0 - lumVal) * _Intensity * 0.15;
                graded.r -= shadowStr * 0.3;
                graded.g += shadowStr * 0.1;
                graded.b -= shadowStr * 0.5;
                float highStr = lumVal * _Intensity * 0.08;
                graded.r -= highStr * 0.2;
                graded.b += highStr * 0.3;
                graded = saturate(graded);

                // ── 3. Brushstroke texture ───────────────────────────
                float brush = brushStroke(uv);
                float brushDark   = 1.0 - (1.0 - brush) * _GrainAmount * _Intensity * 0.45;
                float brushBright = 1.0 + (brush - 0.5)  * _GrainAmount * _Intensity * 0.2;
                float3 painted = graded * brushDark * brushBright;

                // ── 4. Paper fiber ───────────────────────────────────
                float fiber = paperFiber(uv);
                painted = painted * (1.0 - fiber * _GrainAmount * _Intensity * 0.15);

                // ── 5. Shadow sketch hatching ────────────────────────
                // Bright areas stay clean, shadows fill with pencil strokes
                float darkness = saturate(1.0 - sceneLum);
                float hatch    = shadowHatch(uv, darkness);
                float3 hatchColor = lerp(painted, float3(0.08, 0.06, 0.04), hatch);
                float3 hatched    = lerp(painted, hatchColor, _SketchStrength * _Intensity);

                // ── 6. Thick wobbly ink outlines ─────────────────────
                float edge1 = inkEdge(uv, 0.0012 * _SketchStrength, 0.003);
                float edge2 = inkEdge(uv + float2(0.001, 0.0005), 0.0008 * _SketchStrength, 0.0018);
                edge1 = pow(saturate(edge1), 0.5);
                edge2 = pow(saturate(edge2), 0.7);
                float inkLine = saturate(edge1 * 0.8 + edge2 * 0.35);

                float3 inkColor = float3(0.06, 0.04, 0.03);
                float3 outlined = lerp(hatched, inkColor, inkLine * _SketchStrength * _Intensity);

                // ── 7. Ink bleed halo ────────────────────────────────
                float2 bleedPx = float2(0.0015, 0.0015) * _InkBleed;
                float edgeN = inkEdge(uv + float2(0,  bleedPx.y), 0.0005, 0.002);
                float edgeS = inkEdge(uv - float2(0,  bleedPx.y), 0.0005, 0.002);
                float edgeE = inkEdge(uv + float2(bleedPx.x, 0),  0.0005, 0.002);
                float edgeW = inkEdge(uv - float2(bleedPx.x, 0),  0.0005, 0.002);
                float bleedHalo = saturate((edgeN + edgeS + edgeE + edgeW) * 0.25 - inkLine * 0.3);
                outlined = lerp(outlined, inkColor * 1.5, bleedHalo * _InkBleed * _Intensity * 0.3);

                // ── 8. Deep forest vignette ──────────────────────────
                float2 vc    = uv - 0.5;
                float  vDist = dot(vc, vc) * 2.2;
                float  vign  = 1.0 - saturate(pow(vDist, 1.4) * _Vignette * _Intensity);
                float3 vigColor  = float3(0.03, 0.05, 0.03);
                float3 vignetted = lerp(vigColor, outlined, vign);

                // ── 9. Final blend ───────────────────────────────────
                float3 final = lerp(scene.rgb, vignetted, _Intensity);
                return half4(final, 1.0);
            }
            ENDHLSL
        }
    }
}