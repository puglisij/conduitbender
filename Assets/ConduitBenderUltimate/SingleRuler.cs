using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SingleRuler : MonoBehaviour
{
    public RulerSlider rulerSlider;

    public Text     displayText;
    public Text     rulerNameText;

    public Units.RulerUnit   metricRulerUnit
    {
        get { return m_MetricRulerUnit; }
        set { m_MetricRulerUnit = value; }
    }
    public Units.RulerUnit standardRulerUnit
    {
        get { return m_StandardRulerUnit; }
        set { m_StandardRulerUnit = value; }
    }
    /// <summary>
    /// Should not be called until After first Awake() Frame
    /// </summary>
    public Units.Type rulerDisplayType
    {
        get { return m_RulerDisplayType; }
        set
        {
            //if (!m_HasAwoken) { return; }
            m_RulerDisplayType = value;
            Refresh();
        }
    }
    /// <summary>
    /// Returns value in Feet or Meters.
    /// Expects value in Meters. Does not send a callback when set.
    /// </summary>
    public float value
    {
        get { return Get(); }
        set {
            Set( value, false );
        }
    }
    public Slider.SliderEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }


    [SerializeField]
    private Slider.SliderEvent m_OnValueChanged = new Slider.SliderEvent();

    [SerializeField, HideInInspector]
    private Units.Type          m_RulerDisplayType;
    [SerializeField]
    private Units.RulerUnit     m_MetricRulerUnit;
    [SerializeField]
    private Units.RulerUnit     m_StandardRulerUnit;
    private Units.RulerUnit     m_RulerUnit;

    //private string              m_RulerName = "Error";

    private float               m_Range = 1f;
    private float               m_Value = 0f;

    private bool                m_VisualsDirty = false;

    void Awake()
    {

        Initialize();
    }

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (m_VisualsDirty) {
            UpdateVisuals();
            m_VisualsDirty = false;
        }

    }

    private void Initialize()
    {
        Refresh();
        SetRange( m_Range );

        // Listen
        rulerSlider.onValueChanged.AddListener( ( val ) => {
             m_Value = val;
             m_OnValueChanged.Invoke( Get() );
             m_VisualsDirty = true;
        } );
    }

    private void Refresh()
    {

        //if (m_RulerDisplayType == Units.Type.Metric) {
        //    m_RulerName = Units.RulerName( m_MetricRulerUnit );
        //} else {
        //    m_RulerName = Units.RulerName( m_StandardRulerUnit );
        //}

        m_RulerUnit = (m_RulerDisplayType == Units.Type.Metric) ? m_MetricRulerUnit : m_StandardRulerUnit;
    }

    private void UpdateVisuals()
    {
        var displayUnit = (m_RulerDisplayType == Units.Type.Metric) ? Units.RulerUnit.Centimeters : Units.RulerUnit.Inches;
        // Convert Value to proper Unit if necessary
        float displayValue = Units.Convert( m_RulerDisplayType, m_RulerUnit, displayUnit, m_Value );

        // Format String output
        string txt = Units.Format( m_RulerDisplayType, displayUnit, displayValue );

        displayText.text = txt;
    }

    /// <summary>
    /// Returns value of Ruler in Feet or Meters
    /// </summary>
    public float Get()
    {
        var returnUnit = (m_RulerDisplayType == Units.Type.Metric) ? Units.RulerUnit.Meters : Units.RulerUnit.Feet;
        return Units.Convert( m_RulerDisplayType, m_RulerUnit, returnUnit, m_Value );
    }

    /// <summary>
    /// Resets value of slider
    /// </summary>
    public void Reset()
    {
        rulerSlider.SetValue( 0f, false );

        m_VisualsDirty = true;
    }
    /// <summary>
    /// Expected value in Meters
    /// </summary>
    public void Set(float val, bool sendCallback)
    {
        Units.RulerUnit fromUnit = Units.RulerUnit.Meters;
        if(m_RulerDisplayType == Units.Type.Standard) {
            fromUnit = Units.RulerUnit.Feet;
            val = val * Units.k_MToFt;
        }
        m_Value = Units.Convert( m_RulerDisplayType, fromUnit, m_RulerUnit, val );

        rulerSlider.SetValue( m_Value, false );

        if (sendCallback) {
            m_OnValueChanged.Invoke( Get() );
        }

        m_VisualsDirty = true;
    }
    public void SetRange(float range)
    {
        m_Range = range;
        // Set Ruler Min/Max
        rulerSlider.minValue = 0f;
        rulerSlider.maxValue = m_Range;

        m_VisualsDirty = true;
    }
}
