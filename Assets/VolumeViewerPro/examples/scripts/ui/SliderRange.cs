////--------------------------------------------------------------------
/// Namespace:          
/// Class:              SliderRange
/// Description:        Modification of UnityEngine.UI.Slider. Allows
///                         to change and drag a range (two values on 
///                         the same scale)
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
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class SliderRange : Selectable, IDragHandler, IInitializePotentialDragHandler, ICanvasElement
{
    public enum Direction
    {
        LeftToRight,
        RightToLeft,
        BottomToTop,
        TopToBottom,
    }

    public enum Handle
    {
        None,
        Min,
        Max,
        Drag,
    }

    [Serializable]
    public class SliderEvent : UnityEvent<float> {}
    [Serializable]
    public class SliderMaxEvent : UnityEvent<float> {}

    [SerializeField]
    private RectTransform m_FillRect;
    public RectTransform fillRect { get { return m_FillRect; } set { UpdateCachedReferences(); UpdateVisuals(); } }
        
    [SerializeField]
    private RectTransform m_HandleRect;
    public RectTransform handleRect { get { return m_HandleRect; } set {  UpdateCachedReferences(); UpdateVisuals();  } }
    [SerializeField]
    private RectTransform m_HandleMaxRect;
    public RectTransform handleMaxRect { get { return m_HandleMaxRect; } set {  UpdateCachedReferences(); UpdateVisuals();  } }

    [Space(10)]

    [SerializeField]
    private Direction m_Direction = Direction.LeftToRight;
    public Direction direction { get { return m_Direction; } set {  UpdateVisuals(); } }

    [SerializeField]
    private float m_MinValue = 0;
    public float minValue { get { return m_MinValue; } set { Set(m_Value); SetMax(m_ValueMax); UpdateVisuals();  } }

    [SerializeField]
    private float m_MaxValue = 1;
    public float maxValue { get { return m_MaxValue; } set {  Set(m_Value); SetMax(m_ValueMax); UpdateVisuals();  } }

    [SerializeField]
    private bool m_WholeNumbers = false;
    public bool wholeNumbers { get { return m_WholeNumbers; } set {  Set(m_Value); SetMax(m_ValueMax); UpdateVisuals();  } }

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
        
    [SerializeField]
    protected float m_ValueMax;
    public virtual float valueMax
    {
        get
        {
            if (wholeNumbers)
                return Mathf.Round(m_ValueMax);
            return m_ValueMax;
        }
        set
        {
            SetMax(value);
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

    public float normalizedValueMax
    {
        get
        {
            if (Mathf.Approximately(minValue, maxValue))
                return 0;
            return Mathf.InverseLerp(minValue, maxValue, valueMax);
        }
        set
        {
            this.valueMax = Mathf.Lerp(minValue, maxValue, value);
        }
    }

    [Space(10)]
        
    // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    [SerializeField]
    private SliderEvent m_OnValueChanged = new SliderEvent();
    public SliderEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }
    // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    [SerializeField]
    private SliderMaxEvent m_OnValueMaxChanged = new SliderMaxEvent();
    public SliderMaxEvent onValueMaxChanged { get { return m_OnValueMaxChanged; } set { m_OnValueMaxChanged = value; } }

    // Private fields

    //private Image m_FillImage;
    private Transform m_FillTransform;
    private RectTransform m_FillContainerRect;
    private Transform m_HandleTransform;
    private RectTransform m_HandleContainerRect;

    private Handle m_SelectedHandle;

    // The offset from handle position to mouse down position
    private Vector2 m_Offset = Vector2.zero;

    private DrivenRectTransformTracker m_Tracker;

    // Size of each step.
    float stepSize { get { return wholeNumbers ? 1 : (maxValue - minValue) * 0.1f; } }

    protected SliderRange()
    {}

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (wholeNumbers)
        {
            m_MinValue = Mathf.Round(m_MinValue);
            m_MaxValue = Mathf.Round(m_MaxValue);
        }

        //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
        if (IsActive())
        {
            UpdateCachedReferences();
            Set(m_Value, false);
            SetMax(m_ValueMax, false);
            // Update rects since other things might affect them even if value didn't change.
            UpdateVisuals();
        }

        var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
        if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }

#endif // if UNITY_EDITOR

    public virtual void Rebuild(CanvasUpdate executing)
    {
#if UNITY_EDITOR
        if (executing == CanvasUpdate.Prelayout)
            onValueChanged.Invoke(value);
#endif
    }

    public virtual void LayoutComplete()
    {}

    public virtual void GraphicUpdateComplete()
    {}

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateCachedReferences();
        Set(m_Value, false);
        SetMax(m_ValueMax, false);
        // Update rects since they need to be initialized correctly.
        UpdateVisuals();
    }

    protected override void OnDisable()
    {
        m_Tracker.Clear();
        base.OnDisable();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        // Has value changed? Various elements of the slider have the old normalisedValue assigned, we can use this to perform a comparison.
        // We also need to ensure the value stays within min/max.
        m_Value = ClampValue(m_Value);
        m_ValueMax = ClampValue(m_ValueMax);
        float oldNormalizedValue = normalizedValue;
        float oldNormalizedValueMax = normalizedValueMax;
        //if (m_FillContainerRect != null)
        //{
        //    if (m_FillImage != null && m_FillImage.type == Image.Type.Filled)
        //        oldNormalizedValue = m_FillImage.fillAmount;
        //    else
        //        oldNormalizedValue = (reverseValue ? 1 - m_FillRect.anchorMin[(int)axis] : m_FillRect.anchorMax[(int)axis]);
        //}else 
        if (m_HandleContainerRect != null)
        {
            oldNormalizedValue = (reverseValue ? 1 - m_HandleRect.anchorMin[(int)axis] : m_HandleRect.anchorMin[(int)axis]);
            oldNormalizedValueMax = (reverseValue ? 1 - m_HandleMaxRect.anchorMin[(int)axis] : m_HandleMaxRect.anchorMin[(int)axis]);
        }
        UpdateVisuals();

        if (oldNormalizedValue != normalizedValue)
        {
            onValueChanged.Invoke(m_Value);
        }
        if (oldNormalizedValueMax != normalizedValueMax)
        {
            onValueMaxChanged.Invoke(m_ValueMax);
        }
    }

    void UpdateCachedReferences()
    {
        if (m_FillRect)
        {
            m_FillTransform = m_FillRect.transform;
            //m_FillImage = m_FillRect.GetComponent<Image>();
            if (m_FillTransform.parent != null)
                m_FillContainerRect = m_FillTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            m_FillContainerRect = null;
            //m_FillImage = null;
        }

        if (m_HandleRect)
        {
            m_HandleTransform = m_HandleRect.transform;
            if (m_HandleTransform.parent != null)
                m_HandleContainerRect = m_HandleTransform.parent.GetComponent<RectTransform>();
        }
        else
        {
            m_HandleContainerRect = null;
        }
    }

    float ClampValue(float input)
    {
        float newValue = Mathf.Clamp(input, minValue, maxValue);
        if (wholeNumbers)
            newValue = Mathf.Round(newValue);
        return newValue;
    }

    // Set the value. Update the visible Image.
    void Set(float input)
    {
        Set(input, true);
    }
    // Set the value. Update the visible Image.
    void SetMax(float input)
    {
        SetMax(input, true);
    }
        
    protected virtual void Set(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = ClampValue(input);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_Value == newValue)
            return;

        m_Value = newValue;
        UpdateVisuals();
        if (sendCallback)
            m_OnValueChanged.Invoke(newValue);
    }
    protected virtual void SetMax(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = ClampValue(input);

        // If the stepped value doesn't match the last one, it's time to update
        if (m_ValueMax == newValue)
            return;

        m_ValueMax = newValue;
        UpdateVisuals();
        if (sendCallback)
            m_OnValueMaxChanged.Invoke(newValue);
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();

        //This can be invoked before OnEnabled is called. So we shouldn't be accessing other objects, before OnEnable is called.
        if (!IsActive())
            return;

        UpdateVisuals();
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
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateCachedReferences();
#endif

        m_Tracker.Clear();

        if (m_FillContainerRect != null)
        {
            m_Tracker.Add(this, m_FillRect, DrivenTransformProperties.Anchors);
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            //if (m_FillImage != null && m_FillImage.type == Image.Type.Filled)
            //{
            //    m_FillImage.fillAmount = normalizedValue;
            //}
            //else
            {
                if (reverseValue)
                {
                    anchorMin[(int)axis] = 1 - normalizedValue;
                    anchorMax[(int)axis] = 1 - normalizedValueMax;
                }
                else
                {
                    anchorMin[(int)axis] = normalizedValue;
                    anchorMax[(int)axis] = normalizedValueMax;
                }
            }

            m_FillRect.anchorMin = anchorMin;
            m_FillRect.anchorMax = anchorMax;
        }

        if (m_HandleContainerRect != null)
        {
            m_Tracker.Add(this, m_HandleRect, DrivenTransformProperties.Anchors);
            m_Tracker.Add(this, m_HandleMaxRect, DrivenTransformProperties.Anchors);
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.one;

            anchorMin[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);
            anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValue) : normalizedValue);
            m_HandleRect.anchorMin = anchorMin;
            m_HandleRect.anchorMax = anchorMax;

            anchorMin[(int)axis] = (reverseValue ? (1 - normalizedValueMax) : normalizedValueMax);
            anchorMax[(int)axis] = (reverseValue ? (1 - normalizedValueMax) : normalizedValueMax);
            m_HandleMaxRect.anchorMin = anchorMin;
            m_HandleMaxRect.anchorMax = anchorMax;
        }
    }

    // Update the slider's position based on the mouse.
    void UpdateDrag(PointerEventData eventData, Camera cam)
    {
        RectTransform clickRect = m_HandleContainerRect;// ?? m_FillContainerRect;
        if (clickRect != null && clickRect.rect.size[(int)axis] > 0)
        {
            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out localCursor))
            {
                return;
            }
            localCursor -= clickRect.rect.position;

            float val = Mathf.Clamp01((localCursor - m_Offset)[(int)axis] / clickRect.rect.size[(int)axis]);
            val = (reverseValue ? 1f - val : val);
            float middle;
            float normalizedValueDrag;
            switch(m_SelectedHandle)
            {
                case Handle.Min:
                    normalizedValue = val > normalizedValueMax ? normalizedValueMax : val;
                    normalizedValueDrag = normalizedValue + (normalizedValueMax - normalizedValue)/2.0f;
                    break;
                case Handle.Max:
                    normalizedValueMax = val < normalizedValue ? normalizedValue : val;
                    normalizedValueDrag = normalizedValue + (normalizedValueMax - normalizedValue)/2.0f;
                    break;
                case Handle.Drag:
                    middle = (normalizedValueMax - normalizedValue)/2.0f;
                    normalizedValueDrag = val - middle < minValue ? minValue + middle : val;
                    normalizedValueDrag = normalizedValueDrag + middle > maxValue ? maxValue - middle : normalizedValueDrag;
                    normalizedValue = normalizedValueDrag - middle;
                    normalizedValueMax = normalizedValueDrag + middle;
                    break;
                case Handle.None:
                    if(val < normalizedValue)
                    {
                        m_SelectedHandle = Handle.Min;
                        targetGraphic = m_HandleRect.GetComponent<Image>();
                        normalizedValue = val;
                        normalizedValueDrag = normalizedValue + (normalizedValueMax - normalizedValue)/2.0f;
                    }else if(val > normalizedValueMax)
                    {
                        m_SelectedHandle = Handle.Max;
                        targetGraphic = m_HandleMaxRect.GetComponent<Image>();
                        normalizedValueMax = val;
                        normalizedValueDrag = normalizedValue + (normalizedValueMax - normalizedValue)/2.0f;
                    }else
                    {
                        m_SelectedHandle = Handle.Drag;
                        targetGraphic = m_FillRect.GetComponent<Image>();
                        Vector2 localMousePos;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_FillRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                        {
                            m_Offset = localMousePos;
                        }
                        //middle = (normalizedValueMax - normalizedValue)/2.0f;
                        //normalizedValueDrag = val - middle < minValue ? minValue + middle : val;
                        //normalizedValueDrag = normalizedValueDrag + middle > maxValue ? maxValue - middle : normalizedValueDrag;
                        //normalizedValue = normalizedValueDrag - middle;
                        //normalizedValueMax = normalizedValueDrag + middle;
                    }
                    break;
            }
        }
    }

    private bool MayDrag(PointerEventData eventData)
    {
        return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;

        m_SelectedHandle = Handle.None;
        m_Offset = Vector2.zero;
        if (m_HandleContainerRect != null)
        {
            if(RectTransformUtility.RectangleContainsScreenPoint(m_HandleRect, eventData.position, eventData.enterEventCamera))
            {
                m_SelectedHandle = Handle.Min;
                targetGraphic = m_HandleRect.GetComponent<Image>();
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                {
                    m_Offset = localMousePos;
                }
            }else if(RectTransformUtility.RectangleContainsScreenPoint(m_HandleMaxRect, eventData.position, eventData.enterEventCamera))
            {
                m_SelectedHandle = Handle.Max;
                targetGraphic = m_HandleMaxRect.GetComponent<Image>();
                Vector2 localMousePos;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(m_HandleMaxRect, eventData.position, eventData.pressEventCamera, out localMousePos))
                {
                    m_Offset = localMousePos;
                }
            }else
            {   
                // Outside the slider handle - jump to this point instead.
                UpdateDrag(eventData, eventData.pressEventCamera);
            }
        }else
        {
            // Outside the slider handle - jump to this point instead.
            UpdateDrag(eventData, eventData.pressEventCamera);
        }

        base.OnPointerDown(eventData);
            
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (!MayDrag(eventData))
            return;
        UpdateDrag(eventData, eventData.pressEventCamera);
    }


    //OnMove still needs to be updated for range.
    public override void OnMove(AxisEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
        {
            base.OnMove(eventData);
            return;
        }

        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                if (axis == Axis.Horizontal && FindSelectableOnLeft() == null)
                    Set(reverseValue ? value + stepSize : value - stepSize);
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Right:
                if (axis == Axis.Horizontal && FindSelectableOnRight() == null)
                    Set(reverseValue ? value - stepSize : value + stepSize);
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Up:
                if (axis == Axis.Vertical && FindSelectableOnUp() == null)
                    Set(reverseValue ? value - stepSize : value + stepSize);
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Down:
                if (axis == Axis.Vertical && FindSelectableOnDown() == null)
                    Set(reverseValue ? value + stepSize : value - stepSize);
                else
                    base.OnMove(eventData);
                break;
        }
    }

    public override Selectable FindSelectableOnLeft()
    {
        if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
            return null;
        return base.FindSelectableOnLeft();
    }

    public override Selectable FindSelectableOnRight()
    {
        if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Horizontal)
            return null;
        return base.FindSelectableOnRight();
    }

    public override Selectable FindSelectableOnUp()
    {
        if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
            return null;
        return base.FindSelectableOnUp();
    }

    public override Selectable FindSelectableOnDown()
    {
        if (navigation.mode == Navigation.Mode.Automatic && axis == Axis.Vertical)
            return null;
        return base.FindSelectableOnDown();
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
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
