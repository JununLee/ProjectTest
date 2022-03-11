////--------------------------------------------------------------------
/// Namespace:          
/// Class:              ProgressBar
/// Description:        Modification of UnityEngine.UI.Slider. No
///                         graphical user input allowed.
/// Authors:            Unity Technologies  (developed UnityEngine.UI.Slider)
///                     LISCINTEC
///                         http://www.liscintec.com
///                         info@liscintec.com
/// Date:               Feb 2017
/// Notes:              -
/// Revision History:   First release
/// 
/// UnityEngine.UI.Slider (Unity 5.3) from:
/// https://bitbucket.org/Unity-Technologies/ui/src/
/// 
/// This file is part of the examples of the Volume Viewer Pro Package.
/// Volume Viewer Pro is a Unity Asset Store product.
/// https://www.assetstore.unity3d.com/#!/content/83185
////--------------------------------------------------------------------

using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(RectTransform))]
public class ProgressBar : MonoBehaviour
{
    public enum Direction
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
    }

    [Serializable]
    public class SliderEventFloat : UnityEvent<float> { }
    [Serializable]
    public class SliderEventInt : UnityEvent<int> { }

    [SerializeField]
    private RectTransform m_FillRect;
    public RectTransform fillRect { get { return m_FillRect; } set { UpdateVisuals(); } }

    [SerializeField]
    private Direction m_Direction = Direction.LeftToRight;
    public Direction direction { get { return m_Direction; } set { UpdateVisuals(); } }

    [SerializeField]
    private float m_MinValue = 0;
    public float minValue { get { return m_MinValue; } set { Set(m_Value); UpdateVisuals(); } }

    [SerializeField]
    private float m_MaxValue = 1;
    public float maxValue { get { return m_MaxValue; } set { Set(m_Value); UpdateVisuals(); } }

    public float[] snapTo;
    public float[] snapAffinity;

    [SerializeField]
    private bool m_WholeNumbers = false;
    public bool wholeNumbers { get { return m_WholeNumbers; } set { Set(m_Value); UpdateVisuals(); } }

    [SerializeField]
    protected float m_Value;
    public virtual float value
    {
        get
        {
            if (wholeNumbers)
                return Mathf.Round(m_Value);
            return m_Value;
        }
        set
        {
            Set(value);
        }
    }

    public float normalizedValue
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, value);
        }
        set
        {
            this.value = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    [Space(10)]

    // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    [SerializeField]
    private SliderEventFloat m_OnValueChanged = new SliderEventFloat();
    public SliderEventFloat onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

    // Private fields
    private Transform m_FillTransform;

    private DrivenRectTransformTracker m_Tracker;

    // Size of each step.
    float stepSize { get { return wholeNumbers ? 1 : (maxValue - minValue) * 0.1f; } }

    protected ProgressBar()
    { }

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
        if (executing == CanvasUpdate.Prelayout)
            onValueChanged.Invoke(value);
#endif
    }

    public virtual void LayoutComplete()
    { }

    public virtual void GraphicUpdateComplete()
    { }

    float ClampValue(float input)
    {
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);
        return newValue;
    }

    void OnValidate()
    {
        UpdateVisuals();
    }

    // Set the valueUpdate the visible Image.
    void Set(float input)
    {
        Set(input, true);
    }

    protected virtual void Set(float input, bool sendCallback)
    {
        // Clamp the input.
        float newValue = ClampValue(input);

        // Snap to value.
        int snapCount = Mathf.Min(snapTo.Length, snapAffinity.Length);
        for (int i = 0; i < snapCount; i++)
        {
            if (Mathf.Abs(newValue - snapTo[i]) < (maxValue - minValue) * snapAffinity[i])
            {
                newValue = snapTo[i];
                break;
            }
        }
        // If the stepped value doesn't match the last one, it's time to update.
        if (m_Value == newValue)
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

    enum Axis
    {
        Horizontal = 0,
        Vertical = 1
    }

    Axis axis { get { return (m_Direction == Direction.LeftToRight || m_Direction == Direction.RightToLeft) ? Axis.Horizontal : Axis.Vertical; } }
    bool reverseValue { get { return m_Direction == Direction.RightToLeft || m_Direction == Direction.TopToBottom; } }

    // Force-update the slider. Useful if you've changed the properties and want it to update visually.
    private void UpdateVisuals()
    {

        m_Tracker.Clear();

        m_Tracker.Add(this, m_FillRect, DrivenTransformProperties.Anchors);
        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.one;

        if (reverseValue)
            anchorMin[(int)axis] = 1 - normalizedValue;
        else
            anchorMax[(int)axis] = normalizedValue;

        m_FillRect.anchorMin = anchorMin;
        m_FillRect.anchorMax = anchorMax;
    }

    public void SetDirection(Direction direction, bool includeRectLayouts)
    {
        Axis oldAxis = axis;
        bool oldReverse = reverseValue;
        this.direction = direction;

        if (!includeRectLayouts)
            return;

        if (axis != oldAxis)
            RectTransformUtility.FlipLayoutAxes(transform as RectTransform, true, true);

        if (reverseValue != oldReverse)
            RectTransformUtility.FlipLayoutOnAxis(transform as RectTransform, (int)axis, true, true);
    }
}