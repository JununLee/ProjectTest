using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructMesh : MonoBehaviour {

    public GameObject sphere;
    private List<Vector3> verts = new List<Vector3>();
    private List<Vector3> normals = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private float depth = 0.3f;
    private Mesh tempMesh;
    public Vector3 norNormal;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            getPoints();
            //Debug.Log(verts.Count);
        }
        if (Input.GetMouseButtonDown(1)&& verts.Count >= 3)
        {

            //ClockwiseSortPoints(verts);

            EarClip();
            //RestPoints(0);
            getAnotherPoints();
            creatGameobject();
            verts.Clear();
            normals.Clear();
            triangles.Clear();

            //DelaunayTriangulation();
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            changeDepth(0.5f);
        }
    }

    public static void ClockwiseSortPoints(List<Vector3> vPoints)

    {

        //计算重心

        Vector2 center = new Vector2();

        double X = 0, y = 0;

        for (int i = 0; i < vPoints.Count; i++)
        {

            X += vPoints[i].x;

            y += vPoints[i].y;

        }

        center.x = (int)X / vPoints.Count;

        center.y = (int)y / vPoints.Count;



        //冒泡排序

        for (int i = 0; i < vPoints.Count - 1; i++)
        {

            for (int j = 0; j < vPoints.Count - i - 1; j++)
            {

                if (PointCmp(vPoints[j], vPoints[j + 1], center))
                {

                    Vector3 tmp = vPoints[j];

                    vPoints[j] = vPoints[j + 1];

                    vPoints[j + 1] = tmp;

                }

            }

        }

    }

    static bool PointCmp(Vector2 a, Vector2 b, Vector2 center)

    {

        if (a.x >= 0 && b.x < 0)

            return true;

        if (a.x == 0 && b.x == 0)

            return a.y > b.y;

        //向量OA和向量OB的叉积

        int det = Convert.ToInt32((a.x - center.x) * (b.y - center.y) - (b.x - center.x) * (a.y - center.y));

        if (det < 0)

            return true;

        if (det > 0)

            return false;

        //向量OA和向量OB共线，以距离判断大小

        double d1 = (a.x - center.x) * (a.x - center.x) + (a.y - center.y) * (a.y - center.y);

        double d2 = (b.x - center.x) * (b.x - center.y) + (b.y - center.y) * (b.y - center.y);

        return d1 > d2;

    }
  void EarClip()
    {
        List<Vector2> projectVerts = new List<Vector2>();
        Dictionary<Vector2, int> indexs = new Dictionary<Vector2, int>();
        List<int> T = new List<int>();
        for (int i = 0; i < verts.Count; i++)
        {
            projectVerts.Add(Camera.main.WorldToScreenPoint(verts[i]));
            indexs.Add(projectVerts[i], i);
        }
        //ClockwiseSortPoints(projectVerts);
        for (int i = 0; projectVerts.Count >= 3; i++)
        {
            int ind0 = (i - 1 + projectVerts.Count) % projectVerts.Count;
            int ind1 = (i + projectVerts.Count) % projectVerts.Count;
            int ind2 = (i + 1 + projectVerts.Count) % projectVerts.Count;
            Vector2 p0 = projectVerts[ind0];
            Vector2 p1 = projectVerts[ind1];
            Vector2 p2 = projectVerts[ind2];

            if (pointDir(p0, p1, p2))
            {
                Vector3 temp = p0;
                p0 = p2;
                p2 = temp;
            }

            if (Vector3.Cross(p1 - p0, p2 - p1).normalized.z < 0)
            {
                continue;
            }
            bool isear = true;
            for (int j = 0; j < projectVerts.Count; j++)
            {
                if (j == ind0 || j == ind1 || j == ind2)
                    continue;
                if (InTrigon(projectVerts[j],p0,p1,p2))
                {
                    isear = false;
                    break;
                }
            }
            if (!isear)
                continue;
            T.Add(indexs[projectVerts[ind0]]);
            T.Add(indexs[projectVerts[ind1]]);
            T.Add(indexs[projectVerts[ind2]]);
            Debug.Log(indexs[projectVerts[ind0]] + "  " + indexs[projectVerts[ind1]] + "   " + indexs[projectVerts[ind2]]);

            projectVerts.Remove(projectVerts[i]);

            i -= 1;
        }
        triangles = T;
    }
    public bool InTrigon(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {

        Vector3 ab = b - a;
        Vector3 ap = p - a;

        Vector3 bc = c - b;
        Vector3 bp = p - b;

        Vector3 ca = a - c;
        Vector3 cp = p - c;
        if (Vector3.Cross(ab, ap).normalized == Vector3.Cross(bc, bp).normalized && Vector3.Cross(bc, bp).normalized == Vector3.Cross(ca, cp).normalized)
        {
            return true;
        }

        else
        {
            return false;
        }

    }

    //List<int> temptriangle = new List<int>();
    void DelaunayTriangulation()
    {
        List<Vector2> projectVerts = new List<Vector2>();
        /// Project
        float xMin = float.MaxValue, xMax = float.MinValue, yMin = float.MaxValue, yMax = float.MinValue;
        for (int i = 0; i < verts.Count; i++)
        {
            projectVerts.Add(Camera.main.WorldToScreenPoint(verts[i]));
            xMin = projectVerts[projectVerts.Count - 1].x < xMin ? projectVerts[projectVerts.Count - 1].x : xMin;
            xMax = projectVerts[projectVerts.Count - 1].x > xMax ? projectVerts[projectVerts.Count - 1].x : xMax;
            yMin = projectVerts[projectVerts.Count - 1].y < yMin ? projectVerts[projectVerts.Count - 1].y : yMin;
            yMax = projectVerts[projectVerts.Count - 1].y > yMax ? projectVerts[projectVerts.Count - 1].y : yMax;
        }
        /// X-Sort
        for (int i = 0; i < projectVerts.Count - 1; i++)
        {
            for (int j =  i + 1; j < projectVerts.Count; j++)
            {
                if (projectVerts[i].x > projectVerts[j].x)
                {
                    Vector3 temp = projectVerts[i];
                    projectVerts[i] = projectVerts[j];
                    projectVerts[j] = temp;
                    temp = verts[i];
                    verts[i] = verts[j];
                    verts[j] = temp;
                }
            }
        }
        /// Big-triangle
        TempTriangle t = new TempTriangle();
        t.edge0.i = -1;
        t.edge1.i = -2;
        t.edge2.i = -3;

        t.edge0.p = new Vector2(1000, 2000);
        t.edge1.p = new Vector2(-2000, -1000);
        t.edge2.p = new Vector2(4000, -1000);
        List<TempTriangle> tempTriangles = new List<TempTriangle>();
        List<TempTriangle> Triangles = new List<TempTriangle>();
        List<Edge> edges = new List<Edge>();
        tempTriangles.Add(t);

        for (int i = 0; i < projectVerts.Count; i++)
        {
            for (int j = 0; j < tempTriangles.Count; j++)
            {
                Vector2 O = new Vector2();
                float r = 0;
                GetCircular(tempTriangles[j].edge0.p, tempTriangles[j].edge1.p, tempTriangles[j].edge2.p,ref r, ref O);
                if (Vector3.Distance(projectVerts[i], O) > r)
                {
                    //if(projectVerts[i].x> tempTriangles[j].edge0.p.x&& projectVerts[i].x > tempTriangles[j].edge1.p.x && projectVerts[i].x > tempTriangles[j].edge2.p.x)
                    if(projectVerts[i].x > (O.x + r))
                    {
                        Triangles.Add(tempTriangles[j]);
                        tempTriangles.Remove(tempTriangles[j]);
                        j = j - 1;
                    }
                    continue;
                }
                else
                {
                    containEdge(edges, new Edge(tempTriangles[j].edge0, tempTriangles[j].edge1));
                    containEdge(edges, new Edge(tempTriangles[j].edge1, tempTriangles[j].edge2));
                    containEdge(edges, new Edge(tempTriangles[j].edge2, tempTriangles[j].edge0));
                    tempTriangles.Remove(tempTriangles[j]);
                    j = j - 1;
                }
            }
            //for (int j = 0; j < tempTriangles.Count; j++)
            //{
            //    for (int n = 0; n < edges.Count; n++)
            //    {
            //        if (equleEdge(new Edge(tempTriangles[j].edge0, tempTriangles[j].edge1), edges[n])|| equleEdge(new Edge(tempTriangles[j].edge1, tempTriangles[j].edge2), edges[n])|| equleEdge(new Edge(tempTriangles[j].edge2, tempTriangles[j].edge0), edges[n]))
            //        {
            //            edges.Remove(edges[n]);
            //            n = n - 1;
            //        }
            //    }
            
           
            for (int n = 0; n < edges.Count; n++)
            {
                TempTriangle tT = new TempTriangle();
                if (Vector3.Cross(edges[n].edge0.p - edges[n].edge1.p, projectVerts[i] - edges[n].edge1.p).z > 0)
                {
                    tT.edge0 = new Edgebuffer(i, projectVerts[i]);
                    tT.edge1 = edges[n].edge1;
                    tT.edge2 = edges[n].edge0;
                }
                else
                {
                    tT.edge0 = edges[n].edge0;
                    tT.edge1 = edges[n].edge1;
                    tT.edge2 = new Edgebuffer(i, projectVerts[i]);
                }
                tempTriangles.Add(tT);
            }
            edges.Clear();
        }
        for (int m = 0; m < tempTriangles.Count; m++)
        {
            Triangles.Add(tempTriangles[m]);
        }
        for (int j = 0; j < Triangles.Count; j++)
        {
            if(Triangles[j].edge0.i<0|| Triangles[j].edge1.i < 0 || Triangles[j].edge2.i < 0)
            {
                Triangles.Remove(Triangles[j]);
                j = j - 1;
            }
        }
        for (int i = 0; i < Triangles.Count; i++)
        {
            Debug.Log(Triangles[i].edge0.i + "    " + Triangles[i].edge1.i + "       " + Triangles[i].edge2.i);
        }
        int a = 0;
    }
    bool equleEdge(Edge tedge, Edge edge)
    {

        if (tedge.edge0.i == edge.edge0.i  && tedge.edge1.i == edge.edge1.i )
        {
            return true;
        }
        if (tedge.edge0.i == edge.edge1.i && tedge.edge1.i == edge.edge0.i )
        {
            return true;
        }

        return false;
    }
    bool containEdge(List<Edge> edges,Edge edge)
    {
        for (int i = 0; i < edges.Count; i++)
        {
            if (edges[i].edge0.i == edge.edge0.i  && edges[i].edge1.i == edge.edge1.i)
            {
                edges.Remove(edges[i]);
                return true;
            }
            if (edges[i].edge0.i == edge.edge1.i && edges[i].edge1.i == edge.edge0.i)
            {
                edges.Remove(edges[i]);
                return true;
            }
        }
        edges.Add(edge);
        return false;
    }
    struct Edge
    {
        public Edgebuffer edge0;
        public Edgebuffer edge1;
        public Edge(Edgebuffer e0,Edgebuffer e1)
        {
            edge0 = e0;
            edge1 = e1;
        }
    }
    struct Edgebuffer
    {
        public int i;
        public Vector2 p;
        public Edgebuffer(int index,Vector2 pos)
        {
            i = index;
            p = pos;
        }
    }
    struct TempTriangle
    {
        public Edgebuffer edge0;
        public Edgebuffer edge1;
        public Edgebuffer edge2;
    }
    public void GetCircular(Vector2 P1, Vector2 P2, Vector2 P3, ref float R, ref Vector2 PCenter)
    {
        float a = 2 * (P2.x - P1.x);
        float b = 2 * (P2.y - P1.y);
        float c = P2.x * P2.x + P2.y * P2.y - P1.x * P1.x - P1.y * P1.y;
        float d = 2 * (P3.x - P2.x);
        float e = 2 * (P3.y - P2.y);
        float f = P3.x * P3.x + P3.y * P3.y - P2.x * P2.x - P2.y * P2.y;
        float x = (b * f - e * c) / (b * d - e * a);
        float y = (d * c - a * f) / (b * d - e * a);
        float r = Mathf.Sqrt(((x - P1.x) * (x - P1.x) + (y - P1.y) * (y - P1.y)));
        R = r;
        Vector2 pc = new Vector2(x, y);
        PCenter = pc;
    }




    private void getPoints()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if(Physics.Raycast(ray,out hit))
        {
            verts.Add(hit.point);
            Instantiate(sphere,hit.point,Quaternion.identity);
        }
    }

    private void creatGameobject()
    {
        tempMesh = new Mesh();

        for (int i = 0; i < triangles.Count; i += 3)
        {
            if (!pointDir(verts[triangles[i]], verts[triangles[i + 1]], verts[triangles[i + 2]]))
            {
                int temp = triangles[i + 1];
                triangles[i + 1] = triangles[i + 2];
                triangles[i + 2] = temp;
            }
        }
        int count = triangles.Count;
        for (int i = 0; i < count; i++)
        {
            triangles.Add(triangles[i] + verts.Count/2);
        }
        for (int i = count; i < triangles.Count; i += 3)
        {
            if (pointDir(verts[triangles[i]], verts[triangles[i + 1]], verts[triangles[i + 2]]))
            {
                int temp = triangles[i + 1];
                triangles[i + 1] = triangles[i + 2];
                triangles[i + 2] = temp;
            }
        }



        for (int i = 0; i < verts.Count / 2; i++)
        {
            if (i == (verts.Count / 2 - 1))
            {
                if (pointDir(verts[0], verts[i - 1], verts[i]))
                {
                    triangles.Add(i);
                    triangles.Add(i + verts.Count / 2);
                    triangles.Add(0);

                    triangles.Add(0);
                    triangles.Add(i + verts.Count / 2);
                    triangles.Add(verts.Count / 2);
                }
                else
                {
                    triangles.Add(i);
                    triangles.Add(0);
                    triangles.Add(i + verts.Count / 2);

                    triangles.Add(0);
                    triangles.Add(verts.Count / 2);
                    triangles.Add(i + verts.Count / 2);
                }

                continue;
            }
            else if (i == (verts.Count / 2 - 2))
            {
                if (pointDir(verts[0], verts[i], verts[i + 1]))
                {
                    triangles.Add(i);
                    triangles.Add(i + verts.Count / 2);
                    triangles.Add(i + 1);

                    triangles.Add(i + 1);
                    triangles.Add(i + verts.Count / 2);
                    triangles.Add(i + verts.Count / 2 + 1);
                }
                else
                {
                    triangles.Add(i);
                    triangles.Add(i + 1);
                    triangles.Add(i + verts.Count / 2);

                    triangles.Add(i + 1);
                    triangles.Add(i + verts.Count / 2 + 1);
                    triangles.Add(i + verts.Count / 2);
                }
                continue;
            }
            if (pointDir(verts[0], verts[i + 1], verts[i + 2]))
            {
                triangles.Add(i);
                triangles.Add(i + verts.Count / 2);
                triangles.Add(i + 1);

                triangles.Add(i + 1);
                triangles.Add(i + verts.Count / 2);
                triangles.Add(i + verts.Count / 2 + 1);
            }
            else
            {
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + verts.Count / 2);

                triangles.Add(i + 1);
                triangles.Add(i + verts.Count / 2 + 1);
                triangles.Add(i + verts.Count / 2);
            }
        }



        tempMesh.name = "temp";
		tempMesh.vertices = verts.ToArray();
        //tempMesh.normals = normals.ToArray();
		tempMesh.triangles = triangles.ToArray();

        GameObject t = new GameObject();
        MeshFilter meshFilter = t.AddComponent<MeshFilter>();
        meshFilter.mesh = tempMesh;
        MeshRenderer meshRender = t.AddComponent<MeshRenderer>();
        meshRender.material = new Material(Shader.Find("Standard"));
    }
    private Vector3 getNormal(Vector3 p,Vector3 p1,Vector3 p2)
    {
        Vector3 normal = Vector3.Cross(p1 - p, p2 - p).normalized;
        Vector3 dir = Camera.main.transform.position - p1;
        if (Vector3.Dot(normal, dir) < 0)
        {
            normal = -normal;
        }
        return normal;
    }
    private bool pointDir(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 dir1 = Vector3.Cross(p2 - p1, p3 - p2);
        Vector3 dir2 = Camera.main.transform.position - p1;
        if (Vector3.Dot(dir1, dir2) < 0)
        {
            return false;
        }
        return true;
    }
    private void getAnotherPoints()
    {
        int count = verts.Count;

        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                List<Vector3> temp = new List<Vector3>();
                for (int j = 1; j < count - 1; j++)
                {
                    temp.Add(getNormal(verts[0], verts[j], verts[j + 1]));
                }
                Vector3 nor = temp[0];
                for (int n = 1; n < temp.Count; n++)
                {
                    nor += temp[i];
                }
                normals.Add(nor.normalized);
            }
            else if (i == 1)
            {
                normals.Add(getNormal(verts[1], verts[0], verts[2]));
            }
            else if (i==(count - 1))
            {
                normals.Add(getNormal(verts[count - 1], verts[count - 2], verts[0]));
            }
            else
            {
                Vector3 n1 = getNormal(verts[i], verts[i - 1], verts[0]);
                Vector3 n2 = getNormal(verts[i], verts[0], verts[i + 1]);
                normals.Add((n1 + n2).normalized);
            }
            
            //verts.Add(verts[i] - normals[i] * depth);
            //verts[i] += normals[i] * depth;
        }

        norNormal = normals[0];
        for (int i = 1; i < normals.Count; i++)
        {
            norNormal += normals[i];
        }
        norNormal = norNormal.normalized;
        for (int i = 0; i < count; i++)
        {
            verts.Add(verts[i] - norNormal * depth);
            verts[i] += norNormal * depth;
        }
    }


    private void RestPoints(int index)
    {
        if (index >= verts.Count) return;
        List<Vector3> temp = new List<Vector3>();
        for (int i = index; i < verts.Count+index; i++)
        {
            temp.Add(verts[i % verts.Count]);
        }
        for (int i = 0; i < temp.Count-3; i++)
        {
            Vector3 dir1 = (temp[i + 1] - temp[0]).normalized;
            Vector3 dir2 = (temp[i + 2] - temp[0]).normalized;
            Vector3 dir3 = (temp[i + 3] - temp[0]).normalized;
            float dot1 = Vector3.Dot(dir1, dir2);
            float dot2 = Vector3.Dot(dir1, dir3);
            //if (dot1 * dot2 < 0) dot2 = -dot2;
            float angle1 = Mathf.Rad2Deg * Mathf.Acos(dot1);
            float angle2 = Mathf.Rad2Deg * Mathf.Acos(dot2);
            if (angle1 > angle2)
            {
                RestPoints(++index);
                return;
            }
        }
        Debug.Log(index);
        verts.Clear();
        for (int i = 0; i < temp.Count; i++)
        {
            verts.Add(temp[i]);
        }
    }

    public void changeDepth(float depth)
    {
        Vector3[] verts = tempMesh.vertices;
        for (int i = 0; i < verts.Length/2; i++)
        {
            Vector3 tempP = (verts[i] + verts[i + verts.Length / 2]) / 2;
            verts[i] = tempP + norNormal * depth;
            verts[i + verts.Length / 2] = tempP - norNormal * depth;
        }
        tempMesh.vertices = verts;
    }
}
