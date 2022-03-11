using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class floodFilltest : MonoBehaviour {

    public Texture2D tex;
    public RectTransform image;
    Color[] colors;

    private void Start()
    {
        colors = tex.GetPixels();
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            System.DateTime a = System.DateTime.Now;
            Vector2 p = inputPoint();
            floodfill((int)p.x, (int)p.y,p, colors[((int)p.x + 850 * (int)p.y)].r);
            Debug.Log(System.DateTime.Now - a);
            //tex.SetPixels(colors);
            Debug.Log(System.DateTime.Now - a);

            tex.Apply();
            Debug.Log(System.DateTime.Now - a);

        }
    }
    Vector2 inputPoint()
    {
        Vector2 pos = image.worldToLocalMatrix.MultiplyPoint(Input.mousePosition);
        pos = new Vector2((int)(pos.x+425),(int)(pos.y+425));
        return pos;
    }
    void floodfill(int x , int y,Vector2 pos, float value)
    {
        if (x >= 0 && x < 850 && y >= 0 && y < 850 && colors[x + 850 * y].g != 1)
        {
            float temp = colors[x + 850 * y].r;
            if (Mathf.Abs(temp - value) < 0.02f/*&&Vector2.Distance(pos,new Vector2(x,y))<20*/)
            {
                colors[x + 850 * y].g = 1;
                tex.SetPixel(x, y, Color.green);
                floodfill(x - 1, y,pos, value);
                floodfill(x + 1, y,pos, value);
                floodfill(x, y - 1,pos, value);
                floodfill(x, y + 1,pos, value);

            }
        }
    }
   
}
