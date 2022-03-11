////--------------------------------------------------------------------
/// Namespace:          
/// Class:              OverlayExample
/// Description:        Example of creating and animating a
///                         transfer function for an overlay.
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(VolumeViewer.VolumeComponent))]
public class OverlayExample : MonoBehaviour
{

    public float fadeTime = 2;
    public float pauseTime = 0.5f;

    VolumeViewer.VolumeComponent volumeComponent;
    Texture2D overlayTFTex;
    Dictionary<string, int> region = new Dictionary<string, int>{
                                                {"Background" ,                   0},
                                                {"HeadLeftTop" ,                  1},
                                                {"HeadRightTop" ,               128},
                                                {"HeadLeftBottom" ,               2},
                                                {"HeadRightBottom" ,            129},
                                                {"CSFLeft" ,                      5},
                                                {"CSFRight" ,                   132},
                                                {"CerebellumLeft" ,              10},
                                                {"CerebellumRight" ,            137},
                                                {"BrainstemLeft" ,               15},
                                                {"BrainstemRight" ,             142},
                                                {"FrontalLeft" ,             25},
                                                {"FrontalRight" ,           152},
                                                {"ParietalLeft" ,            26},
                                                {"ParietalRight" ,          153},
                                                {"OccipitalLeft" ,           27},
                                                {"OccipitalRight" ,         154},
                                                {"PrecentralGyrusLeft" ,         28},
                                                {"PrecentralGyrusRight" ,       155},
                                                {"PostcentralGyrusLeft" ,        29},
                                                {"PostcentralGyrusRight" ,      156},
                                                {"TemporalLeft" ,            40},
                                                {"TemporalRight" ,          167},
                                                {"ROILeft" ,                     60},
                                                {"ROIRight" ,                   187}
                                            };

    Color[] currentOverlay;
    Color[] coloredOverlay;

    void Start()
    {
        currentOverlay = new Color[256];
        coloredOverlay = new Color[256];
        for (int i = 0; i < 256; i++)
        {
            coloredOverlay[i] = Color.clear;
            currentOverlay[i] = Color.white;
        }
        currentOverlay[region["Background"]]                    = Color.clear;
        currentOverlay[region["CSFLeft"]]                       = Color.clear;
        currentOverlay[region["CSFRight"]]                      = Color.clear;

        coloredOverlay[region["Background"]] = Color.clear;
        coloredOverlay[region["HeadLeftTop"]]                  = new Color(1.000f, 0.874f, 0.756f, 1.000f);
        coloredOverlay[region["HeadRightTop"]]                 = new Color(1.000f, 0.874f, 0.756f, 1.000f);
        coloredOverlay[region["HeadLeftBottom"]]               = new Color(1.000f, 0.874f, 0.756f, 1.000f);
        coloredOverlay[region["HeadRightBottom"]]              = new Color(1.000f, 0.874f, 0.756f, 1.000f);
        coloredOverlay[region["CSFLeft"]]                      = new Color(0.400f, 0.600f, 0.800f, 0.800f);
        coloredOverlay[region["CSFRight"]]                     = new Color(0.400f, 0.600f, 0.800f, 0.800f);
        coloredOverlay[region["CerebellumLeft"]]               = new Color(1.000f, 0.500f, 0.000f, 1.000f);
        coloredOverlay[region["CerebellumRight"]]              = new Color(1.000f, 0.500f, 0.000f, 1.000f);
        coloredOverlay[region["BrainstemLeft"]]                = new Color(1.000f, 0.956f, 0.835f, 1.000f);
        coloredOverlay[region["BrainstemRight"]]               = new Color(1.000f, 0.956f, 0.835f, 1.000f);
        coloredOverlay[region["FrontalLeft"]]              = new Color(1.000f, 0.329f, 0.098f, 1.000f);
        coloredOverlay[region["FrontalRight"]]             = new Color(1.000f, 0.329f, 0.098f, 1.000f);
        coloredOverlay[region["ParietalLeft"]]             = new Color(0.023f, 0.400f, 1.000f, 1.000f);
        coloredOverlay[region["ParietalRight"]]            = new Color(0.023f, 0.400f, 1.000f, 1.000f);
        coloredOverlay[region["OccipitalLeft"]]            = new Color(0.500f, 0.000f, 1.000f, 1.000f);
        coloredOverlay[region["OccipitalRight"]]           = new Color(0.500f, 0.000f, 1.000f, 1.000f);
        coloredOverlay[region["PrecentralGyrusLeft"]]          = new Color(1.000f, 0.694f, 0.117f, 1.000f);
        coloredOverlay[region["PrecentralGyrusRight"]]         = new Color(1.000f, 0.694f, 0.117f, 1.000f);
        coloredOverlay[region["PostcentralGyrusLeft"]]         = new Color(0.300f, 0.749f, 1.000f, 1.000f);
        coloredOverlay[region["PostcentralGyrusRight"]]        = new Color(0.300f, 0.749f, 1.000f, 1.000f);
        coloredOverlay[region["TemporalLeft"]]             = new Color(0.466f, 1.000f, 0.192f, 1.000f);
        coloredOverlay[region["TemporalRight"]]            = new Color(0.466f, 1.000f, 0.192f, 1.000f);
        coloredOverlay[region["ROILeft"]]                      = new Color(1.000f, 0.000f, 0.000f, 1.000f);
        coloredOverlay[region["ROIRight"]]                     = new Color(1.000f, 0.000f, 0.000f, 1.000f);
        overlayTFTex = new Texture2D(256, 1, TextureFormat.ARGB32, false);
        overlayTFTex.SetPixels(currentOverlay);
        overlayTFTex.wrapMode = TextureWrapMode.Clamp;
        overlayTFTex.filterMode = FilterMode.Point;
        overlayTFTex.Apply();

        volumeComponent = GetComponent<VolumeViewer.VolumeComponent>();
        volumeComponent.tfOverlay = overlayTFTex;
    }

