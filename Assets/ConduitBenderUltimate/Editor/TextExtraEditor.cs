using System.Collections.Generic;
using UnityEditor;


[CustomEditor( typeof( TextExtra ) )]
public class TextExtraEditor : Editor
{
    //TextExtra     _textExtra;
    //SerializedObject    textExtra;

    void OnEnable()
    {
        //_textExtra = (TextExtra)target;
        //textExtra = serializedObject;


    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }

}
