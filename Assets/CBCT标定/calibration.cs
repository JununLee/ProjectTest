using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using OpenCVForUnity.CoreModule;


public class calibration : MonoBehaviour {

    public Transform ori;
    public Transform plan;
    public Transform[] gSpheres;
    public Transform[] bSpheres;

    List<Vector2> gPixs;
    List<Vector2> bPixs;


    float N = 0;
    float D = 0;
    float U = 0;
    float V = 0;
    float THETA = 0; 
    void Start ()
    {
        gPixs = new List<Vector2>();
        bPixs = new List<Vector2>();

        getPixsCoord(gSpheres, gPixs);
        getPixsCoord(bSpheres, bPixs);

        caculat_N();

        caculat_D();

        caculat_UV();

        caculat_theta(); 

    }

    // Update is called once per frame
    void Update () {
		
	}


    // 线面交点
    // <param name="m">直线上一点</param>
    // <param name="v">直线方向</param>
    // <param name="p">面上一点</param>
    // <param name="n">面法线</param>
    Vector3 _intersectionLineAndPlan(Vector3 m, Vector3 v, Vector3 p, Vector3 n)
	{
		float t = (p[0] * n[0] + p[1] * n[1] + p[2] * n[2] - m[0] * n[0] - m[1] * n[1] - m[2] * n[2]) / (v[0] * n[0] + v[1] * n[1] + v[2] * n[2]);
        return m + v * t;
	}

