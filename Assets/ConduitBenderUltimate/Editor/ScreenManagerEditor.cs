using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( ScreenManager ) )]
public class ScreenManagerEditor : Editor
{
    

    //UIScreenManager     _screenManager;
    SerializedObject    screenManager;

    SerializedProperty    cornerBtnTL;
    SerializedProperty    cornerBtnTR;
    SerializedProperty    cornerBtnBR;
    SerializedProperty    cornerBtnBL;

    SerializedProperty    mainScreen;
    SerializedProperty    screens;

    void OnEnable()
    {
        //_screenManager = (UIScreenManager)target;
        screenManager = serializedObject;

        cornerBtnTL = screenManager.FindProperty( "cornerBtnTL" );
        cornerBtnTR = screenManager.FindProperty( "cornerBtnTR" );
        cornerBtnBR = screenManager.FindProperty( "cornerBtnBR" );
        cornerBtnBL = screenManager.FindProperty( "cornerBtnBL" );

        mainScreen = screenManager.FindProperty( "mainScreen" );
        screens = screenManager.FindProperty( "screens" );
    }

    public override void OnInspectorGUI()
    {
        //serializedObject.Update();

        //EditorGUILayout.PropertyField( cornerBtnTL );
        //EditorGUILayout.PropertyField( cornerBtnTR );
        //EditorGUILayout.PropertyField( cornerBtnBR );
        //EditorGUILayout.PropertyField( cornerBtnBL );

        //EditorGUILayout.PropertyField( mainScreen );

        ////EditorList.Show( screens, EditorListOption.ListSize | EditorListOption.Buttons | EditorListOption.ElementLabels );
        ////EditorGUILayout.PropertyField( screens );
        //for (int s = 0; s < screens.arraySize; ++s ) {

        //    EditorGUILayout.PropertyField( screens.GetArrayElementAtIndex( s ) );
        //}

        //serializedObject.ApplyModifiedProperties();
        DrawDefaultInspector();
    }


    private void ValidateScreenManager()
    {

    }
}
