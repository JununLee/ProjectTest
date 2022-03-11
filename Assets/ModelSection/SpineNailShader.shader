// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/SpineNailShader"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
		_TopTex ("TopTexture", 2D) = "white" {}
		_BottomTex ("BottomTexture", 2D) = "white" {}
		//_Critical("Critical",Range(0,0.02)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" "IgnoreProject" = "True" }
		ZWrite off
		//ZTest off
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			float4 _MainColor;
			sampler2D _TopTex;
			float4 _TopTex_ST;
			sampler2D _BottomTex;
			float4 _BottomTex_ST;
			float _Critical;

			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD3;
				float3 worldPos : TEXCOORD4;
				float2 uv : TEXCOORD0;
				float3 localPos : TEXCOORD1;
				float3 objScale:TEXCOORD5;
				float depth : TEXCOORD2;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float3 recipObjScale = float3( length(unity_WorldToObject[0].xyz), length(unity_WorldToObject[1].xyz), length(unity_WorldToObject[2].xyz) );
                float3 objScale = 1.0/recipObjScale;
				o.localPos = v.vertex.xyz * objScale;
				o.objScale = objScale;
				o.uv = TRANSFORM_TEX(v.uv, _TopTex);
				o.depth = UnityObjectToViewPos(v.vertex).z;
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 myuv = float2(i.uv.x,i.localPos.y / i.objScale.x);
				fixed4 col1 = tex2D(_TopTex, myuv);
				fixed4 col2 = tex2D(_BottomTex,myuv);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 tempCol = _MainColor.rgb * saturate(dot(i.worldNormal,viewDir));
				float critical = - 2 - i.depth ;

				if(critical < 0) return fixed4((col1.rgb)*_MainColor.rgb*2,0);
				else if(critical >= 0) return fixed4((col2.rgb)*_MainColor.rgb*2,0);
				else return fixed4(0,0,1,1);

			}
			ENDCG
		}
	}
}
