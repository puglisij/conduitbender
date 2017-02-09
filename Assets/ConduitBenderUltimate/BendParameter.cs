using UnityEngine;
using System.Collections;

public static class BendMessages
{
    public const string k_BendsTooClose = "Bends are too close.";
    public const string k_SegmentedRadiusTooSmall = "Segmented Radius is too small.";
    public const string k_AtLeast3Bends = "Accurate Method requires at least 3 Bends.";
    public const string k_StubLengthTooSmall = "Stub Length is too small.";
}

public enum EBendParameterType { FloatAngle, Float, Integer, StringEnum }

public enum EBendParameterName
{
    AngleDegrees,
    AngleFirstDegrees,
    AngleLastDegrees,
    CenterAngleDegrees,
    DevelopedLength,   // The length of straight conduit used to bend at a specific radius and angle
    Diameter,
    DistanceFromEnd,
    DistanceTo1st,
    DistanceTo2nd,
    DistanceTo3rd,
    DistanceToLast,
    Distance1stTo2nd,
    Distance2ndTo3rd,
    DistanceBetween,
    Alternate1stTo2nd,
    Height,
    KickFirstMark,
    KickOffset,
    KickSpread,
    KickTravel,
    Length,
    LengthOfBend,
    LengthOfCenterBend,
    LengthOfHalfBend,
    OffsetHeight,
    Rise,
    Roll,
    RollAngleDegrees,
    Saddle3Method,
    SaddleHeight,
    SaddleLength,
    SegmentedAngle,
    SegmentedCount,
    SegmentedMethod,
    SegmentedRadius,
    Shift,
    ShrinkTo2ndMark,    // 3 point saddle
    ShrinkToCenter,     // 4 point saddle
    Spacing,
    StubLength,
    StubTakeUp,
    TotalShrink,
    Travel
}

/// <summary>
/// Constants pertaining to BendParameter
/// </summary>
public static class BendParameterMeta 
{
    public static readonly string[] NameStrings = new string[43] {
        "Bend Angle (Degrees)",
        "1st Bend Angle (Degrees)",
        "Last Bend Angle (Degrees)",
        "Center Angle (Degrees)",
        "Developed Length",
        "Diameter",
        "Mark Distance from End",
        "Distance to 1st Mark",
        "Distance to 2nd Mark",
        "Distance to 3rd Mark",
        "Distance to Last Mark",
        "Distance 1st to 2nd Mark",
        "Distance 2nd to 3rd Mark",
        "Distance Between Bends",
        "Alternate 1st to 2nd",
        "Height",
        "Mark from Back of 90", // KickFirstMark      
        "Kick Offset",
        "Kick Spread",
        "Kick Travel",
        "Length",
        "Length of Bend",
        "Length of Center Bend",
        "Length of Half Bend",
        "Offset Height",
        "Rise",
        "Roll",
        "Roll Angle (Degrees)",
        "Saddle 3 Method",
        "Saddle Height",
        "Saddle Length",
        "Segmented Angle (Degrees)",
        "Number of Bends",
        "Segmented Method",
        "Segmented Radius",
        "Shift",
        "Shrink to 2nd Mark",
        "Shrink To Saddle Center",
        "Spacing",
        "Stub Length",
        "Stub Take-Up",
        "Total Shrink",
        "Travel"
    };

