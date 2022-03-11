using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using System;
using System.IO;
using System.Globalization;

public class ConebeamBackprojectReconstruction : MonoBehaviour {

    public Camera camera;
    public Transform quad;
    public Transform cube;
    public Camera renderCam;
    public MeshRenderer show;

    RenderTexture rt;

    List<Texture2D> textures;
    List<Matrix4x4> matrixs;


    float[] project;

    int isize = 256;
    int vsize = 128;

    int nx = 0;
    int ny = 0;
    int nz = 0;

    void Start () { 
        Matrix4x4 m = camera.projectionMatrix;
        m.m00 = 2 / quad.localScale.x;
        m.m11 = 2 / quad.localScale.y;
        m.m02 = quad.position.x * m.m00 / 2;
        m.m12 = quad.position.y * m.m11 / 2; 
        camera.projectionMatrix = m;


        Texture3D tex = creatTex();
        quad.GetComponent<MeshRenderer>().material.SetTexture("_VolumeTex", tex);

        rt = new RenderTexture(isize, isize, 24, RenderTextureFormat.ARGBFloat);

        textures = new List<Texture2D>();
        matrixs = new List<Matrix4x4>();

        project = new float[vsize * vsize * vsize];
    }
	 
	void Update () {
        if (textures.Count < 360)
        { 
            renderCam.targetTexture = rt;
            renderCam.Render();
            RenderTexture.active = rt;
            Texture2D t = new Texture2D(isize, isize, TextureFormat.RGBAFloat, false);
            t.ReadPixels(new UnityEngine.Rect(0, 0, isize, isize), 0, 0);
            t.Apply();
            show.material.mainTexture = t;
            textures.Add(t);
            matrixs.Add(quad.localToWorldMatrix);
            camera.transform.RotateAround(cube.position, Vector3.up, 0.5f);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            backProjection();
        }
        quad.GetComponent<MeshRenderer>().material.SetMatrix("_cubeWorld2Local", cube.worldToLocalMatrix);
    } 

    Texture3D creatTex()
    {
        Texture3D tex = new Texture3D(vsize, vsize, vsize, TextureFormat.Alpha8, false);
        Color[] col = new Color[vsize * vsize * vsize];
        Vector3 center = new Vector3(vsize / 2, vsize / 2, vsize / 2);
        for (int x = 0; x < vsize; x++)
        {
            for (int y = 0; y < vsize; y++)
            {
                for (int z = 0; z < vsize; z++)
                {
                    Vector3 v = new Vector3(x+16, y, z);
                    float dis = Vector3.Distance(v, center);
                    if (dis < vsize * 0.375 && dis > vsize * 0.125)
                    {
                        col[z * vsize * vsize + x * vsize + y] = new Color(0, 0, 0, 0.7f);
                    }
                    else
                    {
                        col[z * vsize * vsize + x * vsize + y] = new Color(0, 0, 0, 0);
                    }
                }
            }
        }
        tex.SetPixels(col);
        tex.Apply();
        return tex;
    }

