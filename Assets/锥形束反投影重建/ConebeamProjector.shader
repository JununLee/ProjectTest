Shader "Unlit/ConebeamProjector"
{
	Properties
	{
		_VolumeTex("VolumeTex",3D) = ""{}
		_Slider("Slider",Range(0,100)) = 50
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100
		Cull off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 norml : NORMAL;
			};

			struct v2f
			{
				float3 uv : TEXCOORD0; 
				float4 vertex : SV_POSITION;
				float3 worldNorml : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};

			sampler3D _VolumeTex;
			float4x4 _cubeWorld2Local;
			float _Slider;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = float3(v.uv,0.5); 
				o.worldNorml = mul(unity_ObjectToWorld,float4(0,0,1,0)).xyz;
				o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;
				return o;
			}

			fixed cubeIntersection(float3 origin, float3 direction, float3 aabbMax, float3 aabbMin, out float tNear, out float tFar)
			{
				float3 invDir = 1.0 / direction;
				float3 t1 = invDir * (aabbMax - origin);
				float3 t2 = invDir * (aabbMin - origin);
				float3 tMin = min(t1, t2);
				float3 tMax = max(t1, t2);
				tNear = max(max(tMin.x, tMin.y), tMin.z);
				tFar = min(min(tMax.x, tMax.y), tMax.z);
				return tNear <= tFar;
			}

			float4 frag (v2f i) : SV_Target
			{ 
				float val = 0;
				float3 origin = _WorldSpaceCameraPos.xyz;
				float3 dir = normalize(i.worldPos - origin);
				float3 lorigin = mul(_cubeWorld2Local,float4(origin,1)).xyz;
				float3 ldir = normalize(mul(_cubeWorld2Local,dir).xyz); 
				float tNear, tFar;
				cubeIntersection(lorigin, ldir, float3(0.5, 0.5, 0.5), float3(-0.5, -0.5, -0.5), tNear, tFar);
				float currentRayLength = tNear;
				float maxSamples = 1024;
				float currentSample = 0;
				float steplength = 1.732 / maxSamples ;

				[loop]
				while(currentRayLength<tFar)
				{
					float3 pos = lorigin + ldir * currentRayLength;
					if(abs(pos.x)<0.5&&abs(pos.y)<0.5&&abs(pos.z)<0.5){
						pos =pos + 0.5;
						float4 c = tex3Dlod(_VolumeTex, float4(pos.x,pos.y,pos.z, 0));
						val += c.a/256;  
					}
					currentSample +=1;
					currentRayLength += steplength;
				} 

				float4 cc = tex3Dlod(_VolumeTex, float4(i.uv.x,i.uv.y,_Slider/100, 0));
				val = cc.a > 0 ? cc.a : 0;  

				float4 col = float4(val,val,val,1);
				return col;
			}
			ENDCG
		}

	}
}