    public static readonly object[][] NameRangesMetric = new object[][] {
        new object[] {0.5f, 90f },  // Bend Angle
        new object[] {0f, 0f },     // 1st Bend Angle
        new object[] {0f, 0f },     // Last Bend Angle
        new object[] {0.5f, 90f },  // Center Angle
        new object[] {0f, 0f },   // Developed Length
        new object[] {0f, 3f },   // Diameter
        new object[] {0f, 0f },   // Distance from End
        new object[] {0f, 0f },   // Distance to 1st Mark
        new object[] {0f, 0f },   // Distance to 2nd Mark
        new object[] {0f, 0f },   // Distance to 3rd Mark
        new object[] {0f, 0f },   // Distance to Last Mark
        new object[] {0f, 0f },   // Distance 1st to 2nd 
        new object[] {0f, 0f },   // Distance 2nd to 3rd
        new object[] {0f, 0f },   // Distance Between Bends
        new object[] {0f, 0f },   // Alternate 1st to 2nd Mark 
        new object[] {0f, 3f },   // Height
        new object[] {0f, 0f },   // Kick First Mark       
        new object[] {0f, 30f },  // Kick Offset
        new object[] {0f, 0f },   // Kick Spread
        new object[] {0f, 0f },   // Kick Travel
        new object[] {0f, 3f },   // Length
        new object[] {0f, 0f },   // Length of Bend
        new object[] {0f, 0f },   // Length of Center Bend
        new object[] {0f, 0f },   // Length of Half Bend
        new object[] {0f, 3f },   // Offset Height
        new object[] {0f, 3f },   // Rise
        new object[] {0f, 3f },   // Roll
        new object[] {0f, 0f },   // Roll Angle
        new object[] { GlobalEnum.Saddle3BendMethod.First(), GlobalEnum.SegmentedBendMethod.Last() },
        new object[] {0f, 3f },   // Saddle Height
        new object[] {0f, 3f },   // Saddle Length
        new object[] {0.5f, 90f },  // Segmented Angle
        new object[] {2, 90},     // Number of Bends
        new object[] { GlobalEnum.SegmentedBendMethod.First(), GlobalEnum.SegmentedBendMethod.Last() },
        new object[] {0f, 30f }, // Segmented Radius
        new object[] {0f, 0f },  // Shift
        new object[] {0f, 0f },  // Shrink to 2nd Mark
        new object[] {0f, 0f },   // Shrink To Center
        new object[] {0f, 30f },   // Spacing
        new object[] {0f, 3f },    // Stub Length
        new object[] {0f, 0f },  // Stub Take-Up
        new object[] {0f, 0f },   // Total Shrink
        new object[] {0f, 0f }   // Travel
    };

    public static readonly object[][] NameRangesStandard = new object[][] {
        new object[] {0.5f, 90f },  // Bend Angle
        new object[] {0f, 0f },     // 1st Bend Angle
        new object[] {0f, 0f },     // Last Bend Angle
        new object[] {0.5f, 90f },  // Center Angle
        new object[] {0f, 0f },   // Developed Length
        new object[] {0f, 8f },   // Diameter
        new object[] {0f, 0f },   // Distance from End
        new object[] {0f, 0f },   // Distance to 1st Mark
        new object[] {0f, 0f },   // Distance to 2nd Mark
        new object[] {0f, 0f },   // Distance to 3rd Mark
        new object[] {0f, 0f },   // Distance to Last Mark
        new object[] {0f, 0f },   // Distance 1st to 2nd
        new object[] {0f, 0f },   // Distance 2nd to 3rd
        new object[] {0f, 0f },   // Distance Between Bends
        new object[] {0f, 0f },   // Alternate 1st to 2nd Mark
        new object[] {0f, 8f },   // Height
        new object[] {0f, 0f },   // Kick First Mark      
        new object[] {0f, 8f },   // Kick Offset
        new object[] {0f, 0f },   // Kick Spread
        new object[] {0f, 0f },   // Kick Travel
        new object[] {0f, 8f },   // Length
        new object[] {0f, 0f },   // Length of Bend
        new object[] {0f, 0f },   // Length of Center Bend
        new object[] {0f, 0f },   // Length of Half Bend
        new object[] {0f, 8f },   // Offset Height
        new object[] {0f, 8f },   // Rise
        new object[] {0f, 8f },   // Roll
        new object[] {0f, 0f },   // Roll Angle
        new object[] { GlobalEnum.Saddle3BendMethod.First(), GlobalEnum.SegmentedBendMethod.Last() },
        new object[] {0f, 8f },   // Saddle Height
        new object[] {0f, 8f },   // Saddle Length
        new object[] {0.5f, 90f },  // Segmented Angle
        new object[] {2, 18},     // Number of Bends
        new object[] { GlobalEnum.SegmentedBendMethod.First(), GlobalEnum.SegmentedBendMethod.Last() },
        new object[] {0f, 100f }, // Segmented Radius
        new object[] {0f, 0f },  // Shift
        new object[] {0f, 0f },  // Shrink to 2nd Mark
        new object[] {0f, 0f },   // Shrink To Center
        new object[] {0f, 100f },   // Spacing
        new object[] {0f, 8f },     // Stub Length
        new object[] {0f, 0f },  // Stub Take-Up
        new object[] {0f, 0f },      // Total Shrink
        new object[] {0f, 0f }   // Travel
    };
}



