///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeRenderer
/// Description:        Enables a Camera to render VolumeComponents.
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
using System.Collections.Generic;

namespace VolumeViewer
{
    [RequireComponent(typeof(Camera))]
    public class VolumeRenderer : MonoBehaviour
    {
        //Should the camera consider or ignore culling objects?
        //If you don't need culling, setting enableCulling to false will increase performance.
        [SerializeField]
        private bool _enableCulling = true;
        public bool enableCulling { set { if (_enableCulling == value) { return; } _enableCulling = value; _enableCullingChanged.Invoke(value); } get { return _enableCulling; } }
        [SerializeField]
        private BoolEvent _enableCullingChanged = new BoolEvent();
        public BoolEvent enableCullingChanged { get { return _enableCullingChanged; } set { _enableCullingChanged = value; } }

        //Should the camera consider or ignore the depth of opaque objects in the scene?
        //If you don't need scene integration, setting enableDepthTest to false will increase performance.
        [SerializeField]
        private bool _enableDepthTest = true;
        public bool enableDepthTest { set { if (_enableDepthTest == value) { return; } _enableDepthTest = value; _enableDepthTestChanged.Invoke(value); } get { return _enableDepthTest; } }
        [SerializeField]
        private BoolEvent _enableDepthTestChanged = new BoolEvent();
        public BoolEvent enableDepthTestChanged { get { return _enableDepthTestChanged; } set { _enableDepthTestChanged = value; } }

        //How many samples should be skipped if the intensity is very small?
        //Increasing this value will decrease quality but increase performance.
        [SerializeField]
        [Range(1, 4)]
        protected float _leap = 3.0f;
        public float leap { set { if (_leap == value) { return; } _leap = value; _leapChanged.Invoke(value); } get { return _leap; } }
        [SerializeField]
        protected FloatEvent _leapChanged = new FloatEvent();
        public FloatEvent leapChanged { get { return _leapChanged; } set { _leapChanged = value; } }

        //List of VolumeComponents to be rendered.
        [SerializeField]
		private List<VolumeComponent> _volumeObjects;
		public List<VolumeComponent> volumeObjects { set { if (_volumeObjects == value) { return; } _volumeObjects = value; _volumeObjectsChanged.Invoke(); } get { return _volumeObjects; } }
        [SerializeField]
        private EmptyEvent _volumeObjectsChanged = new EmptyEvent();
        public EmptyEvent volumeObjectsChanged { get { return _volumeObjectsChanged; } set { _volumeObjectsChanged = value; } }
        
        //Will be set to custom VolumeViewer layer only.
        LayerMask volumeLayerMask;

        //Shaders required for volume rendering.
        Shader rayCastShader;
        Shader opaqueDepthShader;
        Shader cullDepthFrontShader;
		Shader cullDepthBackShader;
		Shader blendVolumeShader;
		Shader volumePlaceholderShader;

        //Will be made a duplicate of the camera this script is attached to.
        Camera volumeCam;

        //Materials for VolumeComponents and one to blend everything together.
        Material rayCastMaterial;
        Material blendVolumeMaterial;
		Material volumePlaceholderMaterial;

        // Use this for initialization
        void Start()
        {
            if (LayerMask.NameToLayer("VolumeViewer") < 0)
            {
                Debug.Log("VolumeRenderer disabled. Please add a User Layer named 'VolumeViewer' to this project. Check out the documentation for more details.");
                enabled = false;
                return;
            }

            volumeLayerMask = LayerMask.GetMask("VolumeViewer");

            //Find and assign shaders.
            rayCastShader = Shader.Find("Hidden/VolumeViewer/RayCast");
            opaqueDepthShader = Shader.Find("Hidden/VolumeViewer/OpaqueDepth");
            cullDepthFrontShader = Shader.Find("Hidden/VolumeViewer/CullingDepthFront");
            cullDepthBackShader = Shader.Find("Hidden/VolumeViewer/CullingDepthBack");
            blendVolumeShader = Shader.Find("Hidden/VolumeViewer/BlendVolume");
			volumePlaceholderShader = Shader.Find("Hidden/VolumeViewer/VolumePlaceholder");

            //Create materials.
            rayCastMaterial = new Material(rayCastShader);
			blendVolumeMaterial = new Material(blendVolumeShader);
			volumePlaceholderMaterial = new Material(volumePlaceholderShader);

            //Create a new camera.
            GameObject volumeCamObj = new GameObject("VolumeCam");
            volumeCam = volumeCamObj.AddComponent<Camera>();
            volumeCam.enabled = false;
        }

