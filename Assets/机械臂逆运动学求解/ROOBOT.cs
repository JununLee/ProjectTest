using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;



public class ROOBOT : MonoBehaviour {


    float d1 = 89.2f;  float a1 = 0;    float alpha1 = Mathf.PI / 2;  float theta1 = 0;
    float d2 = 0;      float a2 = -425; float alpha2 = 0;             float theta2 = 0;
    float d3 = 0;      float a3 = -392; float alpha3 = 0;             float theta3 = 0;
    float d4 = 109.3f; float a4 = 0;    float alpha4 = Mathf.PI / 2;  float theta4 = 0;
    float d5 = 94.75f; float a5 = 0;    float alpha5 = -Mathf.PI / 2; float theta5 = 0;
    float d6 = 82.5f;  float a6 = 0;    float alpha6 = 0;             float theta6 = 0;
     


    Matrix4x4 res;
    List<Matrix4x4> mats;
    Mat jacobian;
    void Start () {

        jacobian = new Mat(6, 6, CvType.CV_64FC1);

        Matrix4x4 t1 = matFromDH(d1,a1, alpha1, theta1);
        Debug.Log(t1.ToString("f6"));

        Matrix4x4 t2 = matFromDH(d2, a2, alpha2, theta2);
        Debug.Log(t2.ToString("f6"));

        Matrix4x4 t3 = matFromDH(d3, a3, alpha3, theta3);
        Debug.Log(t3.ToString("f6"));

        Matrix4x4 t4 = matFromDH(d4, a4, alpha4, theta4);
        Debug.Log(t4.ToString("f6"));

        Matrix4x4 t5 = matFromDH(d5, a5, alpha5, theta5);
        Debug.Log(t5.ToString("f6"));

        Matrix4x4 t6 = matFromDH(d6, a6, alpha6, theta6);
        Debug.Log(t6.ToString("f6"));


        mats = new List<Matrix4x4>();
        res = t1 * t2 * t3 * t4 * t5 * t6;
        Debug.Log(res.ToString("f6"));
        mats.Add(t1);
        mats.Add(t2);
        mats.Add(t3);
        mats.Add(t4);
        mats.Add(t5);
        mats.Add(t6);


        Mat target = new Mat(6, 1, CvType.CV_64FC1);
        target.put(0, 0, 190.5759, -191.800000, -612.537400, 0, -1, 0);
        inverseKinematics(target);
    }
	 
	void Update () {
		
	}

    Matrix4x4 matFromDH(float d,float a,float alpha,float theta )
    {
        Matrix4x4 mat1 = Matrix4x4.identity;
        mat1.m00 = Mathf.Cos(theta);
        mat1.m01 = -Mathf.Sin(theta)* Mathf.Cos(alpha);
        mat1.m02 = Mathf.Sin(theta)*Mathf.Sin(alpha);
        mat1.m03 = Mathf.Cos(theta) * a;

        mat1.m10 = Mathf.Sin(theta);
        mat1.m11 = Mathf.Cos(theta) * Mathf.Cos(alpha);
        mat1.m12 = -Mathf.Cos(theta) * Mathf.Sin(alpha);
        mat1.m13 = Mathf.Sin(theta) * a;

        mat1.m20 = 0;
        mat1.m21 = Mathf.Sin(alpha);
        mat1.m22 = Mathf.Cos(alpha);
        mat1.m23 = d;

        mat1.m30 = 0;
        mat1.m31 = 0;
        mat1.m32 = 0;
        mat1.m33 = 1; 


        return mat1;
    } 

    void calcTheta()
    {
        float m = d6 * res.m12 - res.m13;
        float n = d6 * res.m02 - res.m03;

        float angle1_0 = Mathf.Atan2(m, n) - Mathf.Atan2(d4, Mathf.Sqrt(m * m + n * n - d4 * d4));
        float angle1_1 = Mathf.Atan2(m, n) - Mathf.Atan2(d4, -Mathf.Sqrt(m * m + n * n - d4 * d4));

        Debug.Log("1111:  " + angle1_0 + " " + angle1_1);

        float angle5_0 = Mathf.Acos(res.m02 * Mathf.Sin(angle1_0) - res.m12 * Mathf.Cos(angle1_0));
        float angle5_1 = -Mathf.Acos(res.m02 * Mathf.Sin(angle1_0) - res.m12 * Mathf.Cos(angle1_0));
        float angle5_2 = Mathf.Acos(res.m02 * Mathf.Sin(angle1_1) - res.m12 * Mathf.Cos(angle1_1));
        float angle5_3 = -Mathf.Acos(res.m02 * Mathf.Sin(angle1_1) - res.m12 * Mathf.Cos(angle1_1));

        Debug.Log("5555:  " + angle5_0 + " " + angle5_1 + " " + angle5_2 + " " + angle5_3);



    }

