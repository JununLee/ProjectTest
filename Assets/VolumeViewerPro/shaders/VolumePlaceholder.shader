///-----------------------------------------------------------------
/// Shader:             VolumePlaceholder
/// Description:        Placeholder shader that will get replaced
///							to render VolumeComponents.
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

Shader "Hidden/VolumeViewer/VolumePlaceholder" 
{
	SubShader
	{
		Tags {"VolumeTag" = "Volume"}

		ZWrite Off

		Pass
		{
			ColorMask 0
		}
	}
	FallBack Off
}
