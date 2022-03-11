Shader "Custom/backDepth"
{
		SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		//Cull front
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float depth : TEXCOORD1;
			};


			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.depth =-(mul(UNITY_MATRIX_MV, v.vertex).z);

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(i.depth,0,0,1);
			}
			ENDCG
		}
	}
		FallBack Off
}

