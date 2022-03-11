Shader "Unlit/PunctureTextureShader"
{
	Properties
	{
		_MainColor("Color",COLOR) = (1, 1, 1, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry-2"}
		LOD 100
		ZWrite off
		Pass
		{
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
					float2 uv : TEXCOORD0;
					float2 uv1 : TEXCOORD1;
				};

				v2f vert(a2v v){
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					return o;
				}

				float4 frag(v2f i) : SV_TARGET0{

					return _MainColor;
				}

			ENDCG
		}
	}
}
