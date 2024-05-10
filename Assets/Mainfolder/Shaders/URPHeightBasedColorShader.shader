Shader "Custom/URPHeightBasedColorShaderWorld"
{
    Properties
    {
        _ColorAboveThreshold("Color Above Threshold", Color) = (0.12156863, 0.1254902, 0.14509804, 1) // 연한 회색
        _ColorBelowThreshold("Color Below Threshold", Color) = (0.24705882, 0.24705882, 0.24705882, 1) // 짙은 회색
        _HeightThreshold("Height Threshold", Float) = 0.5
        _TransitionHeight("Transition Height", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct VertexInput
            {
                float4 vertex : POSITION;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float worldYPosition : TEXCOORD0;
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                float4 worldPosition = mul(UNITY_MATRIX_M, v.vertex);
                o.worldYPosition = worldPosition.y;
                return o;
            }

            float _HeightThreshold;
            float _TransitionHeight;
            float4 _ColorAboveThreshold;
            float4 _ColorBelowThreshold;

            half4 frag(VertexOutput i) : SV_Target
            {
                float t = saturate((i.worldYPosition - (_HeightThreshold - _TransitionHeight / 2)) / _TransitionHeight);
                return lerp(_ColorBelowThreshold, _ColorAboveThreshold, t);
            }
            ENDHLSL
        }
    }
}
