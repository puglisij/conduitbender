using UnityEngine;
using System.Collections;
using System;
using HedgehogTeam.EasyTouch;

namespace CB
{
    public abstract class AModal : MonoBehaviour 
    {
        public enum ETapAction { CloseOnUnfocus, CloseOnTap, None }

        //--------------
        // Public
        //--------------
        public virtual string modalTitle
        {
            get { return m_ModalTitle; }
            set { m_ModalTitle = value; }
        }
        public virtual bool isOpen { get { return m_IsOpen; } }

        public Animator m_Opener;

        public ETapAction tapAction = ETapAction.None;

        [Tooltip("The view transform of the modal. This is used to determine unfocus when taps are outside this area.")]
        public RectTransform view;
        //--------------
        // Protected
        //--------------
        //Animator State and Transition names we need to check against.
        protected const string k_OpenTransitionName = "isOpen";
        protected const string k_OpenStateName = "Open";
        protected const string k_ClosedStateName = "Closed";

        //Hash of the parameter we use to control the transitions.
        protected int m_OpenParameterId;
        protected int m_HideParameterId;

        [SerializeField, HideInInspector]
        protected bool m_IsOpen = false;

        [SerializeField, HideInInspector]
        protected bool    m_RememberState = false;
        [SerializeField, HideInInspector]
        protected string  m_ModalTitle = "Undefined";


        IEnumerator TapListenerDelay()
        {
            yield return new WaitForSeconds( 0.3f );

            EasyTouch.On_SimpleTap += CheckFocus;
        }

        //Coroutine that will detect when the Closing animation is finished and it will deactivate the
        //hierarchy.
        IEnumerator DisableModalDelayed( Animator anim, bool doDisable )
        {
            bool closedStateReached = false;
            bool wantToClose = true;
            while (!closedStateReached && wantToClose) {
                if (!anim.IsInTransition( 0 )) {
                    closedStateReached = anim.GetCurrentAnimatorStateInfo( 0 ).IsName( k_ClosedStateName );
                }

                wantToClose = !anim.GetBool( m_OpenParameterId );

                yield return new WaitForEndOfFrame();
            }
            //Debug.Log( "AModal: DisableModalDelayed() Transition Complete. Title: " + modalTitle );

            if (wantToClose) {
                m_IsOpen = false;
                if (doDisable) {
                    gameObject.SetActive( false );
                }
            }
        }
        // Coroutine that will detect when the Opening animation is finished
        IEnumerator OpenAnimationMonitor( Animator anim )
        {
            bool openStateReached = false;
            bool wantToOpen = true;
            while (!openStateReached && wantToOpen) {
                if (!anim.IsInTransition( 0 )) {
                    openStateReached = anim.GetCurrentAnimatorStateInfo( 0 ).IsName( k_OpenStateName );
                }
                wantToOpen = anim.GetBool( m_OpenParameterId );

                yield return new WaitForEndOfFrame();
            }
            // Was Animation Not Interrupted?
            if (wantToOpen) {
                m_IsOpen = true;
            }
        }
        void CheckFocus(Gesture gesture)
        {
            if(m_IsOpen) 
            {
                if (tapAction == ETapAction.CloseOnUnfocus) {
                    // Is finger over current MOdal's RectTransform?
                    if (!RectTransformUtility.RectangleContainsScreenPoint( view, gesture.position, Engine.cameraUI )) {
                        Close( true );
                    }
                } else if (tapAction == ETapAction.CloseOnTap) {
                    Close( true );
                }
            }
        }
        /*#############################

               Protected Functions

        ##############################*/
        protected virtual void Awake()
        {
            //We cache the Hash to the "Open" Parameter, so we can feed to Animator.SetBool.
            m_OpenParameterId = Animator.StringToHash( k_OpenTransitionName );

        }
        protected virtual void OnEnable()
        {
            StartCoroutine( TapListenerDelay() );
            Debug.Log( "Modal: OnEnable()" );
        }
        protected virtual void OnDisable()
        {
            EasyTouch.On_SimpleTap -= CheckFocus;
            Debug.Log( "Modal: OnDisable()" );
        }
        /*#############################

                Public Functions

        ##############################*/
        public virtual void Close( bool doDisable )
        {
            if(m_Opener != null) {
                m_Opener.SetBool( m_OpenParameterId, false );
                StartCoroutine( DisableModalDelayed( m_Opener, doDisable ) );
            } else {
                m_IsOpen = false;
                if (doDisable) {
                    gameObject.SetActive( false );
                }
            }      
            //Debug.Log( "AModal: Close() Title: " + modalTitle );
        }
        public virtual void Open()
        {
            gameObject.SetActive( true );

            if(m_Opener != null) {
                m_Opener.SetBool( m_OpenParameterId, true );
                StartCoroutine( OpenAnimationMonitor( m_Opener ) );
            } else {
                m_IsOpen = true;
            }
        }

        public void Nothing() {}
    }
}






