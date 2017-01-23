using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor( typeof( CameraController ) )]
public class CameraControllerEditor : Editor
{
    //SerializedProperty  cameraRig;
    //SerializedProperty  cameraDolly;     // Transform
    //SerializedProperty  camera;          // Camera

    ////----------------
    //// Positioning
    ////----------------
    //SerializedProperty  m_RailEnd;          // Vector3

    //SerializedProperty  m_RailHome;         // float
    //SerializedProperty  m_ZoomRailHome;     // float
    //SerializedProperty  m_ZoomRailLength;   // float
    //SerializedProperty  m_ZoomRailOffset;   // float
    //SerializedProperty  m_TiltAngleHome;    // float
    //SerializedProperty  m_tiltMaxAngle;     // float
    //SerializedProperty  m_TiltDirection;    // int
    
    ////----------------
    //// Control
    ////----------------
    //SerializedProperty  railSpeed;          // float
    //SerializedProperty  zoomRailSpeed;      // float
    //SerializedProperty  tiltSpeed;          // float
    //SerializedProperty  railSensitivity;          // float
    //SerializedProperty  zoomRailSensitivity;      // float
    //SerializedProperty  tiltSensitivity;          // float

    //SerializedProperty  autoHomeSpeed;                // float

    //CameraController.TiltDirection tiltDirection = CameraController.TiltDirection.Negative;

    //bool showPositioning = true;
    //bool showControl = true;

    //void OnEnable()
    //{
    //    cameraRig = serializedObject.FindProperty( "cameraRig" );
    //    cameraDolly = serializedObject.FindProperty( "cameraDolly" );
    //    camera = serializedObject.FindProperty( "theCamera" );

    //    //---------------- 
    //    // Positioning
    //    //----------------
    //    m_RailEnd = serializedObject.FindProperty( "m_RailEnd" );
    //    m_RailHome = serializedObject.FindProperty( "m_RailHome" );

    //    m_ZoomRailHome = serializedObject.FindProperty( "m_ZoomRailHome" );
    //    m_ZoomRailLength = serializedObject.FindProperty( "m_ZoomRailLength" );
    //    m_ZoomRailOffset = serializedObject.FindProperty( "m_ZoomRailOffset" );
    //    m_TiltAngleHome = serializedObject.FindProperty( "m_TiltAngleHome" );
    //    m_tiltMaxAngle = serializedObject.FindProperty( "m_tiltMaxAngle" );
    //    m_TiltDirection = serializedObject.FindProperty( "m_TiltDirection" );

    //    //----------------
    //    // Control
    //    //----------------
    //    railSpeed = serializedObject.FindProperty( "railSpeed" );
    //    zoomRailSpeed = serializedObject.FindProperty( "zoomRailSpeed" );
    //    tiltSpeed = serializedObject.FindProperty( "tiltSpeed" );
    //    railSensitivity = serializedObject.FindProperty( "railSensitivity" );
    //    zoomRailSensitivity = serializedObject.FindProperty( "zoomRailSensitivity" );
    //    tiltSensitivity = serializedObject.FindProperty( "tiltSensitivity" );

    //    autoHomeSpeed = serializedObject.FindProperty( "autoHomeSpeed" );
    //}


    //void OnSceneGUI()
    //{
    //    //Handles.BeginGUI();  // Begin a 2D GUI Block inside the 3D Handle GUI
    //    //GUILayout.BeginArea( new Rect( Screen.width - 100, Screen.height - 80, 90, 50 ) );
    //    //if(GUILayout.Button("Reset Area"))
    //    //  shieldArea = 5;
    //    //GUILayout.EndArea();
    //    //Handles.EndGUI();

    //    Transform cameraRigTransform = cameraRig.objectReferenceValue as Transform;
    //    if(cameraRigTransform == null) {
    //        Debug.LogError( "CameraControllerEditor: Camera Rig null." );
    //        return;
    //    }

    //    Transform cameraDollyTransform = cameraDolly.objectReferenceValue as Transform;
    //    if (cameraDollyTransform != null) {
    //        Handles.Label( cameraDollyTransform.position, "Camera Dolly" );

    //        //-------------------
    //        // Camera Dolly
    //        //-------------------
    //        float zoomOffset = m_ZoomRailOffset.floatValue;
    //        float zoomLength = m_ZoomRailLength.floatValue;
    //        Vector3 zoomStart = cameraDollyTransform.position + cameraDollyTransform.forward * zoomOffset;
    //        Vector3 zoomEnd = cameraDollyTransform.position + -cameraDollyTransform.forward * (zoomLength - zoomOffset);

