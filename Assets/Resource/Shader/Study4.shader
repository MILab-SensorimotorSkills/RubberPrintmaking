Shader "Custom/Study4"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white"{}
        _MainTex2("SubTexture", 2D) = "white"{}
        _Speed("속도", Range(0,20)) = 1
        _GG("구김정도", Range(0,20)) = 1
    }
    SubShader
    {   //Opaque : 불투명 => 알파값을 날려버림
        //Transparent 이용
        Tags { "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        // Study Point ==> fullforwardshadews 를 alpha:blend로 변경
        #pragma surface surf Standard alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex; 
        sampler2D _MainTex2;
        float _Speed;
        float _GG;

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
            float4 maintex2 = tex2D(_MainTex2, float2(IN.uv_MainTex2.x, IN.uv_MainTex2.y - _Time.y*_Speed));
            float4 maintex = tex2D(_MainTex, IN.uv_MainTex + maintex2.r*_GG);

            o.Emission = maintex.rgb * maintex2.rgb;
            o.Alpha = maintex.a * maintex2.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
