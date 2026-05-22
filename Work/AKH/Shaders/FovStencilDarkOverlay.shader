Shader "Hidden/FOV/DarkOverlay"
{
    Properties
    {
        _Darkness ("Darkness", Range(0, 1)) = 0.8
        _DebugVisibilityMask ("Debug Visibility Mask", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
        }

        Pass
        {
            Name "FovDarkOverlay"

            ZTest Always
            ZWrite Off
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D_X(_FovVisibilityTexture);
            SAMPLER(sampler_BlitTexture);
            SAMPLER(sampler_FovVisibilityTexture);

            float _Darkness;
            float _DebugVisibilityMask;
            float _FovVisibilityDebug;

            float4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);
                float visible = SAMPLE_TEXTURE2D_X(_FovVisibilityTexture, sampler_FovVisibilityTexture, uv).r;

                if (_DebugVisibilityMask > 0.5 || _FovVisibilityDebug > 0.5)
                    return float4(visible.xxx, 1);

                float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);
                float darkAmount = saturate(_Darkness) * (1.0 - saturate(visible));
                color.rgb *= 1.0 - darkAmount;
                return color;
            }
            ENDHLSL
        }
    }
}
