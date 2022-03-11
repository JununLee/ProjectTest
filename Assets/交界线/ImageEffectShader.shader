Shader "Custom/ImageEffectShader"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_LineColor("LineColor",COLOR) = (1,1,1,1)
		_Width("LineWidth",float) = 1
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _buffTex;
			float4 _buffTex_TexelSize;
			float4 _LineColor;
			float _Width;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uvs[9] : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				o.uvs[0] = o.uv + _buffTex_TexelSize * float2(-1,-1) * _Width;
				o.uvs[1] = o.uv + _buffTex_TexelSize * float2(0,-1)  * _Width;
				o.uvs[2] = o.uv + _buffTex_TexelSize * float2(1,-1)  * _Width;
				o.uvs[3] = o.uv + _buffTex_TexelSize * float2(-1,0)  * _Width;
				o.uvs[4] = o.uv + _buffTex_TexelSize * float2(0,0)   * _Width;
				o.uvs[5] = o.uv + _buffTex_TexelSize * float2(1,0)   * _Width;
				o.uvs[6] = o.uv + _buffTex_TexelSize * float2(-1,1)  * _Width;
				o.uvs[7] = o.uv + _buffTex_TexelSize * float2(0,1)   * _Width;
				o.uvs[8] = o.uv + _buffTex_TexelSize * float2(1,1)   * _Width;

				return o;
			}
			
			float Sobel(v2f i){
				const float Gx[9] = {-1,-1,-1,0,0,0,1,1,1};
				const float Gy[9] = {-1,0,1,-1,0,1,-1,0,1};
				float texColor;
				float edgeX;
				float edgeY;
				for(int t = 0;t < 9;t++){
					texColor = tex2D(_buffTex,i.uvs[t]).r;
					edgeX +=texColor * Gx[t];
					edgeY +=texColor * Gy[t];
				}
				float edge = saturate(1 - abs(edgeX) - abs(edgeY));
				return edge;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//fixed4 lcol = tex2D(_buffTex, i.uv);
				float edge = Sobel(i);
				col = lerp(_LineColor,col,edge);
				//col += lcol;
				return col;
			}
			ENDCG
		}
	}
}