    void getPixsCoord(Transform[] spheres,List<Vector2> pixs)
    {
        Vector3 m = ori.position;
        Vector3 p = plan.position;
        Vector3 n = plan.forward;
        for (int i = 0; i < spheres.Length; i++)
        {
            Vector3 v = (spheres[i].position - m).normalized;
            Vector3 cross = _intersectionLineAndPlan(m, v, p, n);
            cross = plan.worldToLocalMatrix.MultiplyPoint3x4(cross);
            pixs.Add(new Vector2((cross.x + 0.5f) * 1024, (cross.y + 0.5f) * 1024));
        }
    }
    void caculat_N()
    {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < gPixs.Count/2; i++)
        {
            Mat tempA = new Mat(1, 2, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float a0 = gPixs[i + 5].x - gPixs[i].x;
            float a1 = gPixs[i + 5].y - gPixs[i].y;
            float b0 = gPixs[i].y * gPixs[i + 5].x - gPixs[i + 5].y * gPixs[i].x;

            tempA.put(0, 0, a0, -a1);
            tempb.put(0, 0, b0);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        Mat res = A_I * b;
        Debug.Log(res.dump());

        Mat AA = new Mat();
        Mat bb = new Mat();
        for (int i = 0; i < bPixs.Count / 2; i++)
        {
            Mat tempA = new Mat(1, 2, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float a0 = bPixs[i + 5].x - bPixs[i].x;
            float a1 = bPixs[i + 5].y - bPixs[i].y;
            float b0 = bPixs[i].y * bPixs[i + 5].x - bPixs[i + 5].y * bPixs[i].x;

            tempA.put(0, 0, a0, -a1);
            tempb.put(0, 0, b0);

            AA.push_back(tempA);
            bb.push_back(tempb);
        }
        Mat AA_I = new Mat();
        Core.invert(AA, AA_I, Core.DECOMP_SVD);
        Mat res1 = AA_I * bb;
        Debug.Log(res1.dump());

        float n = Mathf.Atan((float)((res.get(1, 0)[0] - res1.get(1, 0)[0]) / (res.get(0, 0)[0] - res1.get(0, 0)[0])));
        N = n;
        n = Mathf.Rad2Deg*n;
        Debug.Log("N: " + n);
    }

    void caculat_D()
    {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < gPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = gPixs[i].x * Mathf.Cos(N) - gPixs[i].y * Mathf.Sin(N);
            float v = gPixs[i].x * Mathf.Sin(N) + gPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v *v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        Mat res = A_I * b;
        float[] P0 = { (float)res.get(0, 0)[0], (float)res.get(1, 0)[0], (float)res.get(2, 0)[0], (float)res.get(3, 0)[0], (float)res.get(4, 0)[0] };

        Mat AA = new Mat();
        Mat bb = new Mat();
        for (int i = 0; i < bPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = bPixs[i].x * Mathf.Cos(N) - bPixs[i].y * Mathf.Sin(N);
            float v = bPixs[i].x * Mathf.Sin(N) + bPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v * v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            AA.push_back(tempA);
            bb.push_back(tempb);
        }
        Mat AA_I = new Mat();
        Core.invert(AA, AA_I, Core.DECOMP_SVD);
        Mat res1 = AA_I * bb;
        float[] P1 = { (float)res1.get(0, 0)[0], (float)res1.get(1, 0)[0], (float)res1.get(2, 0)[0], (float)res1.get(3, 0)[0], (float)res1.get(4, 0)[0] };

        {
            Vector2 C1 = new Vector2((P0[1] - P0[2] * P0[3]) / (P0[0] - P0[3] * P0[3]), (P0[0] * P0[2] - P0[1] * P0[3]) / (P0[0] - P0[3] * P0[3]));
            Vector2 C2 = new Vector2((P1[1] - P1[2] * P1[3]) / (P1[0] - P1[3] * P1[3]), (P1[0] * P1[2] - P1[1] * P1[3]) / (P1[0] - P1[3] * P1[3]));
            float a1 = P0[0] / (P0[0] * C1.x * C1.x + C1.y * C1.y + 2 * P0[3] * C1.x * C1.y - P0[4]);
            float a2 = P1[0] / (P1[0] * C2.x * C2.x + C2.y * C2.y + 2 * P1[3] * C2.x * C2.y - P1[4]);
            float b1 = a1 / P0[0];
            float b2 = a2 / P1[0];
            float c1 = P0[3] * b1;
            float c2 = P1[3] * b2;
            float m0 = (C2.y - C1.y) * Mathf.Sqrt(b2 - c2 * c2 / a2);
            float m1 = Mathf.Sqrt(b2 - c2 * c2 / a2) / Mathf.Sqrt(b1 - c1 * c1 / a1);
            float n0 = (1 - m0 * m0 - m1 * m1) / (2 * m0 * m1);
            float n1 = (a2 - a1 * m1 * m1) / (2 * m0 * m1);
             
            float d = ((a1 - 2 * n0 * n1) - Mathf.Sqrt(a1 * a1 + 4 * n1 * n1 - 4 * n0 * n1 * a1)) / (2 * n1 * n1);
            D = Mathf.Sqrt(d);
            d = D * 300.0f / 1024;
            Debug.Log("D: " + d);
        }
    }

    void caculat_UV()
    {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < gPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = gPixs[i].x * Mathf.Cos(N) - gPixs[i].y * Mathf.Sin(N);
            float v = gPixs[i].x * Mathf.Sin(N) + gPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v * v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        Mat res = A_I * b;
        float[] P0 = { (float)res.get(0, 0)[0], (float)res.get(1, 0)[0], (float)res.get(2, 0)[0], (float)res.get(3, 0)[0], (float)res.get(4, 0)[0] };

        Mat AA = new Mat();
        Mat bb = new Mat();
        for (int i = 0; i < bPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = bPixs[i].x * Mathf.Cos(N) - bPixs[i].y * Mathf.Sin(N);
            float v = bPixs[i].x * Mathf.Sin(N) + bPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v * v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            AA.push_back(tempA);
            bb.push_back(tempb);
        }
        Mat AA_I = new Mat();
        Core.invert(AA, AA_I, Core.DECOMP_SVD);
        Mat res1 = AA_I * bb;
        float[] P1 = { (float)res1.get(0, 0)[0], (float)res1.get(1, 0)[0], (float)res1.get(2, 0)[0], (float)res1.get(3, 0)[0], (float)res1.get(4, 0)[0] };

        {
            Vector2 C1 = new Vector2((P0[1] - P0[2] * P0[3]) / (P0[0] - P0[3] * P0[3]), (P0[0] * P0[2] - P0[1] * P0[3]) / (P0[0] - P0[3] * P0[3]));
            Vector2 C2 = new Vector2((P1[1] - P1[2] * P1[3]) / (P1[0] - P1[3] * P1[3]), (P1[0] * P1[2] - P1[1] * P1[3]) / (P1[0] - P1[3] * P1[3]));
            float a1 = P0[0] / (P0[0] * C1.x * C1.x + C1.y * C1.y + 2 * P0[3] * C1.x * C1.y - P0[4]);
            float a2 = P1[0] / (P1[0] * C2.x * C2.x + C2.y * C2.y + 2 * P1[3] * C2.x * C2.y - P1[4]);
            float b1 = a1 / P0[0];
            float b2 = a2 / P1[0];
            float c1 = P0[3] * b1;
            float c2 = P1[3] * b2;

            float vv = C1.y - Mathf.Sqrt(a1 + a1 * a1 * D * D) / Mathf.Sqrt(a1 * b1 - c1 * c1);
            float uu = 0.5f * C1.x + 0.5f * C2.x + 0.5f * c1 / a1 * (C1.y - vv) + 0.5f * c2 / a2 * (C2.y - vv);
            U = uu * Mathf.Cos(N) + vv * Mathf.Sin(N);
            V = -uu * Mathf.Sin(N) + vv * Mathf.Cos(N);
 

            Vector3 world = new Vector3(0, 0, 1193);
            Vector3 loacl = plan.worldToLocalMatrix.MultiplyPoint3x4(world);
            float U0 = (loacl.x + 0.5f) * 1024;
            float V0 = (loacl.y + 0.5f) * 1024;
            Debug.Log("U0: " + U0 + " V0:" + V0); 
            Debug.Log("U: " + U + " V:" + V);
        }
    }

    void caculat_theta()
    {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < gPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = gPixs[i].x * Mathf.Cos(N) - gPixs[i].y * Mathf.Sin(N);
            float v = gPixs[i].x * Mathf.Sin(N) + gPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v * v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        Mat res = A_I * b;
        float[] P0 = { (float)res.get(0, 0)[0], (float)res.get(1, 0)[0], (float)res.get(2, 0)[0], (float)res.get(3, 0)[0], (float)res.get(4, 0)[0] };

        Mat AA = new Mat();
        Mat bb = new Mat();
        for (int i = 0; i < bPixs.Count; i++)
        {
            Mat tempA = new Mat(1, 5, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            float u = bPixs[i].x * Mathf.Cos(N) - bPixs[i].y * Mathf.Sin(N);
            float v = bPixs[i].x * Mathf.Sin(N) + bPixs[i].y * Mathf.Cos(N);

            float a0 = u * u;
            float a1 = -2 * u;
            float a2 = -2 * v;
            float a3 = 2 * u * v;
            float a4 = 1.0f;
            float b0 = -(v * v);

            tempA.put(0, 0, a0, a1, a2, a3, a4);
            tempb.put(0, 0, b0);

            AA.push_back(tempA);
            bb.push_back(tempb);
        }
        Mat AA_I = new Mat();
        Core.invert(AA, AA_I, Core.DECOMP_SVD);
        Mat res1 = AA_I * bb;
        float[] P1 = { (float)res1.get(0, 0)[0], (float)res1.get(1, 0)[0], (float)res1.get(2, 0)[0], (float)res1.get(3, 0)[0], (float)res1.get(4, 0)[0] };

        {
            Vector2 C1 = new Vector2((P0[1] - P0[2] * P0[3]) / (P0[0] - P0[3] * P0[3]), (P0[0] * P0[2] - P0[1] * P0[3]) / (P0[0] - P0[3] * P0[3]));
            Vector2 C2 = new Vector2((P1[1] - P1[2] * P1[3]) / (P1[0] - P1[3] * P1[3]), (P1[0] * P1[2] - P1[1] * P1[3]) / (P1[0] - P1[3] * P1[3]));
            float a1 = P0[0] / (P0[0] * C1.x * C1.x + C1.y * C1.y + 2 * P0[3] * C1.x * C1.y - P0[4]);
            float a2 = P1[0] / (P1[0] * C2.x * C2.x + C2.y * C2.y + 2 * P1[3] * C2.x * C2.y - P1[4]);
            float b1 = a1 / P0[0];
            float b2 = a2 / P1[0];
            float c1 = P0[3] * b1;
            float c2 = P1[3] * b2;

            float t1 = D * a1 * Mathf.Sqrt(a1) / Mathf.Sqrt(a1 * b1 + a1 * a1 * b1 * D * D - c1 * c1);
            float t2 = -D * a2 * Mathf.Sqrt(a2) / Mathf.Sqrt(a2 * b2 + a2 * a2 * b2 * D * D - c2 * c2);
            float theta = -0.5f * c1 / a1 * t1 - 0.5f * c2 / a2 * t2;
            theta = Mathf.Asin(theta);
            THETA = theta;
            Debug.Log("THETA: " + Mathf.Rad2Deg * theta);
 
        }
    }
     
}
