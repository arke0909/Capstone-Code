Shader "Custom/UIBlurURP"
{
    Properties
    {
        _BlurSize("Blur Size", Range(0, 20)) = 1.0
    }

    SubShader
    {
        // UI와 투명 처리를 위한 태그 설정
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // URP 배경 텍스처를 편하게 가져오기 위한 라이브러리
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

        struct Attributes
        {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float4 screenPos : TEXCOORD0;
        };

        float _BlurSize;

        Varyings vert(Attributes input)
        {
            Varyings output;
            // 3D 좌표를 클립 공간으로 변환
            output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
            // 화면상의 좌표(UV) 계산
            output.screenPos = ComputeScreenPos(output.positionCS);
            return output;
        }

        half4 frag(Varyings input) : SV_Target
        {
            // 화면 좌표 정규화 (w로 나누기)
            float2 uv = input.screenPos.xy / input.screenPos.w;
            
            // 블러 강도 조절 (화면 해상도 대비 간격)
            float invRes = _BlurSize * 0.002; 
            
            // 4방향 샘플링 (SampleSceneColor는 DeclareOpaqueTexture.hlsl에 정의됨)
            half3 col = SampleSceneColor(uv);
            col += SampleSceneColor(uv + float2(invRes, invRes));
            col += SampleSceneColor(uv + float2(-invRes, invRes));
            col += SampleSceneColor(uv + float2(invRes, -invRes));
            col += SampleSceneColor(uv + float2(-invRes, -invRes));
            
            // 평균값 계산 및 알파값 적용 (UI용이므로 1.0)
            return half4(col / 5.0, 1.0);
        }
        ENDHLSL

        Pass
        {
            Name "UIBlurPass"
            ZWrite Off
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }
    }
}