using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EightBoxController : MonoBehaviour {

   
    List<int[]> vertexIndexs = new List<int[]>();
    List<Transform> pointPos = new List<Transform>();
    MeshFilter mf;
    List<Vector3> verts = new List<Vector3>();
    List<int> triangles = new List<int>();
    Mesh mesh;

    public delegate void UpdataMeshDelegate();
    public UpdataMeshDelegate updaataMesh;
    public void Start()
    {
        updaataMesh = UpdataMesh;
        vertexIndexs.Add(new int[8] { 2, 23, 24, 39, 1, 22,40,56 });
        vertexIndexs.Add(new int[6] { 3, 26, 27, 0, 55, 51 });
        vertexIndexs.Add(new int[8] { 5, 8, 29, 30, 4, 7, 46, 50 });
        vertexIndexs.Add(new int[4] { 9, 32, 6, 45 });
        vertexIndexs.Add(new int[8] { 11, 14, 33, 34, 10, 13, 44, 54 });
        vertexIndexs.Add(new int[5] { 15, 35, 12, 49, 52 });
        vertexIndexs.Add(new int[8] { 17, 20, 36, 37, 16, 19, 43, 47 });
        vertexIndexs.Add(new int[4] { 21, 38, 18, 42 });
        pointPos = transform.GetComponentsInChildren<Transform>().ToList();
        pointPos.Remove(pointPos[0]);
        foreach (var item in pointPos)
        {
            item.gameObject.AddComponent<PointDrag>();
        }
        mf = transform.GetComponent<MeshFilter>();

        mesh = new Mesh();
        foreach (var item in mf.mesh.vertices)
        {
            verts.Add(item);
        }
    }

    private void UpdataMesh()
    {
        for (int i = 0; i < pointPos.Count; i++)
        {
            for (int j = 0; j < vertexIndexs[i].Length; j++)
            {
                if (j < vertexIndexs[i].Length / 2)
                {
                    verts[vertexIndexs[i][j]] = pointPos[i].localPosition;
                }
                else
                {
                    verts[vertexIndexs[i][j]] = pointPos[i].localPosition - Vector3.up;
                }
            }
        }
        mesh.vertices = verts.ToArray();
        mesh.triangles = mf.mesh.triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update()
    {
        UpdataMesh();
    }

}
