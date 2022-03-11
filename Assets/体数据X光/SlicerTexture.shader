// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/SlicerTexture"
{
	Properties
	{
		_Volume ("Texture3D", 3D) = "" {}
	}



	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue" = "Transparent" }
		LOD 100


		Pass
		{
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			#pragma multi_compile _ WHOLE
			#pragma multi_compile __ DR

			sampler3D _Volume;
			float4x4 _world2LocalMatrix;
			vector _forward;
			float maxDataValue;
			float minDataValue;
			float4 pPoint[6];
			float4 pNormal[6];


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
		
		bool effectiveRange(float3 pos)
		{
			pos = mul(pos, _world2LocalMatrix); 
			for(int i = 0; i < 6; i++){
				float3 dir = normalize(pos - pPoint[i].xyz);
				if(dot(dir, pNormal[i].xyz)< 0){
					return false;
				}
			}
			return true;
		}


			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 0);

				float3 lpos = mul(_world2LocalMatrix,float4(i.worldPos,1)).xyz;
#ifdef DR
				float3 ldir = normalize(mul(_world2LocalMatrix,_forward).xyz);
				float3 origin = lpos-ldir;
				float tNear, tFar;
				cubeIntersection(origin, ldir, float3(0.5, 0.5, 0.5), float3(-0.5, -0.5, -0.5), tNear, tFar);
				float currentRayLength = tNear;
				float maxSamples = 1024;
				float currentSample = 0;
				float steplength = 1.732 / maxSamples ;
				[loop]
				while(currentRayLength<tFar)
				{
					float3 pos = origin + ldir * currentRayLength;
		#ifdef WHOLE
					pos += 0.5;
					float4 pcol = tex3Dlod(_Volume, float4(pos, 0));
					if(pcol.r > 0)
					{
						col += (pcol - 0) / (3071 - 0) * (maxSamples - currentSample) / maxSamples * 0.05;
					}
		#else
					if(effectiveRange(pos))
					{
						pos += 0.5;
						float4 pcol = tex3Dlod(_Volume, float4(pos, 0));
						if(pcol.r > 0)
						{
							col += (pcol - 0) / (3071 - 0) * (maxSamples - currentSample) / maxSamples * 0.05;
						}
					}
		#endif
					currentRayLength += steplength;
					currentSample += 1;
				}
#else
		#ifdef WHOLE
				float3 pos = lpos + 0.5;
				if(pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1 && pos.z >= 0 && pos.z <= 1){
					col = tex3Dlod(_Volume, float4(pos, 0));
					col = (col - minDataValue) / (maxDataValue - minDataValue);
				}
		#else
				if(effectiveRange(lpos))
				{
					float3 pos = lpos + 0.5;
					if(pos.x >= 0 && pos.x <= 1 && pos.y >= 0 && pos.y <= 1 && pos.z >= 0 && pos.z <= 1){
						col = tex3Dlod(_Volume, float4(pos, 0));
						col = (col - minDataValue) / (maxDataValue - minDataValue);
					}
				}
				else
				{
					col = float4(0,0,0,0);
				}
		#endif
#endif
				return float4(col.rrr,1);
			}
			ENDCG
		}
	}
}
