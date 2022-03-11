////--------------------------------------------------------------------
/// Namespace:          
/// Class:              TransferFunctionPainter
/// Description:        Simple manager to choose colors and draw on a
///                         texture using UI elements.
/// Author:             LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// This file is part of the examples of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
////--------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

public class TransferFunctionPainter : MonoBehaviour {

#if UNITY_EDITOR
    float timeSinceLastSave = 0;
#endif

    public float autoSaveTexAfterSec = 5; 

    public VolumeViewer.VolumeComponent volumeComponent;

    public RawImage colorPickerImage;
    public RawImage shadePickerImage;
    public RawImage alphaPickerImage;
    public RawImage brushSizePickerImage;
    public RawImage[] colorImage;
    public RawImage transferFunctionImage;

    [SerializeField]
    int textureResolution = 512;

    Texture2D colorTex;
    Texture2D shadeTex;
    Texture2D alphaTex;
    Texture2D transferFunctionTex;

    Color[] colorTexColor;
    Color[] shadeTexColor;
    Color[] alphaTexColor;
    Color[] transferFunctionColor;

    BrushColor[] brushColor;

    Color[] baseColor;

    int selectedColorIndex=0;
    int paintColorIndex=0;
    int brushSize = 5;
    int numColors;

    private class BrushColor {
        public float colorValue;
        public float alphaValue;
        public Vector2 shadeValue;

        public Color selectedColor;
        public Color selectedShade;

        public Texture2D colorTex;
        public Color[] colorTexColor;

        public BrushColor(Vector2 iShadeValue, float iColorValue=0.5f, float iAlphaValue=1.0f)
        {
            colorValue = iColorValue;
            alphaValue = iAlphaValue;
            shadeValue = iShadeValue;
        }
    }
    
	void Start () {

	    colorTex = new Texture2D(textureResolution, 1, TextureFormat.RGBA32, true, true);
        colorTex.filterMode = FilterMode.Bilinear;
        colorTex.wrapMode = TextureWrapMode.Clamp;
	    shadeTex = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, true, true);
        shadeTex.filterMode = FilterMode.Bilinear;
        shadeTex.wrapMode = TextureWrapMode.Clamp;
        alphaTex = new Texture2D(1, textureResolution, TextureFormat.RGBA32, true, true);
        alphaTex.filterMode = FilterMode.Bilinear;
        alphaTex.wrapMode = TextureWrapMode.Clamp;
	    
        colorTexColor = new Color[textureResolution];
        shadeTexColor = new Color[textureResolution*textureResolution];
        alphaTexColor = new Color[textureResolution];
        
        baseColor = new Color[7];
        baseColor[0] = new Color(1,0,0);
        baseColor[1] = new Color(1,1,0);
        baseColor[2] = new Color(0,1,0);
        baseColor[3] = new Color(0,1,1);
        baseColor[4] = new Color(0,0,1);
        baseColor[5] = new Color(1,0,1);
        baseColor[6] = new Color(1,0,0);

        int numPixelPerColor = textureResolution / 6;

        int index = 0;
        for(int i=0; i < 6; i++)
        {
            for(int j=0; j < numPixelPerColor; j++)
            {
                colorTexColor[index++] = Color.Lerp(baseColor[i], baseColor[i+1], j / (float) numPixelPerColor);
            }
        }

        colorTex.SetPixels(colorTexColor);
        colorTex.Apply();
        colorPickerImage.texture = colorTex;

