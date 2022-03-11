// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

///-----------------------------------------------------------------
/// Shader:             CullingDepthFront
/// Description:        Renders the depth of the front of objects
///							with the CullingMaterial on them. 
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

Shader "Hidden/VolumeViewer/CullingDepthFront"
{
	SubShader
	{
		Tags{ "VolumeTag" = "Culling" }

		Pass
		{
			Cull Back
			//ZTest LEqual

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				struct u2v {
					float4 pos : POSITION;
				};

				struct v2f {
					float4 pos : SV_POSITION;
					float  depth : TEXCOORD0;
				};

				v2f vert(u2v v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.pos);
					o.depth = -(mul(UNITY_MATRIX_MV, v.pos).z * _ProjectionParams.w);
					return o;
				}

				half4 frag(v2f i) : COLOR
				{
					return i.depth;
				}
			ENDCG
		}
	}
	FallBack Off
}