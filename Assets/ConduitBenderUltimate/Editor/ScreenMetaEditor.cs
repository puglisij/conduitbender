using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

[CustomPropertyDrawer( typeof( ScreenMeta ) )]
public class ScreenMetaEditor : PropertyDrawer
{ 
    private static GUIContent
        deleteButtonContent     = new GUIContent("-", "delete"),
        addButtonContent        = new GUIContent("+", "add element");

    private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);

    private Texture2D bgTexture;

    public ScreenMetaEditor() : base()
    {
        bgTexture = MakeTex( 100, 1, new Color(0.72f, 0.78f, 0.8f, 1f ) );
    }

    public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
    {
        //base.OnGUI( position, property, label );
        //EditorGUI.PrefixLabel( position, label );
        SerializedProperty screen = property.FindPropertyRelative ("screen");

        SerializedProperty title = property.FindPropertyRelative ("title");
        SerializedProperty modelName = property.FindPropertyRelative ("modelName");
        SerializedProperty closeOnMain = property.FindPropertyRelative ("closeOnMain");
        SerializedProperty remember = property.FindPropertyRelative ("remember");
        SerializedProperty controls = property.FindPropertyRelative ("controls");


        //------------
        // Fields
        //------------
        GUIStyle areaStyle = new GUIStyle( GUI.skin.button );
                 //areaStyle.normal.background = bgTexture; //EditorGUIUtility.FindTexture( "blue_bg.png" );
        EditorGUILayout.BeginVertical( "button" );

        EditorGUILayout.PropertyField( screen );
        EditorGUILayout.PropertyField( title );
        EditorGUILayout.PropertyField( modelName );

        if( title.stringValue != "Main" ) {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField( closeOnMain );
            EditorGUILayout.PropertyField( remember );
            EditorGUILayout.EndHorizontal();
        }

        //-----------------
        // Controls Field
        //-----------------
        GUIStyle controlLabelStyle = new GUIStyle();
                 controlLabelStyle.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField( "Controls" , controlLabelStyle );
        //EditorList.Show( screens, EditorListOption.ListLabel | EditorListOption.Buttons );
        int origIndent = EditorGUI.indentLevel;
        //EditorGUI.indentLevel += 1;

        string[] controlPositionValues = Enum.GetNames( typeof(EScreenControlPosition) );
        string[] controlValues = Enum.GetNames( typeof(EScreenControl) );

        Rect controlRect = position;
             controlRect.width *= 0.5f;
        GUIStyle enumStyle = new GUIStyle( EditorStyles.popup );  // GUI.skin.button
                 enumStyle.margin = new RectOffset( 0, 0, 0, 0 );
        GUIStyle labelStyle = new GUIStyle( EditorStyles.label );
                 labelStyle.margin = new RectOffset( 0, 0, 0, 0 );
                 labelStyle.padding = new RectOffset( 0, 0, 0, 0 );
        // Show Buttons for each Control Element
        for (int i = 0; i < controls.arraySize; i++) {
            // Get the current Screen Control Item
            SerializedProperty control_i = controls.GetArrayElementAtIndex( i );

            // Layout Fields
            EditorGUILayout.BeginHorizontal();
            SerializedProperty controlPosition = control_i.FindPropertyRelative( "position" );
            SerializedProperty control = control_i.FindPropertyRelative( "control" );
            //EditorGUI.BeginProperty( position, new GUIContent("Position"), controlPosition );

            //EditorGUIUtility.labelWidth = 10;
            GUILayoutOption[] labelOptions = {
                GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.1f)
            };
            EditorGUILayout.LabelField( "Position", labelStyle, labelOptions );
            controlPosition.enumValueIndex = EditorGUILayout.Popup( controlPosition.enumValueIndex, controlPositionValues, enumStyle );

            //controlRect.x = controlRect.width + 10;
            //controlRect.width -= 10;

            EditorGUILayout.LabelField( "Control", labelStyle, labelOptions );
            control.enumValueIndex = EditorGUILayout.Popup( control.enumValueIndex, controlValues, enumStyle );
            ShowButtons( controls, i );

            //EditorGUI.EndProperty();
            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel = origIndent;


        // Add Control Button
        if (controls.arraySize < 4) {
            if (GUILayout.Button( "Add", EditorStyles.miniButtonRight)) {
                controls.InsertArrayElementAtIndex( controls.arraySize );
            }
        }

        EditorGUILayout.EndVertical();

    }

    private static void ShowButtons( SerializedProperty list, int index )
    {
        if (GUILayout.Button( deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth )) {
            int oldSize = list.arraySize;
            list.DeleteArrayElementAtIndex( index );
            if (list.arraySize == oldSize) {
                list.DeleteArrayElementAtIndex( index );
            }
        }
    }


    private Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width*height];

        for (int i = 0; i < pix.Length; i++)
            pix[ i ] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels( pix );
        result.Apply();

        return result;
    }
}