    void backProjection()
    {
        Debug.Log(DateTime.Now);
        for (int n = 0; n < textures.Count; n++)
        {
            Color[] c = textures[n].GetPixels();
            double[] ds = new double[c.Length];
            Mat src = new Mat(isize, isize, CvType.CV_64FC1);
            for (int i = 0; i < c.Length; i++)
            {
                float x = (i % isize - isize / 2) * 0.2f / isize;
                float y = (i / isize - isize / 2) * 0.2f / isize; ;
                float s = 0.5f / Mathf.Sqrt(0.5f * 0.5f + x * x + y * y);
                s = 1;
                ds[i] = s * c[i].r;
            }
            src.put(0, 0, ds);
            //傅里叶变换
            Mat fft = new Mat();
            Core.dft(src, fft);

            //低通滤波
            for (int i = 0; i < isize / 2; i++)
            {
                for (int j = 0; j < isize / 2; j++)
                {
                    if (Mathf.Sqrt(i * i + j * j) >= isize / 4)
                    {
                        fft.put(i, j, 0);
                        fft.put(i, isize - j, 0);
                        fft.put(isize - i, j, 0);
                        fft.put(isize - i, isize - j, 0);
                    }
                }
            }
            //傅里叶逆变换
            Mat ifft = new Mat();
            Core.idft(fft, ifft);

            //反投影 拉东变换
            for (int x = 0; x < vsize; x++)
            {
                for (int y = 0; y < vsize; y++)
                {
                    for (int z = 0; z < vsize; z++)
                    {
                        int lx = x - vsize / 2;
                        int ly = y - vsize / 2;
                        int lz = z - vsize / 2;
                        float rad = Mathf.Rad2Deg * n/2;
                        float t = (lx * Mathf.Cos(rad) + ly * Mathf.Sin(rad)) * 0.05f / vsize;
                        float s = (ly * Mathf.Cos(rad) - lx * Mathf.Sin(rad)) * 0.05f / vsize;
                        float u = 1f / (0.5f + s);
                        float p = u * t;
                        float q = u * lz * 0.05f / vsize;

                        int wx = (int)(p / 0.2f * isize) + isize / 2;
                        int wy = (int)(q / 0.2f * isize) + isize / 2;

                        project[y * vsize * vsize + z * vsize + x] += (float)(ifft.get(wy, wx)[0]);
                    }

                }
            }
        }

        float min = float.MaxValue;
        float max = float.MinValue;
        for (int i = 0; i < project.Length; i++)
        {
            min = min < project[i] ? min : project[i];
            max = max > project[i] ? max : project[i];
        }
        Debug.Log(min + "   " + max);
        Texture3D tex = new Texture3D(vsize, vsize, vsize, TextureFormat.RGBAFloat, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Color[] col = new Color[vsize * vsize * vsize];
        for (int x = 0; x < vsize; x++)
        {
            for (int y = 0; y < vsize; y++)
            {
                for (int z = 0; z < vsize; z++)
                {
                    int index = z * vsize * vsize + x * vsize + y;
                    float value = (project[index] - min) / (max - min);
                    col[index] = new Color(value, value, value, value);
                }
            }
        }
        tex.SetPixels(col);
        tex.Apply();
        quad.GetComponent<MeshRenderer>().material.SetTexture("_VolumeTex", tex);
        Debug.Log(DateTime.Now);

    }

    Texture3D dcmReader(string path)
    {
        Dictionary<string, string> tags = new Dictionary<string, string>();
        string[] paths = Directory.GetFiles(path);
        //int nx = 0;
        //int ny = 0;
        //int nz = paths.Length;
        //for (int n = 0; n < 1; n++)
        //{ 
        tags = VolumeViewer.DICOM.getElementsOfFile(paths[0], false);
        int numChannels = 1;
        if (tags.ContainsKey("SamplesPerPixel"))
        {
            numChannels = int.Parse(tags["SamplesPerPixel"], CultureInfo.InvariantCulture);
        }
        if (!tags.ContainsKey("BitsAllocated"))
        {
            Debug.Log("Error: No BitsAllocated element present!");
            return null;
        }
        int bitsStored = int.Parse(tags["BitsAllocated"], CultureInfo.InvariantCulture);
        int bytesPerChannel = (int)Math.Ceiling(bitsStored / 8.0);
        int bytesPerVoxel = bytesPerChannel * numChannels;
        int pixelRepresentation = 0;
        if (tags.ContainsKey("PixelRepresentation"))
        {
            pixelRepresentation = int.Parse(tags["PixelRepresentation"], CultureInfo.InvariantCulture);
        }
        VolumeViewer.DICOM.FormatCode format = VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UNKNOWN;
        if (numChannels == 4 && bytesPerVoxel == 4)
        {
            format = VolumeViewer.DICOM.FormatCode.DICOM_TYPE_RGBA32;
        }
        else if (numChannels == 3 && bytesPerVoxel == 3)
        {
            format = VolumeViewer.DICOM.FormatCode.DICOM_TYPE_RGB24;
        }
        else if (numChannels == 1)
        {
            if (bytesPerVoxel == 1)
            {
                format = pixelRepresentation == 0 ? VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT8 : VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT8;
            }
            else if (bytesPerVoxel == 2)
            {
                format = pixelRepresentation == 0 ? VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT16 : VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT16;
            }
            else if (bytesPerVoxel == 4)
            {
                if (tags.ContainsKey("PixelDataAt"))
                {
                    format = pixelRepresentation == 0 ? VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT32 : VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT32;
                }
                else if (tags.ContainsKey("FloatPixelDataAt"))
                {
                    format = VolumeViewer.DICOM.FormatCode.DICOM_TYPE_FLOAT32;
                }
            }
        }
        if (format == VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UNKNOWN)
        {
            Debug.Log("Error: Unsupported DICOM format.");
            return null;
        }

        nx = int.Parse(tags["Columns"], CultureInfo.InvariantCulture);
        ny = int.Parse(tags["Rows"], CultureInfo.InvariantCulture);
        Color[] colors = new Color[nx * ny * nz];
        for (int n = 0; n < nz; n++)
        {
            BinaryReader imgReader = new BinaryReader(File.Open(paths[n], FileMode.Open));
            string[] pixDataLocLen = tags["PixelDataAt"].Split('\\');
            int pixelDataIndex = int.Parse(pixDataLocLen[0], CultureInfo.InvariantCulture);
            imgReader.BaseStream.Seek(pixelDataIndex, SeekOrigin.Begin);
            byte[] bytes = imgReader.ReadBytes(bytesPerVoxel * nx * ny);

            int byteIndex = 0;
            float maxValue = float.MinValue;
            float minValue = float.MaxValue;
            switch (format)
            {
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UNKNOWN:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_BINARY:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT8:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT16:
                    for (int y = 0; y < ny; y++)
                    {
                        for (int x = 0; x < nx; x++)
                        {
                            colors[x + nx * y + nx * ny * n] = new Color(BitConverter.ToInt16(bytes, byteIndex), 0, 0, 0);
                            if (maxValue < colors[x + nx * y].r)
                            {
                                maxValue = colors[x + nx * y].r;
                            }
                            if (minValue > colors[x + nx * y].r)
                            {
                                minValue = colors[x + nx * y].r;
                            }
                            byteIndex += 2;
                        }
                    }
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT32:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_FLOAT32:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_RGB24:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_INT8:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT16:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UINT32:
                    break;
                case VolumeViewer.DICOM.FormatCode.DICOM_TYPE_RGBA32:
                    break;
                default:
                    break;
            }
            //Texture2D tex = new Texture2D(nx, ny, TextureFormat.RGBA32, false);
            //tex.wrapMode = TextureWrapMode.Clamp;
            //for (int i = 0; i < colors.Length; i++)
            //{
            //    float valueRange = maxValue - minValue;
            //    colors[i].r = (colors[i].r + 1000) / 4000;
            //    colors[i].g = colors[i].r;
            //    colors[i].b = colors[i].r;
            //    colors[i].a = 1;
            //}
            //tex.SetPixels(colors);
            //tex.Apply();

        }
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].r = (colors[i].r + 1000) / 4000;
            colors[i].g = colors[i].r;
            colors[i].b = colors[i].r;
            colors[i].a = colors[i].r;
        }
        Texture3D tex3d = new Texture3D(nx, ny, nz, TextureFormat.RGBA32, false);
        tex3d.SetPixels(colors);
        tex3d.Apply();
        return tex3d;
    }
}
