Shader "Hidden/FOV/StencilDarkOverlay"

{

Properties

{

_Darkness ("Darkness", Range(0, 1)) = 0.8

}



SubShader

{

Tags

{

"RenderPipeline" = "UniversalPipeline"

"RenderType" = "Opaque"

}



Pass

{

Name "StencilDarkOverlay"

ZTest Always

ZWrite Off

Cull Off

Blend One Zero



Stencil

{

Ref 2

Comp NotEqual

Pass Keep

}



HLSLPROGRAM

#pragma vertex Vert

#pragma fragment Frag



#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"



SAMPLER(sampler_BlitTexture);



float _Darkness;



float4 Frag(Varyings input) : SV_Target

{

UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);



float2 uv = UnityStereoTransformScreenSpaceTex(input.texcoord);

float4 color = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

color.rgb *= 1.0 - saturate(_Darkness);

return color;

}

ENDHLSL

}

}

}