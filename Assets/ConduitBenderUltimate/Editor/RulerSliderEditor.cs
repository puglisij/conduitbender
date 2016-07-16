using UnityEngine;
using UnityEditor;


[CustomEditor (typeof(RulerSlider))]
public class RulerSliderEditor : Editor {

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
   
}
