using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

[Serializable]
public class CameraController : MonoBehaviour
{
    public enum TiltDirection { Positive = 1, Negative = -1 }

    /// <summary> To fix bug with EasyTouch where double drag, and single drag can be received at same time </summary>
    private enum GestureLock { None, Drag, DoubleDrag} 
    private enum DragLock { None, Vertical, Horizontal }

    /// <summary>
    /// Data about the Rail(s) itself
    /// </summary>
    [Serializable]
    private struct Rig {
        /// <summary> The root transform parent of the camera rig </summary>
        [SerializeField]
        public Transform rig;
        /// <summary> The transform of the camera dolly that moves along the main rail </summary>
        [SerializeField]
        public Transform dolly;

        /// <summary> Normalized Local Vector in the start -> end direction of the main Rail </summary>
        public Vector3 railPositive {
            get { return railEnd.normalized; }
        }
        /// <summary> The length of the main Rail </summary>
        public float railMagnitude {
            get { return railEnd.magnitude; }
        }
        /// <summary> Normalized Local Vector in the start -> end direction of the Zoom Rail </summary>
        public Vector3 zoomRailPositive {
            get { return -Vector3.Cross( railEnd, rig.InverseTransformDirection(rig.up) ).normalized; }
        }
        /// <summary> NormalizedHome Rail Position for Camera Dolly </summary>  
        [SerializeField, Range(0f, 1f)]
        public float   railHome;
        /// <summary> In Local Space of Camera Rig (parent of Dolly)  </summary>
        [SerializeField]
        public Vector3 railEnd;     
        /// <summary> Normalized Home Dolly/Zoom Rail Positiom for Camera </summary> 
        [SerializeField, Range(0f, 1f)]
        public float   zoomRailHome;
        /// <summary> Tangential Positive Offset of Dolly/Zoom Rail start from the main Rail </summary> 
        [SerializeField]
        public float   zoomRailOffset;
        /// <summary> The localized length of the Dolly/Zoom Rail </summary> 
        [SerializeField]
        public float   zoomRailMagnitude;
    }

    private struct Drag
    {
        public float time;
        public Vector3 localPosition;
    }

    const float k_Epsilon = 0.000001f;

    // Hierarchy should be:
    //      CameraRig 
    //          ->  CameraDolly
    //              ->  Camera
    public Camera       theCamera;

    /// <summary> Control the movement and speed behaviour when tweening to home positions.  </summary>
    public AnimationCurve autoHomeCurve;
    /// <summary> The normalized tilt home position. </summary>
    public float    tiltHome = 0f;
    /// <summary> The minimum angle at which the camera can tilt, in degrees. </summary>
    [Range(-90f, 90f)]
    public float    tiltMinAngle   = 0f;
    /// <summary> The maximum angle at which the camera can tilt, in degrees. </summary>
    [Range(-90f, 90f)]
    public float    tiltMaxAngle   = 0f;
    /// <summary> The direction the camera tilts. </summary>
    public TiltDirection tiltDirection;

    public float    decelerationRate = 0.135f;
    public float    elasticity = 0.1f;

    bool m_MovementEnabled = true;
    /// <summary> User control of camera is enabled </summary>
    public bool movementEnabled
    {
        get { return m_MovementEnabled; }
        set {
            m_MovementEnabled = value;
            if(value) {
                RegisterListeners();
            } else {
                UnRegisterListeners();
                Stop();
            }
        }
    }


    //----------------------------
    //      Private Data
    //----------------------------
    [SerializeField]
    Rig m_rig;

    Transform m_cameraTransform;
    Plane     m_cameraRailPlane;  // World space rig plane, with normal in direction of camera

    Vector3 m_startPositions;     // Normalized positions at start of gesture 
    Vector3 m_prevPositions;      // Normalized positions of the previous frame
    Vector3 m_velocities;         // X(Rail: Drag), Y(Zoom Rail: Drag), and Z(Tilt: Double Drag) axis velocities

    GestureLock m_gestureLock = GestureLock.None;
    Drag        m_startDrag;
    DragLock    m_dragLock;
    Queue<Drag> m_dragBuffer = new Queue<Drag>();

    bool    m_isDragging = false;
    bool    m_isMovingHome = false;

    //float   m_DZInitHeight  = 0f;   // Frustrum Height at start of Dolly Zoom
    //bool    m_DZEnabled = false;    // Is Dolly Zoom Enabled

