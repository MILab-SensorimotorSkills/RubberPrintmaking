Shader "Custom/Study2"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white"{}
        _U("x축 타일링", float) = 1
        _V("y축 타일링", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        float _U;
        float _V;

        struct Input
        {
            float2 uv_MainTex;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        //유니티 내에 내장되어 있는 _Time 메소드를 통한 텍스쳐를 이미지화 하는 방법.
        //float2 (IN.uv_MainTex.y, IN.uv_MainTex.y + _Time.y) ==  IN.uv_MainTex + float(0, _Time.y)
            //float4 MainTex = tex2D(_MainTex, IN.uv_MainTex + float2(0, _Time.y));
            //float4 MainTex = tex2D(_MainTex, float2(IN.uv_MainTex.x + _Time.y, IN.uv_MainTex.y));
            //float4 MainTex = tex2D(_MainTex, float2(IN.uv_MainTex + _Time.y));
            //float4 MainTex = tex2D(_MainTex, float2(IN.uv_MainTex+_Time.x));
            //float4 MainTex = tex2D(_MainTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y)*float2(_U,_V));
        void surf (Input IN, inout SurfaceOutputStandard o)
        {   
            float4 MainTex = tex2D(_MainTex, float2(IN.uv_MainTex.x, IN.uv_MainTex.y + _Time.y));
            
            //흑백처리
            o.Emission = (MainTex.r + MainTex.g + MainTex.b)/3;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
