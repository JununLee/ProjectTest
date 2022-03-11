using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMN : MonoBehaviour {
 
	void Start () {

        List<float> data = new List<float>();
        data.Add(1);
        data.Add(2);
        data.Add(3);
        data.Add(4);
        data.Add(5);
        data.Add(6);
        data.Add(7);
        data.Add(8);
        int n = 3;
        float[] res = new float[n];
        cmn(data, res, n);

        return;
    }
	 
	void Update () {
		
	}

    void cmn(List<float> data, float[] res, int num, int index = -1)
    {
        for (int i = index + 1; i < data.Count; i++)
        { 
            res[num - 1] = data[i];
            if (num == 1)
            {
                string str = "";
                for (int n = 0; n < res.Length; n++)
                {
                    str += res[res.Length - n - 1] + " ";
                }
                Debug.Log(str);
            }
            else
            {
                cmn(data, res, num - 1, i);
            }
        }
    }
}
