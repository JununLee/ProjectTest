////--------------------------------------------------------------------
/// Namespace:          
/// Class:              Crosshairs
/// Description:        Updates the position of RectTransforms to form
///                         crosshairs.
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

public class Crosshairs : MonoBehaviour {
    
    public RectTransform leftTransform;
    public RectTransform rightTransform;
    public RectTransform downTransform;
    public RectTransform upTransform;

    float aspectRatio;
    Vector2 currentPos;

    void Start()
    {
        aspectRatio= (float) Screen.width / Screen.height;
        currentPos = new Vector2(0.5f, 0.5f);
    }
	public void Hide()
    {
        leftTransform.gameObject.SetActive(false);
        rightTransform.gameObject.SetActive(false);
        downTransform.gameObject.SetActive(false);
        upTransform.gameObject.SetActive(false);
    }
    public void Show()
    {
        leftTransform.gameObject.SetActive(true);
        rightTransform.gameObject.SetActive(true);
        downTransform.gameObject.SetActive(true);
        upTransform.gameObject.SetActive(true);
    }
    public void UpdateAspectRatio()
    {
        RectTransform parentView = transform.parent as RectTransform;
        Vector2 parentSize = parentView.anchorMax - parentView.anchorMin;
        aspectRatio = (float) Screen.width / Screen.height * Mathf.Abs(parentSize.x) / Mathf.Abs(parentSize.y);
        aspectRatio = float.IsInfinity(aspectRatio) || float.IsNaN(aspectRatio) || Mathf.Abs(aspectRatio) < Mathf.Epsilon ? 1 : aspectRatio;
        UpdateCrosshairs(currentPos);
    }

    public void UpdateCrosshairs(Vector2 newPos)
    {
        Vector2 minVector;
        Vector2 maxVector;

        minVector = leftTransform.anchorMin;
        minVector.y = newPos.y;
        leftTransform.anchorMin = minVector;

        maxVector = newPos;
        maxVector.x -= 0.02f / aspectRatio;
        leftTransform.anchorMax = maxVector;

        minVector = newPos;
        minVector.x += 0.02f / aspectRatio;
        rightTransform.anchorMin = minVector;

        maxVector = rightTransform.anchorMax;
        maxVector.y = newPos.y;
        rightTransform.anchorMax = maxVector;

        minVector = downTransform.anchorMin;
        minVector.x = newPos.x;
        downTransform.anchorMin = minVector;

        maxVector = newPos;
        maxVector.y -= 0.02f;
        downTransform.anchorMax = maxVector;

        minVector = newPos;
        minVector.y += 0.02f;
        upTransform.anchorMin = minVector;

        maxVector = upTransform.anchorMax;
        maxVector.x = newPos.x;
        upTransform.anchorMax = maxVector;

        currentPos = newPos;
    }

    public void UpdateH(float newVal)
    {
        Vector2 newPos = new Vector2(downTransform.anchorMin.x, newVal);
        UpdateCrosshairs(newPos);

    }

    public void UpdateV(float newVal)
    {
        Vector2 newPos = new Vector2(newVal, leftTransform.anchorMin.y);
        UpdateCrosshairs(newPos);
    }
}
