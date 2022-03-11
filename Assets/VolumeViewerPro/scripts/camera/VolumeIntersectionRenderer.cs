///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeIntersectionRenderer
/// Description:        Renders the intersection of a plane with a 
///                         VolumeComponent.
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
    [RequireComponent(typeof(Camera))]
    public class VolumeIntersectionRenderer : MonoBehaviour
    {
        //VolumeComponent to intersect with the plane.
        //Unlike VolumeRenderer, VolumeIntersectionRenderer can only interact with one VolumeComponent.
        public VolumeComponent volumeComponent;

        //Normal vector of the plane
        [SerializeField]
        Vector3 _planeNormal;
        public Vector3 planeNormal { get { return _planeNormal; } set { _planeNormal = value; } }

        //Offset along the plane normal from the center of the volumeComponent
        [SerializeField]
        float _planeOffset;
        public float planeOffset { get { return _planeOffset; } set { _planeOffset = value; } }

        //Will be set to custom VolumeViewer layer only.
        LayerMask volumeLayerMask;

        //Shaders required for intersection rendering.
        Shader rayIntersectShader;
        Shader blendVolumeShader;
        Shader volumePlaceholderShader;

        //Will be made a duplicate of the camera this script is attached to.
        Camera volumeCam;

        //Materials for VolumeComponents.
        Material rayIntersectMaterial;
        Material blendVolumeMaterial;
        Material volumePlaceholderMaterial;

        //Initialization
        void Start()
        { 
            if (LayerMask.NameToLayer("VolumeViewer") < 0)
            {
                Debug.Log("VolumeIntersectionRenderer disabled. Please add a User Layer named 'VolumeViewer' to this project. Check out the documentation for more details.");
                enabled = false;
                return;
            }

            volumeLayerMask = LayerMask.GetMask("VolumeViewer");
            
            //Find and assign shaders.
            rayIntersectShader = Shader.Find("Hidden/VolumeViewer/RayIntersect");
            blendVolumeShader = Shader.Find("Hidden/VolumeViewer/BlendVolume");
            volumePlaceholderShader = Shader.Find("Hidden/VolumeViewer/VolumePlaceholder");

            //Create materials.
            rayIntersectMaterial = new Material(rayIntersectShader);
            blendVolumeMaterial = new Material(blendVolumeShader);
            volumePlaceholderMaterial = new Material(volumePlaceholderShader);

            //Create a new camera.
            GameObject volumeCamObj = new GameObject("IntersectionCam");
            volumeCam = volumeCamObj.AddComponent<Camera>();
            volumeCam.enabled = false;
        }

        //OnRenderImage is called after all other rendering is complete.
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            ///If the VolumeComponent isn't ready, don't do anything.
            if (!volumeComponent.active)
            {
                Graphics.Blit(source, destination);
                return;
            }

            //Put the VolumeComponent on the VolumeViewer layer.
            volumeComponent.gameObject.layer = LayerMask.NameToLayer("VolumeViewer");

            //Width and height of the image (screen) to process.
            int width = (int)(source.width / volumeComponent.resolution);
            int height = (int)(source.height / volumeComponent.resolution);

            //Get temporary render texture.
            RenderTexture volumeProjection = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);

            //Make volumeCam a copy of the camera this script is attached to.
            volumeCam.CopyFrom(GetComponent<Camera>());
            volumeCam.clearFlags = CameraClearFlags.SolidColor;
            volumeCam.rect = new Rect(0, 0, 1, 1);
            volumeCam.backgroundColor = Color.black;

            //Make sure volumeCam only sees the VolumeViewer layer.
            volumeCam.cullingMask = volumeLayerMask;

            //Set shader keywords.
            if (volumeComponent.dataType == VolumeDataType.RGBAData)
            {
                rayIntersectMaterial.EnableKeyword("VOLUMEVIEWER_RGBA_DATA");
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
            }
            else if (volumeComponent.tfData != null && volumeComponent.tfDataBlendMode != VolumeBlendMode.Disabled)
            {
                rayIntersectMaterial.SetTexture("tfData2D", volumeComponent.tfData);
                rayIntersectMaterial.SetFloat("tfDataBlendMode", ((int)volumeComponent.tfDataBlendMode));
                if (volumeComponent.tf2D)
                {
                    rayIntersectMaterial.EnableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                }
                else
                {
                    rayIntersectMaterial.EnableKeyword("VOLUMEVIEWER_TF_DATA");
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                }
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_DATA");
            }
            else
            {
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_DATA");
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
            }

            if (volumeComponent.overlay != null && volumeComponent.overlayBlendMode != VolumeBlendMode.Disabled)
            {
                if (volumeComponent.overlayType == VolumeDataType.RGBAData)
                {
                    rayIntersectMaterial.SetTexture("overlay3D", volumeComponent.overlay);
                    rayIntersectMaterial.SetColor("overlayChannelWeight", volumeComponent.overlayChannelWeight);
                    rayIntersectMaterial.SetFloat("overlayBlendMode", (int)volumeComponent.overlayBlendMode);
                    rayIntersectMaterial.EnableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                }
                else if (volumeComponent.tfOverlay != null && volumeComponent.tfOverlayBlendMode != VolumeBlendMode.Disabled)
                {
                    rayIntersectMaterial.SetTexture("overlay3D", volumeComponent.overlay);
                    rayIntersectMaterial.SetColor("overlayChannelWeight", volumeComponent.overlayChannelWeight);
                    rayIntersectMaterial.SetFloat("overlayBlendMode", (int)volumeComponent.overlayBlendMode);
                    rayIntersectMaterial.SetTexture("tfOverlay2D", volumeComponent.tfOverlay);
                    rayIntersectMaterial.SetFloat("tfOverlayBlendMode", (int)volumeComponent.tfOverlayBlendMode);
                    rayIntersectMaterial.EnableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                }
                else
                {
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                    rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                }
            }
            else
            {
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                rayIntersectMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
            }

            //Set shader variables.
            rayIntersectMaterial.SetTexture("data3D", volumeComponent.data);
            rayIntersectMaterial.SetTexture("rayOffset2D", volumeComponent.rayOffset);
            rayIntersectMaterial.SetColor("dataChannelWeight", volumeComponent.dataChannelWeight);
            rayIntersectMaterial.SetFloat("maxSamples", (float)volumeComponent.maxSamples);
            rayIntersectMaterial.SetFloat("focalLength", 1.0f / Mathf.Tan(volumeCam.fieldOfView * 0.01745329251f / 2.0f));
            rayIntersectMaterial.SetFloat("nearClipPlane", volumeCam.nearClipPlane);
            rayIntersectMaterial.SetFloat("contrast", 1.1f * (volumeComponent.contrast + 1.0f) / (1.0f * (1.1f - volumeComponent.contrast)));
            rayIntersectMaterial.SetFloat("brightness", volumeComponent.brightness);
            rayIntersectMaterial.SetFloat("valueRangeMin", volumeComponent.valueRangeMin);
            rayIntersectMaterial.SetFloat("valueRangeMax", volumeComponent.valueRangeMax);
            rayIntersectMaterial.SetFloat("cutValueRangeMin", (volumeComponent.cutValueRangeMin - volumeComponent.valueRangeMin) / (volumeComponent.valueRangeMax - volumeComponent.valueRangeMin));
            rayIntersectMaterial.SetFloat("cutValueRangeMax", (volumeComponent.cutValueRangeMax - volumeComponent.valueRangeMin) / (volumeComponent.valueRangeMax - volumeComponent.valueRangeMin));
            rayIntersectMaterial.SetFloat("gradientRangeMin", volumeComponent.gradientRangeMin);
            rayIntersectMaterial.SetFloat("gradientRangeMax", volumeComponent.gradientRangeMax);
            rayIntersectMaterial.SetFloat("cutGradientRangeMin", (volumeComponent.cutGradientRangeMin - volumeComponent.gradientRangeMin) / (volumeComponent.gradientRangeMax - volumeComponent.gradientRangeMin));
            rayIntersectMaterial.SetFloat("cutGradientRangeMax", (volumeComponent.cutGradientRangeMax - volumeComponent.gradientRangeMin) / (volumeComponent.gradientRangeMax - volumeComponent.gradientRangeMin));
            rayIntersectMaterial.SetFloat("maxSlices", volumeComponent.maxSlices);
            rayIntersectMaterial.SetVector("planeNormal", _planeNormal);
            rayIntersectMaterial.SetFloat("planeOffset", _planeOffset);

            //Render volume intersection.
            volumeCam.targetTexture = volumeProjection;
            volumeComponent.GetComponent<Renderer>().material = rayIntersectMaterial;
            volumeCam.Render();
            volumeComponent.GetComponent<Renderer>().material = volumePlaceholderMaterial;

            //Put VolumeComponent back onto the default layer.
            volumeComponent.gameObject.layer = 0;

            //Blend volume projection onto the source image.
            blendVolumeMaterial.SetTexture("_VolumeTex", volumeProjection);
            Graphics.Blit(source, destination, blendVolumeMaterial);

            //Release temporary RenderTexture.
            RenderTexture.ReleaseTemporary(volumeProjection);
        }
    }
}