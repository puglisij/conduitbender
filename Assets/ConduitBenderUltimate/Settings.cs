using UnityEngine;
using System.Collections;

public static class Settings
{

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
