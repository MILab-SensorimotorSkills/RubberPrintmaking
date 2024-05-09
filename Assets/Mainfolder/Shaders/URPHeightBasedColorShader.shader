Shader "Custom/URPHeightBasedColorShaderWorld"
{
    Properties
    {
        _ColorAboveThreshold("Color Above Threshold", Color) = (0,0,1,1) // 파란색
        _ColorBelowThreshold("Color Below Threshold", Color) = (1,0,0,1) // 빨간색
        _HeightThreshold("Height Threshold", Float) = 0.5
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
            float4 _ColorAboveThreshold;
            float4 _ColorBelowThreshold;

            half4 frag(VertexOutput i) : SV_Target
            {
                return i.worldYPosition >= _HeightThreshold ? _ColorAboveThreshold : _ColorBelowThreshold;
            }
            ENDHLSL
        }
    }
}
