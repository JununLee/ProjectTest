using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using System.IO;

public class LMTest : MonoBehaviour {

    public Transform quad;
    public Camera camera;
    public Transform[] transPs;
    public Transform ndiPoint;
    public Transform truePoint;
    public Transform[] projPs;

    RenderTexture rt;
    List<Vector2> points2d;
    List<Vector3> points3d;

    Matrix4x4 camMat;

//0.000594	0.000000	-0.304072	-0.004063
//0.000000	0.000594	-0.304469	-0.000349
//0.000000	0.000001	0.526936	-0.000071
//0.000000	0.000000	0.000000	1.000000
    void Start () {

        Matrix4x4 m = camera.projectionMatrix;
        quad.localScale = new Vector3(2 * 2 / m.m00, 2 * 2 / m.m11, 1);
        float x = 2 * m.m02 / m.m00;
        float y = 2 * m.m12 / m.m11;
        quad.localPosition = new Vector3(x, y, 2);
        //Debug.Log(camera.projectionMatrix);

        rt = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGBFloat);
        points2d = new List<Vector2>();
        points3d = new List<Vector3>();

        for (int i = 0; i < transPs.Length; i++)
        {
            points3d.Add(transPs[i].localPosition);
        }

        camMat = Matrix4x4.identity;
        camMat.SetRow(0, new Vector4(0.000594f, 0.000000f, -0.304072f, -0.004063f));
        camMat.SetRow(1, new Vector4(0.000000f, 0.000594f, -0.304469f, -0.000349f));
        camMat.SetRow(2, new Vector4(0.000001f, 0.000000f, 0.526936f, -0.000071f));
        camMat.SetRow(3, new Vector4(0.000000f, 0.000000f, 0.000000f, 1.000000f));

    
 

