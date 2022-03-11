using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderLine : MonoBehaviour {


    public Shader cullDepthFrontShader;
    public Shader cullDepthBackShader;
    public Material m;
    private Camera volumeCam;
    private RenderTexture cullDepthFrontTex;
    private RenderTexture cullDepthBackTex;
    private LayerMask layerMask;
    RenderTexture buff;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Test");
        GameObject volumeCamObj = new GameObject("VolumeCam");
        volumeCamObj.transform.SetParent(transform.parent);
        volumeCam = volumeCamObj.AddComponent<Camera>();
        volumeCam.enabled = false;
        cullDepthFrontTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        cullDepthBackTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        volumeCam.CopyFrom(GetComponent<Camera>());
        volumeCam.clearFlags = CameraClearFlags.SolidColor;
        volumeCam.rect = new Rect(0, 0, 1, 1);
        volumeCam.backgroundColor = Color.black;
        volumeCam.cullingMask = layerMask;
        buff = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.RFloat);
        volumeCam.targetTexture = buff;
        volumeCam.Render();
        m.SetTexture("_buffTex", buff);
        Graphics.Blit(source, destination, m);
        RenderTexture.ReleaseTemporary(buff);
    }
 
}
