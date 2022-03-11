using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshErasure : MonoBehaviour {

    public Camera ca;
    public Camera ca1;

    public Shader cullDepthFrontShader;
    public Shader cullDepthBackShader;

    Material depthMat;
    RenderTexture cullDepthFrontTex = null;
    RenderTexture cullDepthBackTex = null;
    public Material m;

    RenderTexture hitTex;
    Texture2D Ftex;
    Texture2D Btex;
    private Camera volumeCam;

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    if (depthMat == null)
    //    {
    //        depthMat = new Material(cullDepthBackShader);
    //    }
    //    Graphics.Blit(source, destination, depthMat);
    //}
    private void Start()
    {
        GameObject volumeCamObj = new GameObject("VolumeCam");
        volumeCamObj.transform.SetParent(transform.parent);
        volumeCam = volumeCamObj.AddComponent<Camera>();
        volumeCam.enabled = false;

        //Camera.main.depthTextureMode |= DepthTextureMode.Depth;
        //ca.depthTextureMode = DepthTextureMode.Depth;
        //ca.clearFlags = CameraClearFlags.SolidColor;
        //ca.rect = new Rect(0, 0, 1, 1);
        //ca.backgroundColor = Color.black;

        //ca1.depthTextureMode = DepthTextureMode.Depth;
        //ca1.clearFlags = CameraClearFlags.SolidColor;
        //ca1.rect = new Rect(0, 0, 1, 1);
        //ca1.backgroundColor = Color.black;

        cullDepthFrontTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.RFloat);
        cullDepthBackTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.RFloat);
        //hitTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        //ca.targetTexture = hitTex;
        //Ftex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBAFloat, false);
        //Btex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBAFloat, false);
    }
    private void Update()
    {
        //ca.targetTexture = cullDepthFrontTex;
        //ca.RenderWithShader(cullDepthFrontShader, "RenderType");


        volumeCam.CopyFrom(GetComponent<Camera>());
        volumeCam.clearFlags = CameraClearFlags.SolidColor;
        volumeCam.rect = new Rect(0, 0, 1, 1);
        volumeCam.backgroundColor = Color.black;
        




        ca1.clearFlags = CameraClearFlags.SolidColor;
        ca1.rect = new Rect(0, 0, 1, 1);
        ca1.backgroundColor = Color.black;
        cullDepthBackTex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBHalf);

        volumeCam.targetTexture = cullDepthBackTex;
        volumeCam.RenderWithShader(cullDepthBackShader, "RenderType");
        //m.SetTexture("_FrontDepth", cullDepthFrontTex);
        m.SetTexture("_BackDepth", cullDepthBackTex);

        RenderTexture.ReleaseTemporary(cullDepthBackTex);
    }
}
