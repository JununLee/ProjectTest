using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ttt : MonoBehaviour {
      
    public Transform prefab;
	void Start () {

        for (float i = -10.0f; i <= 10.0f; i+=0.2f)
        { 
            for (float j = -10.0f; j <= 10.0f; j += 0.2f)
            {
                float f = i * i + j * j - 2 * i - 2 * j;
                GameObject obj = Instantiate<GameObject>(prefab.gameObject);
                obj.transform.position = new Vector3(i, j, f);
            }
        }
	}
	 
	void Update () {
		
	}
}
