Shader "Custom/SpotLight"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_X("X",Range(-0.5,0.5)) = 0
		_Y("Y",Range(-0.5,0.5)) = 0
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
			float _X;
			float _Y;

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
				float2 p = ((i.uv - float2(0.5, 0.5)) * _ScreenParams.xy)/min(_ScreenParams.x,_ScreenParams.y);
				col = lerp(float4(0,0,0,1),col, saturate( 5*smoothstep(-0.03, 0.2, 0.2 + 0.02 * sin(_Time.w) - distance(p,float2(_X*16/9,_Y)))));
				return col;
			}
			ENDCG
		}
	}
}

