Shader "Unlit/VolumeSliceShader"
{
	Properties
	{
		_Volume("Volume",3D) = ""{}
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

			sampler3D _Volume;
			float4x4 _volumeWorld2LocalMatrix;
			float4x4 _planeLocal2Worldmatrix;

			struct a2v
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 worldPos : TEXCOORD1;
			};

			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld , v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 pos = mul(_volumeWorld2LocalMatrix,i.worldPos);

				pos = pos + 0.5;

				float4 data = float4(0,0,0,0);

				if (pos.x > 1 || pos.x < 0 || pos.y>1 || pos.y < 0 || pos.z>1 || pos.z < 0)
				{
					data = float4(0,0,0,0);
				}
				else
				{
					data = tex3Dlod(_Volume, float4(pos.x,pos.y,pos.z,0));
				}


				return data;
			}
			ENDCG
		}
	}
}
