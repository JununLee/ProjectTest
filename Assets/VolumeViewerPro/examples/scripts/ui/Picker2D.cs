////--------------------------------------------------------------------
/// Namespace:          
/// Class:              Picker2D
/// Description:        Simple Vector2 picker based on
///                         UnityEngine.UI.Slider.
///                         -Allows to pick 2 values at the same time
///                             (position on a 2D plane).
///                         -Allows to invoke event with value or delta
///                             value (change since last invoke).
///                         -Allows to set multiple values to snap to
///                             and how strong they should snap.
///                         -Allows to invoked an event, even if the
///                             new value is equal to the old value.
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

using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class Picker2D : MonoBehaviour, IPointerDownHandler, IDragHandler {

    [Serializable]
    public class PickerEvent : UnityEvent<Vector2> {}

    [SerializeField]
    private PickerEvent m_OnValueChanged = new PickerEvent();
    public PickerEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    [SerializeField]
    private Vector2 m_MinValue = Vector2.zero;
    public Vector2 minValue { get { return m_MinValue; } set {  Set(m_Value); UpdateVisuals();  } }

    [SerializeField]
    private Vector2 m_MaxValue = Vector2.one;
    public Vector2 maxValue { get { return m_MaxValue; } set {  Set(m_Value); UpdateVisuals();  } }

    [SerializeField]
    private RectTransform m_HandleRect;
    public RectTransform handleRect { get { return m_HandleRect; } set {  UpdateVisuals();  } }

    [SerializeField]
    private bool delta;
    private Vector2 localMousePos;

    [SerializeField]
    private bool alwaysInvoke = true;

    public Vector2[] snapTo;
    public Vector2[] snapAffinity;

    private RectTransform imageTransform;
    private DrivenRectTransformTracker m_Tracker;

	void Awake () {
	    imageTransform = transform as RectTransform;
        if(m_HandleRect != null)
        {
            m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
        }
	}

    [SerializeField]
    protected Vector2 m_Value;
    public virtual Vector2 value
    {
        get
        {
            return m_Value;
        }
        set
        {
            Set(value);
        }
    }

    Vector2 ClampValue(Vector2 input)
    {
        input.x = Mathf.Clamp(input.x, minValue.x, maxValue.x);
        input.y = Mathf.Clamp(input.y, minValue.y, maxValue.y);
        return input;
    }
    void Set(Vector2 input)
    {
        Set(input, true);
    }
    protected virtual void Set(Vector2 input, bool sendCallback)
    {
        // Clamp the input
        Vector2 newValue = ClampValue(input);

        //Snap to values
        int snapCount = Mathf.Min(snapTo.Length, snapAffinity.Length);
        for(int i=0; i < snapCount; i++)
        {
            if((newValue-snapTo[i]).magnitude < (Vector2.Scale((maxValue-minValue),snapAffinity[i])).magnitude)
            {
                newValue =  snapTo[i];
                break;
            }
        }

        // If the stepped value doesn't match the last one, it's time to update
        if (!alwaysInvoke && (m_Value-newValue).magnitude <= Mathf.Epsilon)
        {
            return;
        }

        m_Value = newValue;
        UpdateVisuals();
        if (sendCallback)
        {
            m_OnValueChanged.Invoke(newValue);
        }
    }
    public Vector2 normalizedValue
    {
        get
        {
            Vector2 tempVec = Vector2.zero;
            tempVec.x = Mathf.InverseLerp(minValue.x, maxValue.x, value.x);
            tempVec.y = Mathf.InverseLerp(minValue.y, maxValue.y, value.y); 
            return tempVec;
        }
        set
        {
            Vector2 tempVec = Vector2.zero;
            tempVec.x = Mathf.Lerp(minValue.x, maxValue.x, value.x);
            tempVec.y = Mathf.Lerp(minValue.y, maxValue.y, value.y); 
            this.value = tempVec;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(delta) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(imageTransform, eventData.position, eventData.pressEventCamera, out localMousePos);
            localMousePos.x = Mathf.Clamp01(localMousePos.x / imageTransform.rect.size.x);
            localMousePos.y = Mathf.Clamp01(localMousePos.y / imageTransform.rect.size.y);
        }else {
            OnDrag(eventData);
        }
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if(imageTransform == null)
        {
            return;
        }
        Vector2 newMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(imageTransform, eventData.position, eventData.pressEventCamera, out newMousePos);
        newMousePos.x = Mathf.Clamp01(newMousePos.x / imageTransform.rect.size.x);
        newMousePos.y = Mathf.Clamp01(newMousePos.y / imageTransform.rect.size.y);
        if(delta) {
            value = newMousePos - localMousePos;
        }else {
            normalizedValue = newMousePos;
        }
        localMousePos = newMousePos;
        
    }

    private void UpdateVisuals()
    {

        if (imageTransform == null)
        {
            return;
        }
        
        Vector2 anchorMin = normalizedValue;
        Vector2 anchorMax = normalizedValue;

        if(m_HandleRect != null)
        {
            m_HandleRect.anchorMin = anchorMin;
            m_HandleRect.anchorMax = anchorMax;
        }
        
    }
}
