

namespace CB
{
    namespace Input
    {
        using UnityEngine;
        using System.Collections;
        using System.Collections.Generic;
        using System;

        /// <summary>
        /// Possible states of a Gesture
        /// </summary>
        public enum GestureState
        {
            Possible,
            Began,
            Changed,
            Ended,
            Cancelled,
            Failed,
            Recognized = Ended
        }

        public abstract class AGesture
        {
            public GestureState     state;
            public List<TouchPoint> touchPoints = new List<TouchPoint>();
            public event EventHandler<EventArgs> observers;

            internal virtual void OnTouches(TouchPoint[] touchPoint) { }
            protected virtual void NotifyObservers()
            {
                observers( this, EventArgs.Empty );
            }
        }

        public class DoubleDragGesture : AGesture
        {
            public enum Direction { Horizontal, Vertical }
            /// <summary>
            /// Amount of Movement from Starting Center Position
            /// </summary>
            public Vector2      startCenterPosition;
            public Vector2      currentCenterPosition;
            /// <summary>
            /// Current Direction of Drag
            /// </summary>
            public Direction    direction;
            /// <summary>
            /// Time Passage since Start of Gesture
            /// </summary>
            public float        deltaTime = 0f;

            private int finger0_id = 0;
            private int finger1_id = 1;

            internal override void OnTouches( TouchPoint[] touchPoint )
            {
                if(touchPoint.Length == 2) 
                {
                    // Has Gesture been Recognized?
                    if(state == GestureState.Began || state == GestureState.Changed) {
                        state = GestureState.Changed;
                        // Are Finger ID's same?
                        int f0_id = touchPoint[0].fingerId;
                        int f1_id = touchPoint[1].fingerId;
                        if ((finger0_id == f0_id || finger0_id == f1_id) && (finger1_id == f1_id || finger1_id == f0_id)) {
                            // Is Direction the Same?
                            Vector2 p1 = touchPoint[0].position;
                            Vector2 p2 = touchPoint[1].position;
                            currentCenterPosition = p1 + (p2 - p1) * 0.5f;

                            Vector2 delta = currentCenterPosition - startCenterPosition;
                            if(Mathf.Abs(delta.y) > Mathf.Abs(delta.x)) {
                                direction = Direction.Vertical;
                            } else {
                                direction = Direction.Horizontal;
                            }
                            deltaTime = touchPoint[ 0 ].deltaTime; // Works?

                            // Notify Observers
                            NotifyObservers();
                        }
                    } else {
                        //--------------------
                        // Gesture Started
                        //--------------------
                        state = GestureState.Began;
                        finger0_id = touchPoint[ 0 ].fingerId;
                        finger1_id = touchPoint[ 1 ].fingerId;

                        // Determine Centerpoint
                        Vector2 p1 = touchPoint[0].position;
                        Vector2 p2 = touchPoint[1].position;
                        startCenterPosition = p1 + (p2 - p1) * 0.5f;
                        currentCenterPosition = startCenterPosition;
                        deltaTime = 0f;

                        // Notify Observers
                        NotifyObservers();
                    }
                } else 
                {
                    if (state == GestureState.Began || state == GestureState.Changed) {
                        state = GestureState.Ended;
                        // Notify Observers
                        NotifyObservers();
                    }
                }
            }
        }

        public struct TouchPoint
        {
            public Vector2 deltaPosition;
            public Vector2 position;

            public int fingerId;
            public int tapCount;

            public float deltaTime;

            public TouchPhase phase;

            public TouchPoint( TouchPhase phase, float deltaTime, int tapCount, int fingerId, Vector2 position, Vector2 deltaPosition )
            {
                this.phase = phase;
                this.deltaTime = deltaTime;
                this.tapCount = tapCount;
                this.fingerId = fingerId;
                this.position = position;
                this.deltaPosition = deltaPosition;
            }
            public void Set( TouchPhase phase, float deltaTime, int tapCount, int fingerId, Vector2 position, Vector2 deltaPosition )
            {
                this.phase = phase;
                this.deltaTime = deltaTime;
                this.tapCount = tapCount;
                this.fingerId = fingerId;
                this.position = position;
                this.deltaPosition = deltaPosition;
            }
        }

        /// <summary>
        /// Manages Touch Inputs, including Simulating Touches with the Mouse
        /// </summary>
        public class InputManager : MonoBehaviour
        {
            public enum Gesture { DoubleDrag, Drag, Flick, LongPress, Press, Release, Tap }

            public int touchCount
            {
                get { return m_TouchCount; }
            }

            [HideInInspector]
            public InputManager instance = null;

            [Tooltip("Maximum Time in which to detect a Tap")]
            public float tapThresholdSec = 0.8f;
            [Tooltip("Maximum Time between Taps to detect a Multi-Tap")]
            public float tapMultiThresholdSec = 0.5f;

            //---------------------
            // Private Data
            //---------------------
            private static List<AGesture> m_Gestures = new List<AGesture>();

