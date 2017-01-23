using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class Units
{
    public enum Type { Metric, Standard }
    public enum RulerUnit { Meters, Centimeters, Millimeters, Feet, Inches, Sixteenths }

    public static readonly string[] k_UnitAbbreviations =
    {
        "m",
        "cm",
        "mm",
        "ft",
        "in",
        "frac"
    };

    private static readonly string[] k_DefaultRulerNames = {
        "metre",
        "centimetre",
        "millimetre",
        "feet",
        "inches",
        "fraction"
    };

    // Multipliers:

    // Metric Units
    public static readonly float k_MmToM = 0.001f;
    public static readonly float k_MmToCm = 0.1f;
    public static readonly float k_MToCm = 100f;
    public static readonly float k_MToMm = 1000f;
    public static readonly float k_CmToMm = 10f;
    public static readonly float k_CmToM = 0.01f;

    // Standard Units
    public static readonly float k_FtToIn = 12f;
    public static readonly float k_InToFt = 0.083333333333f;
    public static readonly float k_InToSixteenths = 16f;
    public static readonly float k_FtToSixteenths = 192f;
    public static readonly float k_SixteenthsToIn = 0.0625f;
    public static readonly float k_SixteenthsToFt = 0.0052083333333f;

    // Constant Conversion Multipliers
    public static readonly float k_FtToM = 0.3048f;
    public static readonly float k_InToM = 0.02539998628f;
    public static readonly float k_CmToIn = 0.3937007874f;
    public static readonly float k_MToIn = 39.3701f;
    public static readonly float k_MToFt = 3.28084f;


    /// <summary>
    /// Convert between Units of the same Type (e.g. Inches to Feet)
    /// </summary>
    public static float Convert( Type type, RulerUnit fromUnit, RulerUnit toUnit, float value)
    {
        // @TODO - Find a Cleaner Approach
        int fromUnit_i = (int) fromUnit;
        int toUnit_i = (int) toUnit;

        if( type == Type.Metric ) {
            if(fromUnit_i > 2 || toUnit_i > 2) {
                Debug.LogError( "Units: Convert() Cannot Convert between different Types." );
                return 0f;
            }
            switch(fromUnit_i) {
                case 0:
                    if(toUnit_i == 1) { return value * k_MToCm; } else if(toUnit_i == 2) { return value * k_MToMm; }
                    break;
                case 1:
                    if (toUnit_i == 0) { return value * k_CmToM; } else if (toUnit_i == 2) { return value * k_CmToMm; }
                    break;
                case 2:
                    if (toUnit_i == 0) { return value * k_MmToM; } else if (toUnit_i == 1) { return value * k_MmToCm; }
                    break;
            }
        } else {
            if (fromUnit_i < 3 || toUnit_i < 3) {
                Debug.LogError( "Units: Convert() Cannot Convert between different Types." );
                return 0f;
            }
            switch (fromUnit_i) {
                case 3:
                    if (toUnit_i == 4) { return value * k_FtToIn; } else if (toUnit_i == 5) { return value * k_FtToSixteenths; }
                    break;
                case 4:
                    if (toUnit_i == 3) { return value * k_InToFt; } else if (toUnit_i == 5) { return value * k_InToSixteenths; }
                    break;
                case 5:
                    if (toUnit_i == 3) { return value * k_SixteenthsToFt; } else if (toUnit_i == 4) { return value * k_SixteenthsToIn; }
                    break;
            }
        }
        return value;
    }

    /// <summary>
    /// Formats splitValues for Calculator View Output.  E.g.   4.5ft   becomes   4ft 5in
    /// @TODO - Metric type not implemented
    /// </summary>
    public static string FormatCalculator( Type type, RulerUnit unit, float value)
    {
        if(value == 0f) { return "0"; }

        bool isNegative = false;
        if(value < 0f) {
            value *= -1f;
            isNegative = true;
        }
        // Break the Value Apart into Measures
        var splitValues = Split(type, unit, value);

        // Build the String
        string txt = "";

        switch (type) {
            case Type.Metric:
                
                // @TODO - Implement
                break;
            case Type.Standard:

                if(splitValues[2] > 0f) {
                    txt = splitValues[ 2 ] + "ft";
                }
                if (splitValues[ 1 ] > 0f && splitValues[ 0 ] > 0f) {
                    // fraction
                    txt += splitValues[ 1 ] + "&" + new Rational( (long)splitValues[ 0 ], 16 ).ToString() + "in";
                } else if(splitValues[1] > 0f) {
                    txt += splitValues[ 1 ] + "in";
                } else if(splitValues[0] > 0f) {
                    txt += new Rational( (long)splitValues[ 0 ], 16 ).ToString() + "in";
                }

                break;
            default:
                throw new ArgumentException( "Units: FormatCalculator() Invalid type argument." );
        }
        if(isNegative) {
            return "-" + txt;
        }
        return txt;
    }

    public static string Format( Type type, RulerUnit highUnit, float[] splitValues )
    {
        // Build the String
        string txt = "";

        switch (type) {
            case Type.Metric:
                switch (highUnit) {
                    case RulerUnit.Millimeters:
                        txt = splitValues[ 0 ] + "mm";
                        break;
                    case RulerUnit.Centimeters:
                        txt = splitValues[ 1 ] + "cm - ";
                        txt += splitValues[ 0 ] + "mm";
                        break;
                    case RulerUnit.Meters:
                        txt = splitValues[ 2 ] + "m ";
                        txt += splitValues[ 1 ] + "cm - ";
                        txt += splitValues[ 0 ] + "mm";
                        break;
                }
                break;
            case Type.Standard:
                switch (highUnit) {
                    case RulerUnit.Sixteenths:
                        txt = new Rational( (long)splitValues[ 0 ], (long)16 ).ToString() + "\"";
                        break;
                    case RulerUnit.Inches:
                        txt = splitValues[ 1 ] + " - ";
                        txt += new Rational( (long)splitValues[ 0 ], (long)16 ).ToString() + "\"";
                        break;
                    case RulerUnit.Feet:
                        txt = splitValues[ 2 ] + "' ";
                        txt += splitValues[ 1 ] + " - ";
                        txt += new Rational( (long)splitValues[ 0 ], (long)16 ).ToString() + "\"";
                        break;
                }
                break;
            default:
                throw new ArgumentException( "Units: Format() Invalid type argument." );
        }
        return txt;
    }
    public static string Format( Type type, RulerUnit unit, float value )
    {
        // Break the Value Apart into Measures
        var splitValues = Split(type, unit, value);

        return Format( type, unit, splitValues );
    }
    /// <summary>
    /// 'precision' is number of decimal places to retain. E.g. 2 to round to nearest 1/100th
    /// </summary>
    public static float Round(float value, int precision)
    {
        return (float)Math.Round( value, precision );
    }

    public static string RulerName( RulerUnit rulerUnit )
    {
        int unit_i = (int) rulerUnit;
        if (unit_i < k_DefaultRulerNames.Length) {
            return k_DefaultRulerNames[ unit_i ];
        }
        return null;
    }

    /// <summary>
    /// 'Type' is Metric or Standard. 'RulerUnit' is the unit of the passed value (e.g. Feet, Inches, Centimeters...)
    /// </summary>
    public static float[] Split(Type type, RulerUnit unit, float value)
    {
        float[] splitValues;
        float iValue = value; // Intermediate

        switch(type) 
        {
            case Type.Metric:
                switch(unit) 
                {
                    case RulerUnit.Meters:
                        splitValues = new float[ 3 ];
                        splitValues[ 2 ] = Mathf.Floor( iValue );
                        iValue -= splitValues[ 2 ];
                        splitValues[ 1 ] = Mathf.Floor( iValue * k_MToCm );
                        iValue -= splitValues[ 1 ] * k_CmToM;
                        splitValues[ 0 ] = Mathf.Round( iValue * k_MToMm );
                        break;
                    case RulerUnit.Centimeters:
                        splitValues = new float[ 2 ];
                        splitValues[ 1 ] = Mathf.Floor( iValue );
                        iValue -= splitValues[ 1 ];
                        splitValues[ 0 ] = Mathf.Round( iValue * k_CmToMm );
                        break;
                    case RulerUnit.Millimeters:
                        splitValues = new float[ 1 ];
                        splitValues[ 0 ] = Mathf.Round( iValue );
                        break;
                    default:
                        throw new ArgumentException( "Units: Split() Invalid unit argument." );
                }
                break;
            case Type.Standard:
                switch (unit) {
                    case RulerUnit.Feet:
                        splitValues = new float[ 3 ];
                        splitValues[ 2 ] = Mathf.Floor( iValue );
                        iValue -= splitValues[ 2 ];             // Subtract Feet
                        splitValues[ 1 ] = Mathf.Floor( iValue * k_FtToIn );
                        iValue -= splitValues[ 1 ] * k_InToFt;  // Subtract Inches
                        splitValues[ 0 ] = Mathf.Round( iValue * k_FtToSixteenths );
                        // Carry (i.e. 16/16 sixteenths)
                        if (splitValues[ 0 ] == 16f) {
                            splitValues[ 1 ] += 1f;
                            splitValues[ 0 ] = 0f;
                        }
                        break;
                    case RulerUnit.Inches:
                        splitValues = new float[ 2 ];
                        splitValues[ 1 ] = Mathf.Floor( iValue );
                        iValue -= splitValues[ 1 ];
                        splitValues[ 0 ] = Mathf.Round( iValue * k_InToSixteenths );
                        // Carry (i.e. 16/16 sixteenths)
                        if (splitValues[ 0 ] == 16f) {
                            splitValues[ 1 ] += 1f;
                            splitValues[ 0 ] = 0f;
                        }
                        break;
                    case RulerUnit.Sixteenths:
                        splitValues = new float[ 1 ];
                        splitValues[ 0 ] = Mathf.Round( iValue );
                        break;
                    default:
                        throw new ArgumentException( "Units: Split() Invalid unit argument." );
                }
                break;
            default:
                throw new ArgumentException( "Units: Split() Invalid type argument." );
        }

        return splitValues;
    }
}
