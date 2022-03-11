// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/Sphere"
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
			vector forward;
			vector right;
			vector up;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			float sdfSphere(float3 origin, float3 dir, out float T0, out float T1){
			
				
				float3 O = float3(0,0,0);
				float R = 0.5;
				float L = length(O-origin);
				float Tca = L * dot(normalize(O - origin), normalize(dir));
				float d = L * L - Tca * Tca;
			    float Thc = sqrt(R * R - d);
				T0 = Tca - Thc;
				T1 = Tca + Thc;

				return  (sqrt(d) - R);
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 origin = _WorldSpaceCameraPos;

				float3 dir = forward + right * (i.uv.x - 0.5)  + up * (i.uv.y - 0.5);
				float T0, T1;
				col.rgb = lerp(col.rgb,float3(1,0,0),smoothstep(-0.01,0.01,-sdfSphere(origin,dir,T0,T1)));

				//col.rgb = dir;
				return col;
			}
			ENDCG
		}
	}
}

