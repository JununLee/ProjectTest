///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeComponentEditor
/// Description:        Custom editor for the VolumeComponent class.
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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolumeViewer
{
	[CustomEditor(typeof(VolumeComponent), true)]
	[CanEditMultipleObjects]
	public class VolumeComponentEditor : Editor
	{
		bool showEvents;

        SerializedProperty activateOnStartupProp;
        SerializedProperty dataChannelWeightProp;
        SerializedProperty dataDimensionsProp;
        SerializedProperty tfDataProp;
        SerializedProperty tfDataBlendModeProp;
        SerializedProperty rayOffsetProp;
        SerializedProperty overlayChannelWeightProp;
        SerializedProperty overlayVoidsCullingProp;
        SerializedProperty overlayBlendModeProp;
        SerializedProperty tfOverlayProp;
        SerializedProperty tfOverlayBlendModeProp;
		SerializedProperty resolutionProp;
		SerializedProperty maxSamplesProp;
		SerializedProperty contrastProp;
		SerializedProperty brightnessProp;
		SerializedProperty opacityProp;
		SerializedProperty invertCullingProp;
		SerializedProperty hideZerosProp;
        SerializedProperty valueRangeMinProp;
        SerializedProperty valueRangeMaxProp;
        SerializedProperty cutValueRangeMinProp;
		SerializedProperty cutValueRangeMaxProp;
		SerializedProperty gradientRangeMinProp;
		SerializedProperty gradientRangeMaxProp;
		SerializedProperty cutGradientRangeMinProp;
		SerializedProperty cutGradientRangeMaxProp;
        SerializedProperty surfaceThrProp;
        SerializedProperty enableLightProp;
        SerializedProperty maxShadedSamplesProp;
        SerializedProperty ambientColorProp;
        SerializedProperty diffuseColorProp;
        SerializedProperty specularColorProp;
        SerializedProperty shininessProp;
        SerializedProperty tfLightProp;
        SerializedProperty surfaceAlphaProp;

        SerializedProperty dataTypeChangedProp;
        SerializedProperty overlayTypeChangedProp;
        SerializedProperty activeChangedProp;
        SerializedProperty dataChangedProp;
        SerializedProperty dataChannelWeightChangedProp;
        SerializedProperty tfDataChangedProp;
        SerializedProperty tfDataBlendModeChangedProp;
        SerializedProperty tf2DChangedProp;
        SerializedProperty rayOffsetChangedProp;
        SerializedProperty overlayChangedProp;
        SerializedProperty overlayChannelWeightChangedProp;
        SerializedProperty overlayVoidsCullingChangedProp;
        SerializedProperty overlayBlendModeChangedProp;
        SerializedProperty tfOverlayChangedProp;
        SerializedProperty tfOverlayBlendModeChangedProp;
		SerializedProperty voxelDimensionsChangedProp;
		SerializedProperty resolutionChangedProp;
		SerializedProperty maxSamplesChangedProp;
		SerializedProperty contrastChangedProp;
		SerializedProperty brightnessChangedProp;
		SerializedProperty opacityChangedProp;
		SerializedProperty invertCullingChangedProp;
		SerializedProperty hideZerosChangedProp;
        SerializedProperty valueRangeMinChangedProp;
        SerializedProperty valueRangeMaxChangedProp;
        SerializedProperty cutValueRangeMinChangedProp;
		SerializedProperty cutValueRangeMaxChangedProp;
		SerializedProperty gradientRangeMinChangedProp;
		SerializedProperty gradientRangeMaxChangedProp;
		SerializedProperty cutGradientRangeMinChangedProp;
		SerializedProperty cutGradientRangeMaxChangedProp;
		SerializedProperty surfaceThrChangedProp;
        SerializedProperty enableLightChangedProp;
        SerializedProperty maxShadedSamplesChangedProp;
        SerializedProperty surfaceGradientFetchesChangedProp;
        SerializedProperty ambientColorChangedProp;
        SerializedProperty diffuseColorChangedProp;
        SerializedProperty specularColorChangedProp;
        SerializedProperty shininessChangedProp;
        SerializedProperty tfLightChangedProp;
        SerializedProperty surfaceAlphaChangedProp;

        VolumeComponent volumeComponent;
        VolumeFileLoader volumeFileLoader;

        void OnEnable()
		{
			volumeComponent = (VolumeComponent) target;
            volumeFileLoader = volumeComponent.GetComponent<VolumeFileLoader>();

            activateOnStartupProp = serializedObject.FindProperty("_activateOnStartup");
            dataDimensionsProp = serializedObject.FindProperty("_dataDimensions");
            dataChannelWeightProp = serializedObject.FindProperty("_dataChannelWeight");
            tfDataProp = serializedObject.FindProperty("_tfData");
            tfDataBlendModeProp = serializedObject.FindProperty("_tfDataBlendMode");
            rayOffsetProp = serializedObject.FindProperty("_rayOffset");
            overlayChannelWeightProp = serializedObject.FindProperty("_overlayChannelWeight");
            overlayBlendModeProp = serializedObject.FindProperty("_overlayBlendMode");
            tfOverlayProp = serializedObject.FindProperty("_tfOverlay");
            overlayVoidsCullingProp = serializedObject.FindProperty("_overlayVoidsCulling");
            tfOverlayBlendModeProp = serializedObject.FindProperty("_tfOverlayBlendMode");
			resolutionProp = serializedObject.FindProperty("_resolution");
			maxSamplesProp = serializedObject.FindProperty("_maxSamples");
			contrastProp = serializedObject.FindProperty("_contrast");
			brightnessProp = serializedObject.FindProperty("_brightness");
			opacityProp = serializedObject.FindProperty("_opacity");
			invertCullingProp = serializedObject.FindProperty("_invertCulling");
			hideZerosProp = serializedObject.FindProperty("_hideZeros");
            valueRangeMinProp = serializedObject.FindProperty("_valueRangeMin");
            valueRangeMaxProp = serializedObject.FindProperty("_valueRangeMax");
            cutValueRangeMinProp = serializedObject.FindProperty("_cutValueRangeMin");
			cutValueRangeMaxProp = serializedObject.FindProperty("_cutValueRangeMax");
			gradientRangeMinProp = serializedObject.FindProperty("_gradientRangeMin");
			gradientRangeMaxProp = serializedObject.FindProperty("_gradientRangeMax");
			cutGradientRangeMinProp = serializedObject.FindProperty("_cutGradientRangeMin");
			cutGradientRangeMaxProp = serializedObject.FindProperty("_cutGradientRangeMax");
            surfaceThrProp = serializedObject.FindProperty("_surfaceThr");
            enableLightProp = serializedObject.FindProperty("_enableLight");
            maxShadedSamplesProp = serializedObject.FindProperty("_maxShadedSamples");
            ambientColorProp = serializedObject.FindProperty("_ambientColor");
            diffuseColorProp = serializedObject.FindProperty("_diffuseColor");
            specularColorProp = serializedObject.FindProperty("_specularColor");
            shininessProp = serializedObject.FindProperty("_shininess");
            tfLightProp = serializedObject.FindProperty("_tfLight");
            surfaceAlphaProp = serializedObject.FindProperty("_surfaceAlpha");

            dataTypeChangedProp = serializedObject.FindProperty("_dataTypeChanged");
            overlayTypeChangedProp = serializedObject.FindProperty("_overlayTypeChanged");
            activeChangedProp = serializedObject.FindProperty("_activeChanged");
            dataChangedProp = serializedObject.FindProperty("_dataChanged");
            dataChannelWeightChangedProp = serializedObject.FindProperty("_dataChannelWeightChanged");
            tfDataChangedProp = serializedObject.FindProperty("_tfDataChanged");
            tfDataBlendModeChangedProp = serializedObject.FindProperty("_tfDataBlendModeChanged");
            tf2DChangedProp = serializedObject.FindProperty("_tf2DChanged");
            rayOffsetChangedProp = serializedObject.FindProperty("_rayOffsetChanged");
            overlayChangedProp = serializedObject.FindProperty("_overlayChanged");
            overlayChannelWeightChangedProp = serializedObject.FindProperty("_overlayChannelWeightChanged");
            overlayVoidsCullingChangedProp = serializedObject.FindProperty("_overlayVoidsCullingChanged");
            overlayBlendModeChangedProp = serializedObject.FindProperty("_overlayBlendModeChanged");
            tfOverlayChangedProp = serializedObject.FindProperty("_tfOverlayChanged");
            tfOverlayBlendModeChangedProp = serializedObject.FindProperty("_tfOverlayBlendModeChanged");
            voxelDimensionsChangedProp = serializedObject.FindProperty("_voxelDimensionsChanged");
			resolutionChangedProp = serializedObject.FindProperty("_resolutionChanged");
			maxSamplesChangedProp = serializedObject.FindProperty("_maxSamplesChanged");
			contrastChangedProp = serializedObject.FindProperty("_contrastChanged");
			brightnessChangedProp = serializedObject.FindProperty("_brightnessChanged");
			opacityChangedProp = serializedObject.FindProperty("_opacityChanged");
			invertCullingChangedProp = serializedObject.FindProperty("_invertCullingChanged");
			hideZerosChangedProp = serializedObject.FindProperty("_hideZerosChanged");
            valueRangeMinChangedProp = serializedObject.FindProperty("_valueRangeMinChanged");
            valueRangeMaxChangedProp = serializedObject.FindProperty("_valueRangeMaxChanged");
            cutValueRangeMinChangedProp = serializedObject.FindProperty("_cutValueRangeMinChanged");
			cutValueRangeMaxChangedProp = serializedObject.FindProperty("_cutValueRangeMaxChanged");
			gradientRangeMinChangedProp = serializedObject.FindProperty("_gradientRangeMinChanged");
			gradientRangeMaxChangedProp = serializedObject.FindProperty("_gradientRangeMaxChanged");
			cutGradientRangeMinChangedProp = serializedObject.FindProperty("_cutGradientRangeMinChanged");
			cutGradientRangeMaxChangedProp = serializedObject.FindProperty("_cutGradientRangeMaxChanged");
            surfaceThrChangedProp = serializedObject.FindProperty("_surfaceThrChanged");
            enableLightChangedProp = serializedObject.FindProperty("_enableLightChanged");
            maxShadedSamplesChangedProp = serializedObject.FindProperty("_maxShadedSamplesChanged");
            surfaceGradientFetchesChangedProp = serializedObject.FindProperty("_surfaceGradientFetchesChanged");
            ambientColorChangedProp = serializedObject.FindProperty("_ambientColorChanged");
            diffuseColorChangedProp = serializedObject.FindProperty("_diffuseColorChanged");
            specularColorChangedProp = serializedObject.FindProperty("_specularColorChanged");
            shininessChangedProp = serializedObject.FindProperty("_shininessChanged");
            tfLightChangedProp = serializedObject.FindProperty("_tfLightChanged");
            surfaceAlphaChangedProp = serializedObject.FindProperty("_surfaceAlphaChanged");
        }
        
		public override void OnInspectorGUI()
		{
			serializedObject.Update();

            if(volumeComponent.data != null && (volumeFileLoader == null || volumeFileLoader.enabled == false))
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(activateOnStartupProp);
            }

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Rendering: ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(resolutionProp);
            EditorGUILayout.PropertyField(maxSamplesProp);
            EditorGUILayout.PropertyField(rayOffsetProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Data: ", EditorStyles.boldLabel);
            Texture3D data = null;
            data = (Texture3D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(), "Data", volumeComponent.data, typeof(Texture3D), false);
            if (data != volumeComponent.data)
            {
                volumeComponent.data = data;
                volumeComponent.adjustScale();
                volumeComponent.determineDataType();
            }
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(dataDimensionsProp);
            EditorGUI.EndDisabledGroup();
            volumeComponent.voxelDimensions = (Vector3)EditorGUI.Vector3Field(EditorGUILayout.GetControlRect(), "Voxel Dimensions", volumeComponent.voxelDimensions);
            if (volumeComponent.dataType == VolumeDataType.RGBAData)
            {
                EditorGUILayout.PropertyField(dataChannelWeightProp);
            }
            EditorGUILayout.PropertyField(valueRangeMinProp);
            EditorGUILayout.PropertyField(valueRangeMaxProp);
            EditorGUILayout.PropertyField(cutValueRangeMinProp);
            EditorGUILayout.PropertyField(cutValueRangeMaxProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transfer Function: ", EditorStyles.boldLabel);
            //if (volumeComponent.data != null)
            {
                EditorGUILayout.PropertyField(tfDataProp);
                //if (volumeComponent.tfData != null)
                {
                    EditorGUILayout.PropertyField(tfDataBlendModeProp);
                    volumeComponent.tf2D = (bool)EditorGUI.Toggle(EditorGUILayout.GetControlRect(), "Tf is 2D (f vs |\u2207f|)", volumeComponent.tf2D);
                }
            }
            if (volumeComponent.tfData != null && volumeComponent.tf2D)
            {
                EditorGUILayout.PropertyField(gradientRangeMinProp);
                EditorGUILayout.PropertyField(gradientRangeMaxProp);
                EditorGUILayout.PropertyField(cutGradientRangeMinProp);
                EditorGUILayout.PropertyField(cutGradientRangeMaxProp);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Overlay: ", EditorStyles.boldLabel);
            
            Texture3D overlay = null;
            overlay = (Texture3D)EditorGUI.ObjectField(EditorGUILayout.GetControlRect(), "Overlay", volumeComponent.overlay, typeof(Texture3D), false);
            if (overlay != volumeComponent.overlay)
            {
                volumeComponent.overlay = overlay;
                volumeComponent.determineOverlayType();
            }
            if (volumeComponent.overlayType == VolumeDataType.RGBAData)
            {
                EditorGUILayout.PropertyField(overlayChannelWeightProp);
            }
            //if (volumeComponent.overlay != null)
            {
                EditorGUILayout.PropertyField(overlayBlendModeProp);
                EditorGUILayout.PropertyField(overlayVoidsCullingProp);
                EditorGUILayout.PropertyField(tfOverlayProp);
                //if (volumeComponent.tfOverlay != null)
                {
                    EditorGUILayout.PropertyField(tfOverlayBlendModeProp);
                }
            }
            
            EditorGUILayout.Space();
			EditorGUILayout.LabelField("Properties: ", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(contrastProp);
			EditorGUILayout.PropertyField(brightnessProp);
			EditorGUILayout.PropertyField(opacityProp);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Light: ", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(enableLightProp);
            //if (volumeComponent.enableLight)
            {
                EditorGUILayout.PropertyField(surfaceThrProp);
                EditorGUILayout.PropertyField(surfaceAlphaProp);
                EditorGUILayout.PropertyField(ambientColorProp);
                EditorGUILayout.PropertyField(diffuseColorProp);
                EditorGUILayout.PropertyField(specularColorProp);
                EditorGUILayout.PropertyField(shininessProp);
                EditorGUILayout.PropertyField(maxShadedSamplesProp);
                volumeComponent.surfaceGradientFetches = (VolumeGradientFetches)EditorGUI.EnumPopup(EditorGUILayout.GetControlRect(), "Gradient Fetches", volumeComponent.surfaceGradientFetches);
                EditorGUILayout.PropertyField(tfLightProp);
            }
            
            EditorGUILayout.Space();
			EditorGUILayout.LabelField("Exclusion: ", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(hideZerosProp);
			EditorGUILayout.PropertyField(invertCullingProp);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Events: ", EditorStyles.boldLabel);
			showEvents = EditorGUILayout.Foldout(showEvents, "Events");
			if (showEvents)
            {
                EditorGUILayout.PropertyField(activeChangedProp);
                EditorGUILayout.PropertyField(resolutionChangedProp);
                EditorGUILayout.PropertyField(maxSamplesChangedProp);
                EditorGUILayout.PropertyField(rayOffsetChangedProp);
                EditorGUILayout.PropertyField(dataChangedProp);
                EditorGUILayout.PropertyField(dataTypeChangedProp);
                EditorGUILayout.PropertyField(voxelDimensionsChangedProp);
                EditorGUILayout.PropertyField(dataChannelWeightChangedProp);
                EditorGUILayout.PropertyField(valueRangeMinChangedProp);
                EditorGUILayout.PropertyField(valueRangeMaxChangedProp);
                EditorGUILayout.PropertyField(cutValueRangeMinChangedProp);
                EditorGUILayout.PropertyField(cutValueRangeMaxChangedProp);
                EditorGUILayout.PropertyField(tfDataChangedProp); 
                EditorGUILayout.PropertyField(tfDataBlendModeChangedProp);
                EditorGUILayout.PropertyField(tf2DChangedProp);
                EditorGUILayout.PropertyField(gradientRangeMinChangedProp);
                EditorGUILayout.PropertyField(gradientRangeMaxChangedProp);
                EditorGUILayout.PropertyField(cutGradientRangeMinChangedProp);
                EditorGUILayout.PropertyField(cutGradientRangeMaxChangedProp);
                EditorGUILayout.PropertyField(overlayChangedProp);
                EditorGUILayout.PropertyField(overlayTypeChangedProp);
                EditorGUILayout.PropertyField(overlayChannelWeightChangedProp);
                EditorGUILayout.PropertyField(overlayBlendModeChangedProp);
                EditorGUILayout.PropertyField(overlayVoidsCullingChangedProp);
                EditorGUILayout.PropertyField(tfOverlayChangedProp);
                EditorGUILayout.PropertyField(tfOverlayBlendModeChangedProp);
				EditorGUILayout.PropertyField(contrastChangedProp);
				EditorGUILayout.PropertyField(brightnessChangedProp);
				EditorGUILayout.PropertyField(opacityChangedProp);
                EditorGUILayout.PropertyField(enableLightChangedProp);
                EditorGUILayout.PropertyField(surfaceThrChangedProp);
                EditorGUILayout.PropertyField(surfaceAlphaChangedProp);
                EditorGUILayout.PropertyField(ambientColorChangedProp);
                EditorGUILayout.PropertyField(diffuseColorChangedProp);
                EditorGUILayout.PropertyField(specularColorChangedProp);
                EditorGUILayout.PropertyField(shininessChangedProp);
                EditorGUILayout.PropertyField(maxShadedSamplesChangedProp);
                EditorGUILayout.PropertyField(surfaceGradientFetchesChangedProp);
                EditorGUILayout.PropertyField(tfLightChangedProp);
                EditorGUILayout.PropertyField(hideZerosChangedProp);
                EditorGUILayout.PropertyField(invertCullingChangedProp);
            }
			serializedObject.ApplyModifiedProperties();
		}
	}
}