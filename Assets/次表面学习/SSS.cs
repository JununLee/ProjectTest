using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSS : MonoBehaviour {

    public Camera depthCamera;
    public Shader shader;
    public Material m;
    private RenderTexture m_tex;
    private void Start()
    {
        depthCamera.enabled = false;
        depthCamera.clearFlags = CameraClearFlags.SolidColor;
        depthCamera.backgroundColor = Color.black;
        m_tex = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);
        depthCamera.targetTexture = m_tex;
        //m.SetTexture("_BackDepthTex", m_tex);
       
    }
    private void Update()
    {
        depthCamera.RenderWithShader(shader, "RenderType");
       
        m.SetTexture("_BackDepthTex", m_tex);
    }
}