    float s(float a)
    {
        return Mathf.Sin(a);
    }
    float c(float a)
    {
        return Mathf.Cos(a);
    }

    void calculateJacobian(List<Matrix4x4> mats,int index,Matrix4x4 end_effector)
    {
        Vector3 z0 = new Vector3(0, 0, 1);
        Vector3 zi = Vector3.zero;
        Vector3 pi = Vector3.zero;
        Vector3 p_end = new Vector3(end_effector.m03, end_effector.m13, end_effector.m23);
        Matrix4x4 transf_matrix = Matrix4x4.identity;

        for (int i = 0; i < index; i++)
        {
            transf_matrix = transf_matrix * mats[i];
        }

        zi = transf_matrix.MultiplyVector(z0);
        pi = new Vector3(transf_matrix.m03, transf_matrix.m13, transf_matrix.m23);

        Vector3 delta_vec = p_end - pi;

        Vector3 d_rev = Vector3.Cross(zi, delta_vec);

        jacobian.put(0, index, d_rev[0]);
        jacobian.put(1, index, d_rev[1]);
        jacobian.put(2, index, d_rev[2]);
        jacobian.put(3, index, zi[0]);
        jacobian.put(4, index, zi[1]);
        jacobian.put(5, index, zi[2]);  
    } 
 
    void inverseKinematics(Mat target)
    { 
        while (true)
        {
            Matrix4x4 end = Matrix4x4.identity;
            for (int i = 0; i < mats.Count; i++)
            {
                end = end * mats[i];
            }
            for (int i = 0; i < mats.Count; i++)
            {
                calculateJacobian(mats, i, end);
            }

            Mat current = new Mat(6, 1, CvType.CV_64FC1); 
            current.put(0, 0, end.m03, end.m13, end.m23, end.m02, end.m12, end.m22);
            Mat delta = target - current; 

            double n = Core.norm(delta);
            Debug.Log(n);
            if (n < 0.1)
            {
                outputTheta();
                break;
            }

            Mat j_i = new Mat();
            Core.invert(jacobian, j_i, Core.DECOMP_SVD); 
            Mat delta_theta = j_i * delta; 
            updateMats(delta_theta); 
        }
    }

    void updateMats(Mat delta)
    {
        theta1 += Mathf.Deg2Rad * (float)(delta.get(0, 0)[0]);
        theta2 += Mathf.Deg2Rad * (float)(delta.get(1, 0)[0]);
        theta3 += Mathf.Deg2Rad * (float)(delta.get(2, 0)[0]);
        theta4 += Mathf.Deg2Rad * (float)(delta.get(3, 0)[0]);
        theta5 += Mathf.Deg2Rad * (float)(delta.get(4, 0)[0]);
        theta6 += Mathf.Deg2Rad * (float)(delta.get(5, 0)[0]);
        mats.Clear(); 
        mats.Add( matFromDH(d1, a1, alpha1, theta1));
        mats.Add( matFromDH(d2, a2, alpha2, theta2));
        mats.Add( matFromDH(d3, a3, alpha3, theta3));
        mats.Add( matFromDH(d4, a4, alpha4, theta4));
        mats.Add( matFromDH(d5, a5, alpha5, theta5));
        mats.Add( matFromDH(d6, a6, alpha6, theta6));
    }

    void outputTheta()
    {
        float[] thetas = { Mathf.Rad2Deg * theta1, Mathf.Rad2Deg * theta2, Mathf.Rad2Deg * theta3, Mathf.Rad2Deg * theta4, Mathf.Rad2Deg * theta5, Mathf.Rad2Deg * theta6 };

        for (int i = 0; i < thetas.Length; i++)
        {
            int inte = (int)thetas[i];
            int inte1 = inte % 360;
            if (inte1 < -180)
                inte1 += 360;
            if (inte1 > 180)
                inte1 -= 360;
            thetas[i] = inte1 + thetas[i] - inte;
            Debug.Log(thetas[i]);
        }
    }
}
