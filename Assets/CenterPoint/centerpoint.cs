using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class centerpoint : MonoBehaviour {

    public MeshRenderer quad;
    public Texture2D input;
    private Texture2D output;

    private void Start()
    {
        output = new Texture2D(512, 512);

        Color[] colors = input.GetPixels();
        List<Vector2> signs = new List<Vector2>();

        List<List<Vector2>> lines = new List<List<Vector2>>();
        for (int x = 0; x < 512; x++)
        {
            List<Vector2> line = new List<Vector2>();
            for (int y = 0; y < 512; y++)
            {
                if(colors[y * 512 + x].r == 1)
                {
                    Vector2 p = new Vector2(x, y);
                    line.Add(p);
                }
            }
            if(line.Count!=0)
                lines.Add(line);
        }

        Debug.Log(lines.Count);
        List<Vector2> centers = new List<Vector2>();
        for (int i = 0; i < lines.Count; i++)
        {
            float max = 0;
            Vector2 center = Vector2.zero;
            for (int j = 0; j < lines[i].Count; j++)
            {
                float tmp = 0;
                for (int k = 0; k < lines[i].Count; k++)
                {
                    tmp+= 1 / (Vector2.Distance(lines[i][j], lines[i][k]) + 1);
                }
                if (tmp > max)
                {
                    max = tmp;
                    center = lines[i][j];
                }
            }
            centers.Add(center);
        }

        output.SetPixels(colors);
        for (int i = 0; i < centers.Count; i++)
        {
            output.SetPixel((int)centers[i].x, (int)centers[i].y, Color.red);
        }
        output.Apply();
        quad.material.mainTexture = output;
    }


    private void centerP()
    {
        output = new Texture2D(512, 512);

        Color[] colors = input.GetPixels();
        List<Vector2> signs = new List<Vector2>();
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].r == 1)
            {
                int x = i % 512;
                int y = i / 512;
                Vector2 p = new Vector2(x, y);
                signs.Add(p);
            }
        }
        Vector2 center = Vector2.zero;
        float maxValue = 0;
        for (int i = 0; i < signs.Count; i++)
        {
            float tempsum = 0;
            for (int j = 0; j < signs.Count; j++)
            {
                tempsum += 1 / (Vector2.Distance(signs[i], signs[j]) + 1);
            }
            if (tempsum > maxValue)
            {
                center = signs[i];
                maxValue = tempsum;
            }
        }
        output.SetPixels(colors);
        output.SetPixel((int)center.x, (int)center.y, Color.red);
        output.Apply();
        quad.material.mainTexture = output;
    }
}
