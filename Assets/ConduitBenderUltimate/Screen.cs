using UnityEngine;
using System.Collections;
using System;

public abstract class AScreen : MonoBehaviour, ILinkable
{
    public string modelName {
        get;
        set;
    }
    public string screenTitle {
        get { return m_ScreenTitle; }
        set { m_ScreenTitle = value; }
    }
    public bool rememberState {
        get { return m_RememberState; }
        set { m_RememberState = value; }
    }
    public abstract bool isOpen { get; }
    public abstract bool isHidden { get; }

    [SerializeField, HideInInspector]
    protected bool m_IsOpen = false;
    [SerializeField, HideInInspector]
    protected bool m_IsHidden = false;

    [SerializeField, HideInInspector]
    bool    m_RememberState = false;
    [SerializeField, HideInInspector]
    string  m_ScreenTitle = "Undefined";

    /*#############################
            Public Functions
    ##############################*/
    public abstract void Close(bool doDisable);
    public abstract void Hide();
    public abstract void Open();
    public virtual  void OnUnfocus() { }

    public abstract void Link( IModel model );
}


/// <summary>
/// Attach to GameObject which represents the Screen's Hierarchy
/// </summary>
public class AnimScreen : AScreen {

    //Animator State and Transition names we need to check against.
    const string k_OpenTransitionName = "isOpen";
    const string k_HideTransitionName = "isHidden";
    const string k_OpenStateName = "Open";
    const string k_ClosedStateName = "Closed";
    const string k_HiddenStateName = "Hidden";

    public override bool isOpen { get { return m_IsOpen; }  }
    public override bool isHidden { get { return m_IsHidden; } }

    //-------------------------
    //      Public Data
    //-------------------------
    public Animator m_Opener;

    

    //-------------------------
    //      Private Data
    //-------------------------
    //Hash of the parameter we use to control the transitions.
    private int m_OpenParameterId;
    private int m_HideParameterId;
    
    protected virtual void Awake()
    {
        //We cache the Hash to the "Open" Parameter, so we can feed to Animator.SetBool.
        m_OpenParameterId = Animator.StringToHash( k_OpenTransitionName );
        m_HideParameterId = Animator.StringToHash( k_HideTransitionName );
    }
    protected virtual void OnEnable()
    {

    }
    protected virtual void Start()
    {
        // Initialize state
        m_IsOpen = m_Opener.GetBool( m_OpenParameterId );
        m_IsHidden = m_Opener.GetBool( m_HideParameterId );
    }

    public override void Close(bool doDisable)
    {
        // For now, we cannot be both Hidden and Closed
        m_IsHidden = false;
        m_Opener.SetBool( m_HideParameterId, false );

        m_Opener.SetBool( m_OpenParameterId, false );
        StartCoroutine( DisableScreenDelayed( m_Opener, doDisable ) );
        //Debug.Log( "AnimScreen: Close() Title: " + screenTitle );
    }
    /// <summary>
    /// Toggles Hide State
    /// </summary>
    public override void Hide()
    {
        if( m_IsHidden ) {
            m_Opener.SetBool( m_HideParameterId, false );
            StartCoroutine( UnHiddenAnimationMonitor( m_Opener ) );
        } else {
            //Start the hide animation.
            m_Opener.SetBool( m_HideParameterId, true );
            StartCoroutine( HiddenAnimationMonitor( m_Opener ) );
        }
    }
    public override void Open()
    {
        gameObject.SetActive( true );
        m_Opener.SetBool( m_OpenParameterId, true );
        StartCoroutine( OpenAnimationMonitor( m_Opener ) );
    }


    //Coroutine that will detect when the Closing animation is finished and it will deactivate the
    //hierarchy.
    IEnumerator DisableScreenDelayed( Animator anim, bool doDisable )
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
        //Debug.Log( "AnimScreen: DisableScreenDelayed() Transition Complete. Title: " + screenTitle );

        if (wantToClose) {
            m_IsOpen = false;
            if(doDisable) {
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
    // Coroutine that will detect when the Hidden animation is finished
    IEnumerator HiddenAnimationMonitor( Animator anim )
    {
        bool stateReached = false;
        bool wantToHide = true;
        while (!stateReached && wantToHide) {
            if (!anim.IsInTransition( 0 )) {
                stateReached = anim.GetCurrentAnimatorStateInfo( 0 ).IsName( k_HiddenStateName );
            }
            wantToHide = anim.GetBool( m_HideParameterId );

            yield return new WaitForEndOfFrame();
        }
        // Was Animation Not Interrupted?
        if (wantToHide) {
            m_IsHidden = true;
        }
    }
    IEnumerator UnHiddenAnimationMonitor( Animator anim )
    {
        bool stateReached = false;
        bool wantToUnHide = true;
        while (!stateReached && wantToUnHide) {
            if (!anim.IsInTransition( 0 )) {
                stateReached = anim.GetCurrentAnimatorStateInfo( 0 ).IsName( k_OpenStateName );
            }
            wantToUnHide = !anim.GetBool( m_HideParameterId );

            yield return new WaitForEndOfFrame();
        }
        // Was Animation Not Interrupted?
        if (wantToUnHide) {
            m_IsHidden = false;
        }
    }

    public override void Link( IModel model )
    {
        throw new NotImplementedException();
    }
}
