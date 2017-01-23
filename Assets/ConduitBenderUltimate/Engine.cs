using UnityEngine;
using System.Collections;
using CB;

public class Engine : MonoBehaviour {

    // Unity Text Assets:
    // It is not intended for text file generation at runtime. For that you 
    // will need to use traditional Input/Output programming techniques to read and write external files.

    /*
        Trade size Typical bend radius (EMT)
        1/2"............................4 3/16"
        3/4"............................5 1/8"
        1"...............................6 1/2"
        1 1/4"........................8"
        1 1/2"........................8 1/4"
        2"...............................9 1/2"
        2 1/2"........................12 1/2"
        3"...............................15"
        3 1/2"........................17 1/2"
        4"...............................20"
    */

    public static float             benderRadiusM
    {
        get { return s_BenderRadiusM; }
        set
        {
            s_BenderRadiusM = value;
            Refresh();
        }
    }   
    public static float             conduitDiameterM
    {
        get { return s_ConduitDiameterM; }
        set
        {
            s_ConduitDiameterM = value;
            Refresh();
        }
    }
    public static float cameraRailSensitivity
    {
        get { return s_CameraRailSensitivity; }
        set {
            s_CameraRailSensitivity = value;
        }
    }
    public static float cameraZoomSensitivity
    {
        get { return s_CameraZoomSensitivity; }
        set {
            s_CameraZoomSensitivity = value;
        }
    }
    public static float cameraTiltSensitivity
    {
        get { return s_CameraTiltSensitivity; }
        set {
            s_CameraTiltSensitivity = value;
        }
    }

    // Settings
    // IMPORTANT: All Measurements will be in either Meters or Feet
    public static Units.Type        unitType;
    public static Units.RulerUnit   outputRulerUnit;
    
    public static int               conduitSideCount = 6;

    // Static Referenes
    //public static AppData           appData;

    public static ScreenManager     screenManager;      // In Scene
    public SequenceManager          sequenceManager;    // In Scene
    public static EventManager      eventManager;       // In Scene
    public static BendFactory       bendFactory;
    public static BendManager       bendManager;
    public static ConduitManager    conduitManager;
    public static ConduitGenerator  conduitGenerator;
    public static CameraController  mainCameraController;

    // Root Objects
    public static Transform         root_ui;
    public static Transform         root_geometry;
    public static Transform         root_conduit;
    public static Transform         root_particles;
    public static Transform         root_flag;

    // Camera Objects
    public static Camera         cameraUI;
    public static Camera         cameraMain;

    // Singleton
    public static Engine    engine;


    //-------------------------
    //      Private Data
    //-------------------------
    //static References references = null;

    private static float             s_BenderRadiusM = 0.4064f;     // 5 in (0.127 m), 5.25 in (0.13335 m), 5.5 in (0.1397 m), 16 in (0.4064 m)
    private static float             s_ConduitDiameterM = 0.0762f;  // 3/4 in (0.01905 m), 1/2 in (0.0127 m), 3 in (0.0762 m)
    private static float             s_CameraRailSensitivity = 0.2f;
    private static float             s_CameraZoomSensitivity = 0.2f;
    private static float             s_CameraTiltSensitivity = 0.2f;

    private string lastBendScreenTitle = "";


