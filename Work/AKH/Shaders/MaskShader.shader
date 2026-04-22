Shader "Gondr/MaskShader"
{
    Properties
    {
        _EdgeBand ("Edge Band", Range(0.02, 1)) = 0.82
        _NoiseScale ("Noise Scale", Range(0.01, 2)) = 0.07
        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.12
        _NoiseScroll ("Noise Scroll", Vector) = (0, 0, 0, 0)
        _DitherStrength ("Dither Strength", Range(0, 1)) = 0.11
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry-10"
        }
        LOD 100
        ColorMask 0
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Mask"
            ZTest Always

            Stencil
            {
                Ref 2
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            float _EdgeBand;
            float _NoiseScale;
            float _NoiseStrength;
            float4 _NoiseScroll;
            float _DitherStrength;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float FractalNoise(float2 p)
            {
                float v = 0.0;
                float a = 0.6;
                v += Noise(p) * a;
                p *= 2.02;
                a *= 0.5;
                v += Noise(p) * a;
                p *= 2.01;
                a *= 0.5;
                v += Noise(p) * a;
                return v;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float band = max(_EdgeBand, 0.001);
                float edgeFactor = saturate(input.uv.x / band);
                float linearFade = edgeFactor;
                float curvedFade = edgeFactor * edgeFactor * (3.0 - 2.0 * edgeFactor);
                float hazeFade = smoothstep(0.0, 0.7, edgeFactor);
                float visibility = lerp(linearFade, curvedFade, 0.55);
                visibility = lerp(visibility, hazeFade, 0.35);

                float2 worldUv = input.positionWS.xz * _NoiseScale;
                float lowFreq = FractalNoise(worldUv);
                float hiFreq = FractalNoise(input.positionWS.xz * (_NoiseScale * 0.45 + 0.0001));
                float organicNoise = saturate(lowFreq * 0.75 + hiFreq * 0.25) * 2.0 - 1.0;
                float noiseInfluence = saturate(1.0 - edgeFactor);
                visibility = saturate(visibility + organicNoise * _NoiseStrength * noiseInfluence);

                float dither = saturate(FractalNoise(input.positionWS.xz * 14.0 + float2(11.7, 3.1)));
                float threshold = 0.5 + (dither - 0.5) * _DitherStrength;

                clip(visibility - threshold);
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
