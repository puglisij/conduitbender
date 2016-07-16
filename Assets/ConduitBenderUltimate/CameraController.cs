using UnityEngine;
using System.Collections;
using System;
using HedgehogTeam.EasyTouch;

[Serializable]
public class CameraController : MonoBehaviour
{
    public enum TiltDirection { Positive, Negative }

    const float k_Epsilon = 0.000001f;

    // Hierarchy should be:
    //      CameraRig 
    //          ->  CameraDolly
    //              ->  Camera
    public Transform    cameraRig;     
    public Transform    cameraDolly;
    public Camera       theCamera;

    public bool movementEnabled
    {
        get { return m_MovementEnabled; }
        set {
            m_MovementEnabled = value;
            if(value) {
                RegisterListeners();
            } else {
                UnRegisterListeners();
            }
        }
    }

    public float    railSensitivity      = 1f;
    public float    zoomRailSensitivity  = 1f;
    public float    tiltSensitivity = 1f;   // Degrees per Pixel
    public float    railSpeed       = 0.1f;
    public float    zoomRailSpeed   = 0.1f;
    public float    tiltSpeed       = 0.1f;         // Degrees per Sec
    public float    autoHomeSpeed   = 1f;           // Speed at which Camera returns to Home Positions. In Secs?

    //----------------------------
    //      Private Data
    //----------------------------
    private Transform   m_CameraTransform;

    [SerializeField]
    private Vector3 m_RailEnd       = Vector3.zero;     // In Local Space of Camera Rig (parent of Dolly) 
    private Vector3 m_RailPositive  = Vector3.zero;     // Normalized Vector in direction of start to end of main Rail
    private Vector3 m_DZTarget      = Vector3.zero;


    [SerializeField]
    private float   m_ZoomRailLength;
    [SerializeField]
    private float   m_ZoomRailOffset;
    [SerializeField]
    private float   m_ZoomRailHome   = 0f;  // Home Zoom Rail Positiom for Camera
    private float   m_ZoomRailTarget = 0f;  // Normalized Position along zoom rail we're moving towards

    [SerializeField]
    private float   m_RailHome      = 0f;   // Home Rail Position for Camera Dolly
    private float   m_RailTarget    = 0f;   // Normalized Position along rail we're moving towards
    private float   m_RailLength;           // Magnitude of main Rail

    [SerializeField]
    private float   m_TiltAngleHome  = 0f;  // Home Tilt Angle Position for Camera
    public float   m_tiltMaxAngle   = 0f;  // In Degrees 
    private float   m_DeltaRail     = 0f;
    private float   m_DeltaZoomRail = 0f;
    private float   m_DeltaTiltTotal = 0f;  // Total amount of Tilt Angle received from Input and Unused
    private float   m_TiltAngle     = 0f;   // Current Tilt Angle
    private float   m_TiltTarget    = 0f;   // Target Tilt Angle
    private float   m_DZInitHeight  = 0f;   // Frustrum Height at start of Dolly Zoom

    private float   m_AutoHomeStartTime = 0f;

    [SerializeField, HideInInspector]
    private int     m_TiltDirection = -1;   // -1 for Negative, 1 for Positive

    private bool    m_DZEnabled = false;        // Is Dolly Zoom Enabled
    private bool    m_MovementEnabled = true;   // User Control Enabled
    private bool    m_IsZooming = false;        // Is Camera Dolly moving along Dolly Zoom Rail
    private bool    m_IsRailing = false;        // Is Camera Dolly moving along main Rail
    private bool    m_IsTilting = false;        // Is Camera rotating along its X-Axis
    private bool    m_IsMovingHome = false;

	void Awake()
    {
        // Checks
#if UNITY_EDITOR
        if(theCamera == null) {
            Debug.LogError( "CameraController: Awake() Camera not found." );
            return;
        }
#endif

        m_CameraTransform = theCamera.transform;
        m_RailPositive = m_RailEnd.normalized;
        m_RailLength = m_RailEnd.magnitude;
    }

