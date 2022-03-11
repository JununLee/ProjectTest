Shader "Custom/CylinderShader1"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Mask("Mask",float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
		LOD 100
		//ZTest off
		Pass
		{
			Stencil{
				Ref[_Mask]
				Comp Always
				Pass Replace
			}
			ColorMask 0
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 viewPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				if(-i.viewPos.z<5){
					discard;
				}
				return float4(1,0,0,1);
			}
			ENDCG
		}
		Pass
		{
			Stencil{
				Ref 1
				Comp NotEqual
			}
			Cull front
			//ZTest Less
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 viewPos : TEXCOORD1;
			};

			v2f vert(a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.viewPos = UnityObjectToViewPos(v.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				/*if (-i.viewPos.z<5) {
					discard;
				}*/
				return float4(1,0,0,1);
			}
			ENDCG
		}
	}
}

