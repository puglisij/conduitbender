using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using CB;


public class SettingsScreen : AnimScreen
{
    public enum SettingName { ConduitDiameter, BenderRadius, CameraControls, Audio, None }

    private const string k_conduitDiameterDescription = "Conduit Diameter is given as nominal Outside Diameter.";
    private const string k_benderRadiusDescription = "Bender Radius is the centerline radius of the bender.";
    private const string k_cameraControlsDescription = "Adjust the sensitivity of the camera controls for conduit view.";
    private const string k_audioDescription = "";

    /// <summary> Preset values are in Meters. </summary>
    private const string k_presetsFileName = "SettingPresets";
    //private const string k_saveFileName = "userPresets.dat";

    public SelectionModal        selectionModalPrefab;
    public SettingNumeric        settingNumericPrefab; 
    public RectTransform         sliderPrefab;

    [Tooltip("Where the setting prefabs will be placed.")]
    public RectTransform         inputView;

    public Text             settingHeaderText;

    //---------------------
    //  Private Data
    //---------------------
    SelectionModal   m_presetModal = null;

    SettingName      m_activeSetting = SettingName.None;

    //ValueThrottle<float> m_valueThrottle;

    bool m_hasInitialized = false;

    //---------------------------
    // Unity & Overrides
    //---------------------------
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

    public override void Open()
    {
        base.Open();
    }

    public override void Close( bool doDisable )
    {
        base.Close( doDisable );
    }

    private IEnumerator PostEnableInitialize()
    {
        yield return new WaitForEndOfFrame();

        // Read Current Settings from Engine Here and not in Start()
        if(m_activeSetting == SettingName.None) {
            // Default to Conduit Diameter
            OpenSetting( SettingName.ConduitDiameter );
        }
    }

    private void Initialize()
    {
        if (m_hasInitialized) { return; }

        // Register Listeners
        //benderRadiusThrottle = new ValueThrottle<float>( updateThrottleMs );
        //benderRadiusThrottle.onValue += OnBenderRadius;
        //benderRadiusRuler.onValueChanged.AddListener( benderRadiusThrottle.Set );

        ////------------------------------
        //// Bender Radius Ruler Setup
        ////------------------------------
        //benderRadiusRuler.rulerDisplayType = Engine.unitType;
        //benderRadiusRuler.SetRange( benderRange );


        m_hasInitialized = true;

        Debug.Log( "SettingsScreen: Initialize()" );
    }

    /*==============================

          Public Functions

    ==============================*/
    public void OpenSetting(string settingName)
    {
        var setting = (SettingName) Enum.Parse( typeof( SettingName ), settingName );
        OpenSetting( setting );
    }
    public void OpenSetting( SettingName settingName )
    {
        ClearInputView();

        m_activeSetting = settingName;

        if (settingName == SettingName.ConduitDiameter || settingName == SettingName.BenderRadius) 
        {
            SettingNumeric setting = (SettingNumeric) Instantiate(settingNumericPrefab, inputView, false);

            SettingNumeric.OnValue       onValueFn;
            float   unitMultiplier,
                    bigIncDec,
                    smallIncDec,
                    minValue,
                    maxValue,
                    value;
            string  description;
            
            if (settingName == SettingName.ConduitDiameter) 
            {
                settingHeaderText.text = "Conduit Diameter";
                
                if (Engine.unitType == Units.Type.Metric) {
                    bigIncDec = 1f;
                    smallIncDec = 0.1f;
                    unitMultiplier = Units.k_MToCm;
                } else {
                    bigIncDec = 1f;
                    smallIncDec = 0.0625f;
                    unitMultiplier = Units.k_MToIn;
                }

                minValue = Settings.k_minConduitDiameterM * unitMultiplier;
                maxValue = Settings.k_maxConduitDiameterM * unitMultiplier;
                value = Engine.conduitDiameterM * unitMultiplier;
                description = k_conduitDiameterDescription;
                onValueFn = OnConduitDiameter;
            } 
            else 
            {
                settingHeaderText.text = "Bender Radius";

                if (Engine.unitType == Units.Type.Metric) {
                    bigIncDec = 1f;
                    smallIncDec = 0.1f;
                    unitMultiplier = Units.k_MToCm;
                } else {
                    bigIncDec = 1f;
                    smallIncDec = 0.0625f;
                    unitMultiplier = Units.k_MToIn;
                }

                minValue = Settings.k_minBenderRadiusM * unitMultiplier;
                maxValue = Settings.k_maxBenderRadiusM * unitMultiplier;
                value = Engine.benderRadiusM * unitMultiplier;
                description = k_benderRadiusDescription;
                onValueFn = OnBenderRadius;
            }

            // Setup the Numeric input widget
            setting.Setup(  minValue, maxValue,
                            bigIncDec, smallIncDec, 
                            description, 
                            Engine.unitType == Units.Type.Metric ? Units.RulerUnit.Centimeters : Units.RulerUnit.Inches );
            setting.value = value;
            setting.onPresetClick += OpenPresets;
            setting.onValue += onValueFn;
        } 
        else if(settingName == SettingName.CameraControls) 
        {

        } 
        else if(settingName == SettingName.Audio) 
        {

        }
    }