    void OnEnable()
    {

    }

	void Start () {
        // Register for Touch Gestures
        //InputManager.Register( InputManager.Gesture.DoubleDrag, DoubleDragHandler );

        
    }

    void Update ()
    {
        if (m_DZEnabled) {
            // Measure the new distance and readjust the FOV accordingly.
            var currDistance = Vector3.Distance( transform.position, m_DZTarget );
            theCamera.fieldOfView = FOVAtHeightDistance( m_DZInitHeight, currDistance );
        }

        // Are We Receiving User Inputs this Update?
        //float   deltaRail = 0f;
        //float   deltaZoomRail = 0f;
        bool    haveRailInput = false;
        bool    haveZoomInput = false;
        bool    haveTiltInput = false;
        bool    haveInput = false;

        if (m_MovementEnabled) 
        {
            //deltaRail = InputManager.GetHorizontalAxis() * Time.deltaTime * railSensitivity;
            //deltaZoomRail = InputManager.GetVerticalAxis() * Time.deltaTime * zoomRailSensitivity;

            haveRailInput = Mathf.Abs( m_DeltaRail ) > k_Epsilon;
            haveZoomInput = Mathf.Abs( m_DeltaZoomRail ) > k_Epsilon;
            haveTiltInput = Mathf.Abs( m_DeltaTiltTotal ) > k_Epsilon;
            haveInput = haveRailInput | haveZoomInput | haveTiltInput;
        }

        
        if (m_IsMovingHome) {
            // Was Auto Move Home Interrupted by User Input?
            if (haveInput) {

                SetMainRailPosition( GetNormalizedRailPosition() );
                SetZoomRailPosition( GetNormalizedZoomRailPosition() );
                SetCameraTiltPositionToCurrent();

                m_IsMovingHome = false;
            } else {
                MoveHome();
            }
        }


        //--------------------------
        //  Update Inputs
        //--------------------------
        if (haveInput) 
        {
            m_IsRailing = haveRailInput;
            m_IsZooming = haveZoomInput;
            m_IsTilting = haveTiltInput;

            if ( haveRailInput && Mathf.Abs( m_DeltaRail ) > Mathf.Abs( m_DeltaZoomRail ) ) { // Single Axis Movement
                m_RailTarget = Mathf.Clamp( m_RailTarget + m_DeltaRail, 0f, 1f );
                m_DeltaRail = 0f;
            } else if (haveZoomInput) {
                m_ZoomRailTarget = Mathf.Clamp( m_ZoomRailTarget + m_DeltaZoomRail, 0f, 1f );
                m_DeltaZoomRail = 0f;
            }

            if(haveTiltInput) {
                m_TiltTarget = Mathf.Clamp( m_TiltTarget + m_DeltaTiltTotal, 0f, m_tiltMaxAngle );
                m_DeltaTiltTotal = 0f;
            }

            //Debug.Log( "CameraController: deltaRail: " + deltaRail + " deltaZoomRail: " + deltaZoomRail 
            //    + " HorizontalAxis: " + InputManager.GetHorizontalAxis() + " VerticalAxis: " + InputManager.GetVerticalAxis() );
        }

        //-------------------
        // Update Movement
        //-------------------
        float deltaTime = Time.deltaTime;

        if (m_IsRailing) {
            MoveRail( deltaTime * railSpeed );
        }
        if (m_IsZooming) {
            MoveZoomRail( deltaTime * zoomRailSpeed );
        }
        if (m_IsTilting) {
            MoveTilt( deltaTime * tiltSpeed );
        }

    }

    void OnDrawGizmosSelected()
    {
        Color oldGizmoColor = Gizmos.color;

        // Draw Rail
        Gizmos.color = Color.white;
        Gizmos.DrawSphere( cameraRig.position, 0.15f );
        Gizmos.color = Color.white;
        Gizmos.DrawSphere( cameraRig.TransformPoint( m_RailEnd ), 0.15f );


        // Restore Color
        Gizmos.color = oldGizmoColor;
    }

