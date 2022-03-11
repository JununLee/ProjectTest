////--------------------------------------------------------------------
/// Namespace:          
/// Class:              RotationPlane
/// Description:        Changes the rotation of a VolumeComponent.
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

public class RotationPlane : MonoBehaviour {

    public Transform[] objTransform;
    public float rotationSpeed;

    public void changeRotation(Vector2 delta)
    {
        if(objTransform.Length < 1)
        {
            return;
        }
        objTransform[0].Rotate(objTransform[0].InverseTransformDirection(Vector3.down), delta.x * rotationSpeed);
        objTransform[0].Rotate(objTransform[0].InverseTransformDirection(Vector3.right), delta.y * rotationSpeed);
        for (int i = 1; i < objTransform.Length; i++)
        {
            objTransform[i].localRotation = objTransform[0].localRotation;
        }
    }

}
