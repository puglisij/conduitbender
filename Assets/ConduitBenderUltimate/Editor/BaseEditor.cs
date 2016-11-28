//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using System;
//using System.Reflection;

//public class BaseEditor<T> : Editor where T : MonoBehaviour
//{
//    public override void OnInspectorGUI()
//    {
//        T data = (T) target;

//        GUIContent label = new GUIContent();
//        label.text = "Properties"; //

//        DrawDefaultInspectors( label, data );

//        if (GUI.changed) {
//            EditorUtility.SetDirty( target );
//        }
//    }

//    public static void DrawDefaultInspectors<T>( GUIContent label, T target ) where T : new()
//    {
//        EditorGUILayout.Separator();
//        Type type = typeof(T);
//        FieldInfo[] fields = type.GetFields();
//        EditorGUI.indentLevel++;

//        foreach (FieldInfo field in fields) {
//            if (field.IsPublic) {
//                if (field.FieldType == typeof( int )) {
//                    field.SetValue( target, EditorGUILayout.IntField(
//                    MakeLabel( field ), (int)field.GetValue( target ) ) );
//                } else if (field.FieldType == typeof( float )) {
//                    field.SetValue( target, EditorGUILayout.FloatField(
//                    MakeLabel( field ), (float)field.GetValue( target ) ) );
//                }

//                  ///etc. for other primitive types

//                  else if (field.FieldType.IsClass) {
//                    Type[] parmTypes = new Type[]{ field.FieldType};

//                    string methodName = "DrawDefaultInspectors";

//                    MethodInfo drawMethod = typeof(CSEditorGUILayout).GetMethod(methodName);

//                    if (drawMethod == null) {
//                        Debug.LogError( "No method found: " + methodName );
//                    }

//                    bool foldOut = true;

//                    drawMethod.MakeGenericMethod( parmTypes ).Invoke( null,
//                       new object[]
//                       {
//                  MakeLabel(field),
//                  field.GetValue(target)
//                       } );
//                } else {
//                    Debug.LogError(
//                       "DrawDefaultInspectors does not support fields of type " +
//                       field.FieldType );
//                }
//            }
//        }

//        EditorGUI.indentLevel--;
//    }

//    private static GUIContent MakeLabel( FieldInfo field )
//    {
//        GUIContent guiContent = new GUIContent();
//        guiContent.text = field.Name.SplitCamelCase();
//        object[] descriptions = field.GetCustomAttributes(typeof(DescriptionAttribute), true);

//        if (descriptions.Length > 0) {
//            //just use the first one.
//            guiContent.tooltip =
//               (descriptions[ 0 ] as DescriptionAttribute).Description;
//        }

//        return guiContent;
//    }
//}
