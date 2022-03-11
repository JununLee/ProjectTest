///-----------------------------------------------------------------
/// Shader:             CullingPlaceholder
/// Description:        Materials with this shader will cause
///							objects to cull their intersections
///							with VolumeComponents.
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

Shader "Hidden/VolumeViewer/CullingPlaceholder"
{
	SubShader
	{
		Tags{ "VolumeTag" = "Culling" }

		ZWrite Off

		Pass
		{
			ColorMask 0
		}
	}
	FallBack Off
}