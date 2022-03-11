// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/water"
{
	Properties
	{
		_MainColor("MainColor",COLOR) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Bumpmap("NormalMap",2D) = "bump"{}
		_BumpScale("BumpScale",float) = 1
		_Specular("Specular",COLOR) = (1,1,1,1)
		_Gloss("Gloss",Range(8,256)) = 20
		_A("A",float) = 1
		_W("W",float) = 1
		_V("V",float) = 0
		_K("K",float) = 0
		_D("D",vector) = (1,0,0)
	}
	SubShader
	{ 
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" }
		LOD 100
		//Cull off
		blend SrcAlpha OneMinusSrcAlpha 
		Pass
		{
			Tags{"LightMode" = "ForwardBase"}
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainColor;
			sampler2D _Bumpmap;
			float4 _Bumpmap_ST;
			float _BumpScale;
			float4 _Specular;
			float _Gloss;
			float _A;
			float _W;
			float _V;
			float _K;
			float3 _D;
			struct a2v
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float4 pos : TEXCOORD2;
				float3 lightDir : TEXCOORD3;
				float3 viewDir : TEXCOORD4;
			
			};
			
			v2f vert (a2v v)
			{
				v2f o;
				v.vertex.y = _A*sin(_W * (v.vertex.x * _D.x + v.vertex.z * _D.y) + _V * _Time.y) + _K;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldPos = mul(unity_ObjectToWorld,v.vertex);
				o.pos = v.vertex;

				TANGENT_SPACE_ROTATION;
				o.lightDir = mul(rotation,ObjSpaceLightDir(v.vertex)).xyz;
				o.viewDir = mul(rotation,ObjSpaceViewDir(v.vertex)).xyz;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);//float2(i.pos.y/_A,i.uv.y));
				float3 lightDir = normalize(i.lightDir);
				float3 viewDir = normalize(i.viewDir);
				float3 bump = UnpackNormal(tex2D(_Bumpmap,i.uv));
				bump.xy *=_BumpScale;
				bump.z = sqrt(1 - saturate(dot(bump.xy,bump.xy)));
				bump = normalize(bump);

				float3 diffuse = saturate(dot(lightDir,bump)*0.5 + 0.5)*col.xyz;
				float3 halfDir = normalize(lightDir + viewDir);
				float3 specular = _Specular.xyz * pow(max(0,dot(bump,halfDir)),_Gloss);
				return float4(diffuse + specular,1);
			}
			ENDCG
		}
	}
}

