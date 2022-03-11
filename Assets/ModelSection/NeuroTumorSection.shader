Shader "Custom/NeuroTumorSection"
{
		Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader{
		Tags{"Queue" = "Transparent+20" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
		Pass{
			Cull Front
			ZWrite off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
				
			#pragma vertex vert 
			#pragma fragment frag 
			#include "Lighting.cginc"
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
				if(depth<=0){
					discard;
				}
				return fixed4(_MainColor.rgb,0.5);
			}
			ENDCG
		}
	}
}
