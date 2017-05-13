using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using CB;

public class BendInputScreen : AnimScreen
{
    [Space]

    public ScrollRectExtra  bendParameterScroller;
    public RectTransform    bendParameterPrefab;

    [Space]

    public RectTransform angleWidgetPrefab;
    public RectTransform rulerWidgetPrefab;
    public RectTransform integerWidgetPrefab;
    public RectTransform enumWidgetPrefab;
    public RectTransform helpWidgetPrefab;

    [Space]

    public RectTransform        widgetView;
    public RectTransform        outputView;
    public ConduitOutputModal   outputModal;

    [Space]

    public AlertText    alertText;

    public float        bendUpdateThrottleMs = 50f;
    //------------------------------
    //      Private Data
    //------------------------------
    private ValueThrottle<float> angleThrottle;
    private ValueThrottle<float> rulerThrottle;
    private ValueThrottle<int>   intThrottle;

    private RectTransform m_ActiveWidget;
    private RectTransform m_AngleWidget;
    private RectTransform m_RulerWidget;
    private RectTransform m_IntegerWidget;
    private RectTransform m_EnumWidget;
    private RectTransform m_HelpWidget;

    private TextColumns   m_OutputText;

    [SerializeField, HideInInspector]
    private Bend          m_Bend;

    private int           m_ActiveParameter = -1;
    private int           m_EnabledParameterMask = 0;

    private bool          m_HasInitialized = false;
    

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if(angleWidgetPrefab == null) {
            Debug.LogError( "BendInputScreen: Awake() Angle Widget Prefab null." );
            return;
        }
        if(rulerWidgetPrefab == null) {
            Debug.LogError( "BendInputScreen: Awake() Ruler Widget Prefab null." );
            return;
        }
        if (integerWidgetPrefab == null) {
            Debug.LogError( "BendInputScreen: Awake() Integer Widget Prefab null." );
            return;
        }
        if (enumWidgetPrefab == null) {
            Debug.LogError( "BendInputScreen: Awake() Enum Widget Prefab null." );
            return;
        }
        if(widgetView == null || outputView == null) {
            Debug.LogError( "BendInputScreen: Awake() Views null." );
            return;
        }
#endif
        m_ActiveWidget = null;
        Initialize();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StartCoroutine( PostEnableInitialize() );

