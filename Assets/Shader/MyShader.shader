// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/MyShader"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
	}
	SubShader
	{
		
		LOD 100
				Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _MainColor;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos :TEXCOORD1;
				float3 worldNor : TEXCOORD2;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos=mul(unity_ObjectToWorld,v.vertex);
				o.worldNor = UnityObjectToWorldNormal(v.normal);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				//fixed4 col = tex2D(_MainTex, i.uv);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 nor = normalize(i.worldNor);
				float3 diffuse = _MainColor.rgb*saturate(dot(viewDir,nor)*0.5+0.5);
				return fixed4(diffuse,_MainColor.a);
			}
			ENDCG
		}
	}
}

