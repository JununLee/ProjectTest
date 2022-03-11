Shader "Custom/Image2DShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}

	}
	SubShader
	{
		Tags{"RenderType" = "Opaque"}
		LOD 100
		
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			int _pos_Num;
			float4 _pos[1000];
			float _clamp;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				for(int n = 0; n < _pos_Num; n = n + 1)
				{
					if(distance(i.uv,_pos[n].xy)<=(_clamp+_MainTex_TexelSize.x)||distance(i.uv,_pos[n].zw)<=(_clamp+_MainTex_TexelSize.x)){
						col.g=1;
					}
				}
				if(col.g==1){
					col.gb = col.r / 2;
					col.r *= 2;
				}
				return col;
			}
			ENDCG
		}
	}
}
