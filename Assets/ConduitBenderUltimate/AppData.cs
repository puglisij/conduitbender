using UnityEngine;
using LitJson;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class AppData
{
    public class Data
    {

    }

    public const string k_SettingsFilePath = "/Settings.json";

    // Main Application Data Store Object
    static Data data = new Data();
    // Path to the Game Data Folder (e.g. points directly at APK on Android, or <path to player app>/<AppName.app> on iOS)
    static string dataPath             = Application.dataPath;
    // Where data that is expected to be kept between runs can be stored. (Files wont be erased with each update of App)
    static string persistentDataPath   = Application.persistentDataPath;

    private static JsonData m_JsonData;

    public static void Initialize()
    {

    }

    /// <summary>
    /// Load all Saved data files.
    /// </summary>
    public static void Load()
    {
        //string jsonString = File.ReadAllText( Application.dataPath + k_SettingsFilePath );
        //m_JsonData = JsonMapper.ToObject( jsonString );
        // MyObject object = JsonMapper.ToObject<MyObject>( jsonString );

        //Debug.Log( "First Item: " + m_JsonData[ "ConduitSizes" ][ "Standard" ][ 0 ] );
        //Debug.Log( "AppData: Load()" );

        throw new NotImplementedException();
    }

    /// <summary>
    /// Loads given filename (from persistentDataPath) into 'object' data parameter. 
    /// null if file does not exist.
    /// </summary>
    public static void Load<T>( string fileName, out T data )
    {
        Load( fileName, out data, Application.persistentDataPath );
    }
    /// <summary>
    /// Loads given filename (from given path) into 'object' data parameter. 
    /// null if file does not exist.
    /// </summary>
    public static void Load<T>( string fileName, out T data, string path )
    {
        if (File.Exists( path + "/" + fileName )) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path + "/" + fileName, FileMode.Open);

            // Load from File
            data = (T)bf.Deserialize( file );
            file.Close();
        } else {
            data = default( T );
        }
    }

    public static void Save()
    {
        JsonData dataToSave = JsonMapper.ToJson( data );
        File.WriteAllText( k_SettingsFilePath, dataToSave.ToJson() );
    }

    /// <summary>
    /// Save 'object' data parameter to Binary File at Application persistentDataPath
    /// Good for Saving 'USER' Generated Data
    /// </summary>
    public static void Save<T>( string fileName, T data )
    {
        Save( fileName, data, Application.persistentDataPath );
    }
    /// <summary>
    /// Save 'object' data parameter to Binary File at given path
    /// </summary>
    public static void Save<T>( string fileName, T data, string path )
    {
        // Application.persistentDataPath is device specific
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create( path + "/" + fileName);

        // Write to file
        bf.Serialize( file, data );
        file.Close();
    }

    //public static T GetItem<T>(string itemPath)
    //{
    //    string[] objects = itemPath.Split('.');

    //    JsonData nextObject = m_JsonData;
    //    for(int o = 0; o < objects.Length - 1; ++o) {
    //        nextObject = nextObject[ objects[ o ] ];
    //    }
    //    T item = nextObject[ objects[objects.Length - 1] ];
    //}

    /// <summary>
    /// Null any locally cached data (to free memory). To access the data again, it must be Re-Loaded.
    /// </summary>
    public static void ReleaseData()
    {
        throw new NotImplementedException();
    }

}
