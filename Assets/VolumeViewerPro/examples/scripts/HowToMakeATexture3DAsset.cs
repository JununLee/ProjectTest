////--------------------------------------------------------------------
/// Namespace:          
/// Class:              HowToMakeATexture3DAsset
/// Description:        Example of of how to create and save a 
///                         Texture3D asset.
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

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HowToMakeATexture3DAsset : MonoBehaviour {

	void Start ()
    {
        //Use a coroutine, so your project stays responsive during the creation of the Texture3D asset.
        StartCoroutine(makeTexture3D());
    }

    IEnumerator makeTexture3D()
    {
        //How big is each dimension?
        //Make sure it's a power of two!
        int xDim = 16;
        int yDim = 16;
        int zDim = 16;

        //Total number of voxels
        int numVoxInTex = xDim * yDim * zDim;

        //Create color array to fill Texture3D with.
        Color[] volumeColors = new Color[numVoxInTex];

        int index = 0;
        for (int z = 0; z < zDim; z++)
        {
            //yield return in the outer most loop to keep project responsive.
            yield return null;
            for (int y = 0; y < yDim; y++)
            {
                for (int x = 0; x < xDim; x++)
                {
                    //Fill color array with colors of your choosing.
                    volumeColors[index] = new Color(Random.value, Random.value, Random.value, Random.value);
                    index++;
                }
            }
        }
        yield return null;

        //Create a new Texture3D
        Texture3D volumetricTexture = new Texture3D(xDim, yDim, zDim, TextureFormat.ARGB32, false);
        volumetricTexture.filterMode = FilterMode.Point;
        volumetricTexture.wrapMode = TextureWrapMode.Clamp;
        //Fill the Texture3D with the previously prepared Color array.
        volumetricTexture.SetPixels(volumeColors);
        //Apply the changes to the Texture3D.
        volumetricTexture.Apply();
        yield return null;

#if UNITY_EDITOR
        //Define where to save the asset.
        //Make sure the file name ends with .asset!
        string assetPath = "Assets/VolumeViewerPro/resources/volumetricTexture.asset";

        //Create the asset at assetPath.
        AssetDatabase.CreateAsset(volumetricTexture, assetPath);
#endif

    }
    
}
