////--------------------------------------------------------------------
/// Namespace:          
/// Class:              ProceduralExample
/// Description:        Example of procedurally creating volumetric
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

using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(VolumeViewer.VolumeComponent))]
public class ProceduralExample : MonoBehaviour
{
    public float minValue = 0.25f;
    public float maxValue = 0.425f;
    [SerializeField]
    private VolumeViewer.FloatEvent _loadingProgressChanged = new VolumeViewer.FloatEvent();
    public VolumeViewer.FloatEvent loadingProgressChanged { get { return _loadingProgressChanged; } set { _loadingProgressChanged = value; } }

    VolumeViewer.VolumeComponent volumeComponent;
    Texture3D volumeTexture;

    //Initialization
    IEnumerator Start()
    {
        yield return StartCoroutine(generateTexture());
        volumeComponent = GetComponent<VolumeViewer.VolumeComponent>();
        volumeComponent.data = volumeTexture;
        volumeComponent.activate();
        yield return StartCoroutine(UpdateAnimation());
    }

    public IEnumerator generateTexture()
    {
        //Voxel dimension lengths.
        int nx = 128;
        int ny = 128;
        int nz = 128;
        int numVox = nx * ny * nz;

        //Allocate texture.
        volumeTexture = new Texture3D(nx, ny, nz, TextureFormat.ARGB32, false);

        //Allocate color array.
        Color[] texColor = new Color[numVox];

        //Noise modifiers.
        float noiseMultiplier = 2.0f;
        float noiseResolution = 5.0f;

        //Noise indices.
        float xf, yf, zf;

        //Initialize 4 simplectic noise generators (one for each channel).
        OpenSimplexNoise noiseR = new OpenSimplexNoise((int)DateTime.Now.Ticks);
        yield return null;
        OpenSimplexNoise noiseG = new OpenSimplexNoise((int)DateTime.Now.Ticks);
        yield return null;
        OpenSimplexNoise noiseB = new OpenSimplexNoise((int)DateTime.Now.Ticks);

        Vector3 radiusMultiplier = Vector3.one - new Vector3(0.83f, 0.99f, 0.83f) + Vector3.one;
        float fadeRadius = 0.1f;
        float xs, ys, zs;

        //Fill channels with noise
        Vector4 tempColor = new Vector4();
        for (int z = 0; z < nz; z++)
        {
            //yield return in the outer most loop to keep project responsive.
            yield return null;
            zs = (float)z / nz;
            zf = zs * noiseResolution;
            for (int y = 0; y < ny; y++)
            {
                ys = (float)y / ny;
                yf = ys * noiseResolution;
                for (int x = 0; x < nx; x++)
                {
                    xs = (float)x / nx;
                    xf = xs * noiseResolution;
                    tempColor.x = noiseR.eval(xf, yf, zf) * noiseMultiplier;
                    tempColor.y = noiseG.eval(xf, yf, zf) * noiseMultiplier;
                    tempColor.z = noiseB.eval(xf, yf, zf) * noiseMultiplier;
                    tempColor.w = 1;
                    //Make sure channel values don't exceed 1.0f;
                    tempColor = tempColor.normalized;
                    float radius = Vector3.Scale(radiusMultiplier, new Vector3(xs - 0.5f, ys - 0.5f, zs - 0.5f)).magnitude;
                    if (radius >= (0.5f - fadeRadius))
                    {
                        texColor[z * ny * nx + y * nx + x] = new Color(tempColor.x, tempColor.y, tempColor.z, tempColor.w) * (fadeRadius - (radius-0.5f+fadeRadius))/ fadeRadius;
                    }
                    else
                    {
                        texColor[z * ny * nx + y * nx + x] = new Color(tempColor.x, tempColor.y, tempColor.z, tempColor.w);
                    }
                    
                }
            }
            _loadingProgressChanged.Invoke((float)(z+1) / nz);
        }
        volumeTexture.SetPixels(texColor);
        volumeTexture.filterMode = FilterMode.Trilinear;
        volumeTexture.wrapMode = TextureWrapMode.Clamp;
        volumeTexture.Apply();
    }

    IEnumerator UpdateAnimation()
    {
        float[] frame = { 0.0014f, 0.0641f, 0.2020f, 0.4223f, 0.6600f, 0.7781f, 0.7914f, 0.7727f, 0.7427f, 0.7034f, 0.6574f, 0.6085f, 0.5617f, 0.5231f, 0.5002f, 0.5003f, 0.5297f, 0.5909f, 0.6809f, 0.7893f, 0.8992f, 0.9898f, 1.0000f, 0.9959f, 0.9914f, 0.9758f, 0.9527f, 0.9286f, 0.9032f, 0.8761f, 0.8474f, 0.8170f, 0.7850f, 0.7516f, 0.7168f, 0.6810f, 0.6444f, 0.6073f, 0.5700f, 0.5327f, 0.4957f, 0.4593f, 0.4236f, 0.3890f, 0.3555f, 0.3234f, 0.2927f, 0.2635f, 0.2360f, 0.2102f, 0.1861f, 0.1637f, 0.1430f, 0.1239f, 0.1065f, 0.0906f, 0.0763f, 0.0633f, 0.0517f, 0.0413f, 0.0320f, 0.0238f, 0.0166f, 0.0103f, 0.0048f, 0f };
        int numFrames = frame.Length;
        Color weight = new Color(1, 1, 1, 1);
        volumeComponent.dataChannelWeight = weight;
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < numFrames; i++)
                {
                    weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                    weight.g = (1 - frame[i]) * (maxValue - minValue) + minValue;
                    weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                    volumeComponent.dataChannelWeight = weight;
                    yield return null;
                }
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                weight.g = ((1 - frame[i]) * (maxValue - minValue) + minValue) * (numFrames - i) / numFrames;
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                weight.b = ((1 - frame[i]) * (maxValue - minValue) + minValue) * (numFrames - i) / numFrames;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = ((1 - frame[i]) * (maxValue - minValue) + minValue) * (numFrames - i) / numFrames;
                weight.g = ((1 - frame[i]) * (maxValue - minValue) + minValue) * i / numFrames;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.g = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.g = ((1 - frame[i]) * (maxValue - minValue) + minValue) * (numFrames - i) / numFrames;
                weight.b = ((1 - frame[i]) * (maxValue - minValue) + minValue) * i / numFrames;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = ((1 - frame[i]) * (maxValue - minValue) + minValue) * i / numFrames;
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
            for (int i = 0; i < numFrames; i++)
            {
                weight.r = (1 - frame[i]) * (maxValue - minValue) + minValue;
                weight.g = ((1 - frame[i]) * (maxValue - minValue) + minValue) * i / numFrames;
                weight.b = (1 - frame[i]) * (maxValue - minValue) + minValue;
                volumeComponent.dataChannelWeight = weight;
                yield return null;
            }
        }
    }
}