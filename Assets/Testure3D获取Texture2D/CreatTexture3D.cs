using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatTexture3D : MonoBehaviour {

    public Renderer target;
    public int size = 16;
    public Renderer plane;

    void Start()
    {
        Texture3D tex = new Texture3D(size, size, size, TextureFormat.RGBA32, false);
        Color[] colors = new Color[size * size * size];
        int k = 0;

        for (int z = 0; z < size; z++)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++, k++)
                {
                    colors[k] = new Color((float)x / (size-1), (float)y / (size - 1), (float)z / (size - 1));
                }
            }
        }
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels(colors);
        tex.Apply();
        target.material.SetTexture("_Volume", tex);
        plane.material.SetTexture("_Volume", tex);
    }
}
