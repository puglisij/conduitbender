/***********************************************
				EasyTouch V
	Copyright © 2014-2015 The Hedgehog Team
    http://www.thehedgehogteam.com/Forum/
		
	  The.Hedgehog.Team@gmail.com
		
**********************************************/
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace HedgehogTeam.EasyTouch{
public class BaseFinger {

	public int fingerIndex;	
	public int touchCount;
	public Vector2 startPosition;
    /// <summary> Current screen coordinate position of Gesture </summary>
	public Vector2 position;
    /// <summary> Delta since last Gesture, not since start of gesture </summary>
	public Vector2 deltaPosition;	
	public float actionTime;
    /// <summary> The time passage since the last gesture or the last frame </summary>
	public float deltaTime;		
	
	public PickedData pickedData;
    /// <summary> Syntactic sugar for pickedData.camera </summary>
    public Camera pickedCamera
    {
        get { return pickedData != null ? pickedData.camera : null; }
    }
    /// <summary> Syntactic sugar for pickedData.pickedObj </summary>
    public GameObject pickedObject
    {
        get { return pickedData != null ? pickedData.pickedObj : null; }
    }
		
	public bool isOverGui;
	public GameObject pickedUIElement;

	#if UNITY_5_3
	public float altitudeAngle;
	public float azimuthAngle;
	public float maximumPossiblePressure;
	public float pressure;

	public float radius;
	public float radiusVariance;
	public TouchType touchType;
	#endif
	
    /// <summary>
    /// Allocate and clones (shallow-copy) a new Gesture from 'this' data
    /// </summary>
	public Gesture GetGesture() {

		Gesture gesture = new Gesture();
		gesture.fingerIndex = fingerIndex;
		gesture.touchCount = touchCount;
		gesture.startPosition = startPosition;
		gesture.position = position;
		gesture.deltaPosition = deltaPosition;
		gesture.actionTime = actionTime;
		gesture.deltaTime = deltaTime;
		gesture.isOverGui = isOverGui;

		gesture.pickedData = pickedData;
		gesture.pickedUIElement = pickedUIElement;

		#if UNITY_5_3
		gesture.altitudeAngle = altitudeAngle;
		gesture.azimuthAngle = azimuthAngle;
		gesture.maximumPossiblePressure = maximumPossiblePressure;
		gesture.pressure = pressure;
		gesture.radius = radius;
		gesture.radiusVariance = radiusVariance;
		gesture.touchType = touchType;
		#endif

		return gesture;
	}

}
}
