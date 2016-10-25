using UnityEngine;
using System.Collections;

public delegate void AngleDeviceEvent();

/// <summary>
/// Implemented by Level & Protractor and used by LevelScreen
/// </summary>
public interface IAngleDevice
{
    float xAngle { get; }
    float yAngle { get; }
    float zAngle { get; }
    Vector3 angles { get; }

    event AngleDeviceEvent onAngleChange;
}