        numColors = colorImage.Length;
        brushColor = new BrushColor[numColors];
        for(int i=0; i < numColors; i++)
        {
            brushColor[i] = new BrushColor(Vector2.one, 0.0f, i==0 ? 1.0f : 0);
            brushColor[i].colorTex = new Texture2D(8, 8, TextureFormat.RGBAFloat, true, true);
            brushColor[i].colorTex.filterMode = FilterMode.Point;
            brushColor[i].colorTex.wrapMode = TextureWrapMode.Clamp;
            brushColor[i].colorTexColor = new Color[8*8];
            selectColor(i);
        }
        selectColor(0);
        brushSizePickerImage.GetComponent<Picker1D>().value = brushSize;
        updateAlphaTexture();

    }
    void updateSelectedColor()
    {
        int colorIndex=(int)(brushColor[selectedColorIndex].colorValue*6.0f);
        float t = brushColor[selectedColorIndex].colorValue*6.0f - colorIndex;
        brushColor[selectedColorIndex].selectedColor = Color.Lerp(baseColor[colorIndex],baseColor[Mathf.Min(6,colorIndex+1)],t);
    }

    void updateSelectedShade()
    {
        brushColor[selectedColorIndex].selectedShade = Color.Lerp(Color.Lerp(brushColor[selectedColorIndex].selectedColor, Color.white, 1.0f - brushColor[selectedColorIndex].shadeValue.x),Color.black, 1.0f - brushColor[selectedColorIndex].shadeValue.y);
    }
    
    void updateColorTexture()
    {
        int index=0;
        for(int i=0; i < 8; i++)
        {
            for(int j=0; j < 8; j++)
            {
                index = i*8 +j;
                brushColor[selectedColorIndex].colorTexColor[index] = brushColor[selectedColorIndex].selectedShade*brushColor[selectedColorIndex].alphaValue + ((i+j) % 2 > 0 ? Color.white*(1.0f-brushColor[selectedColorIndex].alphaValue) : new Color(0.8f,0.8f,0.8f,1.0f)*(1.0f-brushColor[selectedColorIndex].alphaValue));
            }
        }
        brushColor[selectedColorIndex].colorTex.SetPixels(brushColor[selectedColorIndex].colorTexColor);
        brushColor[selectedColorIndex].colorTex.Apply();
        colorImage[selectedColorIndex].texture = brushColor[selectedColorIndex].colorTex;
    }

    void updateShadeTexture()
    {
        int index=0;
        for(int i=0; i < textureResolution; i++)
        {
            Color xColor = Color.Lerp(brushColor[selectedColorIndex].selectedColor, Color.white, 1.0f - i / (float) textureResolution);
            for(int j=0; j < textureResolution; j++)
            {
                index = i + j*textureResolution;
                shadeTexColor[index] = Color.Lerp(xColor, Color.black, 1.0f - j / (float) textureResolution);
            }
        }
        shadeTex.SetPixels(shadeTexColor);
        shadeTex.Apply();
        shadePickerImage.texture = shadeTex;
    }

    void updateAlphaTexture()
    {
        int index=0;
        for(int i=0; i < textureResolution; i++)
        {
            alphaTexColor[index++] = Color.Lerp(Color.white, Color.black, 1.0f - i / (float) textureResolution);
            //alphaTexColor[index++] = Color.Lerp(selectedColor, Color.clear, 1.0f - i / (float) textureResolution);
        }
        alphaTex.SetPixels(alphaTexColor);
        alphaTex.Apply();
        alphaPickerImage.texture = alphaTex;
    }

    public void setColorValue(float value)
    {
        brushColor[selectedColorIndex].colorValue = value;
        updateSelectedColor();
        updateSelectedShade();
        updateShadeTexture();
        updateColorTexture();
    }

    public void setShadeValue(Vector2 value)
    {
        brushColor[selectedColorIndex].shadeValue = value;
        updateSelectedShade();
        updateColorTexture();
    }
    
    public void setAlphaValue(float value)
    {
        brushColor[selectedColorIndex].alphaValue = value;
        updateColorTexture();
    }
    
    public void setBrushSizeValue(float value)
    {
        brushSize = (int)value;
        transferFunctionImage.GetComponent<Picker2D>().handleRect.sizeDelta = new Vector2(brushSize*2, brushSize*2);
        transferFunctionImage.GetComponent<Picker2D>().handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        transferFunctionImage.GetComponent<Picker2D>().handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        transferFunctionImage.GetComponent<Picker2D>().handleRect.anchoredPosition = new Vector2(0, 0);
        transferFunctionImage.GetComponent<Picker2D>().handleRect.gameObject.SetActive(true);
    }

    public void selectColor(int value)
    {
        selectedColorIndex = value;
        updateSelectedColor();
        updateSelectedShade();
        updateShadeTexture();
        updateColorTexture();
        colorPickerImage.GetComponent<Picker1D>().normalizedValue = brushColor[selectedColorIndex].colorValue;
        shadePickerImage.GetComponent<Picker2D>().normalizedValue = brushColor[selectedColorIndex].shadeValue;
        alphaPickerImage.GetComponent<Picker1D>().normalizedValue = brushColor[selectedColorIndex].alphaValue;
    }

    public void paintToTexture(Vector2 value)
    {
        if(transferFunctionTex==null)
        {
            Debug.Log("transferFunctionTex is null");
            return;
        }

        if(paintColorIndex != 1 && Input.GetMouseButtonDown(1))
        {
            paintColorIndex=1;
        }
        if(paintColorIndex != 0 && Input.GetMouseButtonDown(0))
        {
            paintColorIndex=0;
        }
        int xVal = (int) (value.x * transferFunctionTex.width);
        int yVal = (int) (value.y * transferFunctionTex.height);
        int xDim = transferFunctionTex.width;
        int yDim = transferFunctionTex.height;
        for(int i=Mathf.Max(0,xVal - brushSize); i < Mathf.Min(xDim, xVal +  brushSize); i++)
        {
            for(int j=Mathf.Max(0,yVal - brushSize); j < Mathf.Min(yDim, yVal +  brushSize); j++)
            {
                if(new Vector2(xVal - i, yVal - j).magnitude > brushSize)
                {
                    continue;
                }
                int index = i + j * transferFunctionTex.width;
                transferFunctionColor[index] = brushColor[paintColorIndex].selectedShade;
                transferFunctionColor[index].a = brushColor[paintColorIndex].alphaValue;
            }
        }
        
        transferFunctionTex.SetPixels(transferFunctionColor);
        transferFunctionTex.filterMode = FilterMode.Point;
        transferFunctionTex.Apply();
        transferFunctionImage.texture = transferFunctionTex;
        volumeComponent.tfData = transferFunctionTex;
    }

    public void UpdateTransferFunctionTex()
    {
        transferFunctionTex = new Texture2D(volumeComponent.tfData.width, volumeComponent.tfData.height, TextureFormat.ARGB32, true);
        transferFunctionTex.SetPixels(volumeComponent.tfData.GetPixels());
        transferFunctionTex.wrapMode = TextureWrapMode.Clamp;
        transferFunctionTex.Apply();
        volumeComponent.tfData = transferFunctionTex;
        transferFunctionColor = transferFunctionTex.GetPixels();
        transferFunctionImage.texture = transferFunctionTex;
    }

#if UNITY_EDITOR
    void Update() {
        timeSinceLastSave += Time.deltaTime;

        if(transferFunctionTex==null)
        {
            return;
        }
        if (timeSinceLastSave >= autoSaveTexAfterSec)
        {
            timeSinceLastSave = 0;
		    System.IO.File.WriteAllBytes("Assets/VolumeViewerPro/examples/textures/transfer.png", transferFunctionTex.EncodeToPNG());
        }
    }
#endif

}
