using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class F1Test : MonoBehaviour {

    public MeshFilter mesh;
    List<Vector3> value = new List<Vector3>();
    List<int>[] index = new List<int>[8] {new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>(), new List<int>() };
    private void Start()
    {
        for (int i = 0; i < mesh.mesh.vertices.Length; i++)
        {
            Vector3 item = mesh.mesh.vertices[i];
            if (item.y<0/*||(item.x==0&&item.z==0)*/) continue;
            if (value.Contains(item)) continue;
            value.Add(mesh.mesh.vertices[i]);
            Debug.Log(i + "     "+ item.x+"   "+item.y+"       "+item.z + "         " );
        }
        for (int i = 0; i < mesh.mesh.vertices.Length; i++)
        {

            Debug.Log("_____________________");
            for (int j = 0; j < 8; j++)
            {
                if (Mathf.Abs(mesh.mesh.vertices[i].x - value[j].x)<0.1&& Mathf.Abs(mesh.mesh.vertices[i].y - value[j].y) < 0.1&& Mathf.Abs(mesh.mesh.vertices[i].z - value[j].z) < 0.1)
                {
                    index[j].Add(i);
                    Debug.Log(j + "*********" + i);
                }
                
                //if (Mathf.Abs(mesh.mesh.vertices[i].x - (value[j] - Vector3.up).x ) < 0.1 && Mathf.Abs(mesh.mesh.vertices[i].y - (value[j] - Vector3.up).y) < 0.1 && Mathf.Abs(mesh.mesh.vertices[i].z - (value[j] - Vector3.up).z) < 0.1)
                //{
                //    index[j].Add(i);
                //}
            }
        }
        int a = 0;
    }

}
