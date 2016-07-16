using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( BendInputScreen ) )]
public class BendInputScreenEditor : Editor {

    BendInputScreen _screen;

    //BendType screenSelection;
    string prefabPath = "";
    string fullSavePath = "";

    void OnEnable()
    {
        _screen = (BendInputScreen)target;

    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // Styles
        GUIStyle  btnStyle = new GUIStyle( GUI.skin.button );
        btnStyle.fontStyle = FontStyle.Bold;

        GUIStyle  savePathLabelStyle = new GUIStyle();
        savePathLabelStyle.normal.textColor = Color.black;
        savePathLabelStyle.wordWrap = true;
        savePathLabelStyle.stretchWidth = false;

        GUIStyle  saveLabelStyle = new GUIStyle();
        saveLabelStyle.fontStyle = FontStyle.Bold;
        saveLabelStyle.stretchWidth = false;
        // End Styles

        EditorGUILayout.Separator();

        
        EditorGUILayout.BeginVertical( "Button" );
        //----------
        // Make
        //----------
        // Ensure Screen is Initialized
        //_screen.Initialize();
        //screenSelection = (BendType) EditorGUILayout.EnumPopup("Screen Type", screenSelection);
        //if(GUILayout.Button("Make Screen", EditorStyles.miniButton)) {
        //    _screen.MakeScreen( (int) screenSelection );
        //}

        //----------
        // Path
        //----------
        //GUILayoutOption[] options1 = { GUILayout.MaxWidth( EditorGUIUtility.currentViewWidth * 0.1f ) };
        //GUILayoutOption[] options2 = { GUILayout.MaxWidth( EditorGUIUtility.currentViewWidth * 0.9f ) };
        EditorGUILayout.LabelField( "Save Location: ", saveLabelStyle );
        EditorGUILayout.LabelField( fullSavePath, savePathLabelStyle );

        //----------
        // Buttons
        //----------

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button( "Browse", btnStyle )) {
            prefabPath = EditorUtility.SaveFolderPanel("Directory where Screen Prefab will be Saved", prefabPath, "");
            fullSavePath = prefabPath + "/" + _screen.screenTitle + ".prefab";
            // var files = Directory.GetFiles(prefabPath);
            if (prefabPath.Length != 0) {

            }
        }
        if ( GUILayout.Button( "Save As Prefab", btnStyle ) ) {
            
            // Set up the Screen


            // Create new Prefab for this Screen
            if(fullSavePath.Length != 0) {
                Object emptyObj = PrefabUtility.CreateEmptyPrefab( fullSavePath );
                PrefabUtility.ReplacePrefab( _screen.gameObject, emptyObj, ReplacePrefabOptions.ConnectToPrefab );
            }
        }

        //Debug.Log( "BendInputScreenEditor: OnInspectorGUI()" );
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

}
