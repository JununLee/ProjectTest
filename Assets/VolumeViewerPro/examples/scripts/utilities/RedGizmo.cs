////--------------------------------------------------------------------
/// Namespace:          
/// Class:              RedGizmo
/// Description:        Draws its GameObject's mesh in the scene view.
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

public class RedGizmo : MonoBehaviour {

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        //Gizmos.DrawCube(transform.position, new Vector3(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
        Gizmos.DrawMesh(GetComponent<MeshFilter>().sharedMesh, transform.position, transform.rotation, transform.lossyScale);
    }
#endif

}