    void RegisterListeners()
    {
        //EasyTouch.On_Drag += On_Drag;
        //EasyTouch.On_Drag2Fingers += On_Drag2Fingers;
        EasyTouch.On_Swipe += On_Drag;
        EasyTouch.On_Swipe2Fingers += On_Drag2Fingers;
        EasyTouch.On_PinchIn += On_PinchIn;
        EasyTouch.On_PinchOut += On_PinchOut;
    }
    void UnRegisterListeners()
    {
        //EasyTouch.On_Drag -= On_Drag;
        //EasyTouch.On_Drag2Fingers -= On_Drag2Fingers;
        EasyTouch.On_Swipe -= On_Drag;
        EasyTouch.On_Swipe2Fingers -= On_Drag2Fingers;
        EasyTouch.On_PinchIn -= On_PinchIn;
        EasyTouch.On_PinchOut -= On_PinchOut;
    }
    /*###########################################

                Movement Functions

    ############################################*/

    void MoveHome()
    {
        float deltaTime = Time.time - m_AutoHomeStartTime;
        float normalizedTime = deltaTime / autoHomeSpeed;

        if (deltaTime > autoHomeSpeed) {
            normalizedTime = 1f;
            m_IsMovingHome = false;
        }
        MoveRail( normalizedTime );
        MoveZoomRail( normalizedTime );
        MoveTilt( normalizedTime );
    }
    /// <summary>
    /// Move Dolly along Main Rail
    /// </summary>
    void MoveRail(float deltaTime)
    {
        // Local Positions
        Vector3 dollyTarget = m_RailPositive * m_RailTarget * m_RailLength;
        cameraDolly.localPosition = Vector3.Lerp( cameraDolly.localPosition, dollyTarget, deltaTime );

        if (Vector3.Distance( cameraDolly.localPosition, dollyTarget ) < k_Epsilon) {
            cameraDolly.localPosition = dollyTarget;
            m_IsRailing = false;
        }
    }

    /// <summary>
    /// Move Camera along Zoom Rail
    /// </summary>
    void MoveZoomRail(float deltaTime)
    {
        // Local Positions
        Vector3 dollyFwd = cameraDolly.InverseTransformDirection( cameraDolly.forward );
        Vector3 zoomStart = dollyFwd * m_ZoomRailOffset;
        Vector3 cameraTarget = zoomStart + (-dollyFwd * m_ZoomRailTarget * m_ZoomRailLength);
        theCamera.transform.localPosition = Vector3.Lerp( theCamera.transform.localPosition, cameraTarget, deltaTime );

        if (Vector3.Distance( theCamera.transform.localPosition, cameraTarget ) < k_Epsilon) {
            theCamera.transform.localPosition = cameraTarget;
            m_IsZooming = false;
        }
    }

    /// <summary>
    /// Tilt Camera
    /// </summary>
    void MoveTilt(float deltaTime)
    {
        m_TiltAngle = Mathf.Lerp( m_TiltAngle, m_TiltTarget, deltaTime );
        theCamera.transform.localRotation = Quaternion.Euler( m_TiltAngle * m_TiltDirection, 0f, 0f );

        if (Mathf.Abs( m_TiltTarget - m_TiltAngle ) < k_Epsilon) {
            theCamera.transform.localRotation = Quaternion.Euler( m_TiltTarget * m_TiltDirection, 0f, 0f );
            m_DeltaTiltTotal = 0f;
            m_IsTilting = false;
        }
    }

    /*##############################

            Input Functions

    ###############################*/

    //private void DoubleDragHandler( object sender, EventArgs e )
    //{
    //    DoubleDragGesture dg = (DoubleDragGesture) sender;