    void Awake()
    {
        // Defaults
        unitType = Units.Type.Standard;
        outputRulerUnit = Units.RulerUnit.Feet;

        // Ensure one instance of this 
        if (engine == null) {
            DontDestroyOnLoad( this );
            engine = this;
        } else if (engine != this) {
            Destroy( this );
        }

        // Limit Frame Rate
        Application.targetFrameRate = 30;


        // Get References
        GameObject found = GameObject.FindWithTag( "ScreenManager" );
        if(found != null) {
            screenManager = found.GetComponent<ScreenManager>();
        }
        found = GameObject.FindWithTag( "EventManager" );
        if (found != null) {
            eventManager = found.GetComponent<EventManager>();
        }
        found = GameObject.FindWithTag( "ConduitManager" );
        if (found != null) {
            conduitManager = found.GetComponent<ConduitManager>();
            conduitGenerator = found.GetComponent<ConduitGenerator>();
        }
        GameObject[] foundarr = GameObject.FindGameObjectsWithTag( "MainCamera" );
        if (foundarr.Length != 0) {
            for(int i = 0; i < foundarr.Length; ++i) {
                if(mainCameraController = foundarr[i].GetComponent<CameraController>()) { break; }
            }
        }
        mainCameraController.movementEnabled = false;

        root_ui         = GameObject.Find( "_UI" ).transform;
        root_geometry   = GameObject.Find( "_Geometry" ).transform;
        root_conduit    = GameObject.Find( "_Conduit" ).transform;
        root_particles  = GameObject.Find( "_Particles" ).transform;
        root_flag       = GameObject.Find( "_Flag" ).transform;

        cameraUI    = GameObject.FindWithTag( "UICamera" ).GetComponent<Camera>();
        cameraMain  = GameObject.FindWithTag( "MainCamera" ).GetComponent<Camera>();

#if UNITY_EDITOR
        // Check Nulls
        if (screenManager == null) {
            Debug.LogError( "Engine: Initialize() No Screen Manager found in scene with the tag 'SceneManager'." );
            return;
        }
        if(eventManager == null ) {

        }
        if (conduitManager == null) {
            Debug.LogError( "Engine: Initialize() No Conduit Manager found in scene on object with the tag 'ConduitManager'." );
            return;
        }
        if (conduitGenerator == null) {
            Debug.LogError( "Engine: Initialize() No Conduit Generator found in scene on object with the tag 'ConduitManager'." );
            return;
        }
        if(mainCameraController == null) {
            Debug.LogError( "Engine: Initialize() No Main Camera Controller found in scene on object with the tag 'MainCamera'." );
            return;
        }
#endif

        DebugToScreen.Log( "Engine: Initialize() Awake Complete!" );
    }
    void Start()
    {
        DebugToScreen.Log( "Engine: Start() Loading Saved Data..." );

        //------------------------
        // Load Saved Data
        //------------------------
        //appData = new AppData();
        //AppData.Initialize();
        //AppData.Load();

        if(!PlayerPrefs.HasKey("FirstRun")) {
            // First Time Application has been Run
            PlayerPrefs.SetInt( "FirstRun", 1 );

            Debug.Log( "Engine: Start() First Application Run" );
            DebugToScreen.Log( "Engine: Start() First Application Run" );
        } else {
            benderRadiusM = PlayerPrefs.GetFloat( "BenderRadiusM" );
            conduitDiameterM = PlayerPrefs.GetFloat( "ConduitDiameterM" );
            cameraRailSensitivity = PlayerPrefs.GetFloat( "CameraRailSensitivity" );
            cameraZoomSensitivity = PlayerPrefs.GetFloat( "CameraZoomSensitivity" );
            cameraTiltSensitivity = PlayerPrefs.GetFloat( "CameraTiltSensitivity" );
        }


        DebugToScreen.Log( "Engine: Start() Initializing Components..." );
        //------------------------
        // Initialize Components
        //------------------------
        bendFactory = new BendFactory();
        BendFactory.Initialize();

        bendManager = new BendManager();
        BendManager.Initialize();

        ConduitGenerator.degreesPerVerticeSet = 5f;
        ConduitGenerator.conduitDiameterM = conduitDiameterM;
        ConduitGenerator.numberOfSides = conduitSideCount;
        ConduitGenerator.Initialize();

        // Screen Manager is ultimately what will trigger 3d conduit generation
        screenManager.AddLinker( bendManager );
        screenManager.onEvent += ScreenManagerOnEvent;

        //------------------------
        // Startup Sequence
        //------------------------
        StartSequence();

        DebugToScreen.Log( "Engine: Start() Complete!" );
    }

    void Update()
    {
        // TODO: Exit App on Android Back Button x 3 
        //if (Input.GetKeyDown( KeyCode.Escape )) {
        //    if(screenManager.ScreenInHistory()) {
        //        screenManager.Back();
        //    } else {
        //        // Start Quit Queue
        //        //Application.Quit();
        //    }
        //}
    }

    /*######################################
    
            Private Functions

    ######################################*/

    /// <summary>
    /// Settings Changed
    /// </summary>
    private static void Refresh()
    {
        ConduitGenerator.conduitDiameterM = s_ConduitDiameterM;

    }
    /// <summary>
    /// Save User Settings
    /// </summary>
    private static void SaveSettings()
    {
        PlayerPrefs.SetFloat( "BenderRadiusM", s_BenderRadiusM );
        PlayerPrefs.SetFloat( "ConduitDiameterM", s_ConduitDiameterM );
        PlayerPrefs.SetFloat( "CameraRailSensitivity", s_CameraRailSensitivity );
        PlayerPrefs.SetFloat( "CameraZoomSensitivity", s_CameraZoomSensitivity );
        PlayerPrefs.SetFloat( "CameraTiltSensitivity", cameraTiltSensitivity );
    }

