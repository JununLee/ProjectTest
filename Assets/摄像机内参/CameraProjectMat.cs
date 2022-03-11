using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraProjectMat : MonoBehaviour {

    public Camera camera;
    public Transform quad;

    Matrix4x4 originalProjection;
    private void Start()
    {
        originalProjection = camera.projectionMatrix;
        Matrix4x4 m = camera.projectionMatrix;
        ////m.m01 = 0.2f;
        m.m02 = 0.5f;
        m.m12 = 0.6f;
        camera.projectionMatrix = m;

        quad.localScale = new Vector3(2 * 2 / m.m00, 2 * 2 / m.m11, 1);
        float x = 2 * m.m02 / m.m00;
        float y = 2 * m.m12 / m.m11;
        quad.localPosition = new Vector3(x, y, 2);
        Debug.Log(camera.projectionMatrix);
    }
    void Update()
    {
        //Matrix4x4 p = originalProjection;
        //p.m12 += Mathf.Sin(Time.time * 1.2F) * 0.3F;
        //p.m10 += Mathf.Sin(Time.time * 1.5F) * 0.3F;
        //p.m12 += Mathf.Sin(Time.time * 1.5F) * 0.3F;
        //camera.projectionMatrix = p;
    }
}
