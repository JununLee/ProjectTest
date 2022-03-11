///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              VolumeComponent
/// Description:        Representation of a volumetric data-set.
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
    //The MeshFilter's mesh should be a cube.
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class VolumeComponent : MonoBehaviour
    {
        //Should the VolumeComponent be activated in Start()?
        [SerializeField]
        protected bool _activateOnStartup = false;
        public bool activateOnStartup { get { return _activateOnStartup; } set { _activateOnStartup = value; } }

        //Is the VolumeComponent ready to be rendered?
        protected bool _active = false;
        public bool active { get { return _active; } }
        [SerializeField]
        protected BoolEvent _activeChanged = new BoolEvent();
        public BoolEvent activeChanged { get { return _activeChanged; } set { _activeChanged = value; } }

        protected GameObject _gameObject;
        public new GameObject gameObject { get { return _gameObject; } }

        protected Transform _transform;
        public new Transform transform { get { return _transform; } }

        //Image resolution when rendering (divisor for screen width and height).
        //Increasing this value will decrease quality but increase performance.
        [SerializeField]
        [Range(1, 4)]
        protected float _resolution = 1.0f;
        public float resolution { set { if (_resolution == value) { return; } _resolution = value; _resolutionChanged.Invoke(value); } get { return _resolution; } }
        [SerializeField]
        protected FloatEvent _resolutionChanged = new FloatEvent();
        public FloatEvent resolutionChanged { get { return _resolutionChanged; } set { _resolutionChanged = value; } }

        //Maximum number of values that are sampled along the ray.
        //Increasing this value will increase quality but decrease performance.
        [SerializeField]
        protected int _maxSamples = 64;
        public int maxSamples { set { if (_maxSamples == value) { return; } _maxSamples = value; _maxSamplesChanged.Invoke(value); } get { return _maxSamples; } }
        public float maxSamplesAsFloat { set { if (_maxSamples == (int)value) { return; } _maxSamples = (int)value; _maxSamplesChanged.Invoke((int)value); } get { return _maxSamples; } }
        [SerializeField]
        protected FloatEvent _maxSamplesChanged = new FloatEvent();
        public FloatEvent maxSamplesChanged { get { return _maxSamplesChanged; } set { _maxSamplesChanged = value; } }

        //Should be a texture with random noise in channel R.
        //Used to reduce wood grain artefacts.
        [SerializeField]
        protected Texture2D _rayOffset;
        public Texture2D rayOffset { set { if (_rayOffset == value) { return; } _rayOffset = value; _rayOffsetChanged.Invoke(); } get { return _rayOffset; } }
        [SerializeField]
        protected EmptyEvent _rayOffsetChanged = new EmptyEvent();
        public EmptyEvent rayOffsetChanged { get { return _rayOffsetChanged; } set { _rayOffsetChanged = value; } }

        //Volumetric data.
        [SerializeField]
        protected Texture3D _data;
        public Texture3D data { set { if (_data == value) { return; } _data = value; adjustScale(); determineDataType(); _dataChanged.Invoke(); } get { return _data; } }
        [SerializeField]
        protected EmptyEvent _dataChanged = new EmptyEvent();
        public EmptyEvent dataChanged { get { return _dataChanged; } set { _dataChanged = value; } }
        //Dimensions of the volumetric data.
        [SerializeField]
        protected Vector3 _dataDimensions;
        public Vector3 dataDimensions { get { return _dataDimensions; } }
        //max(dataDimensions)
        [SerializeField]
        protected float _maxSlices;
        public float maxSlices { get { return _maxSlices; } }

        //Is volumetric data scalar or RGBA?
        [SerializeField]
        protected VolumeDataType _dataType;
        public VolumeDataType dataType { set { if (_dataType == value) { return; } _dataType = value; _dataTypeChanged.Invoke((int)value); } get { return _dataType; } }
        [SerializeField]
        protected IntEvent _dataTypeChanged = new IntEvent();
        public IntEvent dataTypeChanged { get { return _dataTypeChanged; } set { _dataTypeChanged = value; } }

        //Grid spacing (width, height and depth of a single voxel).
        [SerializeField]
        protected Vector3 _voxelDimensions = Vector3.one;
        public Vector3 voxelDimensions { set { if (_voxelDimensions == value) { return; } _voxelDimensions = value; adjustScale(); _voxelDimensionsChanged.Invoke(value); } get { return _voxelDimensions; } }
        [SerializeField]
        protected Vector3Event _voxelDimensionsChanged = new Vector3Event();
        public Vector3Event voxelDimensionsChanged { get { return _voxelDimensionsChanged; } set { _voxelDimensionsChanged = value; } }

        //A weight for the channels of the volumetric data.
        //Should only be used when dataType is RGBAData.
        [SerializeField]
        protected Color _dataChannelWeight = Color.white;
        public Color dataChannelWeight { set { if (_dataChannelWeight == value) { return; } _dataChannelWeight = value; _dataChannelWeightChanged.Invoke(value); } get { return _dataChannelWeight; } }
        [SerializeField]
        protected ColorEvent _dataChannelWeightChanged = new ColorEvent();
        public ColorEvent dataChannelWeightChanged { get { return _dataChannelWeightChanged; } set { _dataChannelWeightChanged = value; } }

        //Rescale values according to the formula (max(voxel.rgb) - valueRangeMin) / (valueRangeMax - valueRangeMin).
        [SerializeField]
        [Range(0, 1)]
        protected float _valueRangeMin = 0.00f;
        public float valueRangeMin { set { if (Mathf.Approximately(_valueRangeMin, value)) { return; } _valueRangeMin = value; _valueRangeMinChanged.Invoke(value); } get { return _valueRangeMin; } }
        [SerializeField]
        protected FloatEvent _valueRangeMinChanged = new FloatEvent();
        public FloatEvent valueRangeMinChanged { get { return _valueRangeMinChanged; } set { _valueRangeMinChanged = value; } }

        //Rescale values according to the formula (max(voxel.rgb) - valueRangeMin) / (valueRangeMax - valueRangeMin).
        [SerializeField]
        [Range(0, 1)]
        protected float _valueRangeMax = 1.00f;
        public float valueRangeMax { set { if (Mathf.Approximately(_valueRangeMax, value)) { return; } _valueRangeMax = value; _valueRangeMaxChanged.Invoke(value); } get { return _valueRangeMax; } }
        [SerializeField]
        protected FloatEvent _valueRangeMaxChanged = new FloatEvent();
        public FloatEvent valueRangeMaxChanged { get { return _valueRangeMaxChanged; } set { _valueRangeMaxChanged = value; } }

        //Before rescaling, set values below cutValueRangeMin to 0.
        [SerializeField]
        [Range(0, 1)]
        protected float _cutValueRangeMin = 0.00f;
        public float cutValueRangeMin { set { if (Mathf.Approximately(_cutValueRangeMin, value)) { return; } _cutValueRangeMin = value; _cutValueRangeMinChanged.Invoke(value); } get { return _cutValueRangeMin; } }
        [SerializeField]
        protected FloatEvent _cutValueRangeMinChanged = new FloatEvent();
        public FloatEvent cutValueRangeMinChanged { get { return _cutValueRangeMinChanged; } set { _cutValueRangeMinChanged = value; } }

        //Before rescaling, set values above cutValueRangeMax to 0.
        [SerializeField]
        [Range(0, 1)]
        protected float _cutValueRangeMax = 1.00f;
        public float cutValueRangeMax { set { if (Mathf.Approximately(_cutValueRangeMax, value)) { return; } _cutValueRangeMax = value; _cutValueRangeMaxChanged.Invoke(value); } get { return _cutValueRangeMax; } }
        [SerializeField]
        protected FloatEvent _cutValueRangeMaxChanged = new FloatEvent();
        public FloatEvent cutValueRangeMaxChanged { get { return _cutValueRangeMaxChanged; } set { _cutValueRangeMaxChanged = value; } }

        //Transfer function that should be used on the data.
        [SerializeField]
        protected Texture2D _tfData;
        public Texture2D tfData { set { if (_tfData == value) { return; } _tfData = value; _tfDataChanged.Invoke(); } get { return _tfData; } }
        [SerializeField]
        protected EmptyEvent _tfDataChanged = new EmptyEvent();
        public EmptyEvent tfDataChanged { get { return _tfDataChanged; } set { _tfDataChanged = value; } }

        //How should the transfer function interact with the data?
        [SerializeField]
        protected VolumeBlendMode _tfDataBlendMode;
        public VolumeBlendMode tfDataBlendMode { set { if (_tfDataBlendMode == value) { return; } _tfDataBlendMode = value; _tfDataBlendModeChanged.Invoke((int)value); } get { return _tfDataBlendMode; } }
        public int tfDataBlendModeAsInt { set { if (_tfDataBlendMode == (VolumeBlendMode)value) { return; } _tfDataBlendMode = (VolumeBlendMode)value; _tfDataBlendModeChanged.Invoke(value); } get { return (int)_tfDataBlendMode; } }
        [SerializeField]
        protected IntEvent _tfDataBlendModeChanged = new IntEvent();
        public IntEvent tfDataBlendModeChanged { get { return _tfDataBlendModeChanged; } set { _tfDataBlendModeChanged = value; } }

        //Is the second dimension of the transfer function (gradient magnitude) needed?
        //Setting tf2D to false will assume a gradient magnitude of 0 for each voxel.
        //Setting tf2D to true will require calculation of gradient magnitudes for each voxel within the shader. This will decrease performance.
        [SerializeField]
        protected bool _tf2D = false;
        public bool tf2D { set { if (_tf2D == value) { return; } _tf2D = value; _tf2DChanged.Invoke(value); } get { return _tf2D; } }
        [SerializeField]
        protected BoolEvent _tf2DChanged = new BoolEvent();
        public BoolEvent tf2DChanged { get { return _tf2DChanged; } set { _tf2DChanged = value; } }

        //Rescale gradient magnitude values according to the formula (magnitude - gradientRangeMin) / (gradientRangeMax - gradientRangeMin).
        [SerializeField]
        [Range(0, 1)]
        protected float _gradientRangeMin = 0.00f;
        public float gradientRangeMin { set { if (Mathf.Approximately(_gradientRangeMin, value)) { return; } _gradientRangeMin = value; _gradientRangeMinChanged.Invoke(value); } get { return _gradientRangeMin; } }
        [SerializeField]
        protected FloatEvent _gradientRangeMinChanged = new FloatEvent();
        public FloatEvent gradientRangeMinChanged { get { return _gradientRangeMinChanged; } set { _gradientRangeMinChanged = value; } }

        //Rescale gradient magnitude values according to the formula (magnitude - gradientRangeMin) / (gradientRangeMax - gradientRangeMin).
        [SerializeField]
        [Range(0, 1)]
        protected float _gradientRangeMax = 1.00f;
        public float gradientRangeMax { set { if (Mathf.Approximately(_gradientRangeMax, value)) { return; } _gradientRangeMax = value; _gradientRangeMaxChanged.Invoke(value); } get { return _gradientRangeMax; } }
        [SerializeField]
        protected FloatEvent _gradientRangeMaxChanged = new FloatEvent();
        public FloatEvent gradientRangeMaxChanged { get { return _gradientRangeMaxChanged; } set { _gradientRangeMaxChanged = value; } }

        //Before rescaling, set magnitudes below cutGradientRangeMin to 0.
        [SerializeField]
        [Range(0, 1)]
        protected float _cutGradientRangeMin = 0.00f;
        public float cutGradientRangeMin { set { if (Mathf.Approximately(_cutGradientRangeMin, value)) { return; } _cutGradientRangeMin = value; _cutGradientRangeMinChanged.Invoke(value); } get { return _cutGradientRangeMin; } }
        [SerializeField]
        protected FloatEvent _cutGradientRangeMinChanged = new FloatEvent();
        public FloatEvent cutGradientRangeMinChanged { get { return _cutGradientRangeMinChanged; } set { _cutGradientRangeMinChanged = value; } }

        //Before rescaling, set magnitudes above cutGradientRangeMax to 0.
        [SerializeField]
        [Range(0, 1)]
        protected float _cutGradientRangeMax = 1.00f;
        public float cutGradientRangeMax { set { if (Mathf.Approximately(_cutGradientRangeMax, value)) { return; } _cutGradientRangeMax = value; _cutGradientRangeMaxChanged.Invoke(value); } get { return _cutGradientRangeMax; } }
        [SerializeField]
        protected FloatEvent _cutGradientRangeMaxChanged = new FloatEvent();
        public FloatEvent cutGradientRangeMaxChanged { get { return _cutGradientRangeMaxChanged; } set { _cutGradientRangeMaxChanged = value; } }

        //Volumetric overlay (e.g. fMRI ROI).
        //Used to distinguish certain voxels from others by giving them the same value in the volumetric overlay.
        [SerializeField]
        protected Texture3D _overlay;
        public Texture3D overlay { set { if (_overlay == value) { return; } _overlay = value; determineOverlayType(); _overlayChanged.Invoke(); } get { return _overlay; } }
        [SerializeField]
        protected EmptyEvent _overlayChanged = new EmptyEvent();
        public EmptyEvent overlayChanged { get { return _overlayChanged; } set { _overlayChanged = value; } }

        //Is volumetric overlay scalar or RGBA?
        [SerializeField]
        protected VolumeDataType _overlayType;
        public VolumeDataType overlayType { set { if (_overlayType == value) { return; } _overlayType = value; _overlayTypeChanged.Invoke((int)value); } get { return _overlayType; } }
        [SerializeField]
        protected IntEvent _overlayTypeChanged = new IntEvent();
        public IntEvent overlayTypeChanged { get { return _overlayTypeChanged; } set { _overlayTypeChanged = value; } }

        //A weight for the channels of the volumetric overlay.
        //Should only be used when overlayType is RGBAData.
        [SerializeField]
        protected Color _overlayChannelWeight = Color.white;
        public Color overlayChannelWeight { set { if (_overlayChannelWeight == value) { return; } _overlayChannelWeight = value; _overlayChannelWeightChanged.Invoke(value); } get { return _overlayChannelWeight; } }
        [SerializeField]
        protected ColorEvent _overlayChannelWeightChanged = new ColorEvent();
        public ColorEvent overlayChannelWeightChanged { get { return _overlayChannelWeightChanged; } set { _overlayChannelWeightChanged = value; } }

        //How should the overlay interact with the data?
        [SerializeField]
        protected VolumeBlendMode _overlayBlendMode;
        public VolumeBlendMode overlayBlendMode { set { if (_overlayBlendMode == value) { return; } _overlayBlendMode = value; _overlayBlendModeChanged.Invoke((int)value); } get { return _overlayBlendMode; } }
        public int overlayBlendModeAsInt { set { if (_overlayBlendMode == (VolumeBlendMode)value) { return; } _overlayBlendMode = (VolumeBlendMode)value; _overlayBlendModeChanged.Invoke(value); } get { return (int)_overlayBlendMode; } }
        [SerializeField]
        protected IntEvent _overlayBlendModeChanged = new IntEvent();
        public IntEvent overlayBlendModeChanged { get { return _overlayBlendModeChanged; } set { _overlayBlendModeChanged = value; } }

        //Should a visible voxel of the overlay be prevented from being culled?
        [SerializeField]
        protected bool _overlayVoidsCulling = false;
        public bool overlayVoidsCulling { set { if (_overlayVoidsCulling == value) { return; } _overlayVoidsCulling = value; _overlayVoidsCullingChanged.Invoke(value); } get { return _overlayVoidsCulling; } }
        [SerializeField]
        protected BoolEvent _overlayVoidsCullingChanged = new BoolEvent();
        public BoolEvent overlayVoidsCullingChanged { get { return _overlayVoidsCullingChanged; } set { _overlayVoidsCullingChanged = value; } }

        //Transfer function that should be used on the overlay.
        [SerializeField]
        protected Texture2D _tfOverlay;
        public Texture2D tfOverlay { set { if (_tfOverlay == value) { return; } _tfOverlay = value; _tfOverlayChanged.Invoke(); } get { return _tfOverlay; } }
        [SerializeField]
        protected EmptyEvent _tfOverlayChanged = new EmptyEvent();
        public EmptyEvent tfOverlayChanged { get { return _tfOverlayChanged; } set { _tfOverlayChanged = value; } }

        //How should the transfer function interact with the overlay?
        [SerializeField]
        protected VolumeBlendMode _tfOverlayBlendMode;
        public VolumeBlendMode tfOverlayBlendMode { set { if (_tfOverlayBlendMode == value) { return; } _tfOverlayBlendMode = value; _tfOverlayBlendModeChanged.Invoke((int)value); } get { return _tfOverlayBlendMode; } }
        public int tfOverlayBlendModeAsInt { set { if (_tfOverlayBlendMode == (VolumeBlendMode)value) { return; } _tfOverlayBlendMode = (VolumeBlendMode)value; _tfOverlayBlendModeChanged.Invoke(value); } get { return (int)_tfOverlayBlendMode; } }
        [SerializeField]
        protected IntEvent _tfOverlayBlendModeChanged = new IntEvent();
        public IntEvent tfOverlayBlendModeChanged { get { return _tfOverlayBlendModeChanged; } set { _tfOverlayBlendModeChanged = value; } }

        //Change the VolumeComponent's contrast according to the formulas:
        //contrastValue = 1.1 * (contrast + 1.0) / (1.0 * (1.1 - contrast))
        //rgb = contrastValue * (rgb - 0.5) + 0.5 + brightness
        [SerializeField]
        [Range(-1, 1)]
        protected float _contrast = 0;
        public float contrast { set { if (Mathf.Approximately(_contrast, value)) { return; } _contrast = value; _contrastChanged.Invoke(value); } get { return _contrast; } }
		[SerializeField]
		protected FloatEvent _contrastChanged = new FloatEvent();
		public FloatEvent contrastChanged { get { return _contrastChanged; } set { _contrastChanged = value; } }

        //Change the VolumeComponent's brightness according to the formula rgb = contrastValue * (rgb - 0.5) + 0.5 + brightness.
        [SerializeField]
        [Range(-1, 1)]
        protected float _brightness = 0;
        public float brightness { set { if (Mathf.Approximately(_brightness, value)) { return; } _brightness = value; _brightnessChanged.Invoke(value); } get { return _brightness; } }
		[SerializeField]
		protected FloatEvent _brightnessChanged = new FloatEvent();
		public FloatEvent brightnessChanged { get { return _brightnessChanged; } set { _brightnessChanged = value; } }

        //Change the VolumeComponent's opacity.
        [SerializeField]
        [Range(0, 1)]
        protected float _opacity = 1.0f;
        public float opacity { set { if (Mathf.Approximately(_opacity, value)) { return; } _opacity = value; _opacityChanged.Invoke(value); } get { return _opacity; } }
		[SerializeField]
		protected FloatEvent _opacityChanged = new FloatEvent();
		public FloatEvent opacityChanged { get { return _opacityChanged; } set { _opacityChanged = value; } }

        //Enable interaction with a directional light.
        [SerializeField]
        protected bool _enableLight = false;
        public bool enableLight { set { if (_enableLight == value) { return; } _enableLight = value; _enableLightChanged.Invoke(value); } get { return _enableLight; } }
        [SerializeField]
        protected BoolEvent _enableLightChanged = new BoolEvent();
        public BoolEvent enableLightChanged { get { return _enableLightChanged; } set { _enableLightChanged = value; } }

        //Threshold value to determine the iso surface that shall interact with light.
        [SerializeField]
        [Range(0, 1)]
        protected float _surfaceThr = 0;
        public float surfaceThr { set { if (Mathf.Approximately(_surfaceThr, value)) { return; } _surfaceThr = value; _surfaceThrChanged.Invoke(value); } get { return _surfaceThr; } }
        [SerializeField]
        protected FloatEvent _surfaceThrChanged = new FloatEvent();
        public FloatEvent surfaceThrChanged { get { return _surfaceThrChanged; } set { _surfaceThrChanged = value; } }

        //Alpha value of the iso surface and its interaction with light.
        [SerializeField]
        [Range(0, 1)]
        protected float _surfaceAlpha = 0.5f;
        public float surfaceAlpha { set { if (Mathf.Approximately(_surfaceAlpha, value)) { return; } _surfaceAlpha = value; _surfaceAlphaChanged.Invoke(value); } get { return _surfaceAlpha; } }
        [SerializeField]
        protected FloatEvent _surfaceAlphaChanged = new FloatEvent();
        public FloatEvent surfaceAlphaChanged { get { return _surfaceAlphaChanged; } set { _surfaceAlphaChanged = value; } }
        
        [SerializeField]
        protected Color _ambientColor = new Color(0.1f, 0.1f, 0.1f, 1.0f);
        public Color ambientColor { set { if (_ambientColor == value) { return; } _ambientColor = value; _ambientColorChanged.Invoke(value); } get { return _ambientColor; } }
        [SerializeField]
        protected ColorEvent _ambientColorChanged = new ColorEvent();
        public ColorEvent ambientColorChanged { get { return _ambientColorChanged; } set { _ambientColorChanged = value; } }

        [SerializeField]
        protected Color _diffuseColor = new Color(0.6f, 0.6f, 0.6f, 1.0f);
        public Color diffuseColor { set { if (_diffuseColor == value) { return; } _diffuseColor = value; _diffuseColorChanged.Invoke(value); } get { return _diffuseColor; } }
        [SerializeField]
        protected ColorEvent _diffuseColorChanged = new ColorEvent();
        public ColorEvent diffuseColorChanged { get { return _diffuseColorChanged; } set { _diffuseColorChanged = value; } }

        [SerializeField]
        protected Color _specularColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        public Color specularColor { set { if (_specularColor == value) { return; } _specularColor = value; _specularColorChanged.Invoke(value); } get { return _specularColor; } }
        [SerializeField]
        protected ColorEvent _specularColorChanged = new ColorEvent();
        public ColorEvent specularColorChanged { get { return _specularColorChanged; } set { _specularColorChanged = value; } }

        [SerializeField]
        [Range(4, 100)]
        protected float _shininess = 4;
        public float shininess { set { if (Mathf.Approximately(_shininess, value)) { return; } _shininess = value; _shininessChanged.Invoke(value); } get { return _shininess; } }
        [SerializeField]
        protected FloatEvent _shininessChanged = new FloatEvent();
        public FloatEvent shininessChanged { get { return _shininessChanged; } set { _shininessChanged = value; } }

        //If surfaceAlpha is below 1, more than 1 iso surface could be deteted along the ray.
        //How many iso surfaces should be shaded along the ray?
        //Increasing this value will decrease performance.
        [SerializeField]
        protected int _maxShadedSamples = 1;
        public int maxShadedSamples { set { if (_maxShadedSamples == value) { return; } _maxShadedSamples = value; _maxShadedSamplesChanged.Invoke(value); } get { return _maxShadedSamples; } }
        [SerializeField]
        protected IntEvent _maxShadedSamplesChanged = new IntEvent();
        public IntEvent maxShadedSamplesChanged { get { return _maxShadedSamplesChanged; } set { _maxShadedSamplesChanged = value; } }

        //Determines the smoothness of the gradient used for light interaction.
        //Increasing this value will increase visual quality but decrease performance.
        [SerializeField]
        protected VolumeGradientFetches _surfaceGradientFetches = VolumeGradientFetches._3;
        public VolumeGradientFetches surfaceGradientFetches { set { if (_surfaceGradientFetches == value) { return; } _surfaceGradientFetches = value; _surfaceGradientFetchesChanged.Invoke((int)value); } get { return _surfaceGradientFetches; } }
        public int surfaceGradientFetchesAsInt { set { if (_surfaceGradientFetches == (VolumeGradientFetches)value) { return; } _surfaceGradientFetches = (VolumeGradientFetches)value; _surfaceGradientFetchesChanged.Invoke(value); } get { return (int)_surfaceGradientFetches; } }
        [SerializeField]
        protected IntEvent _surfaceGradientFetchesChanged = new IntEvent();
        public IntEvent surfaceGradientFetchesChanged { get { return _surfaceGradientFetchesChanged; } set { _surfaceGradientFetchesChanged = value; } }

        //Instead of using the Blinn-Phong lighting model, use a custom-lighting transfer function.
        //n*l vs n*h.
        [SerializeField]
        protected Texture2D _tfLight;
        public Texture2D tfLight { set { if (_tfLight == value) { return; } _tfLight = value; _tfLightChanged.Invoke(); } get { return _tfLight; } }
        [SerializeField]
        protected EmptyEvent _tfLightChanged = new EmptyEvent();
        public EmptyEvent tfLightChanged { get { return _tfLightChanged; } set { _tfLightChanged = value; } }

        //Set the alpha of values that are 0.0 to 0.0.
        [SerializeField]
        protected bool _hideZeros = true;
        public bool hideZeros { set { if (_hideZeros == value) { return; } _hideZeros = value; _hideZerosChanged.Invoke(value); } get { return _hideZeros; } }
        [SerializeField]
        protected BoolEvent _hideZerosChanged = new BoolEvent();
        public BoolEvent hideZerosChanged { get { return _hideZerosChanged; } set { _hideZerosChanged = value; } }

        //Invert the culling effect of objects with a CullingMaterial on them.
        [SerializeField]
        protected bool _invertCulling = false;
        public bool invertCulling { set { if (_invertCulling == value) { return; } _invertCulling = value; _invertCullingChanged.Invoke(value); } get { return _invertCulling; } }
        [SerializeField]
        protected BoolEvent _invertCullingChanged = new BoolEvent();
        public BoolEvent invertCullingChanged { get { return _invertCullingChanged; } set { _invertCullingChanged = value; } }

        void Awake()
        {
            _transform = base.transform;
			_gameObject = base.gameObject;
        }

        void Start()
        {
            if (_activateOnStartup)
            {
                activate();
            }
        }

        //Declare this VolumeComponent active for rendering.
        public int activate()
        {
            if(_data == null)
            {
                Debug.Log("Activation of " + _gameObject.name + " failed. Volume data is null.");
                return 0;
            }

            //Adjust the local scale.
            adjustScale();

            //The last thing to do before the VolumeComponent is active is to change its material to VolumePlaceholder.
            GetComponent<Renderer>().material = new Material(Shader.Find("Hidden/VolumeViewer/VolumePlaceholder"));

            _active = true;
            activeChanged.Invoke(_active);

            return 1;
        }
        
        //Transform local scale based on the edge length of a single voxel and the number of voxels in each dimension in a way
        //  that the local x scale stays as assigned in the inspector, while the y and z scale are adjusted to create the correct ratios.
        public void adjustScale()
		{
            if (_data == null)
            {
                _dataDimensions = Vector3.zero;
                _maxSlices = 0;
                return;
            }
            if(_transform == null)
            {
                _transform = base.transform;
                _gameObject = base.gameObject;
            }
			_dataDimensions = new Vector3( _data.width, _data.height, _data.depth );
			_maxSlices = _dataDimensions.x > _dataDimensions.y ? _dataDimensions.x : _dataDimensions.y;
			_maxSlices = _maxSlices > _dataDimensions.z ? _maxSlices : _dataDimensions.z;
            float xDim = transform.localScale.x;
            transform.localScale = new Vector3(xDim, _voxelDimensions.y * xDim / _voxelDimensions.x * _dataDimensions[1] / _dataDimensions[0], _voxelDimensions.z * xDim / _voxelDimensions.x * _dataDimensions[2] / _dataDimensions[0]);
        }

        public void determineDataType()
        {
            if (_data == null)
            {
                _dataType = VolumeDataType.ScalarData;
                return;
            }
            switch (_data.format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.RFloat:
                case TextureFormat.RHalf:
                    _dataType = VolumeDataType.ScalarData;
                    break;
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    _dataType = VolumeDataType.RGBAData;
                    break;
            }
        }

        public void determineOverlayType()
        {
            switch (_overlay.format)
            {
                case TextureFormat.Alpha8:
                case TextureFormat.RFloat:
                case TextureFormat.RHalf:
                    _overlayType = VolumeDataType.ScalarData;
                    break;
                case TextureFormat.RGB24:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.RGBAFloat:
                case TextureFormat.RGBAHalf:
                    _overlayType = VolumeDataType.RGBAData;
                    break;
            }
        }

#if UNITY_EDITOR
        //Once the VolumeComponent is active, it is no longer rendered in the scene view.
        //So render a blue mesh instead.
        protected void OnDrawGizmos()
        {
            if (active)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawMesh(GetComponent<MeshFilter>().sharedMesh, transform.position, transform.rotation, transform.lossyScale);
            }
        }
#endif

    }

}