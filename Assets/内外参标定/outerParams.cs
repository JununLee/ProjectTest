using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using OpenCVForUnity.CoreModule;

public class outerParams : MonoBehaviour {

    public Transform ori;
    public Transform plan;
    public Transform[] gSpheres;

    List<Vector2> gPixs;
    void Start () {

        gPixs = new List<Vector2>();
        getPixsCoord(gSpheres, gPixs);

        //DLT();

        H();
    }
	
 
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

    void getPixsCoord(Transform[] spheres, List<Vector2> pixs)
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


    //五点不能共面
    void DLT()
    {
        Mat W3d = new Mat();
        Mat p2d = new Mat();
        for (int i = 0; i < gSpheres.Length; i++)
        {
            Mat tempA = new Mat(2, 11, CvType.CV_64FC1);
            Mat tempb = new Mat(2, 1, CvType.CV_64FC1);

            double x = gSpheres[i].localPosition.x;
            double y = gSpheres[i].localPosition.y;
            double z = gSpheres[i].localPosition.z;
            double u = gPixs[i].x;
            double v = gPixs[i].y;

            tempA.put(0, 0, x, y, z, 1, 0, 0, 0, 0, -u * x, -u * y, -u * z);
            tempA.put(1, 0, 0, 0, 0, 0, x, y, z, 1, -x * v, -y * v, -z * v);
            tempb.put(0, 0, u, v);

            W3d.push_back(tempA);
            p2d.push_back(tempb);
        }

        Mat w3d_I = new Mat();
        Core.invert(W3d, w3d_I, Core.DECOMP_SVD);

        Mat res = w3d_I * p2d;

        {
            Mat rr_res = new Mat(3, 3, CvType.CV_64FC1);
            rr_res.put(0, 0, res.get(0, 0)[0], res.get(1, 0)[0], res.get(2, 0)[0], res.get(4, 0)[0], res.get(5, 0)[0], res.get(6, 0)[0], res.get(8, 0)[0], res.get(9, 0)[0], res.get(10, 0)[0]);
            Mat r_res = rr_res * rr_res.t();
            //Debug.Log(r_res.dump());

            double f2 = r_res.get(2, 2)[0];
            double f = System.Math.Sqrt(f2);
            double b = r_res.get(0, 2)[0] / f2;
            double d = r_res.get(1, 2)[0] / f2;
            double a = System.Math.Sqrt((r_res.get(0, 0)[0] / f2) - b * b);
            double c = System.Math.Sqrt((r_res.get(1, 1)[0] / f2) - d * d);

            Debug.Log(a + " " + b + " " + c + " " + d + " " + f);

            Mat inter = new Mat(3, 3, CvType.CV_64FC1);
            inter.put(0, 0, a, 0, b, 0, c, d, 0, 0, 1);
            Debug.Log(inter.dump());

            Mat rrr_res = new Mat(3, 4, CvType.CV_64FC1);
            rrr_res.put(0, 0, res.get(0, 0)[0], res.get(1, 0)[0], res.get(2, 0)[0], res.get(3, 0)[0], res.get(4, 0)[0], res.get(5, 0)[0], res.get(6, 0)[0], res.get(7, 0)[0], res.get(8, 0)[0], res.get(9, 0)[0], res.get(10, 0)[0], 1);
            //Debug.Log(rrr_res.dump());

            inter.put(0, 0, 4857, 0, 512, 0, 4857, 512, 0, 0, 1);

            Mat r = inter.inv() * rrr_res;

            Debug.Log(r.dump());
            r = r / f;
            Debug.Log(r.dump());


        }
    }

    //三点不能共线
    void H()
    {
        Mat W3d = new Mat();
        Mat p2d = new Mat();
        for (int i = 0; i < gSpheres.Length; i++)
        {
            Mat tempA = new Mat(2, 8, CvType.CV_64FC1);
            Mat tempb = new Mat(2, 1, CvType.CV_64FC1);

            double x = gSpheres[i].localPosition.x;
            double y = gSpheres[i].localPosition.y; 
            double u = gPixs[i].x;
            double v = gPixs[i].y;

            tempA.put(0, 0, x, y, 1, 0, 0, 0, -u * x, -u * y);
            tempA.put(1, 0, 0, 0, 0, x, y, 1, -x * v, -y * v);
            tempb.put(0, 0, u, v);

            W3d.push_back(tempA);
            p2d.push_back(tempb);
        }

        Mat w3d_I = new Mat();
        Core.invert(W3d, w3d_I, Core.DECOMP_SVD); 
        Mat res = w3d_I * p2d;

        Mat r_res = new Mat(3, 3, CvType.CV_64FC1);
        r_res.put(0, 0, res.get(0, 0)[0], res.get(1, 0)[0], res.get(2, 0)[0], res.get(3, 0)[0], res.get(4, 0)[0], res.get(5, 0)[0], res.get(6, 0)[0], res.get(7, 0)[0], 1);

        Mat inter = new Mat(3, 3, CvType.CV_64FC1);

        inter.put(0, 0, 4857, 0, 512, 0, 4857, 512, 0, 0, 1);

        Mat r = inter.inv() * r_res;

        Vector3 vx = new Vector3((float)(r.get(0, 0)[0]), (float)(r.get(1, 0)[0]), (float)(r.get(2, 0)[0]));
        Vector3 vy = new Vector3((float)(r.get(0, 1)[0]), (float)(r.get(1, 1)[0]), (float)(r.get(2, 1)[0]));
        Vector3 t = new Vector3((float)(r.get(0, 2)[0]), (float)(r.get(1, 2)[0]), (float)(r.get(2, 2)[0]));

        float sx = 1.0f / vx.magnitude;
        float sy = 1.0f / vx.magnitude;
        Debug.Log(sx + " " + sy);

        Vector3 vz = Vector3.Cross(vx.normalized, vy.normalized);
        
        r *= sx;

        Debug.Log(r.dump());
    }
}
