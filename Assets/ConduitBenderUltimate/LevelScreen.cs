using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using CB;

/// <summary>
///     @TODO - May need a 1st Time Calibration, since axis may be represented differently on different devices.
/// </summary>
public class LevelScreen : AnimScreen
{
    private enum Axis { X, Y, Z, None }

    public RectTransform    controls;
    public RectTransform    axisDisplays;
    public RectTransform    unavailableMessage;

    public GameObject       levelObjectPrefab;
    public Transform        levelObjectPoint;

    public Image            yAxisBgImage;
    public Image            zAxisBgImage;
    public Text             yAxisText;
    public Text             zAxisText;
    public Text             debugText;

    public float            updateIntervalSec = 0.5f;

    //----------------------
    //     Private Data
    //----------------------
    private GameObject      m_LevelObject;

    private Quaternion      m_ZeroInverseRotation = Quaternion.identity;

    private Vector3         m_LastDeltaAngles = Vector3.zero;
    private Vector3         m_ZeroAngles = Vector3.zero;
    private Vector3         m_ZeroVectorUp = Vector3.up;
    private Vector3         m_ZeroVectorRight = Vector3.right;
    private Vector3         m_ZeroVectorForward = Vector3.forward;

    private WeightedMean<Vector3>    m_weightedMean;

    private float           m_ElapsedSec = 0f;
    private float           m_LowPassFactor = 10f;

    private bool            m_UsingGravity = true;
    private bool            m_Off = true;
    private Axis            m_CurrGreatestAxisRotation = Axis.None;
     
    protected override void Awake()
    {
        base.Awake();

        m_weightedMean = new WeightedMean<Vector3>( 10,
            ( a, w ) => { return a * w; },
            ( a, b ) => { return a + b; } );
    }

    void Update()
    {
        if (m_Off) { return; }

        m_ElapsedSec += Time.deltaTime;
        if (m_ElapsedSec >= updateIntervalSec) 
        {
            /*
                The device's "Reference Frame" is the axis of the phone: 
                The phone's z-axis points through the screen. 
                The phone's x-axis points from top through bottom of the phone (speaker through home button)
                The phone's y-axis points from side to side through the phone (volume through power button)
            */
            Quaternion attitude;
            Quaternion deltaAttitude = Quaternion.identity;
            if (m_UsingGravity) 
            {
                Vector3 gravity = Input.gyro.gravity;
                gravity = new Vector3( gravity.z, gravity.y, gravity.x );
                // x, y, z
                // x, z, y
                // z, y, x
                // z, x, y
                // y, x, z
                // y, z, x
                // 3/1/2016 Left Off:
                //  Try recording the gyro.attitude as the zero on gravity with the Level object rotated by this attitude
                //  Then, inside here, check the rotation between level object (e.g. forward vector) and the starting gravity direction....
                deltaAttitude = Quaternion.identity;

                // Point Level in Direction of Gravity
                m_LevelObject.transform.LookAt( m_LevelObject.transform.position + gravity.normalized );

            } else {
                // Update Level Info   
                attitude = Input.gyro.attitude;
                deltaAttitude = m_ZeroInverseRotation * attitude;

                // Rotate the Level Object
                m_LevelObject.transform.rotation = deltaAttitude;
            }

            // Get Axis Angle changes from Zero Vector
            //Vector3    deltaQuaternionAngles = deltaAttitude.eulerAngles;
            Vector3    deltaVector = deltaAttitude * m_ZeroVectorRight;
            Vector3    deltaAngles = Vector3.zero;

            //deltaAngles.x = Vector3.Angle( m_ZeroVectorUp, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorUp, m_ZeroVectorForward ) );
            deltaAngles.y = Vector3.Angle( m_ZeroVectorRight, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorRight, m_ZeroVectorUp).normalized );
            deltaAngles.z = Vector3.Angle( m_ZeroVectorForward, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorForward, m_ZeroVectorRight ).normalized );

            // Run through Low-Pass Filter
            deltaAngles = m_weightedMean.Add( deltaAngles ).Mean();
            //deltaAngles = LowPassGyro( deltaAngles );

            // Write Values to Output
            DrawAngles( 0f, ClampAngle( deltaAngles.y, 1 ), ClampAngle( deltaAngles.z, 1 ) );

