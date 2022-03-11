///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeComponentAsset
/// Description:        ScriptableObject to save a VolumeComponent
///                         as an .asset file.
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

using UnityEngine;

namespace VolumeViewer
{
    public class VolumeComponentAsset : ScriptableObject
    {
        public float                    resolution              = 1.0f;
        public int                      maxSamples              = 64;
        public Texture2D                rayOffset;
        public Texture3D                data;
        public VolumeDataType           dataType;
        public Vector3                  voxelDimensions         = Vector3.one;
        public Color                    dataChannelWeight       = Color.white;
        public float                    valueRangeMin           = 0.00f;
        public float                    valueRangeMax           = 1.00f;
        public float                    cutValueRangeMin        = 0.00f;
        public float                    cutValueRangeMax        = 1.00f;
        public Texture2D                tfData;
        public VolumeBlendMode          tfDataBlendMode;
        public bool                     tf2D                    = false;
        public float                    gradientRangeMin        = 0.00f;
        public float                    gradientRangeMax        = 1.00f;
        public float                    cutGradientRangeMin     = 0.00f;
        public float                    cutGradientRangeMax     = 1.00f;
        public Texture3D                overlay;
        public VolumeDataType           overlayType;
        public Color                    overlayChannelWeight    = Color.white;
        public VolumeBlendMode          overlayBlendMode;
        public bool                     overlayVoidsCulling     = false;
        public Texture2D                tfOverlay;
        public VolumeBlendMode          tfOverlayBlendMode;
        public float                    contrast                = 0;
        public float                    brightness              = 0;
        public float                    opacity                 = 1.0f;
        public bool                     enableLight             = false;
        public float                    surfaceThr              = 0;
        public float                    surfaceAlpha            = 0.5f;
        public Color                    ambientColor            = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        public Color                    diffuseColor            = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        public Color                    specularColor           = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public float                    shininess               = 4;
        public int                      maxShadedSamples        = 1;
        public VolumeGradientFetches    surfaceGradientFetches  = VolumeGradientFetches._3;
        public Texture2D                tfLight;
        public bool                     hideZeros               = true;
        public bool                     invertCulling           = false;
    }
}