        //OnRenderImage is called after all other rendering is complete.
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            //Sort VolumeComponents according to their depth from the camera.
            //This ensures VolumeComponents in front are rendered on top.
            int numVolumes = _volumeObjects.Count;
            float[] objDeph = new float[numVolumes];
            int[] objIndex = new int[numVolumes];
			int numActiveVolumes = 0;
            for (int i = 0; i < numVolumes; i++)
            {
                objIndex[i] = i;
                objDeph[i] = Vector3.Dot(_volumeObjects[i].transform.position - transform.position, transform.forward);
				if (_volumeObjects[i].active)
				{
					numActiveVolumes++;
				}
			}
            VolumeUtilities.sortTwo(ref objDeph, ref objIndex);
			RenderTexture cullDepthFrontTex = null;
			RenderTexture cullDepthBackTex = null;
			RenderTexture sceneDepthTex = null;
			if(numActiveVolumes > 0)
			{
				//Make volumeCam a copy of the camera this script is attached to.
				volumeCam.CopyFrom(GetComponent<Camera>());
				volumeCam.clearFlags = CameraClearFlags.SolidColor;
				volumeCam.rect = new Rect(0, 0, 1, 1);
                volumeCam.backgroundColor = Color.black;

                //Render depth of culling objects.
                if (_enableCulling)
                {
                    cullDepthFrontTex = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.RFloat);
                    cullDepthBackTex = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.RFloat);
                    volumeCam.targetTexture = cullDepthFrontTex;
                    volumeCam.RenderWithShader(cullDepthFrontShader, "VolumeTag");
                    volumeCam.targetTexture = cullDepthBackTex;
                    volumeCam.RenderWithShader(cullDepthBackShader, "VolumeTag");
                    
                }
                //Render depth of opaque objects.
                if (_enableDepthTest)
                {
                    volumeCam.backgroundColor = Color.white;
                    sceneDepthTex = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.RFloat);
                    volumeCam.targetTexture = sceneDepthTex;
                    volumeCam.RenderWithShader(opaqueDepthShader, "RenderType");
                }
			}
			volumeCam.backgroundColor = Color.clear;
            //Make sure volumeCam only sees the VolumeViewer layer.
            volumeCam.cullingMask = volumeLayerMask;
            //Loop through all VolumeComponents to render.
            int index;
            for (int i = 0; i < numVolumes; i++)
            {
                //Get the index of the next furthest VolumeComponent.
                index = objIndex[numVolumes - 1 - i];

                //Reference to that VolumeComponent.
                VolumeComponent volumeComponent = _volumeObjects[index];

                //If it's not ready, continue with the next VolumeComponent.
                if (!volumeComponent.active)
				{
					continue;
				}

                //Put the VolumeComponent on the VolumeViewer layer.
                volumeComponent.gameObject.layer = LayerMask.NameToLayer("VolumeViewer");

                //Width and height of the image (screen) to process.
				int width = (int)(source.width / volumeComponent.resolution);
				int height = (int)(source.height / volumeComponent.resolution);

                //Get temporary RenderTexture.
                RenderTexture volumeProjection = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat);

                //Set shader keywords.
                if (volumeComponent.dataType == VolumeDataType.RGBAData)
                {
                    rayCastMaterial.EnableKeyword("VOLUMEVIEWER_RGBA_DATA");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                }
                else if (volumeComponent.tfData != null && volumeComponent.tfDataBlendMode != VolumeBlendMode.Disabled)
                {
                    rayCastMaterial.SetTexture("tfData2D", volumeComponent.tfData);
                    rayCastMaterial.SetFloat("tfDataBlendMode", ((int)volumeComponent.tfDataBlendMode));
                    if (volumeComponent.tf2D)
                    {
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                    }
                    else
                    {
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_TF_DATA");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                    }
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_DATA");
                }
                else
                {
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_DATA");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_DATA_2D");
                }

                if (volumeComponent.overlay != null && volumeComponent.overlayBlendMode != VolumeBlendMode.Disabled)
                {
                    if (volumeComponent.overlayType == VolumeDataType.RGBAData)
                    {
                        rayCastMaterial.SetTexture("overlay3D", volumeComponent.overlay);
                        rayCastMaterial.SetColor("overlayChannelWeight", volumeComponent.overlayChannelWeight);
                        rayCastMaterial.SetFloat("overlayBlendMode", (int)volumeComponent.overlayBlendMode);
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                    }
                    else if (volumeComponent.tfOverlay != null && volumeComponent.tfOverlayBlendMode != VolumeBlendMode.Disabled)
                    {
                        rayCastMaterial.SetTexture("overlay3D", volumeComponent.overlay);
                        rayCastMaterial.SetColor("overlayChannelWeight", volumeComponent.overlayChannelWeight);
                        rayCastMaterial.SetFloat("overlayBlendMode", (int)volumeComponent.overlayBlendMode);
                        rayCastMaterial.SetTexture("tfOverlay2D", volumeComponent.tfOverlay);
                        rayCastMaterial.SetFloat("tfOverlayBlendMode", (int)volumeComponent.tfOverlayBlendMode);
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                    }else
                    {
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                    }
                    if (volumeComponent.overlayVoidsCulling) { rayCastMaterial.EnableKeyword("VOLUMEVIEWER_OVERLAY_VOIDS_CULLING"); } else { rayCastMaterial.DisableKeyword("VOLUMEVIEWER_OVERLAY_VOIDS_CULLING"); }
                }
                else
                {
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_RGBA_OVERLAY");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_OVERLAY");
                }

                if (_enableDepthTest)
                {
                    rayCastMaterial.EnableKeyword("VOLUMEVIEWER_DEPTH_TEST");
                    rayCastMaterial.SetTexture("depth2D", sceneDepthTex);

                }
                else
                {
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_DEPTH_TEST");
                }

                if (_enableCulling)
                {
                    if(volumeComponent.invertCulling)
                    {
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_INVERT_CULLING");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_CULLING");
                    }
                    else
                    {
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_CULLING");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_INVERT_CULLING");
                    }
                    
                    rayCastMaterial.SetTexture("cullFront2D", cullDepthFrontTex);
                    rayCastMaterial.SetTexture("cullBack2D", cullDepthBackTex);
                }
                else
                {
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_CULLING");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_INVERT_CULLING");
                }
                
                if (volumeComponent.enableLight)
                {
                    rayCastMaterial.SetFloat("surfaceThr", volumeComponent.surfaceThr);
                    rayCastMaterial.SetFloat("surfaceAlpha", volumeComponent.surfaceAlpha);
                    rayCastMaterial.SetColor("ambientColor", volumeComponent.ambientColor);
                    rayCastMaterial.SetColor("diffuseColor", volumeComponent.diffuseColor);
                    rayCastMaterial.SetColor("specularColor", volumeComponent.specularColor);
                    rayCastMaterial.SetFloat("shininess", volumeComponent.shininess);
                    rayCastMaterial.SetFloat("maxShadedSamples", volumeComponent.maxShadedSamples);
                    rayCastMaterial.SetFloat("surfaceGradientFetches", (int)volumeComponent.surfaceGradientFetches);
                    if (volumeComponent.tfLight != null)
                    {
                        rayCastMaterial.SetTexture("tfLight2D", volumeComponent.tfLight);
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_TF_LIGHT");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_LIGHT");
                    }
                    else
                    {
                        rayCastMaterial.EnableKeyword("VOLUMEVIEWER_LIGHT");
                        rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_LIGHT");
                    }
                }
                else
                {
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_LIGHT");
                    rayCastMaterial.DisableKeyword("VOLUMEVIEWER_TF_LIGHT");
                }
                
                //Set shader variables.
                rayCastMaterial.SetTexture("data3D", volumeComponent.data);
                rayCastMaterial.SetTexture("rayOffset2D", volumeComponent.rayOffset);
                rayCastMaterial.SetColor("dataChannelWeight", volumeComponent.dataChannelWeight);
                rayCastMaterial.SetFloat("leap", _leap);
                rayCastMaterial.SetFloat("maxSamples", (float)volumeComponent.maxSamples);
                rayCastMaterial.SetFloat("focalLength", 1.0f / Mathf.Tan(volumeCam.fieldOfView*0.01745329251f/2.0f));
                rayCastMaterial.SetFloat("nearClipPlane", volumeCam.nearClipPlane);
                rayCastMaterial.SetFloat("opacity", volumeComponent.opacity);
                rayCastMaterial.SetFloat("contrast", 1.1f * (volumeComponent.contrast + 1.0f) / (1.0f * (1.1f - volumeComponent.contrast)));
                rayCastMaterial.SetFloat("brightness", volumeComponent.brightness);
                rayCastMaterial.SetFloat("valueRangeMin", volumeComponent.valueRangeMin);
                rayCastMaterial.SetFloat("valueRangeMax", volumeComponent.valueRangeMax);
                rayCastMaterial.SetFloat("cutValueRangeMin", (volumeComponent.cutValueRangeMin - volumeComponent.valueRangeMin) / (volumeComponent.valueRangeMax - volumeComponent.valueRangeMin));
                rayCastMaterial.SetFloat("cutValueRangeMax", (volumeComponent.cutValueRangeMax - volumeComponent.valueRangeMin) / (volumeComponent.valueRangeMax - volumeComponent.valueRangeMin));
                rayCastMaterial.SetFloat("gradientRangeMin", volumeComponent.gradientRangeMin);
                rayCastMaterial.SetFloat("gradientRangeMax", volumeComponent.gradientRangeMax);
                rayCastMaterial.SetFloat("cutGradientRangeMin", (volumeComponent.cutGradientRangeMin - volumeComponent.gradientRangeMin) / (volumeComponent.gradientRangeMax - volumeComponent.gradientRangeMin));
                rayCastMaterial.SetFloat("cutGradientRangeMax", (volumeComponent.cutGradientRangeMax - volumeComponent.gradientRangeMin) / (volumeComponent.gradientRangeMax - volumeComponent.gradientRangeMin));
                rayCastMaterial.SetFloat("hideZeros", volumeComponent.hideZeros ? 1.0f : 0.0f);
                rayCastMaterial.SetFloat("maxSlices", volumeComponent.maxSlices);

                //Render volume projection.
                volumeCam.targetTexture = volumeProjection;
				volumeComponent.GetComponent<Renderer>().material = rayCastMaterial;
				volumeCam.Render();
				volumeComponent.GetComponent<Renderer>().material = volumePlaceholderMaterial;

                //Put VolumeComponent back onto the default layer.
                volumeComponent.gameObject.layer = 0;

                //Blend volume projection onto the source image.
                blendVolumeMaterial.SetTexture("_VolumeTex", volumeProjection);
                Graphics.Blit(source, destination, blendVolumeMaterial);

                //Overwrite source in case there is another VolumeComponent to render.
                Graphics.Blit(destination, source);

				//Release temporary RenderTexture.
                RenderTexture.ReleaseTemporary(volumeProjection);
            }
            //Release temporary RenderTextures.
            if (_enableDepthTest)
            {
                RenderTexture.ReleaseTemporary(sceneDepthTex);
            }
            if (_enableCulling)
            {
                RenderTexture.ReleaseTemporary(cullDepthFrontTex);
                RenderTexture.ReleaseTemporary(cullDepthBackTex);
            }
            //Copy source image to destination image in case there is no active VolumeComponent.
			if (numActiveVolumes == 0)
            {
                Graphics.Blit(source, destination);
            }
        }

    }
}