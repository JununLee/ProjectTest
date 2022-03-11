// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/CylineShader"
{
	Properties
	{
		_Color("Main Color",COLOR) = (1,1,1,1)
		_LeftPos("LeftPos",vector) = (0,0,0,1)
		_RightPos("RightPos",vector) = (0,0,0,1)
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

			float4 _Color;
			vector _LeftPos;
			vector _RightPos;

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 ViewPos : TEXCOORD0;
				float2 left : TEXCOORD1;
				float2 right : TEXCOORD2;
			};

			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.ViewPos = mul(UNITY_MATRIX_MV,v.vertex);
				o.left = mul(UNITY_MATRIX_V,float4(_LeftPos.xyz,1)).xy;
				o.right = mul(UNITY_MATRIX_V,float4(_RightPos.xyz,1)).xy;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if(i.ViewPos.x > i.right.x||i.ViewPos.x < i.left.x){
					clip(-1);
				}
				if(i.ViewPos.y > i.right.y||i.ViewPos.y < i.left.y){
					clip(-1);
				}
				return _Color;
			}
			ENDCG
		}
	}
}
