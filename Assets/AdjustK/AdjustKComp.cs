using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class AdjustKComp : MonoBehaviour {

    public Texture2D[] texs;
    public Texture2D tex;
    public MeshRenderer quad;
    private Texture2D tt;


    private Color[] colors;
    private int nx;
    private int ny;
    private Dictionary<string, int> keys = new Dictionary<string, int>();
    private List<Vector2> maxContent = new List<Vector2>();
    private bool complete;
    private Action leftRightMirror;
    private Action upDownMirror;

    int index = 0;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            //tex = texs[index];
            //index++;
            AdjustKWay(tex, () => {
                Debug.Log("左右镜像");
            }, () => {
                Debug.Log("上下镜像");
            });
        }
        if (complete)
        {
            Feature(maxContent);
            complete = false;
            Debug.Log("End");

            tt = new Texture2D(nx, ny); 
            tt.SetPixels(colors);
            tt.Apply();
            quad.material.mainTexture = tt;
        }
    }
    /// <summary>
    /// 调整K
    /// </summary>
    /// <param name="texture">图</param>
    /// <param name="leftRightMirror">左右镜像</param>
    /// <param name="upDownMirror">上下镜像</param>
    public void AdjustKWay(Texture2D texture,Action leftRightMirror,Action upDownMirror)
    {
        colors = texture.GetPixels(2);
        nx = texture.width/4;
        ny = texture.height/4;
        keys = new Dictionary<string, int>();
        maxContent = new List<Vector2>();
        complete = false;
        this.leftRightMirror = leftRightMirror;
        this.upDownMirror = upDownMirror;

        colors = MedianFilter(colors);

        ImageEnhance(colors);
        
        //colors = Erode(colors);

        Binaryzation(colors);
        //complete = true;

        Thread thread = new Thread(delegate () { MaxContent(colors); });
        thread.IsBackground = true;
        thread.Start();
    }
    /// <summary>
    ///滤波
    /// </summary>
    private Color[] MedianFilter(Color[] colors)
    {
        Color[] values = new Color[nx * ny];
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                if (x <= 10 || y <= 10 || x >= nx -10 || y >= nx -10)
                {
                    values[y * nx + x] = Color.white;
                    continue;
                }

                List<float> temp = new List<float>();
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        temp.Add(colors[(y + j) * nx + (x + i)].r);
                    }
                }
                temp.Sort();
                values[y * nx + x] = Color.white * temp[5];
            }
        }
        return values;
    }
    /// <summary>
    /// 图像增强
    /// </summary>
    private void ImageEnhance(Color[] colors)
    {
        float mean = 0;
        float min = 1f;
        for (int i = 0; i < colors.Length; i++)
        {
            mean += colors[i].r / colors.Length;
            min = min < colors[i].r ? min : colors[i].r;
        }
        Debug.Log("mean: " + mean);
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white * (colors[i].r - min) / (mean - min);
        }
    }
    /// <summary>
    /// 腐蚀
    /// </summary>
    private Color[] Erode(Color[] colors)
    {
        Color[] values = new Color[nx * ny];
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                if (x <= 0 || y <= 0 || x >= nx - 1 || y >= nx - 1)
                {
                    values[y * nx + x] = Color.white;
                    continue;
                }

                float minValue = 1;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        minValue = minValue < colors[(y + j) * nx + (x + i)].r ? minValue : colors[(y + j) * nx + (x + i)].r;
                    }
                }
                values[y * nx + x] = Color.white * minValue;
            }
        }
        return values;
    }
    /// <summary>
    /// 二值化
    /// </summary>
    private void Binaryzation(Color[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].r > 0.5)
            {
                colors[i] = Color.white;
            }
            else
            {
                colors[i] = Color.black;
            }
        }
    }
    /// <summary>
    /// 最大连通域
    /// </summary>
    private void MaxContent(Color[] colors)
    {
        for (int x = 1; x < nx - 1; x++)
        {
            for (int y = 1; y < ny - 1; y++)
            {
                List<Vector2> inds = new List<Vector2>();
                Recursion(x, y, inds, colors);
                if (inds.Count > maxContent.Count)
                {
                    maxContent = inds;
                }
            }
        }
        complete = true;
    }
    /// <summary>
    /// 递归
    /// </summary>
    private void Recursion(int x, int y, List<Vector2> inds, Color[] colors)
    {
        if (x < 0 || y < 0 || x >= nx || y >= nx)
            return;
        string key = x + "_" + y;
        if (colors[x + y * nx].r == 0 && !keys.ContainsKey(key))
        {
            keys.Add(key, 1);
            inds.Add(new Vector2(x, y));
            Recursion(x - 1, y, inds, colors);
            Recursion(x + 1, y, inds, colors);
            Recursion(x, y - 1, inds, colors);
            Recursion(x, y + 1, inds, colors);
        }             
    }
    /// <summary>
    /// 特征比较
    /// </summary>
    private void Feature(List<Vector2> inds)
    {
        float minx = nx;
        float miny = ny;
        float maxx = 0;
        float maxy = 0;
        for (int i = 0; i < inds.Count; i++)
        {
            minx = minx < inds[i].x ? minx : inds[i].x;
            miny = miny < inds[i].y ? miny : inds[i].y;
            maxx = maxx > inds[i].x ? maxx : inds[i].x;
            maxy = maxy > inds[i].y ? maxy : inds[i].y;
            //
            colors[(int)inds[i].x + (int)inds[i].y * nx] = Color.red;
            //
        }
        float lx = minx + (maxx - minx) / 4;
        float rx = maxx - (maxx - minx) / 4;
        float ly = miny + (maxy - miny) / 4;
        float ry = maxy - (maxy - miny) / 4;

        int leftCount = 0;
        int RightCount = 0;
        int UpCount = 0;
        int DownCount = 0;
        for (int i = 0; i < inds.Count; i++)
        {
            if (inds[i].x < lx)
            {
                leftCount++;

            }
            else if (inds[i].x > rx)
            {
                RightCount++;

            }
            if (inds[i].y < ly)
            {
                DownCount++;

            }
            else if (inds[i].y > ry)
            {
                UpCount++;

            }
        }

        if (leftCount < RightCount)
        {
            leftRightMirror();
        }
        if (DownCount > UpCount)
        {
            upDownMirror();
        }
    }
}
