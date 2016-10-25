using UnityEngine;
using System.Collections;

public class Calculator
{
    public const float k_HalfPi = Mathf.PI * 0.5f;

    public static bool PointsTowards( Vector3 dir, Vector3 point )
    {
        return Vector3.Dot( dir, point ) > 0f;
    }

    public static float Chord( float radius, float angleRad )
    {
        return 2f * radius * Mathf.Sin( 0.5f * angleRad );
    }
    /// <summary>
    /// Clamp Angle to positive 0 to 180 degrees
    /// </summary>
    public static float ClampAngle( float angle, int decimalPrecision )
    {
        angle -= (180f * Mathf.Floor( angle / 180f ));
        return Units.Round( angle * Mathf.Sign( angle ), 1 );
    }
    public static float Ab( float bendAngleRad )
    {
        return bendAngleRad * 0.5f;
    }
    public static float Bh( float radius, float bendAngleRad )
    {
        return radius * Mathf.Cos( k_HalfPi - bendAngleRad );
    }
    public static float Bv( float radius, float bendAngleRad )
    {
        return radius - radius * Mathf.Sin( k_HalfPi - bendAngleRad );
    }
    public static float Vb( float radius, float angleRad )
    {
        return radius - (radius * Mathf.Cos( angleRad ));
    }
    public static float Es( float radius, float angleRad )
    {
        return (radius * Mathf.Sin( angleRad / 2f )) / Mathf.Cos( angleRad / 2f );
    }
    public static float Hb( float radius, float angleRad )
    {
        return radius * Mathf.Sin( angleRad );
    }
    public static float Hs( float Ls, float angleRad )
    {
        return Ls * Mathf.Cos( angleRad );
    }
    public static float Hs_(float Vs, float angleRad )
    {
        return Vs / Mathf.Tan( angleRad );
    }
    public static float Lb( float radius, float angleRad )
    {
        return radius * angleRad;
    }
    public static float Ls( float Vs, float angleRad )
    {
        return Vs / Mathf.Sin( angleRad );
    }

    /// <summary>
    /// If facing the opposite direction of the axis vector, 
    /// Rotation is of given vector is CounterClockwise around given axis by given angle (in degrees)
    /// </summary>
    public static Vector3 RotateCCW( float angleDeg, Vector3 bendAxis, Vector3 vec )
    {
        return Quaternion.AngleAxis( -angleDeg, bendAxis ) * vec;
    }
    /// <summary>
    /// If facing the opposite direction of the axis vector, 
    /// Rotation is of given vector is Clockwise around given axis by given angle (in degrees)
    /// </summary>
    public static Vector3 RotateCW( float angleDeg, Vector3 bendAxis, Vector3 vec )
    {
        return Quaternion.AngleAxis( angleDeg, bendAxis ) * vec;
    }
}
