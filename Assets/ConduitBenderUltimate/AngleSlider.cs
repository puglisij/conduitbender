using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Collections.Generic;

public class AngleSlider : Widget
{
    [Serializable]
    public class SnapButton
    {
        [SerializeField]
        public Button btn;
        [SerializeField]
        public float  snapValue;
    }
    public float value
    {
        get { return m_Value; }
        set { Set( value, true ); }
    }
    public Slider.SliderEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }

    public Text displayText;

    public RectTransform rotatingImage;

    public Graphic touchImage;

    public Color touchPressedColor;

    public Button minusButton;
    public Button plusButton;

    public List<SnapButton> snapButtons = new List<SnapButton>();

    public float rangeMin;
    public float rangeMax;
    public float startValue = 0f;
    public float stepValue = 0.5f;

    public bool wholeNumbers;

    //-------------------------------
    //      Private Data
    //-------------------------------
    [SerializeField]
    private Slider.SliderEvent m_OnValueChanged = new Slider.SliderEvent();

    private Color m_TouchNormalColor;

    private Vector3 m_StartRight;           // Right vector of rotator image on Start
    private Vector3 m_StartRotation;        // Euler Rotation of rotator image on Start
    private Vector3 m_StartUp;              // Up vector of rotator image on Start

    private Vector2 m_PivotScreenPosition;  // Position of pivot of rotator image in Screen Space

    private float m_MaxRotation;
    private float m_Value;

    void Awake()
    {
        m_MaxRotation = rangeMax - rangeMin;
        m_Value = startValue;

        // Setup Touch Image
        if (touchImage != null) {
            EventTrigger et = touchImage.gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry beginDragEntry = new EventTrigger.Entry();
            EventTrigger.Entry dragEntry = new EventTrigger.Entry();
            EventTrigger.Entry endDragEntry = new EventTrigger.Entry();

            beginDragEntry.eventID = EventTriggerType.BeginDrag;
            dragEntry.eventID = EventTriggerType.Drag;
            endDragEntry.eventID = EventTriggerType.EndDrag;

            beginDragEntry.callback.AddListener( RotatorBeginDrag );
            dragEntry.callback.AddListener( RotatorDrag );
            endDragEntry.callback.AddListener( RotatorEndDrag );

            et.triggers.Add( beginDragEntry );
            et.triggers.Add( dragEntry );
            et.triggers.Add( endDragEntry );
        }
        // Register callbacks on Buttons
        if (minusButton != null) {
            minusButton.onClick.AddListener( () => { Set( m_Value - stepValue, true ); } );
        }
        if (plusButton != null) {
            plusButton.onClick.AddListener( () => { Set( m_Value + stepValue, true ); } );
        }
        // Snap Buttons
        if(snapButtons.Count > 0) {
            for(int i = 0; i < snapButtons.Count; ++i) {
                if(snapButtons[ i ].btn != null) {
                    float sv = snapButtons[ i ].snapValue;
                    snapButtons[ i ].btn.onClick.AddListener( () => {
                        Set( sv, true );
                    } );
                }
            }
        }
    }

    void Start()
    {
        // @TODO - Update these Cached values when Parent RectTransform changes
        Camera parentCanvasCam = GetComponentInParent<Canvas>().worldCamera;
        m_PivotScreenPosition = parentCanvasCam.WorldToScreenPoint( 
            rotatingImage.TransformPoint( Rect.NormalizedToPoint(rotatingImage.rect, rotatingImage.pivot) )
            );
        m_StartRight = rotatingImage.right;
        m_StartRotation = rotatingImage.localRotation.eulerAngles;
        m_StartUp = rotatingImage.up;
        m_TouchNormalColor = touchImage.color;

        //Debug.Log( "AngleSlider: Start()" );
    }

    public void SetRange(float min, float max)
    {
        rangeMin = (min < 0f) ? 0f : min;
        rangeMax = Mathf.Clamp( max, startValue, 90f );
    }
    /*##################################

            Private Functions

    ###################################*/
    private float GetRoundedAngleByPoint(Vector2 screenPoint)
    {
        Vector2 toDragCurrent = screenPoint - m_PivotScreenPosition;
        float   rotatedAngle;
        if (Vector3.Angle( m_StartRight, toDragCurrent ) < 90f) {
            rotatedAngle = 0f;
        } else {
            rotatedAngle = Mathf.Clamp( Vector3.Angle( m_StartUp, toDragCurrent ), 0f, m_MaxRotation );
            rotatedAngle = Mathf.Round( rotatedAngle * 2f ) / 2f;
        }
        return rotatedAngle;
    }
    private void RotatorBeginDrag(BaseEventData eventData)
    {
        touchImage.color = touchPressedColor;
        //Debug.Log( "AngleSlider: RotatorBeginDrag()" );
    }
    private void RotatorDrag(BaseEventData eventData)
    {
        // Get Angle Between from Pivot
        PointerEventData e = null;
        try {
            e = (PointerEventData)eventData;
        } catch (Exception ex) {
            Debug.LogError( "AngleSlider: RotatorDrag() Exception: " + ex.ToString() );
        }

        Set( rangeMin + GetRoundedAngleByPoint( e.position ), false );

        //Debug.Log( "AngleSlider: RotatorDrag()" );
    }
    private void RotatorEndDrag(BaseEventData eventData)
    {
        touchImage.color = m_TouchNormalColor;

        // Send Callback
        m_OnValueChanged.Invoke( m_Value );
        //Debug.Log( "AngleSlider: RotatorEndDrag()" );
    }
    private float ClampValue(float input)
    {
        float newValue = Mathf.Clamp( input, rangeMin, rangeMax );
        if (wholeNumbers) {
            newValue = Mathf.Round( newValue );
        }
        return newValue;
    }

    private void Set(float input, bool sendCallback)
    {
        // Clamp the input
        float newValue = ClampValue( input );

        // If the stepped m_Value doesn't match the last one, it's time to update
        if (m_Value == newValue) {
            return;
        }

        m_Value = newValue;
        UpdateVisuals();

        if (sendCallback) {
            m_OnValueChanged.Invoke( newValue );
        } 
    }
    private void UpdateVisuals()
    {
        // Update Text
        displayText.text = m_Value.ToString( "F1", CultureInfo.InvariantCulture );
        // Rotate Image
        Vector3 angles = m_StartRotation;
        angles.z += m_Value - rangeMin;
        rotatingImage.localRotation = Quaternion.Euler( angles );
    }
}
