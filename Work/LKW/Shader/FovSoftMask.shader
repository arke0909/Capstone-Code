Shader "Custom/FOV/FovSoftMask"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Power ("Blur Power", Range(0.5, 5.0)) = 2.0 // 경계가 사라지는 곡선 조절
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // 부드러운 합성을 위해 필수
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _Color;
            float _Power;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // i.uv.x는 중심부에서 1, 외곽 끝에서 0입니다.
                // pow를 사용하면 그라데이션의 부드러움을 조절할 수 있습니다.
                float alpha = pow(i.uv.x, _Power);
                
                fixed4 col = _Color;
                col.a *= alpha; // UV 값에 따라 투명도 결정
                return col;
            }
            ENDCG
        }
    }
}