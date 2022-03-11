using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;

public class eyeinhand : MonoBehaviour {

    public Transform tcp;
    public Transform rigid;
    public Transform ndi; 

    List<Matrix4x4> tcpMat;
    List<Matrix4x4> rigidMat;

    List<Matrix4x4> AX;
    List<Matrix4x4> XB;

    List<Vector3> ra;
    List<Vector3> rb;

    List<Vector3> Pas;
    List<Vector3> Pbs;

    List<Mat> Ra;
    List<Mat> Rb;
    List<Mat> Ta;
    List<Mat> Tb;

    //private float[,] A;
    //private float[] b;

    private Mat A;
    private Mat b;

    void Start () {
        //A = new float[3, 9];
        //b = new float[9];

        tcpMat = new List<Matrix4x4>();
        rigidMat = new List<Matrix4x4>();
        AX = new List<Matrix4x4>();
        XB = new List<Matrix4x4>();
        ra = new List<Vector3>();
        rb = new List<Vector3>();
        A = new Mat();
        b = new Mat();
        Pas = new List<Vector3>();
        Pbs = new List<Vector3>();

        Ra = new List<Mat>();
        Rb = new List<Mat>();
        Ta = new List<Mat>();
        Tb = new List<Mat>();

        //Debug.Log(tcp.localToWorldMatrix.inverse * rigid.localToWorldMatrix);

        //left
        float[] tcp_0 = { -0.649587f, -0.731427f, 0.207489f, 493.617f, -0.663491f, 0.412108f, -0.624457f, -318.649f, 0.371237f, -0.543306f, -0.752995f, 15.5624f };
        float[] tcp_1 = { -0.538352f, -0.806694f, 0.243766f, 457.31f, -0.818022f, 0.430717f, -0.381211f, -422.877f, 0.202527f, -0.404631f, -0.891771f, 95.1216f };
        float[] tcp_2 = { -0.0880307f, -0.964323f, 0.249662f, 484.624f, -0.982922f, 0.0434312f, -0.178824f, -545.588f, 0.161601f, -0.261141f, -0.951678f, 80.2886f };

        float[] tcp_3 = { -0.170308f, -0.933588f, 0.315292f, 475.184f, -0.937392f, 0.25214f, 0.240253f, -664.812f, -0.303795f, -0.254635f, -0.918079f, 113.111f };
        float[] tcp_4 = { -0.237972f, -0.67822f, 0.69526f, 256.715f, -0.950741f, 0.0162545f, -0.309561f, -490.165f, 0.198649f, -0.734679f, -0.64868f, 64.9622f };
        //right
        //float[] tcp_0 = { -0.421212f, 0.898489f, 0.123683f, 154.048f, 0.226219f, 0.23614f, -0.94502f, -318.997f, -0.878297f, -0.370075f, -0.30272f, 286.315f };
        //float[] tcp_1 = { 0.0322676f, 0.996218f, 0.0806719f, 138.403f, 0.787595f, 0.024349f, -0.615712f, -274.267f, -0.615348f, 0.0834043f, -0.783831f, 346.277f };
        //float[] tcp_2 = { 0.106754f, 0.866397f, -0.487811f, 65.535f, 0.854204f, -0.331007f, -0.400962f, -429.532f, -0.508861f, -0.373885f, -0.775416f, 207.288f };

        tcpMat.Add(creatMat(tcp_0));
        tcpMat.Add(creatMat(tcp_1));
        tcpMat.Add(creatMat(tcp_2));

        tcpMat.Add(creatMat(tcp_3));
        tcpMat.Add(creatMat(tcp_4));

        //left
        float[] ndi_0 = { -0.589166f, 0.7040125f, -0.3965473f, 21.3693f, -0.7753932f, -0.6306464f, 0.03241074f, 18.5061f, -0.2272635f, 0.3265753f, 0.917442f, 266.82f };
        float[] ndi_1 = { -0.4671354f, 0.8550217f, -0.2252166f, -2.1571f, -0.8548695f, -0.5017963f, -0.1319039f, -7.31123f, -0.2257936f, 0.1309138f, 0.9653388f, 289.72f };
        float[] ndi_2 = { -0.01005979f, 0.9818774f, -0.1892501f, -43.7534f, -0.9501763f, -0.06835327f, -0.3041264f, 17.7238f, -0.3115507f, 0.1767615f, 0.9336442f, 255.54f };

        float[] ndi_3 = { -0.09223121f, 0.9546511f, 0.2830806f, 38.3988f, -0.9537804f, -0.003043662f, -0.3004893f, 13.494f, -0.2860008f, -0.2977112f, 0.9108082f, 317.26f };
        float[] ndi_4 = { -0.1673881f, 0.9568004f, -0.2377273f, -67.6583f, -0.9617069f, -0.1053834f, 0.2530103f, 15.0657f, 0.2170279f, 0.2709749f, 0.9378014f, 332.16f };
        //right
        //float[] ndi_0 = { -0.0919862f, - 0.772796f, - 0.627954f, - 196.39f,   0.423339f, - 0.601148f,   0.677794f,   234.33f, - 0.901289f, - 0.20349f,   0.382452f, - 1965.05f };
        //float[] ndi_1 = { -0.659012f, -0.679106f, -0.323293f, -245.71f, 0.630619f, -0.733151f, 0.254576f, 201.44f, -0.409906f, -0.0361062f, 0.911413f, -1977.73f };
        //float[] ndi_2 = { -0.247758f, -0.966619f, -0.0653003f, -37.79f, 0.956344f, -0.254794f, 0.14313f, 74.78f, -0.15499f, -0.026988f, 0.987547f, -1874.96f };

        rigidMat.Add(creatMat(ndi_0));
        rigidMat.Add(creatMat(ndi_1));
        rigidMat.Add(creatMat(ndi_2));

        rigidMat.Add(creatMat(ndi_3));
        rigidMat.Add(creatMat(ndi_4));
        //
    }
	
