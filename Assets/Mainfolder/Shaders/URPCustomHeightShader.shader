Shader "Custom/URPCustomHeightShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
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
                float2 uv : TEXCOORD0;
            };

            struct VertexOutput
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float worldYPosition : TEXCOORD1; // 월드 Y 좌표
            };

            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;
                o.pos = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                float4 worldPosition = float4(TransformObjectToWorld(v.vertex.xyz), 1.0); // 명시적으로 float4 형변환
                o.worldYPosition = worldPosition.y;
                return o;
            }
            

            sampler2D _MainTex;
            sampler2D _AltTex;
            float _HeightThreshold;
            float4 _MainTex_ST;

            half4 frag(VertexOutput i) : SV_Target
            {
                float2 uv = TRANSFORM_TEX(i.uv, _MainTex);
                half4 color = tex2D(_MainTex, uv);
                if (i.worldYPosition < _HeightThreshold) // 월드 Y 좌표 기준으로 비교
                {
                    color = tex2D(_AltTex, uv);
                }
                return color;
            }
            ENDHLSL
        }
    }
}
