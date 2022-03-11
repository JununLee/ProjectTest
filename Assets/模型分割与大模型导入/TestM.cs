using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestM : MonoBehaviour {

    public float planey;
    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
            Test(planey);
    }
    void Test(float planeY)
    {
        //MeshFilter mf = this.gameObject.GetComponent<MeshFilter>();

        //if (Input.GetMouseButtonDown(1))
        {
            //顶点数组转顶点容器  
            List<Vector3> verticeList = new List<Vector3>();
            int verticeCount = ImportObj.verts.Count;//  mf.mesh.vertices.Length;
            for (int verticeIndex = 0; verticeIndex < verticeCount; ++verticeIndex)
            {
                verticeList.Add(ImportObj.verts[verticeIndex]);
            }
            //三角形数组转三角形容器  
            List<int> triangleList = new List<int>();
            int triangleCount = ImportObj.triangles.Count;// mf.mesh.triangles.Length;
            for (int triangleIndex = 0; triangleIndex < triangleCount; ++triangleIndex)
            {
                triangleList.Add(ImportObj.triangles[triangleIndex]);
            }
            ////uv坐标数组转uv坐标容器  
            //List<Vector2> uvList = new List<Vector2>();
            //int uvCount = mf.mesh.uv.Length;
            //for (int uvIndex = 0; uvIndex < uvCount; ++uvIndex)
            //{
            //    uvList.Add(mf.mesh.uv[uvIndex]);
            //}
            //顶点颜色数组转顶点颜色容器  
            List<Vector3> normalList = new List<Vector3>();
            int normalCount = ImportObj.vertNormals.Count;// mf.mesh.normals.Length;
            for (int normalIndex = 0; normalIndex < normalCount; ++normalIndex)
            {
                normalList.Add(ImportObj.vertNormals[normalIndex]);
            }
            //float planeY = 0f;
            //检查每个三角面，是否存在两个顶点连接正好在直线上  
            for (int triangleIndex = 0; triangleIndex < triangleList.Count;)
            {
                int trianglePoint0 = triangleList[triangleIndex];
                int trianglePoint1 = triangleList[triangleIndex + 1];
                int trianglePoint2 = triangleList[triangleIndex + 2];

                Vector3 point0 = verticeList[trianglePoint0];
                Vector3 point1 = verticeList[trianglePoint1];
                Vector3 point2 = verticeList[trianglePoint2];

                // float planeY = 0.31f;
                //0-1，1-2相连线段被切割  
                if ((point0.y - planeY) * (point1.y - planeY) < 0 && (point1.y - planeY) * (point2.y - planeY) < 0)
                {
                    //截断0-1之间的顶点  
                    float k01 = (point1.y - point0.y) / (planeY - point0.y);
                    float newPointX01 = (point1.x - point0.x) / k01 + point0.x;
                    float newPointZ01 = (point1.z - point0.z) / k01 + point0.z;
                    Vector3 newPoint0_1 = new Vector3(newPointX01, planeY, newPointZ01);
                    verticeList.Add(newPoint0_1);
                    ////uv  
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv0 = uvList[trianglePoint0];
                    //    Vector2 uv1 = uvList[trianglePoint1];
                    //    float newUV_x = (uv1.x - uv0.x) / k01 + uv0.x;
                    //    float newUV_y = (uv1.y - uv0.y) / k01 + uv0.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    Vector3 normalX0 = normalList[trianglePoint0];
                    Vector3 normalX1 = normalList[trianglePoint1];
                    Vector3 normalX2 = normalList[trianglePoint2];
                    float newNoramlX01 = (normalX1.x - normalX0.x) / k01 + normalX0.x;
                    float newNoramlY01 = (normalX1.y - normalX0.y) / k01 + normalX0.y;
                    float newNoramlZ01 = (normalX1.z - normalX0.z) / k01 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX01, newNoramlY01, newNoramlZ01));
                    //截断1-2之间的顶点  
                    float k12 = (point2.y - point1.y) / (planeY - point1.y);
                    float newPointX12 = (point2.x - point1.x) / k12 + point1.x;
                    float newPointZ12 = (point2.z - point1.z) / k12 + point1.z;
                    Vector3 newPoint1_2 = new Vector3(newPointX12, planeY, newPointZ12);
                    verticeList.Add(newPoint1_2);
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv1 = uvList[trianglePoint1];
                    //    Vector2 uv2 = uvList[trianglePoint2];
                    //    float newUV_x = (uv2.x - uv1.x) / k12 + uv1.x;
                    //    float newUV_y = (uv2.y - uv1.y) / k12 + uv1.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    float newNoramlX12 = (normalX2.x - normalX1.x) / k12 + normalX1.x;
                    float newNoramlY12 = (normalX2.y - normalX1.y) / k12 + normalX1.y;
                    float newNoramlZ12 = (normalX2.z - normalX1.z) / k12 + normalX1.z;
                    normalList.Add(new Vector3(newNoramlX12, newNoramlY12, newNoramlZ12));

                    int newVerticeCount = verticeList.Count;
                    //插入顶点索引，以此构建新三角形  
                    triangleList.Insert(triangleIndex + 1, newVerticeCount - 2);
                    triangleList.Insert(triangleIndex + 2, newVerticeCount - 1);

                    triangleList.Insert(triangleIndex + 3, newVerticeCount - 1);
                    triangleList.Insert(triangleIndex + 4, newVerticeCount - 2);

                    triangleList.Insert(triangleIndex + 6, trianglePoint0);
                    triangleList.Insert(triangleIndex + 7, newVerticeCount - 1);
                }
                //1-2，2-0相连线段被切割  
                else if ((point1.y - planeY) * (point2.y - planeY) < 0 && (point2.y - planeY) * (point0.y - planeY) < 0)
                {
                    //截断1-2之间的顶点  
                    float k12 = (point2.y - point1.y) / (planeY - point1.y);
                    float newPointX12 = (point2.x - point1.x) / k12 + point1.x;
                    float newPointZ12 = (point2.z - point1.z) / k12 + point1.z;
                    Vector3 newPoint1_2 = new Vector3(newPointX12, planeY, newPointZ12);
                    verticeList.Add(newPoint1_2);
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv1 = uvList[trianglePoint1];
                    //    Vector2 uv2 = uvList[trianglePoint2];
                    //    float newUV_x = (uv2.x - uv1.x) / k12 + uv1.x;
                    //    float newUV_y = (uv2.y - uv1.y) / k12 + uv1.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    Vector3 normalX0 = normalList[trianglePoint0];
                    Vector3 normalX1 = normalList[trianglePoint1];
                    Vector3 normalX2 = normalList[trianglePoint2];
                    float newNoramlX12 = (normalX2.x - normalX1.x) / k12 + normalX1.x;
                    float newNoramlY12 = (normalX2.y - normalX1.y) / k12 + normalX1.y;
                    float newNoramlZ12 = (normalX2.z - normalX1.z) / k12 + normalX1.z;
                    normalList.Add(new Vector3(newNoramlX12, newNoramlY12, newNoramlZ12));

                    //截断0-2之间的顶点  
                    float k02 = (point2.y - point0.y) / (planeY - point0.y);
                    float newPointX02 = (point2.x - point0.x) / k02 + point0.x;
                    float newPointZ02 = (point2.z - point0.z) / k02 + point0.z;
                    Vector3 newPoint0_2 = new Vector3(newPointX02, planeY, newPointZ02);
                    verticeList.Add(newPoint0_2);
                    ////uv  
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv0 = uvList[trianglePoint0];
                    //    Vector2 uv2 = uvList[trianglePoint2];
                    //    float newUV_x = (uv2.x - uv0.x) / k02 + uv0.x;
                    //    float newUV_y = (uv2.y - uv0.y) / k02 + uv0.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    float newNoramlX02 = (normalX1.x - normalX0.x) / k02 + normalX0.x;
                    float newNoramlY02 = (normalX1.y - normalX0.y) / k02 + normalX0.y;
                    float newNoramlZ02 = (normalX1.z - normalX0.z) / k02 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX02, newNoramlY02, newNoramlZ02));

                    int newVerticeCount = verticeList.Count;
                    //插入顶点索引，以此构建新三角形  

                    //{0}  
                    //{1}  
                    triangleList.Insert(triangleIndex + 2, newVerticeCount - 2);

                    triangleList.Insert(triangleIndex + 3, newVerticeCount - 1);
                    triangleList.Insert(triangleIndex + 4, newVerticeCount - 2);
                    //{2}  

                    triangleList.Insert(triangleIndex + 6, newVerticeCount - 1);
                    triangleList.Insert(triangleIndex + 7, trianglePoint0);
                    triangleList.Insert(triangleIndex + 8, newVerticeCount - 2);
                }
                //0-1，2-0相连线段被切割  
                else if ((point0.y - planeY) * (point1.y - planeY) < 0 && (point2.y - planeY) * (point0.y - planeY) < 0)
                {
                    //截断0-1之间的顶点  
                    float k01 = (point1.y - point0.y) / (planeY - point0.y);
                    float newPointX01 = (point1.x - point0.x) / k01 + point0.x;
                    float newPointZ01 = (point1.z - point0.z) / k01 + point0.z;
                    Vector3 newPoint0_1 = new Vector3(newPointX01, planeY, newPointZ01);
                    verticeList.Add(newPoint0_1);
                    ////uv  
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv0 = uvList[trianglePoint0];
                    //    Vector2 uv1 = uvList[trianglePoint1];
                    //    float newUV_x = (uv1.x - uv0.x) / k01 + uv0.x;
                    //    float newUV_y = (uv1.y - uv0.y) / k01 + uv0.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    Vector3 normalX0 = normalList[trianglePoint0];
                    Vector3 normalX1 = normalList[trianglePoint1];
                    Vector3 normalX2 = normalList[trianglePoint2];
                    float newNoramlX01 = (normalX1.x - normalX0.x) / k01 + normalX0.x;
                    float newNoramlY01 = (normalX1.y - normalX0.y) / k01 + normalX0.y;
                    float newNoramlZ01 = (normalX1.z - normalX0.z) / k01 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX01, newNoramlY01, newNoramlZ01));

                    //截断0-2之间的顶点  
                    float k02 = (point2.y - point0.y) / (planeY - point0.y);
                    float newPointX02 = (point2.x - point0.x) / k02 + point0.x;
                    float newPointZ02 = (point2.z - point0.z) / k02 + point0.z;
                    Vector3 newPoint0_2 = new Vector3(newPointX02, planeY, newPointZ02);
                    verticeList.Add(newPoint0_2);
                    ////uv  
                    //if (uvList.Count > 0)
                    //{
                    //    Vector2 uv0 = uvList[trianglePoint0];
                    //    Vector2 uv2 = uvList[trianglePoint2];
                    //    float newUV_x = (uv2.x - uv0.x) / k02 + uv0.x;
                    //    float newUV_y = (uv2.y - uv0.y) / k02 + uv0.y;
                    //    uvList.Add(new Vector2(newUV_x, newUV_y));
                    //}
                    //法向量  
                    float newNoramlX02 = (normalX1.x - normalX0.x) / k02 + normalX0.x;
                    float newNoramlY02 = (normalX1.y - normalX0.y) / k02 + normalX0.y;
                    float newNoramlZ02 = (normalX1.z - normalX0.z) / k02 + normalX0.z;
                    normalList.Add(new Vector3(newNoramlX02, newNoramlY02, newNoramlZ02));

                    int newVerticeCount = verticeList.Count;
                    //插入顶点索引，以此构建新三角形  

                    //{0}  
                    triangleList.Insert(triangleIndex + 1, newVerticeCount - 2);
                    triangleList.Insert(triangleIndex + 2, newVerticeCount - 1);

                    triangleList.Insert(triangleIndex + 3, newVerticeCount - 2);
                    //{1}  
                    //{2}  

                    triangleList.Insert(triangleIndex + 6, trianglePoint2);
                    triangleList.Insert(triangleIndex + 7, newVerticeCount - 1);
                    triangleList.Insert(triangleIndex + 8, newVerticeCount - 2);
                }
                //只有0-1被切  
                else if ((point0.y - planeY) * (point1.y - planeY) < 0)
                {
                    Debug.Log("只有01被切");
                }
                //只有1-2被切  
                else if ((point1.y - planeY) * (point2.y - planeY) < 0)
                {
                    Debug.Log("只有12被切");
                }
                //只有2-0被切  
                else if ((point2.y - planeY) * (point0.y - planeY) < 0)
                {
                    Debug.Log("只有02被切");
                }
                triangleIndex += 3;
            }

            //筛选出切割面两侧的顶点索引  
            List<int> triangles1 = new List<int>();
            List<Vector3> verts1 = new List<Vector3>();
            Dictionary<int, int> index = new Dictionary<int, int>();
            List<Vector3> normal1 = new List<Vector3>();
            List<int> triangles2 = new List<int>();
            for (int triangleIndex = 0; triangleIndex < triangleList.Count; triangleIndex += 3)
            {
                int trianglePoint0 = triangleList[triangleIndex];
                int trianglePoint1 = triangleList[triangleIndex + 1];
                int trianglePoint2 = triangleList[triangleIndex + 2];

                Vector3 point0 = verticeList[trianglePoint0];
                Vector3 point1 = verticeList[trianglePoint1];
                Vector3 point2 = verticeList[trianglePoint2];
                //切割面  
                //float planeY = 0.3f;
                if (point0.y > planeY || point1.y > planeY || point2.y > planeY)
                {
                    //triangles1.Add(trianglePoint0);
                    //triangles1.Add(trianglePoint1);
                    //triangles1.Add(trianglePoint2);
                    if (index.ContainsKey(trianglePoint0))
                    {
                        triangles1.Add(index[trianglePoint0]);
                    }
                    else
                    {
                        triangles1.Add(index.Count);
                        verts1.Add(verticeList[trianglePoint0]);
                        normal1.Add(normalList[trianglePoint0]);
                        index.Add(trianglePoint0, index.Count);
                    }
                    if (index.ContainsKey(trianglePoint1))
                    {
                        triangles1.Add(index[trianglePoint1]);
                    }
                    else
                    {
                        triangles1.Add(index.Count);
                        verts1.Add(verticeList[trianglePoint1]);
                        normal1.Add(normalList[trianglePoint1]);
                        index.Add(trianglePoint1, index.Count);
                    }
                    if (index.ContainsKey(trianglePoint2))
                    {
                        triangles1.Add(index[trianglePoint2]);
                    }
                    else
                    {
                        triangles1.Add(index.Count);
                        verts1.Add(verticeList[trianglePoint2]);
                        normal1.Add(normalList[trianglePoint2]);
                        index.Add(trianglePoint2, index.Count);
                    }
                }
                else
                {
                    triangles2.Add(trianglePoint0);
                    triangles2.Add(trianglePoint1);
                    triangles2.Add(trianglePoint2);
                }
            }

            //缝合切口  
            //for (int verticeIndex = verticeCount; verticeIndex < verticeList.Count - 2; ++verticeIndex)  
            //{  
            //    triangles1.Add(verticeIndex + 2);  
            //    triangles1.Add(verticeIndex);  
            //    triangles1.Add(verticeCount);  

            //    triangles2.Add(verticeCount);  
            //    triangles2.Add(verticeIndex);  
            //    triangles2.Add(verticeIndex + 2);  
            //}  


            //mf.mesh.vertices = verticeList.ToArray();
            //mf.mesh.triangles = triangles1.ToArray();
            //if (uvList.Count > 0)
            //{
            //    mf.mesh.uv = uvList.ToArray();
            //}
            //mf.mesh.normals = normalList.ToArray();


            //分割模型  
            GameObject newModel = new GameObject("New Model");
            newModel.transform.position = transform.position;
            MeshFilter meshFilter = newModel.AddComponent<MeshFilter>();
            meshFilter.mesh.vertices = verts1.ToArray();
            //meshFilter.mesh.vertices = verticeList.ToArray();
            meshFilter.mesh.triangles = triangles1.ToArray();
            //meshFilter.mesh.uv = mf.mesh.uv;
            meshFilter.mesh.normals = normal1.ToArray();
            //meshFilter.mesh.normals = normalList.ToArray();
            Renderer newRenderer = newModel.AddComponent<MeshRenderer>();
            newRenderer.material = this.gameObject.GetComponent<MeshRenderer>().material;
        }//mousedown
    }
}