    void Awake()
    {
        // Checks
#if UNITY_EDITOR
        if(theCamera == null) {
            Debug.LogError( "CameraController: Awake() Camera not found." );
            return;
        }
#endif

        Debug.Log( "CameraController: Awake() m_cameraRailPlane: " + m_cameraRailPlane.normal );
    }

    void Start()
    {
        m_cameraTransform = theCamera.transform;
        m_cameraRailPlane = new Plane( Vector3.right, m_rig.rig.position );
        m_prevPositions.x = GetNormalizedRailPosition();
        m_prevPositions.y = GetNormalizedZoomRailPosition();
        m_prevPositions.z = GetNormalizedTiltAngle();
    }

    void Update ()
    {
        //if (m_DZEnabled) {
        //    // Measure the new distance and readjust the FOV accordingly.
        //    var currDistance = Vector3.Distance( transform.position, m_DZTarget );
        //    theCamera.fieldOfView = FOVAtHeightDistance( m_DZInitHeight, currDistance );
        //}
        if(m_isMovingHome) {
            return;
        }

        float deltaTime = Time.unscaledDeltaTime;

        Vector3 positions;
                positions.x = GetNormalizedRailPosition();
                positions.y = GetNormalizedZoomRailPosition();
                positions.z = GetNormalizedTiltAngle();
        Vector3 offsets = CalculateOffset(positions);

        if (!m_isDragging && (offsets != Vector3.zero || m_velocities != Vector3.zero)) 
        {
            for(int axis = 0; axis < 3; ++axis)
            {               
                if(offsets[axis] != 0f) {
                    // Apply spring physics if camera position is offset from extents
                    float speed = m_velocities[axis];
                    positions[axis] = Mathf.SmoothDamp( positions[ axis ], positions[ axis ] + offsets[axis], ref speed, elasticity, Mathf.Infinity, deltaTime );
                    if (Mathf.Abs( speed ) < 0.0001f) {
                        speed = 0f;
                    }
                    m_velocities[axis] = speed;
                } else {
                    // Inertia 
                    m_velocities[axis] *= Mathf.Pow( decelerationRate, deltaTime );
                    if( Mathf.Abs(m_velocities[axis]) < 0.0001f ) {
                        m_velocities[axis] = 0f;
                    }
                    positions[axis] += m_velocities[axis] * deltaTime;
                } 
            }

            if(m_velocities.x != 0f) {
                SetMainRailPosition( positions.x );
            }
            if(m_velocities.y != 0f) {
                SetZoomRailPosition( positions.y );
            }
            if (m_velocities.z != 0f) {
               SetCameraTiltPosition( positions.z );
            }

        }

        if (m_isDragging) {
            // Inertia 
            float newVelocity;
            for(var axis = 0; axis < 3; ++axis) {
                newVelocity = (positions[ axis ] - m_prevPositions[ axis ]) / deltaTime;
                m_velocities[axis] = Mathf.Lerp( m_velocities[axis], newVelocity, deltaTime * 10f );
            }      
        }

        m_prevPositions = positions;
    }

    void OnDrawGizmosSelected()
    {
        Color oldGizmoColor = Gizmos.color;

        // Draw Rail
        Gizmos.color = Color.white;
        Gizmos.DrawSphere( m_rig.rig.position, 0.15f );
        Gizmos.color = Color.white;
        Gizmos.DrawSphere( m_rig.rig.TransformPoint( m_rig.railEnd ), 0.15f );


        // Restore Color
        Gizmos.color = oldGizmoColor;
    }

    void RegisterListeners()
    {
        //EasyTouch.On_DragStart += On_DragStart;
        //EasyTouch.On_Drag += On_Drag;
        EasyTouch.On_SwipeStart += On_DragStart;
        EasyTouch.On_Swipe += On_Drag;
        EasyTouch.On_SwipeEnd += On_DragEnd;
        EasyTouch.On_SwipeStart2Fingers += On_DragStart2Fingers;
        EasyTouch.On_Swipe2Fingers += On_Drag2Fingers;
        EasyTouch.On_SwipeEnd2Fingers += On_DragEnd2Fingers;
        //EasyTouch.On_PinchIn += On_PinchIn;
        //EasyTouch.On_PinchOut += On_PinchOut;
        //EasyTouch.On_PinchEnd += On_PinchEnd;
    }
    void UnRegisterListeners()
    {
        //EasyTouch.On_DragStart -= On_DragStart;
        //EasyTouch.On_Drag -= On_Drag;
        EasyTouch.On_SwipeStart -= On_DragStart;
        EasyTouch.On_Swipe -= On_Drag;
        EasyTouch.On_SwipeEnd -= On_DragEnd;
        EasyTouch.On_SwipeStart2Fingers -= On_DragStart2Fingers;
        EasyTouch.On_Swipe2Fingers -= On_Drag2Fingers;
        EasyTouch.On_SwipeEnd2Fingers -= On_DragEnd2Fingers;
        //EasyTouch.On_PinchIn -= On_PinchIn;
        //EasyTouch.On_PinchOut -= On_PinchOut;
        //EasyTouch.On_PinchEnd -= On_PinchEnd;
    }

