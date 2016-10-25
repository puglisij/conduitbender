using UnityEngine;
using System.Collections;
using CB;

public class Protractor : MonoBehaviour, IAngleDevice
{
    private const float k_lowPassFactor = 10f;
    //----------------------
    //     Public Data
    //----------------------
    public event AngleDeviceEvent onAngleChange;

    public float xAngle {
        get { return m_angles.x; }
    }

    public float yAngle {
        get { return m_angles.y; }
    }

    public float zAngle {
        get { return m_angles.z; }
    }

    public Vector3 angles
    {
        get { return m_angles; }
    }

    // Spawn Location for pointer level cube
    public Transform        deviceSpawnPoint3D;

    // Pointer Object prefab for visual indicator of direction
    public GameObject       pointerCubePrefab;

    // Gyroscope update frequency
    public float            updateIntervalSec = 0.2f;

    //----------------------
    //     Private Data
    //----------------------
    private WeightedMean<Vector3>    m_weightedMean;

    // Reference to Input.gyro
    private Gyroscope       m_gyroscope;

    // The Protractor Prefab Instance
    private GameObject      m_pointerCube;

    // Protractor data
    private Quaternion      m_ZeroInverseRotation = Quaternion.identity;
    private Vector3         m_LastDeltaAngles = Vector3.zero;
    private Vector3         m_ZeroVectorUp = Vector3.up;
    private Vector3         m_ZeroVectorRight = Vector3.right;
    private Vector3         m_ZeroVectorForward = Vector3.forward;
    // Angles along DEVICE Axis
    private Vector3         m_angles;

    private float           m_elapsedTime = 0f;

    void Awake()
    {
        m_weightedMean = new WeightedMean<Vector3>( 10,
            ( a, w ) => { return a * w; },
            ( a, b ) => { return a + b; } );
    }

    void OnEnable()
    {
        m_gyroscope = Input.gyro;
        m_gyroscope.updateInterval = updateIntervalSec;  // Max is 0.0167f ?

        // Enable Gyroscope
        m_gyroscope.enabled = true;

        // Create Protractor Object
        m_pointerCube = Instantiate( pointerCubePrefab );
        m_pointerCube.transform.SetParent( Engine.root_geometry, false );
        m_pointerCube.transform.position = deviceSpawnPoint3D.position;
        m_pointerCube.SetActive( true );

        m_weightedMean.Clear();
    }

    void OnDisable()
    {
        // Disable Gyroscope
        m_gyroscope.enabled = false;

        // Destroy Protractor Object
        Destroy( m_pointerCube );
    }

    private Vector3 LowPassGyro( Vector3 newDeltaAngles )
    {
        m_LastDeltaAngles = Vector3.Lerp( m_LastDeltaAngles, newDeltaAngles, Time.deltaTime * k_lowPassFactor );
        return m_LastDeltaAngles;
    }

    void Update()
    {
        m_elapsedTime += Time.deltaTime;
        if (m_elapsedTime < updateIntervalSec) {
            return;
        }
        m_elapsedTime = 0f;

        /*
            The device's "Reference Frame" is the axis of the phone: 
            The phone's z-axis points through the screen. 
            The phone's x-axis points from top through bottom of the phone (speaker through home button)
            The phone's y-axis points from side to side through the phone (volume through power button)
        */
        Quaternion attitude;
        Quaternion deltaAttitude = Quaternion.identity;

        // Update Level Info   
        attitude = m_gyroscope.attitude;
        deltaAttitude = m_ZeroInverseRotation * attitude;

        // Rotate the Level Object
        m_pointerCube.transform.rotation = deltaAttitude;

        // Get Axis Angle changes from Zero Vector
        //Vector3    deltaQuaternionAngles = deltaAttitude.eulerAngles;
        Vector3    deltaVector = deltaAttitude * m_ZeroVectorRight;
        Vector3    deltaAngles = Vector3.zero;

        //deltaAngles.x = Vector3.Angle( m_ZeroVectorUp, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorUp, m_ZeroVectorForward ) );
        deltaAngles.y = Vector3.Angle( m_ZeroVectorRight, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorRight, m_ZeroVectorUp ).normalized );
        deltaAngles.z = Vector3.Angle( m_ZeroVectorForward, Vector3.ProjectOnPlane( deltaAttitude * m_ZeroVectorForward, m_ZeroVectorRight ).normalized );

        // Write Values to Output
        m_angles = new Vector3( 0f, Calculator.ClampAngle( deltaAngles.y, 1 ), Calculator.ClampAngle( deltaAngles.z, 1 ) );

        m_angles = m_weightedMean.Add( m_angles ).Mean();
        m_angles.y = Units.Round( m_angles.y, 1 );
        m_angles.z = Units.Round( m_angles.z, 1 );

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

        // Fire event
        onAngleChange();
    }

    /*==========================================

                Public Functions

    ==========================================*/
    /// <summary>
    /// Zero the Protractor
    /// </summary>
    public void Zero()
    {
        m_pointerCube.transform.LookAt( Engine.cameraUI.transform );

        m_ZeroInverseRotation = m_gyroscope.attitude;
        m_ZeroInverseRotation = Quaternion.Inverse( m_ZeroInverseRotation );

        m_ZeroVectorUp = m_pointerCube.transform.up;
        m_ZeroVectorRight = m_pointerCube.transform.right;
        m_ZeroVectorForward = m_pointerCube.transform.forward;

        m_weightedMean.Clear();
    }
}
