using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class EffectTest : MonoBehaviour {

    public Shader shader;
    private Material _M;
    private Material m
    {
        get
        {
            if (_M == null)
            {
                _M = new Material(shader);
                _M.hideFlags = HideFlags.HideAndDontSave;
            }
            return _M;
        }
    }
    private CommandBuffer commandBuffer = null;
    private RenderTexture renderTexture = null;
    private Renderer targetRenderer = null;
    public GameObject targetObject = null;
    public Material replaceMaterial = null;
    public RawImage image;
    void OnEnablel()
    {
        targetRenderer = targetObject.GetComponentInChildren<Renderer>();
        //申请RT  
        renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default, 4);
        commandBuffer = new CommandBuffer();
        //设置Command Buffer渲染目标为申请的RT  
        commandBuffer.SetRenderTarget(renderTexture);
        //初始颜色设置为灰色  
        commandBuffer.ClearRenderTarget(true, true, Color.white);
        //绘制目标对象，如果没有替换材质，就用自己的材质  
        Material mat = replaceMaterial == null ? targetRenderer.sharedMaterial : replaceMaterial;
        commandBuffer.DrawRenderer(targetRenderer, mat);
        //然后接受物体的材质使用这张RT作为主纹理  
        //targetObject.GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;
        //直接加入相机的CommandBuffer事件队列中  
        //Camera.main.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
        image.texture = renderTexture;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                targetObject = hit.collider.gameObject;
                OnEnablel();
            }
        }
        
    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        
        if (m != null)
        {
            if (commandBuffer != null)
                Graphics.ExecuteCommandBuffer(commandBuffer);
            m.SetTexture("_RenderTex", renderTexture);
            Graphics.Blit(source, destination, m);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}
