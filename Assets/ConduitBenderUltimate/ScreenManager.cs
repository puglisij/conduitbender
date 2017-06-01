using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using HedgehogTeam.EasyTouch;

public enum EScreenControlPosition { TL, TR, BL, BR }
public enum EScreenControl { Back, Exit, ExitToMain, Expand, Hide, Shrink }

[Serializable]
public class ScreenControlItem
{
    public EScreenControlPosition position;
    public EScreenControl control;
}

[Serializable]
public class ScreenMeta //: ScriptableObject
{
    [SerializeField]
    public AScreen      screen = null;
    [SerializeField]
    public string       title = "Undefined";
    [SerializeField]
    public string       modelName = "Undefined";

    [SerializeField]
    public bool         closeOnMain = false;
    [SerializeField]
    public bool         remember = true;
    //public bool         doInstantiate;

    [SerializeField]
    public List<ScreenControlItem> controls = new List<ScreenControlItem>();
}

public delegate void ScreenEvent( ScreenManager.Event e );

public class ScreenManager : MonoBehaviour, ILinkableAnnouncer
{
    public struct Event
    {
        public enum Type { Hidden, UnHidden, Closed, Opened }
        public Type type;
        public string screenTitle;

        public Event(Type type, string screenTitle)
        {
            this.type = type;
            this.screenTitle = screenTitle;
        }
    }
    

    private const int k_MaxHistory = 10;

    //-----------------------------
    //      Public Data
    //-----------------------------
    public Canvas canvas;
    // Corner Controls
    public ScreenControl  cornerBtnTL;
    public ScreenControl  cornerBtnTR;
    public ScreenControl  cornerBtnBR;
    public ScreenControl  cornerBtnBL;

    public Sprite  backIcon;
    public Sprite  exitIcon;
    public Sprite  expandIcon;
    public Sprite  hideIcon;    //Back, Exit, ExitToMain, Expand, Hide, Shrink
    public Sprite  showIcon;
    public Sprite  shrinkIcon;

    [SerializeField]
    public ScreenMeta       mainScreen = new ScreenMeta();
    [SerializeField]
    public List<ScreenMeta> screens = new List<ScreenMeta>();

    public event ScreenEvent onEvent;
    //-----------------------------
    //      Private Data
    //-----------------------------
    private List<ILinker>  m_Linkers = new List<ILinker>();

    //Screen History  (Except Main Menu)
    private FixedStack<ScreenMeta> m_ScreenHistory;

    //Screen Mapping of <Screen Title> to <Screen Prefab>
    //public ObjectDictionary m_ScreenHash = new ObjectDictionary();
    private Dictionary<string, ScreenMeta>     m_ScreenHash;

    //Screen to open automatically at the start of the Scene
    private string  m_InitiallyOpen = "";

    //Currently Open Screen
    private ScreenMeta m_Open = null;



    /*###################################

             Unity Functions

    #####################################*/

    void Awake()
    {
        // Init Variables
        m_ScreenHistory = new FixedStack<ScreenMeta>( k_MaxHistory );
        m_ScreenHash = new Dictionary<string, ScreenMeta>();

        // Load Dictionary
        for(int i = 0; i < screens.Count; ++i) {
            m_ScreenHash.Add( screens[ i ].title, screens[ i ] );
            // Ensure Screen Title is set to value set in Inspector
            screens[ i ].screen.screenTitle = screens[ i ].title;
        }

        // Set Main Screen Data
        mainScreen.screen.screenTitle = mainScreen.title;

        // Listen for Tap Events
        EasyTouch.On_SimpleTap += CheckFocus;
    }

    void OnEnable()
    {

    }

    void Start()
    {

    }

    /*###################################

            Private Functions

    #####################################*/
    private void CheckFocus(Gesture gesture)
    {
        AScreen screen;
        if (mainScreen.screen.isOpen) {
            screen = mainScreen.screen;
        } else if (m_Open != null) {
            screen = m_Open.screen;
        } else {
            return;
        }
        // Is finger over current Screen's RectTransform?
        if ( !RectTransformUtility.RectangleContainsScreenPoint( screen.transform as RectTransform, gesture.position, Engine.cameraUI ) ) {
            //Debug.Log( "ScreenManager: CheckFocus() Unfocusing Screen: " + screen.name );
            screen.OnUnfocus();
        }
    }

