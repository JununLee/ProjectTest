// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/OpaqueBlendAlpha"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry+100"}
		LOD 100

		GrabPass{
		
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _MainColor;
			sampler2D _GrabTexture;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 grabUV : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 worldNor : TEXCOORD2;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.grabUV = ComputeGrabScreenPos(o.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldNor = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 worldNormal = normalize(i.worldNor);
				float3 diffuse = _MainColor * saturate(dot(viewDir,worldNormal) * 0.5 + 0.5);
				fixed3 col = lerp(tex2D(_GrabTexture, i.grabUV.xy/i.grabUV.w), diffuse, _MainColor.a);
				return fixed4(col,1);
			}
			ENDCG
		}
	}
}

