////--------------------------------------------------------------------
/// Namespace:          
/// Class:              SinMoveObj
/// Description:        Moves its GameObject in a sinusoidal fashion.
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

public class SinMoveObj : MonoBehaviour {

    public Vector3 moveDirection;
    public float multiplier;

    Vector3 originalPosition;
    float elapsedTime;

    // Initialization
    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        transform.localPosition = Mathf.Sin(elapsedTime * multiplier) * moveDirection + originalPosition;
        if (elapsedTime > 2 * Mathf.PI)
        {
            elapsedTime -= 2 * Mathf.PI;
        }
    }
}
