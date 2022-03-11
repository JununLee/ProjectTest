using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SpineDivTest : MonoBehaviour {

    public GameObject quad;
    List<Vector3> postions = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    private void Start()
    {
        string path = "G://Data/Dcm/result.csv";
        StreamReader reader = new StreamReader(path);
        string data = reader.ReadToEnd();
        reader.Close();
        string[] datas = data.Split(',');
        for (int i = 0; i < datas.Length; i=i+6)
        {
            Vector3 pos = new Vector3(float.Parse(datas[i+0]), float.Parse(datas[i+1]), float.Parse(datas[i+2]));
            Vector3 nor = new Vector3(float.Parse(datas[i+3]), float.Parse(datas[i+4]), float.Parse(datas[i+5]));
            postions.Add(pos);
            normals.Add(nor);
        }

        for (int i = 0; i < postions.Count; i++)
        {
            GameObject g = Instantiate(quad);
            g.transform.position = normals[i]/1000;
            g.transform.forward = postions[i];
        }
    }
}
