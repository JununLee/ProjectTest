Shader "Custom/DtiShader"
{
	Properties
	{
		_Color("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 color : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNor : TEXCOORD3;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldNor = normalize(UnityObjectToWorldNormal(v.normal));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = i.color;float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 diffuse = col * saturate(dot(i.worldNor,viewDir) * 0.5 + 0.5);
				return fixed4(diffuse,col.a);
			}
			ENDCG
		}
	}
}

