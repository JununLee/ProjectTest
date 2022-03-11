// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/materialSetBuffer"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
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

			struct Pbuffer{
				float4 pos;
			};

			float4 _MainColor;
			StructuredBuffer<Pbuffer> buffer;
			int bufferLength;
	
		
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
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _MainColor;
				for(int n = 0; n < bufferLength; n++){
					if(length(i.worldPos - buffer[n].pos.xyz)<buffer[n].pos.w){
						col = fixed4(1,0,0,1);
					}
				}
				return col;
			}
			ENDCG
		}
	}
}

