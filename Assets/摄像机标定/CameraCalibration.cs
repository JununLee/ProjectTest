using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using OpenCVForUnity.CoreModule;

public class CameraCalibration : MonoBehaviour {

    public Camera camera;
    public Transform plane;
    public Transform[] points;
    public Image[] images;

    List<Vector3> worldPoints;
    List<Vector3> localPoints;

	void Start () {

        worldPoints = new List<Vector3>();
        localPoints = new List<Vector3>();

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 m = points[i].position;
            Vector3 v = (m - camera.transform.position).normalized;
            Vector3 p = plane.position;
            Vector3 n = plane.forward;

            Vector3 lp = IntersectionLineAndPlan(m, v, p, n);
            lp = plane.worldToLocalMatrix.MultiplyPoint3x4(lp);
            lp = (lp + Vector3.one / 2) * 1024;
            Debug.Log(lp.ToString("f6"));
            images[i].transform.position = lp;
            localPoints.Add(lp);
            worldPoints.Add(m);
        }

        norm_point();
        get_H();
    }
	 
	void Update () {
		
	}

    void norm_point()
    {
        double Mu = 0, Mv = 0, ls = 0;
        double Mx = 0, My = 0, ws = 0;
         
        for (int i = 0; i < points.Length; i++)
        {
            Mu += localPoints[i].x / points.Length;
            Mv += localPoints[i].y / points.Length;
            Mx += worldPoints[i].x / points.Length;
            My += worldPoints[i].y / points.Length;
        }

        double lmean_dis = 0 , wmean_dis = 0;

        for (int i = 0; i < points.Length; i++)
        {
            lmean_dis += Mathf.Sqrt((float)((localPoints[i].x - Mu) * (localPoints[i].x - Mu) + (localPoints[i].y - Mv) * (localPoints[i].y - Mv))) / points.Length;
            wmean_dis += Mathf.Sqrt((float)((worldPoints[i].x - Mx) * (worldPoints[i].x - Mx) + (worldPoints[i].y - My) * (worldPoints[i].y - My))) / points.Length;
        }
        ls = Mathf.Sqrt(2) / lmean_dis;
        ws = Mathf.Sqrt(2) / wmean_dis;

        Mat lA = new Mat(3, 3, CvType.CV_64FC1);
        lA.put(0, 0, ls, 0, -ls * Mu, 0, ls, -ls * Mv, 0, 0, ls);
        Mat wA = new Mat(3, 3, CvType.CV_64FC1);
        wA.put(0, 0, ws, 0, -ws * Mx, 0, ws, -ws * My, 0, 0, ws);

        for (int i = 0; i < points.Length; i++)
        {
            Mat lb = new Mat(3, 1, CvType.CV_64FC1);
            Mat wb = new Mat(3, 1, CvType.CV_64FC1);
            Mat lr = new Mat(3, 1, CvType.CV_64FC1);
            Mat wr = new Mat(3, 1, CvType.CV_64FC1);

            lb.put(0, 0, localPoints[i].x, localPoints[i].y, 1);
            wb.put(0, 0, worldPoints[i].x, worldPoints[i].y, 1);

            lr = lA * lb;
            wr = wA * wb;

            localPoints[i] = new Vector3((float)(lr.get(0, 0)[0]), (float)(lr.get(1, 0)[0]), (float)(lr.get(2, 0)[0]));
            worldPoints[i] = new Vector3((float)(wr.get(0, 0)[0]), (float)(wr.get(1, 0)[0]), (float)(wr.get(2, 0)[0]));
        }
    }

    void get_H()
    {
        // sUV = HXY
        // H = {h11,h12,h13,
        //      h21,h22,h23,
        //      h31,h32,1}
        // su = h11Xw + h12Yw + h13
        // sv = h21Xw + h22Yw + h23
        // s  = h31Xw + h32Yw + 1
        //u = su/s   u = h11Xw + h12Yw + h13 - uh31Xw - uh32Yw
        //v = sv/s   v = h21Xw + h22Yw + h23 - vh31Xw - vh32Yw
        //  h11 h12 1 0 0 0 -uXw -uYw     [h11 h12 h13 h21 h22 h23 h31 h32 ]      u
        //  0 0 0 h21 h22 1 -vXw -vYw                                             v


        Mat A = new Mat();
        Mat b = new Mat();

        for (int i = 0; i < points.Length; i++)
        {
            Mat tempA = new Mat(2, 8, CvType.CV_64FC1);
            Mat tempb = new Mat(2, 1, CvType.CV_64FC1);
            double Xw = worldPoints[i].x;
            double Yw = worldPoints[i].y;
            double u = localPoints[i].x;
            double v = localPoints[i].y;
            tempA.put(0, 0, Xw, Yw, 1, 0, 0, 0, -u * Xw, -u * Yw,
                            0, 0, 0, Xw, Yw, 1, -v * Xw, -v * Yw);
            tempb.put(0, 0, u, v);

            A.push_back(tempA);
            b.push_back(tempb);
        }

        Mat pin_A = new Mat();
        Core.invert(A, pin_A, Core.DECOMP_SVD);
        Mat H = new Mat(8, 1, CvType.CV_64FC1);
        H = pin_A * b;
        Debug.Log("H: " + H.dump());

    }


    /// <summary>
    /// 线面交点
    /// </summary>
    /// <param name="m">直线上一点</param>
    /// <param name="v">直线方向</param>
    /// <param name="p">面上一点</param>
    /// <param name="n">面法线</param>
    private Vector3 IntersectionLineAndPlan(Vector3 m, Vector3 v, Vector3 p, Vector3 n)
    {
        float t = (p.x * n.x + p.y * n.y + p.z * n.z - m.x * n.x - m.y * n.y - m.z * n.z) / (v.x * n.x + v.y * n.y + v.z * n.z);
        return m + v * t;
    }
}
