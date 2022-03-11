using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class ImportBigModel : MonoBehaviour {

    private List<Vector3> TVerts = new List<Vector3>();
    private List<Vector3> TNormals = new List<Vector3>();
    private List<int> TTriangles = new List<int>();
    private List<int> TNormalIndexs = new List<int>();
    private List<Mesh> meshs = new List<Mesh>();
    private bool readFinsh;

    private void Start()
    {
        //Thread thread = new Thread(delegate() { ReadInfoObj(@"G:\Data\ALL\dti.obj"); });
        //thread.Start();
        Debug.Log(DateTime.Now);
        ReadInfoObj(@"G:\Data\20180326\body\骨头.obj");
    }
    private void Update()
    {
        if (readFinsh)
        {
            readFinsh = false;
            ClipMesh();
            CreatGameObject();
            Debug.Log(DateTime.Now);

        }
    }
    void ReadInfoObj(string path)
    {
        readFinsh = false;
        if (!File.Exists(path)) return;
        StreamReader reader = new StreamReader(path);
        string obj = reader.ReadToEnd();
        reader.Close();
        string[] lines = obj.Split('\n');
        foreach (string line in lines)
        {
            string[] unit = System.Text.RegularExpressions.Regex.Split(line, "\\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            switch (unit[0])
            {
                case "v":
                    TVerts.Add(new Vector3(float.Parse(unit[1]), float.Parse(unit[2]), float.Parse(unit[3])));
                    break;
                case "vn":
                    TNormals.Add(new Vector3(float.Parse(unit[1]), float.Parse(unit[2]), float.Parse(unit[3])));
                    break;
                case "f":
                    TTriangles.Add(int.Parse(unit[1].Split('/')[0]) - 1);
                    TNormalIndexs.Add(int.Parse(unit[1].Split('/')[2]) - 1);
                    TTriangles.Add(int.Parse(unit[2].Split('/')[0]) - 1);
                    TNormalIndexs.Add(int.Parse(unit[2].Split('/')[2]) - 1);
                    TTriangles.Add(int.Parse(unit[3].Split('/')[0]) - 1);
                    TNormalIndexs.Add(int.Parse(unit[3].Split('/')[2]) - 1);
                    break;
                default:
                    break;
            }
        }
        readFinsh = true;
    }
    void ClipMesh()
    {
        int fragNum = TTriangles.Count ;
        int meshNum = fragNum % 60000 > 0 ? fragNum / 60000 + 1 : fragNum / 60000;
        List<int> triangles = new List<int>();
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        Dictionary<int, int> index = new Dictionary<int, int>();
        for (int i = 0; i < meshNum; i++)
        {
            triangles.Clear();
            verts.Clear();
            normals.Clear();
            index.Clear();
            int num = i == (meshNum - 1) ? (fragNum) % 60000 : 60000;
            for (int j = 0; j < num; j++)
            {
                int n = 60000 * i + j;
                triangles.Add(j);
                verts.Add(TVerts[TTriangles[n]]);
                normals.Add(TNormals[TNormalIndexs[n]]);
            }
            Mesh mesh = new Mesh();
            mesh.name = "obj" + i.ToString();
            mesh.vertices = verts.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = triangles.ToArray();
            meshs.Add(mesh);
        }
    }
    void CreatGameObject()
    {
        GameObject obj = new GameObject("Obj");
        for (int i = 0; i < meshs.Count; i++)
        {
            GameObject newModel = new GameObject("newModel"+i.ToString());
            newModel.transform.SetParent(obj.transform);
            MeshFilter meshFilter = newModel.AddComponent<MeshFilter>();
            meshFilter.mesh = meshs[i];
            MeshRenderer meshRender = newModel.AddComponent<MeshRenderer>();
            meshRender.material = new Material(Shader.Find("Standard"));
        }
    }
}
