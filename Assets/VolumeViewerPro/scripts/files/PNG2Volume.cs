///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Class:              PNG2Volume
/// Description:        Loading PNG files into a volume object.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
///-----------------------------------------------------------------

#if !NETFX_CORE
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VolumeViewer
{
    public class PNG2Volume
    {
        public Volume volume;

		public IEnumerator loadFile(string fName, VolumeTextureFormat forceTextureFormat, FloatEvent loadingProgressChanged, Ref<bool> completed, Ref<int> returned)
        {
            returned.val = 1;
            fName = Path.GetFullPath(fName);
            if (!File.Exists(fName))
            {
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            byte[] texBytes;
            try
            {
                texBytes = File.ReadAllBytes(fName);
            }
            catch (IOException) {
                Debug.Log("IOException: " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            catch (UnauthorizedAccessException) {
                Debug.Log("UnauthorizedAccessException: " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            catch (NotSupportedException) {
                Debug.Log("NotSupportedException: " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            catch (ArgumentException) {
                Debug.Log("ArgumentException: " + fName);
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            string fExt = Path.GetExtension(fName);
            string fDir = Path.GetDirectoryName(fName);
            List<string> associatedFileNames = new List<string>();
            string[] filePaths = Directory.GetFiles(fDir);
            foreach (string filePath in filePaths)
            {
                if (!Path.GetExtension(filePath).Equals(fExt))
                {
                    continue;
                }
                associatedFileNames.Add(Path.GetFileNameWithoutExtension(filePath));
            }
            Texture2D tex2D = new Texture2D(1, 1);
            if(!tex2D.LoadImage(texBytes))
            {
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            int nxImg = tex2D.width;
            int nyImg = tex2D.height;
            int nzImg = associatedFileNames.Count;
            volume = new Volume();
            volume.format = tex2D.format;
            switch (forceTextureFormat)
            { 
                case VolumeTextureFormat.Alpha8:
                    volume.format = TextureFormat.Alpha8;
                    break;
                case VolumeTextureFormat.RFloat:
                    volume.format = TextureFormat.RFloat;
                    break;
                case VolumeTextureFormat.RHalf:
                    volume.format = TextureFormat.RHalf;
                    break;
                case VolumeTextureFormat.RGB24:
                    volume.format = TextureFormat.RGB24;
                    break;
                case VolumeTextureFormat.RGBA32:
                    volume.format = TextureFormat.RGBA32;
                    break;
                case VolumeTextureFormat.ARGB32:
                    volume.format = TextureFormat.ARGB32;
                    break;
                case VolumeTextureFormat.RGBAFloat:
                    volume.format = TextureFormat.RGBAFloat;
                    break;
                case VolumeTextureFormat.RGBAHalf:
                    volume.format = TextureFormat.RGBAHalf;
                    break;
            }
            volume.nx = Mathf.NextPowerOfTwo(nxImg);
            volume.ny = Mathf.NextPowerOfTwo(nyImg);
            volume.nz = Mathf.NextPowerOfTwo(nzImg);
            int startX = (int)Mathf.Floor((volume.nx - nxImg) / 2.0f);
            int startY = (int)Mathf.Floor((volume.ny - nyImg) / 2.0f);
            int startZ = (int)Mathf.Floor((volume.nz - nzImg) / 2.0f);
            volume.numVoxels = volume.nx * volume.ny * volume.nz;
            Color[] volColors = new Color[volume.numVoxels];
            try
            {
                volume.texture = new Texture3D(volume.nx, volume.ny, volume.nz, volume.format, false);
            }
            catch (UnityException)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            if (volume.texture == null)
            {
                Debug.Log("Couldn't create Texture3D(" + volume.nx + ", " + volume.ny + ", " + volume.nz + ", " + volume.format + ", " + "false).\nTry forcing a different format in the VolumeFileLoader component.");
                returned.val = 0;
                completed.val = true;
                yield break;
            }
            associatedFileNames.Sort(new NumStrComparer());
            int iy, iz;
            int stopZ = startZ + nzImg;
            int stopY = startY + nyImg;
            for (int z = startZ; z < stopZ; z++)
            {
                iz = z - startZ;
                try
                {
                    texBytes = File.ReadAllBytes(fDir + Path.DirectorySeparatorChar + associatedFileNames[iz] + fExt);
                }
                catch (IOException)
                {
                    Debug.Log("IOException: " + associatedFileNames[iz] + fExt);
                    returned.val = 0;
                    completed.val = true;
                    yield break;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.Log("UnauthorizedAccessException: " + associatedFileNames[iz] + fExt);
                    returned.val = 0;
                    completed.val = true;
                    yield break;
                }
                catch (NotSupportedException)
                {
                    Debug.Log("NotSupportedException: " + associatedFileNames[iz] + fExt);
                    returned.val = 0;
                    completed.val = true;
                    yield break; ;
                }
                catch (ArgumentException)
                {
                    Debug.Log("ArgumentException: " + associatedFileNames[iz] + fExt);
                    returned.val = 0;
                    completed.val = true;
                    yield break;
                }
                tex2D.LoadImage(texBytes);
                Color[] tex2DColor = tex2D.GetPixels();
                for (int y = startY; y < stopY; y++)
                {
                    iy = y - startY;
                    Array.Copy(tex2DColor, iy * nxImg, volColors, z * volume.ny * volume.nx + y * volume.nx + startX, nxImg);
                }
                loadingProgressChanged.Invoke(iz / (float) nzImg);
                yield return null;
            }
            volume.texture.SetPixels(volColors);
			volume.texture.filterMode = FilterMode.Trilinear;
            volume.texture.wrapMode = TextureWrapMode.Clamp;
            volume.texture.Apply();

            completed.val = true;
        }
    }
}
#endif