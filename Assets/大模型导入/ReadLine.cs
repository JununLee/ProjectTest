using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading;

public class ReadLine : MonoBehaviour {
	private List<Vector3> TVerts = new List<Vector3>();
	List<List<int>> TLines = new List<List<int>>();
	public float width = 0.01f;
	public int faceScale = 3;

    private bool readFin;
    Action action;
    Material material;
	void Start () {
		importLine (@"G:\test\FiberBundle_Model-new2.obj", new Material(Shader.Find("Standard")),()=> { });

	}
	void Update () {
        if (readFin)
        {
            readFin = false;
            try
            {
                GameObject go = new GameObject("line");
                for (int i = 0; i < TLines.Count; i++)
                {
                    Mesh ms = ConstructPoly(TLines[i], i);
                    GameObject t = new GameObject(i.ToString());
                    t.transform.SetParent(go.transform);
                    MeshFilter meshFilter = t.AddComponent<MeshFilter>();
                    meshFilter.mesh = ms;
                    MeshRenderer meshRender = t.AddComponent<MeshRenderer>();
                    meshRender.material = material;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
	}

    public void importLine(string path,Material m, Action callback)
    {
        Thread thread = new Thread(delegate () { ReadInfoObj(path); });
        thread.Start();
        material = m;
        action = callback;
    }

	void ReadInfoObj(string path)
	{
		if (!File.Exists(path)) return;
		StreamReader reader = new StreamReader(path);
		string obj = reader.ReadToEnd();
		reader.Close();
		string[] lines = obj.Split('\n');
		int tempt;
		foreach (string line in lines)
		{
			string[] unit = System.Text.RegularExpressions.Regex.Split (line, "\\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			switch (unit[0])
			{
			case "v":
				TVerts.Add(new Vector3(float.Parse(unit[1]), float.Parse(unit[2]), float.Parse(unit[3])));
				break;
			case "l":
				List<int> newline = new List<int> ();
				for (int i = 1; i < unit.Length; i++){
					if (int.TryParse (unit[i], out tempt)) {
						newline.Add (int.Parse (unit [i]));
					}
				}
				TLines.Add (newline);
				break;
			default:
				break;
			}
		}
        readFin = true;
	}

	Mesh ConstructPoly(List<int> lineT , int num)
	{
		Mesh tempLine = new Mesh ();
		List<int> triangles = new List<int>();
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();

		Vector3 forwordTemp = (TVerts[lineT[1]] - TVerts[lineT[0]]).normalized;
		Vector3 rightTemp = Vector3.Cross (forwordTemp, Vector3.right).normalized;
		Vector3 normaTemp = Vector3.Cross(rightTemp,forwordTemp);


		Vector3 lastPos_v1 = TVerts[lineT[0]] + rightTemp * width + normaTemp * width;
		Vector3 lastPos_v2 = TVerts[lineT[0]] + rightTemp * width - normaTemp * width;
		Vector3 lastPos_v3 = TVerts[lineT[0]] - rightTemp * width - normaTemp * width;
		Vector3 lastPos_v4 = TVerts[lineT[0]] - rightTemp * width + normaTemp * width;
		Vector3 lastNormal = forwordTemp;
		for (int i = 1; i < lineT.Count - 2; i++) {
			Vector3 startPos = TVerts [lineT[i-1]];
			Vector3 endPos = TVerts [lineT[i]];
			Vector3 forword = (endPos - startPos).normalized;
			if (i % faceScale != 0) {
				continue;
			}

			int n = triangles.Count;
			for (int j = 0; j < 24; j++) {
				triangles.Add (n + j);
			}
			Vector3 v1_pos = lastPos_v1 + Vector3.Project((endPos - lastPos_v1),forword);
			Vector3 v2_pos = lastPos_v2 + Vector3.Project((endPos - lastPos_v2),forword);
			Vector3 v3_pos = lastPos_v3 + Vector3.Project((endPos - lastPos_v3),forword);
			Vector3 v4_pos = lastPos_v4 + Vector3.Project((endPos - lastPos_v4),forword);

			Vector3 normal1 = -Vector3.Cross ((v2_pos - v1_pos), (lastPos_v1 - v1_pos)).normalized;
			Vector3 normal2 = -Vector3.Cross ((v3_pos - v2_pos), (lastPos_v2 - v2_pos)).normalized;
			Vector3 normal3 = -Vector3.Cross ((v4_pos - v3_pos), (lastPos_v3 - v3_pos)).normalized;
			Vector3 normal4 = -Vector3.Cross ((v1_pos - v4_pos), (lastPos_v4 - v4_pos)).normalized;


			verts.Add (lastPos_v1);
			verts.Add (lastPos_v2);
			verts.Add (v1_pos);
			normals.Add (normal1);
			normals.Add (normal1);
			normals.Add (normal1);

			verts.Add (lastPos_v2);
			verts.Add (v2_pos);
			verts.Add (v1_pos);
			normals.Add (normal1);
			normals.Add (normal1);
			normals.Add (normal1);

			verts.Add (lastPos_v2);
			verts.Add (v3_pos);
			verts.Add (v2_pos);
			normals.Add (normal2);
			normals.Add (normal2);
			normals.Add (normal2);

			verts.Add (lastPos_v2);
			verts.Add (lastPos_v3);
			verts.Add (v3_pos);
			normals.Add (normal2);
			normals.Add (normal2);
			normals.Add (normal2);

			verts.Add (lastPos_v3);
			verts.Add (v4_pos);
			verts.Add (v3_pos);
			normals.Add (normal3);
			normals.Add (normal3);
			normals.Add (normal3);

			verts.Add (lastPos_v3);
			verts.Add (lastPos_v4);
			verts.Add (v4_pos);
			normals.Add (normal3);
			normals.Add (normal3);
			normals.Add (normal3);

			verts.Add (lastPos_v1);
			verts.Add (v1_pos);
			verts.Add (lastPos_v4);
			normals.Add (normal4);
			normals.Add (normal4);
			normals.Add (normal4);

			verts.Add (lastPos_v4);
			verts.Add (v1_pos);
			verts.Add (v4_pos);
			normals.Add (normal4);
			normals.Add (normal4);
			normals.Add (normal4);



			lastPos_v1 = v1_pos;
			lastPos_v2 = v2_pos;
			lastPos_v3 = v3_pos;
			lastPos_v4 = v4_pos;
			lastNormal = forword;
		}
		tempLine.name = "line" + num.ToString();
		tempLine.vertices = verts.ToArray();
		tempLine.normals = normals.ToArray();
		tempLine.triangles = triangles.ToArray();

        return tempLine;
	}
}
