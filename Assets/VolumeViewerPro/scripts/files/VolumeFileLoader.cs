///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeFileLoader
/// Description:        Loading volumetric data-sets of various 
///                         types into a VolumeComponent.
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

using System.Collections;
using UnityEngine;
#if !NETFX_CORE
using System.IO;
#endif

namespace VolumeViewer
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(VolumeComponent))]
    public class VolumeFileLoader : MonoBehaviour
    {
        //Start loading in Start().
        [SerializeField]
        private bool loadOnStartup = true;

        //Path to the file that contains the volumetric data.
        [SerializeField]
        private string _dataPath = "Assets/";
        public string dataPath { set { if (_dataPath.Equals(value)) { return; } _dataPath = value; _dataPathChanged.Invoke(value); } get { return _dataPath; } }
        [SerializeField]
		private StringEvent _dataPathChanged = new StringEvent();
		public StringEvent dataPathChanged { get { return _dataPathChanged; } set { _dataPathChanged = value; } }

        //Force a particular data format.
        [SerializeField]
        private VolumeTextureFormat _forceDataFormat = VolumeTextureFormat.Native;
        public VolumeTextureFormat forceDataFormat { get { return _forceDataFormat; } set { _forceDataFormat = value; } }

        //Force a particular planar configuration in case the data is RGBA.
        //If the correct planar configuration isn't inferred from the header, this can be used to load color channels in the desired order.
        //You should use this if you see duplicates (3 or 6) of your data.
        [SerializeField]
        private PlanarConfiguration _forceDataPlanarConfiguration = PlanarConfiguration.Native;
        public PlanarConfiguration forceDataPlanarConfiguration { get { return _forceDataPlanarConfiguration; } set { _forceDataPlanarConfiguration = value; } }

        //Path to the file that contains the volumetric overlay.
        [SerializeField]
        private string _overlayPath;
        public string overlayPath { set { if (_overlayPath.Equals(value)) { return; } _overlayPath = value; _overlayPathChanged.Invoke(value); } get { return _overlayPath; } }
        [SerializeField]
        private StringEvent _overlayPathChanged = new StringEvent();
        public StringEvent overlayPathChanged { get { return _overlayPathChanged; } set { _overlayPathChanged = value; } }

        //Force a particular overlay format.
        [SerializeField]
        private VolumeTextureFormat _forceOverlayFormat = VolumeTextureFormat.Native;
        public VolumeTextureFormat forceOverlayFormat { get { return _forceOverlayFormat; } set { _forceOverlayFormat = value; } }

        //Force a particular planar configuration in case the overlay is RGBA.
        //If the correct planar configuration isn't inferred from the header, this can be used to load color channels in the desired order.
        //You should use this if you see duplicates (3 or 6) of your overlay.
        [SerializeField]
        private PlanarConfiguration _forceOverlayPlanarConfiguration = PlanarConfiguration.Native;
        public PlanarConfiguration forceOverlayPlanarConfiguration { get { return _forceOverlayPlanarConfiguration; } set { _forceOverlayPlanarConfiguration = value; } }

        //This event will send the current progress (between 0 and 1) of loading the data (and if present, the overlay) to subscribers.
        [SerializeField]
        private FloatEvent _loadingProgressChanged = new FloatEvent();
        public FloatEvent loadingProgressChanged { get { return _loadingProgressChanged; } set { _loadingProgressChanged = value; } }

        //A reference to a VolumeComponentAsset (cache) that should be loaded instead of dataPath or overlayPath.
        [SerializeField]
        private VolumeComponentAsset _loadAssetInstead;
        public VolumeComponentAsset loadAssetInstead { get { return _loadAssetInstead; } set { _loadAssetInstead = value;  } }

        private VolumeComponent volumeComponent;
        public Volume dataVolume;
        private Volume overlayVolume;

        void Awake()
        {
            volumeComponent = GetComponent<VolumeComponent>();
        }
        
        void Start()
        {
            if(loadOnStartup)
            {
                StartCoroutine(loadFiles(_dataPath));
            }
        }

        public IEnumerator loadFiles(string _dataPath)
        {
            if(_loadAssetInstead != null)
            {
                assignAssetToComponent();
                yield break;
            }
#if !NETFX_CORE
            if (!File.Exists(_dataPath))
            {
                Debug.Log("Error: File doesn't exist: " + _dataPath);
                changeTextureColor(Color.red);
                yield break;
            }
            Ref<bool> ended = new Ref<bool>();
            Ref<int> result = new Ref<int>();
            FloatEvent loadingDataProgressChanged = new FloatEvent();
            loadingDataProgressChanged.AddListener(forwardProgressData);
            if (NIfTI.isNIFTI(_dataPath))
            {
                NII2Volume niiLoader = new NII2Volume();
                yield return StartCoroutine(niiLoader.loadFile(_dataPath, _forceDataFormat, loadingDataProgressChanged, _forceDataPlanarConfiguration, ended, result));
                dataVolume = niiLoader.volume;
                Brush.info = dataVolume;

                //transform.rotation = new Quaternion(dataVolume.niiInfo.header.quatern_b, dataVolume.niiInfo.header.quatern_c, dataVolume.niiInfo.header.quatern_d, Mathf.Sqrt(1 - (dataVolume.niiInfo.header.quatern_b * dataVolume.niiInfo.header.quatern_b + dataVolume.niiInfo.header.quatern_c * dataVolume.niiInfo.header.quatern_c + dataVolume.niiInfo.header.quatern_d * dataVolume.niiInfo.header.quatern_d)));

                //transform.position = new Vector3(dataVolume.niiInfo.header.qoffset_x / 1000, dataVolume.niiInfo.header.qoffset_y / 1000, dataVolume.niiInfo.header.qoffset_z / 1000);
                //transform.position += transform.right * dataVolume.niiInfo.dx * dataVolume.niiInfo.nx / 2 / 1000;
                //transform.position += transform.up * dataVolume.niiInfo.dy * dataVolume.niiInfo.ny / 2 / 1000;
                //transform.position += transform.forward * dataVolume.niiInfo.dz * dataVolume.niiInfo.nz / 2 / 1000;

                //transform.localScale = new Vector3(dataVolume.dx * dataVolume.nx / 1000, dataVolume.dy * dataVolume.ny / 1000, dataVolume.nz * dataVolume.dz / 1000);





            }
            else if(DICOM.isDICOM(_dataPath))
            {
                DCM2Volume dcmLoader = new DCM2Volume();
                yield return StartCoroutine(dcmLoader.loadFile(_dataPath, _forceDataFormat, loadingDataProgressChanged, _forceDataPlanarConfiguration, ended, result));
                dataVolume = dcmLoader.volume;
            }
            if(dataVolume==null)
            {
                PNG2Volume pngLoader = new PNG2Volume();
                yield return StartCoroutine(pngLoader.loadFile(_dataPath, _forceDataFormat, loadingDataProgressChanged, ended, result));
                dataVolume = pngLoader.volume;
            }
            if (result.val < 1)
            {
                Debug.Log("Unable to load files: " + _dataPath);
                changeTextureColor(Color.red);
                yield break;
            }
            
            switch(dataVolume.format)
            {
                case TextureFormat.Alpha8:
                    volumeComponent.dataChannelWeight = new Color(0, 0, 0, 1);
                    break;
                case TextureFormat.RFloat:
                case TextureFormat.RHalf:
                    volumeComponent.dataChannelWeight = new Color(1, 0, 0, 0);
                    break;
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    break;
                default:
                    Debug.Log("TextureFormat not supported: " + dataVolume.format);
                    changeTextureColor(Color.red);
                    yield break;
            }
			volumeComponent.data = dataVolume.texture;
			volumeComponent.voxelDimensions = new Vector3(dataVolume.dx, dataVolume.dy, dataVolume.dz);

            if(_overlayPath.Length > 0)
            {
                if (!File.Exists(_overlayPath))
                {
                    Debug.Log("Error: File doesn't exist: " + _overlayPath);
                    changeTextureColor(Color.magenta);
                    yield break;
                }
                FloatEvent loadingOverlayProgressChanged = new FloatEvent();
                loadingOverlayProgressChanged.AddListener(forwardProgressOverlay);
                if (NIfTI.isNIFTI(_overlayPath))
                {
                    NII2Volume niiLoader = new NII2Volume();
                    yield return StartCoroutine(niiLoader.loadFile(_overlayPath, _forceOverlayFormat, loadingOverlayProgressChanged, _forceOverlayPlanarConfiguration, ended, result));
                    overlayVolume = niiLoader.volume;
                }
                else if (DICOM.isDICOM(_overlayPath))
                {
                    DCM2Volume dcmLoader = new DCM2Volume();
                    yield return StartCoroutine(dcmLoader.loadFile(_overlayPath, _forceOverlayFormat, loadingOverlayProgressChanged, _forceOverlayPlanarConfiguration, ended, result));
                    overlayVolume = dcmLoader.volume;
                }
                if (overlayVolume == null)
                {
                    PNG2Volume pngLoader = new PNG2Volume();
                    yield return StartCoroutine(pngLoader.loadFile(_overlayPath, _forceOverlayFormat, loadingOverlayProgressChanged, ended, result));
                    overlayVolume = pngLoader.volume;
                }
                if (result.val < 1)
                {
                    Debug.Log("Unable to load files: " + _overlayPath);
                    changeTextureColor(Color.magenta);
                    yield break;
                }
                
                switch (overlayVolume.format)
                {
                    case TextureFormat.Alpha8:
                        volumeComponent.overlayChannelWeight = new Color(0, 0, 0, 1);
                        break;
                    case TextureFormat.RFloat:
                    case TextureFormat.RHalf:
                        volumeComponent.overlayChannelWeight = new Color(1, 0, 0, 0);
                        break;
                    case TextureFormat.RGB24:
                    case TextureFormat.RGBA32:
                    case TextureFormat.ARGB32:
                    case TextureFormat.RGBAFloat:
                    case TextureFormat.RGBAHalf:
                        break;
                    default:
                        Debug.Log("TextureFormat not supported: " + overlayVolume.format);
                        changeTextureColor(Color.magenta);
                        yield break;
                }
                overlayVolume.texture.filterMode = FilterMode.Point;
                volumeComponent.overlay = overlayVolume.texture;
            }
			volumeComponent.activate();
            yield return null;
#endif
        }

        void assignAssetToComponent()
        {
            if (_loadAssetInstead != null)
            {
                volumeComponent.ambientColor = _loadAssetInstead.ambientColor;
                volumeComponent.brightness = _loadAssetInstead.brightness;
                volumeComponent.contrast = _loadAssetInstead.contrast;
                volumeComponent.cutGradientRangeMax = _loadAssetInstead.cutGradientRangeMax;
                volumeComponent.cutGradientRangeMin = _loadAssetInstead.cutGradientRangeMin;
                volumeComponent.cutValueRangeMax = _loadAssetInstead.cutValueRangeMax;
                volumeComponent.cutValueRangeMin = _loadAssetInstead.cutValueRangeMin;
                volumeComponent.data = _loadAssetInstead.data;
                volumeComponent.dataChannelWeight = _loadAssetInstead.dataChannelWeight;
                volumeComponent.dataType = _loadAssetInstead.dataType;
                volumeComponent.diffuseColor = _loadAssetInstead.diffuseColor;
                volumeComponent.enableLight = _loadAssetInstead.enableLight;
                volumeComponent.gradientRangeMax = _loadAssetInstead.gradientRangeMax;
                volumeComponent.gradientRangeMin = _loadAssetInstead.gradientRangeMin;
                volumeComponent.hideZeros = _loadAssetInstead.hideZeros;
                volumeComponent.invertCulling = _loadAssetInstead.invertCulling;
                volumeComponent.maxSamples = _loadAssetInstead.maxSamples;
                volumeComponent.maxShadedSamples = _loadAssetInstead.maxShadedSamples;
                volumeComponent.opacity = _loadAssetInstead.opacity;
                volumeComponent.overlay = _loadAssetInstead.overlay;
                volumeComponent.overlayBlendMode = _loadAssetInstead.overlayBlendMode;
                volumeComponent.overlayChannelWeight = _loadAssetInstead.overlayChannelWeight;
                volumeComponent.overlayVoidsCulling = _loadAssetInstead.overlayVoidsCulling;
                volumeComponent.overlayType = _loadAssetInstead.overlayType;
                volumeComponent.rayOffset = _loadAssetInstead.rayOffset;
                volumeComponent.resolution = _loadAssetInstead.resolution;
                volumeComponent.shininess = _loadAssetInstead.shininess;
                volumeComponent.specularColor = _loadAssetInstead.specularColor;
                volumeComponent.surfaceAlpha = _loadAssetInstead.surfaceAlpha;
                volumeComponent.surfaceGradientFetches = _loadAssetInstead.surfaceGradientFetches;
                volumeComponent.surfaceThr = _loadAssetInstead.surfaceThr;
                volumeComponent.tf2D = _loadAssetInstead.tf2D;
                volumeComponent.tfData = _loadAssetInstead.tfData;
                volumeComponent.tfDataBlendMode = _loadAssetInstead.tfDataBlendMode;
                volumeComponent.tfLight = _loadAssetInstead.tfLight;
                volumeComponent.tfOverlay = _loadAssetInstead.tfOverlay;
                volumeComponent.tfOverlayBlendMode = _loadAssetInstead.tfOverlayBlendMode;
                volumeComponent.valueRangeMax = _loadAssetInstead.valueRangeMax;
                volumeComponent.valueRangeMin = _loadAssetInstead.valueRangeMin;
                volumeComponent.voxelDimensions = _loadAssetInstead.voxelDimensions;
                volumeComponent.activate();
                loadingProgressChanged.Invoke(1.0f);
            }
        }

        void forwardProgressData(float progress)
        {
            if (_overlayPath.Length > 0)
            {
                progress *= 0.5f;
            }
            loadingProgressChanged.Invoke(progress);
        }

        void forwardProgressOverlay(float progress)
        {
            progress *= 0.5f;
            progress += 0.5f;
            loadingProgressChanged.Invoke(progress);
        }

        void changeTextureColor(Color newColor)
        {
            Renderer thisRenderer = GetComponent<Renderer>();
            if(thisRenderer != null)
            {
                thisRenderer.material.color = newColor;
            }
        }
    }
}