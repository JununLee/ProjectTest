////--------------------------------------------------------------------
/// Namespace:          
/// Struct:             VolumeHelper
/// Description:        Static helper functions.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the examples of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
////--------------------------------------------------------------------

using System.Collections;
using UnityEngine;

public struct VolumeHelper
{
    public static IEnumerator getGrayscaleFromVolume(Texture3D volume, VolumeViewer.Ref<float[,,]> grayscale, Color channelWeight, VolumeViewer.Ref<bool> completed)
    {
        completed.val = false;
        int nx = volume.width;
        int ny = volume.height;
        int nz = volume.depth;
        Color[] volColors;
        switch (volume.format)
        {
            case TextureFormat.Alpha8:
            case TextureFormat.ARGB32:
            case TextureFormat.RGBA32:
            case TextureFormat.RGBAFloat:
            case TextureFormat.RGBAHalf:
                //As of 5.5.0 this Exception can't be caught using try/catch or using if(volColors == null || volColors.Length < nx * ny * nz)
                volColors = volume.GetPixels();
                break;
            default:
                Debug.Log("Error in VolumeHelper.getGrayscaleFromVolume(): " + volume.format + " not supported by Texture3D.GetPixels(). Try forcing one of these formats: Alpha8, ARGB32, RGBA32, RGBAFloat, RGBAHalf.");
                yield break;
        }
        grayscale.val = new float[nx, ny, nz];
        float[,,] gray = grayscale.val;
        int index = 0;
        for (int z = 0; z < nz; z++)
        {
            //yield return in the outer most loop to keep project responsive.
            yield return null;
            for (int y = 0; y < ny; y++)
            {
                for (int x = 0; x < nx; x++)
                {
                    Color thisColor = volColors[index] * channelWeight;
                    gray[x, y, z] = Mathf.Max(thisColor.maxColorComponent, thisColor.a);
                    index++;
                }
            }
        }
        completed.val = true;
    }

    public static IEnumerator getGradientFromGrayscale(float[,,] grayscale, VolumeViewer.Ref<float[,,]> gradient, Color channelWeight, VolumeViewer.Ref<bool> completed)
    {
        completed.val = false;
        int nx = grayscale.GetLength(0);
        int ny = grayscale.GetLength(1);
        int nz = grayscale.GetLength(2);
        gradient.val = new float[nx, ny, nz];
        float[,,] grad = gradient.val;
        Vector3 negSample, posSample;
        int xn, xp, yn, yp, zn, zp;
        for (int z = 0; z < nz; z++)
        {
            //yield return in the outer most loop to keep project responsive.
            yield return null;
            zn = Mathf.Max(z - 1, 0);
            zp = Mathf.Min(z + 1, nz - 1);
            for (int y = 0; y < ny; y++)
            {
                yn = Mathf.Max(y - 1, 0);
                yp = Mathf.Min(y + 1, ny - 1);
                for (int x = 0; x < nx; x++)
                {
                    xn = Mathf.Max(x - 1, 0);
                    xp = Mathf.Min(x + 1, nx - 1);
                    negSample.x = grayscale[xn, y, z];
                    posSample.x = grayscale[xp, y, z];
                    negSample.y = grayscale[x, yn, z];
                    posSample.y = grayscale[x, yp, z];
                    negSample.z = grayscale[x, y, zn];
                    posSample.z = grayscale[x, y, zp];
                    grad[x, y, z] = Mathf.Min((posSample - negSample).magnitude, 1.0f);
                }
            }
        }
        completed.val = true;
    }

