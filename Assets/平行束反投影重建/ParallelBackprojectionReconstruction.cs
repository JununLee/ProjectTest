using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using System.IO;
using System.Globalization;

public class ParallelBackprojectionReconstruction : MonoBehaviour {

    public Transform camera;
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


        //Texture3D tex = creatTex();
        Texture3D tex = dcmReader("E:\\Data\\lxm");
        quad.GetComponent<MeshRenderer>().material.SetTexture("_VolumeTex", tex);

        rt = new RenderTexture(isize, isize, 24, RenderTextureFormat.ARGBFloat);

        textures = new List<Texture2D>();
        matrixs = new List<Matrix4x4>();

        project = new float[vsize * vsize * vsize];

         


    }

    void Update () {
        if (Input.GetKey(KeyCode.S))
        {

            if (textures.Count < 180)
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
                camera.RotateAround(cube.position, Vector3.up, 1f);
                //saveTextureToPng(t, textures.Count.ToString());
            }
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            backProjection();
        }
        quad.GetComponent<MeshRenderer>().material.SetMatrix("_cubeWorld2Local", cube.worldToLocalMatrix);
    }
    private void OnRenderObject()
    {

    }

    Texture3D creatTex()
    {
        Texture3D tex = new Texture3D(vsize, vsize, vsize, TextureFormat.Alpha8, false);
        Color[] col = new Color[vsize * vsize * vsize];
        Vector3 center = new Vector3(vsize/2, vsize/2, vsize/2);
        for (int x = 0; x < vsize; x++)
        {
            for (int y = 0; y < vsize; y++)
            {
                for (int z = 0; z < vsize; z++)
                {
                    Vector3 v = new Vector3(x, y, z);
                    float dis = Vector3.Distance(v, center);
                    if (dis < vsize*0.375 && dis> vsize * 0.125)
                    {
                        col[z * vsize * vsize + x * vsize + y] = new Color(0, 0, 0, 0.7f);
                    }
                    else
                    {
                        col[z * vsize * vsize + x * vsize + y] = new Color(0,0,0,0);
                    }
                }
            }
        }
        tex.SetPixels(col);
        tex.Apply();
        return tex;
    }
     
    bool cubeIntersection(Vector3 origin, Vector3 direction, Vector3 aabbMax, Vector3 aabbMin, out float tNear, out float tFar)
    {
        if (direction.x == 0)
        {
            return boxIntersection(new Vector2(origin.y, origin.z), new Vector2(direction.y, direction.z), Vector2.one / 2, -Vector2
                .one / 2, out tNear, out tFar) && Mathf.Abs(origin.x) <= 0.5;
        }
        else if(direction.y == 0)
        {
            return boxIntersection(new Vector2(origin.x, origin.z), new Vector2(direction.x, direction.z), Vector2.one / 2, -Vector2
                .one / 2, out tNear, out tFar) && Mathf.Abs(origin.y) <= 0.5;
        }
        else if (direction.z == 0)
        {
            return boxIntersection(new Vector2(origin.x, origin.y), new Vector2(direction.x, direction.y), Vector2.one / 2, -Vector2
                .one / 2, out tNear, out tFar) && Mathf.Abs(origin.z) <= 0.5;
        }
        
        Vector3 invDir = new Vector3(1 / direction.x, 1 / direction.y, 1 / direction.z);
        Vector3 t1 = aabbMax - origin;
        t1 = new Vector3(invDir.x * t1.x, invDir.y * t1.y, invDir.z * t1.z);
        Vector3 t2 = aabbMin - origin;
        t2 = new Vector3(invDir.x * t2.x, invDir.y * t2.y, invDir.z * t2.z);
        Vector3 tMin = new Vector3(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y), Mathf.Min(t1.z, t2.z));
        Vector3 tMax = new Vector3(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y), Mathf.Max(t1.z, t2.z));
        tNear = Mathf.Max(Mathf.Max(tMin.x, tMin.y), tMin.z); 
        tFar = Mathf.Min(Mathf.Min(tMax.x, tMax.y), tMax.z); 
        return tNear <= tFar; 
    }
    bool boxIntersection(Vector2 origin, Vector2 direction, Vector2 aabbMax, Vector2 aabbMin, out float tNear, out float tFar)
    {
        if (direction.x == 0)
        {
            float inv = 1 / direction.y;
            float t2 = (aabbMin.y - origin.y) * inv;
            float t1 = (aabbMax.y - origin.y) * inv;
            tNear = Mathf.Min(t1, t2);
            tFar = Mathf.Max(t1, t2);
            return Mathf.Abs(origin.x) <= 0.5;
        }
        else if (direction.y == 0)
        {
            float inv = 1 / direction.x;
            float t2 = (aabbMin.x - origin.x) * inv;
            float t1 = (aabbMax.x - origin.x) * inv;
            tNear = Mathf.Min(t1, t2);
            tFar = Mathf.Max(t1, t2);
            return Mathf.Abs(origin.y) <= 0.5;
        }
        else
        {
            Vector2 invDir = new Vector2(1 / direction.x, 1 / direction.y);
            Vector3 t1 = aabbMax - origin;
            t1 = new Vector2(invDir.x * t1.x, invDir.y * t1.y);
            Vector2 t2 = aabbMin - origin;
            t2 = new Vector2(invDir.x * t2.x, invDir.y * t2.y);
            Vector2 tMin = new Vector2(Mathf.Min(t1.x, t2.x), Mathf.Min(t1.y, t2.y));
            Vector2 tMax = new Vector2(Mathf.Max(t1.x, t2.x), Mathf.Max(t1.y, t2.y));
            tNear = Mathf.Max(tMin.x, tMin.y);
            tFar = Mathf.Min(tMax.x, tMax.y);
            return tNear <= tFar;
        }
    }

    void backProjection()
    {
        Debug.Log(DateTime.Now);
        for (int n = 0; n <textures.Count; n++)
        {
            Color[] c = textures[n].GetPixels();
            double[] ds = new double[c.Length];
            Mat src = new Mat(isize, isize, CvType.CV_64FC1);
            for (int i = 0; i < c.Length; i++)
            {
                ds[i] = c[i].r;
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

            float rad = Mathf.Deg2Rad * n;
            //Debug.Log("RAD:  " + rad);
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

                        int wy = ly + isize / 2;
                        int wx = isize / 2;
                        //if (lx == 0 && lz == 0)
                        //{
                        //    project[z * vsize * vsize + y * vsize + x] += (float)(ifft.get(wy, wx)[0]) / (isize * isize * 180);
                        //    continue;
                        //}

                        //float r = Mathf.Sqrt(lx * lx + lz * lz);
                        //float theta = Mathf.Acos(Mathf.Abs(lx / r));
                        //if (lx >= 0 && lz >= 0)
                        //{

                        //}
                        //else if (lx <= 0 && lz >= 0)
                        //{
                        //    theta = Mathf.PI - theta;
                        //}
                        //else if (lx <= 0 && lz <= 0)
                        //{
                        //    theta = Mathf.PI + theta;
                        //}
                        //else if (lx >= 0 && lz <= 0)
                        //{ 
                        //    theta = 2 * Mathf.PI - theta;
                        //}
                        //wx = (int)(r * Mathf.Cos(theta - rad)) + isize / 2;
                        wx = (int)(lx * Mathf.Cos(rad) + lz * Mathf.Sin(rad)) + isize / 2;
             
                        project[z * vsize * vsize + y * vsize + x] += (float)(ifft.get(wy, wx)[0]) / (isize * isize * 180);
                             
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
         nz = paths.Length;
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

        float maxValue = float.MinValue;
        float minValue = float.MaxValue;
        nx = int.Parse(tags["Columns"], CultureInfo.InvariantCulture);
        ny = int.Parse(tags["Rows"], CultureInfo.InvariantCulture);
        Color[] colors = new Color[nx * ny * nz];
        for (int n = 0; n < nz; n++)
        {
            tags = VolumeViewer.DICOM.getElementsOfFile(paths[n], false);
            numChannels = 1;
            if (tags.ContainsKey("SamplesPerPixel"))
            {
                numChannels = int.Parse(tags["SamplesPerPixel"], CultureInfo.InvariantCulture);
            }
            if (!tags.ContainsKey("BitsAllocated"))
            {
                Debug.Log("Error: No BitsAllocated element present!");
                return null;
            }
            bitsStored = int.Parse(tags["BitsAllocated"], CultureInfo.InvariantCulture);
            bytesPerChannel = (int)Math.Ceiling(bitsStored / 8.0);
            bytesPerVoxel = bytesPerChannel * numChannels;
            pixelRepresentation = 0;
            if (tags.ContainsKey("PixelRepresentation"))
            {
                pixelRepresentation = int.Parse(tags["PixelRepresentation"], CultureInfo.InvariantCulture);
            }
            format = VolumeViewer.DICOM.FormatCode.DICOM_TYPE_UNKNOWN;
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
            BinaryReader imgReader = new BinaryReader(File.Open(paths[n], FileMode.Open));
            string[] pixDataLocLen = tags["PixelDataAt"].Split('\\');
            int pixelDataIndex = int.Parse(pixDataLocLen[0], CultureInfo.InvariantCulture);
            imgReader.BaseStream.Seek(pixelDataIndex, SeekOrigin.Begin);
            byte[] bytes = imgReader.ReadBytes(bytesPerVoxel * nx * ny);

            int byteIndex = 0;
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

        }
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].r = (colors[i].r - minValue) / (maxValue - minValue);
            colors[i].g = colors[i].r;
            colors[i].b = colors[i].r;
            colors[i].a = colors[i].r;
        }
        Texture3D tex3d = new Texture3D(nx, ny, nz, TextureFormat.RGBAHalf, false);
        tex3d.wrapMode = TextureWrapMode.Clamp;
        tex3d.SetPixels(colors);
        tex3d.Apply();
        return tex3d;
    }

    void saveTextureToPng(Texture2D png, string pngName)
    {
        byte[] bytes = png.EncodeToPNG();
        File.WriteAllBytes("E://Data/PNG" + "/" + pngName + ".png", bytes);
    }
}
