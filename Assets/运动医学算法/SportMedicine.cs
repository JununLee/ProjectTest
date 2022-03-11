using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;

public class SportMedicine : MonoBehaviour {

    public Transform mark1;
    public Transform mark2;
    public Transform tibia;

    public Transform point0;
    public Transform point1;
    public Transform point2;

    public Transform[] points;
    public Transform cylinder;
    public Transform cylinder1;
    public Transform cylinder2;

    private Matrix4x4 mark1Mat_0;
    private Matrix4x4 mark2Mat_0;

    private Matrix4x4 mark1Mat_1;
    private Matrix4x4 mark2Mat_1;

    private Matrix4x4 zeroRelativeMat1;
    private Matrix4x4 zeroRelativeMat2;

    void Start ()
    {
        mark1Mat_0 = mark1.localToWorldMatrix;
        mark2Mat_0 = mark2.localToWorldMatrix;

        zeroRelativeMat1 = mark1Mat_0.inverse * tibia.localToWorldMatrix;
        zeroRelativeMat2 = mark2Mat_0.inverse * tibia.localToWorldMatrix;

        Vector3 v0 = point1.position - point0.position;
        Vector3 v1 = point2.position - point1.position;
        Vector3 norm = Vector3.Cross(v0, v1).normalized;
        Vector3 p0 = Vector3.ProjectOnPlane(point0.position, norm);
        Vector3 x = p0.normalized;
        Vector3 y = Vector3.Cross(norm, x).normalized;
        Vector3 p1 = Vector3.ProjectOnPlane(point1.position, norm);
        Vector3 p2 = Vector3.ProjectOnPlane(point2.position, norm);

        Vector2 np0 = new Vector2(Vector3.Project(p0, x).magnitude, Vector3.Project(p0, y).magnitude);
        Vector2 np1 = new Vector2(Vector3.Project(p1, x).magnitude, Vector3.Project(p1, y).magnitude);
        Vector2 np2 = new Vector2(Vector3.Project(p2, x).magnitude, Vector3.Project(p2, y).magnitude);

        Mat A = new Mat(3, 3, CvType.CV_64FC1);
        Mat b = new Mat(3, 1, CvType.CV_64FC1);
        A.put(0, 0, np0.x, np0.y, 1, np1.x, np1.y, 1, np2.x, np2.y, 1);
        b.put(0, 0, -(np0.x * np0.x + np0.y * np0.y), -(np1.x * np1.x + np1.y * np1.y), -(np2.x * np2.x + np2.y * np2.y)); 
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD); 
        Mat res = A_I * b;

        Vector2 O = new Vector2((float)(res.get(0, 0)[0] / -2), (float)(res.get(1, 0)[0] / -2));
        float r = Mathf.Sqrt((float)(O.x * O.x + O.y * O.y - res.get(2, 0)[0]));

        Vector2 d0 = np0 - O;
        Vector2 d1 = np2 - O;

        float angle = Vector2.Dot(d0.normalized, d1.normalized);
        angle = Mathf.Rad2Deg * Mathf.Acos(angle);
        Debug.Log(angle);

        //cylinder.up = new Vector3(0.124275f, -0.255712f, 1.81777f);
        //return;

