using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TRS : MonoBehaviour {

    public Transform target;
    public Transform cube;
    public Transform par;
    private void Start()
    {
        Matrix4x4 tar = Matrix4x4.TRS(target.position, target.rotation, Vector3.one);
        Matrix4x4 pa = Matrix4x4.TRS(par.position, par.rotation, Vector3.one);
        Matrix4x4 cub = Matrix4x4.TRS(cube.position, cube.rotation, Vector3.one);
        Matrix4x4 m = (tar * pa.inverse)*(pa * cub.inverse);
        Vector3 ford = par.forward;
        Vector3 up = par.up;
        par.position = m.MultiplyPoint3x4(par.position);
        ford = m.MultiplyVector(ford);
        up = m.MultiplyVector(up);
        par.rotation = Quaternion.LookRotation(ford, up);

        //cube.localScale = getScale(target.localToWorldMatrix);
        //cube.rotation = getRotation(target.localToWorldMatrix);
        //cube.position = getPosition(target.localToWorldMatrix);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Matrix4x4 m = target.localToWorldMatrix * par.localToWorldMatrix.inverse;
            Vector3 ford = par.forward;
            Vector3 up = par.up;
            par.position = m.MultiplyPoint3x4(par.position);
            ford = m.MultiplyVector(ford);
            up = m.MultiplyVector(up);
            par.rotation = Quaternion.LookRotation(ford, up);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            Matrix4x4 m1 = par.localToWorldMatrix * cube.localToWorldMatrix.inverse;
            //par.position = m1.MultiplyPoint3x4(par.position);
            Vector3 ford = par.forward;
            Vector3 up = par.up;

            ford = m1.MultiplyVector(ford);
            up = m1.MultiplyVector(up);
            par.rotation = Quaternion.LookRotation(ford, up);
            m1 = par.localToWorldMatrix * cube.localToWorldMatrix.inverse;
            par.position = m1.MultiplyPoint3x4(par.position);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
        }
    }
    public Quaternion getRotation(Matrix4x4 m)
    {
        return Quaternion.LookRotation(m.GetColumn(2), m.GetColumn(1));

        Vector3 s = getScale(m);
        float qw = Mathf.Sqrt(1 + m.m00 / s.x+ m.m11 / s.y + m.m22 / s.z) / 2;
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
