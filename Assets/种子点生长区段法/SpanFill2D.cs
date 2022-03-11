using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpanFill2D : MonoBehaviour {

    public Texture2D texture;
    public Transform quad;
    private TexInfo texInfo;
    private Stack<Span> spans = new Stack<Span>();
    private bool[] flags;
    private List<int> indexs = new List<int>();
    private float threshold;
    private Color referenceColor;


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Reset();
            sw.Start();
            Vector3 p = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.WorldToScreenPoint(quad.position).z));
            p = quad.worldToLocalMatrix.MultiplyPoint3x4(p);
            int x = (int)(texture.width * (p.x + 0.5f));
            int y = (int)(texture.height * (p.y + 0.5f));
            SpanFill(texture, x, y, 0.03f);
            sw.Stop();
            for (int i = 0; i < indexs.Count; i++)
            {
                texInfo.colors[indexs[i]] = Color.red;
            }
            Texture2D t = new Texture2D(texInfo.nx, texInfo.ny, TextureFormat.ARGB32, false);
            t.SetPixels(texInfo.colors);
            t.Apply();
            quad.GetComponent<MeshRenderer>().material.mainTexture = t;
            Debug.Log(indexs.Count);
            Debug.Log(sw.ElapsedMilliseconds+"ms");
        }
    }
    void FloodFill(int x,int y)
    {
        int index = texInfo.nx * y + x;
        if (!flags[index] && IncludePixel(index))
        {
            flags[index] = true;
            indexs.Add(index);
            if (x - 1 >= 0)
                FloodFill(x - 1, y);
            if (x + 1 < texInfo.nx)
                FloodFill(x + 1, y);
            if (y - 1 >= 0)
                FloodFill(x, y - 1);
            if (y + 1 < texInfo.ny)
                FloodFill(x, y + 1);
        }
        else
        {
            return;
        }

    }
    void Flood(int x,int y)
    {
        Stack<int[]> ps = new Stack<int[]>();
        ps.Push(new int[2] { x, y });
        while (ps.Count > 0)
        {
            int[] p = ps.Pop();
            int index = texInfo.nx * p[1] + p[0];
            if (!flags[index] && IncludePixel(index))
            {
                flags[index] = true;
                indexs.Add(index);
                if (p[0] - 1 >= 0)
                    ps.Push(new int[2] { p[0] - 1, p[1] });
                if (p[0] + 1 < texInfo.nx)
                    ps.Push(new int[2] { p[0] + 1, p[1] });
                if (p[1] - 1 >= 0)
                    ps.Push(new int[2] { p[0], p[1] - 1 });
                if (p[1] + 1 < texInfo.ny)
                    ps.Push(new int[2] { p[0], p[1] + 1 });
            }
        }

    }
    void SpanFill(Texture2D tex,int x,int y,float thres)
    {
        threshold = thres;
        referenceColor = tex.GetPixel(x, y);
        texInfo = GetTextureInfo(tex);
        flags = new bool[texInfo.nx * texInfo.ny];
        indexs.Clear();
        ///区段法
        flags[texInfo.nx * y + x] = true;
        indexs.Add(texInfo.nx * y + x);
        SpanStack(x, y);
        Fill();
        ///泛洪法
        //FloodFill(x, y);//递归
        //Flood(x, y);//压栈
    }

    TexInfo GetTextureInfo(Texture2D tex)
    {
        TexInfo texInfo = new TexInfo();
        texInfo.nx = tex.width;
        texInfo.ny = tex.height;
        texInfo.colors = tex.GetPixels();
        return texInfo;
    }

    void SpanStack(int x,int y)
    {
        Span sp = new Span();
        sp.xLeft = x;
        sp.xRight = x;
        sp.y = y;
        sp.extendType = ExtendType.UNREZ;
        sp.parentDirection = ParentDirection.NON;
        spans.Clear();
        spans.Push(sp);
    }
    void Fill()
    {
        while (spans.Count > 0)
        {
            Span sp = spans.Pop();
            int xl, xr;
            switch (sp.extendType)
            {
                case ExtendType.LEFTREQUIRED:
                    xl = FindXLeft(sp.xLeft,sp.y);
                    if (sp.parentDirection == ParentDirection.UP)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(xl, sp.xRight, sp.y - 1, ParentDirection.UP);
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(xl, sp.xLeft, sp.y + 1, ParentDirection.DOWN);
                    }
                    else if (sp.parentDirection == ParentDirection.DOWN)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(xl, sp.xLeft, sp.y - 1, ParentDirection.UP);
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(xl, sp.xRight, sp.y + 1, ParentDirection.DOWN);
                    }
                    continue;
                case ExtendType.RIGHTREQUIRED:
                    xr = FindXRight(sp.xRight, sp.y);
                    if (sp.parentDirection == ParentDirection.UP)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(sp.xLeft, xr, sp.y - 1, ParentDirection.UP);
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(sp.xRight, xr, sp.y + 1, ParentDirection.DOWN);
                    }
                    else if (sp.parentDirection == ParentDirection.DOWN)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(sp.xRight, xr, sp.y - 1, ParentDirection.UP);
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(sp.xLeft, xr, sp.y + 1, ParentDirection.DOWN);
                    }
                    continue;
                case ExtendType.ALLREZ:
                    if (sp.parentDirection == ParentDirection.DOWN)
                    {
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(sp.xLeft, sp.xRight, sp.y + 1, ParentDirection.DOWN);
                    }
                    else if (sp.parentDirection == ParentDirection.UP)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(sp.xLeft, sp.xRight, sp.y - 1, ParentDirection.UP);
                    }
                    continue;
                case ExtendType.UNREZ:
                    xl = FindXLeft(sp.xLeft, sp.y);
                    xr = FindXRight(sp.xRight, sp.y);
                    if (sp.parentDirection == ParentDirection.UP)
                    {
                        if (sp.y - 1 >= 0)
                        {
                            CheckRange(xl, xr, sp.y - 1, ParentDirection.UP);
                            //CheckRange(sp.xRight, xr, sp.y - 1, ParentDirection.UP);
                        }
                        if (sp.y + 1 < texInfo.ny)
                        {
                            CheckRange(xl, sp.xLeft, sp.y + 1, ParentDirection.DOWN);
                            CheckRange(sp.xRight, xr, sp.y + 1, ParentDirection.DOWN);
                        }
                    }
                    else if (sp.parentDirection == ParentDirection.DOWN)
                    {
                        if (sp.y + 1 < texInfo.ny)
                        {
                            CheckRange(xl, xr, sp.y + 1, ParentDirection.DOWN);
                        }
                        if (sp.y - 1 >= 0)
                        {
                            CheckRange(xl, sp.xLeft, sp.y - 1, ParentDirection.UP);
                            CheckRange(sp.xRight, xr, sp.y - 1, ParentDirection.UP);
                        }
                    }
                    else if (sp.parentDirection == ParentDirection.NON)
                    {
                        if (sp.y - 1 >= 0)
                            CheckRange(xl, xr, sp.y - 1, ParentDirection.UP);
                        if (sp.y + 1 < texInfo.ny)
                            CheckRange(xl, xr, sp.y + 1, ParentDirection.DOWN);
                    }
                        continue;
                default:
                    break;
            }
        }
    }

    private int FindXLeft(int x, int y)
    {
        int xleft = x - 1;
        while (true)
        {
            int index = texInfo.nx * y + xleft;
            if (xleft < 0 || flags[index] || !IncludePixel(index))
            {
                break;
            }
            else
            {
                flags[index] = true;
                indexs.Add(index);
                xleft--;
            }
        }
        return xleft + 1;
    }
    private int FindXRight(int x, int y)
    {
        int xright = x + 1;
        while (true)
        {
            int index = texInfo.nx * y + xright;
            if (xright >= texInfo.nx || flags[index] || !IncludePixel(index))
            {
                break;
            }
            else
            {
                flags[index] = true;
                indexs.Add(index);
                xright++;
            }
        }
        return xright - 1;
    }
    private void CheckRange(int xleft, int xright, int y, ParentDirection ptype)
    {
        for (int i = xleft; i < xright; )
        {
            int index = texInfo.nx * y + i;
            if (!flags[index] && IncludePixel(index))
            {
                int xl = i;
                int xr = i + 1;
                while (xr <= xright && !flags[texInfo.nx * y + xr] && IncludePixel(texInfo.nx * y + xr))
                {
                    xr++;
                }
                xr--;
                Span span = new Span();
                span.xLeft = xl;
                span.xRight = xr;
                span.y = y;
                if (xl == xleft && xr == xright)
                {
                    span.extendType = ExtendType.UNREZ;
                }
                else if (xr == xright)
                {
                    span.extendType = ExtendType.RIGHTREQUIRED;
                }
                else if (xl == xleft)
                {
                    span.extendType = ExtendType.LEFTREQUIRED;
                }
                else
                {
                    span.extendType = ExtendType.ALLREZ;
                }
                span.parentDirection = ptype;
                spans.Push(span);
                for (int j = xl; j <= xr; j++)
                {
                    flags[texInfo.nx * y + j] = true;
                    indexs.Add(texInfo.nx * y + j);
                }
            }
            else
            {
                i++;
            }
        }
    }
    private bool IncludePixel(int index)
    {
        if (Mathf.Abs(texInfo.colors[index].r - referenceColor.r) < threshold)
        {
            return true;
        }
        return false;
    }
}

public struct TexInfo
{
    public int nx;
    public int ny;
    public Color[] colors;
}

public enum ParentDirection
{
    DOWN,
    UP,
    NON
}
public enum ExtendType
{
    LEFTREQUIRED,
    RIGHTREQUIRED,
    ALLREZ,
    UNREZ
}
public struct Span
{
    public int xLeft;
    public int xRight;
    public int y;
    public ExtendType extendType;
    public ParentDirection parentDirection;
}