    /*================================

            Input Functions

    ================================*/

    /// <summary>
    /// Calculates the elastic position. 
    /// 'startPos' is the normalized position on the rail at the start of the drag
    /// 'dragDelta' is the amount of drag to apply which is normalized by the 'magnitude' of the rail 
    /// 'sensitivity' is multipled by the 'dragDelta' to amplify the drag
    /// 'maxStretch' is the portion of 'magnitude' to stretch. It is divided by 'magnitude'
    /// </summary>
    float CalculateElasticPosition(float startPos, float dragDelta, float magnitude, float maxStretch, float sensitivity)
    {
        var delta = dragDelta / magnitude;
        var pos = startPos + delta * sensitivity;

        // Offset past extents
        var offset = (pos < 0f ? -pos : pos > 1f ? -(pos - 1f) : 0f);
        pos += offset;
        // Apply stretch
        if (offset != 0f) {
            pos -= RubberDelta( offset, maxStretch / magnitude ); 
        }

        return pos;
    }

    void On_DragStart( Gesture gesture )
    {
        if(m_gestureLock != GestureLock.None) {
            return;
        }
        var drag = new Drag();
        drag.time = Time.time;
        
        Engine.cameraUI.ScreenPointToWorldPosition( m_cameraRailPlane, gesture.startPosition, out drag.localPosition );
        drag.localPosition = transform.InverseTransformPoint( drag.localPosition );

        m_gestureLock = GestureLock.Drag;
        m_dragLock = DragLock.None;
        m_startDrag = drag;
        m_startPositions.x = GetNormalizedRailPosition();
        m_startPositions.y = GetNormalizedZoomRailPosition();
        m_isDragging = true;
    }

    void On_Drag( Gesture gesture )
    {
        if (m_gestureLock != GestureLock.Drag || gesture.touchCount != 1) { return; }

        // Example Inputs (where sw = screen widths)
        //      0.1 sw in 0.2 sec (slow drag)
        //      0.5 sw in 0.2 sec (medium drag)
        //      1 sw in 0.2 sec (fast drag)

        // Calculate drag position
        var drag = new Drag();
        drag.time = Time.time;
        Engine.cameraUI.ScreenPointToWorldPosition( m_cameraRailPlane, gesture.position, out drag.localPosition );
        drag.localPosition = transform.InverseTransformPoint( drag.localPosition );

        if (m_dragLock == DragLock.None) {
            m_dragLock = Mathf.Abs( gesture.deltaPosition.x ) > Mathf.Abs( gesture.deltaPosition.y ) ? DragLock.Horizontal : DragLock.Vertical;
        }

        if (m_dragLock == DragLock.Horizontal) {
            var railPos = CalculateElasticPosition(
                m_startPositions.x, 
                (drag.localPosition.x - m_startDrag.localPosition.x), 
                m_rig.railMagnitude, 
                0.5f,
                1f
            );

            SetMainRailPosition( railPos );
        } 
        else 
        {
            var zoomPos = CalculateElasticPosition(
                m_startPositions.y,
                (drag.localPosition.y - m_startDrag.localPosition.y),
                m_rig.zoomRailMagnitude,
                0.5f,
                1f
            );

            SetZoomRailPosition( zoomPos );
        }
    }

    void On_DragEnd( Gesture gesture )
    {
        m_gestureLock = GestureLock.None;
        m_isDragging = false;
    }


    void On_DragStart2Fingers( Gesture gesture)
    {
        if(m_gestureLock != GestureLock.None) {
            return;
        }
        var drag = new Drag();
        drag.time = Time.time;

        Engine.cameraUI.ScreenPointToWorldPosition( m_cameraRailPlane, gesture.startPosition, out drag.localPosition );
        drag.localPosition = transform.InverseTransformPoint( drag.localPosition );

        m_gestureLock = GestureLock.DoubleDrag;
        m_dragLock = DragLock.None;
        m_startDrag = drag;
        m_startPositions[2] = GetNormalizedTiltAngle();
        m_isDragging = true;
    }

