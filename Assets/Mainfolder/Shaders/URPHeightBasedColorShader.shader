Shader "Custom/URPHeightBasedColorShaderWorld"
{
    Properties
    {
        _ColorAboveThreshold("Color Above Threshold", Color) = (0.12156863, 0.1254902, 0.14509804, 1) // 연한 회색
        _ColorBelowThreshold("Color Below Threshold", Color) = (0.24705882, 0.24705882, 0.24705882, 1) // 짙은 회색
        _HeightThreshold("Height Threshold", Float) = 0.5
        _TransitionHeight("Transition Height", Float) = 0.1
        _MainTex("Main Texture", 2D) = "white" {} // 텍스처 추가
        _BaseColor("Base Color", Color) = (1, 1, 1, 1) // 기본 색상 추가
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
                float2 uv : TEXCOORD0; // UV 좌표 추가
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float worldYPosition : TEXCOORD0;
                float2 uv : TEXCOORD1; // UV 좌표 추가
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                float4 worldPosition = mul(UNITY_MATRIX_M, v.vertex);
                o.worldYPosition = worldPosition.y;
                o.uv = v.uv; // UV 좌표 전달
                return o;
            }

            float _HeightThreshold;
            float _TransitionHeight;
            float4 _ColorAboveThreshold;
            float4 _ColorBelowThreshold;
            sampler2D _MainTex; // 텍스처 샘플러 추가
            float4 _BaseColor; // 기본 색상 추가

            half4 frag(VertexOutput i) : SV_Target
            {
                // 높이 기반 색상 계산
                float t = saturate((i.worldYPosition - (_HeightThreshold - _TransitionHeight / 2)) / _TransitionHeight);
                float4 color = lerp(_ColorBelowThreshold, _ColorAboveThreshold, t);

                // 텍스처 적용 범위 결정
                float textureBlendFactor = step(_HeightThreshold, i.worldYPosition);
                float4 texColor = tex2D(_MainTex, i.uv);

                // 텍스처 색상과 기본 색상을 곱하여 채도 낮춤
                texColor *= _BaseColor;

                // 텍스처와 색상 혼합
                return lerp(color, texColor, textureBlendFactor);
            }
            ENDHLSL
        }
    }
}
