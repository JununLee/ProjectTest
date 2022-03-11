using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gaussian : MonoBehaviour {

    public Shader shader;
    public Texture tex;
    private Material m;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (m == null)
        {
            m = new Material(shader);
        }
        m.SetTexture("_Tex", tex);
        RenderTexture buffer0 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0);
        buffer0.filterMode = FilterMode.Bilinear;
        Graphics.Blit(source, buffer0);
        for (int i = 0; i < 3; i++)
        {
            RenderTexture buffer1 = RenderTexture.GetTemporary(source.width / 2, source.height / 2, 0);
            Graphics.Blit(buffer0, buffer1, m);
            RenderTexture.ReleaseTemporary(buffer0);
            buffer0 = buffer1;
        }
        Graphics.Blit(buffer0, destination, m);
        RenderTexture.ReleaseTemporary(buffer0);
        Graphics.Blit(source, destination, m);
    }

}