    private void ControlsOff()
    {
        cornerBtnTL.Off();
        cornerBtnTR.Off();
        cornerBtnBR.Off();
        cornerBtnBL.Off();
    }

    private void OpenScreen( ScreenMeta screenMeta, bool closeCurrent )
    {
        //DebugToScreen.Log( "ScreenManager: OpenScreen() Title: " + screenMeta.title 
        //    + " Model: " + screenMeta.modelName + " Screen: " + screenMeta.screen
        //    + " Opening..." );

        // If already open, return
        if (m_Open != null && m_Open == screenMeta && m_Open.screen.isOpen) { return; }

        AScreen screen = screenMeta.screen;
        screen.screenTitle = screenMeta.title;
        screen.modelName = screenMeta.modelName;

        if (closeCurrent && m_Open != null) {
            if(m_Open.remember) {
                // Push onto History
                m_ScreenHistory.Push( m_Open );
            }
            m_Open.screen.Close( true );
            // Fire 'Closed' Event
            onEvent( new Event( Event.Type.Closed, m_Open.title ) );
        }

        // Open the new Screen
#if DEBUG
        //Debug.Log( "ScreenManager: About to Open Screen: " + screen.screenTitle + " isOpen: " + screen.isOpen + " isHidden: " + screen.isHidden );
#endif

        screen.Open();
        m_Open = screenMeta;

#if DEBUG
        //Debug.Log( "ScreenManager: About to Set Controls: " + screen.screenTitle + " isOpen: " + screen.isOpen + " isHidden: " + screen.isHidden );
#endif

        SetControls( screenMeta.controls );

        // Fire 'Opened' Event
        onEvent( new Event( Event.Type.Opened, screenMeta.title ) );

        Announce();
    }

    private ScreenMeta PopHistory()
    {
        return m_ScreenHistory.Pop();
    }

    private void PushHistory( ScreenMeta screen )
    {
        m_ScreenHistory.Push( screen );
    }

    private void SetControl(ScreenControl controlObj, EScreenControl control)
    {
        switch (control) 
        {
            case EScreenControl.Back:
                controlObj.Set( Back, backIcon );
                break;
            case EScreenControl.Exit:
                controlObj.Set( CloseCurrent, exitIcon );
                break;
            case EScreenControl.ExitToMain:
                controlObj.Set( () => {
                        CloseCurrent();
                        OpenMain();
                    }, exitIcon );
                break;
            case EScreenControl.Expand:
                controlObj.Set( () => {
                        //Debug.LogWarning( "ScreenManager: GetControlHandler() Control not Implemented." );
                    }, expandIcon );
                break;
            case EScreenControl.Hide:
                controlObj.Set( HideCurrent, hideIcon, showIcon, 
                    (m_Open.screen.isHidden) ? ScreenControl.EStartIcon.Icon2 : ScreenControl.EStartIcon.Icon1 );
                break;
            case EScreenControl.Shrink:
                controlObj.Set( () => {
                        //Debug.LogWarning( "ScreenManager: GetControlHandler() Control not Implemented." );
                    }, shrinkIcon );
                break;
            default:
                controlObj.Set( () => {
                    //Debug.LogWarning( "ScreenManager: GetControlHandler() Control not Implemented." );
                }, null );
                break;
        }
    }

    private void SetControls(List<ScreenControlItem> controls)
    {
        // Disable old Controls
        ControlsOff();

        if (controls == null || controls.Count == 0) { return; }
        // Set up new Controls
        for (int c = 0; c < controls.Count; ++c) {
            switch(controls[c].position) 
            {
                case EScreenControlPosition.TL:
                    SetControl( cornerBtnTL, controls[ c ].control );
                    break;
                case EScreenControlPosition.TR:
                    SetControl( cornerBtnTR, controls[ c ].control );
                    break;
                case EScreenControlPosition.BR:
                    SetControl( cornerBtnBR, controls[ c ].control );
                    break;
                case EScreenControlPosition.BL:
                    SetControl( cornerBtnBL, controls[ c ].control );
                    break;
            }
        }
    }


