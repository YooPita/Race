// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/Pixel Art AA"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
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
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
       
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
       
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uvPixels = i.uv * _MainTex_TexelSize.zw;
 
                float2 uvPixels_fwidth = min(float2(1.0, 1.0), fwidth(uvPixels));
                float2 uvPixels_floor = floor(uvPixels);
                float2 uvPixels_frac = frac(uvPixels);
                // magic ...
                float2 uvPixels_aa = (1.0 - saturate((1.0 - abs(uvPixels_frac * 2.0 - 1.0)) / uvPixels_fwidth)) * sign(uvPixels_frac - 0.5) * 0.5 + 0.5;
                float2 uv = (floor(uvPixels) + uvPixels_aa) * _MainTex_TexelSize.xy;
 
                fixed4 col = tex2Dgrad(_MainTex, uv, ddx(i.uv), ddy(i.uv));
                return col;
            }
            ENDCG
        }
    }
}