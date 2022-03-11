using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OBBBox : MonoBehaviour {

    public Transform[] spheres;
    public Transform box; 
    List<Vector3> points = new List<Vector3>();
	void Start () {

        for (int i = 0; i < spheres.Length; i++)
        {
            points.Add(spheres[i].position);
        }

        //points.Add(new Vector3(3.7f, 1.7f, 0));
        //points.Add(new Vector3(4.1f, 3.8f, 0));
        //points.Add(new Vector3(4.7f, 2.9f, 0));
        //points.Add(new Vector3(5.2f, 2.8f, 0));
        //points.Add(new Vector3(6.0f, 4.0f, 0));
        //points.Add(new Vector3(6.3f, 3.6f, 0));
        //points.Add(new Vector3(9.7f, 6.3f, 0));
        //points.Add(new Vector3(10.0f, 4.9f, 0));
        //points.Add(new Vector3(11.0f, 3.6f, 0));
        //points.Add(new Vector3(12.5f, 6.4f, 0));
         

        Matrix4x4 ori = Matrix4x4.identity;
        Vector3 scale = Vector3.zero; 
        Vector3 center = Vector3.zero;
        getOBBBox(points, ref ori, ref scale, ref center);
        box.position = center; 
        box.rotation = Quaternion.LookRotation(ori.GetColumn(2), ori.GetColumn(1));
        box.localScale = scale+Vector3.one;
        Debug.Log(box.localToWorldMatrix.ToString("F6"));
    }
	
 
	void Update () {
		
	}

    /// <summary>
    /// 协方差矩阵
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    Matrix4x4 getConvarianceMat(List<Vector3> points)
    {
        int n = points.Count;

        float[] s1 = new float[3];
        float[] s2 = new float[6];

        for (int i = 0; i < n; i++)
        {
            s1[0] += points[i].x;
            s1[1] += points[i].y;
            s1[2] += points[i].z;

            s2[0] += points[i].x * points[i].x;
            s2[1] += points[i].y * points[i].y;
            s2[2] += points[i].z * points[i].z;
            s2[3] += points[i].x * points[i].y;
            s2[4] += points[i].x * points[i].z;
            s2[5] += points[i].y * points[i].z;
        }

        Matrix4x4 mat = Matrix4x4.identity;
        mat.m00 = (s2[0] - s1[0] * s1[0] / n) / n;
        mat.m11 = (s2[1] - s1[1] * s1[1] / n) / n;
        mat.m22 = (s2[2] - s1[2] * s1[2] / n) / n;
        mat.m01 = mat.m10 = (s2[3] - s1[0] * s1[1] / n) / n;
        mat.m02 = mat.m20 = (s2[4] - s1[0] * s1[2] / n) / n;
        mat.m12 = mat.m21 = (s2[5] - s1[1] * s1[2] / n) / n;

        return mat;
    }
     
    void rotate(ref Matrix4x4 a, ref int i,ref int j, ref int k,ref int l,ref float g ,ref float h,ref float tau,ref float s)
    {
        g = a[i + 4 * j];
        h = a[k + 4 * l];
        a[i + 4 * j] = (float)(g - s * (h + g * tau));
        a[k + 4 * l] = (float)(h + s * (g - h * tau));
    }

    void getEigenVectors( ref Matrix4x4 vout,ref Vector3 dout,Matrix4x4 cov)
    {
        int n = 3;
        int j, iq, ip, i;
        float tresh, theta, tau, t, sm, s, h, g, c;
        int nrot;
        Vector3 b = Vector3.zero;
        Vector3 z = Vector3.zero;
        Matrix4x4 v;
        Vector3 d = Vector3.zero;

        v = Matrix4x4.identity;
        for (ip = 0; ip < n; ip++)
        { 
            b[ip] = cov[ip, ip]; 
            d[ip] = cov[ip, ip];
            z[ip] = 0; 
        }

        nrot = 0;

        for (i = 0; i < 50; i++)
        {
            sm = 0.0f;
            for (ip = 0; ip < n; ip++)
                for (iq = ip + 1; iq < n; iq++)
                    sm += Mathf.Abs(cov[ip + 4 * iq]);
            if (Mathf.Abs(sm) < float.Epsilon)
            {
                v = v.transpose;
                vout = v;
                dout = d;
                return;
            }

            if (i < 3)
                tresh = 0.2f * sm / (n * n);
            else
                tresh = 0.0f;

            for (ip = 0; ip < n; ip++)
            {
                for (iq = ip + 1; iq < n; iq++)
                {
                    g = 100.0f * Mathf.Abs(cov[ip + iq * 4]);
                    float dmip = d[ip];
                    float dmiq = d[iq];

                    if (i > 3 && Mathf.Abs(dmip) + g == Mathf.Abs(dmip) && Mathf.Abs(dmiq) + g == Mathf.Abs(dmiq))
                    {
                        cov[ip + 4 * iq] = 0.0f;
                    }
                    else if (Mathf.Abs(cov[ip + 4 * iq]) > tresh)
                    {
                        h = dmiq - dmip;
                        if (Mathf.Abs(h) + g == Mathf.Abs(h))
                        {
                            t = (cov[ip + 4 * iq]) / h;
                        }
                        else
                        {
                            theta = 0.5f * h / (cov[ip + 4 * iq]);
                            t = 1.0f / (Mathf.Abs(theta) + Mathf.Sqrt(1.0f + theta * theta));
                            if (theta < 0.0f) t = -t;
                        }
                        c = 1.0f / Mathf.Sqrt(1 + t * t);
                        s = t * c;
                        tau = s / (1.0f + c);
                        h = t * cov[ip + 4 * iq];
                        z[ip] -= h;
                        z[iq] += h;
                        d[ip] -= h;
                        d[iq] += h;
                        cov[ip + 4 * iq] = 0.0f;
                        for (j = 0; j < ip; j++)
                        { 
                            rotate(ref cov, ref j, ref ip, ref j, ref iq, ref g, ref h, ref tau, ref s);
                        }
                        for (j = ip + 1; j < iq; j++)
                        { 
                            rotate(ref cov, ref ip, ref j, ref j, ref iq, ref g, ref h, ref tau, ref s);
                        }
                        for (j = iq + 1; j < n; j++)
                        { 
                            rotate(ref cov, ref ip, ref j, ref iq, ref j, ref g, ref h, ref tau, ref s);
                        }
                        for (j = 0; j < n; j++)
                        { 
                            rotate(ref v, ref j, ref ip, ref j, ref iq, ref g, ref h, ref tau, ref s);
                        }
                        nrot++;
                    }
                }
            }

            for (ip = 0; ip < n; ip++)
            {
                b[ip] += z[ip];
                d[ip] = b[ip];
                z[ip] = 0.0f;
            }
        }

        v = v.transpose;
        vout = v;
        dout = d;
        return;
    }

    Matrix4x4 getOBBOrientation(List<Vector3> points)
    {
        Matrix4x4 mat = getConvarianceMat(points);

        Matrix4x4 Evecs = Matrix4x4.identity;
        Vector3 Evals = Vector3.zero;
        getEigenVectors(ref Evecs, ref Evals, mat);

        return Evecs.transpose;
    }

    void getOBBBox(List<Vector3> points,ref Matrix4x4 orientation,ref Vector3 scale,ref Vector3 center)
    {
        Matrix4x4 m = getOBBOrientation(points);
        float xmin = 10000,ymin = 10000, zmin = 10000;
        float xmax = -10000,ymax = -10000, zmax = -10000;
        Vector3 XA = m.GetColumn(0).normalized;
        Vector3 YA = m.GetColumn(1).normalized;
        Vector3 ZA = m.GetColumn(2).normalized;
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 px = Vector3.Project(points[i], XA);
            Vector3 py = Vector3.Project(points[i], YA);
            Vector3 pz = Vector3.Project(points[i], ZA);
            float dx = px.magnitude;
            float dy = py.magnitude;
            float dz = pz.magnitude;
            px.Normalize();
            py.Normalize();
            pz.Normalize();
            dx = Vector3.Dot(px, XA) > 0 ? dx : -dx;
            dy = Vector3.Dot(py, YA) > 0 ? dy : -dy;
            dz = Vector3.Dot(pz, ZA) > 0 ? dz : -dz;
            if(dx>xmax)
            {
                xmax = dx;
            }
            if(dx<xmin)
            {
                xmin = dx;
            }
            if (dy > ymax)
            {
                ymax = dy;
            }
            if (dy < ymin)
            {
                ymin = dy;
            }
            if (dz > zmax)
            {
                zmax = dz;
            }
            if (dz < zmin)
            {
                zmin = dz;
            }

        }
        Vector3 min = new Vector3(xmin, ymin, zmin);
        Vector3 max = new Vector3(xmax, ymax, zmax);

        scale = max - min;

        min = m.MultiplyPoint3x4(min);
        max = m.MultiplyPoint3x4(max);

        center = (min + max) / 2;
         
        orientation = m;
    }
}