    void On_Drag2Fingers( Gesture gesture )
    {
        if (m_gestureLock != GestureLock.DoubleDrag || gesture.touchCount != 2) { return; }

        // Calculate drag position
        var drag = new Drag();
        drag.time = Time.time;
        Engine.cameraUI.ScreenPointToWorldPosition( m_cameraRailPlane, gesture.position, out drag.localPosition );
        drag.localPosition = transform.InverseTransformPoint( drag.localPosition );

        // Calculate Tilt
        var magnitude = Mathf.Abs(tiltMaxAngle - tiltMinAngle);
        var tiltPos = CalculateElasticPosition(
                m_startPositions[2],
                (drag.localPosition.y - m_startDrag.localPosition.y),
                magnitude,
                0.05f * magnitude,
                10f
            );

        SetCameraTiltPosition( tiltPos );
    }

    void On_DragEnd2Fingers( Gesture gesture )
    {
        m_gestureLock = GestureLock.None;
        m_isDragging = false;
    }

    //void On_PinchIn( Gesture gesture )
    //{
    //    var nDeltaPinch = Gesture.NormalizedPinch( gesture.deltaPinch, false );
    //    m_targets.z -= nDeltaPinch * Engine.cameraZoomSensitivity;
    //}

    //void On_PinchOut( Gesture gesture )
    //{
    //    var nDeltaPinch = Gesture.NormalizedPinch( gesture.deltaPinch, false );
    //    m_targets.z += nDeltaPinch * Engine.cameraZoomSensitivity;
    //}

    //void On_PinchEnd( Gesture gesture )
    //{       
    //}

    /*================================

            Private Functions

    ================================*/

    // For elasticity
    static float RubberDelta( float overStretching, float maxExtent )
    {
        return (1 - (1 / ((Mathf.Abs( overStretching ) * 0.55f / maxExtent) + 1))) * maxExtent * Mathf.Sign( overStretching );
    }

    //float DistanceAtFrustumHeight( float frustrumHeight ) {
    //    return (frustrumHeight * 0.5f) / Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    //}

