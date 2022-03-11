Shader "Custom/Gaussian"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_BlurLevel("BlurLevel",float) = 3
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
			float4 _MainTex_TexelSize;
			float4 _Color;

			sampler2D _Tex;
			float4 _Tex_TexelSize;
			float _BlurLevel;
			sampler2D _CameraDepthTexture;

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
				o.uv =  TRANSFORM_TEX(v.uv, _MainTex);

				o.uvs[0] = o.uv + _MainTex_TexelSize.xy * float2(-1,-1)*_BlurLevel;
				o.uvs[1] = o.uv + _MainTex_TexelSize.xy * float2(0,-1) *_BlurLevel;
				o.uvs[2] = o.uv + _MainTex_TexelSize.xy * float2(1,-1) *_BlurLevel;
				o.uvs[3] = o.uv + _MainTex_TexelSize.xy * float2(-1,0) *_BlurLevel;
				o.uvs[4] = o.uv + _MainTex_TexelSize.xy * float2(0,0)  *_BlurLevel;
				o.uvs[5] = o.uv + _MainTex_TexelSize.xy * float2(1,0)  *_BlurLevel;
				o.uvs[6] = o.uv + _MainTex_TexelSize.xy * float2(-1,1) *_BlurLevel;
				o.uvs[7] = o.uv + _MainTex_TexelSize.xy * float2(0,1)  *_BlurLevel;
				o.uvs[8] = o.uv + _MainTex_TexelSize.xy * float2(1,1)  *_BlurLevel;

				return o;
			}
			
			fixed4 gaussian(v2f i){
			
				fixed4 c;
				for(int t = 0; t < 9; t++){
					c +=tex2D(_MainTex,i.uvs[t]);
				}

				c = c / 9;
				return c;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = gaussian(i);
				//float d =1- Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv));
				//fixed4 col = tex2D(_Tex,i.uv);
				//col = float4(d,d,d,1);
				return col;
			}
			ENDCG
		}
	}
}

