Shader "Custom/UIOutline_Emission"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (Pixels)", Range(0, 50)) = 5
        
        [Header(Emission Settings)]
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Range(0, 10)) = 2.0
        [Toggle(USE_PULSE)] _UsePulse ("Use Pulse Effect", Float) = 0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ USE_PULSE

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineWidth;
            
            fixed4 _EmissionColor;
            float _EmissionIntensity;
            float _PulseSpeed;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(o.worldPosition);
                o.texcoord = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float originalAlpha = tex2D(_MainTex, IN.texcoord).a;
                
                // 1. 가로세로 비율 보정
                float aspect = _MainTex_TexelSize.y / _MainTex_TexelSize.x;
                float2 unit = _MainTex_TexelSize.xy * _OutlineWidth;
                if (aspect > 1.0) unit.x /= aspect;
                else if (aspect < 1.0) unit.y *= aspect;

                // 2. 8방향 샘플링
                float maxAlpha = originalAlpha;
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord + float2(0, unit.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord - float2(0, unit.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord + float2(unit.x, 0)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord - float2(unit.x, 0)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord + float2(unit.x, unit.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord - float2(unit.x, unit.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord + float2(unit.x, -unit.y)).a);
                maxAlpha = max(maxAlpha, tex2D(_MainTex, IN.texcoord - float2(unit.x, -unit.y)).a);

                // 3. 테두리 영역 추출 (본체 제거)
                float borderMask = saturate(maxAlpha - originalAlpha);
                
                // 4. 이미션 및 이미션 강도 계산
                fixed4 finalColor = _OutlineColor;
                float intensity = _EmissionIntensity;

                #ifdef USE_PULSE
                    // 시간에 따른 깜빡임 효과 (0.5 ~ 1.5 사이 왕복)
                    intensity *= (sin(_Time.y * _PulseSpeed) * 0.5 + 1.0);
                #endif

                finalColor.rgb += _EmissionColor.rgb * intensity;
                finalColor.a *= borderMask;

                return finalColor;
            }
            ENDCG
        }
    }
}