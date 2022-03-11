using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GaussNoiseTest : MonoBehaviour {

    public Texture2D tex;
    public Transform quad;
    private void Start()
    {
        Color[] c = tex.GetPixels();

        for (int i = 0; i < c.Length; i++)
        {
            float temp = c[i].r + 0.25f * GuassNoise(0, 0.5f);
            //float temp = c[i].r + 0.15f *(float) GaussNiose1();

            temp = Mathf.Clamp01(temp);

            c[i] = new Color(temp, temp, temp);

        }

        Texture2D t = new Texture2D(tex.width, tex.height, TextureFormat.RGBAFloat, false);
        t.SetPixels(c);
        t.Apply();

        quad.GetComponent<MeshRenderer>().material.mainTexture = t;
    }




    private int phase = 0;
    private float v1 = 0;
    private float v2 = 0;
    private float s = 0;

    private float GuassNoise(float mu, float sigma)
    {
        float x = 0;
        float u1 = 0;
        float u2 = 0;
        if (phase == 0)
        {
            do
            {
                u1 = UnityEngine.Random.Range(0f, 1f);
                u2 = UnityEngine.Random.Range(0f, 1f);

                v1 = 2 * u1 - 1;
                v2 = 2 * u2 - 1;
                s = v1 * v1 + v2 * v2;
            }
            while (s >= 1 || s == 0);
            x = v1 * Mathf.Sqrt(-2 * Mathf.Log(s) / s);
        }
        else
        {
            x = v2 * Mathf.Sqrt(-2 * Mathf.Log(s) / s);
        }
        phase = 1 - phase;

        return mu + sigma * x;
    }
    public int GetRandomSeed() //产生随机种子
    {
        byte[] bytes = new byte[4];
        System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }

    public double GaussNiose1()//用box muller的方法产生均值为0，方差为1的正太分布随机数
    {
        // Random ro = new Random(10);
        // long tick = DateTime.Now.Ticks;
        System.Random ran = new System.Random(GetRandomSeed());
        // Random rand = new Random();
        double r1 = ran.NextDouble();
        double r2 = ran.NextDouble();
        double result = Math.Sqrt((-2) * Math.Log(r2)) * Math.Sin(2 * Math.PI * r1);
        return result;//返回随机数
    }
}
