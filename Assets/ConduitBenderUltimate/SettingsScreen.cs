using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SettingsScreen : AnimScreen
{
    public Dropdown benderRadiusDropdown;
    public Dropdown conduitDiameterDropdown;
     
    public MultiRulerSlider benderRadiusRuler;
    public SingleRuler      conduitDiameterRuler;

    public Slider           cameraRailSensitivitySlider;
    public Slider           cameraZoomSensitivitySlider;
    public Slider           cameraTiltSensitivitySlider;

    public float updateThrottleMs = 100f;

    //---------------------
    //  Private Data
    //---------------------
    private ValueThrottle<float>    benderRadiusThrottle;
    private ValueThrottle<float>    conduitDiameterThrottle;

    private bool m_HasInitialized = false;


    protected override void Awake()
    {
        base.Awake();

    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StartCoroutine( PostEnableInitialize() );
    }
    protected override void Start()
    {
        base.Start();

        Initialize();
    }

    private IEnumerator PostEnableInitialize()
    {
        yield return new WaitForEndOfFrame();

        // Read Current Settings from Engine
        benderRadiusRuler.rulerDisplayType = Engine.unitType;
        benderRadiusRuler.value = Engine.benderRadiusM;

        conduitDiameterRuler.rulerDisplayType = Engine.unitType;
        conduitDiameterRuler.value = Engine.conduitDiameterM;

        cameraRailSensitivitySlider.value = Engine.cameraRailSensitivity;
        cameraZoomSensitivitySlider.value = Engine.cameraZoomSensitivity;
        cameraTiltSensitivitySlider.value = Engine.cameraTiltSensitivity;
    }

    private void Initialize()
    {
        if (m_HasInitialized) { return; }

        // Register Listeners
        benderRadiusThrottle = new ValueThrottle<float>( updateThrottleMs );
        benderRadiusThrottle.onValue += ListenerBenderRadius;
        benderRadiusRuler.onValueChanged.AddListener( benderRadiusThrottle.Set );

        conduitDiameterThrottle = new ValueThrottle<float>( updateThrottleMs );
        conduitDiameterThrottle.onValue += ListenerConduitDiameter;
        conduitDiameterRuler.onValueChanged.AddListener( conduitDiameterThrottle.Set );

        cameraRailSensitivitySlider.onValueChanged.AddListener( ListenerRailSensitivity );
        cameraZoomSensitivitySlider.onValueChanged.AddListener( ListenerZoomSensitivity );
        cameraTiltSensitivitySlider.onValueChanged.AddListener( ListenerTiltSensitivity );

        // Get Settings
        float       benderRange = 0f;
        float       conduitRange = 0f;
        string[]    benderOptions;
        string[]    conduitOptions;

        if (Engine.unitType == Units.Type.Metric) {
            benderRange = BenderInfo.Metric.k_BenderRadiusRange;
            conduitRange = BenderInfo.Metric.k_ConduitDiameterRange;
            benderOptions = BenderInfo.Metric.k_BenderRadiusPresets;
            conduitOptions = BenderInfo.Metric.k_ConduitDiameterPresets;
        } else {
            benderRange = BenderInfo.Standard.k_BenderRadiusRange;
            conduitRange = BenderInfo.Standard.k_ConduitDiameterRange;
            benderOptions = BenderInfo.Standard.k_BenderRadiusPresets;
            conduitOptions = BenderInfo.Standard.k_ConduitDiameterPresets;
        }

        //-------------------
        // Init Dropdowns
        //-------------------
        var benderRadiusOptions = new List<Dropdown.OptionData>();
        var conduitDiameterOptions = new List<Dropdown.OptionData>();

        for (int i = 0; i < benderOptions.Length; ++i) {
            benderRadiusOptions.Add( new Dropdown.OptionData( benderOptions[ i ] ) );
        }
        for (int i = 0; i < conduitOptions.Length; ++i) {
            conduitDiameterOptions.Add( new Dropdown.OptionData( conduitOptions[ i ] ) );
        }

        benderRadiusDropdown.options = benderRadiusOptions;
        conduitDiameterDropdown.options = conduitDiameterOptions;

        //------------------------------
        // Bender Radius Ruler Setup
        //------------------------------
        benderRadiusRuler.rulerDisplayType = Engine.unitType;
        benderRadiusRuler.SetRange( benderRange );
        //------------------------------
        // Conduit Diameter Ruler Setup
        //------------------------------
        conduitDiameterRuler.rulerDisplayType = Engine.unitType;
        conduitDiameterRuler.SetRange( conduitRange );


        m_HasInitialized = true;

        Debug.Log( "SettingsScreen: Initialize()" );
    }

    private void ListenerConduitDiameter( float value )
    {
        Engine.conduitDiameterM = (Engine.unitType == Units.Type.Metric) ? value : value * Units.k_FtToM;
        //Debug.Log( "SettingsScreen: ListenerConduitDiameter() value: " + value );
    }
    private void ListenerBenderRadius(float value)
    {
        Engine.benderRadiusM = (Engine.unitType == Units.Type.Metric) ? value : value * Units.k_FtToM;
        //Debug.Log( "SettingsScreen: ListenerBenderRadius() value: " + value );
    }

    private void ListenerRailSensitivity(float value)
    {
        Engine.cameraRailSensitivity = value;
        //Debug.Log( "SettingsScreen: ListenerRailSensitivity() value: " + value );
    }
    private void ListenerZoomSensitivity(float value)
    {
        Engine.cameraZoomSensitivity = value;
        //Debug.Log( "SettingsScreen: ListenerZoomSensitivity() value: " + value );
    }
    private void ListenerTiltSensitivity(float value)
    {
        Engine.cameraTiltSensitivity = value;
        //Debug.Log( "SettingsScreen: ListenerTiltSensitivity() value: " + value );
    }
    /// <summary>
    /// Called, for example, when Unit Mode changes from Metric to Standard.
    /// </summary>
    private void Reset()
    {

    }
}
