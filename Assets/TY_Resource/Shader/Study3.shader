Shader "Custom/Study3"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white"{}
        _MainTex2("SubTexture", 2D) = "white"{}
        _FS("Speed", float) = 1
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
        float _FS;

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
            float2 fs = (0, _Time.x * _FS);
            float4 MainTex2 = tex2D(_MainTex2, IN.uv_MainTex2 + fs);
            float4 MainTex = tex2D(_MainTex, IN.uv_MainTex + MainTex2.r);

            //Emission이던 Albedo이던 꼭 해줘야 한다. 
            o.Emission = MainTex;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
