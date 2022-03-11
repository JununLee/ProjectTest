using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Delaunay : MonoBehaviour {

    public Transform[] points;

    List<Vector2> vertexs;
    List<Tri> triangles;
    List<Edge> edges;
    class Tri
    {
        public int index_p0;
        public int index_p1;
        public int index_p2; 
        public Tri(int ind0, int ind1, int ind2)
        {
            index_p0 = ind0;
            index_p1 = ind1;
            index_p2 = ind2;
             
        } 
    }

    class Edge
    {
        public int index_p0;
        public int index_p1; 
        public Edge(int ind0, int ind1)
        {
            index_p0 = ind0;
            index_p1 = ind1; 
        }
    }

    void Start()
    {

        vertexs = new List<Vector2>(); //顶点链表

        for (int i = 0; i < points.Length; i++)
        {
            vertexs.Add(points[i].localPosition);
        }

        triangles = new List<Tri>(); //三角形链表
        //确定超级三角形
        List<Vector2> bigTri = creatBigTriangle(vertexs);
        for (int i = 0; i < bigTri.Count; i++)
        {
            Debug.Log(bigTri[i].ToString("f6"));
        }
        //将超级三角形的顶点加入到顶点链表中
        vertexs.Insert(0, bigTri[2]);
        vertexs.Insert(0, bigTri[1]);
        vertexs.Insert(0, bigTri[0]);
        //将超级三角形加入到三角形链表中
        triangles.Add(new Tri(0, 1, 2));

        for (int i = 0; i < vertexs.Count; i++) //对顶点链表中的每一个点
        { 
            edges = new List<Edge>(); //初始化边数组
            for (int n = 0; n < triangles.Count; n++) //对于三角形链表中的每一个三角形
            {
                Vector2 ori = Vector2.zero;
                float rad = 0;
                //计算出外接圆圆心和半径
                calculateCircle(vertexs[triangles[n].index_p0], vertexs[triangles[n].index_p1], vertexs[triangles[n].index_p2], ref ori, ref rad);

                float dis = Vector2.Distance(ori, vertexs[i]);
                if (dis < rad) //如果这个点在三角形的外接圆内
                {
                    //把这个三角形的三条边加入到边数组中
                    edges.Add(new Edge(triangles[n].index_p0, triangles[n].index_p1));
                    edges.Add(new Edge(triangles[n].index_p0, triangles[n].index_p2));
                    edges.Add(new Edge(triangles[n].index_p2, triangles[n].index_p1));
                    //从三角形链表中将这个三角形删除
                    triangles.Remove(triangles[n]);
                    n--;
                }

            }
            //把边数组中所有重复的边都删除，注意这里是把所有的重复边都删除，比如有边e1,e2,e2,e3，删除后应该只剩e1和e3
            for (int j = 0; j < edges.Count; j++)
            {
                bool removeSelf = false;
                for (int k = j+1; k < edges.Count; k++)
                {
                    if((edges[j].index_p0 == edges[k].index_p0&& edges[j].index_p1 == edges[k].index_p1)|| 
                        (edges[j].index_p0 == edges[k].index_p1 && edges[j].index_p1 == edges[k].index_p0))
                    {
                        edges.Remove(edges[k]); 
                        k--;
                        removeSelf = true;
                    }
                }
                if (removeSelf)
                {
                    edges.Remove(edges[j]);
                    j--;
                }
            }
            //用这个点和边数组中的每一条边都组合成一个三角形，加入到三角形链表中
            for (int j = 0; j < edges.Count; j++)
            {
                triangles.Add(new Tri(i, edges[j].index_p0, edges[j].index_p1));
            }

        }
        //从三角形链表中删除使用超级三角形顶点的三角形
        for (int i = 0; i < triangles.Count; i++)
        {
            if(triangles[i].index_p0 ==0|| triangles[i].index_p0==1|| triangles[i].index_p0 == 2||
                triangles[i].index_p1 == 0 || triangles[i].index_p1 == 1 || triangles[i].index_p1 == 2||
                triangles[i].index_p2 == 0 || triangles[i].index_p2 == 1 || triangles[i].index_p2 == 2)
            {
                triangles.Remove(triangles[i]);
                i--;
            }
        }
        //将超级三角形的顶点从顶点链表中删除
        vertexs.Remove(vertexs[0]);
        vertexs.Remove(vertexs[0]);
        vertexs.Remove(vertexs[0]);

        for (int i = 0; i < triangles.Count; i++)
        {
            Debug.Log("TTT:  " + (triangles[i].index_p0-3) + " " + (triangles[i].index_p1-3) + " " + (triangles[i].index_p2-3));
        }

    }
	 
	void Update () {
		
	}

    List<Vector2> creatBigTriangle(List<Vector2> vertex)
    {
        float xMin = float.MaxValue;
        float xMax = float.MinValue;
        float yMin = float.MaxValue;
        float yMax = float.MinValue;
        for (int i = 0; i < vertex.Count; i++)
        {
            xMin = xMin < vertex[i].x ? xMin : vertex[i].x;
            xMax = xMax > vertex[i].x ? xMax : vertex[i].x; 
            yMin = yMin < vertex[i].y ? yMin : vertex[i].y;
            yMax = yMax > vertex[i].y ? yMax : vertex[i].y;
        }

        Vector2 top = new Vector2((xMin + xMax) / 2, 2 * yMax - yMin);
        Vector2 right = new Vector2(xMax + (xMax - xMin) / 2 + 10, yMin - 5);
        Vector2 left = new Vector2(xMin - (xMax - xMin) / 2 - 10, yMin - 5);

        List<Vector2> triangle = new List<Vector2>();
        triangle.Add(left);
        triangle.Add(right);
        triangle.Add(top);
        return triangle;
    }

    void calculateCircle(Vector2 p0,Vector2 p1,Vector2 p2,ref Vector2 ori, ref float rad)
    {
        float ax = p1.x - p0.x;
        float ay = p1.y - p0.y;
        float bx = p2.x - p0.x;
        float by = p2.y - p0.y;

        float m = p1.x * p1.x - p0.x * p0.x + p1.y * p1.y - p0.y * p0.y;
        float u = p2.x * p2.x - p0.x * p0.x + p2.y * p2.y - p0.y * p0.y;
        float s = 1f / (2 * (ax * by - ay * bx));

        ori.x = ((p2.y - p0.y) * m + (p0.y - p1.y) * u) * s;
        ori.y = ((p0.x - p2.x) * m + (p1.x - p0.x) * u) * s;

        float dx = p0.x - ori.x;
        float dy = p0.y - ori.y;
        rad = Mathf.Sqrt(dx * dx + dy * dy);
    }
}
