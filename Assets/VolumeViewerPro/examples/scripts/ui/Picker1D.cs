////--------------------------------------------------------------------
/// Namespace:          
/// Class:              Picker1D
/// Description:        Simple float picker based on
///                         UnityEngine.UI.Slider.
///                         -Allows to invoke its event with value or
///                             delta value (change since last invoke).
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
public class Picker1D : MonoBehaviour, IPointerDownHandler, IDragHandler {


    public enum Direction
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
    }

    [Serializable]
    public class PickerEvent : UnityEvent<float> {}

    [SerializeField]
    private PickerEvent m_OnValueChanged = new PickerEvent();
    public PickerEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    [SerializeField]
    private float m_MinValue = 0;
    public float minValue { get { return m_MinValue; } set {  Set(m_Value); UpdateVisuals();  } }

    [SerializeField]
    private float m_MaxValue = 1;
    public float maxValue { get { return m_MaxValue; } set {  Set(m_Value); UpdateVisuals();  } }

    [SerializeField]
    private RectTransform m_HandleRect;
    public RectTransform handleRect { get { return m_HandleRect; } set {  UpdateVisuals();  } }

    [SerializeField]
    private bool delta;
    private Vector2 localMousePos;

    [SerializeField]
    private bool alwaysInvoke = true;

    public float[] snapTo;
    public float[] snapAffinity;

    [SerializeField]
    private Direction m_Direction = Direction.LeftToRight;
    public Direction direction { get { return m_Direction; } set {  UpdateVisuals();  } }

    enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    Axis axis { get { return (m_Direction == Direction.LeftToRight || m_Direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
    bool reverseValue { get { return m_Direction == Direction.RightToLeft || m_Direction == Direction.TopToBottom; } }

    private RectTransform imageTransform;
    private DrivenRectTransformTracker m_Tracker;

	void Awake () {
	    imageTransform = transform as RectTransform;
        m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
	}
	
	[SerializeField]
    protected float m_Value;
    public virtual float value
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

    float ClampValue(float input)
    {
        input = Mathf.Clamp(input, minValue, maxValue);
        return input;
    }
    void Set(float input)
    {
        Set(input, true);
    }
    protected virtual void Set(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = ClampValue(input);

        // Snap to value
        int snapCount = Mathf.Min(snapTo.Length, snapAffinity.Length);
        for(int i=0; i < snapCount; i++)
        {
            if(Mathf.Abs(newValue-snapTo[i]) < (maxValue-minValue)*snapAffinity[i])
            {
                newValue =  snapTo[i];
                break;
            }
        }

        // If the stepped value doesn't match the last one, it's time to update
        if (!alwaysInvoke && m_Value == newValue)
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
    public float normalizedValue
    {
        get
        {
            return Mathf.InverseLerp(minValue, maxValue, value);
        }
        set
        {
            this.value = Mathf.Lerp(minValue, maxValue, value); 
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
            Vector2 deltaPos=newMousePos - localMousePos;
            value = (reverseValue ? 1f - deltaPos[(int)axis] : deltaPos[(int)axis]);
        }else {
            float val = (reverseValue ? 1f - newMousePos[(int)axis] : newMousePos[(int)axis]);
            normalizedValue = val;
        }
        localMousePos = newMousePos;
    }

    private void UpdateVisuals()
    {

        if (imageTransform == null)
        {
            return;
        }

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.one;

        anchorMin[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);
        anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);

        m_HandleRect.anchorMin = anchorMin;
        m_HandleRect.anchorMax = anchorMax;
        
    }
}