    //    if(dg.direction == DoubleDragGesture.Direction.Vertical) {
    //        Vector2 totalDragDelta = dg.currentCenterPosition - dg.startCenterPosition;
    //        float   totalDragDeltaDistance = totalDragDelta.y * tiltSensitivity;
    //        m_DeltaTiltTotal += totalDragDeltaDistance;
    //    }
    //    //Debug.Log( "CameraController: DoubleDragHandler() startCenter: " + dg.startCenterPosition + " currCenter: " + dg.currentCenterPosition
    //    //    + " deltaTime: " + dg.deltaTime + " direction: " + dg.direction + " m_DeltaTiltTotal: " + m_DeltaTiltTotal);
    //}

    private void On_Drag( Gesture gesture )
    {
        if (gesture.touchCount != 1) { return; }

        var normalDelta = Gesture.NormalizedPosition( gesture.deltaPosition );
        if (gesture.swipe == EasyTouch.SwipeDirection.Left || gesture.swipe == EasyTouch.SwipeDirection.Right) {
            m_DeltaRail += normalDelta.x * railSensitivity;
        } else if(gesture.swipe == EasyTouch.SwipeDirection.Up || gesture.swipe == EasyTouch.SwipeDirection.Down) {
            m_DeltaZoomRail += normalDelta.y * zoomRailSensitivity;
        }
    }

    private void On_Drag2Fingers( Gesture gesture )
    {
        if(gesture.touchCount != 2) { return; }

        m_DeltaTiltTotal += Gesture.NormalizedPosition( gesture.deltaPosition ).y * tiltSensitivity;
    }

    private void On_PinchIn( Gesture gesture )
    {
        if (Screen.orientation == ScreenOrientation.Landscape) {
            m_DeltaZoomRail += Gesture.NormalizedPinch( gesture.deltaPinch, false ) * zoomRailSensitivity;
        } else {
            m_DeltaZoomRail += Gesture.NormalizedPinch( gesture.deltaPinch ) * zoomRailSensitivity;
        }
    }

    private void On_PinchOut( Gesture gesture )
    {
        if(Screen.orientation == ScreenOrientation.Landscape) {
            m_DeltaZoomRail -= Gesture.NormalizedPinch( gesture.deltaPinch, false ) * zoomRailSensitivity;
        } else {
            m_DeltaZoomRail -= Gesture.NormalizedPinch( gesture.deltaPinch ) * zoomRailSensitivity;
        }
    }

    /*##############################

            Private Functions

    ###############################*/

    private float DistanceAtFrustumHeight( float frustrumHeight ) {
        return (frustrumHeight * 0.5f) / Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    }

