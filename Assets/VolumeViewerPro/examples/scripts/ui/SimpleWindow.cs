////--------------------------------------------------------------------
/// Namespace:          
/// Class:              SimpleWindow
/// Description:        Simple window mechanic to drag something around
///                         on a title bar.
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

public class SimpleWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
    
    [SerializeField] private RectTransform dragRectTransform;
    [SerializeField] private GameObject windowContent;

    private RectTransform windowRectTransform;
    private RectTransform parentRectTransform;
    private Vector2 pointerOffset;
    private bool DownOnDrag;

    void Awake()
    {
        DownOnDrag = false;
        windowRectTransform = transform as RectTransform;
        parentRectTransform = windowRectTransform.parent as RectTransform;
    }

    public void OnPointerDown (PointerEventData data) {
        transform.SetAsLastSibling ();
        if (RectTransformUtility.RectangleContainsScreenPoint(dragRectTransform, data.position, data.enterEventCamera))
        {
            DownOnDrag = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle (windowRectTransform, data.position, data.pressEventCamera, out pointerOffset);
        }
    }

    public void OnPointerUp(PointerEventData data) {
        DownOnDrag = false;
    }

    public void OnDrag (PointerEventData data) {
        if(DownOnDrag)
        {
            Vector2 localPointerPosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle (parentRectTransform, ClampToParent(data.position), data.pressEventCamera, out localPointerPosition)) {
                windowRectTransform.localPosition = (localPointerPosition - pointerOffset);
            }
        }
    }

    public void DisplayContent(bool visible)
    {
        windowContent.SetActive(visible);
    }

    Vector2 ClampToParent (Vector2 pos) {

        Vector3[] parentCorners = new Vector3[4];
        parentRectTransform.GetWorldCorners (parentCorners);
        
        Vector2 newPos = Vector2.zero;
        newPos.x = Mathf.Clamp (pos.x, parentCorners[0].x, parentCorners[2].x);
        newPos.y = Mathf.Clamp (pos.y, parentCorners[0].y, parentCorners[2].y);

        return newPos;
    }
}