            //------------------------------------------------------------------------
            // NOTE: Keep This. It's Accurate and Useful for Debugging
            //------------------------------------------------------------------------
            // Quaternion Representing Amount of Rotation Between Two Quaternions:
            //      deltaQuaternion = Quaternion.Inverse( fromQuaternion ) * toQuaternion;
            //debugText.text = "Quaternion Delta Angles:"
            //    + "\nX - " + ClampAngle( deltaQuaternionAngles.x, 1 )
            //    + "\nY - " + ClampAngle( deltaQuaternionAngles.y, 1 )
            //    + "\nZ - " + ClampAngle( deltaQuaternionAngles.z, 1 );
            //------------------------------------------------------------------------

            // Reset Elapsed Time
            m_ElapsedSec = 0f;
        }
    }

    /// <summary>
    /// Clamp Angle to positive 0 to 180 degrees
    /// </summary>
    private float ClampAngle(float angle, int decimalPrecision)
    {
        angle -= (180f * Mathf.Floor(angle / 180f));
        return Units.Round( angle * Mathf.Sign( angle ), 1);
    }

    private void Available(bool isAvailable)
    {
        // Disable/Enable UI Elements
        controls.gameObject.SetActive( isAvailable );
        axisDisplays.gameObject.SetActive( isAvailable );
        unavailableMessage.gameObject.SetActive( !isAvailable );

        m_Off = !isAvailable;
    }

    private void DrawAngles(float x, float y, float z)
    {
        if(y > z && m_CurrGreatestAxisRotation != Axis.Y) {
            //yAxisText.fontStyle = FontStyle.Bold;
            //zAxisText.fontStyle = FontStyle.Normal;
            yAxisBgImage.enabled = true;
            zAxisBgImage.enabled = false;
            m_CurrGreatestAxisRotation = Axis.Y;
        } else if(z > y && m_CurrGreatestAxisRotation != Axis.Z) {
            //yAxisText.fontStyle = FontStyle.Normal;
            //zAxisText.fontStyle = FontStyle.Bold;
            yAxisBgImage.enabled = false;
            zAxisBgImage.enabled = true;
            m_CurrGreatestAxisRotation = Axis.Z;
        } 
        yAxisText.text = y.ToString();
        zAxisText.text = z.ToString();
    }


    private Vector3 LowPassGyro(Vector3 newDeltaAngles)
    {
        m_LastDeltaAngles = Vector3.Lerp( m_LastDeltaAngles, newDeltaAngles, Time.deltaTime * m_LowPassFactor );
        return m_LastDeltaAngles;
    }

    /*##########################################

                Public Functions

    ###########################################*/
    public override void Close( bool doDisable )
    {
        base.Close( doDisable );

        // Disable Gyroscope
        Input.gyro.enabled = false;
        m_Off = true;

        // Destroy Level Object
        Destroy( m_LevelObject );
    }
    public override void Open()
    {
        base.Open();

        if (!SystemInfo.supportsGyroscope) {
            Available( false );
            return;
        }
        Available( true );

        // Enable Gyroscope
        Input.gyro.updateInterval = updateIntervalSec;  // Max is 0.0167f ?
        Input.gyro.enabled = true;
        m_Off = false;

        // Create Level Object
        m_LevelObject = Instantiate( levelObjectPrefab );
        m_LevelObject.transform.SetParent( Engine.root_geometry, false );
        m_LevelObject.transform.position = levelObjectPoint.position;
        m_LevelObject.SetActive( true );

        m_weightedMean.Clear();

        ZeroOnGravity();
    }

    public void ZeroOnGravity()
    {
        m_UsingGravity = true;

        Vector3 gravity = Input.gyro.gravity;

        m_LevelObject.transform.forward = gravity;

        m_ZeroInverseRotation = m_LevelObject.transform.rotation;
        m_ZeroAngles = m_ZeroInverseRotation.eulerAngles;
        m_ZeroInverseRotation = Quaternion.Inverse( m_ZeroInverseRotation );

        m_ZeroAngles = Vector3.zero;

        m_ZeroVectorUp = m_LevelObject.transform.up;
        m_ZeroVectorRight = m_LevelObject.transform.right;
        m_ZeroVectorForward = m_LevelObject.transform.forward;
    }

    /// <summary>
    /// Zero the Level
    /// </summary>
    public void Zero()
    {
        m_UsingGravity = false;

        m_LevelObject.transform.LookAt( Engine.cameraUI.transform );

        m_ZeroInverseRotation = Input.gyro.attitude;
        m_ZeroAngles = m_ZeroInverseRotation.eulerAngles;
        m_ZeroInverseRotation = Quaternion.Inverse( m_ZeroInverseRotation );

        m_ZeroVectorUp = m_LevelObject.transform.up;
        m_ZeroVectorRight = m_LevelObject.transform.right;
        m_ZeroVectorForward = m_LevelObject.transform.forward;
    }

}