    private float FOVAtHeightDistance( float frustrumHeight, float distance ) {
        return 2f * Mathf.Atan( frustrumHeight * 0.5f / distance ) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Get Frustum Height at distance of 'atWorldPoint' to Camera Position
    /// </summary>
    private float FrustumHeightAtPoint( Vector3 atWorldPoint)
    {
        Vector3 delta = atWorldPoint - theCamera.transform.position;
        return 2.0f * delta.magnitude * Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    }
    private float FrustumHeightAtDistance( float distance ) {
        return 2.0f * distance * Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    }

    private float GetFrustumWidth( float frustrumHeight ) {
        return frustrumHeight * theCamera.aspect;
    }
    
    private float GetFrustumHeight( float frustumWidth ) {
        return frustumWidth / theCamera.aspect;
    }

    /// <summary>
    /// Returns current normalized Position of Camera Dolly along main rail.
    /// </summary>
    private float GetNormalizedRailPosition()
    {
        Vector3 currDollyPos = cameraDolly.localPosition;
        return currDollyPos.magnitude / m_RailLength;
    }
    /// <summary>
    /// Returns current normalized Position of Camera along zoom rail.
    /// </summary>
    private float GetNormalizedZoomRailPosition()
    {
        Vector3 dollyFwd = cameraDolly.InverseTransformDirection( cameraDolly.forward );
        Vector3 zoomStart = dollyFwd * m_ZoomRailOffset;
        Vector3 currCameraPos = m_CameraTransform.localPosition;
        return (currCameraPos - zoomStart).magnitude / m_ZoomRailLength;
    }
    /// <summary>
    /// Returns current normalized tilt angle position of Camera
    /// </summary>
    private float GetNormalizedTiltAngle()
    {
        if( m_TiltDirection == -1 ) {
            return Mathf.Abs( 360f - m_CameraTransform.localRotation.eulerAngles.x ) / m_tiltMaxAngle;
        }
        return Mathf.Abs( m_CameraTransform.localRotation.eulerAngles.x ) / m_tiltMaxAngle;
    }

    private void StartDollyZoom()
    {
        var distance = Vector3.Distance(transform.position, m_DZTarget);
        m_DZInitHeight = FrustumHeightAtDistance( distance );
        m_DZEnabled = true;
    }

    private void StopDollyZoom()
    {
        m_DZEnabled = false;
    }


    /*##############################

            Public Functions

    ###############################*/


    public void EnableMovement(bool enabled)
    {
        m_MovementEnabled = enabled;
    }
    public void FitToView(BoxCollider bounds)
    {
        throw new NotImplementedException();
    }
    /// <summary>
    /// Moves Camera and Dolly to Home Positions
    /// </summary>
    public void MoveCameraHome()
    {
        // Stop any Current User Movement
        m_IsRailing = m_IsZooming = m_IsTilting = false;
        m_DeltaTiltTotal = 0f;

        // Set Destinations
        m_RailTarget = m_RailHome;
        m_ZoomRailTarget = m_ZoomRailHome;
        m_TiltTarget = m_TiltAngleHome;

        // Start AutoHome Coroutine
        m_AutoHomeStartTime = Time.time;
        m_IsMovingHome = true;
    }
    public void SetViewportRect(Rect rect)
    {
        theCamera.rect = rect;
    }
    public void SetPixelRect(Rect rect)
    {
        theCamera.pixelRect = rect;
    }
    /// <summary>
    /// Sets Camera Dolly to normalizedPosition (0 to 1) along the main Rail.
    /// </summary>
    public void SetMainRailPosition(float normalizedPosition)
    {
        normalizedPosition = Mathf.Clamp( normalizedPosition, 0f, 1f );
        m_RailTarget = normalizedPosition;

        Vector3 destination = m_RailPositive * normalizedPosition * m_RailLength;
        cameraDolly.localPosition = destination;
    }
    /// <summary>
    /// Sets Camera to normalizedPosition (0 to 1) along the Zoom Rail.
    /// </summary>
    public void SetZoomRailPosition(float normalizedPosition)
    {
        normalizedPosition = Mathf.Clamp( normalizedPosition, 0f, 1f );
        m_ZoomRailTarget = normalizedPosition;

        Vector3 dollyFwd = cameraDolly.InverseTransformDirection( cameraDolly.forward );
        Vector3 zoomStart = dollyFwd * m_ZoomRailOffset;
        Vector3 destination = zoomStart + (-dollyFwd * normalizedPosition * m_ZoomRailLength);
        theCamera.transform.localPosition = destination;
    }
    /// <summary>
    /// Sets Camera Tilt Angle to normalizedAngle (0 to 1) of its Max Tilt Angle
    /// </summary>
    public void SetCameraTiltPosition(float normalizedAngle)
    {
        normalizedAngle = Mathf.Clamp( normalizedAngle, 0f, 1f );

        m_TiltAngle = normalizedAngle * m_tiltMaxAngle;
        theCamera.transform.localRotation = Quaternion.Euler( m_TiltAngle * m_TiltDirection, 0f, 0f );
    }
    public void SetCameraTiltPositionToCurrent()
    {
        // @TODO Redundant Since m_TiltAngle is always equal to the current angle
        //float currAngle = m_CameraTransform.eulerAngles.x;
        //if (m_TiltDirection == -1) {
        //    currAngle = 360f - currAngle;
        //}

        //m_TiltAngle = currAngle;
        //theCamera.transform.localRotation = Quaternion.Euler( m_TiltAngle * m_TiltDirection, 0f, 0f );
        m_TiltTarget = m_TiltAngle;
    }

}
