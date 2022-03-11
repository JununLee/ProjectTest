///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeRendererEditor
/// Description:        Custom editor for the VolumeRenderer class.
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

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VolumeViewer
{
    [CustomEditor(typeof(VolumeRenderer), true)]
    [CanEditMultipleObjects]
    public class VolumeRendererEditor : Editor
    {

        bool showEvents;

        SerializedProperty volumeObjectsProp;
        SerializedProperty enableCullingProp;
        SerializedProperty enableDepthTestProp;
        SerializedProperty leapProp;

        SerializedProperty volumeObjectsChangedProp;
        SerializedProperty enableCullingChangedProp;
        SerializedProperty enableDepthTestChangedProp;
        SerializedProperty leapChangedProp;

        void OnEnable()
        {
            volumeObjectsProp = serializedObject.FindProperty("_volumeObjects");
            enableCullingProp = serializedObject.FindProperty("_enableCulling");
            enableDepthTestProp = serializedObject.FindProperty("_enableDepthTest");
            leapProp = serializedObject.FindProperty("_leap");
            volumeObjectsChangedProp = serializedObject.FindProperty("_volumeObjectsChanged");
            enableCullingChangedProp = serializedObject.FindProperty("_enableCullingChanged");
            enableDepthTestChangedProp = serializedObject.FindProperty("_enableDepthTestChanged");
            leapChangedProp = serializedObject.FindProperty("_leapChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(enableCullingProp);
            EditorGUILayout.PropertyField(enableDepthTestProp);
            EditorGUILayout.PropertyField(leapProp);
            EditorGUILayout.PropertyField(volumeObjectsProp, true);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events: ", EditorStyles.boldLabel);
            showEvents = EditorGUILayout.Foldout(showEvents, "Events");
            if (showEvents)
            {
                EditorGUILayout.PropertyField(enableCullingChangedProp);
                EditorGUILayout.PropertyField(enableDepthTestChangedProp);
                EditorGUILayout.PropertyField(leapChangedProp);
                EditorGUILayout.PropertyField(volumeObjectsChangedProp);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}