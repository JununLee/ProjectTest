Shader "Custom/MeshCulling"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_FrontDepth ("Texture", 2D) = "white" {}
		_BackDepth ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "Queue" = "Geometry-1"}
		LOD 100
		cull Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _FrontDepth;
			float4 _FrontDepth_ST;
			sampler2D _BackDepth;
			float4 _BackDepth_ST;
			float4 _MainColor;
			sampler2D _CameraDepthTexture;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float4 screenUV : TEXCOORD1;
				float depth : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				float3 worldNor : TEXCOORD4;
				float eyeZ:TEXCOORD5;
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.screenUV = ComputeScreenPos(o.vertex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.worldNor = normalize(UnityObjectToWorldNormal(v.normal));
				COMPUTE_EYEDEPTH(o.eyeZ);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float depth = Linear01Depth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screenUV)));
				float frontDep = tex2Dproj(_FrontDepth, (i.screenUV)).r;
				float backDep = tex2Dproj(_BackDepth, (i.screenUV)).r;
				//return float4((i.eyeZ / frontDep),0,0,1);
				if(-1<0){
					discard;
				}
				fixed4 col = tex2D(_MainTex, i.uv);
				float3 viewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				float3 diffuse = _MainColor * saturate(dot(i.worldNor,viewDir) * 0.5 + 0.5);
				return fixed4(diffuse,_MainColor.a);
			}
			ENDCG
		}
	}
}