    private void StartSequence()
    {
        // Fireworks
        sequenceManager.RunSequence( "StartUpSequence" );
    }
    
    /*######################################
    
            Public Functions

    ######################################*/
    
    /// <summary>
    /// Parameter 'value' should be in same unit type as Engine.unitType. 
    /// Also must be either Feet or Meters.
    /// </summary>
    public static float GetInternalValue(float value)
    {
        if(Engine.unitType == Units.Type.Standard) {
            return value * Units.k_FtToM;
        }
        return value;
    }
    /// <summary>
    /// Return possible characters for current language
    /// </summary>
    public static string GetLanguageCharacters()
    {
        return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuv0123456789~`!#$%^&*()-_+='\",.?;:";
    }
    public static int GetLanguageMaxParameterStringLength()
    {
        int len = 0;
        int maxLength = 0;
        for(int i = 0; i < BendParameterMeta.NameStrings.Length; ++i) {
           len = BendParameterMeta.NameStrings[ i ].Length;
           if (len > maxLength) {
                maxLength = len;
           }
        }

        return maxLength + ("000ft - 00 00/00\"").Length;
    }


    ///// <summary>
    ///// Returns the References asset, which should be located in a Resources directory.
    ///// </summary>
    //public static References GetReferences()
    //{
    //    if (references == null) {
    //        references = (References)Resources.Load( Resource.references );
    //    }
    //    return references;
    //}
    public void ScreenManagerOnEvent( ScreenManager.Event e )
    {
        // Was Bend Screen Opened?
        bool isBendScreen = false;
        var screenMeta = screenManager.GetOpenScreen();
        if (screenMeta != null) 
        {
            if(screenMeta.screen is BendInputScreen) {
                isBendScreen = true;

                if (!conduitManager.gameObject.activeSelf) {
                    DebugToScreen.Log( "Engine: Conduit Manager object Activated." );
                    conduitManager.gameObject.SetActive( true );
                }
            } else {
                // Disable Conduit Stuff
                DebugToScreen.Log( "Engine: Conduit Manager object DeActivated." );
                conduitManager.gameObject.SetActive( false );
            }
        }

        switch (e.type) {
            case ScreenManager.Event.Type.Closed:
                if(e.screenTitle == "Settings") {
                    // Save User Settings
                    SaveSettings();
                }
                break;
            case ScreenManager.Event.Type.Opened:
                if (isBendScreen) {
                    // Was it the Same Screen as Previous?
                    if (screenMeta.title != lastBendScreenTitle) {

                        mainCameraController.MoveCameraHome();
                        lastBendScreenTitle = screenMeta.title;
                    }
                }
                break;
            case ScreenManager.Event.Type.Hidden:
                if (isBendScreen) {
                    mainCameraController.movementEnabled = true;
                    return;
                }
                break;
            case ScreenManager.Event.Type.UnHidden:

                break;

        }
        mainCameraController.movementEnabled = false;

        Debug.Log( "Engine: ScreenManagerOnEvent() Event: " + e.type + " Title: " + e.screenTitle );
    }


    public void Quit()
    {
        Debug.Log( "Engine: Quit() Exiting Application." );
        // Save data
        bendManager.Save();

        Application.Quit();
    }


    /*===========================================================
    
                    Assets / Resources

    ===========================================================*/
    //IEnumerator LoadAssets()
    //{
    //    // Path in filesystem. For downloading from internet, replace with your url eg. "http://www.myserver.com/hd"
    //    var url = "file://" + Application.dataPath.Replace ("Assets", "AssetBundles/") + "hd";

    //    // Download bundle and wait until it's finished
    //    var www = new WWW (url);
    //    yield return www;

    //    // Find all renderers from scene
    //    var renderers = FindObjectsOfType<SpriteRenderer>();

    //    // Use renderers name to find the correct sprite from bundle
    //    foreach (var renderer in renderers) {
    //        renderer.sprite = www.assetBundle.LoadAsset( renderer.name, typeof( Sprite ) ) as Sprite;
    //    }

    //    // Unload assets that we don't need right now
    //    www.assetBundle.Unload( false );
    //    www.Dispose();
    //}

}
