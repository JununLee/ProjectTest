using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

public class ImportObj : MonoBehaviour {

    public GameObject go;

    public static List<Vector3> verts = new List<Vector3>();
    public static List<Vector3> vertNormals = new List<Vector3>();
    public static List<Vector3> vertTexcoords = new List<Vector3>();
    public static List<int> triangles = new List<int>();
    Mesh mesh;

    string files;
	// Use this for initialization
	void Start () {
        verts.Add(Vector3.zero);
        vertNormals.Add(Vector3.zero);
        LoadInfoObj("G:\\tou.obj");
        //ObjImporter.Import()
        //ObjImporter.Import(files);
        //mesh = new Mesh();
        //mesh.name = "Head";
        //mesh.vertices = verts.ToArray();
        //mesh.triangles = triangles.ToArray();     
        //mesh.RecalculateBounds();
        //mesh.RecalculateNormals();
        //go.GetComponent<MeshFilter>().mesh = mesh;
        //go.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Standard"));

    }
	
	// Update is called once per frame
	void Update () {


    }
    public void LoadInfoObj(string path)
    {
        if (!File.Exists(path)) return ;
        StreamReader reader = new StreamReader(path);
        string content = reader.ReadToEnd();
        files = content;
        reader.Close();
        string[] lines = content.Split('\n');
        Debug.Log(lines.Length);
        foreach (var item in lines)
        {
            string[] splits = item.Split(' ');
            switch (splits[0])
            {
                case "v": verts.Add(new Vector3(float.Parse(splits[1]), float.Parse(splits[2]), float.Parse(splits[3]))); break;
                case "vn": vertNormals.Add(new Vector3(float.Parse(splits[1]), float.Parse(splits[2]), float.Parse(splits[3]))); break;
                case "vt": vertTexcoords.Add(new Vector3(float.Parse(splits[1]), float.Parse(splits[2]), float.Parse(splits[3]))); break;
                case "f":
                    for (int i = 1; i < 4; i++)
                    {
                        triangles.Add(int.Parse(splits[i].Split('/')[0]));
                    }; break;
                default:
                    break;
            }
        }
    }
}
