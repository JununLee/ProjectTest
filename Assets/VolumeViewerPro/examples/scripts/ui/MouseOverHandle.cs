////--------------------------------------------------------------------
/// Namespace:          
/// Class:              MouseOverHandle
/// Description:        Hides a Handle unless the cursor is inside of a
///                         certain RectTransform. If a mouse button is
///                         pushed down while inside the RectTransform,
///                         the Handle is kept visible until that button 
///                         is released, regardless of the cursor's 
///                         position.
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
using UnityEngine.EventSystems;

public class MouseOverHandle : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler {

    [SerializeField]
    public RectTransform m_HandleRect;

    private RectTransform imageTransform;
    private DrivenRectTransformTracker m_Tracker;
    private Vector3[] imageCorners;
    private bool insideImage = false;
    private bool buttonDown = false;

	// Initialization
	void Start () {
	    imageTransform = transform as RectTransform;
        m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
        imageCorners = new Vector3[4];
	}

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonDown = true;
        m_HandleRect.gameObject.SetActive(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonDown = false;
        if(insideImage == false)
        {
            m_HandleRect.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        insideImage = true;
        m_HandleRect.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        insideImage = false;
        if(buttonDown == false)
        {
            m_HandleRect.gameObject.SetActive(false);
        }
    }

    Vector2 ClampToImage (Vector2 position)
    {
        imageTransform.GetWorldCorners(imageCorners);

        float clampedX = Mathf.Clamp (position.x, imageCorners[0].x, imageCorners[2].x);
        float clampedY = Mathf.Clamp (position.y, imageCorners[0].y, imageCorners[2].y);

        Vector2 newPointerPosition = new Vector2 (clampedX, clampedY);
        return newPointerPosition;
    }

    void Update()
    {

        if (imageTransform == null || (insideImage == false && buttonDown == false))
        {
            return;
        }
        
        m_HandleRect.position = ClampToImage(Input.mousePosition);
        
    }
}
