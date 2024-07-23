Shader "Custom/SandShader"
{
    Properties
    {
        _MainTex ("Color Texture", 2D) = "white" {}
        _RedTex ("Red Texture", 2D) = "white" {} // 변형된 부분에 사용할 텍스처
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            sampler2D _RedTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                if (i.color.r > 0.5) // 빨간색 버텍스를 감지
                {
                    col = tex2D(_RedTex, i.uv); // 변형된 부분에 다른 텍스처 적용
                }
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
