///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeFileLoaderEditor
/// Description:        Custom editor for the VolumeFileLoader class.
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
    [CustomEditor(typeof(VolumeFileLoader), true)]
    [CanEditMultipleObjects]
    public class VolumeFileLoaderEditor : Editor
    {

        bool showEvents;
        bool showOverlay;
        bool showTF;

        SerializedProperty loadOnStartupProp;
        SerializedProperty dataPathProp;
        SerializedProperty forceDataFormatProp;
        SerializedProperty forceDataPlanarConfigurationProp;
        SerializedProperty overlayPathProp;
        SerializedProperty forceOverlayFormatProp;
        SerializedProperty forceOverlayPlanarConfigurationProp;
        SerializedProperty loadAssetInsteadProp;

        SerializedProperty dataPathChangedProp;
        SerializedProperty overlayPathChangedProp;
        SerializedProperty loadingProgressChangedProp;

        VolumeFileLoader volumeFileLoader;
        VolumeComponent volumeComponent;

        void OnEnable()
        {
			volumeFileLoader = (VolumeFileLoader)target;
            volumeComponent = volumeFileLoader.GetComponent<VolumeComponent>();

            loadOnStartupProp = serializedObject.FindProperty("loadOnStartup");
            dataPathProp = serializedObject.FindProperty("_dataPath");
            forceDataFormatProp = serializedObject.FindProperty("_forceDataFormat");
            forceDataPlanarConfigurationProp = serializedObject.FindProperty("_forceDataPlanarConfiguration");
            overlayPathProp = serializedObject.FindProperty("_overlayPath");
            forceOverlayFormatProp = serializedObject.FindProperty("_forceOverlayFormat");
            forceOverlayPlanarConfigurationProp = serializedObject.FindProperty("_forceOverlayPlanarConfiguration");
            loadAssetInsteadProp = serializedObject.FindProperty("_loadAssetInstead");

            dataPathChangedProp = serializedObject.FindProperty("_dataPathChanged");
            overlayPathChangedProp = serializedObject.FindProperty("_overlayPathChanged");
            loadingProgressChangedProp = serializedObject.FindProperty("_loadingProgressChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(loadOnStartupProp);
            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(volumeFileLoader.loadAssetInstead != null);
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(dataPathProp);
            if (GUILayout.Button("Choose"))
            {
                string path = chooseFile();
                if (path.Length > 0)
                {
                    volumeFileLoader.dataPath = path;
                }
            }
			GUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(forceDataFormatProp);
            EditorGUILayout.PropertyField(forceDataPlanarConfigurationProp);
            EditorGUILayout.Space();
            if (!showOverlay && overlayPathProp.stringValue.Length == 0)
            {
                if (GUILayout.Button("Add Overlay"))
                {
                    showOverlay = true;
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(overlayPathProp);
                if (GUILayout.Button("Choose"))
                {
                    string path = chooseFile();
                    if(path.Length > 0)
                    {
                        volumeFileLoader.overlayPath = path;
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(forceOverlayFormatProp);
                EditorGUILayout.PropertyField(forceOverlayPlanarConfigurationProp);
            }
            GUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            EditorGUILayout.PropertyField(loadAssetInsteadProp);
            if (volumeComponent.active)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Save VolumeComponent as Asset"))
                {
                    saveCache();
                }
            }
            GUILayout.EndVertical();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events: ", EditorStyles.boldLabel);
            showEvents = EditorGUILayout.Foldout(showEvents, "Events");
            if (showEvents)
            {
                EditorGUILayout.PropertyField(dataPathChangedProp);
                EditorGUILayout.PropertyField(overlayPathChangedProp);
                EditorGUILayout.PropertyField(loadingProgressChangedProp);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public string chooseFile()
        {
            string path = EditorUtility.OpenFilePanel("Choose file", "", "");
            int assetFolderIndex = path.IndexOf("Assets/");
            if (assetFolderIndex >= 0 && path.IndexOf(Application.dataPath) == 0)
            {
                path = path.Substring(assetFolderIndex);
            }
            return path;
        }

        public void saveCache()
        {
            string path = EditorUtility.SaveFilePanelInProject("Save VolumeComponent as an Asset file", "", "asset", "Please enter a file name to save the Asset to.");
            if (path.Length > 6)
            {
                if (AssetDatabase.GetAssetPath(volumeComponent.data).Length < 3)
                {
                    AssetDatabase.CreateAsset(volumeComponent.data, path.Substring(0, path.Length - 6) + "_data.asset");
                }
                if (volumeComponent.overlay != null && AssetDatabase.GetAssetPath(volumeComponent.overlay).Length < 3)
                {
                    AssetDatabase.CreateAsset(volumeComponent.overlay, path.Substring(0, path.Length - 6) + "_overlay.asset");
                }
                VolumeComponentAsset vcAsset = CreateInstance<VolumeComponentAsset>();
                vcAsset.ambientColor = volumeComponent.ambientColor;
                vcAsset.brightness = volumeComponent.brightness;
                vcAsset.contrast = volumeComponent.contrast;
                vcAsset.cutGradientRangeMax = volumeComponent.cutGradientRangeMax;
                vcAsset.cutGradientRangeMin = volumeComponent.cutGradientRangeMin;
                vcAsset.cutValueRangeMax = volumeComponent.cutValueRangeMax;
                vcAsset.cutValueRangeMin = volumeComponent.cutValueRangeMin;
                vcAsset.data = volumeComponent.data;
                vcAsset.dataChannelWeight = volumeComponent.dataChannelWeight;
                vcAsset.dataType = volumeComponent.dataType;
                vcAsset.diffuseColor = volumeComponent.diffuseColor;
                vcAsset.enableLight = volumeComponent.enableLight;
                vcAsset.gradientRangeMax = volumeComponent.gradientRangeMax;
                vcAsset.gradientRangeMin = volumeComponent.gradientRangeMin;
                vcAsset.hideZeros = volumeComponent.hideZeros;
                vcAsset.invertCulling = volumeComponent.invertCulling;
                vcAsset.maxSamples = volumeComponent.maxSamples;
                vcAsset.maxShadedSamples = volumeComponent.maxShadedSamples;
                vcAsset.opacity = volumeComponent.opacity;
                vcAsset.overlay = volumeComponent.overlay;
                vcAsset.overlayBlendMode = volumeComponent.overlayBlendMode;
                vcAsset.overlayChannelWeight = volumeComponent.overlayChannelWeight;
                vcAsset.overlayVoidsCulling = volumeComponent.overlayVoidsCulling;
                vcAsset.overlayType = volumeComponent.overlayType;
                vcAsset.rayOffset = volumeComponent.rayOffset;
                vcAsset.resolution = volumeComponent.resolution;
                vcAsset.shininess = volumeComponent.shininess;
                vcAsset.specularColor = volumeComponent.specularColor;
                vcAsset.surfaceAlpha = volumeComponent.surfaceAlpha;
                vcAsset.surfaceGradientFetches = volumeComponent.surfaceGradientFetches;
                vcAsset.surfaceThr = volumeComponent.surfaceThr;
                vcAsset.tf2D = volumeComponent.tf2D;
                vcAsset.tfData = volumeComponent.tfData;
                vcAsset.tfDataBlendMode = volumeComponent.tfDataBlendMode;
                vcAsset.tfLight = volumeComponent.tfLight;
                vcAsset.tfOverlay = volumeComponent.tfOverlay;
                vcAsset.tfOverlayBlendMode = volumeComponent.tfOverlayBlendMode;
                vcAsset.valueRangeMax = volumeComponent.valueRangeMax;
                vcAsset.valueRangeMin = volumeComponent.valueRangeMin;
                vcAsset.voxelDimensions = volumeComponent.voxelDimensions;
                AssetDatabase.CreateAsset(vcAsset, path);
                //EditorUtility.FocusProjectWindow();
                Selection.activeObject = vcAsset;
            }
        }
    }
}