using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof( ScrollRectExtra ) )]
public class ScrollRectExtraEditor : Editor {

    //ScrollRectExtra     _sre;
    SerializedObject    sre;

    //SerializedProperty    isVertical;
    //SerializedProperty      allowCentering;
    //PropertyField[]     fields;

    void OnEnable()
    {
        //_sre = (ScrollRectExtra)target;
        sre = new SerializedObject( target );

        //isVertical = sre.FindProperty( "isVertical" );
        //allowCentering = sre.FindProperty( "allowCentering" );
        //fields = ExposeProperties.GetProperties( _sre );
    }
    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        base.OnInspectorGUI();
        sre.Update();

        //isVertical.boolValue = EditorGUILayout.ToggleLeft( "Is Vertical", isVertical.boolValue );
        //allowCentering.boolValue = EditorGUILayout.ToggleLeft( "Allow Centering?", allowCentering.boolValue );
        //ExposeProperties.Expose( fields );

        // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
        sre.ApplyModifiedProperties();
    }
}
