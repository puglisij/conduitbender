using UnityEngine;
using System.Collections;
using CB;

public class Protractor : MonoBehaviour, IAngleDevice
{
    const float k_lowPassFactor = 10f;


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
    public GameObject       protractorPrefab;

    // Gyroscope update frequency
    public float            updateIntervalSec = 0.2f;

    //----------------------
    //     Private Data
    //----------------------
    WeightedMean<Vector3>    m_weightedMean;

    // Reference to Input.gyro
    Gyroscope       m_gyroscope;

    // The Protractor Prefab Instance
    GameObject      m_protractor;

    // Protractor data
    Quaternion      m_zeroInverseRotation = Quaternion.identity;
    Vector3         m_LastDeltaAngles = Vector3.zero;
    // Angles along DEVICE Axis
    Vector3         m_angles;

    float           m_elapsedTime = 0f;

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
        m_protractor = Instantiate( protractorPrefab );
        m_protractor.transform.SetParent( Engine.root_geometry, false );
        m_protractor.transform.position = deviceSpawnPoint3D.position;
        m_protractor.SetActive( true );

        m_weightedMean.Clear();
    }

    void OnDisable()
    {
        // Disable Gyroscope
        m_gyroscope.enabled = false;

        // Destroy Protractor Object
        Destroy( m_protractor );
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

        // Map to correct axis

        // TODO - I really need to fix this. I have the UI of this app oriented facing down the -x axis which results in the 
        // Z axis being treated as the X axis, which is why they're swapped here.
        Vector3 acceleration = Input.acceleration;
        acceleration = new Vector3( acceleration.z, acceleration.y, acceleration.x );

        /*
            The device's "Reference Frame" is the axis of the phone: 
            The phone's z-axis points through the screen. 
            The phone's x-axis points from top through bottom of the phone (speaker through home button)
            The phone's y-axis points from side to side through the phone (volume through power button)
        */
        Quaternion attitude = m_gyroscope.attitude;
        Quaternion deltaAttitude = m_zeroInverseRotation * attitude;

        // Get Axis Angle changes from Zero Vector
        Vector3    deltaAngles = Vector3.zero;

        deltaAngles.x = Vector3.Angle( Vector3.up, Vector3.ProjectOnPlane( deltaAttitude * Vector3.up, Vector3.right ) );
        deltaAngles.y = Vector3.Angle( Vector3.forward, Vector3.ProjectOnPlane( deltaAttitude * Vector3.forward, Vector3.up ) );
        deltaAngles.z = Vector3.Angle( Vector3.right, Vector3.ProjectOnPlane( deltaAttitude * Vector3.right, Vector3.forward ) );

        // Write Values to Output
        m_angles = new Vector3( Calculator.ClampAngle( deltaAngles.x, 1 ), Calculator.ClampAngle( deltaAngles.y, 1 ), Calculator.ClampAngle( deltaAngles.z, 1 ) );

        m_angles = m_weightedMean.Add( m_angles ).Mean();
        m_angles.x = Units.Round( m_angles.x, 1 );
        m_angles.y = Units.Round( m_angles.y, 1 );
        m_angles.z = Units.Round( m_angles.z, 1 );


        int maxAxis = 0;
        for(int axis = 1; axis < 3; ++axis) {
            if(Mathf.Abs( m_angles[axis] ) > Mathf.Abs( m_angles[maxAxis] )) {
                maxAxis = axis;
            }
        }

        if(maxAxis == 0) {
            // X
            m_protractor.transform.rotation = Quaternion.Euler( 0f, 180f, 90f );
        } else if(maxAxis == 1) {
            // Y
            m_protractor.transform.rotation = Quaternion.Euler( 0f, 180f, 0f );
        } else {
            // Z
            m_protractor.transform.rotation = Quaternion.Euler( -90f, 180f, 0f );
        }

        //------------------------------------------------------------------------
        // NOTE: Keep This. It's Accurate and Useful for Debugging
        //------------------------------------------------------------------------
        // Vector3    deltaQuaternionAngles = deltaAttitude.eulerAngles;
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
        m_protractor.transform.LookAt( Engine.cameraUI.transform );

        m_zeroInverseRotation = m_gyroscope.attitude;
        m_zeroInverseRotation = Quaternion.Inverse( m_zeroInverseRotation );

        m_weightedMean.Clear();
    }
}
