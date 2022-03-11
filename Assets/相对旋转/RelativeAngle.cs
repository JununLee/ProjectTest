using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeAngle : MonoBehaviour {

    public Transform A;
    public Transform B;
    
    private void Start()
    {
        //A相对B的旋转角度
        Quaternion relative =  Quaternion.Inverse(B.rotation) * A.rotation;
        
        Debug.Log(relative.x +"   "+ relative.y+"  "+relative.z+"   "+ relative.w);
        //Debug.Log(angle.x + "   " + angle.y + "    " + angle.z);
        Debug.Log(relative.eulerAngles.x + "  " + relative.eulerAngles.y + "  " + relative.eulerAngles.z);
    }
}
