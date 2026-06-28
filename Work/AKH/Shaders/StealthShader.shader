Shader "Custom/URP/CloakingStealth"
{
    Properties
    {
        [Header(Cloak)]
        _Alpha("Overall Alpha", Range(0, 1)) = 0.65
        _DistortionStrength("Distortion Strength", Range(0, 0.15)) = 0.035

        [Header(Noise)]
        _NoiseScale("Noise Scale", Float) = 18
        _NoiseSpeed("Noise Speed", Float) = 1.5
        _NoiseIntensity("Noise Intensity", Range(0, 2)) = 1

        [Header(Fresnel Edge)]
        [HDR]_EdgeColor("Edge Color", Color) = (0.1, 0.8, 1.0, 1)
        _RimPower("Rim Power", Range(0.2, 10)) = 2.5
        _RimIntensity("Rim Intensity", Range(0, 5)) = 1.8
        _EdgeAlpha("Edge Alpha", Range(0, 1)) = 0.9

        [Header(Scan Line)]
        _ScanLineStrength("Scan Line Strength", Range(0, 1)) = 0.18
        _ScanLineScale("Scan Line Scale", Float) = 35
        _ScanLineSpeed("Scan Line Speed", Float) = 2

        [Header(Vertex Wave)]
        _VertexWaveStrength("Vertex Wave Strength", Range(0, 0.2)) = 0.02
        _VertexWaveScale("Vertex Wave Scale", Float) = 10
        _VertexWaveSpeed("Vertex Wave Speed", Float) = 2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "CloakingStealthPass"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float _Alpha;
                float _DistortionStrength;

                float _NoiseScale;
                float _NoiseSpeed;
                float _NoiseIntensity;

                float4 _EdgeColor;
                float _RimPower;
                float _RimIntensity;
                float _EdgeAlpha;

                float _ScanLineStrength;
                float _ScanLineScale;
                float _ScanLineSpeed;

                float _VertexWaveStrength;
                float _VertexWaveScale;
                float _VertexWaveSpeed;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                float a = Hash21(i);
                float b = Hash21(i + float2(1, 0));
                float c = Hash21(i + float2(0, 1));
                float d = Hash21(i + float2(1, 1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float FBM(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;

                value += ValueNoise(uv) * amplitude;
                uv *= 2.0;
                amplitude *= 0.5;

                value += ValueNoise(uv) * amplitude;
                uv *= 2.0;
                amplitude *= 0.5;

                value += ValueNoise(uv) * amplitude;
                uv *= 2.0;
                amplitude *= 0.5;

                value += ValueNoise(uv) * amplitude;

                return value;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;

                float3 positionOS = input.positionOS.xyz;

                float wave =
                    sin((positionOS.y + _Time.y * _VertexWaveSpeed) * _VertexWaveScale) *
                    _VertexWaveStrength;

                positionOS += input.normalOS * wave;

                VertexPositionInputs positionInputs = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

                output.positionHCS = positionInputs.positionCS;
                output.screenPos = ComputeScreenPos(output.positionHCS);
                output.normalWS = normalize(normalInputs.normalWS);
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;

                float3 viewDirWS = normalize(_WorldSpaceCameraPos.xyz - input.positionWS);
                float3 normalWS = normalize(input.normalWS);

                float fresnel = 1.0 - saturate(dot(normalWS, viewDirWS));
                fresnel = pow(fresnel, _RimPower);

                float2 noiseUV = input.uv * _NoiseScale;
                noiseUV.y += _Time.y * _NoiseSpeed;

                float noise = FBM(noiseUV);
                noise = (noise - 0.5) * 2.0;
                noise *= _NoiseIntensity;

                float scanLine =
                    sin((input.positionWS.y * _ScanLineScale) - (_Time.y * _ScanLineSpeed)) * 0.5 + 0.5;

                float scanMask = scanLine * _ScanLineStrength;

                float2 distortionOffset;
                distortionOffset.x = noise;
                distortionOffset.y = FBM(noiseUV + 12.34) - 0.5;

                distortionOffset *= _DistortionStrength;
                distortionOffset *= 1.0 + fresnel * 1.5;

                float2 distortedUV = screenUV + distortionOffset;

                half3 sceneColor = SAMPLE_TEXTURE2D(
                    _CameraOpaqueTexture,
                    sampler_CameraOpaqueTexture,
                    distortedUV
                ).rgb;

                half3 edgeColor = _EdgeColor.rgb * _RimIntensity;

                float edgeMask = saturate(fresnel + scanMask);
                half3 finalColor = lerp(sceneColor, edgeColor, edgeMask);

                float finalAlpha = saturate(_Alpha + fresnel * _EdgeAlpha + scanMask);

                return half4(finalColor, finalAlpha);
            }

            ENDHLSL
        }
    }
}