    float FOVAtHeightDistance( float frustrumHeight, float distance ) {
        return 2f * Mathf.Atan( frustrumHeight * 0.5f / distance ) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Get Frustum Height at distance of 'atWorldPoint' to Camera Position
    /// </summary>
    //float FrustumHeightAtPoint( Vector3 atWorldPoint)
    //{
    //    Vector3 delta = atWorldPoint - theCamera.transform.position;
    //    return 2.0f * delta.magnitude * Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    //}

    float FrustumHeightAtDistance( float distance ) {
        return 2.0f * distance * Mathf.Tan( theCamera.fieldOfView * 0.5f * Mathf.Deg2Rad );
    }

    //float GetFrustumWidth( float frustrumHeight ) {
    //    return frustrumHeight * theCamera.aspect;
    //}
    
    //float GetFrustumHeight( float frustumWidth ) {
    //    return frustumWidth / theCamera.aspect;
    //}

    Vector3 CalculateOffset(Vector3 normalizedPos)
    {
        Vector3 offset = Vector3.zero;

        for(var axis = 0; axis < 3; ++axis) {
            offset[axis] = (normalizedPos[ axis] < 0f ? -normalizedPos[ axis ] : normalizedPos[ axis ] > 1f ? -(normalizedPos[ axis ] - 1f) : 0f);
        }

        return offset;
    }

    /// <summary>
    /// Returns current normalized Position of Camera Dolly along main rail.
    /// </summary>
    float GetNormalizedRailPosition()
    {
        Vector3 currDollyPos = m_rig.dolly.localPosition;
        return (currDollyPos.magnitude / m_rig.railMagnitude) * Mathf.Sign( Vector3.Dot(currDollyPos, m_rig.railEnd) );
    }
    /// <summary>
    /// Returns current normalized Position of Camera along zoom rail.
    /// </summary>
    float GetNormalizedZoomRailPosition()
    {
        Vector3 zoomFwd = m_rig.zoomRailPositive;
        Vector3 zoomStart = zoomFwd * m_rig.zoomRailOffset; // Assume zero is local origin
        Vector3 cameraPos = m_cameraTransform.localPosition;
        Vector3 cameraDelta = (cameraPos - zoomStart);

        return (cameraDelta.magnitude / m_rig.zoomRailMagnitude) * Mathf.Sign( Vector3.Dot(cameraDelta, zoomFwd) );
    }
    /// <summary>
    /// Returns current normalized tilt angle position of Camera 
    /// NOTE: Handles Euler range 0 to 360 degrees
    /// </summary>
    float GetNormalizedTiltAngle()
    {
        float angle = m_cameraTransform.localRotation.eulerAngles.x;
              angle = angle > 180f ? -(360f - angle) : angle;
        float mag = Mathf.Abs(tiltMaxAngle - tiltMinAngle);
        float pos = Mathf.Abs(angle - tiltMinAngle) / mag;

        if(tiltDirection == TiltDirection.Positive) {
            pos *= angle < tiltMinAngle ? -1f : 1f;
        } else {
            pos *= angle > tiltMinAngle ? -1f : 1f;
        }

        return pos;
    }

    /// <summary>
    /// Sets Camera Dolly to normalizedPosition (0 to 1) along the main Rail.
    /// </summary>
    void SetMainRailPosition( float normalizedPosition )
    {
        Vector3 destination = m_rig.railPositive * Mathf.Clamp( normalizedPosition, -0.5f, 1.5f ) * m_rig.railMagnitude;
        m_rig.dolly.localPosition = destination;
    }
    /// <summary>
    /// Sets Camera to normalizedPosition (0 to 1) along the Zoom Rail.
    /// </summary>
    void SetZoomRailPosition( float normalizedPosition )
    {
        Vector3 zoomFwd = m_rig.zoomRailPositive;
        Vector3 zoomStart = zoomFwd * m_rig.zoomRailOffset;
        Vector3 pos = zoomStart + (zoomFwd * Mathf.Clamp( normalizedPosition, -0.5f, 1.5f ) * m_rig.zoomRailMagnitude);

        theCamera.transform.localPosition = pos;
    }
    /// <summary>
    /// Sets Camera Tilt Angle to normalizedAngle (0 to 1) of its Max Tilt Angle
    /// </summary>
    void SetCameraTiltPosition( float normalizedAngle )
    {
        float mag = Mathf.Abs(tiltMaxAngle - tiltMinAngle);
        float delta = Mathf.Clamp( normalizedAngle, -0.5f, 1.5f ) * mag;
        float pos = tiltMinAngle + (int)tiltDirection * delta;

        theCamera.transform.localRotation = Quaternion.Euler( pos, 0f, 0f );
    }

    /*##############################

            Public Functions

    ###############################*/

    //public void FitToView(BoxCollider bounds)
    //{
    //    throw new NotImplementedException();
    //}

    IEnumerator TweenHome()
    {
        m_isMovingHome = true;
        UnRegisterListeners();

        float startRailPos = GetNormalizedRailPosition(),
              startZoomPos = GetNormalizedZoomRailPosition(),
              startTiltPos = GetNormalizedTiltAngle();
        float deltaTime,
              time = Time.time,
              startTime = time,
              endTime = startTime + autoHomeCurve.keys[ autoHomeCurve.length - 1 ].time;

        while(time < endTime) 
        {
            deltaTime = time - startTime;

            float tweenDelta = autoHomeCurve.Evaluate( deltaTime );

            SetMainRailPosition( startRailPos + (m_rig.railHome - startRailPos) * tweenDelta );
            SetZoomRailPosition( startZoomPos + (m_rig.zoomRailHome - startZoomPos) * tweenDelta );
            SetCameraTiltPosition( startTiltPos + (tiltHome - startTiltPos) * tweenDelta );

            yield return new WaitForEndOfFrame();
            time = Time.time;
        }

        if(movementEnabled) {
            RegisterListeners();
        }
        m_isMovingHome = false;   
    }

    /// <summary>
    /// Moves Camera and Dolly to Home Positions
    /// </summary>
    public void MoveCameraHome()
    {
        // Start AutoHome Coroutine
        StartCoroutine( TweenHome() );
    }

    void Stop()
    {
        m_velocities = Vector3.zero;
    }

    //public void SetViewportRect(Rect rect)
    //{
    //    theCamera.rect = rect;
    //}

    //public void SetPixelRect(Rect rect)
    //{
    //    theCamera.pixelRect = rect;
    //}

}
