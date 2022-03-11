using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;

public class MatchingSphere : MonoBehaviour {

    // (x - Ox)² + (y - Oy)² + (z - Oz)² = r²;
    // x² + y² + z² - 2xOx - 2yOy - 2zOz + Ox² + Oy² + Oz² - r² = 0
    // -2*Ox = a;-2*Oy = b;-2*Oz = c;Ox² + Oy² + Oz² - r² = d;
    // x² + y² + z² + ax + by + cz + d = 0;
    // ax + by + cz + d = -(x² + y² + z²);
    //  { x0, y0, z0, 1          { a              {-(x0² + y0² + z0²)
    //    x1, y1, z1, 1    X       b        =      -(x1² + y1² + z1²)
    //    ... ... ... 1            c                      ....
    //    xi, yi, zi, 1}           d}              -(xi² + yi² + zi²)}
    //  求解超定方程：伪逆法（以下） 高斯消去法（matlab的 左除（A\b)） 最小二乘 

    public Transform sphere;
    public Transform[] points;
	void Start () {
        Mat A = new Mat();
        Mat b = new Mat();
        for (int i = 0; i < points.Length; i++)
        {
            Mat tempA = new Mat(1, 4, CvType.CV_64FC1);
            Mat tempb = new Mat(1, 1, CvType.CV_64FC1);

            tempA.put(0, 0, points[i].position.x, points[i].position.y, points[i].position.z, 1);
            tempb.put(0, 0, -(points[i].position.x * points[i].position.x + points[i].position.y * points[i].position.y + points[i].position.z * points[i].position.z));

            A.push_back(tempA);
            b.push_back(tempb); 
        }
        string ss = "A: " + A.dump() + "\n";
        ss+= "b: " + b.dump() + "\n"; 
        Mat A_I = new Mat();
        Core.invert(A, A_I, Core.DECOMP_SVD);
        ss += "A_I: " + A_I.dump() + "\n";
        Mat res = A_I * b;
        ss += "res: " + res.dump() + "\n";
        Debug.Log(ss);

        double x = res.get(0, 0)[0] / -2;
        double y = res.get(1, 0)[0] / -2;
        double z = res.get(2, 0)[0] / -2;
        float r = Mathf.Sqrt((float)(x * x + y * y + z * z - res.get(3, 0)[0]));

        Debug.Log("x: " + x + " y: " + y + " z: " + z + " r: " + r);
        sphere.position = new Vector3((float)(x), (float)(y), (float)(z));
        sphere.localScale = Vector3.one * r * 2;
    }
	
 
	void Update () {
		
	}
}
