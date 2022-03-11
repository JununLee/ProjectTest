///-----------------------------------------------------------------
/// Namespace:          VolumeViewer
/// Definitions:        VolumeUtilities and others.
/// Description:        Various utility definitions.
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

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace VolumeViewer
{
    [Serializable]
    public class Vector3Event : UnityEvent<Vector3> { }
    [Serializable]
    public class ColorEvent : UnityEvent<Color> { }
    [Serializable]
    public class StringEvent : UnityEvent<string> { }
    [Serializable]
    public class FloatEvent : UnityEvent<float> { }
    [Serializable]
    public class IntEvent : UnityEvent<int> { }
    [Serializable]
	public class BoolEvent : UnityEvent<bool> { }
	[Serializable]
	public class EmptyEvent : UnityEvent { }

    public enum VolumeDataType
    {
        ScalarData,
        RGBAData
    }

    public enum VolumeBlendMode
    {
		Disabled,
        Replace,
        Multiply,
        Add
    }

    public enum VolumeTextureFormat
    {
        Native,
        Alpha8,
        RHalf,
        RFloat,
        RGB24,
        RGBA32,
        ARGB32,
        RGBAHalf,
        RGBAFloat
    }

    public enum VolumeGradientFetches
    {
        _3,
        _6,
        _32,
        _54
    }

    public enum PlanarConfiguration
    {
        Native,
        Interlaced,
        Separated
    }

    public class Ref<T>
    {
        public T val;
    }

    public class Volume
    {
        public int nx = 1;
        public int ny = 1;
        public int nz = 1;
        public float dx = 1.0f;
        public float dy = 1.0f;
        public float dz = 1.0f;
        public int numVoxels = 1;
        public TextureFormat format;
        public Texture3D texture;
        public NIfTI.NiiInfo niiInfo;
    }

    public struct VolumeUtilities
    {
        //(Quicksort) Sort a float array and an int array based on the values of the float array.
        //Useful to get the order of the float values after sorting (the original indices).
        public static void sortTwo(ref float[] master, ref int[] slave)
        {
            int stackSize = 64;
            int sortSwitch = 8;
            int n = master.Length;
            int low = 0;
            int high = n - 1;
            int pivotIndex;
            float pivot;
            int pivot2;
            float buffer;
            int buffer2;
            int[] stack = new int[stackSize];
            int stackPos = 0;
            int i, j;
            for (;;)
            {
                if (high - low > sortSwitch)
                {
                    pivotIndex = (high + low) >> 1;
                    if (master[low] > master[high])
                    {
                        buffer = master[high]; master[high] = master[low]; master[low] = buffer;
                        buffer2 = slave[high]; slave[high] = slave[low]; slave[low] = buffer2;
                    }
                    if (master[pivotIndex] < master[low])
                    {
                        buffer = master[low]; master[low] = master[pivotIndex]; master[pivotIndex] = buffer;
                        buffer2 = slave[low]; slave[low] = slave[pivotIndex]; slave[pivotIndex] = buffer2;
                    }
                    if (master[pivotIndex] > master[high])
                    {
                        buffer = master[high]; master[high] = master[pivotIndex]; master[pivotIndex] = buffer;
                        buffer2 = slave[high]; slave[high] = slave[pivotIndex]; slave[pivotIndex] = buffer2;
                    }
                    pivot = master[pivotIndex];
                    pivot2 = slave[pivotIndex];
                    buffer = master[high - 1]; master[high - 1] = pivot; master[pivotIndex] = buffer;
                    buffer2 = slave[high - 1]; slave[high - 1] = pivot2; slave[pivotIndex] = buffer2;
                    i = low;
                    j = high - 1;
                    for (;;)
                    {
                        do
                        {
                            i++;
                        } while (master[i] < pivot);
                        do
                        {
                            j--;
                        } while (master[j] > pivot);
                        if (j < i)
                        {
                            break;
                        }
                        buffer = master[i]; master[i] = master[j]; master[j] = buffer;
                        buffer2 = slave[i]; slave[i] = slave[j]; slave[j] = buffer2;
                    }
                    master[high - 1] = master[i]; master[i] = pivot;
                    slave[high - 1] = slave[i]; slave[i] = pivot2;
                    if (high - (i + 1) >= j - low)
                    {
                        if (high - (i + 1) > sortSwitch)
                        {
                            stack[stackPos++] = i + 1;
                            stack[stackPos++] = high;
                        }
                        high = j;
                    }
                    else
                    {
                        if (j - low > sortSwitch)
                        {
                            stack[stackPos++] = low;
                            stack[stackPos++] = j;
                        }
                        low = i + 1;
                    }
                }
                else
                {
                    if (stackPos == 0)
                    {
                        break;
                    }
                    else
                    {
                        high = stack[--stackPos];
                        low = stack[--stackPos];
                    }
                }
            }
            for (j = 1; j < n; j++)
            {
                buffer = master[j];
                buffer2 = slave[j];
                for (i = j - 1; i >= 0; i--)
                {
                    if (master[i] <= buffer)
                    {
                        break;
                    }
                    master[i + 1] = master[i];
                    slave[i + 1] = slave[i];
                }
                master[i + 1] = buffer;
                slave[i + 1] = buffer2;
            }
        }
    }

    public class NumStrComparer : IComparer<string>
    {
        public int Compare(string d1, string d2)
        {
            float f1, f2;
            bool b1 = float.TryParse(d1, NumberStyles.Any, CultureInfo.InvariantCulture, out f1);
            bool b2 = float.TryParse(d2, NumberStyles.Any, CultureInfo.InvariantCulture, out f2);
            if (b1 && b2)
            {
                if (f1 > f2)
                {
                    return 1;
                }
                if (f1 < f2)
                {
                    return -1;
                }
                if (f1 == f2)
                {
                    return 0;
                }
            }
            if (b1 && !b2)
            {
                return -1;
            }
            if (!b1 && b2)
            {
                return 1;
            }
            return string.Compare(d1, d2, true);
        }
    }
}