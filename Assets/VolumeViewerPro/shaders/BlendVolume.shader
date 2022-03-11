// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///-----------------------------------------------------------------
/// Shader:             BlendVolume
/// Description:        Blends VolumeTex onto MainTex.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
///-----------------------------------------------------------------

Shader "Hidden/VolumeViewer/BlendVolume"
{
	Properties {
		_MainTex ("Source (RGB)", 2D) = "" {}
		_VolumeTex ("Volume (RGB)", 2D) = "" {}	
	}
	SubShader 
	{
		Pass 
		{
			ZWrite Off
			ZTest Always
			Cull Off
			
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				
				struct u2v {
					float4 	pos : POSITION;
					half2 	uv 	: TEXCOORD0;
				};

				struct v2f {
					float4 	pos : SV_POSITION;
					half2 	uvSrc 	: TEXCOORD0;
					half2 	uvDst 	: TEXCOORD1;
				};

				sampler2D _MainTex;
				sampler2D _VolumeTex;
				float4 _MainTex_TexelSize;
				
				v2f vert( u2v v ) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.uvSrc = v.uv;
					o.uvDst = v.uv;
#if UNITY_UV_STARTS_AT_TOP
					if (_MainTex_TexelSize.y < 0)
					{
						o.uvDst.y = 1.0 - o.uvDst.y;
					}
#endif
					return o;
				}
				
				half4 frag(v2f i) : COLOR 
				{		
					half4 src = tex2D(_MainTex, i.uvSrc);
					half4 dst = tex2D(_VolumeTex, i.uvDst);
					return (1.0f - dst.a) * src + dst;
				}
			ENDCG
		}
	} 
	FallBack Off
}