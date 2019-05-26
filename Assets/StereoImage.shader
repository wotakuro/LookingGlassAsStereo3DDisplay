﻿Shader "Unlit/StereoImage"
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
            // make fog work
            #pragma multi_compile_fog

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
				float4 color : COLOR;
            };

            sampler2D _MainTex;
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
                if( i.color.g < 0.5){
                    return fixed4(0,0,0,1);
                }
				float2 left = i.uv * float2(0.5,1.0);
				float2 right = left + float2(0.5, 0.0);
				// sample the texture
				fixed4 col = tex2D(_MainTex, left);
				fixed4 col2 = tex2D(_MainTex, right);

                return (col * i.color.r) + (col2 * (1.0 - i.color.r) );
            }
            ENDCG
        }
    }
}