    public void startAnimation(bool start)
    {
        if (start == false)
        {
            return;
        }
        StartCoroutine(UpdateAnimation());
    }

    void updateOverlayTF()
    {
        overlayTFTex.SetPixels(currentOverlay);
        overlayTFTex.Apply();
    }
    
    IEnumerator UpdateAnimation () {

        Color transparent = new Color(1, 1, 1, 0);

        yield return new WaitForSeconds(0.5f);

        while (true)
        {
            float cumTime = 0;
            Color originColor = currentOverlay[region["HeadLeftTop"]];
            while (true)
            {
                currentOverlay[region["HeadLeftTop"]] = Color.Lerp(originColor, transparent, cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["HeadRightTop"]];
            while (true)
            {
                currentOverlay[region["HeadRightTop"]] = Color.Lerp(originColor, transparent, cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["HeadRightBottom"]];
            while (true)
            {
                currentOverlay[region["HeadRightBottom"]] = Color.Lerp(originColor, transparent, cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["HeadLeftBottom"]];
            while (true)
            {
                currentOverlay[region["HeadLeftBottom"]] = Color.Lerp(originColor, transparent, cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                updateOverlayTF();

                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);
            
            cumTime = 0;
            originColor = currentOverlay[region["CSFLeft"]];
            float originAlpha = volumeComponent.surfaceAlpha;
            float originThr = volumeComponent.surfaceThr;
            float originShininess = volumeComponent.shininess;
            while (true)
            {
                volumeComponent.surfaceAlpha = Mathf.Lerp(originAlpha, 0.35f, cumTime / (fadeTime));
                volumeComponent.surfaceThr = Mathf.Lerp(originThr, 0.2f, cumTime / (fadeTime));
                volumeComponent.shininess = Mathf.Lerp(originShininess, 30, cumTime / (fadeTime));
                currentOverlay[region["CSFLeft"]] = Color.Lerp(originColor, coloredOverlay[region["CSFLeft"]], cumTime / (fadeTime));
                currentOverlay[region["CSFRight"]] = Color.Lerp(originColor, coloredOverlay[region["CSFRight"]], cumTime / (fadeTime));
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["CerebellumLeft"]];
            while (true)
            {
                currentOverlay[region["CerebellumLeft"]] = Color.Lerp(originColor, coloredOverlay[region["CerebellumLeft"]], cumTime / fadeTime);
                currentOverlay[region["CerebellumRight"]] = Color.Lerp(originColor, coloredOverlay[region["CerebellumRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["TemporalLeft"]];
            while (true)
            {
                currentOverlay[region["TemporalLeft"]] = Color.Lerp(originColor, coloredOverlay[region["TemporalLeft"]], cumTime / fadeTime);
                currentOverlay[region["TemporalRight"]] = Color.Lerp(originColor, coloredOverlay[region["TemporalRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["FrontalLeft"]];
            while (true)
            {
                currentOverlay[region["FrontalLeft"]] = Color.Lerp(originColor, coloredOverlay[region["FrontalLeft"]], cumTime / fadeTime);
                currentOverlay[region["FrontalRight"]] = Color.Lerp(originColor, coloredOverlay[region["FrontalRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["OccipitalLeft"]];
            while (true)
            {
                currentOverlay[region["OccipitalLeft"]] = Color.Lerp(originColor, coloredOverlay[region["OccipitalLeft"]], cumTime / fadeTime);
                currentOverlay[region["OccipitalRight"]] = Color.Lerp(originColor, coloredOverlay[region["OccipitalRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["ParietalLeft"]];
            while (true)
            {
                currentOverlay[region["ParietalLeft"]] = Color.Lerp(originColor, coloredOverlay[region["ParietalLeft"]], cumTime / fadeTime);
                currentOverlay[region["ParietalRight"]] = Color.Lerp(originColor, coloredOverlay[region["ParietalRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["PostcentralGyrusLeft"]];
            while (true)
            {
                currentOverlay[region["PrecentralGyrusLeft"]] = Color.Lerp(originColor, coloredOverlay[region["PrecentralGyrusLeft"]], cumTime / fadeTime);
                currentOverlay[region["PrecentralGyrusRight"]] = Color.Lerp(originColor, coloredOverlay[region["PrecentralGyrusRight"]], cumTime / fadeTime);
                currentOverlay[region["PostcentralGyrusLeft"]] = Color.Lerp(originColor, coloredOverlay[region["PostcentralGyrusLeft"]], cumTime / fadeTime);
                currentOverlay[region["PostcentralGyrusRight"]] = Color.Lerp(originColor, coloredOverlay[region["PostcentralGyrusRight"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime * 2f);
            
            cumTime = 0;
            originColor = currentOverlay[region["HeadRightBottom"]];
            originThr = volumeComponent.surfaceThr;
            originShininess = volumeComponent.shininess;
            while (true)
            {
                currentOverlay[region["HeadRightBottom"]] = Color.Lerp(originColor, coloredOverlay[region["HeadRightBottom"]], cumTime / fadeTime);
                volumeComponent.surfaceThr = Mathf.Lerp(originThr, 0.12f, cumTime / (fadeTime / 2f));
                volumeComponent.shininess = Mathf.Lerp(originShininess, 12f, cumTime / (fadeTime / 2f));
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["HeadLeftBottom"]];
            while (true)
            {
                currentOverlay[region["HeadLeftBottom"]] = Color.Lerp(originColor, coloredOverlay[region["HeadLeftBottom"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                updateOverlayTF();

                yield return null;
            }
            yield return new WaitForSeconds(pauseTime * 23);

            cumTime = 0;
            originColor = currentOverlay[region["HeadLeftTop"]];
            while (true)
            {
                currentOverlay[region["HeadLeftTop"]] = Color.Lerp(originColor, coloredOverlay[region["HeadLeftTop"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);

            cumTime = 0;
            originColor = currentOverlay[region["HeadRightTop"]];
            while (true)
            {
                currentOverlay[region["HeadRightTop"]] = Color.Lerp(originColor, coloredOverlay[region["HeadRightTop"]], cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime * 2);

            cumTime = 0;
            while (true)
            {
                currentOverlay[region["HeadLeftTop"]] = Color.Lerp(coloredOverlay[region["HeadLeftTop"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["HeadRightTop"]] = Color.Lerp(coloredOverlay[region["HeadRightTop"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["HeadLeftBottom"]] = Color.Lerp(coloredOverlay[region["HeadLeftBottom"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["HeadRightBottom"]] = Color.Lerp(coloredOverlay[region["HeadRightBottom"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["CerebellumLeft"]] = Color.Lerp(coloredOverlay[region["CerebellumLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["CerebellumRight"]] = Color.Lerp(coloredOverlay[region["CerebellumRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["TemporalLeft"]] = Color.Lerp(coloredOverlay[region["TemporalLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["TemporalRight"]] = Color.Lerp(coloredOverlay[region["TemporalRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["FrontalLeft"]] = Color.Lerp(coloredOverlay[region["FrontalLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["FrontalRight"]] = Color.Lerp(coloredOverlay[region["FrontalRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["OccipitalLeft"]] = Color.Lerp(coloredOverlay[region["OccipitalLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["OccipitalRight"]] = Color.Lerp(coloredOverlay[region["OccipitalRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["ParietalLeft"]] = Color.Lerp(coloredOverlay[region["ParietalLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["ParietalRight"]] = Color.Lerp(coloredOverlay[region["ParietalRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["PostcentralGyrusLeft"]] = Color.Lerp(coloredOverlay[region["PostcentralGyrusLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["PostcentralGyrusRight"]] = Color.Lerp(coloredOverlay[region["PostcentralGyrusRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["PrecentralGyrusLeft"]] = Color.Lerp(coloredOverlay[region["PrecentralGyrusLeft"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["PrecentralGyrusRight"]] = Color.Lerp(coloredOverlay[region["PrecentralGyrusRight"]], Color.white, cumTime / fadeTime);
                currentOverlay[region["CSFLeft"]] = Color.Lerp(coloredOverlay[region["CSFLeft"]], Color.clear, cumTime / fadeTime);
                currentOverlay[region["CSFRight"]] = Color.Lerp(coloredOverlay[region["CSFRight"]], Color.clear, cumTime / fadeTime);
                updateOverlayTF();
                if (cumTime >= fadeTime)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                updateOverlayTF();
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime * 2);

            cumTime = 0;
            originAlpha = volumeComponent.surfaceAlpha;
            originThr = volumeComponent.surfaceThr;
            while (true)
            {
                volumeComponent.surfaceAlpha = Mathf.Lerp(originAlpha, 0, cumTime / (fadeTime / 2f));
                volumeComponent.surfaceThr = Mathf.Lerp(originThr, 0, cumTime / (fadeTime / 2f));
                if (cumTime >= fadeTime / 2f)
                {
                    break;
                }
                cumTime += Time.deltaTime;
                yield return null;
            }
            yield return new WaitForSeconds(pauseTime);
        }
	}
}