    public static IEnumerator generateHistogramFromVolume(float[,,] volume, float[,,] gradient, int numCellsVolume, int numCellsGradient, VolumeViewer.Ref<float[,]> histogram, VolumeViewer.Ref<bool> completed)
    {
        completed.val = false;
        int nx = volume.GetLength(0);
        int ny = volume.GetLength(1);
        int nz = volume.GetLength(2);
        histogram.val = new float[numCellsVolume, numCellsGradient];
        numCellsVolume -= 1;
        numCellsGradient -= 1;
        float[,] hist2D = histogram.val;
        for (int z = 0; z < nz; z++)
        {
            yield return null;
            for (int y = 0; y < ny; y++)
            {
                for (int x = 0; x < nx; x++)
                {
                    hist2D[(int)(volume[x, y, z] * numCellsVolume + UnityEngine.Random.Range(-0.75f, 0.75f)), (int)(gradient[x, y, z] * numCellsGradient + UnityEngine.Random.Range(-0.75f, 0.75f))]++;
                }
            }
        }
        completed.val = true;
    }

    public static float estimateCap(float[,] values, float voxelSum, float threshold)
    {
        int nx = values.GetLength(0);
        int ny = values.GetLength(1);
        float numCrossings = 0;
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                numCrossings += values[x, y] >= threshold ? 1 : 0;
            }
        }
        return voxelSum / numCrossings * 5.0f;
    }

    public static void capOffHistogram(ref float[,] values, float cap)
    {
        int nx = values.GetLength(0);
        int ny = values.GetLength(1);
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                values[x, y] = values[x, y] > cap ? cap : values[x, y];
            }
        }
    }

    public static void normalizeHistogram(ref float[,] values)
    {
        int nx = values.GetLength(0);
        int ny = values.GetLength(1);
        float maxValue = Mathf.NegativeInfinity;
        float minValue = Mathf.Infinity;
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                if (maxValue < values[x, y])
                {
                    maxValue = values[x, y];
                }
                if (minValue > values[x, y])
                {
                    minValue = values[x, y];
                }
            }
        }
        for (int x = 0; x < nx; x++)
        {
            for (int y = 0; y < ny; y++)
            {
                values[x, y] = (values[x, y] - minValue) / (maxValue - minValue);
            }
        }
    }

    public static void mapGrayscaleHistogram2Color(float[,] grayscale, Color[] colorMap, Color background, out Color[] colors)
    {
        int nx = grayscale.GetLength(0);
        int ny = grayscale.GetLength(1);
        int numColors = colorMap.Length;
        colors = new Color[nx * ny];
        int index = 0;
        for (int y = 0; y < ny; y++)
        {
            for (int x = 0; x < nx; x++)
            {
                if (grayscale[x, y] == 0)
                {
                    colors[index++] = background;
                }
                else
                {
                    float scaledGray = grayscale[x, y] * (numColors - 1);
                    int colorIndex = Mathf.Min(numColors - 1, (int)Mathf.Abs(scaledGray));
                    float t = scaledGray - colorIndex;
                    colors[index++] = Color.Lerp(colorMap[colorIndex], colorMap[Mathf.Min(numColors - 1, colorIndex + 1)], t);
                }
            }
        }
    }

    //Normal probability density function.
    public static float normpdf(float x, float mu, float sigm)
    {
        return 0.398942280401433f / sigm * Mathf.Exp(-((x - mu) * (x - mu)) / (2.0f * sigm * sigm));
    }

    //Normal probability density function.
    public static void normpdf(float[] x, out float[] y, float mu, float sigm)
    {
        int xLen = x.Length;
        y = new float[xLen];
        for (int i = 0; i < xLen; i++)
        {
            y[i] = 0.398942280401433f / sigm * Mathf.Exp(-((x[i] - mu) * (x[i] - mu)) / (2.0f * sigm * sigm));
        }
    }

    public static void createGaussKernel3D(int kernelLen, float[] voxDimSize, float sigma, out float[,] kernel)
    {
        if (kernelLen % 2 == 0)
        {
            Debug.Log("Error in createGaussKernel3D: kernelLen needs to be odd");
            kernel = null;
            return;
        }
        kernel = new float[3, kernelLen];
        float kernelLenH = (kernelLen - 1.0f) / 2.0f;
        for (int i = 0; i < 3; i++)
        {
            float maxDist = kernelLenH * voxDimSize[i];
            for (int j = 0; j < kernelLen; j++)
            {
                kernel[i, j] = normpdf(-maxDist + j * voxDimSize[i], 0, sigma);
            }
        }
    }

    public static IEnumerator gaussFilt3D(VolumeViewer.Ref<float[,,]> volumeRef, VolumeViewer.Ref<float[,]> kernelRef, VolumeViewer.Ref<bool> completed)
    {
        completed.val = false;
        float[,,] volume = volumeRef.val;
        float[,] kernel = kernelRef.val;
        if (kernel.GetLength(0) != 3)
        {
            Debug.Log("Error in gaussFilt3D: Wrong kernel dimensions: " + kernel.GetLength(0));
            completed.val = true;
            yield break;
        }
        int nx = volume.GetLength(0);
        int ny = volume.GetLength(1);
        int nz = volume.GetLength(2);
        int kDim = kernel.GetLength(1);
        if (kDim % 2 == 0)
        {
            Debug.Log("Error in gaussFilt3D: Kernels need to be odd: " + kDim);
            completed.val = true;
            yield break;
        }
        //Make sure weights sum up to 1.0.
        float kSumX = 0;
        float kSumY = 0;
        float kSumZ = 0;
        for (int i = 0; i < kDim; i++)
        {
            kSumX += kernel[0, i];
            kSumY += kernel[1, i];
            kSumZ += kernel[2, i];
        }
        for (int i = 0; i < kDim; i++)
        {
            kernel[0, i] /= kSumX;
            kernel[1, i] /= kSumY;
            kernel[2, i] /= kSumZ;
        }
        //From this point on, kDim is equal to (kDim-1)/2 (same as kDim/2 since we made sure kDim is odd).
        kDim /= 2;
        float[,,] filtVolume = new float[nx, ny, nz];
        float[,,] tempVolumePtr;
        //Filter dimension.
        for (int z = 0; z < nz; z++)
        {
            yield return null;
            for (int y = 0; y < ny; y++)
            {
                for (int x = 0; x < nx; x++)
                {
                    float tempVec = 0;
                    for (int k = Mathf.Max(0, x - kDim); k <= Mathf.Min(x + kDim, nx - 1); k++)
                    {
                        tempVec += volume[k, y, z] * kernel[0, k - x + kDim];
                    }
                    filtVolume[x, y, z] = tempVec;
                }
            }
        }
        //Swap.
        tempVolumePtr = volume;
        volume = filtVolume;
        filtVolume = tempVolumePtr;
        //Filter dimension.
        for (int x = 0; x < nx; x++)
        {
            yield return null;
            for (int z = 0; z < nz; z++)
            {
                for (int y = 0; y < ny; y++)
                {
                    float tempVec = 0;
                    for (int k = Mathf.Max(0, y - kDim); k <= Mathf.Min(y + kDim, ny - 1); k++)
                    {
                        tempVec += volume[x, k, z] * kernel[1, k - y + kDim];
                    }
                    filtVolume[x, y, z] = tempVec;
                }
            }
        }
        //Swap.
        tempVolumePtr = volume;
        volume = filtVolume;
        filtVolume = tempVolumePtr;
        //Filter dimension.
        for (int y = 0; y < ny; y++)
        {
            yield return null;
            for (int x = 0; x < nx; x++)
            {
                for (int z = 0; z < nz; z++)
                {
                    float tempVec = 0;
                    for (int k = Mathf.Max(0, z - kDim); k <= Mathf.Min(z + kDim, nz - 1); k++)
                    {
                        tempVec += volume[x, y, k] * kernel[2, k - z + kDim];
                    }
                    filtVolume[x, y, z] = tempVec;
                }
            }
        }
        //Swap.
        tempVolumePtr = volume;
        volume = filtVolume;
        filtVolume = tempVolumePtr;
        completed.val = true;
    }
}
