Shader "Unlit/EffectTest"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RenderTex("RenderTex",2D)="white"{}
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
			sampler2D _RenderTex;
			float4 _RenderTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				fixed4 col1 = tex2D(_RenderTex,i.uv);
				float gray = 0.2989 * col.r + 0.5870 * col.b + 0.1140 * col.b;
				if(col1.r==0&&col1.g==0&&col1.b==0){
					return col;
				}
				return fixed4(gray,gray,gray,1);
			}
			ENDCG
		}
	}
}
