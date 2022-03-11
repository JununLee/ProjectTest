Shader "Custom/NeuroTumorSectionFront"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader
	{
		LOD 100
		Tags { "RenderType"="Opaque" "Queue" = "Geometry-1" }
		Pass
		{
			ColorMask 0
			Cull Back
			ZWrite On
			CGPROGRAM	
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _MainColor;

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = UnityObjectToViewPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_TARGET0
			{
				float depth = -2 - i.worldPos.z;
				clip(depth);
				return (_MainColor.rgb,1);
			}
			ENDCG
		}
		}
}
