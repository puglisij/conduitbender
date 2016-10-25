using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CB;
using System;

/// <summary>
///     @TODO - May need a 1st Time Calibration, since axis may be represented differently on different devices.
/// </summary>
public class LevelScreen : AnimScreen
{
    public enum DeviceMode { Level, Protractor, None }

    private const string k_deviceModeKey = "Level_DeviceMode";

    //----------------------
    //     Public Data
    //----------------------
    public Level            level;
    public Protractor       protractor;

    // Button controls for leveling devices
    public GameObject       controls;
    // Object to be enabled when gyroscope/accelerometer not available on device
    public GameObject       unavailableMessage;
    // Parent Object holding all axis displays (used for disable/enable all displays at once)
    public GameObject       axisDisplayContainer;

    public GameObject       xAxis;
    public GameObject       yAxis;
    public GameObject       zAxis;
    public Text             xAxisText;
    public Text             yAxisText;
    public Text             zAxisText;

    public Button           levelBtn;
    public Button           protractorBtn;
    public Button           zeroBtn;

    public Color            deviceNormal;
    public Color            deviceSelected;
    public Color            deviceDisabled;
    //----------------------
    //     Private Data
    //----------------------
    private IAngleDevice             m_device;

    private DeviceMode      m_deviceMode = DeviceMode.None;

    private float           m_ElapsedSec = 0f;

    private bool            m_isDeviceActive = false;
    private bool            m_isViewDirty = true;

    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Start()
    {
        base.Start();

        var deviceMode = Settings.GetString( k_deviceModeKey );
        if(!String.IsNullOrEmpty(deviceMode)) {
            m_deviceMode = (DeviceMode)Enum.Parse( typeof( DeviceMode ), deviceMode );
        }

        // Register for Listeners on Level & Protractor
        ((IAngleDevice)level).onAngleChange += OnAngleChange;
        ((IAngleDevice)protractor).onAngleChange += OnAngleChange;
    }

    IEnumerator UpdateView()
    {
        while(m_isDeviceActive) 
        {
            if(m_isViewDirty) {
                DrawAngles( m_device.angles );

                m_isViewDirty = false;
            }

            yield return null;
        }
    }

    private void DrawAngles( Vector3 angles )
    {
        xAxisText.text = angles.x.ToString();
        yAxisText.text = angles.y.ToString();
        zAxisText.text = angles.z.ToString();
    }

    /// <summary>
    /// Disable/Enable UI Elements and enable and display Unavailable Message 
    /// </summary>
    private void DrawUnavailable(string message)
    {
        controls.SetActive( false );
        axisDisplayContainer.SetActive( false );
        unavailableMessage.GetComponentInChildren<Text>().text = message;
        unavailableMessage.SetActive( true );
    }

    private void OnAngleChange()
    {
        m_isViewDirty = true;
    }

    private void SetDevice( DeviceMode device )
    {
        if (device == DeviceMode.None) {
            return;
        }

        if (device == DeviceMode.Level) {
            protractorBtn.GetComponent<Image>().color = deviceNormal;
            levelBtn.GetComponent<Image>().color = deviceSelected;
            level.enabled = true;
            protractor.enabled = false;
            ToggleZero( false );

            m_isDeviceActive = true;
            m_device = level;
            
        } else if(device == DeviceMode.Protractor) {
            protractorBtn.GetComponent<Image>().color = deviceSelected;
            levelBtn.GetComponent<Image>().color = deviceNormal;
            level.enabled = false;
            protractor.enabled = true;
            ToggleZero( true );

            m_isDeviceActive = true;
            m_device = protractor;
        }

        // Start Coroutine to update angles
        StartCoroutine( UpdateView() );

        m_deviceMode = device;
        Settings.SetValue( k_deviceModeKey, m_deviceMode.ToString() );
    }

    private void SetDeviceDisabled( Button deviceBtn )
    {
        deviceBtn.interactable = false;
        deviceBtn.GetComponent<Image>().color = deviceDisabled;
    }
    private void ToggleZero( bool onOff )
    {
        zeroBtn.interactable = onOff;
        zeroBtn.GetComponentInChildren<Text>().color = (onOff) ? zeroBtn.colors.normalColor : deviceDisabled;
    }
    /*==========================================

                Public Functions

    ==========================================*/
    public override void Close( bool doDisable )
    {
        base.Close( doDisable );

        // Disable active device <Component>
        if(m_deviceMode == DeviceMode.Level) {
            level.enabled = false;
        } else if(m_deviceMode == DeviceMode.Protractor) {
            protractor.enabled = false;
        }
    }

    public override void Open()
    {
        base.Open();

        if (!SystemInfo.supportsAccelerometer && !SystemInfo.supportsGyroscope) {
            DrawUnavailable( "Sorry. Accelerometer and Gyroscope are unavailable on this device." );
            return;
        }
        if (!SystemInfo.supportsAccelerometer) {
            // Disable Level
            SetDeviceDisabled( levelBtn );
            DrawUnavailable( "Sorry. No accelerometer is available on this device." );
        }
        if (!SystemInfo.supportsGyroscope) {
            // Disable Protractor
            SetDeviceDisabled( protractorBtn );
            DrawUnavailable( "Sorry. No gyroscope is available on this device." );
        }

        // Select initial device mode if none set
        DeviceMode mode = m_deviceMode;
        if(m_deviceMode == DeviceMode.None) {
            mode = SystemInfo.supportsAccelerometer ? DeviceMode.Level : DeviceMode.Protractor;
        }

        SetDevice( mode );
    }

    /// <summary>
    /// Set to 0 for Digital Level. Set to 1 for Digital Protractor.
    /// </summary>
    public void SelectDevice( int device )
    {
        if(device == (int) m_deviceMode) {
            Debug.Log( "LevelScreen: Device mode: " + device + " already selected." );
            return;
        }

        SetDevice( (DeviceMode)device );
    }

    /// <summary>
    /// Pass through to Protractor
    /// </summary>
    public void Zero()
    {
        protractor.Zero();
    }
}
