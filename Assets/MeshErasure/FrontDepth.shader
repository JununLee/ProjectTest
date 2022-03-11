Shader "Custom/FrontDepth"
{
		SubShader
	{
		cull back

		Tags { "RenderType"="Opaque" }

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
				float depth : TEXCOORD1;
			};

			//sampler2D _CameraDepthTexture;
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.depth = mul(UNITY_MATRIX_MV,v.vertex).z;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(i.depth,0,0,i.depth);
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}

