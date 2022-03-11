Shader "Custom/QuadShader"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _frontDepthTex;
			float4 _frontDepthTex_TexelSize;
			sampler2D _backDepthTex;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float depth : TEXCOORD1;
				float4 screenUV : TEXCOORD2;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				//v.vertex.z+=1;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//o.vertex.z +=0.001;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.depth = mul(UNITY_MATRIX_MV,v.vertex).z;
				float4 suv = ComputeScreenPos(o.vertex);
				o.screenUV= suv;
				//o.screenUV[0] = suv + _frontDepthTex_TexelSize * float2(-1,-1);
				//o.screenUV[1] = suv + _frontDepthTex_TexelSize * float2(0,-1);
				//o.screenUV[2] = suv + _frontDepthTex_TexelSize * float2(1,-1);
				//o.screenUV[3] = suv + _frontDepthTex_TexelSize * float2(-1,0);
				//o.screenUV[4] = suv + _frontDepthTex_TexelSize * float2(0,0);
				//o.screenUV[5] = suv + _frontDepthTex_TexelSize * float2(1,0);
				//o.screenUV[6] = suv + _frontDepthTex_TexelSize * float2(-1,1);
				//o.screenUV[7] = suv + _frontDepthTex_TexelSize * float2(0,1);
				//o.screenUV[8] = suv + _frontDepthTex_TexelSize * float2(1,1);


				return o;
			}
			
			float Sobel(v2f i){
				const float Gx[9] = {-1,-1,-1,0,0,0,1,1,1};
				const float Gy[9] = {-1,0,1,-1,0,1,-1,0,1};
				float texColor;
				float edgeX = 0;
				float edgeY = 0;
				for(int t = 0;t < 9;t++){
					//texColor = 
				}
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//float front = tex2Dproj(_frontDepthTex, i.screenUV).r;
				//float back = tex2Dproj(_backDepthTex, i.screenUV).r;
				//if(abs(i.depth-front)<0.01 ||abs(i.depth - back)<0.01 ){
				//	col = float4(1,0,0,1);
				//}
				return col;
			}
			ENDCG
		}
	}
}

