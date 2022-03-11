using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class test : MonoBehaviour {

    public Transform tcp;
    public Transform cam;
    public Transform sph;

    public Transform cube1;

    public MeshRenderer quad;
    public Image img;

    string[] lines;
    List<Vector3> vecs;
    private void Start() {
        
        Debug.Log(cube1.rotation.ToString("f6"));
        Debug.Log(cube1.localToWorldMatrix.ToString("f6"));

        Vector3 v0 = new Vector3(0.045f, -0.17f, -0.98f);
        Vector3 v1 = new Vector3(-18, -7, -54);
        v1 = v1.normalized;

        float ang = Vector3.Dot(v0, v1);
        Debug.Log(v1.ToString("f6") +" "+Mathf.Rad2Deg * Mathf.Acos(ang));


        vecs = new List<Vector3>();
        Matrix4x4 mat = new Matrix4x4();
        mat.m00 = 0.99252f; mat.m01 = -0.01123f; mat.m02 = 0.12153f; mat.m03 = -10.96167f;
        mat.m10 = 0.01522f; mat.m11 = 0.99936f; mat.m12 = -0.03233f; mat.m13 = 1.11493f;
        mat.m20 = -0.12109f; mat.m21 = 0.03395f; mat.m22 = 0.99206f; mat.m23 = -0.46447f;
        mat.m30 = 0; mat.m31 = 0; mat.m32 = 0; mat.m33 = 1;

        //Matrix4x4 mat1 = creatMat(new Vector3(-266.717f, -381.744f, 2578.2f), new Vector3(-494.652f, -444.234f, 2480f), new Vector3(-553.531f, -43.1178f, 2372.2f));

        //debugMat(mat1);

        //Matrix4x4 mat2 = creatMat(new Vector3(273.328f, -440.49f, 2459.1f), new Vector3(26.6866f, -514.202f, 2446.4f), new Vector3(-92.6725f, -115.1f, 2406.7f));
        //debugMat(mat2);


        //Matrix4x4 mat3 = creatMat(new Vector3(329.287f, -473.199f, 2362.4f), new Vector3(96.6775f, -512.126f, 2469.7f), new Vector3(34.2001f, -87.4624f, 2490.5f));
        //debugMat(mat3);
         

        lines = File.ReadAllLines("C:/Users/dell/Desktop/TEST/TEST/pc.ply");
        Color[] col = new Color[640 * 400]; 
        for (int i = 0; i < col.Length; i++)
        {
            string[] strs = lines[i+14].Split(' ');
            col[i] = new Color(float.Parse(strs[6])/255f, float.Parse(strs[7]) / 255f, float.Parse(strs[8]) / 255f);
        }
        Texture2D tex = new Texture2D(640, 400,TextureFormat.ARGB32,false);
        tex.SetPixels(col);
        tex.Apply();
        byte[] bs = tex.EncodeToPNG();
        File.WriteAllBytes("C:/Users/dell/Desktop/TEST/TEST/pc.png", bs);
        //quad.material.mainTexture = tex;
        img.sprite = Sprite.Create(tex, new Rect(0, 0, 640, 400), Vector2.zero);
    }
    int num = 0;
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            num++;
            Vector2 p = Input.mousePosition;
            int index = (int)(p.x + p.y * 640 + 14);
            string[] strs = lines[index].Split(' ');
            Vector3 v = new Vector3(float.Parse(strs[0]), float.Parse(strs[1]), float.Parse(strs[2]));
            Debug.Log(p.x+" " + p.y+" "+ index+"-----" +v.ToString("f6") + " (" + -float.Parse(strs[3]) + " " + -float.Parse(strs[4]) + " " + -float.Parse(strs[5])+") " + float.Parse(strs[6])+" "+ float.Parse(strs[7])+" "+ float.Parse(strs[8])+"------" +num);
            vecs.Add(v);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            Matrix4x4 mat = creatMat(vecs[0], vecs[1], vecs[2]);
            debugMat(mat);
            num = 0;
            Debug.Log(vecs[1].x + "," + vecs[1].y + "," + vecs[1].z + "," + mat.m02 + "," + mat.m12 + "," + mat.m22 + "," + mat.m00 + "," + mat.m10 + "," + mat.m20);

            vecs.Clear();
        }
    }

  
    Matrix4x4 creatMat(Vector3 p0 ,Vector3 p1,Vector3 p2 )
    {
        Vector3 x = (p0 - p1).normalized;
        Vector3 y = (p2 - p1).normalized;
        Vector3 z = Vector3.Cross(x, y).normalized;
        x = Vector3.Cross(y, z).normalized;

        Matrix4x4 mat = Matrix4x4.identity;
        mat.m00 = x.x;
        mat.m10 = x.y;
        mat.m20 = x.z;

        mat.m01 = y.x;
        mat.m11 = y.y;
        mat.m21 = y.z;

        mat.m02 = z.x;
        mat.m12 = z.y;
        mat.m22 = z.z;

        mat.m03 = p1.x;
        mat.m13 = p1.y;
        mat.m23 = p1.z;

        return mat;
    }

    void debugMat(Matrix4x4 m)
    {
        Debug.Log(m.m00 + "f," + m.m01 + "f," + m.m02 + "f," + m.m03 + "f,\n" + m.m10 + "f," + m.m11 + "f," + m.m12 + "f," + m.m13 + "f,\n" + m.m20 + "f," + m.m21 + "f," + m.m22 + "f," + m.m23 + "f,\n" + m.m30 + "f," + m.m31 + "f," + m.m32 + "f," + m.m33 + "f");
    }
    Matrix4x4 creatMat(float[] arr)
    {
        Matrix4x4 m = Matrix4x4.identity;
        m.m00 = arr[0]; m.m01 = arr[1]; m.m02 = arr[2]; m.m03 = arr[3];
        m.m10 = arr[4]; m.m11 = arr[5]; m.m12 = arr[6]; m.m13 = arr[7];
        m.m20 = arr[8]; m.m21 = arr[9]; m.m22 = arr[10]; m.m23 = arr[11];
        return m;
    }
}
