using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawLineDemo : MonoBehaviour
{
    public Material mat;
    private Material lineMaterial;

    void Start()
    {
    //    mat = new Material("Shader \"Lines/Colored Blended\" {" +

    //"SubShader { Pass {" +

    //"   BindChannels { Bind \"Color\",color }" +

    //"   Blend SrcAlpha OneMinusSrcAlpha" +

    //"   ZWrite Off Cull Off Fog { Mode Off }" +

    //"} } }");
        // Unity has a built-in shader that is useful for drawing
        // simple colored things.
        Shader shader = Shader.Find("Hidden/Internal-Colored");
        lineMaterial = new Material(shader);
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        // Turn on alpha blending
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        // Turn backface culling off
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        // Turn off depth writes
        lineMaterial.SetInt("_ZWrite", 0);
        
    }


    void OnPostRender()
    {
        //GL.PushMatrix();
        //mat.SetPass(0);
        ////绘制2D线段，注释掉GL.LoadOrtho();则绘制3D图形
        //GL.LoadOrtho();
        ////开始绘制直线类型，需要两个顶点
        //GL.Begin(GL.QUADS);
        ////绘制起点，绘制的点需在Begin和End之间
        //GL.Vertex3(0, 0, 0.1f);
        //GL.Color(new Color(1,0,0,1));
        //GL.Vertex3(0, 1, 0.1f);
        //GL.Color(new Color(0, 1, 0, 1));

        //GL.Vertex3(1, 1, 0.1f);
        //GL.Color(new Color(0, 0, 1, 1));

        //GL.Vertex3(1, 0, 0.1f);
        //GL.Color(new Color(1, 1, 1, 1));

        ////GL.s
        //GL.End();
        //GL.PopMatrix();

        GL.PushMatrix();
        lineMaterial.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);
        GL.Color(Color.red);
        GL.Vertex3(1, 0, 0);
        GL.Vertex3(0, 1, 0);
        GL.Color(Color.yellow);
        GL.Vertex3(0, 0, 0);
        GL.Vertex3(1, 1, 0);
        GL.End();
        GL.PopMatrix();
    }
}

