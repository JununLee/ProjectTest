using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class Brush : MonoBehaviour {

    public ComputeShader shader;
    public ComputeShader shader3d;
    public VolumeViewer.VolumeComponent volume;
    public static VolumeViewer.Volume info;
    public Transform forward;
    public Transform right;
    public Transform top;
    public RawImage[] dirImages;
    private RenderTexture[] m_rTexs = new RenderTexture[3];
    private Texture2D[] m_tex2ds = new Texture2D[3];
    private Texture3D t3d;
    Color[] colors;
    Color[] col2ds;

    private List<Vector2> posList = new List<Vector2>();
    Vector4[] p = new Vector4[1000];

    private List<int> index = new List<int>();
    float[] _x = new float[1000];
    float[] _y = new float[1000];
    float[] _z = new float[1000];
    private void Start()
    {
        for (int i = 0; i < m_rTexs.Length; i++)
        {
            m_rTexs[i] = new RenderTexture(512, 512, 24);
            m_rTexs[i].enableRandomWrite = true;
            m_rTexs[i].Create();
            m_rTexs[i].filterMode = FilterMode.Bilinear;
            m_tex2ds[i] = new Texture2D(512, 512);
        }
    }
    
    private void Update()
    {
        if (volume.data/*&&Input.GetKeyDown(KeyCode.T)*/)
        {
            //changeT3D();
            _setSlicerTex();
            //col2ds = m_tex2ds[0].GetPixels();
            //colors = volume.data.GetPixels();
            nx = volume.data.width;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
           // _setDirPlaneTrans(volume.transform);
           
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            // _setDirPlaneTrans(volume.transform);
            t3d = new Texture3D(volume.data.width, volume.data.height, volume.data.depth, TextureFormat.RGBAHalf, false);
            //t3d.SetPixels(volume.data.GetPixels());
            //t3d.Apply(); 
        }
        if (Input.GetMouseButtonDown(0))
        {
            colors = volume.data.GetPixels();

            //changeT3D(inputMouse(Input.mousePosition));

            deletePixels();
            //volume.data.SetPixels(colors);
            //volume.data.Apply();
        }
        if (Input.GetMouseButton(0))
        {
            //changeT3D(inputMouse());

            //changT2D();
            //changeT3D(inputMouse());
            deletePixels();
            //volume.data.SetPixels(colors);
            //volume.data.Apply();
        }
        if (Input.GetMouseButtonUp(0))
        {
            posList.Clear();
            _clamp = 0;
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = Vector4.zero;
            }
            changT2D();
            volume.data.SetPixels(colors);
            volume.data.Apply();
        }
        if (Input.GetMouseButtonDown(1))
        {
            //posList.Clear();
            //volume.data.SetPixels(colors);
            //volume.data.Apply();
            for (int i = 0; i < indexs.Count; i++)
            {
                colors[indexs[i]].g = 0;
            }
            volume.data.SetPixels(colors);
            volume.data.Apply();
        }
        if (isFin)
        {
            //Debug.Log(System.DateTime.Now-a);
            isFin = false;
            volume.data.SetPixels(colors);
            volume.data.Apply();
            //m_tex2ds[0].SetPixels(col2ds);
            //m_tex2ds[0].Apply();
            points();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            points();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            //colors = volume.data.GetPixels();
        }
        sphere.localScale = Vector3.one * thresold * 1.4f / 50;
        sphere.localPosition = dirImages[0].transform.worldToLocalMatrix.MultiplyPoint(Input.mousePosition);
    }

    private void _setSlicerTex()
    {

        dirImages[0].texture = _getSlicerTex(m_rTexs[0], forward, volume.transform, volume.data);
        dirImages[1].texture = _getSlicerTex(m_rTexs[1], right, volume.transform, volume.data);
        dirImages[2].texture = _getSlicerTex(m_rTexs[2], top, volume.transform, volume.data);

    }
    private RenderTexture _getSlicerTex(RenderTexture m_rTex, Transform dirObj, Transform volume, Texture3D texture)
    {
        int kernel = shader.FindKernel("CSMain");

        shader.SetVector("v_row0", volume.worldToLocalMatrix.GetRow(0));
        shader.SetVector("v_row1", volume.worldToLocalMatrix.GetRow(1));
        shader.SetVector("v_row2", volume.worldToLocalMatrix.GetRow(2));
        shader.SetVector("v_row3", volume.worldToLocalMatrix.GetRow(3));

        shader.SetVector("p_row0", dirObj.localToWorldMatrix.GetRow(0));
        shader.SetVector("p_row1", dirObj.localToWorldMatrix.GetRow(1));
        shader.SetVector("p_row2", dirObj.localToWorldMatrix.GetRow(2));
        shader.SetVector("p_row3", dirObj.localToWorldMatrix.GetRow(3));

        shader.SetTexture(kernel, "_Volume", texture);
        shader.SetTexture(kernel, "Result", m_rTex);
        shader.Dispatch(kernel, 512 / 8, 512 / 8, 1);
       
        return m_rTex;
    }

    private void _setDirPlaneTrans(Transform volume)
    {
        forward.localScale = Vector3.one * volume.localScale.x;
        right.localScale = Vector3.one * volume.localScale.x;
        top.localScale = Vector3.one * volume.localScale.x;
        forward.forward = volume.forward;
        right.forward = -volume.right;
        top.forward = -volume.up;
        forward.position = volume.position;
        right.position = volume.position;
        top.position = volume.position;
    }
    bool isFin;
    int nx;
    List<float> values = new List<float>();
    List<int> indexs = new List<int>();
    System.DateTime a;
    void changeT3D(Vector2 scale)
    {
        indexs.Clear();
        values.Clear();
        for (int i = 0; i < tempcolors.Length; i++)
        {
            if (tempcolors[i].g == 1)
                tempcolors[i].g = 0;
        }
        int z = (int)(volume.data.depth * (0.5f));
        int x = (int)(nx * (scale.x));
        int y= (int)(nx * (scale.y));
        a = System.DateTime.Now;
        //Debug.Log(a);
        for (int i = x-1; i <= x+1; i++)
        {
            for (int j = y-1; j <= y+1; j++)
            {
                values.Add(colors[nx * nx * z + i + nx * j].r);
            }
        }
        Thread t = new Thread(delegate ()
        {
            floodFill(x, y, z, new Vector3(x, y, z), values, indexs);
            isFin = true;
        }, int.MaxValue);
        t.Start();

    }
    public float _clamp;
    void changT2D()
    {
        if (posList.Count >= 2000) return;
        dirImages[0].material.SetVectorArray("_pos", p);
        dirImages[0].material.SetInt("_pos_Num", posList.Count/2);
        dirImages[0].material.SetFloat("_clamp", _clamp);
    }
    [Range(0,100)]
    public float thresold = 20;
    public Transform sphere;
    void deletePixels()
    {
        Vector2 p = inputMouse(Input.mousePosition);
        Vector2 p1 = inputMouse(new Vector2(Input.mousePosition.x+thresold,Input.mousePosition.y+thresold));
        int z = (int)(volume.data.depth * (0.5f));
        int x = (int)(nx * (p.x));
        int y = (int)(nx * ((p.y + volume.transform.localScale.y/volume.transform.localScale.x/2-0.5f)/volume.transform.localScale.y));
        int ny = info.ny;

        int clamp = Mathf.Abs((int)(nx * (p1.x)) - x);
        for (int i = x - clamp; i <= x + clamp; i++)
        {
            for (int j = y - clamp; j <= y + clamp; j++)
            {
                if (Vector2.Distance(new Vector2(x, y), new Vector2(i, j)) <= clamp)
                {
                    colors[nx * nx * z + i + nx * j].g = 1;


                }
            }
        }
        _clamp = Mathf.Abs(p.x - p1.x);
        if (posList.Contains(p)) return;
        posList.Add(p);
       
        if (posList.Count % 2 == 0)
        {
            this.p[posList.Count / 2 - 1] = new Vector4(posList[posList.Count - 2].x, posList[posList.Count - 2].y, posList[posList.Count - 1].x, posList[posList.Count - 1].y);
        }
        else
        {
            this.p[posList.Count / 2 ] = new Vector4(posList[posList.Count - 1].x, posList[posList.Count - 1].y, posList[posList.Count - 1].x, posList[posList.Count - 1].y);
        }
        changT2D();
    }
    Vector2 inputMouse(Vector2 point)
    {
        Vector2 pos = dirImages[0].transform.worldToLocalMatrix.MultiplyPoint(point);
        Vector2 scale = new Vector2((pos.x + 420) / 840.0f, (pos.y + 420) / 840.0f);
        return scale;
    }
    List<int> poi = new List<int>();
    Color[] tempcolors = new Color[512*512*256];
    void floodFill(int x,int y,int z,Vector3 pos, List<float> value,List<int> indexs)
    {
        if (x >= 0 && x < nx && y >= 0 && y < nx &&/*!poi.Contains(512 * 512 * z + x + 512 * y)*/ tempcolors[nx * nx * z + x + nx * y].g != 1/*&&Vector3.Distance(pos,new Vector3(x,y,z))<30*/)
        {
            for (int i = 0; i < value.Count; i++)
            {
                int p = nx * nx * z + x + nx * y;
                float temp = colors[p].r;
                if(Mathf.Abs(temp - value[i]) < 0.005f)
                {
                    //poi.Add(p);
                    colors[p].g = 1;
                    tempcolors[p].g = 1;
                    //col2ds[x + 512 * y].g = 1;
                    indexs.Add(p);
                    floodFill(x - 1, y, z,pos,value, indexs);
                    floodFill(x + 1, y, z,pos,value, indexs);
                    floodFill(x, y - 1, z,pos,value, indexs);
                    floodFill(x, y + 1, z,pos,value, indexs);
                    floodFill(x, y, z - 1,pos, value, indexs);
                    floodFill(x, y, z + 1,pos, value, indexs);
                    break;
                }
            }
           
        }
    }
    void points()
    {
        int sum = 0;
        for (int i = 0; i < colors.Length; i++)
        {
            if (colors[i].g == 1)
            {
                sum++;
            }
        }
        Debug.Log(sum);
    }
}
