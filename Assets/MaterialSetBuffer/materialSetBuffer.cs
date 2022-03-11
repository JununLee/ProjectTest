using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class materialSetBuffer : MonoBehaviour {

    public Material m;
    List<Vector4> p = new List<Vector4>();
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            _createBuffer();
        }
    }
    private void _createBuffer()
    {

        int len = 10000;
        Pbuffer[] bufferData = new Pbuffer[len];
        for (int i = 0; i < len; i++)
        {
            bufferData[i] = new Pbuffer();
            bufferData[i].pos = new Vector4(0.2f, 0, -0.5f, 0.2f);
        }
        ComputeBuffer buffer = new ComputeBuffer(bufferData.Length, 16);
        buffer.SetData(bufferData);
        m.SetInt("bufferLength", len);
        m.SetBuffer("buffer", buffer);
    }
    struct Pbuffer
    {
        public Vector4 pos;
    }
}
