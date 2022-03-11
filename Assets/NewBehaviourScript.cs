using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NewBehaviourScript : MonoBehaviour {

    public Camera cam;
    public Transform quad;
    public Transform empty;

    public GameObject prefab;
    public GameObject prefab1;
    public Material material;

    private void Start()
    {

        Debug.Log(cam.projectionMatrix);



        Matrix4x4 intr = cam.projectionMatrix;
        //intr.m00 = 4411.94043215521f / 1000;
        //intr.m01 = 0;
        //intr.m02 = 510.7157460950564f / 1000;

        //intr.m10 = 0;
        //intr.m11 = 4409.337545803506f / 1000;
        //intr.m12 = 506.648137469619f / 1000;

        //intr.m20 = 0;
        //intr.m21 = 0;
        //intr.m22 = 1;
        intr.m00 = 4424.780923062344f / 498.9636360571804f;
        intr.m11 = 4423.688934020416f / 499.4387959690157f;

        //intr.m11 = intr.m00 * 4411.940432177591f / 4409.33754582196f;
        //intr.m02 = 1;
        //intr.m12 = -0.0012541546455302734375f;


        cam.projectionMatrix = intr;

        Debug.Log(intr);

        quad.localScale = new Vector3(2 * 2 / intr.m00, 2 * 2 / intr.m11, 1);
        float x = 2 * intr.m02 / intr.m00;
        float y = 2 * intr.m12 / intr.m11;
        quad.localPosition = new Vector3(x, y, 2);

        Matrix4x4 extr = Matrix4x4.identity;
        extr.m00 = 0.02505291245027119f;
        extr.m01 = 0.02625476168803997f;
        extr.m02 = 0.9993413025920943f;
        extr.m03 = 91.17690748653905f / 1000;

        extr.m10 = 0.9985099053909803f;
        extr.m11 = -0.04913578768330829f;
        extr.m12 = -0.0237411710923684f;
        extr.m13 = -36.35370699881337f / 1000;

        extr.m20 = 0.04848010327810087f;
        extr.m21 = 0.9984469749853747f;
        extr.m22 = -0.02744663419619231f;
        extr.m23 = 66.74528706055963f / 1000;
         
        extr = extr.inverse;

        cam.transform.rotation = getRotation(extr);
        cam.transform.position = getPosition(extr);


        string[] lines = File.ReadAllLines("C:\\Users\\dell\\Desktop\\WeChat Files\\wxid_ix1enjc4tn3421\\FileStorage\\File\\2020-08\\data0811.txt");
        List<Vector2> v2d = new List<Vector2>();
        for (int i = 57; i < 96; i++)
        {
            string[] strs = lines[i].Split(',');
            strs[0]=strs[0].Replace("[","");
            strs[1]=strs[1].Replace("]", "");
            v2d.Add(new Vector2(float.Parse(strs[0]),float.Parse(strs[1])));
        }

        List<Vector3> v3d = new List<Vector3>();
        for (int i = 97; i < 136; i++)
        {
            string[] strs = lines[i].Split(',');
            strs[0] = strs[0].Replace("[", "");
            strs[2] = strs[2].Replace("]", "");
            v3d.Add(new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2])));
        }




        for (int i = 0; i < v3d.Count; i++)
        {
            GameObject obj = Instantiate<GameObject>(prefab1, empty);

            obj.transform.position = v3d[i] / 1000;
        }

        for (int i = 0; i < v2d.Count; i++)
        {
            GameObject obj = Instantiate<GameObject>(prefab,quad);

            obj.transform.localPosition = new Vector3(v2d[i].x / 1000 -0.5f,  v2d[i].y / 1000 -0.5f, 0);
        }

    }

    int num = 0;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            quad.GetChild(num).GetComponent<MeshRenderer>().material = material;
            empty.GetChild(num).GetComponent<MeshRenderer>().material = material;
            num++;
        }
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

    void test()
    {
        Vector3 p1 = new Vector3(-37.4562f, 73.1471f, -2043.35f);
        Vector3 p2 = new Vector3(-68.0333f, 69.2193f, -2060.67f);

        Matrix4x4 m = Matrix4x4.identity;
        m.SetRow(0, new Vector4(-0.055766f, -0.99666f, 0.0596646f, -207.193f));
        m.SetRow(1, new Vector4(0.996131f, -0.0514722f, 0.0712304f, 33.1782f));
        m.SetRow(2, new Vector4(-0.0679214f, 0.0634059f, 0.995674f, -2064.66f));
        m.SetRow(3, new Vector4(0, 0, 0, 1));

        Vector3 y = (p2 - p1).normalized;
        Vector3 x = Vector3.Cross(y, m.GetColumn(2)).normalized;
        Vector3 z = Vector3.Cross(x, y).normalized;

        Matrix4x4 m1 = Matrix4x4.identity;
        //m1.SetColumn(0, new Vector4(x.x, x.y, x.z, 0));
        //m1.SetColumn(1, new Vector4(y.x, y.y, y.z, 0));
        //m1.SetColumn(2, new Vector4(z.x, z.y, z.z, 0));
        //m1.SetColumn(3, new Vector4(p1.x, p1.y, p1.z, 1));

        m1.SetColumn(0, m.GetColumn(0));
        m1.SetColumn(1, m.GetColumn(1));
        m1.SetColumn(2, m.GetColumn(2));
        m1.SetColumn(3, new Vector4(p1.x, p1.y, p1.z, 1));

        Debug.Log(m.ToString("f6"));
        Debug.Log(m1.ToString("f6"));

        Matrix4x4 relative = m.inverse * m1;

        Debug.Log(relative.ToString("f6"));
    }
}
