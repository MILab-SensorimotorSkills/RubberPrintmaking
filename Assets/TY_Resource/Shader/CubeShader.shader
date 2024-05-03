Shader "Custom/CubeShader"
{
    Properties
    {
        _CubeMap("CubeMap", cube) = "" {}
        _PaintTex("Painted Texture", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        // 셰이더에 필요한 텍스쳐 변수 선언
        samplerCUBE _CubeMap;   // CubeMap 텍스쳐 변수
        sampler2D _PaintTex;    // 그려진 텍스쳐 변수

        // 입력 구조체 정의
        struct Input
        {
            float3 worldNormal; // 월드 공간의 표면 노멀 벡터
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // 픽셀당 painted와 cubeColor 가져오기
            fixed4 painted = tex2D(_PaintTex, float2(0,0)); // 픽셀당 painted 값 가져오기
            fixed4 cubeColor = texCUBE(_CubeMap, IN.worldNormal); // 픽셀당 CubeMap에서 색상 가져오기

            // Alpha 값을 사용하여 색상 보정 조절
            fixed3 lerpColor = lerp(cubeColor.rgb, painted.rgb, painted.a);

            // 최종 색상 할당
            o.Emission = lerpColor; // Emission 색상에 보정된 색상 할당
            o.Alpha = 1.0; // 알파 값 1로 설정
        }
        ENDCG
    }
    FallBack "Diffuse"
}
