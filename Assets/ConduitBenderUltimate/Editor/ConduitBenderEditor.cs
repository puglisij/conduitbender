using UnityEngine;
using UnityEditor;
using System.IO;
using CB;

public class ConduitBenderEditor : EditorWindow
{

    public References references;

    private bool showSettings = false;
    private bool showScreenManager = false;

    private Vector2 scroll;

    private static GUILayoutOption tabWidth = GUILayout.MinWidth (60f);


    [MenuItem( "Conduit Bender/Editor" )]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        ConduitBenderEditor window = (ConduitBenderEditor) EditorWindow.GetWindow (typeof (ConduitBenderEditor));
        window.GetReferences();
        window.titleContent.text = "Conduit Bender Editor";
    }


    private void GetReferences()
    {
        //references = (References)Resources.Load( Resource.references );
    }


    private void OnEnable()
    {

    }


    private void OnInspectorUpdate()
    {

        Repaint();
    }


    private void OnGUI()
    {
        if(GUILayout.Button( "Unload Resources" )) {
            Resources.UnloadUnusedAssets();
        }


        //if (!references) {
        //    GetReferences();
        //}

        if (references) {
            GUILayout.Space( 10 );
            GUILayout.BeginHorizontal();

            if (GUILayout.Toggle( showSettings, "Settings", "toolbarbutton", tabWidth )) {
                SetTab( 0 );
            }
            if (GUILayout.Toggle( showScreenManager, "Screen Manager", "toolbarbutton", tabWidth )) {
                SetTab( 1 );
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space( 5 );

            scroll = GUILayout.BeginScrollView( scroll );

            if (showSettings) {
                GUILayout.Label( "Settings Manager", EditorStyles.largeLabel );

                //references.settingsManager = (SettingsManager)EditorGUILayout.ObjectField( "Asset file: ", references.settingsManager, typeof( SettingsManager ), false );

                //EditorGUILayout.Separator();
                //GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

                //if (!references.settingsManager) {
                //    AssetUtility.AskToCreate<SettingsManager>( "SettingsManager" );
                //} else {
                //    references.settingsManager.ShowGUI();
                //}
            } else if (showScreenManager) {
                GUILayout.Label( "Screen Manager", EditorStyles.largeLabel );

            //    references.actionsManager = (ScreenManager)EditorGUILayout.ObjectField( "Asset file: ", references.actionsManager, typeof( ScreenManager ), false );

            //    EditorGUILayout.Separator();
            //    GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

            //    if (!references.actionsManager) {
            //        AskToCreate<ScreenManager>( "ScreenManager" );
            //    } else {
            //        references.actionsManager.ShowGUI();
            //    }
            } 

            //EditorGUILayout.Separator();
            //GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( 1 ) );

            //GUILayout.EndScrollView();
        } else {
            //EditorStyles.label.wordWrap = true;
            //GUILayout.Label( "No 'References' asset found in the resources folder. Please click to create one.", EditorStyles.label );

            //if (GUILayout.Button( "Create 'References' file" )) {
            //    references = AssetUtility.CreateAsset<References>( "References", "ConduitBender" + Path.DirectorySeparatorChar.ToString() + "Resources" );
            //}
        }

        if (GUI.changed) {

            EditorUtility.SetDirty( this );
            //EditorUtility.SetDirty( references );
        }
    }


    private void SetTab( int tab )
    {
        showSettings = false;
        showScreenManager = false;

        if (tab == 0) {
            showSettings = true;
        } else if (tab == 1) {
            showScreenManager = true;
        }
    }

}



