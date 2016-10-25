using UnityEngine;
using System.Collections;
using System;
using CB;

public class Level : MonoBehaviour, IAngleDevice
{
    private enum Orientation { Flat, Landscape }
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

    public Vector3 angles {
        get { return m_angles; }
    }

    // Area where bubble level will be instantiated and parented to
    public RectTransform    deviceSpawnView2D;

    // Bubble Level Object prefab
    public GameObject       bubbleLevelPrefab;

    public float            updateIntervalSec = 0.2f;

    //----------------------
    //     Private Data
    //----------------------
    private WeightedMean<Vector3>    m_weightedMean;

    private Orientation     m_orientation;

    // The Level Prefab Instance
    private GameObject      m_bubbleLevel;
    private RectTransform   m_bubbleLevelDisplay;
    private RectTransform   m_bubbleLevelBubble;

    // Angles along DEVICE Axis
    private Vector3         m_angles;
    private Vector2         m_bubbleRange;

    private float           m_bubbleRadius;
    private float           m_elapsedTime = 0f;

    void Awake()
    {
        m_weightedMean = new WeightedMean<Vector3>( 10,
            ( a, w ) => { return a * w; },
            ( a, b ) => { return a + b; } );
    }

    void OnEnable()
    {
        // Create Bubble Level
        m_bubbleLevel = Instantiate( bubbleLevelPrefab );
        m_bubbleLevel.transform.SetParent( deviceSpawnView2D, false );
        m_bubbleLevel.SetActive( true );

        m_bubbleLevelDisplay = (RectTransform)m_bubbleLevel.transform.Find( "display" );
        m_bubbleLevelBubble = (RectTransform)m_bubbleLevelDisplay.Find( "bubble" );

        var displaySize = m_bubbleLevelDisplay.rect.size;
        var bubbleSize = m_bubbleLevelBubble.rect.size;
        m_bubbleRange = new Vector2( displaySize.x * 0.5f - (bubbleSize.x * 0.5f) , displaySize.y * 0.5f - (bubbleSize.y * 0.5f) );
        m_bubbleRadius = m_bubbleRange.x;

        m_weightedMean.Clear();
    }

    void OnDisable()
    {
        // Destroy Level
        Destroy( m_bubbleLevel );
        m_bubbleLevelDisplay = null;
        m_bubbleLevelBubble = null;
    }

    void Update()
    {
        m_elapsedTime += Time.deltaTime;
        if(m_elapsedTime < updateIntervalSec) {
            return;
        }
        m_elapsedTime = 0f;

        // Map to correct axis

        // TODO - I really need to fix this. I have the UI of this app oriented facing down the -x axis which results in the 
        // Z axis being treated as the X axis, which is why they're swapped here.
        Vector3 acceleration = Input.acceleration;
                acceleration = new Vector3( acceleration.z, acceleration.y, acceleration.x );

        // Is device mostly flat, or mostly landscape?
        // Flat = acceleration along -X axis  [Display Y and X axis]
        // Landscape = acceleration along Y axis  [Display Z axis only]
        if (Vector3.Angle( acceleration, new Vector3( -1, 0, 0 ) ) < 75f) 
        {
            SetOrientation( Orientation.Flat );

            var yAccel = Vector3.ProjectOnPlane(acceleration, Vector3.forward);
            var xAccel = Vector3.ProjectOnPlane(acceleration, Vector3.up);
            // Because Acceleration along the x axis IS a rotation along the Y axis, and vice-versa
            var yAngle = 180f - Vector3.Angle(xAccel, Vector3.right);
            var xAngle = 180f - Vector3.Angle(yAccel, Vector3.right);

            m_angles = m_weightedMean.Add( new Vector3( xAngle, yAngle, 0f ) ).Mean();
            m_angles.x = Units.Round( angles.x, 1 );
            m_angles.y = Units.Round( angles.y, 1 );

            // Update Bubble
            var bubblePos = new Vector2(
                m_bubbleRange.x * Mathf.Min( yAngle / 30f, 1f ) * Mathf.Sign( acceleration.z ) * -1f,
                m_bubbleRange.y * Mathf.Min( xAngle / 30f, 1f ) * Mathf.Sign( acceleration.y ) * -1f
                );
            bubblePos = Vector3.ClampMagnitude( bubblePos, m_bubbleRadius );

            m_bubbleLevelBubble.anchoredPosition = bubblePos;
        } else {
            SetOrientation( Orientation.Landscape );

            var zAccel = Vector3.ProjectOnPlane(acceleration, Vector3.right);
            var zAngle = 180f - Vector3.Angle(zAccel, Vector3.up);

            m_angles = m_weightedMean.Add( new Vector3( 0f, 0f, zAngle ) ).Mean();
            m_angles.z = Units.Round( angles.z, 1 );

            // Update Bubble
            var bubblePos = new Vector2(
                Mathf.Sin( zAngle * Mathf.Deg2Rad ) * m_bubbleRadius * Mathf.Sign( acceleration.z ) * -1f,
                Mathf.Cos( zAngle * Mathf.Deg2Rad ) * m_bubbleRadius
                );

            m_bubbleLevelBubble.anchoredPosition = bubblePos;
        }
        // Fire event
        onAngleChange();
    }

    private void SetOrientation(Orientation orientation)
    {
        if (orientation != m_orientation) {
            m_weightedMean.Clear();
            m_orientation = orientation;
        }
    }
}
