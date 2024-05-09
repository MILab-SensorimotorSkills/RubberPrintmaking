Shader "Custom/URPCustomHeightShader2"
{
    Properties
    {
        _MainColor("Main Color", Color) = (1,1,1,1)
        _AltTex("Alternative Texture", 2D) = "white" {}
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
                float worldYPosition : TEXCOORD0; // 월드 Y 좌표
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                float4 worldPosition = float4(TransformObjectToWorld(v.vertex.xyz), 1.0);
                o.worldYPosition = worldPosition.y;
                return o;
            }

            float4 _MainColor;
            sampler2D _AltTex;
            float _HeightThreshold;

            half4 frag(VertexOutput i) : SV_Target
            {
                half4 color = _MainColor; // 기본 컬러 설정
                if (i.worldYPosition < _HeightThreshold) // 월드 Y 좌표 기준으로 비교
                {
                    float2 uv = float2(i.worldYPosition, 0); // 컬러를 텍스처처럼 사용하기 위해 임의의 UV 좌표 생성
                    color = tex2D(_AltTex, uv); // 텍스처 사용
                }
                return color;
            }
            ENDHLSL
        }
    }
}
