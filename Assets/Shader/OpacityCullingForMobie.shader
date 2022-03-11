// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/OpacityCullingForMobie"
{
	Properties
	{
		_Color ("MainColor", color) = (1,1,1,1)
		_OutColor("OutColor",color) = (1,1,1,1)
		_OutLine("OutLineGross",Range(0,1)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"}
		LOD 100

		Pass
		{

			Tags {"LightMode"="ForwardBase"}
			Cull off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc" // for UnityObjectToWorldNormal
		
            fixed4 _Color;
			fixed4 _OutColor;
			float _OutLine;
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 localPos : TEXCOORD2;
            };

			v2f vert (appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
                o.localPos = v.vertex + 1000;
                return o;
            }

            fixed4 frag (v2f i,float facing : VFACE) : SV_Target
            {
				if(facing < 0){
				
					float _DiscardSize = _Color.a * 5;

					float x = i.localPos.z * 100 % 5;
					float y = i.localPos.y * 100 % 5;
					if(x > _DiscardSize)	
					{
               			discard;
					}
					fixed3 worldNormal = normalize(i.worldNormal);
					fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
					fixed3 diffuse = _Color.rgb * (saturate(dot(worldNormal,viewDir)*0.5+0.5));
					fixed3 outcolor  = _OutColor.rgb * saturate(1-dot(viewDir,worldNormal)) * _OutLine;
					fixed3 finalColor =diffuse + outcolor;

					return fixed4(finalColor,1);
				}
				else{
					fixed3 worldNormal = normalize(i.worldNormal);
					fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
					fixed3 diffuse = _Color.rgb * (saturate(dot(-worldNormal,viewDir)*0.5+0.5));
					fixed3 outcolor  = _OutColor.rgb * saturate(1-dot(viewDir,-worldNormal)) * _OutLine;
					fixed3 finalColor =diffuse + outcolor;

					return fixed4(finalColor,1);
				
				}
            }
			ENDCG
		}
	}
}