    /*==============================

          Private Functions

    ==============================*/
    private void ClearInputView()
    {
        inputView.DestroyChildren();
    }

    private void OpenPresets()
    {
        var           presetsFile = LoadPresets();
        KeyFloatSet[] presets = null;

        switch (m_activeSetting) {
            case SettingName.ConduitDiameter:
                presets = presetsFile.conduitDiameter;
                break;
            case SettingName.BenderRadius:
                presets = presetsFile.benderRadius;
                break;
        }

        if (presets == null || presets.Length == 0) {
#if DEBUG
            Debug.LogError( "SettingScreen: OpenConduitDiameterPresets() No presets could be loaded." );
#endif
            return;
        }

        OpenPresetModal( presets, OnPresetSelect );
    }

    private void OpenPresetModal(KeyFloatSet[] presets, SelectionModal.OnSelect handler)
    {
        // Instantiate Modal from prefab 
        var modal = (SelectionModal) Instantiate( selectionModalPrefab, transform, false );

        // OPT: All this casting, pretty inefficient...
        // Convert to KeyValueSet 
        var selections = Array.ConvertAll( presets, ( p ) => {
            return (KeyValueSet<object>)p;
        });

        // Set Preset Selections
        modal.SetSelections( selections );
        modal.onSelect += handler;
        modal.transform.SetAsLastSibling();
        modal.gameObject.SetActive( true );

        m_presetModal = modal;
    }

    /// <summary> Loads the presets file. The preset values are in Meters. </summary>
    private SettingPresets LoadPresets()
    {
        // Read Presets file from disk
        var presets = new SettingPresets();
        AppData.LoadResourceJSON( k_presetsFileName, out presets );

        return presets;
    }

    private void OnPresetSelect( object value, int[] route )
    {
        var floatValue = (float)value * (Engine.unitType == Units.Type.Metric ? Units.k_MToCm : Units.k_MToIn);

        // Setting value here will trigger a change event
        inputView.GetComponentInChildren<SettingNumeric>( true ).value = floatValue;

        // Close & Destroy the Preset Modal
        m_presetModal.Close( true );
        Destroy( m_presetModal );
        m_presetModal = null;
    }

    private void OnBenderRadius( float value )
    {
        Engine.benderRadiusM = (Engine.unitType == Units.Type.Metric) ? value * Units.k_CmToM : value * Units.k_InToM;
        Debug.Log( "SettingsScreen: OnBenderRadius() value: " + value );
    }
    private void OnConduitDiameter( float value )
    {
        Engine.conduitDiameterM = (Engine.unitType == Units.Type.Metric) ? value * Units.k_CmToM : value * Units.k_InToM;
        Debug.Log( "SettingsScreen: OnConduitDiameter() value: " + value );
    }

    private void OnRailSensitivity(float value)
    {
        Engine.cameraRailSensitivity = value;
        //Debug.Log( "SettingsScreen: OnRailSensitivity() value: " + value );
    }

    /// <summary>
    /// Called, for example, when Unit Mode changes from Metric to Standard.
    /// </summary>
    private void Reset()
    {

    }
}
