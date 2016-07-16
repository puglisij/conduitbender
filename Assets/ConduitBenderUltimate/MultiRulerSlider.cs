using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Text;
using System.Diagnostics;
using System.Collections;

/// <summary>
/// This Asset works based on its RectTransform anchors being at 0,0, and 1,1  and its offsets at 0
/// Also assumes the ruler prefab is oriented in the direction specified by rulerOrientation.
/// Also assumes RulerSlider instances are using whole numbers.
/// </summary>
public class MultiRulerSlider : Widget
{
    private static readonly string[] k_StandardDefaultRulerNames = {
        "fraction",
        "inches",
        "feet"
    };
    private static readonly string[] k_MetricDefaultRulerNames = {
        "millimetre",
        "centimetre",
        "metre"
    };
    const int k_MetricDefaultRuler1Range = 9;
    const int k_MetricDefaultRuler2Range = 99;
    const int k_StandardDefaultRuler1Range = 15;
    const int k_StandardDefaultRuler2Range = 11;

    // Constant Conversion Multipliers
    const float k_mmToM = 0.001f;
    const float k_mmToCm = 0.1f;
    const float k_mToCm = 100f;
    const float k_mToMm = 1000f;
    const float k_cmToMm = 10f;
    const float k_cmToM = 0.01f;

    const float k_ftToIn = 12f;
    const float k_inToFt = 0.083333333333f;
    const float k_inToSixteenths = 16f;
    const float k_ftToSixteenths = 192f;
    const float k_sixteenthsToIn = 0.0625f;
    const float k_sixteenthsToFt = 0.0052083333333f;

    public enum RulerOrientation { Horizontal, Vertical }