        //Debug.Log( "BendInputScreen: OnEnable()" );
    }

    protected override void Start()
    {
        base.Start();

    }
    //void LateUpdate()
    //{
    //    if(m_TextNeedsSizing) {
    //        SizeFont();
    //        m_TextNeedsSizing = false;
    //    }
    //}

    private IEnumerator PostEnableInitialize()
    {
        yield return new WaitForEndOfFrame();

        // Read Current Display Setting from Engine
        //MultiRulerSlider rulerSlider = m_RulerWidget.GetComponentsInChildren<MultiRulerSlider>(true)[0];
        //rulerSlider.rulerDisplayType = Engine.unitType;
    }

    private void Initialize()
    {
        if(m_HasInitialized) { return; }
        // Create Value Throttles
        angleThrottle = new ValueThrottle<float>( bendUpdateThrottleMs );
        angleThrottle.onValue += ListenerAngle;
        rulerThrottle = new ValueThrottle<float>( bendUpdateThrottleMs );
        rulerThrottle.onValue += ListenerRuler;
        intThrottle = new ValueThrottle<int>( bendUpdateThrottleMs );
        intThrottle.onValue += ListenerInteger;

        // Create Prefabs
        m_AngleWidget = Instantiate( angleWidgetPrefab );
        m_AngleWidget.SetParent( widgetView, false );
        m_AngleWidget.gameObject.SetActive( false );
        AngleSlider angleSlider = m_AngleWidget.GetComponentsInChildren<AngleSlider>(true)[0];
        angleSlider.onValueChanged.AddListener( angleThrottle.Set );

        m_RulerWidget = Instantiate( rulerWidgetPrefab );
        m_RulerWidget.SetParent( widgetView, false );
        m_RulerWidget.gameObject.SetActive( false );
        MultiRulerSlider rulerSlider = m_RulerWidget.GetComponentsInChildren<MultiRulerSlider>(true)[0];
        rulerSlider.onValueChanged.AddListener( rulerThrottle.Set );
        rulerSlider.rulerDisplayType = Engine.unitType;

        m_IntegerWidget = Instantiate( integerWidgetPrefab );
        m_IntegerWidget.SetParent( widgetView, false );
        m_IntegerWidget.gameObject.SetActive( false );
        IntegerSlider integerSlider = m_IntegerWidget.GetComponentsInChildren<IntegerSlider>(true)[0];
        integerSlider.onValueChanged = intThrottle.Set;

        m_EnumWidget = Instantiate( enumWidgetPrefab );
        m_EnumWidget.SetParent( widgetView, false );
        m_EnumWidget.gameObject.SetActive( false );
        EnumDropdown enumDropdown = m_EnumWidget.GetComponentsInChildren<EnumDropdown>(true)[0];
        enumDropdown.onValueChanged.AddListener( ListenerEnum );

        m_HelpWidget = Instantiate( helpWidgetPrefab );
        m_HelpWidget.SetParent( widgetView, false );
        m_HelpWidget.gameObject.SetActive( false );
        BendHelp bendHelp = m_HelpWidget.GetComponentsInChildren<BendHelp>(true)[0];
        bendHelp.onValueChanged = ListenerHelp;

        m_OutputText = outputView.GetComponentsInChildren<TextColumns>( true )[ 0 ];
        m_OutputText.maxCharPerLineTotal = Engine.GetLanguageMaxParameterStringLength() * m_OutputText.columns.Count;


        m_HasInitialized = true;
    }
    /*##################################

            Screen Functions

    ###################################*/
    public override void Open()
    {
        base.Open();


    }
    public override void Close( bool doDisable )
    {
        base.Close( doDisable );

        if (outputModal.isOpen) {
            outputModal.Close( true );
        }
    }

    public void OpenOutputModal()
    {
        // If currently displaying alert, ignore
        if (!string.IsNullOrEmpty( m_Bend.alert )) {
            return;
        }

        // Get Text from Output View and Write to Modal
        string outputLines = m_OutputText.GetColumnText(0) + m_OutputText.GetColumnText(1);
        outputModal.WriteLines( outputLines );
        outputModal.modalTitle = screenTitle;

        outputModal.Open();
    }
    /*##################################

            Public Functions

    ###################################*/
    /// <summary>
    /// Links this Bend Input Screen to a Bend and Sets up Screen Appropriately
    /// </summary>
    public override void Link( IModel model )
    {
        if(!m_HasInitialized) {
            Debug.LogError( "BendInputScreen: Link() Screen must first be Initialized." );
            return;
        }
        if(m_Bend != null) {
            // Unregister Listeners on Previous Bend
            m_Bend.onEvent.RemoveListener( ListenerBend );
        }
        Bend bend = (Bend) model;
        m_Bend = bend;
        m_Bend.onEvent.AddListener( ListenerBend );

        // Initialize Output View
        InitializeOutput();

        RectTransform   scrollerContent = bendParameterScroller.content;
        RectTransform   bendParamPrefab;
        UIBendParameter uiBendParam;
        BendParameter   bendParam;

        bendParameterScroller.ClearContent();

        // Set up Bend Input Parameter View
        for (int i = 0; i < bend.inputParameters.Count; ++i) 
        {
            bendParam = bend.inputParameters[ i ];
            bendParamPrefab = Instantiate(bendParameterPrefab);
            bendParamPrefab.SetParent( scrollerContent, false );

            uiBendParam = bendParamPrefab.GetComponent<UIBendParameter>();
            uiBendParam.Set( bendParam.type, BendParameter.GetStringValue( bendParam.name ), i );
            uiBendParam.onClick = DisplayWidget;
            if(i == 0) {
                // Set 1st Parameter as Selected
                uiBendParam.button.Select();
            }       
        }

        bendParameterScroller.Layout();

        // Set up Help Widget
        bendParamPrefab = Instantiate( bendParameterPrefab );
        bendParamPrefab.SetParent( scrollerContent, false );
        uiBendParam = bendParamPrefab.GetComponent<UIBendParameter>();
        uiBendParam.SetAsHelp();
        uiBendParam.onClick = DisplayHelpWidget;

        // Display 1st Parameter Widget
        m_ActiveParameter = -1;
        DisplayWidget( 0 );
    }

    
    /// <summary>
    ///     Refresh Output View 
    /// </summary>
    public void ListenerBend( Bend.EventType type )
    {
        if(type != Bend.EventType.Calculated) { return; }

        BendParameter           bendParam;
        TextColumns.TextPair    textPair;
        int enabledMask = 0;
        int line        = 0;
        int pinput      = 0;
        var textPairs   = m_OutputText.GetLines();

        // Assert no bitmask overflow
        Debug.Assert( m_Bend.inputParameters.Count + m_Bend.outputParameters.Count <= sizeof( int ) * 8 );

        // Input Parameters
        for (int p = 0; p < m_Bend.inputParameters.Count && p < textPairs.Count; ++p) 
        {
            bendParam = m_Bend.inputParameters[ p ];
            if(bendParam.enabled) {
                enabledMask |= 1 << (p);

                textPair = textPairs[ line ];
                textPair.right = "<color=#" + bendParam.colorHexString + ">" + BendParameter.GetFormattedValue( bendParam ) + "</color>";
                textPairs[ line ] = textPair;
                line += 1;
            }
        }
        pinput = m_Bend.inputParameters.Count;

        // Output Parameters  (FF1800FF, EA75FF)
        for (int p = 0; p < m_Bend.outputParameters.Count && line < textPairs.Count; ++p) {
            bendParam = m_Bend.outputParameters[ p ];
            if(bendParam.enabled) {
                enabledMask |= 1 << (pinput + p);

                textPair = textPairs[ line ];
                textPair.right = "<b><color=#" + bendParam.colorHexString + ">" + BendParameter.GetFormattedValue( bendParam ) + "</color></b>";
                textPairs[ line ] = textPair;
                line += 1;
            }
        }
        
        if(enabledMask != m_EnabledParameterMask) {
            InitializeOutput();
        } else {
            m_OutputText.WriteLines( textPairs );
        }

        // Display any Alert Text
        if(!string.IsNullOrEmpty(m_Bend.alert)) {
            alertText.Set( m_Bend.alert );
        } else {
            alertText.Off();
        }

        //Debug.Log( "UIBendInputScreen: ListenerBend() Output Refreshed." );
    }


    /*##################################

            Private Functions

    ###################################*/
    private void DisplayHelpWidget(int id)
    {
        if (m_ActiveParameter == id) { return; }

        // Disable current Widget 
        if (m_ActiveWidget != null) {
            m_ActiveWidget.gameObject.SetActive( false );
        }

        // Set current Widget as active (id is -1)
        m_ActiveParameter = id; 
        m_ActiveWidget = m_HelpWidget;

        // Highlightable parameters may have changed in the case of a different bend Method being chosen. Thus we initialize here.
        var help = m_HelpWidget.GetComponentInChildren<BendHelp>();
            help.SetHighlightables( m_Bend.GetHighlightables() );
        m_HelpWidget.gameObject.SetActive( true );
    }

    private void DisplayWidget( int id )
    {
        // Remove any active Highlighting
        m_Bend.SetHighlight( null );
        m_HelpWidget.GetComponentInChildren<BendHelp>().UnSelect();

        // Don't reselect the same Widget
        if(m_ActiveParameter == id) { return; }
        // Disable current Widget
        if (m_ActiveWidget != null) {
            m_ActiveWidget.gameObject.SetActive( false );
        }
        m_ActiveParameter = id;

        BendParameter param         = m_Bend.inputParameters[ id ];
        EBendParameterType type     = param.type;
        object[] range              = BendParameter.GetRange( param.name );

        switch (type) {
            case EBendParameterType.FloatAngle:
                m_ActiveWidget = m_AngleWidget;
                m_ActiveWidget.gameObject.SetActive( true );
                AngleSlider ans = m_AngleWidget.GetComponentInChildren<AngleSlider>();
                ans.SetRange( (float)range[ 0 ], (float)range[ 1 ] );
                ans.value = (float) param.value;
                break;
            case EBendParameterType.Float:
                m_ActiveWidget = m_RulerWidget;
                m_ActiveWidget.gameObject.SetActive( true );
                MultiRulerSlider mr = m_RulerWidget.GetComponentInChildren<MultiRulerSlider>();
                mr.SetRange( (float)range[ 1 ] );
                mr.Set( (float) param.value, false );
                break;
            case EBendParameterType.Integer:
                m_ActiveWidget = m_IntegerWidget;
                m_ActiveWidget.gameObject.SetActive( true );
                IntegerSlider ins = m_IntegerWidget.GetComponentInChildren<IntegerSlider>();
                ins.SetRange( (int)range[ 0 ], (int)range[ 1 ] );
                ins.SetName( BendParameter.GetStringValue( param.name ) );
                ins.value = (int) param.value;
                break;
            case EBendParameterType.StringEnum:
                m_ActiveWidget = m_EnumWidget;
                m_ActiveWidget.gameObject.SetActive( true );
           
                EnumDropdown ed = m_EnumWidget.GetComponentInChildren<EnumDropdown>();
                List<Dropdown.OptionData> enumList = ed.options;
                StringEnum se = (StringEnum) param.valueObject;
                int enumStart = (int)range[0];
                int enumEnd = (int)range[1];
                enumList.Clear();
                for(int i = enumStart; i <= enumEnd; ++i) {
                    enumList.Add( new EnumDropdown.EnumOptionData( se.ToStringValue( i ), null, se.ToDescription( i ) ) );
                }
                ed.value = (int) param.value;
                ed.Refresh();
                break;
            default:
                throw new ArgumentException( "BendInputScreen: DisplayWidget() Invalid type." );
        }

    }
    
    private void InitializeOutput()
    {
        // Init Output View
        TextColumns.TextPair textPair;
        BendParameter        bendParam;
        int                  pinput = 0;

        m_EnabledParameterMask = 0;
        m_OutputText.Clear();

        // Input Parameters
        for (int p = 0; p < m_Bend.inputParameters.Count; ++p) {
            bendParam = m_Bend.inputParameters[ p ];

            textPair = new TextColumns.TextPair();
            textPair.left = "<b>" + BendParameter.GetStringValue( bendParam.name ) + ":</b> ";
            textPair.right = "<color=#" + bendParam.colorHexString + ">" + BendParameter.GetFormattedValue( bendParam ) + "</color>";

            m_OutputText.Write( textPair );
        }
        pinput = m_Bend.inputParameters.Count;

        // Output Parameters  (FF1800FF, EA75FF)
        for (int p = 0; p < m_Bend.outputParameters.Count; ++p) {
            bendParam = m_Bend.outputParameters[ p ];
            if(bendParam.enabled) {
                m_EnabledParameterMask |= 1 << (pinput + p);

                textPair = new TextColumns.TextPair();
                textPair.left = "<b>" + BendParameter.GetStringValue( bendParam.name ) + ":</b> ";
                textPair.right = "<b><color=#" + bendParam.colorHexString + ">" + BendParameter.GetFormattedValue( bendParam ) + "</color></b>";

                m_OutputText.Write( textPair );
            }
        }

        //Debug.Log( "BendInputScreen: InitializeOutput() m_EnabledParameterMask: " + m_EnabledParameterMask + " Lines: " + m_OutputText.lineCount );
    }

    private void ListenerAngle(float val)
    {
        m_Bend.SetInputParameter( m_ActiveParameter, val );
    }
    private void ListenerRuler(float val)
    {
        m_Bend.SetInputParameter( m_ActiveParameter, Engine.GetInternalValue( val ) );
    }
    private void ListenerInteger(int val)
    {
        m_Bend.SetInputParameter( m_ActiveParameter, val );
    }
    private void ListenerEnum(int val)
    {
        m_Bend.SetInputParameter( m_ActiveParameter, val );
    }
    private void ListenerHelp(BendParameter val)
    {
        m_Bend.SetHighlight( val );
    }

    //private void SizeFont()
    //{
    //    UIBendParameter[] uiBendParams  = bendParameterScroller.content.GetComponentsInChildren<UIBendParameter>();
    //    // Use size of 'BestFit' of the Text box with most characters for all
    //    float maxFontSize = 0f;
    //    for(int i = 0; i < uiBendParams.Length; ++i) {
    //        float fontSize = uiBendParams[ i ].nameText.cachedTextGenerator.fontSizeUsedForBestFit;
    //        if (fontSize > maxFontSize) {
    //            maxFontSize = fontSize;
    //        }
    //        Debug.Log( "SizeFont() Font Size: " + fontSize );
    //    }
    //}
	
    

}
