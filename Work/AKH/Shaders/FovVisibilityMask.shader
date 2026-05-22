Shader "Hidden/FOV/VisibilityMask"
{
    Properties
    {
        _FadeStart ("Fade Start", Range(0, 1)) = 0
        _FadeEnd ("Fade End", Range(0, 1)) = 1
        _VisibilityPower ("Visibility Power", Range(0.1, 4)) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry-10"
        }

        Pass
        {
            Name "FovVisibilityMask"

            ZWrite Off
            ZTest Always
            Cull Off
            ColorMask R
            Blend One One
            BlendOp Max

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

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
            };

            float _FadeStart;
            float _FadeEnd;
            float _VisibilityPower;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float fadeEnd = max(_FadeEnd, _FadeStart + 0.0001);
                float visibility = smoothstep(_FadeStart, fadeEnd, saturate(input.uv.x));
                visibility = pow(saturate(visibility), max(_VisibilityPower, 0.0001));
                return half4(visibility, 0, 0, 1);
            }
            ENDHLSL
        }
    }
}