    //        Handles.color = Color.white;
    //        Handles.DrawSolidDisc( zoomStart, cameraDollyTransform.forward, 0.05f );
    //        Handles.DrawSolidDisc( zoomEnd, cameraDollyTransform.forward, 0.05f );
    //        Handles.DrawLine( zoomStart, zoomEnd );
    //    }

    //    //-------------------
    //    // Camera Track 
    //    //-------------------
    //    GUIStyle style = new GUIStyle();
    //             style.normal.textColor = Color.black;

    //    Vector3 railStart = cameraRigTransform.position;
    //    Vector3 railEnd = cameraRigTransform.TransformPoint( m_RailEnd.vector3Value );
    //    Handles.Label( railStart, "Rail Start", style );
    //    Handles.Label( railEnd, "Rail End", style );
    //    Handles.DrawLine( railStart, railEnd );
    //    m_RailEnd.vector3Value = cameraRigTransform.InverseTransformPoint( Handles.PositionHandle( railEnd, Quaternion.identity ) );

    //    //-------------------
    //    // Camera Tilt Angle 
    //    //-------------------
    //    Camera cam = camera.objectReferenceValue as Camera;
    //    if(cam != null) {
    //        Handles.color = new Color(0.14f, 0.9f, 1f, 1f); // #23E8FF
    //        Handles.DrawSolidArc( cam.transform.position, cam.transform.right, cam.transform.forward, m_TiltDirection.intValue * m_tiltMaxAngle.floatValue, 0.5f );
    //    }
        

    //    //if (GUI.changed) {
    //    //    EditorUtility.SetDirty( target );
    //    //}


    //    // Handles.DrawSolidRectangleWithOutline(Vector3[] rectangleVerts, Color faceColor, Color outlineColor);
    //    //for (Vector3 posCube in rectangleVerts) {
    //    //    range = Handles.ScaleValueHandle( range, posCube, Quaternion.identity, 1, Handles.CubeCap, 1 );
    //    //}

    //    serializedObject.ApplyModifiedProperties();
    //}

    //public override void OnInspectorGUI()
    //{
    //    //base.OnInspectorGUI();
    //    serializedObject.Update();

    //    //----------------
    //    // Object Refs
    //    //---------------- //EditorGUILayout.ObjectField( "Asset file: ", references.settingsManager, typeof( SettingsManager ), false );
    //    cameraRig.objectReferenceValue = EditorGUILayout.ObjectField( "Camera Rig: ", cameraRig.objectReferenceValue, typeof( Transform ), true );
    //    cameraDolly.objectReferenceValue = EditorGUILayout.ObjectField( "Camera Dolly: ", cameraDolly.objectReferenceValue, typeof( Transform ), true );
    //    camera.objectReferenceValue = EditorGUILayout.ObjectField( "The Camera: ", camera.objectReferenceValue, typeof( Camera ), true );

    //    //----------------
    //    // Positioning
    //    //----------------
    //    showPositioning = EditorGUILayout.Foldout( showPositioning, "Positioning" );

    //    if(showPositioning) 
    //    {
    //        EditorGUILayout.BeginVertical( "Button" );

    //        m_RailEnd.vector3Value = EditorGUILayout.Vector3Field( "Rail End", m_RailEnd.vector3Value );

    //        m_RailHome.floatValue = EditorGUILayout.Slider( 
    //            new GUIContent( "Rail Home", "Normalized 'Home' Position of Dolly along main Rail." ), m_RailHome.floatValue, 0f, 1f );

    //        m_ZoomRailHome.floatValue = EditorGUILayout.Slider( 
    //            new GUIContent( "Zoom Rail Home", "Normalized 'Home' Position of Camera along the Zoom Rail." ), m_ZoomRailHome.floatValue, 0f, 1f );

    //        m_ZoomRailLength.floatValue = EditorGUILayout.Slider( "Zoom Rail Length", m_ZoomRailLength.floatValue, 0f, 1000f );

    //        m_ZoomRailOffset.floatValue = EditorGUILayout.Slider( "Zoom Rail Offset", m_ZoomRailOffset.floatValue, 0f, m_ZoomRailLength.floatValue );

    //        m_TiltAngleHome.floatValue = EditorGUILayout.Slider( 
    //            new GUIContent( "Camera Tilt Angle Home", "'Home' Angle of Camera." ), m_TiltAngleHome.floatValue, 0f, 360f );