            private static TouchPoint[] m_ActiveTouches = new TouchPoint[10];
            private static float[]      m_ActiveTouchTimes = new float[10];    // Time signature on Mouse Touch 'Press'/'Release'

            private static int   m_TouchCount = 0;      // Number of Touches in the m_ActiveTouches array that are Active

            private static float m_Horizontal = 0f;
            private static float m_Vertical = 0f;



            void Awake()
            {
                // Singleton
                if (instance == null) {
                    instance = this;
                } else if (instance != this) {
#if UNITY_EDITOR
                    Debug.LogError( "InputManager: Awake() Only one instance of InputManager should exist in the scene." );
#endif
                    Destroy( gameObject );
                    return;
                }

                // Add Global Gestures
                m_Gestures.Insert( (int)Gesture.DoubleDrag, new DoubleDragGesture() );

            }
            void OnEnable()
            {
                //Debug.Log("InputManager: OnEnable()");
            }

            void Update()
            {
#if MOBILE_INPUT
                // Copy Touches to Local Struct Array (We can't create instances of Unity 'Touch')
                m_TouchCount = Input.touchCount;
                var touches = Input.touches;
                Touch      touch;
                TouchPoint touchPoint;

                for(int t = 0; t < touches.Length; ++t) {
                    touch = touches[ t ];
                    touchPoint = m_ActiveTouches[ t ];
                    touchPoint.deltaPosition = touch.deltaPosition;
                    touchPoint.position = touch.position;
                    touchPoint.phase = touch.phase;
                    touchPoint.fingerId = touch.fingerId;
                    touchPoint.tapCount = touch.tapCount;
                    touchPoint.deltaTime = touch.deltaTime;

                    m_ActiveTouches[ t ] = touchPoint;
                }

                //Vector2 debugPoint = Vector2.zero;
                if (m_TouchCount == 1) {
                    m_Horizontal = m_ActiveTouches[ 0 ].deltaPosition.x;
                    m_Vertical = m_ActiveTouches[ 0 ].deltaPosition.y;
                    //debugPoint = m_ActiveTouches[ 0 ].position;
                } else {
                    m_Horizontal = 0f;
                    m_Vertical = 0f;
                }

                //DebugToScreen.Log( "InputManager: m_TouchCount: " + m_TouchCount + " m_Horizontal: " + m_Horizontal + " m_Vertical: " + m_Vertical 
                //    + " debugPoint: " + debugPoint );
#else
                Vector2     mousePosition = Input.mousePosition;
                TouchPoint  touchPoint = new TouchPoint();
                int         lastTouchCount = m_TouchCount;
                float       time = Time.time;

                if (lastTouchCount == 0) {
                    //--------------
                    // 0 Touches
                    //--------------
                    if (Input.GetMouseButtonDown( 0 )) 
                    {
                        m_TouchCount = 1;
                        //---------------
                        // Create Touch 1
                        //---------------
                        if (m_ActiveTouches[ 0 ].tapCount > 0 && (time - m_ActiveTouchTimes[ 0 ]) < tapMultiThresholdSec) {
                            // Multi-Tap Possible
                            m_ActiveTouches[ 0 ].deltaPosition = mousePosition - m_ActiveTouches[ 0 ].position;
                            m_ActiveTouches[ 0 ].position = mousePosition;
                            m_ActiveTouches[ 0 ].phase = TouchPhase.Began;
                        } else {
                            touchPoint.Set( TouchPhase.Began, 0f, 0, 0, Input.mousePosition, Vector2.zero );
                            m_ActiveTouches[ 0 ] = touchPoint;
                        }
                        m_ActiveTouchTimes[ 0 ] = time;

                        // Fire Press Event
                        //Press( m_ActiveTouches[ 0 ], EventArgs.Empty );
                    }
                } else if (lastTouchCount == 1) {
                    //--------------
                    // 1 Touch
                    //--------------
                    if (Input.GetKey( KeyCode.LeftAlt ) && Input.GetMouseButtonDown( 0 )) 
                    {
                        m_TouchCount = 2;
                        //---------------
                        // Create Touch 2
                        //---------------
                        if (m_ActiveTouches[ 1 ].tapCount > 0 && (time - m_ActiveTouchTimes[ 1 ]) < tapMultiThresholdSec) {
                            // Multi-Tap Possible
                            m_ActiveTouches[ 1 ].deltaPosition = mousePosition - m_ActiveTouches[ 1 ].position;
                            m_ActiveTouches[ 1 ].position = mousePosition;
                            m_ActiveTouches[ 1 ].phase = TouchPhase.Began;
                        } else {
                            touchPoint.Set( TouchPhase.Began, 0f, 0, 1, Input.mousePosition, Vector2.zero );
                            m_ActiveTouches[ 1 ] = touchPoint;
                        }
                        m_ActiveTouchTimes[ 1 ] = time;

                        // Fire Press Event
                        //Press( m_ActiveTouches[ 0 ], EventArgs.Empty );
                    } else if (!Input.GetMouseButton( 0 ) && !Input.GetKey( KeyCode.LeftAlt )) {
                        m_TouchCount = 0;
                        //---------------
                        // Release Touch 1
                        //---------------
                        if ((time - m_ActiveTouchTimes[ 0 ]) < tapThresholdSec) {
                            // Tap
                            m_ActiveTouches[ 0 ].tapCount += 1;
                            m_ActiveTouches[ 0 ].deltaPosition = mousePosition - m_ActiveTouches[ 0 ].position;
                            m_ActiveTouches[ 0 ].position = mousePosition;
                            m_ActiveTouches[ 0 ].phase = TouchPhase.Ended;
                        }
                        m_ActiveTouchTimes[ 0 ] = time;

                        // Fire Release Event
                        //Release( m_ActiveTouches[ 0 ], EventArgs.Empty );
                    } else {
                        // Update Touch 1
                        m_ActiveTouches[ 0 ].deltaPosition = mousePosition - m_ActiveTouches[ 0 ].position;
                        m_ActiveTouches[ 0 ].position = mousePosition;
                        m_ActiveTouches[ 0 ].phase = TouchPhase.Moved;
                    }
                } else if (lastTouchCount == 2) {
                    //--------------
                    // 2 Touches
                    //--------------
                    if (Input.GetKey( KeyCode.LeftAlt ) && !Input.GetMouseButton( 0 )) 
                    {
                        m_TouchCount = 1;
                        //---------------
                        // Release Touch 2
                        //---------------
                        if ((time - m_ActiveTouchTimes[ 1 ]) < tapThresholdSec) {
                            // Tap
                            m_ActiveTouches[ 1 ].tapCount += 1;
                            m_ActiveTouches[ 1 ].deltaPosition = mousePosition - m_ActiveTouches[ 0 ].position;
                            m_ActiveTouches[ 1 ].position = mousePosition;
                            m_ActiveTouches[ 1 ].phase = TouchPhase.Ended;
                        }
                        m_ActiveTouchTimes[ 1 ] = time;

                        // Fire Release Event
                        //Release( m_ActiveTouches[ 1 ], EventArgs.Empty );
                    } else if (!Input.GetKey( KeyCode.LeftAlt ) && !Input.GetMouseButton( 0 )) {
                        m_TouchCount = 0;
                        // No Touches

                    } else {
                        // Update Touch 2
                        m_ActiveTouches[ 1 ].deltaPosition = mousePosition - m_ActiveTouches[ 0 ].position;
                        m_ActiveTouches[ 1 ].position = mousePosition;
                        m_ActiveTouches[ 1 ].phase = TouchPhase.Moved;
                    }
                } else {
                    // Invalid State
                    m_TouchCount = 0;
                }

                // Update Axis
                if (m_TouchCount == 1) {
                    m_Horizontal = Input.GetAxis( "Mouse X" );
                    m_Vertical = Input.GetAxis( "Mouse Y" );
                } else {
                    m_Horizontal = 0f;
                    m_Vertical = 0f;
                }

                // Update All Tap DeltaTime
                for (int t = 0; t < m_TouchCount; ++t) {
                    m_ActiveTouches[ t ].deltaTime += Time.deltaTime;
                }

                // Whew.... Definitely not Object Oriented but hey, works for this gig
#endif




                UpdateGestures();

                //----------------
                // DELETE
                //if(m_TouchCount == 2) {
                //    DebugToScreen.Log( "InputManager: Touch Count is 2" );
                //}
                //----------------

                //Debug.Log( "InputManager: Update() m_TouchCount: " + m_TouchCount + " Horizontal: " + m_Horizontal + " Vertical: " + m_Vertical
                //    + " Touch[0] TapCount: " + m_ActiveTouches[ 0 ].tapCount + " Touch[1] TapCount: " + m_ActiveTouches[ 1 ].tapCount );
            }

            /*##########################################

                        Public Functions

            ###########################################*/
            public static float GetHorizontalAxis()
            {
                return m_Horizontal;
            }
            public static float GetVerticalAxis()
            {
                return m_Vertical;
            }

            public static void Register( Gesture gesture, EventHandler<EventArgs> handler )
            {
                if( (int)gesture < m_Gestures.Count ) {
                    m_Gestures[ (int)gesture ].observers += handler;
                }
            }
            /*##########################################

                        Gesture Recognizers

            ###########################################*/
           
            private void UpdateGestures()
            {
                TouchPoint[] activeTouches = new TouchPoint[m_TouchCount];
                Array.Copy( m_ActiveTouches, 0, activeTouches, 0, m_TouchCount );

                for(int g = 0; g < m_Gestures.Count; ++g) 
                {
                    m_Gestures[ g ].OnTouches( activeTouches );
                }
            }

            /*##########################################

                        Private Functions

            ###########################################*/


        } // CB.Input.InputManager
    } // CB.Input
} // CB



