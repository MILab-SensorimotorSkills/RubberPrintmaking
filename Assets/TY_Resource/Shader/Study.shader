Shader "Custom/Study"
{
    Properties
    {
        _R("R", Range(0,1)) = 0.5
        _G("G", Range(0,1)) = 0.5
        _B("B", Range(0,1)) = 0.5
        _Bright("밝기", Range(-1,1)) = 0
        _MainTex("Texture", 2D) = "white"{}
        _MainTex2("SubTexture", 2D) = "white"{}
        _GS("Lerp", Range(0,1)) = 0.5
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
        sampler2D _MainTex2;
        float _R;
        float _G;
        float _B;
        float _Bright;
        float _GS;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_MainTex2;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            //Texture uses float4, but albedo and emission use float3
            float4 MainTex = tex2D(_MainTex, IN.uv_MainTex);
            float4 MainTex2 = tex2D(_MainTex2, IN.uv_MainTex2);
            //흑백 이미지
            //o.Emission = (MainTex.r + MainTex.g + MainTex.b)/3;
            o.Emission = lerp(MainTex, MainTex2, _GS);
            //o.Emission = float3(_R, _G, _B)+_Bright;
            //o.Albedo = float3(_R, _G, _B);



            // // Albedo comes from a texture tinted by color
            // fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            // o.Albedo = c.rgb;
            // // Metallic and smoothness come from slider variables
            // o.Metallic = _Metallic;
            // o.Smoothness = _Glossiness;
            // o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