    /*###################################

            Public Functions

    #####################################*/

    public void Back()
    {
        var wasHistory = false;
        CloseCurrent(out wasHistory);
        if(!wasHistory) {
            OpenMain();
        }
    }

    public void CloseCurrent()
    {
        bool wasHistory;
        CloseCurrent(out wasHistory);
    }
    /// <summary>
    /// Closes the currently open Screen. 
    /// Opens previous screen if one exists in History
    /// 
    /// Returns true if a screen from History was opened
    /// </summary>
    public void CloseCurrent(out bool wasHistory)
    {
        wasHistory = false;

        if (m_Open != null) {
            m_Open.screen.Close( true );
            // Fire 'Closed' Event
            onEvent( new Event( Event.Type.Closed, m_Open.title ) );

            if (m_ScreenHistory.Count > 0) {
                // Pop screen from Stack History
                m_Open = m_ScreenHistory.Pop();
                OpenScreen( m_Open, false );
                wasHistory = true;
            } else {
                m_Open = null;
                ControlsOff();
                
            }
        }
    }

    /// <summary>
    /// Returns currently open screen, or null
    /// </summary>
    public ScreenMeta GetOpenScreen()
    {
        return m_Open;
    }

    /// <summary>
    /// Hides the currently open Screen.
    /// </summary>
    public void HideCurrent()
    {
        if (m_Open != null) {
            // Fire 'Hidden'/'UnHidden' Event
            if (m_Open.screen.isHidden) {
                //controlObj.SetIcon( hideIcon );
                onEvent( new Event( Event.Type.UnHidden, m_Open.title ) );
            } else {
                //controlObj.SetIcon( expandIcon );
                onEvent( new Event( Event.Type.Hidden, m_Open.title ) );
            }
            m_Open.screen.Hide();
        }
    }
    /// <summary>
    /// Returns whether current screen is hidden.
    /// If no screen open, returns false.
    /// </summary>
    public bool IsCurrentHidden()
    {
        if (m_Open != null) {
            return m_Open.screen.isHidden;
        }
        return false;
    }

    public void OpenMain()
    {
        if(m_Open != null) {
            if(m_Open.closeOnMain) {
                CloseCurrent();
            }
        }
        // Open Main Screen
        mainScreen.screen.Open();
        SetControls( mainScreen.controls );
    }
    public void CloseMain()
    {
        if(mainScreen.screen.isOpen) {
            // Close Main Screen
            mainScreen.screen.Close(false);
            if(m_Open != null) {
                SetControls( m_Open.controls );
            }
        }

        //Debug.Log( "ScreenManager: CloseMain()" );
    }
    /// <summary>
    /// Closes the currently open screen and opens the one by the given name.
    /// Also ensures the Screens name is properly set.
    /// </summary>
    public void OpenScreen(string screenName)
    {
        // @TODO - Better place to do this?
        // Close Main Screen if it's open
        if(mainScreen.screen.isOpen) {
            CloseMain();
        }

        ScreenMeta screenMeta;
        if( m_ScreenHash.TryGetValue( screenName, out screenMeta ) ) {
            OpenScreen( screenMeta, true );
        }
#if DEBUG
        else {
            //Debug.Log( "ScreenManager: OpenScreen() " + screenName + " Not Found." );
        }
#endif
    }




    public void AddLinker( ILinker linker )
    {
        m_Linkers.Add( linker );
    }

    public void RemoveLinker( ILinker linker )
    {
        m_Linkers.Remove( linker );
    }

    public void Announce()
    {
        // Don't Announce Screens which have no Model name defined
        if(string.IsNullOrEmpty(m_Open.modelName)) { return; }

        m_Linkers.ForEach( (linker) => {
            linker.Announce( m_Open.screen );
        } );
    }
}
