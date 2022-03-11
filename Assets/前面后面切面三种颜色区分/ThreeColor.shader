Shader "Unlit/ThreeColor"
{
	Properties
	{
		_MainColor("Color",COLOR) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+2"}
		
		ZWrite off
		ZTest off
		blend SrcAlpha OneMinusSrcAlpha 

		Pass
		{
			//Cull Back
			CGPROGRAM
				
				#pragma vertex vert 
				#pragma fragment frag 
				#include "Lighting.cginc"

				float4 _MainColor;

				struct a2v{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f{
					float4 pos : SV_POSITION;
					float3 worldPos : TEXCOORD0;
				};

				v2f vert(a2v v){
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
					return o;
				}

				float4 frag(v2f i) : SV_TARGET0{
					
					if(i.worldPos.z>0){
						return float4(0,1,0,0.2);
					}

					return float4(0,1,0,0.5);
				}

			ENDCG
		}
	}
}
