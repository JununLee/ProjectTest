using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TipPos : MonoBehaviour {

    public Texture2D cursor;
    public Camera cam;
    public Transform quad;
    public Transform ndi;
    public Transform sphere;

    private void Start()
    {
        Cursor.SetCursor(cursor, new Vector2(16,16), CursorMode.Auto);
        QuadTRS();
        string str = sphere.localPosition.x + " " + sphere.localPosition.y + " " + sphere.localPosition.z;
        File.WriteAllText(Application.dataPath + "/TipPos/TipLocalPos.txt",str);
        str = cam.transform.position.x + " " + cam.transform.position.y + " " + cam.transform.position.z;
        File.WriteAllText(Application.dataPath + "/TipPos/CamPos.txt", str);
        str = quad.position.x + " " + quad.position.y + " " + quad.position.z;
        File.WriteAllText(Application.dataPath + "/TipPos/PlanePos.txt",str);
        str = quad.forward.x + " " + quad.forward.y + " " + quad.forward.z;
        File.WriteAllText(Application.dataPath + "/TipPos/PlaneFor.txt", str);



    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            File.WriteAllText(Application.dataPath + "/TipPos/PointPosOri.txt", "");
            File.WriteAllText(Application.dataPath + "/TipPos/NdiMatOri.txt", "");
            File.WriteAllText(Application.dataPath + "/TipPos/PointPosOriReal.txt", "");
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("cam:"+ cam.transform.position.ToString("f6"));
            Vector3 p1 = (Input.mousePosition - new Vector3(512, 512, 0)) / 1024;
            Matrix4x4 m = Matrix4x4.TRS(quad.position, quad.rotation, Vector3.one);
            p1 = quad.localToWorldMatrix.MultiplyPoint3x4(p1);
            Debug.Log("P:" + p1.ToString("f6"));
            Vector3 p = IntersectionLineAndPlan(cam.transform.position, (sphere.position - cam.transform.position).normalized, quad.position, quad.forward);
            Debug.Log("P:" + p.ToString("f6"));
            string str = p1.x + " " + p1.y + " " + p1.z + "\n"; ;
            File.AppendAllText(Application.dataPath + "/TipPos/PointPosOri.txt", str);
            Debug.Log("M:" + ndi.localToWorldMatrix.ToString("f6"));
            str = p.x + " " + p.y + " " + p.z + "\n"; ;
            File.AppendAllText(Application.dataPath + "/TipPos/PointPosOriReal.txt", str);
            Matrix4x4 nm = ndi.localToWorldMatrix;
            str = nm.m00 + " " + nm.m01 + " " + nm.m02 + " " + nm.m03 + " " + nm.m10 + " " + nm.m11 + " " + nm.m12 + " " + nm.m13 + " " + nm.m20 + " " + nm.m21 + " " + nm.m22 + " " + nm.m23 + " " + nm.m30 + " " + nm.m31 + " " + nm.m32 + " " + nm.m33 + "\n";
            File.AppendAllText(Application.dataPath + "/TipPos/NdiMatOri.txt", str);

        }
        //Debug.Log(Input.mousePosition);
    }
    private void QuadTRS()
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, new Vector4(9.52344f, 0.00000f, 0.00000f, 0.00000f));
        m.SetRow(1, new Vector4(0.00000f, 9.52344f, 0.00000f, 0.00000f));
        m.SetRow(2, new Vector4(0.00000f, 0.00000f, -1.02020f, -0.20202f));
        m.SetRow(3, new Vector4(0.00000f, 0.00000f, -1.00000f, 0.00000f));
        cam.projectionMatrix = m;
        quad.localScale = new Vector3(1 * 2 / m.m00, 1 * 2 / m.m11, 1);
        float x = 1 * m.m02 / m.m00;
        float y = 1 * m.m12 / m.m11;
        quad.localPosition = new Vector3(x, y, 1);
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
