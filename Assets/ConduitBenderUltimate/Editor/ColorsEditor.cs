//using UnityEngine;
//using System.Collections;
//using UnityEditor;

//[CustomEditor( typeof( Colors ) )]
//public class ColorsEditor : Editor
//{

//    Colors _colors;

//    SerializedProperty flagRed;

//    void OnEnable()
//    {
//        _colors = (Colors)target;

//        flagRed = serializedObject.FindProperty( "flagRed" );

//    }

//    public override void OnInspectorGUI()
//    {
//        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
//        serializedObject.Update();

//        EditorGUILayout.BeginVertical( "Button" );

//        flagRed.colorValue = EditorGUILayout.ColorField( flagRed.colorValue );

//        EditorGUILayout.EndVertical();

//        serializedObject.ApplyModifiedProperties();
//    }



//}
