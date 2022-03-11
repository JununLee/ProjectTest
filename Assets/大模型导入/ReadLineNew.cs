using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Threading;

public class ReadLineNew : MonoBehaviour {
	private List<Vector3> TVerts = new List<Vector3>();
    private List<Color> TColors = new List<Color>();
    List<List<int>> TLines = new List<List<int>>();
	public float width = 0.05f;
	public int faceScale = 3;

    List<int> triangles = new List<int>();
    List<Vector3> verts = new List<Vector3>();
    List<Color> colors = new List<Color>();
    List<Vector3> normals = new List<Vector3>();
    private string colorline;
    private bool readFin;
    Action action;
    Material material;
    //private TumorVO vo;
    public Material m;
    private void Start()
    {
        importLine(@"G:\test\FiberBundle_Model-new2.obj", m, () => { });
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
                    ConstructPoly(TLines[i], i);
                }
                List<int> ts = new List<int>();
                List<Vector3> vs = new List<Vector3>();
                List<Color> cs = new List<Color>();
                List<Vector3> ns = new List<Vector3>();
                int num = 0;
                for (int i = 0; i < verts.Count ; i++)
                {
                    ts.Add(triangles[i] - 60000 * num);
                    vs.Add(verts[i]);
                    cs.Add(colors[i]);
                    ns.Add(normals[i]);
                    if (ts.Count % 60000 == 0 || i == verts.Count - 1)
                    {
                        Mesh tempLine = new Mesh();
                        tempLine.vertices = vs.ToArray();
                        //tempLine.normals = ns.ToArray();
                        tempLine.triangles = ts.ToArray();
                        tempLine.colors = cs.ToArray();
                        tempLine.RecalculateNormals();

                        GameObject t = new GameObject(num.ToString());
                        t.transform.SetParent(go.transform, false);
                        MeshFilter meshFilter = t.AddComponent<MeshFilter>();
                        meshFilter.mesh = tempLine;
                        MeshRenderer meshRender = t.AddComponent<MeshRenderer>();
                        meshRender.material = material;
                        vs.Clear();
                        cs.Clear();
                        ns.Clear();
                        ts.Clear();
                        num++;
                    }
                }
                //if (vo != null)
                //{
                //    _parseColor(colorline, vo);
                //}
                action();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
	}

    public void importLine(string path,Material m, Action callback /*,TumorVO vo = null*/)
    {
        //this.vo = vo;
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
            if (line.IndexOf("# RGB") > -1)
            {
                colorline = line;
                continue;
            }
            string[] unit = System.Text.RegularExpressions.Regex.Split (line, "\\s+", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
      
            switch (unit[0])
			{
			case "v":
				TVerts.Add(new Vector3(float.Parse(unit[1]), float.Parse(unit[2]), float.Parse(unit[3])));
                TColors.Add(new Color(float.Parse(unit[4]) / 255, float.Parse(unit[5]) / 255, float.Parse(unit[6]) / 255));
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
    void ConstructPoly(List<int> lineT , int num)
	{
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
            colors.Add(TColors[lineT[i - 1]]);
			verts.Add (lastPos_v2);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v1_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal1);
			normals.Add (normal1);
			normals.Add (normal1);

			verts.Add (lastPos_v2);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v2_pos);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v1_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal1);
			normals.Add (normal1);
			normals.Add (normal1);

			verts.Add (lastPos_v2);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v3_pos);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v2_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal2);
			normals.Add (normal2);
			normals.Add (normal2);

			verts.Add (lastPos_v2);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (lastPos_v3);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v3_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal2);
			normals.Add (normal2);
			normals.Add (normal2);

			verts.Add (lastPos_v3);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v4_pos);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v3_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal3);
			normals.Add (normal3);
			normals.Add (normal3);

			verts.Add (lastPos_v3);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (lastPos_v4);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v4_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal3);
			normals.Add (normal3);
			normals.Add (normal3);

			verts.Add (lastPos_v1);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v1_pos);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (lastPos_v4);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal4);
			normals.Add (normal4);
			normals.Add (normal4);

			verts.Add (lastPos_v4);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v1_pos);
            colors.Add(TColors[lineT[i - 1]]);
            verts.Add (v4_pos);
            colors.Add(TColors[lineT[i - 1]]);
            normals.Add (normal4);
			normals.Add (normal4);
			normals.Add (normal4);



			lastPos_v1 = v1_pos;
			lastPos_v2 = v2_pos;
			lastPos_v3 = v3_pos;
			lastPos_v4 = v4_pos;
			lastNormal = forword;
		}
	}
    //private void _parseColor(string line, TumorVO vo)
    //{
    //    if (string.IsNullOrEmpty(line)) return;
    //    string strValue = line.Replace("# RGB ", "0x");
    //    uint c = 0xFFFFFF;
    //    //uint.TryParse(strValue, out c);//16进制字符串转uint不能这样转，略坑
    //    c = Convert.ToUInt32(strValue, 16);//要这样玩
    //    uint nR = c >> 16;
    //    uint nG = (c & 0x00FF00) >> 8;
    //    uint nB = c & 0x0000FF;
    //    vo.colorR = ((float)nR) / 255f;
    //    vo.colorG = ((float)nG) / 255f;
    //    vo.colorB = ((float)nB) / 255f;
    //}
}
