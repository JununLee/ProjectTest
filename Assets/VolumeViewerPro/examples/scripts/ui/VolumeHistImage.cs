////--------------------------------------------------------------------
/// Namespace:          
/// Class:              VolumeHistImage
/// Description:        Creates a histogram of a VolumeComponent's data
///                         and changes the uvRect of a RawImage (or
///                         obstructs part of said RawImage), to
///                         reflect normalization parameters of that
///                         data.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the examples of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
////--------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class VolumeHistImage : MonoBehaviour
{
    float _cutValueRangeMin = 0.00f;
    public float cutValueRangeMin { set { _cutValueRangeMin = value; UpdateImages(); } get { return _cutValueRangeMin; } }
    float _cutValueRangeMax = 1.00f;
    public float cutValueRangeMax { set { _cutValueRangeMax = value; UpdateImages(); } get { return _cutValueRangeMax; } }
    float _cutGradientRangeMin = 0.00f;
    public float cutGradientRangeMin { set { _cutGradientRangeMin = value; UpdateImages(); } get { return _cutGradientRangeMin; } }
    float _cutGradientRangeMax = 1.00f;
    public float cutGradientRangeMax { set { _cutGradientRangeMax = value; UpdateImages(); } get { return _cutGradientRangeMax; } }
    float _valueRangeMin = 0.00f;
    public float valueRangeMin { set { _valueRangeMin = value; UpdateImages(); } get { return _valueRangeMin; } }
    float _valueRangeMax = 1.00f;
    public float valueRangeMax { set { _valueRangeMax = value; UpdateImages(); } get { return _valueRangeMax; } }
    float _gradientRangeMin = 0.00f;
    public float gradientRangeMin { set { _gradientRangeMin = value; UpdateImages(); } get { return _gradientRangeMin; } }
    float _gradientRangeMax = 1.0f;
    public float gradientRangeMax { set { _gradientRangeMax = value; UpdateImages(); } get { return _gradientRangeMax; } }
    
    public VolumeViewer.VolumeComponent volumeComponent;
    public RawImage grayMinImage;
    public RawImage grayMaxImage;
    public RawImage gradMinImage;
    public RawImage gradMaxImage;
    public RawImage histImage;
    public int imageDimension = 256;
    public Color[] histColorMap = { new Color(0, 0, 0), new Color(0, 0, 1), new Color(0, 1, 1), new Color(0, 1, 0), new Color(1, 1, 0), new Color(1, 0, 0), new Color(1, 0, 1) };
    public Color histBGColor = new Color(1, 1, 1);
    public Texture2D histTexture;

    public void triggerGenerateHistImage()
    {
        StartCoroutine(generateHistImage());
    }

    IEnumerator generateHistImage()
    {
        while(!volumeComponent.active)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Color[] histColors;
        float[,] hist2D;
        float[,,] grayscale;
        float[,,] gradient;
        int nx = (int)volumeComponent.dataDimensions.x;
        int ny = (int)volumeComponent.dataDimensions.y;
        int nz = (int)volumeComponent.dataDimensions.z;
        VolumeViewer.Ref<float[,,]> gray = new VolumeViewer.Ref<float[,,]>();
        VolumeViewer.Ref<float[,,]> grad = new VolumeViewer.Ref<float[,,]>();
        VolumeViewer.Ref<float[,]> hist = new VolumeViewer.Ref<float[,]>();
        VolumeViewer.Ref<bool> ended = new VolumeViewer.Ref<bool>();
        yield return StartCoroutine(VolumeHelper.getGrayscaleFromVolume(volumeComponent.data, gray, volumeComponent.dataChannelWeight, ended));
        grayscale = gray.val;
        if(grayscale == null)
        {
            yield break;
        }
        yield return StartCoroutine(VolumeHelper.getGradientFromGrayscale(grayscale, grad, volumeComponent.dataChannelWeight, ended));
        gradient = grad.val;
        if (gradient == null)
        {
            yield break;
        }
        yield return StartCoroutine(VolumeHelper.generateHistogramFromVolume(grayscale, gradient, imageDimension, imageDimension, hist, ended));
        hist2D = hist.val;
        if (hist2D == null)
        {
            yield break;
        }
        VolumeHelper.capOffHistogram(ref hist2D, VolumeHelper.estimateCap(hist2D, nx*ny*nz, 0.5f));
        yield return null;
        VolumeHelper.normalizeHistogram(ref hist2D);
        yield return null;
        VolumeHelper.mapGrayscaleHistogram2Color(hist2D, histColorMap, histBGColor, out histColors);
        yield return null;
        histTexture = new Texture2D(imageDimension, imageDimension);
        histTexture.SetPixels(histColors);
        histTexture.Apply();
        histTexture.wrapMode = TextureWrapMode.Clamp;
        histImage.texture = histTexture;
        histImage.texture.wrapMode = TextureWrapMode.Clamp;
    }
    
    void UpdateImages()
    {
        float _cutValueRangeMinScaled = Mathf.Clamp01((_cutValueRangeMin - _valueRangeMin) / (_valueRangeMax - _valueRangeMin));
        float _cutValueRangeMaxScaled = Mathf.Clamp01((_cutValueRangeMax - _valueRangeMin) / (_valueRangeMax - _valueRangeMin));
        float _cutGradientRangeMinScaled = Mathf.Clamp01((_cutGradientRangeMin - _gradientRangeMin) / (_gradientRangeMax - _gradientRangeMin));
        float _cutGradientRangeMaxScaled = Mathf.Clamp01((_cutGradientRangeMax - _gradientRangeMin) / (_gradientRangeMax - _gradientRangeMin));

        Rect histImageRect = histImage.rectTransform.rect;
        Rect histImageUVRect = histImage.uvRect;

        Vector2 grayMinImageSize = grayMinImage.rectTransform.sizeDelta;
        grayMinImageSize.x = histImageRect.width * _cutValueRangeMinScaled;
        grayMinImage.rectTransform.sizeDelta = grayMinImageSize;
        
        Vector2 grayMaxImageSize = grayMaxImage.rectTransform.sizeDelta;
        grayMaxImageSize.x = histImageRect.width * (1.0f - _cutValueRangeMaxScaled);
        grayMaxImage.rectTransform.sizeDelta = grayMaxImageSize;
        
        Vector2 gradMinImageSize = gradMinImage.rectTransform.sizeDelta;
        gradMinImageSize.y = histImageRect.height * _cutGradientRangeMinScaled;
        gradMinImage.rectTransform.sizeDelta = gradMinImageSize;
        
        Vector2 gradMaxImageSize = gradMaxImage.rectTransform.sizeDelta;
        gradMaxImageSize.y = histImageRect.height * (1.0f - _cutGradientRangeMaxScaled);
        gradMaxImage.rectTransform.sizeDelta = gradMaxImageSize;
        
        histImageUVRect.x = _valueRangeMin;
        histImageUVRect.y = _gradientRangeMin;
        histImageUVRect.width = _valueRangeMax - _valueRangeMin;
        histImageUVRect.height = _gradientRangeMax - _gradientRangeMin;
        histImage.uvRect = histImageUVRect;

    }

}