    Matrix4x4 creatMat(float[] arr)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = arr[0]; m.m01 = arr[1]; m.m02 = arr[2]; m.m03 = arr[3];
        m.m10 = arr[4]; m.m11 = arr[5]; m.m12 = arr[6]; m.m13 = arr[7];
        m.m20 = arr[8]; m.m21 = arr[9]; m.m22 = arr[10]; m.m23 = arr[11];
        return m;
    }

    void Update () {

        if (Input.GetKeyDown(KeyCode.A))
        {
            tcpMat.Add(tcp.localToWorldMatrix);
            rigidMat.Add(ndi.localToWorldMatrix.inverse * rigid.localToWorldMatrix);
            Debug.Log("A");
        } 

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I");
            A_B();
            ra_rb();
            Pa_Pb();
            CalculatePx();
        }

    }

    private void A_B()
    {
        for (int i = 0; i < tcpMat.Count; i++)
        {
            AX.Add(tcpMat[i % tcpMat.Count].inverse * tcpMat[(i + 1) % tcpMat.Count]);
            //XB.Add(rigidMat[i % rigidMat.Count].inverse * rigidMat[(i + 1) % rigidMat.Count]);
            XB.Add(rigidMat[i % rigidMat.Count] * rigidMat[(i + 1) % rigidMat.Count].inverse);
            Mat tempRa = new Mat(3, 3, CvType.CV_64FC1);
            Mat tempRb = new Mat(3, 3, CvType.CV_64FC1);
            Mat tempTa = new Mat(3, 1, CvType.CV_64FC1);
            Mat tempTb = new Mat(3, 1, CvType.CV_64FC1);
            tempRa.put(0, 0, AX[i].m00, AX[i].m01, AX[i].m02, AX[i].m10, AX[i].m11, AX[i].m12, AX[i].m20, AX[i].m21, AX[i].m22);
            tempRb.put(0, 0, XB[i].m00, XB[i].m01, XB[i].m02, XB[i].m10, XB[i].m11, XB[i].m12, XB[i].m20, XB[i].m21, XB[i].m22);
            tempTa.put(0, 0, AX[i].m03, AX[i].m13, AX[i].m23);
            tempTb.put(0, 0, XB[i].m03, XB[i].m13, XB[i].m23);
            Ra.Add(tempRa);
            Rb.Add(tempRb);
            Ta.Add(tempTa);
            Tb.Add(tempTb);
        }
    }

    private void ra_rb()
    {
        for (int i = 0; i < AX.Count; i++)
        {
            ra.Add(rodrigues(AX[i]));
            rb.Add(rodrigues(XB[i]));
        } 
         
    }

    private Vector3 rodrigues(Matrix4x4 mat)
    {
        float theta = (mat.m00 + mat.m11 + mat.m22 - 1)/2;
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

    private void Pa_Pb()
    {
        for (int i = 0; i < ra.Count; i++)
        {
            Pas.Add(amendRodrigues(ra[i]));
            Pbs.Add(amendRodrigues(rb[i]));
        } 

    }

    private Vector3 amendRodrigues(Vector3 v)
    {
        float theta = Vector3.Magnitude(v);
        v = v.normalized;
        v = 2 * Mathf.Sin(theta / 2) * v;
        return v;

    }
    private void CalculatePx()
    {
        for (int i = 0; i < Pas.Count; i++)
        {
            Mat tempA = new Mat(3, 3, CvType.CV_64FC1);
            Mat tempB = new Mat(3, 1, CvType.CV_64FC1);
            Vector3 add = Pas[i] + Pbs[i];
            Vector3 sub = Pbs[i] - Pas[i];
            tempA.put(0, 0, 0, -add.z, add.y, add.z, 0, -add.x, -add.y, add.x, 0); 
            tempB.put(0, 0, sub.x, sub.y, sub.z);
            A.push_back(tempA);
            b.push_back(tempB);
        }
        string sss = "A: " + A.dump() + "\n"; 
        sss += "B: " + b.dump() + "\n";
        Mat A_I = new Mat();


        Core.invert(A, A_I, Core.DECOMP_SVD);
        sss += "A_I: " + A_I.dump() + "\n";

        Mat pcg = new Mat(3,1, CvType.CV_64FC1);
        pcg = A_I * b;
        sss += "pcg: " + pcg.dump() + "\n";

        pcg = 2 * pcg / Mathf.Sqrt((float)(1 + Core.norm(pcg) * Core.norm(pcg)));
        sss += "pcg: " + pcg.dump() + "\n";
        Mat eyeM = Mat.eye(3, 3, CvType.CV_64FC1);

        Vector3 px = new Vector3((float)pcg.get(0, 0)[0], (float)pcg.get(1, 0)[0], (float)pcg.get(2, 0)[0]);

        Mat sknewPcg = new Mat(3, 3, CvType.CV_64FC1);
        sknewPcg.put(0, 0, 0, -px.z, px.y, px.z, 0, -px.x, -px.y, px.x, 0);
        Mat Rcg = (1 - Core.norm(pcg) * Core.norm(pcg) / 2) * eyeM + 0.5 * (pcg *pcg.t() + Mathf.Sqrt((float)(4 - Core.norm(pcg) * Core.norm(pcg))) * sknewPcg);
        sss += "Rcg: " + Rcg.dump() + "\n";

        Mat AA = new Mat();
        Mat bb = new Mat();
        Mat tempAA = new Mat(3, 3, CvType.CV_64FC1);
        Mat tempbb = new Mat(3, 1, CvType.CV_64FC1);
        for (int i = 0; i < Ra.Count; i++)
        {
            tempAA = Ra[i] - eyeM;
            tempbb = Rcg * Tb[i] - Ta[i];

            AA.push_back(tempAA);
            bb.push_back(tempbb);
        }
        Mat AA_I = new Mat();
        Core.invert(AA, AA_I, Core.DECOMP_SVD);
        Mat Tcg = AA_I * bb;
        sss += "Tcg: " + Tcg.dump() + "\n";

        //Debug.Log(sss);

        Matrix4x4 X = Matrix4x4.identity;
        X.m00 = (float)Rcg.get(0, 0)[0];
        X.m01 = (float)Rcg.get(0, 1)[0];
        X.m02 = (float)Rcg.get(0, 2)[0];
        X.m03 = (float)Tcg.get(0, 0)[0];

        X.m10 = (float)Rcg.get(1, 0)[0];
        X.m11 = (float)Rcg.get(1, 1)[0];
        X.m12 = (float)Rcg.get(1, 2)[0];
        X.m13 = (float)Tcg.get(1, 0)[0];

        X.m20 = (float)Rcg.get(2, 0)[0];
        X.m21 = (float)Rcg.get(2, 1)[0];
        X.m22 = (float)Rcg.get(2, 2)[0];
        X.m23 = (float)Tcg.get(2, 0)[0];

        //Debug.Log(X.ToString("f5"));
        debugMat(X);
        mat2Q(X);
    }

    void mat2Q(Matrix4x4 m)
    {
        float w = Mathf.Sqrt(m.m00 + m.m11 + m.m22 + 1) / 2;
        float x = (m.m21 - m.m12) / (4 * w);
        float y = (m.m02 - m.m20) / (4 * w);
        float z = (m.m10 - m.m01) / (4 * w);

        Debug.Log("Q :  " + w + " " + x + " " + y + " " + z);

        float[] channel = { 0.946428f, -0.0468944f, -0.319491f, 17.66f, -0.111151f, 0.88163f, -0.458666f, -3.15f, 0.303182f, 0.469606f, 0.829187f, -1875.3f };
        //float[] channel = { -0.204477f, -0.775967f, -0.596711f, -216.55f, 0.596885f, -0.581988f, 0.552284f, 178.12f, -0.775833f, -0.243239f, 0.582167f, -1957.85f };

        Matrix4x4 tcp = creatMat(channel);

        Vector3 top = new Vector3(180.973f, 227.606f, -1756.04f);
        Vector3 bot = new Vector3(260.53f, 220.21f, -1760.01f);
        //Vector3 top = new Vector3(-57.6884f, 102.733f, -1817f);
        //Vector3 bot = new Vector3(15.32f, 134.24f, -1825.76f);

        top = tcp.inverse.MultiplyPoint3x4(top);
        bot = tcp.inverse.MultiplyPoint3x4(bot);

        //top = m.MultiplyPoint3x4(top);
        //bot = m.MultiplyPoint3x4(bot);
        Debug.Log(top.ToString("f6"));
        Debug.Log(bot.ToString("f6"));
         
    }

    void debugMat(Matrix4x4 m)
    {
        Debug.Log(m.m00 + "f," + m.m01 + "f," + m.m02 + "f," + m.m03 + "f,\n" + m.m10 + "f," + m.m11 + "f," + m.m12 + "f," + m.m13 + "f,\n" + m.m20 + "f," + m.m21 + "f," + m.m22 + "f," + m.m23 + "f,\n" + m.m30 + "f," + m.m31 + "f," + m.m32 + "f," + m.m33 + "f");
    }
}