        string ss = "";
        for (int i = 0; i < points.Length; i++)
        {
            ss += points[i].position.x + "," + points[i].position.y + "," + points[i].position.z + "\n"; 
        }
        Debug.Log(ss);
        //cylinder.up = matchingAxis(points); 
        cylinder1.up = matchingAxis1(points);
        //cylinder2.up = matchingAxis2(points);
    }
	 
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            mark1Mat_1 = mark1.localToWorldMatrix;
            mark2Mat_1 = mark2.localToWorldMatrix;

            Debug.Log(distance(mark1Mat_0, mark1Mat_1));
            Debug.Log(distance(mark2Mat_0, mark2Mat_1));

            Matrix4x4 m = mark1Mat_1 * zeroRelativeMat1;
            Matrix4x4 m2 = mark2Mat_1 * zeroRelativeMat2;

            Vector3 mx = new Vector3(m.m00, m.m10, m.m20);
            Vector3 my = new Vector3(m.m01, m.m11, m.m21);
            Vector3 mz = new Vector3(m.m02, m.m12, m.m22);

            Vector3 m2x = new Vector3(m2.m00, m2.m10, m2.m20);
            Vector3 m2y = new Vector3(m2.m01, m2.m11, m2.m21);
            Vector3 m2z = new Vector3(m2.m02, m2.m12, m2.m22);

            float anglex = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(mx, m2x));
            float angley = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(my, m2y));
            float anglez = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(mz, m2z));

            Matrix4x4 res = m.inverse * m2;
            Quaternion qua = Quaternion.LookRotation(res.GetColumn(2), res.GetColumn(1));

            anglex = Mathf.Rad2Deg * Mathf.Atan2(res.m21, res.m22);
            angley = Mathf.Rad2Deg * Mathf.Atan2(-res.m20, Mathf.Sqrt(res.m21 * res.m21 + res.m22 * res.m22));
            anglez = Mathf.Rad2Deg * Mathf.Atan2(res.m10, res.m00);
            //Vector3 angle = Quaternion.ToEulerAngles(qua);

            Vector3 vx = Vector3.ProjectOnPlane(m2.GetColumn(1), m.GetColumn(2));
            anglex = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(vx.normalized, m.GetColumn(1)));
            Vector3 vy = Vector3.ProjectOnPlane(m2.GetColumn(0), m.GetColumn(1));
            angley = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(vy.normalized, m.GetColumn(0)));
            Vector3 vz = Vector3.ProjectOnPlane(m2.GetColumn(1), m.GetColumn(0));
            anglez = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(vz.normalized, m.GetColumn(1)));
            Debug.Log("X: " + anglex + " Y: " + angley + " Z: " + anglez);

            //Matrix4x4 m = mark1Mat_0.inverse * mark1Mat_1;

            //Debug.Log(distance(Matrix4x4.identity, m));

            //Vector3 v = rodrigues(m);
            //Debug.Log(m+"-- "+v.magnitude);

            //m = mark2Mat_0.inverse * mark2Mat_1;
            //Debug.Log(distance(Matrix4x4.identity, m));

            //v = rodrigues(m);
            //Debug.Log(m +"-- "+ v.magnitude);

            //Vector3 v0 = rodrigues(mark1Mat_0);
            //Vector3 v1 = rodrigues(mark1Mat_1);

            //Debug.Log(Vector3.Dot(v0.normalized, v1.normalized));

            //v0 = rodrigues(mark2Mat_0);
            //v1 = rodrigues(mark2Mat_1);

            //Debug.Log(Vector3.Dot(v0.normalized, v1.normalized));

        }
    }

    private Vector3 rodrigues(Matrix4x4 mat)
    {
        float theta = (mat.m00 + mat.m11 + mat.m22 - 1) / 2;
        theta = Mathf.Acos(theta);
        Matrix4x4 mat_T = mat.transpose;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                mat[i, j] -= mat_T[i, j];
                mat[i, j] = mat[i, j] / (2 * Mathf.Sin(theta));
            }
        }
        return new Vector3(mat.m21, mat.m02, mat.m10);
    }

    private float distance(Matrix4x4 m0, Matrix4x4 m1)
    {
        Vector3 x0 = m0.GetColumn(0);
        Vector3 y0 = m0.GetColumn(1);
        Vector3 z0 = m0.GetColumn(2);

        Vector3 x1 = m1.GetColumn(0);
        Vector3 y1 = m1.GetColumn(1);
        Vector3 z1 = m1.GetColumn(2);

        float x = Vector3.Dot(x0.normalized, x1.normalized);
        float y = Vector3.Dot(y0.normalized, y1.normalized);
        float z = Vector3.Dot(z0.normalized, z1.normalized);

        //x = Mathf.Rad2Deg * Mathf.Acos(x);
        //y = Mathf.Rad2Deg * Mathf.Acos(y);
        //z = Mathf.Rad2Deg * Mathf.Acos(z);

        Debug.Log("angle: " + x + " " + y + " " + z);

        return Mathf.Min(x, y, z);
    }

    private Vector3 matchingAxis(Transform[] points)
    {
        Mat A = new Mat();
        Mat b = new Mat();

        List<Vector3> vs = new List<Vector3>();
        for (int i = 1; i < points.Length; i++)
        {
            vs.Add((points[i].position - points[i - 1].position).normalized);
        }

        for (int i = 1; i < vs.Count; i++)
        {
            Vector3 va = Vector3.Cross(vs[i - 1], vs[i]).normalized;

            //GameObject obj = Instantiate<GameObject>(cylinder.gameObject);
            //obj.transform.position = cylinder.position;
            //obj.transform.localScale = new Vector3(0.1f, 20, 0.1f);
            //obj.transform.up = va;

            Mat tempA = new Mat(1, 3, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1); 

            tempA.put(0, 0, va.x, va.y, va.z);
            tempb.put(0, 0, 1);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        string ss = "A: " + A.dump() + "\n";
        ss += "b: " + b.dump() + "\n";
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        ss += "A_I: " + A_I.dump() + "\n";
        Mat res = A_I * b;
        ss += "res: " + res.dump() + "\n";
        Debug.Log(ss);

        double x = res.get(0, 0)[0];
        double y = res.get(1, 0)[0];
        double z = res.get(2, 0)[0];

        Debug.Log("x: " + x + " y: " + y + " z: " + z + " " + Mathf.Sqrt((float)(x * x + y * y + z * z)));
        return new Vector3((float)(x), (float)(y), (float)(z)); 
    }

    private Vector3 matchingAxis1(Transform[] points)
    {
        Mat A = new Mat();
        Mat b = new Mat();

        Vector3 d0 = points[points.Length / 2].position - points[0].position;
        Vector3 d1 = points[points.Length - 1].position - points[points.Length/2].position;

        Vector3 d = Vector3.Cross(d0.normalized, d1.normalized).normalized;

        for (int i = 0; i < points.Length; i++)
        {
            //ax+by+cz+d = 0; -a/c * x -b/c * y - d/c = z
            //Ax+By+Z=z;

            Mat tempA = new Mat(1, 3, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            tempA.put(0, 0, points[i].position.x, points[i].position.y, 1);
            tempb.put(0, 0, points[i].position.z);

            A.push_back(tempA);
            b.push_back(tempb);
        } 
        string ss = "A: " + A.dump() + "\n";
        ss += "b: " + b.dump() + "\n";
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        ss += "A_I: " + A_I.dump() + "\n";
        Mat res = A_I * b;
        ss += "res: " + res.dump() + "\n";
        Debug.Log(ss);

        double x = res.get(0, 0)[0];
        double y = res.get(1, 0)[0];
        double z = res.get(2, 0)[0];


        Mat rmsA = new Mat();
        Mat rmsb = new Mat(4, 1, CvType.CV_64FC1);
        for (int i = 0; i < points.Length; i++)
        {  
            Mat tempA = new Mat(1, 4, CvType.CV_64FC1);  
            tempA.put(0, 0, points[i].position.x, points[i].position.y, points[i].position.z,1);
            rmsA.push_back(tempA); 
        }
        rmsb.put(0, 0, x, y, -1, z);
        Mat rms = new Mat();
        rms = rmsA * rmsb;

        double num = 0;
        for (int i = 0; i < rms.rows(); i++)
        {
            num += rms.get(i, 0)[0] * rms.get(i, 0)[0]/ rms.rows();
        }

        float r = Mathf.Sqrt((float)num);
        Debug.Log("RMS:  " + r);


        Vector3 p0 = new Vector3(0, 0, (float)z);
        Vector3 p1 = new Vector3(0, (float)(-z / y), 0);
        Vector3 p2 = new Vector3((float)(-z / x), 0, 0);
        Vector3 v0 = p1 - p0;
        Vector3 v1 = p2 - p1;
        Vector3 norm = Vector3.Cross(v0.normalized, v1.normalized).normalized;
        int sign = Vector3.Dot(norm, d) > 0 ? 1 : -1;
        norm *= sign;

        Debug.Log("x: " + norm.x + " y: " + norm.y + " z: " + norm.z + " " + norm.magnitude);
        return norm;
    }

    private Vector3 matchingAxis2(Transform[] points)
    {
        Mat A = new Mat();
        Mat b = new Mat();

        List<Vector3> vs = new List<Vector3>();
        List<Vector3> vs1 = new List<Vector3>();
        int offset1 = points.Length / 4;
        int offset2 = points.Length / 2;
        for (int i = 0; i < points.Length; i++)
        {
            int index1 = i + offset1;
            int index2 = i + offset2;
            if (index2 >= points.Length)
            {
                break;
            }
            vs.Add((points[index1].position - points[i].position).normalized);
            vs1.Add((points[index2].position - points[index1].position).normalized);
        }

        for (int i = 0; i < vs.Count; i++)
        {
            Vector3 va = Vector3.Cross(vs[i], vs1[i]).normalized;

            //GameObject obj = Instantiate<GameObject>(cylinder.gameObject);
            //obj.transform.position = cylinder.position;
            //obj.transform.localScale = new Vector3(0.1f, 20, 0.1f);
            //obj.transform.up = va;

            Mat tempA = new Mat(1, 3, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            tempA.put(0, 0, va.x, va.y, va.z);
            tempb.put(0, 0, 1);

            A.push_back(tempA);
            b.push_back(tempb);
        }
        string ss = "A: " + A.dump() + "\n";
        ss += "b: " + b.dump() + "\n";
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        ss += "A_I: " + A_I.dump() + "\n";
        Mat res = A_I * b;
        ss += "res: " + res.dump() + "\n";
        Debug.Log(ss);

        double x = res.get(0, 0)[0];
        double y = res.get(1, 0)[0];
        double z = res.get(2, 0)[0];

        Debug.Log("x: " + x + " y: " + y + " z: " + z + " " + Mathf.Sqrt((float)(x * x + y * y + z * z)));
        return new Vector3((float)(x), (float)(y), (float)(z));
    }
}