        //LMAlgorithm();
    }
	 
	void Update () {

        project();
        if (Input.GetKeyDown(KeyCode.A))
        {
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            Texture2D t = new Texture2D(1024, 1024, TextureFormat.RGBAFloat, false);
            t.ReadPixels(new UnityEngine.Rect(0, 0, 1024, 1024), 0, 0);
            t.Apply();
            byte[] b = t.EncodeToPNG();
            File.WriteAllBytes(Application.dataPath+ "/LM/1.png", b);
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            string[] lines = File.ReadAllLines(Application.dataPath + "/LM/1.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] strs = lines[i].Split(',');
                Vector2 p = new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
                points2d.Add(p);
             
            }
            Mat proj = calculationProj();
            Matrix4x4 m = Matrix4x4.identity;
            m.SetRow(0, new Vector4((float)proj.get(0, 0)[0], (float)proj.get(0, 1)[0], (float)proj.get(0, 2)[0], (float)proj.get(0, 3)[0]));
            m.SetRow(1, new Vector4((float)proj.get(1, 0)[0], (float)proj.get(1, 1)[0], (float)proj.get(1, 2)[0], (float)proj.get(1, 3)[0]));
            m.SetRow(2, new Vector4((float)proj.get(2, 0)[0], (float)proj.get(2, 1)[0], (float)proj.get(2, 2)[0], (float)proj.get(2, 3)[0]));
            Debug.Log((ndiPoint.localToWorldMatrix*m.inverse).ToString("f6"));

            Mat ex = QR(proj);
            Vector3 x = new Vector3((float)ex.get(0, 0)[0], (float)ex.get(1, 0)[0], (float)ex.get(2, 0)[0]);
            Vector3 y = new Vector3((float)ex.get(0, 1)[0], (float)ex.get(1, 1)[0], (float)ex.get(2, 1)[0]);
            Vector3 z = new Vector3((float)ex.get(0, 2)[0], (float)ex.get(1, 2)[0], (float)ex.get(2, 2)[0]);
            float s = (x.magnitude + y.magnitude + z.magnitude) / 3;
            x = x / s;
            y = y / s;
            z = z / s;
            Matrix4x4 mm = Matrix4x4.identity;
            mm.SetColumn(0, x);
            mm.SetColumn(1, y);
            mm.SetColumn(2, z);
            mm.SetColumn(3, new Vector4((float)(ex.get(0, 3)[0] / s), (float)(ex.get(1, 3)[0] / s), (float)(ex.get(2, 3)[0] / s), 1));

            Debug.Log("mm:\n" + mm.ToString("f6"));
            Debug.Log(mat2rod(mm).dump());
            Debug.Log(rod2Mat(mat2rod(mm)).ToString("f6"));

            Matrix4x4 cam = ndiPoint.localToWorldMatrix * mm.inverse;
            Debug.Log(cam.ToString("f6"));
        }
        if(Input.GetKeyDown(KeyCode.C))
        { 
            Mat pp = caliOffset();
            Mat ex = QR(pp);
        }


	}

    Mat calculationProj()
    {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < points3d.Count; i++)
        {
            float x = points3d[i].x;
            float y = points3d[i].y;
            float z = points3d[i].z;
            float u = points2d[i].x;
            float v = points2d[i].y;
            Mat tempA = new Mat(2, 11, CvType.CV_32F);
            tempA.put(0, 0, x, y, z, 1, 0, 0, 0, 0, -x * u, -y * u, -z * u, 0, 0, 0, 0, x, y, z, 1, -x * v, -y * v, -z * v);
            A.push_back(tempA);
            Mat tempb = new Mat(2, 1, CvType.CV_32F);
            tempb.put(0, 0, u, v);
            b.push_back(tempb);
        }
        Mat A_i = new Mat();
        Core.invert(A, A_i, Core.DECOMP_SVD);
        Mat res = A_i * b;

        Mat proj = new Mat(3, 4, CvType.CV_32F);
        proj.put(0, 0, res.get(0, 0)[0], res.get(1, 0)[0], res.get(2, 0)[0], res.get(3, 0)[0], res.get(4, 0)[0], res.get(5, 0)[0], res.get(6, 0)[0], res.get(7, 0)[0], res.get(8, 0)[0], res.get(9, 0)[0], res.get(10, 0)[0], 1);
         
        //Debug.Log(proj.dump());
        return proj;
    }

    Mat QR(Mat m)
    {
        Mat B = new Mat(3, 3, CvType.CV_32F);
        Mat BT = new Mat(3, 3, CvType.CV_32F);
        B.put(0, 0, m.get(0, 0)[0], m.get(0, 1)[0], m.get(0, 2)[0], m.get(1, 0)[0], m.get(1, 1)[0], m.get(1, 2)[0], m.get(2, 0)[0], m.get(2, 1)[0], m.get(2, 2)[0]);
        BT = B.t();
        Mat C = B * BT;

        double u0 = C.get(0, 2)[0] / C.get(2, 2)[0];
        double v0 = C.get(1, 2)[0] / C.get(2, 2)[0];
        double ku = C.get(0, 0)[0] / C.get(2, 2)[0];
        double kc = C.get(0, 1)[0] / C.get(2, 2)[0];
        double kv = C.get(1, 1)[0] / C.get(2, 2)[0];
        double b = ((kv - v0 * v0) > 0) ? Mathf.Sqrt((float)(kv - v0 * v0)) : -999;
        double g = (kc - u0 * v0) / b;
        double a = (ku - u0 * u0 - g * g > 0) ? Mathf.Sqrt((float)(ku - u0 * u0 - g * g)) : -999;

        Mat intr = new Mat(3, 3, CvType.CV_32F);
        intr.put(0, 0, a, g, u0, 0, b, v0, 0, 0, C.get(2, 2)[0] / C.get(2, 2)[0]);
        Debug.Log("intr: \n" +intr.dump());
        Mat ex = intr.inv() * m;
        //Debug.Log(ex.dump());
        return ex;
    }

    void project()
    {
        Matrix4x4 pj = camMat.inverse * ndiPoint.localToWorldMatrix;
        for (int i = 0; i < points3d.Count; i++)
        {
            Vector3 p = pj.MultiplyPoint(points3d[i]);
            p /= p.z;
            p /= 1024.0f;
            p -= Vector3.one/2;
            p.z = 0;
            projPs[i].localPosition = p;
        }
    }

    Mat caliOffset()
    {
        Matrix4x4 pj = camMat.inverse * ndiPoint.localToWorldMatrix;
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < points3d.Count; i++)
        {
            Vector3 p = pj.MultiplyPoint(points3d[i]);
            p /= p.z;
            float x = points3d[i].x;
            float y = points3d[i].y;
            float z = points3d[i].z;
            float u = p.x;
            float v = p.y;
            Mat tempA = new Mat(2, 11, CvType.CV_32F);
            tempA.put(0, 0, x, y, z, 1, 0, 0, 0, 0, -x * u, -y * u, -z * u, 0, 0, 0, 0, x, y, z, 1, -x * v, -y * v, -z * v);
            A.push_back(tempA);
            Mat tempb = new Mat(2, 1, CvType.CV_32F);
            tempb.put(0, 0, u, v);
            b.push_back(tempb);
        }
        Mat A_i = new Mat();
        Core.invert(A, A_i, Core.DECOMP_SVD);
        Mat res = A_i * b;

        Mat pp = new Mat(3, 4, CvType.CV_32F);
        pp.put(0, 0, res.get(0, 0)[0], res.get(1, 0)[0], res.get(2, 0)[0], res.get(3, 0)[0], res.get(4, 0)[0], res.get(5, 0)[0], res.get(6, 0)[0], res.get(7, 0)[0], res.get(8, 0)[0], res.get(9, 0)[0], res.get(10, 0)[0], 1);

        Mat intr = new Mat(3, 3, CvType.CV_32F);
        intr.put(0, 0, 886.39648f, -0.024471456f, 510.24313f, 0, 886.79529f, 512.08191f, 0, 0, 1);
        Mat intr_i = new Mat();
        Core.invert(intr, intr_i);

        Mat ppp = new Mat(3, 4, CvType.CV_32F);
        ppp.put(0, 0, pj.m00, pj.m01, pj.m02, pj.m03, pj.m10, pj.m11, pj.m12, pj.m13, pj.m20, pj.m21, pj.m22, pj.m23);
        ppp /= pj.m23;
        Debug.Log("intr_i:\n" + intr_i.dump());
        Debug.Log("ppp:\n" + ppp.dump());
        Debug.Log("pp:\n" + pp.dump());

        Mat t0 = intr_i * ppp;
        Mat t1 = intr_i * pp;
        Mat t_i = new Mat();
        Debug.Log("t0:\n" + t0.dump());
        Debug.Log("t1:\n" + t1.dump());
        Core.invert(t0, t_i);
        Mat offset = t_i * t1;
        //float s = offset.GetColumn(0).magnitude;
        //offset.m00 /= s;
        //offset.m01 /= s;
        //offset.m02 /= s;
        //offset.m03 /= s;
        //offset.m10 /= s;
        //offset.m11 /= s;
        //offset.m12 /= s;
        //offset.m13 /= s;
        //offset.m20 /= s;
        //offset.m21 /= s;
        //offset.m22 /= s;
        //offset.m23 /= s;
        Debug.Log(offset.dump());
        return pp;
    }

    Matrix4x4 rod2Mat(Mat ps)
    {
        Vector3 vr = new Vector3((float)ps.get(0, 0)[0], (float)ps.get(1, 0)[0], (float)ps.get(2, 0)[0]);
        float theta = vr.magnitude;
        vr = vr.normalized;
        float sint = Mathf.Sin(theta);
        float cost = Mathf.Cos(theta);
        Mat I = Mat.eye(3, 3, CvType.CV_64F);
        Mat r = new Mat(3, 1, CvType.CV_64F);
        r.put(0, 0, vr.x, vr.y, vr.z);
        Mat rt = r.t();
        Mat m = new Mat(3, 3, CvType.CV_64F);
        m.put(0, 0, 0, -vr.z, vr.y, vr.z, 0, -vr.x, -vr.y, vr.x, 0);
        Mat R = cost * I + (1 - cost) * r * rt + sint * m;
        Matrix4x4 mat = Matrix4x4.identity;
        mat.m00 = (float)R.get(0, 0)[0];
        mat.m10 = (float)R.get(1, 0)[0];
        mat.m20 = (float)R.get(2, 0)[0];

        mat.m01 = (float)R.get(0, 1)[0];
        mat.m11 = (float)R.get(1, 1)[0];
        mat.m21 = (float)R.get(2, 1)[0];

        mat.m02 = (float)R.get(0, 2)[0];
        mat.m12 = (float)R.get(1, 2)[0];
        mat.m22 = (float)R.get(2, 2)[0];

        mat.m03 = (float)ps.get(3, 0)[0];
        mat.m13 = (float)ps.get(4, 0)[0];
        mat.m23 = (float)ps.get(5, 0)[0];

        return mat;
    }

    Mat mat2rod(Matrix4x4 mat)
    {
        float theta = (mat.m00 + mat.m11 + mat.m22 - 1) / 2;
        theta = Mathf.Acos(theta);
        Matrix4x4 mat_T = mat.transpose;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                mat[i, j] -= mat_T[i, j];
                float s = 2 * Mathf.Sin(theta);
                if (s == 0) mat[i, j] = 0;
                else mat[i, j] = mat[i, j] / (2 * Mathf.Sin(theta));
            }
        }
        Vector3 v = new Vector3(mat.m21, mat.m02, mat.m10);
        v = v.normalized;
        v = v * theta;
        Mat m = new Mat(6, 1, CvType.CV_64F);
        m.put(0, 0, v.x, v.y, v.z, mat.m03, mat.m13, mat.m23);
        return m;
    }


    Vector2 Func(Vector3 p,Mat ps)
    {
        Matrix4x4 mat = rod2Mat(ps);
        Vector3 pos = mat.MultiplyPoint3x4(p);
        Matrix4x4 pj = camMat.inverse * ndiPoint.localToWorldMatrix;
        pos = pj.MultiplyPoint(pos);
        pos /= pos.z;
        pos += Vector3.one / 2;
        return new Vector2(pos[0], pos[1]);
    }

    double Deriv(Vector3 p, Mat ps,int n,double step)
    {
        Mat param1 = ps.clone();
        Mat param2 = ps.clone();
        param1.put(n, 0, param1.get(n, 0)[0] + step);
        param2.put(n, 0, param2.get(n, 0)[0] - step);

        Vector2 p1 = Func(p, param1);
        Vector2 p2 = Func(p, param2);
        return (p1.magnitude - p2.magnitude) / (2 * step);
    }

    void LMAlgorithm()
    {
        string[] lines = File.ReadAllLines(Application.dataPath + "/LM/1.txt");
        for (int i = 0; i < lines.Length; i++)
        {
            string[] strs = lines[i].Split(',');
            Vector2 p = new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
            points2d.Add(p);

        }

        float step = 0.0001f;

        Mat ps = new Mat(6, 1, CvType.CV_64F);
        ps.put(0, 0, 0, 0, 0, 0, 0, 0);

        int m = points3d.Count;
        int num_params = ps.rows();

        Mat r = new Mat(m, 1, CvType.CV_64F);//残差
        Mat Jf = new Mat(m, num_params, CvType.CV_64F);//雅可比 

        float last_mse = 0;
        for (int i = 0; i < 1000; i++)
        {
            float mse = 0;
            for (int n = 0; n < m; n++)
            {
                float d = (points2d[n].magnitude - Func(points3d[n], ps).magnitude);//.magnitude;
                r.put(n, 0, d);
                mse += d * d;

                for (int k = 0; k < num_params; k++)
                {
                    Jf.put(n, k, Deriv(points3d[n],ps,k,step));
                }
            }
            mse /= m;

            Debug.Log(i + " MSE: " + mse + " LAST MSE: " + last_mse);
            if (Mathf.Abs(mse-last_mse)<0.0000001f)
            {
                break;
            }

            Mat delta = (Jf.t() * Jf).inv() * Jf.t() * r;
            ps += delta;


            last_mse = mse;
        }


        Matrix4x4 mmmm = rod2Mat(ps);
        Debug.Log(mmmm.ToString("f6"));
    }
}
