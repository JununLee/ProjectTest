using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class relativematrix : MonoBehaviour {

    public Transform c1;
    public Transform c2;

    Matrix4x4 mt;
    private Vector3 ford;
    private Vector3 up;
    Matrix4x4 mr;
    private void Start()
    {
       // mt = c2.localToWorldMatrix.inverse * c1.localToWorldMatrix;


        mt = c1.localToWorldMatrix.inverse * c2.localToWorldMatrix;
        //c1.position =mt.MultiplyPoint3x4(c1.position);
        //Vector3 ford = c1.forward;
        //Vector3 up = c1.up;
        //c1.forward = m.MultiplyVector(ford);
        //c1.up = m.MultiplyVector(up);

        Matrix4x4 m1 = Matrix4x4.TRS(c1.position,c1.rotation,c1.localScale);
        Matrix4x4 m2 = Matrix4x4.TRS(c2.position, c2.rotation, c2.localScale);
        //Matrix4x4 m = m1 * m2.inverse;
        //mt = m;
        //c1.position = m.MultiplyPoint3x4(c1.position);
        //m = m * m2;
        //c1.rotation = Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));
        //m1 = Matrix4x4.TRS(Vector3.zero, c2.rotation, Vector3.one);
        //m2 = Matrix4x4.TRS(Vector3.zero, c1.rotation, Vector3.one);
        //mr = m1 * m2.inverse;
        //Debug.Log(c1.localToWorldMatrix);

        Debug.Log(m1.inverse *m2);
        ford = c2.forward;
       up = c2.up;
        
       //c1.position = getPosition(mt * c1.localToWorldMatrix);
    }
    private void Update()
    {
        Matrix4x4 mmm = c1.localToWorldMatrix * mt;

        c2.rotation = getRotation(mmm);
        c2.position = getPosition(mmm);

        //c2.rotation = Quaternion.LookRotation(c1.localToWorldMatrix.MultiplyVector(mt.GetColumn(2)), c1.localToWorldMatrix.MultiplyVector(mt.GetColumn(1)));
        //c2.position = c1.localToWorldMatrix.MultiplyPoint3x4(mt.GetPosition());
    }
    public Quaternion getRotation(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));

        Vector3 s = getScale(m);
        float qw = Mathf.Sqrt(1 + m.m00 / s.x + m.m11 / s.y + m.m22 / s.z) / 2;
        float qx = (m.m21 / s.y - m.m12 / s.z) / (4 * qw);
        float qy = (m.m02 / s.z - m.m20 / s.x) / (4 * qw);
        float qz = (m.m10 / s.x - m.m01 / s.y) / (4 * qw);
        return new Quaternion(qx, qy, qz, qw);
    }
    public Vector3 getPosition(Matrix4x4 m)
    {
        float x = m.m03;
        float y = m.m13;
        float z = m.m23;
        return new Vector3(x, y, z);
    }
    public Vector3 getScale(Matrix4x4 m)
    {
        float x = Mathf.Sqrt(m.m00 * m.m00 + m.m10 * m.m10 + m.m20 * m.m20);
        float y = Mathf.Sqrt(m.m01 * m.m01 + m.m11 * m.m11 + m.m21 * m.m21);
        float z = Mathf.Sqrt(m.m02 * m.m02 + m.m12 * m.m12 + m.m22 * m.m22);
        return new Vector3(x, y, z);
    }
}
