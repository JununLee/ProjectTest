////--------------------------------------------------------------------
/// Namespace:          
/// Class:              RotateObj
/// Description:        Rotates its GameObject.
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

public class RotateObj : MonoBehaviour {

    public Vector3 rotationAngle;
    public float multiplier;
    [SerializeField]
    public bool _animate = true;
    public bool animate { get { return _animate; } set { _animate = value; } }

    // Update is called once per frame
    void Update () {
        if(_animate == false)
        {
            return;
        }
		transform.Rotate (rotationAngle * multiplier * Time.deltaTime);
	}
}
