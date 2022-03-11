using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;

public class dcmReader : MonoBehaviour {

    public string path;
    public MeshRenderer render;
    public Texture2D t;
    Dictionary<string, string> tags = new Dictionary<string, string>();
    private void Start()
    {
        //path = Application.streamingAssetsPath + "/ImageFileName000.dcm";
        string[] paths = Directory.GetFiles(Path.GetDirectoryName(path));
        for (int n = 0; n < 1; n++)
        {
       
        
        tags = VolumeViewer.DICOM.getElementsOfFile(paths[n], false);
        int numChannels = 1;
        if (tags.ContainsKey("SamplesPerPixel"))
        {
            numChannels = int.Parse(tags["SamplesPerPixel"], CultureInfo.InvariantCulture);
        }
        if (!tags.ContainsKey("BitsAllocated"))
        {
            Debug.Log("Error: No BitsAllocated element present!");
            return;
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
            return;
        }

        int nx = int.Parse(tags["Columns"], CultureInfo.InvariantCulture);
        int ny = int.Parse(tags["Rows"], CultureInfo.InvariantCulture);
        Color[] colors = new Color[nx * ny];

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
                        colors[x+nx*y] = new Color(BitConverter.ToInt16(bytes, byteIndex), 0, 0, 0);
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
        Texture2D tex = new Texture2D(nx, ny, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int i = 0; i < colors.Length; i++)
        {
            float valueRange = maxValue - minValue;
            colors[i].r = (colors[i].r + 1000) / 4000;
            colors[i].g = colors[i].r;
            colors[i].b = colors[i].r;
            colors[i].a = 1;
        }
        tex.SetPixels(colors);
        tex.Apply();
        render.material.mainTexture = tex;
        //saveTextureToPng(tex, (n+1).ToString());
        }
    }
    void saveTextureToPng(Texture2D png,string pngName)
    {
        byte[] bytes = png.EncodeToPNG();
        File.WriteAllBytes("G://Data/PNG" + "/" + pngName + ".png", bytes);
    }
}
