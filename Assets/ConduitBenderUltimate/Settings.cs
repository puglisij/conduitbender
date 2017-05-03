using UnityEngine;
using System.Collections;

public static class Settings
{
    /*
            Center Radius for Greenlee Benders:

            Sizes:                  1/2"        3/4"        1"          1 1/4"      1 1/2"      2"
            Site-Rite:              4 3/16"     5 1/8"      6 1/2"      11"         -           -
            Site-Rite 2:            4 3/16"     5 1/8"      6 1/2"      9 5/8"      -           -
            555 EMT:                4 1/4"      5 3/8"      6 3/4"      8 3/4"      8 9/32"     9 3/16"
            555 IMC:                4 3/8"      4 1/2"      5 3/4"      7 1/4"      8 1/4"      9 1/2"
            555 Rigid:              4 3/8"      4 1/2"      5 3/4"      7 1/4"      8 9/32"     9 3/16"
            854/855 EMT:            4 5/16"     5 1/2"      7"          8 13/16"    8 3/8"      9 1/4"
            854/855 IMC:            4 1/4"      5 7/16"     6 15/16"    8 3/4"      8 1/4"      9"
            880:                    4"          4 1/2"      5 3/4"      7 1/4"      8 1/4"      9 1/2"
            882 EMT:                 -             -           -        7 7/32"     8 1/16"     9 5/16"
            882 IMC/Rigid:           -             -           -        7 1/4"      8 1/4"      8 7/8"
            1800/1801 Rigid & IMC:  2 5/8"      4 5/8"      5 7/8"      8 1/16"     9 11/16"       -

            Sizes:                  1 1/4"      1 1/2"      2"          2 1/2"      3"          3 1/2"      4"          5"
            777:                    7 1/4"      8 1/4"      9 1/2"      11 7/16"    13 3/4"     16"         18 1/4"     -
            881:                    -           -           -           13 1/2"     16"         18 5/8"     20 7/8"     -
            884 / 885:              7 1/4"      8 1/4"      9 1/2"      12 1/2"     15"         17 1/2"     20"         25"
        */

    /// <summary> Max value in Meters </summary>
    public static readonly float k_minBenderRadiusM = 0.0254f; // 1"
    /// <summary> Max value in Meters </summary>
    public static readonly float k_maxBenderRadiusM = 2.5f;
    /// <summary> Max value in Meters </summary>
    public static readonly float k_minConduitDiameterM = 0.0015875f;  // 1/16"
    /// <summary> Max value in Meters </summary>
    public static readonly float k_maxConduitDiameterM = 2.5f;


    /// <summary>
    /// Currently just an abstraction of PlayerPrefs but could easily be altered.
    /// </summary>
    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat( key );
    }
    public static string GetString(string key)
    {
        return PlayerPrefs.GetString( key );
    }

    //public static T GetValue<T>(string key)
    //{

    //}

    public static void SetValue(string key, float value)
    {
        PlayerPrefs.SetFloat( key, value );
    }
    public static void SetValue(string key, string value)
    {
        PlayerPrefs.SetString( key, value );
    }
}