[System.Serializable]
public class BendParameter
{

    public string colorHexString
    {
        get { return m_ColorHexString; }
    }
    /// <summary>
    /// A color to associate with the parameter
    /// </summary>
    [SerializeField]
    public Color                color;
    [SerializeField]
    public EBendParameterName   name;
    [SerializeField]
    public EBendParameterType   type;
    [SerializeField]
    public object               valueObject;
    /// <summary>
    /// Value should always be in Metric, if it is a measurement of Length.
    /// </summary>
    [SerializeField]
    public object       value;
    [SerializeField]
    public bool         enabled = true;
    /// <summary>
    /// Whether the parameter can be highlighted on the conduit mesh
    /// </summary>
    [SerializeField]
    public bool         canHighlight = false;

    private string      m_ColorHexString;

    public BendParameter( EBendParameterName name, EBendParameterType type, Color color, object value, object valueObject = null, bool enabled = true)
    {
        this.name = name;
        this.type = type;
        this.color = color;
        this.value = value;
        this.valueObject = valueObject;

        // Make Color String
        m_ColorHexString = ((int)(color.r * 255)).ToString( "X2" ) 
            + ((int)(color.g * 255)).ToString( "X2" )
            + ((int)(color.b * 255)).ToString( "X2" )
            + ((int)(color.a * 255)).ToString( "X2" );
    }

    public static object[] GetRange( EBendParameterName name )
    {
        switch (Engine.unitType) {
            case Units.Type.Metric:
                return BendParameterMeta.NameRangesMetric[ (int)name ];

            case Units.Type.Standard:
                return BendParameterMeta.NameRangesStandard[ (int)name ];

        }
        return null;
    }
    /// <summary>
    /// Returns value of Parameter formatted as a string, returned in current display unit type (Feet or Meters)
    /// </summary>
    /// <param name="bendParam"></param>
    /// <returns></returns>
    public static string GetFormattedValue( BendParameter bendParam )
    {
        switch (bendParam.type) {
            case EBendParameterType.FloatAngle:
                return bendParam.value.ToString();
            case EBendParameterType.Float:
                return Units.Format( Engine.unitType, Engine.outputRulerUnit, GetExternalValue( (float) bendParam.value ) );
            case EBendParameterType.Integer:
                return bendParam.value.ToString();
            case EBendParameterType.StringEnum:
                StringEnum se = (StringEnum) bendParam.valueObject;
                return se.ToStringValue( (int)bendParam.value );
        }
        return "";
    }
    public static string GetStringValue( EBendParameterName name )
    {
        return BendParameterMeta.NameStrings[ (int)name ];
    }
    /// <summary>
    /// Converts 'value' from internal units (which are Metric) to Standard (Feet) if Engine unitMode is set 
    /// to Standard. If 'value' is not a numeric type, an exception may be thrown.
    /// Does Not modify 'value'.
    /// </summary>
    public static float GetExternalValue( BendParameter param )
    {
        if (Engine.unitType == Units.Type.Standard) {
            return (float)param.value * Units.k_MToFt;
        }
        return (float)param.value;
    }
    /// <summary>
    /// Converts 'value' from internal units (which are Metric) to Standard (Feet) if Engine unitMode is set 
    /// to Standard. If 'value' is not a numeric type, an exception may be thrown.
    /// Does Not modify 'value'.
    /// </summary>
    public static float GetExternalValue( float value )
    {
        if (Engine.unitType == Units.Type.Standard) {
            return value * Units.k_MToFt;
        }
        return value;
    }

}