    /*-------------------------------

            Public Data

    -------------------------------*/
    /// <summary>
    /// Should not be set until after first Awake() frame
    /// </summary>
    public float value
    {
        get { return Get(); }
        set {
            //if (!m_HasAwoken) { return; }
            Set( value, true );
        }
    }
    public float rulerRange
    {
        get { return m_RulerRange; }
        set {
            //if(!m_HasAwoken) { return; }
            SetRange( value );
        }
    }
    /// <summary>
    /// Should not be set until after first Awake() frame
    /// </summary>
    public Units.Type rulerDisplayType
    {
        get { return m_RulerDisplayType; }
        set
        {
            //if (!m_HasAwoken) { return; }
            m_RulerDisplayType = value;
            SetDefaultThresholdsAndRanges();
            SetDefaultNames();
        }
    }
    public Slider.SliderEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }

    [SerializeField]
    public Text             displayText;

    [SerializeField]
    public RectTransform    rulerPrefab;

    [SerializeField]
    public RulerOrientation rulerOrientation;
    

    [Tooltip("As (0 to 1) percentage of this Components RectTransform." 
       + " If rulers Oriented Horizontally, this specifies vertical sizing, and vice-versa."), Range(0.01f, 1f)]
    public float            size = 0.25f;

    /*-------------------------------

            Private Data

    -------------------------------*/
    [SerializeField]
    private Slider.SliderEvent m_OnValueChanged = new Slider.SliderEvent();

    private RulerSlider[]       m_RulerSliders;

    private RectTransform[]     m_Rulers;
    //private RectTransform       m_ThisRectTransform;

    [SerializeField, HideInInspector]
    private Units.Type          m_RulerDisplayType;
    // Whole Numbers - From the Ruler representing the Smallest measurement scale to the Largest (i.e. indices 0 to rulerCount)
    private float[]             m_RulerValues;

    [SerializeField, Tooltip("Measurement Range of the MultiRuler (Feet or Meters).")]
    private float               m_RulerRange = 10f;

    private int               m_RulersCount = 1;
    //[SerializeField, Tooltip("(Sixteenths or Millimiters) Value over which 2nd ruler is displayed. 0 means Ruler 2 is always displayed.")]
    private int               m_OneRulerThreshold;
    private int               m_Ruler1Range;
    //[SerializeField, Tooltip("(Inches or Centimeters) Value over which 3rd ruler is displayed. 0 means Ruler 3 is always displayed. If equal to 2nd ruler threshold, will transtion from 1 to 3 Rulers.")]
    private int               m_TwoRulerThreshold;
    private int               m_Ruler2Range;


    private bool              m_VisualsDirty = false;
    private bool              m_RulerVisualsDirty = false;

    void Awake()
    {

        Initialize();
    }
    void Start()
    {

        Layout();
        Reset();
    }
    void Update()
    {
        if(m_VisualsDirty) {
            UpdateVisuals();
            if(m_RulerVisualsDirty) {
                UpdateRulerVisuals();
                m_RulerVisualsDirty = true;
            }
            m_VisualsDirty = false;
        }

    }

    private void Initialize()
    {
        ClearRulers();

        // Initializations
        //m_ThisRectTransform = (RectTransform)transform;

        // Checks
#if UNITY_EDITOR
        if (displayText == null) {
            UnityEngine.Debug.LogError( "MultiRulerSlider: Awake() Display Text null." );
            return;
        }
        if (rulerPrefab == null) {
            UnityEngine.Debug.LogError( "MultiRulerSlider: Awake() Ruler Prefab null." );
            return;
        }
        if (1f / size < m_RulersCount) {
            UnityEngine.Debug.LogError( "MultiRulerSlider: Awake() Ruler Size does not allow for Ruler Count." );
            return;
        }
#endif
        // Create & Setup Ruler Prefab Instances
        m_RulerSliders = new RulerSlider[ 3 ];
        m_Rulers = new RectTransform[ 3 ];
        m_RulerValues = new float[ 3 ];
        for (int r = 0; r < 3; ++r) {
            m_Rulers[ r ] = Instantiate( rulerPrefab );
            m_Rulers[ r ].SetParent( transform, false );
            m_RulerSliders[ r ] = m_Rulers[ r ].GetComponentInChildren<RulerSlider>();
            // Register Listener
            int ruler = r;
            m_RulerSliders[ r ].onValueChanged.AddListener( ( val ) => {
                m_RulerValues[ ruler ] = val;
                m_VisualsDirty = true;
            } );
        }

        // Set Ruler Ranges
        SetDefaultThresholdsAndRanges();
        SetRange( m_RulerRange );
    }


    /// <summary>
    /// Returns the combined value of the Sliders. 
    /// If rulerType is Metric, returned value is in Meters.
    /// If rulerType is Standard, returned value is in Feet.
    /// 
    /// @TODO Use 'Units' class
    /// </summary>
    public float Get()
    {
        float value = 0f;
        if(m_RulerDisplayType == Units.Type.Metric) 
        {
            switch (m_RulersCount) {
                case 1:
                    value = m_RulerValues[ 0 ] * k_mmToM;
                    break;
                case 2:
                    value = m_RulerValues[ 1 ] * k_cmToM;
                    value += m_RulerValues[ 0 ] * k_mmToM;
                    break;
                case 3:
                    value = m_RulerValues[ 2 ];
                    value += m_RulerValues[ 1 ] * k_cmToM;
                    value += m_RulerValues[ 0 ] * k_mmToM;
                    break;
            }
        } else 
        {
            switch (m_RulersCount) {
                case 1:
                    value = m_RulerValues[ 0 ] * k_sixteenthsToFt;
                    break;
                case 2:
                    value = m_RulerValues[ 1 ] * k_inToFt;
                    value += m_RulerValues[ 0 ] * k_sixteenthsToFt;
                    break;
                case 3:
                    value = m_RulerValues[ 2 ];
                    value += m_RulerValues[ 1 ] * k_inToFt;
                    value += m_RulerValues[ 0 ] * k_sixteenthsToFt;
                    break;
            }
        }
        
        return value;
    }

    /// <summary>
    /// Resets value of all sliders
    /// </summary>
    public void Reset()
    {
        for (int r = 0; r < m_RulersCount; ++r) {
            m_RulerSliders[ r ].SetValue( 0f, false );
        }
        m_VisualsDirty = true;
    }
    /// <summary>
    /// Value should be given in Meters.
    /// If rulerCount is 1, only fractions of an inch will be displayed. (or their metric equivalent)
    /// If rulerCount is 2, only inches and fractions will be displayed. (or their metric equivalent)
    /// If rulerCount is 3, feet, inches, and fractions will be displayed. (or their metric equivalent)
    /// </summary>
    public void Set(float val, bool sendCallback)
    {
        if (m_RulerDisplayType == Units.Type.Standard) {
            val = val * Units.k_MToFt;
        }
        SplitValue( val, m_RulerValues );

        if (sendCallback) {
            m_OnValueChanged.Invoke( Get() );
        }
        m_RulerVisualsDirty = true;
        m_VisualsDirty = true;
    }
    public void SetDefaultNames()
    {
        if (m_RulerDisplayType == Units.Type.Metric) {
            SetNames( k_MetricDefaultRulerNames );
        } else {
            SetNames( k_StandardDefaultRulerNames );
        }
        m_VisualsDirty = true;
    }
    public void SetDefaultThresholdsAndRanges()
    {
        if (m_RulerDisplayType == Units.Type.Metric) {
            m_OneRulerThreshold = 9;
            m_TwoRulerThreshold = 305;
            m_Ruler1Range = k_MetricDefaultRuler1Range;
            m_Ruler2Range = k_MetricDefaultRuler2Range;
        } else {
            m_OneRulerThreshold = 15;
            m_TwoRulerThreshold = 120;
            m_Ruler1Range = k_StandardDefaultRuler1Range;
            m_Ruler2Range = k_StandardDefaultRuler2Range;
        }
        m_VisualsDirty = true;
    }
    /// <summary>
    /// Set the Thresholds and Ranges for the Rulers. When a threshold is exceeded, another ruler slider is added.
    /// Thresholds are used when calling SetRange(). Given the range value, Thresholds are used to determine how many Rulers
    /// are needed to cover that range.
    /// Ranges are used as the 'maxValue' for the Ruler Sliders. 
    /// For example:
    /// If setting 1 Ruler Threshold to 15 and Ruler #1 Range to 15
    /// And setting 2 Ruler Threshold to 120 and Ruler #2 Range to 11
    /// 
    /// This would mean that if SetRange() were called with 96 inches, 
    /// Two Rulers would be displayed, since the Threshold of One Ruler has been exceeded, but the Threshold of Two Ruler's has not been exceeded.
    /// Since 2 Rulers are displayed, Ruler #2 Range is not used. The 1st ruler would have a range of 15 Sixteenths, and the 2nd ruler would have a range of 96 inches.
    /// 
    /// Parameters should be given in MultiRulerSlider's current RulerType (i.e. Metric or Standard)
    /// Should also be given in each Rulers respective Unit. For example, if setting the Threshold for Ruler 1 (the first/lowest unit ruler),
    /// parameter value should be in 'sixteenths' for Standard, or millimeters for Metric.
    /// </summary>
    public void SetThresholdsAndRanges( int oneRulerThreshold, int twoRulerThreshold, int ruler1Range, int ruler2Range )
    {
        m_OneRulerThreshold = oneRulerThreshold;
        m_TwoRulerThreshold = twoRulerThreshold;
        m_Ruler1Range = ruler1Range;
        m_Ruler2Range = ruler2Range;

        SetRange( m_RulerRange );
        m_VisualsDirty = true;
    }

    /// <summary>
    /// This automatically sets the RulerCount based on the Ruler Thresholds.
    /// Expected parameter value in Feet (Standard) or Meters (Metric).
    /// Also automatically sets the Ruler Count, and Names.
    /// </summary>
    public void SetRange( float range )
    {
        m_RulerRange = range;

        int m;
        int s;
        if (m_RulerDisplayType == Units.Type.Metric) {
            m = (int) Mathf.Floor( range * k_mToCm );
            s = (int) Mathf.Floor( range * k_mToMm );
        } else {
            m = (int) Mathf.Floor( range * k_ftToIn );
            s = (int) Mathf.Floor( range * k_ftToSixteenths );
        }
        
        if (m >= m_TwoRulerThreshold ) { // Inches or Cm     
            // 3 Rulers
            m_RulersCount = 3;
            m_RulerSliders[ 2 ].minValue = 0f;
            m_RulerSliders[ 2 ].maxValue = Mathf.Floor(range);
            m_RulerSliders[ 1 ].minValue = 0f;
            m_RulerSliders[ 1 ].maxValue = m_Ruler2Range;
            m_RulerSliders[ 0 ].minValue = 0f;
            m_RulerSliders[ 0 ].maxValue = m_Ruler1Range;
        } else if (s >= m_OneRulerThreshold ) { // Fractions or Mm
            // 2 Rulers
            m_RulersCount = 2;
            m_RulerSliders[ 1 ].minValue = 0f;
            m_RulerSliders[ 1 ].maxValue = Mathf.Floor( m );
            m_RulerSliders[ 0 ].minValue = 0f;
            m_RulerSliders[ 0 ].maxValue = m_Ruler1Range;
        } else {
            // 1 Ruler
            m_RulersCount = 1;
            m_RulerSliders[ 0 ].minValue = 0f;
            m_RulerSliders[ 0 ].maxValue = s;
        }
        SetDefaultNames();
        Layout();
    }


    private void ClearRulers()
    {
        for (int i = 0; i < transform.childCount; ++i) {
            Destroy( transform.GetChild( i ).gameObject );
        }
    }
    private void Layout()
    {

        for (int r = 0; r < m_RulersCount; ++r) {
            m_Rulers[ r ].gameObject.SetActive( true );
        }
        for (int r = m_RulersCount; r < 3; ++r) {
            m_Rulers[ r ].gameObject.SetActive( false );
        }
        // Position Rulers
        float spacing = (1f - (m_RulersCount * size));
        spacing = spacing / (m_RulersCount + 1);

        if (rulerOrientation == RulerOrientation.Horizontal) {
            Vector2 anchorMin = new Vector2(0f, spacing);
            Vector2 anchorMax = new Vector2(1f, spacing + size);
            for (int r = 0; r < m_RulersCount; ++r) {
                m_Rulers[ r ].anchorMin = anchorMin;
                m_Rulers[ r ].anchorMax = anchorMax;
                m_Rulers[ r ].offsetMin = Vector2.zero;
                m_Rulers[ r ].offsetMax = Vector2.zero;
                anchorMin.y = anchorMax.y + spacing;
                anchorMax.y = anchorMin.y + size;
            }
        } else {
            Vector2 anchorMin = new Vector2( spacing, 0f);
            Vector2 anchorMax = new Vector2( spacing + size, 1f );
            for (int r = 0; r < m_RulersCount; ++r) {
                m_Rulers[ r ].anchorMin = anchorMin;
                m_Rulers[ r ].anchorMax = anchorMax;
                m_Rulers[ r ].offsetMin = Vector2.zero;
                m_Rulers[ r ].offsetMax = Vector2.zero;
                anchorMin.x = anchorMax.x + spacing;
                anchorMax.x = anchorMin.x + size;
            }
        }

    }
    //private IEnumerator PostSetNames( string[] names )
    //{
    //    while(!m_HasAwoken) {
    //        yield return new WaitForEndOfFrame();
    //    }
    //    if (names.Length < m_RulersCount) {
    //        UnityEngine.Debug.LogError( "MultiRulerSlider: SetRulerNames() Invalid names count." );
    //        yield break;
    //    }
    //    for (int r = 0; r < m_RulersCount; ++r) {
    //        m_Rulers[ r ].GetComponent<RulerNameText>().nameText.text = names[ r ];
    //    }
    //}

    private void SetNames( string[] names )
    {
        //StartCoroutine( PostSetNames( names ) );
        for (int r = 0; r < m_RulersCount; ++r) {
            m_Rulers[ r ].GetComponent<RulerNameText>().nameText.text = names[ r ];
        }
    }
    /// <summary>
    /// 'val' is in Feet or Meters
    /// </summary>
    private void SplitValue( float val, float[] splitValues )
    {
        if (splitValues.Length != 3) { throw new ArgumentException( "MultiRulerSlider: SplitValue() Invalid Array Length." ); }

        System.Array.Clear( splitValues, 0, 3 );

        Units.RulerUnit unit;
        float[]         sv = null;
        switch(m_RulersCount) 
        {
            case 1:
                if(m_RulerDisplayType == Units.Type.Metric) {
                    unit = Units.RulerUnit.Millimeters;
                    val *= Units.k_MToMm;
                } else {
                    unit = Units.RulerUnit.Sixteenths;
                    val *= Units.k_FtToSixteenths;
                }
                sv = Units.Split( m_RulerDisplayType, unit, val );
                break;
            case 2:
                if (m_RulerDisplayType == Units.Type.Metric) {
                    unit = Units.RulerUnit.Centimeters;
                    val *= Units.k_MToCm;
                } else {
                    unit = Units.RulerUnit.Inches;
                    val *= Units.k_FtToIn;
                }
                sv = Units.Split( m_RulerDisplayType, unit, val );
                break;
            case 3:
                if (m_RulerDisplayType == Units.Type.Metric) {
                    unit = Units.RulerUnit.Meters;
                } else {
                    unit = Units.RulerUnit.Feet;
                }
                sv = Units.Split( m_RulerDisplayType, unit, val );
                break;
        }
        for(int i = 0; i < sv.Length; ++i) {
            splitValues[ i ] = sv[ i ];
        }

    }

    private void UpdateRulerVisuals()
    {
        for (int r = 0; r < m_RulersCount; ++r) {
            m_RulerSliders[ r ].SetValue( m_RulerValues[ r ], false );
        }
    }
    private void UpdateVisuals()
    {

        string txt = "";
        if (m_RulerDisplayType == Units.Type.Metric) {
            switch (m_RulersCount) {
                case 1:
                    txt = m_RulerValues[ 0 ] + "mm";
                    break;
                case 2:
                    txt = m_RulerValues[ 1 ] + "cm - ";
                    txt += m_RulerValues[ 0 ] + "mm";
                    break;
                case 3:
                    txt = m_RulerValues[ 2 ] + "m ";
                    txt += m_RulerValues[ 1 ] + "cm - ";
                    txt += m_RulerValues[ 0 ] + "mm";
                    break;
            }
        } else {
            switch (m_RulersCount) {
                case 1:
                    txt = new Rational( (long)m_RulerValues[ 0 ], (long)k_inToSixteenths ).ToString() + "\"";
                    break;
                case 2:
                    txt = m_RulerValues[ 1 ] + " - ";
                    txt += new Rational( (long)m_RulerValues[ 0 ], (long)k_inToSixteenths ).ToString() + "\"";
                    break;
                case 3:
                    txt = m_RulerValues[ 2 ] + "' ";
                    txt += m_RulerValues[ 1 ] + " - ";
                    txt += new Rational( (long)m_RulerValues[ 0 ], (long)k_inToSixteenths ).ToString() + "\"";
                    break;
            }
        }
        // Update Text
        displayText.text = txt;


        // Send Callback
        m_OnValueChanged.Invoke( Get() );
    }
}