    //        m_tiltMaxAngle.floatValue = EditorGUILayout.Slider( "Max Camera Tilt Angle", m_tiltMaxAngle.floatValue, 0f, 360f );

    //        tiltDirection = (CameraController.TiltDirection)EditorGUILayout.EnumPopup( "Camera Angle Tilt Direction", tiltDirection );
    //        if (tiltDirection == CameraController.TiltDirection.Negative) {
    //            m_TiltDirection.intValue = -1;
    //        } else {
    //            m_TiltDirection.intValue = 1;
    //        }

    //        EditorGUILayout.EndVertical();
    //    }


    //    //--------------------
    //    // Check Value Ranges
    //    //--------------------
    //    //float zrailLen = m_zoomRailLength.floatValue;
    //    //float zrailOffset = m_zoomRailOffset.floatValue;
    //    //if(zrailLen < 0f ) {
    //    //    zrailLen = 0f;
    //    //    m_zoomRailLength.floatValue = zrailLen;
    //    //}
    //    //if (zrailOffset > zrailLen) {
    //    //    m_zoomRailOffset.floatValue = zrailLen;
    //    //} else if (zrailOffset < 0f) {
    //    //    m_zoomRailOffset.floatValue = 0f;
    //    //}

    //    //----------------
    //    // Control
    //    //----------------
    //    showControl = EditorGUILayout.Foldout( showControl, "Controls" );

    //    if (showControl) 
    //    {
    //        EditorGUILayout.BeginVertical( "Button" );

    //        EditorGUILayout.BeginHorizontal(); EditorGUILayout.PrefixLabel( "Rail Speed" );
    //        railSpeed.floatValue = EditorGUILayout.FloatField( railSpeed.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal(); EditorGUILayout.PrefixLabel( "Zoom Rail Speed" );
    //        zoomRailSpeed.floatValue = EditorGUILayout.FloatField( zoomRailSpeed.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal(); EditorGUILayout.PrefixLabel( "Tilt Speed" );
    //        tiltSpeed.floatValue = EditorGUILayout.FloatField( tiltSpeed.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal(); //EditorGUILayout.PrefixLabel( "Rail Input Sensitivity" );
    //        railSensitivity.floatValue = EditorGUILayout.FloatField( new GUIContent( "Rail Input Sensitivity", "Overridden by Engine Settings"), railSensitivity.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal(); //EditorGUILayout.PrefixLabel( "Zoom Rail Input Sensitivity" );
    //        zoomRailSensitivity.floatValue = EditorGUILayout.FloatField( new GUIContent( "Zoom Rail Input Sensitivity", "Overridden by Engine Settings" ), zoomRailSensitivity.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.BeginHorizontal(); //EditorGUILayout.PrefixLabel( "Tilt Input Sensitivity" );
    //        tiltSensitivity.floatValue = EditorGUILayout.FloatField( new GUIContent( "Tilt Input Sensitivity", "Overridden by Engine Settings" ), tiltSensitivity.floatValue );
    //        EditorGUILayout.EndHorizontal();

    //        EditorGUILayout.Space();
    //        FloatField( "Auto Home Speed", autoHomeSpeed );

    //        EditorGUILayout.EndVertical();
    //    }

    //    EditorGUILayout.Space();
    //    //-------------------
    //    // Snap Dolly Button
    //    //-------------------
    //    EditorGUILayout.BeginHorizontal();
    //    if(GUILayout.Button("Snap Dolly to Rail")) {
    //        Transform cameraDollyTransform = cameraDolly.objectReferenceValue as Transform;
    //        if (cameraDollyTransform != null) {
    //            //Vector3 railStart = m_railStart.vector3Value;
    //            //Vector3 railDelta = m_railEnd.vector3Value - m_railStart.vector3Value;
    //            Vector3 railDelta = m_RailEnd.vector3Value;
    //            Vector3 ontoRail = Vector3.Project(cameraDollyTransform.localPosition, railDelta);
    //            cameraDollyTransform.localPosition = railDelta.normalized * ontoRail.magnitude;
    //        }
    //    }
    //    EditorGUILayout.EndHorizontal();

    //    serializedObject.ApplyModifiedProperties();
    //}


    //private void FloatField(string prefixLabel, SerializedProperty property)
    //{
    //    EditorGUILayout.BeginHorizontal(); EditorGUILayout.PrefixLabel( prefixLabel);
    //    property.floatValue = EditorGUILayout.FloatField( property.floatValue );
    //    EditorGUILayout.EndHorizontal();
    //}
}
